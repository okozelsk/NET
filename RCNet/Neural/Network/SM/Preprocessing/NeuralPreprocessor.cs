using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Data;
using RCNet.Neural.Network.SM.Preprocessing.Input;
using RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.Neural.Network.SM.Preprocessing
{
    /// <summary>
    /// Implements the neural preprocessor supporting multiple reservoirs.
    /// </summary>
    [Serializable]
    public class NeuralPreprocessor
    {
        //Enums
        /// <summary>
        /// The way of bidirectional processing of input pattern.
        /// </summary>
        public enum BidirProcessing
        {
            /// <summary>
            /// Enabled bi-directional processing without reservoir reset when the direction to be turned.
            /// </summary>
            Continuous,
            /// <summary>
            /// Enabled bi-directional processing with reservoir reset when the direction to be turned.
            /// </summary>
            WithReset,
            /// <summary>
            /// The bi-directional processing is forbidden.
            /// </summary>
            Forbidden
        }

        //Delegates
        /// <summary>
        /// The delegate of the PreprocessingProgressChanged event handler.
        /// </summary>
        /// <param name="totalNumOfInputs">The total number of inputs to be processed.</param>
        /// <param name="numOfProcessedInputs">The number of already processed inputs.</param>
        /// <param name="finalPreprocessingOverview">The final overview of the preprocessing.</param>
        public delegate void PreprocessingProgressChangedHandler(int totalNumOfInputs,
                                                                 int numOfProcessedInputs,
                                                                 PreprocessingOverview finalPreprocessingOverview
                                                                 );
        //Events
        /// <summary>
        /// This informative event occurs every time the progress of neural preprocessing has changed.
        /// </summary>
        [field: NonSerialized]
        public event PreprocessingProgressChangedHandler PreprocessingProgressChanged;


        //Attribute properties
        /// <summary>
        /// The collection of reservoir instances.
        /// </summary>
        public List<ReservoirInstance> ReservoirCollection { get; }

        /// <summary>
        /// The number of boot cycles.
        /// </summary>
        public int BootCycles { get; }

        /// <summary>
        /// The total number of hidden neurons in all reservoirs.
        /// </summary>
        public int TotalNumOfHiddenNeurons { get; }

        /// <summary>
        /// The descriptors of all predictors.
        /// </summary>
        public List<PredictorDescriptor> PredictorDescriptorCollection { get; private set; }

        /// <summary>
        /// The collection of switches generally enabling/disabling the predictors.
        /// </summary>
        public bool[] OutputFeatureGeneralSwitchCollection { get; private set; }

        /// <summary>
        /// The number of active predictors (predictors + routed inputs).
        /// </summary>
        public int NumOfActivePredictors { get; private set; }


        //Attributes
        private readonly NeuralPreprocessorSettings _preprocessorCfg;
        private readonly InputEncoder _inputEncoder;
        private List<int> _predictorsTimePointSlicesPlan;
        private int _totalNumOfReservoirsPredictors;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="preprocessorCfg">The configuration of the neural preprocessor.</param>
        /// <param name="randomizerSeek">The random number generator initial seek.</param>
        public NeuralPreprocessor(NeuralPreprocessorSettings preprocessorCfg, int randomizerSeek)
        {
            _preprocessorCfg = (NeuralPreprocessorSettings)preprocessorCfg.DeepClone();
            TotalNumOfHiddenNeurons = 0;
            ///////////////////////////////////////////////////////////////////////////////////
            //Input encoder
            _inputEncoder = new InputEncoder(_preprocessorCfg.InputEncoderCfg);
            ///////////////////////////////////////////////////////////////////////////////////
            //Reservoir instance(s)
            BootCycles = 0;
            //Random generator used for reservoir structure initialization
            Random rand = (randomizerSeek < 0 ? new Random() : new Random(randomizerSeek));
            ReservoirCollection = new List<ReservoirInstance>(_preprocessorCfg.ReservoirInstancesCfg.ReservoirInstanceCfgCollection.Count);
            int reservoirInstanceID = 0;
            int defaultBootCycles = 0;
            foreach (ReservoirInstanceSettings reservoirInstanceCfg in _preprocessorCfg.ReservoirInstancesCfg.ReservoirInstanceCfgCollection)
            {
                ReservoirStructureSettings structCfg = _preprocessorCfg.ReservoirStructuresCfg.GetReservoirStructureCfg(reservoirInstanceCfg.StructureCfgName);
                ReservoirInstance reservoir = new ReservoirInstance(reservoirInstanceID++,
                                                                    structCfg,
                                                                    reservoirInstanceCfg,
                                                                    _inputEncoder,
                                                                    rand
                                                                    );
                ReservoirCollection.Add(reservoir);
                TotalNumOfHiddenNeurons += reservoir.Size;
                defaultBootCycles = Math.Max(defaultBootCycles, reservoir.GetDefaultBootCycles());
            }
            //Boot cycles setup
            if (_preprocessorCfg.InputEncoderCfg.FeedingCfg.FeedingType == InputEncoder.InputFeedingType.Continuous)
            {
                FeedingContinuousSettings feedingCfg = (FeedingContinuousSettings)preprocessorCfg.InputEncoderCfg.FeedingCfg;
                BootCycles = feedingCfg.BootCycles == FeedingContinuousSettings.AutoBootCyclesNum ? defaultBootCycles : feedingCfg.BootCycles;
            }
            else
            {
                BootCycles = 0;
            }
            //Output features
            _totalNumOfReservoirsPredictors = 0;
            _predictorsTimePointSlicesPlan = null;
            PredictorDescriptorCollection = null;
            OutputFeatureGeneralSwitchCollection = null;
            NumOfActivePredictors = 0;
            return;
        }

        //Properties
        /// <inheritdoc cref="BidirProcessing"/>
        private BidirProcessing Bidir
        {
            get
            {
                return _preprocessorCfg.InputEncoderCfg.FeedingCfg.FeedingType == InputEncoder.InputFeedingType.Patterned ? ((FeedingPatternedSettings)_preprocessorCfg.InputEncoderCfg.FeedingCfg).Bidir : BidirProcessing.Forbidden;
            }
        }

        /// <summary>
        /// Gets the number of suppressed predictors.
        /// </summary>
        public int NumOfSuppressedPredictors { get { return PredictorDescriptorCollection.Count - NumOfActivePredictors; } }

        //Methods
        /// <summary>
        /// Compares two predictors.
        /// </summary>
        /// <param name="p1">Predictor 1.</param>
        /// <param name="p2">Predictor 2.</param>
        public static int ComparePredictors(Tuple<int, double> p1, Tuple<int, double> p2)
        {
            if (p1.Item2 > p2.Item2)
            {
                return -1;
            }
            else if (p1.Item2 < p2.Item2)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Initializes the collection of predictors descriptors.
        /// </summary>
        private void InitPredictorsDescriptors()
        {
            //Final descriptors collection
            PredictorDescriptorCollection = new List<PredictorDescriptor>();
            //Routed input values
            if (_inputEncoder.NumOfRoutedValues > 0)
            {
                PredictorDescriptorCollection.AddRange(_inputEncoder.GetPredictorsDescriptorsOfRoutedInputs());
            }
            //Hidden neurons predictors
            List<PredictorDescriptor> reservoirsPredictorDescriptorCollection = new List<PredictorDescriptor>();
            foreach (ReservoirInstance reservoir in ReservoirCollection)
            {
                reservoirsPredictorDescriptorCollection.AddRange(reservoir.GetPredictorsDescriptors());
            }
            if (_preprocessorCfg.InputEncoderCfg.FeedingCfg.FeedingType == InputEncoder.InputFeedingType.Continuous)
            {
                //Continuous feeding
                PredictorDescriptorCollection.AddRange(reservoirsPredictorDescriptorCollection);
                _totalNumOfReservoirsPredictors = reservoirsPredictorDescriptorCollection.Count;
            }
            else
            {
                //Patterned feeding
                FeedingPatternedSettings patternedCfg = (FeedingPatternedSettings)_preprocessorCfg.InputEncoderCfg.FeedingCfg;
                for (int i = 0; i < (patternedCfg.Bidir != BidirProcessing.Forbidden ? 2 : 1); i++)
                {
                    for (int j = 0; j < patternedCfg.Slices; j++)
                    {
                        PredictorDescriptorCollection.AddRange(reservoirsPredictorDescriptorCollection);
                        _totalNumOfReservoirsPredictors += reservoirsPredictorDescriptorCollection.Count;
                    }
                }
                //Predictors time-point slices plan
                if (_inputEncoder.NumOfTimePoints != InputEncoder.VariableNumOfTimePoints)
                {
                    //Check correctness
                    if (patternedCfg.Slices > _inputEncoder.NumOfTimePoints)
                    {
                        throw new InvalidOperationException("Resulting number of input pattern's time points is less than requested number of slices of predictors.");
                    }
                    //Build plan
                    _predictorsTimePointSlicesPlan = new List<int>(patternedCfg.Slices);
                    double avgDistance = (double)_inputEncoder.NumOfTimePoints / (double)patternedCfg.Slices;
                    //The first phase - naive distribution of time-points
                    double countDown = _inputEncoder.NumOfTimePoints;
                    int lastTimePoint = -1;
                    while ((int)Math.Round(countDown, 0) >= 1 && _predictorsTimePointSlicesPlan.Count < patternedCfg.Slices)
                    {
                        int roundedTimePoint = (int)Math.Round(countDown, 0, MidpointRounding.AwayFromZero);
                        if (roundedTimePoint != lastTimePoint)
                        {
                            _predictorsTimePointSlicesPlan.Insert(0, roundedTimePoint);
                            lastTimePoint = roundedTimePoint;
                        }
                        countDown -= avgDistance;
                    }
                    //Second phase - distribution of remaining time-points
                    while (_predictorsTimePointSlicesPlan.Count < patternedCfg.Slices)
                    {
                        for (int i = _predictorsTimePointSlicesPlan.Count - 2; i > -1; i--)
                        {
                            int span = _predictorsTimePointSlicesPlan[i + 1] - (i >= 0 ? _predictorsTimePointSlicesPlan[i] : 1);
                            if (span > 1)
                            {
                                int timePoint = (i >= 0 ? _predictorsTimePointSlicesPlan[i] : 1) + (int)Math.Round(span / 2d, 0, MidpointRounding.AwayFromZero);
                                _predictorsTimePointSlicesPlan.Insert(i + 1, timePoint);
                                if (_predictorsTimePointSlicesPlan.Count == patternedCfg.Slices)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return;
        }

        /// <summary>
        /// Checks the predictors and sets the general enabling/disabling switches.
        /// </summary>
        /// <param name="predictorsCollection">The collection of predictors.</param>
        private void InitOutputFeaturesGeneralSwitches(List<double[]> predictorsCollection)
        {
            //Allocate general switches
            OutputFeatureGeneralSwitchCollection = new bool[PredictorDescriptorCollection.Count];
            //Init general predictor switches to false
            OutputFeatureGeneralSwitchCollection.Populate(false);
            //Compute statistics on predictors
            Tuple<int, double>[] predictorValueSpanCollection = new Tuple<int, double>[PredictorDescriptorCollection.Count];
            Parallel.For(0, PredictorDescriptorCollection.Count, i =>
            {
                BasicStat stat = new BasicStat();
                for (int row = 0; row < predictorsCollection.Count; row++)
                {
                    stat.AddSample(predictorsCollection[row][i]);
                }
                //Use predictor's value span as a differentiator
                predictorValueSpanCollection[i] = new Tuple<int, double>(i, stat.Span);
            });
            //Sort collected predictor differentiators
            Array.Sort(predictorValueSpanCollection, ComparePredictors);
            //Enable predictors
            int numOfPredictorsToBeRejected = (int)(Math.Round(PredictorDescriptorCollection.Count * _preprocessorCfg.PredictorsReductionRatio));
            int firstIndexToBeRejected = predictorValueSpanCollection.Length - numOfPredictorsToBeRejected;
            NumOfActivePredictors = 0;
            for (int i = 0; i < predictorValueSpanCollection.Length; i++)
            {
                if (predictorValueSpanCollection[i].Item2 > _preprocessorCfg.PredictorValueMinSpan && i < firstIndexToBeRejected)
                {
                    //Enable predictor
                    OutputFeatureGeneralSwitchCollection[predictorValueSpanCollection[i].Item1] = true;
                    ++NumOfActivePredictors;
                }
            }
            return;
        }

        /// <summary>
        /// Resets the neural preprocessor to its initial state.
        /// </summary>
        public void Reset()
        {
            //Reset input encoder
            _inputEncoder.Reset();
            //Reset reservoirs
            foreach (ReservoirInstance reservoir in ReservoirCollection)
            {
                reservoir.Reset(true);
            }
            //Reset predictors related members
            _totalNumOfReservoirsPredictors = 0;
            _predictorsTimePointSlicesPlan = null;
            NumOfActivePredictors = 0;
            PredictorDescriptorCollection = null;
            OutputFeatureGeneralSwitchCollection = null;
            return;
        }

        /// <summary>
        /// Resets the reservoir instances.
        /// </summary>
        /// <param name="resetStatistics">Specifies whether to reset the reservoir statistics.</param>
        private void ResetReservoirs(bool resetStatistics)
        {
            //Reset reservoirs
            foreach (ReservoirInstance reservoir in ReservoirCollection)
            {
                reservoir.Reset(resetStatistics);
            }
            return;
        }

        /// <summary>
        /// Preprocesses all pending input data prepared by InputEncoder and collects the predictors.
        /// </summary>
        /// <param name="collectStatistics">Indicates whether to update internal statistics.</param>
        private double[] ProcessPendingData(bool collectStatistics)
        {
            double[] predictors = new double[_totalNumOfReservoirsPredictors / (Bidir == BidirProcessing.Forbidden ? 1 : 2)];
            int predictorsIdx = 0;
            int predictorsTimePointSlicesPlanIdx = 0;
            int computationStep = 1;
            //Loop pending data
            while (_inputEncoder.NumOfRemainingInputs > 0)
            {
                _inputEncoder.EncodeNextInputData();
                while (_inputEncoder.Fetch(collectStatistics))
                {
                    //Compute reservoir(s)
                    foreach (ReservoirInstance reservoir in ReservoirCollection)
                    {
                        reservoir.Compute(collectStatistics);
                    }
                }
                if ((_predictorsTimePointSlicesPlan != null && computationStep == _predictorsTimePointSlicesPlan[predictorsTimePointSlicesPlanIdx]) ||
                    (_predictorsTimePointSlicesPlan == null && _inputEncoder.NumOfRemainingInputs == 0))
                {
                    //Collect predictors from reservoirs
                    foreach (ReservoirInstance reservoir in ReservoirCollection)
                    {
                        predictorsIdx += reservoir.CopyPredictorsTo(predictors, predictorsIdx);
                    }
                    ++predictorsTimePointSlicesPlanIdx;
                }
                ++computationStep;
            }
            return predictors;
        }

        /// <summary>
        /// Pushes an external input vector into the input encoder, computes reservoirs and returns the predictors.
        /// </summary>
        /// <param name="inputVector">An external input vector.</param>
        /// <param name="collectStatistics">Indicates whether to update internal statistics.</param>
        private double[] PushExtInputVector(double[] inputVector, bool collectStatistics)
        {
            //Output features buffer allocation and index
            double[] outputFeatures = new double[PredictorDescriptorCollection.Count];
            int outputFeaturesIdx = 0;
            //Put new data into the InputEncoder
            _inputEncoder.StoreNewData(inputVector);
            //Collect routed input data
            outputFeaturesIdx += _inputEncoder.CopyRoutedInputsTo(outputFeatures, outputFeaturesIdx);
            //Reset reservoirs in case of patterned feeding
            if (_preprocessorCfg.InputEncoderCfg.FeedingCfg.FeedingType == InputEncoder.InputFeedingType.Patterned)
            {
                ResetReservoirs(false);
            }
            //Process input data in reservoirs and collect predictors
            double[] predictors = ProcessPendingData(collectStatistics);
            predictors.CopyTo(outputFeatures, outputFeaturesIdx);
            outputFeaturesIdx += predictors.Length;
            //Bidirectional input processing?
            if (Bidir != BidirProcessing.Forbidden)
            {
                if (Bidir == BidirProcessing.WithReset)
                {
                    ResetReservoirs(false);
                }
                //Set reverse mode
                _inputEncoder.SetReverseMode();
                //Process reversed input data in reservoirs
                predictors = ProcessPendingData(collectStatistics);
                predictors.CopyTo(outputFeatures, outputFeaturesIdx);
            }
            return outputFeatures;
        }

        /// <summary>
        /// Pushes an external input data into the preprocessor and returns the predictors.
        /// </summary>
        /// <param name="input">The external input data in natural form.</param>
        public double[] Preprocess(double[] input)
        {
            if (OutputFeatureGeneralSwitchCollection == null)
            {
                throw new InvalidOperationException($"Preprocessor is not initialized. Call InitializeAndPreprocessBundle method first.");
            }
            return PushExtInputVector(input, false);
        }

        /// <summary>
        /// Initializes the preprocessor, preprocess the specified data bundle and returns the predictors together with the ideal values.
        /// </summary>
        /// <param name="inputBundle">The data bundle to be preprocessed.</param>
        /// <param name="preprocessingOverview">The statistics and other important information related to data preprocessing.</param>
        public VectorBundle InitializeAndPreprocessBundle(VectorBundle inputBundle, out PreprocessingOverview preprocessingOverview)
        {
            //Check amount of input data
            if (BootCycles > 0 && inputBundle.InputVectorCollection.Count <= BootCycles)
            {
                throw new InvalidOperationException($"Insufficient number of input data instances. The number of instances must be greater than the number of boot cycles ({BootCycles.ToString(CultureInfo.InvariantCulture)}).");
            }
            //Reset reservoirs
            ResetReservoirs(true);
            //Reset input encoder and initialize its feature filters
            _inputEncoder.Initialize(inputBundle);
            //Initialize output features descriptors
            InitPredictorsDescriptors();
            //Allocate output bundle
            VectorBundle outputBundle = new VectorBundle(inputBundle.InputVectorCollection.Count);
            //Process data
            //Collect predictors
            for (int dataSetIdx = 0; dataSetIdx < inputBundle.InputVectorCollection.Count; dataSetIdx++)
            {
                bool readyToCollect = dataSetIdx >= BootCycles || _preprocessorCfg.InputEncoderCfg.FeedingCfg.FeedingType == InputEncoder.InputFeedingType.Patterned;
                //Push input data into the network
                double[] outputFeatures = PushExtInputVector(inputBundle.InputVectorCollection[dataSetIdx], readyToCollect);
                //Collect output features?
                if (readyToCollect)
                {
                    //Predictors
                    outputBundle.InputVectorCollection.Add(outputFeatures);
                    //Desired outputs
                    outputBundle.OutputVectorCollection.Add(inputBundle.OutputVectorCollection[dataSetIdx]);
                }
                //Raise informative event
                PreprocessingProgressChanged(inputBundle.InputVectorCollection.Count, dataSetIdx + 1, null);
            }
            //Initialize output features switches
            InitOutputFeaturesGeneralSwitches(outputBundle.InputVectorCollection);
            //Buld preprocessing overview
            preprocessingOverview = new PreprocessingOverview(CollectStatatistics(),
                                                              TotalNumOfHiddenNeurons,
                                                              PredictorDescriptorCollection.Count,
                                                              NumOfSuppressedPredictors,
                                                              NumOfActivePredictors
                                                              );
            //Raise final informative event
            PreprocessingProgressChanged(inputBundle.InputVectorCollection.Count, inputBundle.InputVectorCollection.Count, preprocessingOverview);
            //Return output
            return outputBundle;
        }

        /// <summary>
        /// Collects the statistics of the reservoir instances.
        /// </summary>
        /// <remarks>
        /// It is very important to follow these statistics to make sure the reservoirs exhibit the proper behavior.
        /// </remarks>
        public List<ReservoirStat> CollectStatatistics()
        {
            List<ReservoirStat> stats = new List<ReservoirStat>();
            foreach (ReservoirInstance reservoir in ReservoirCollection)
            {
                stats.Add(reservoir.CollectStatistics());
            }
            return stats;
        }

        //Inner classes
        /// <summary>
        /// Implements an overview of the data bundle preprocessing.
        /// </summary>
        [Serializable]
        public class PreprocessingOverview
        {
            //Attribute properties
            /// <summary>
            /// The collection of the reservoirs statistics.
            /// </summary>
            public List<ReservoirStat> ReservoirStatCollection { get; }
            /// <summary>
            /// The total number of neurons.
            /// </summary>
            public int TotalNumOfNeurons { get; }
            /// <summary>
            /// The total number of predictors.
            /// </summary>
            public int TotalNumOfPredictors { get; }
            /// <summary>
            /// The number of suppressed predictors.
            /// </summary>
            public int NumOfSuppressedPredictors { get; }
            /// <summary>
            /// The number of active predictors.
            /// </summary>
            public int NumOfActivePredictors { get; }

            //Constructor
            /// <summary>
            /// Creates an initialized instance.
            /// </summary>
            /// <param name="reservoirStatCollection">The collection of the reservoirs statistics.</param>
            /// <param name="totalNumOfNeurons">The total number of neurons.</param>
            /// <param name="totalNumOfPredictors">The total number of predictors.</param>
            /// <param name="numOfSuppressedPredictors">The number of suppressed predictors.</param>
            /// <param name="numOfActivePredictors">The number of active predictors.</param>
            public PreprocessingOverview(List<ReservoirStat> reservoirStatCollection,
                                         int totalNumOfNeurons,
                                         int totalNumOfPredictors,
                                         int numOfSuppressedPredictors,
                                         int numOfActivePredictors
                                         )
            {
                ReservoirStatCollection = reservoirStatCollection;
                TotalNumOfNeurons = totalNumOfNeurons;
                TotalNumOfPredictors = totalNumOfPredictors;
                NumOfSuppressedPredictors = numOfSuppressedPredictors;
                NumOfActivePredictors = numOfActivePredictors;
                return;
            }

            //Methods
            private string FNum(double num)
            {
                return num.ToString("N8", CultureInfo.InvariantCulture).PadLeft(12);
            }

            private string StatLine(BasicStat stat)
            {
                return $"Avg:{FNum(stat.ArithAvg)},  Max:{FNum(stat.Max)},  Min:{FNum(stat.Min)},  StdDev:{FNum(stat.StdDev)}";
            }

            private void AppendStandardStatSet(int margin, StringBuilder sb, ReservoirStat.StandardStatSet sss)
            {
                string leftMargin = margin == 0 ? string.Empty : new string(' ', margin);
                sb.Append(leftMargin + $" Avg> {StatLine(sss.AvgStat)}" + Environment.NewLine);
                sb.Append(leftMargin + $" Max> {StatLine(sss.MaxStat)}" + Environment.NewLine);
                sb.Append(leftMargin + $" Min> {StatLine(sss.MinStat)}" + Environment.NewLine);
                sb.Append(leftMargin + $"Span> {StatLine(sss.SpanStat)}" + Environment.NewLine);
                return;
            }

            private void AppendSynapsesStat(int margin, StringBuilder sb, ReservoirStat.SynapsesByRoleStat srs)
            {
                string leftMargin = margin == 0 ? string.Empty : new string(' ', margin);
                sb.Append(leftMargin + $"Synapses" + Environment.NewLine);
                foreach (ReservoirStat.SynapseStat synapseStat in srs.SynapseRole)
                {
                    sb.Append(leftMargin + $"    {synapseStat.Role}: {((double)synapseStat.Count / (double)srs.Count).ToString(CultureInfo.InvariantCulture)} ({synapseStat.Count})" + Environment.NewLine);
                    if (synapseStat.Count > 0)
                    {
                        sb.Append(leftMargin + $"       Distance: {StatLine(synapseStat.Distance)}" + Environment.NewLine);
                        sb.Append(leftMargin + $"          Delay: {StatLine(synapseStat.Delay)}" + Environment.NewLine);
                        sb.Append(leftMargin + $"         Weight: {StatLine(synapseStat.Weight)}" + Environment.NewLine);
                        sb.Append(leftMargin + $"        Efficacy statistics" + Environment.NewLine);
                        AppendStandardStatSet(margin + 12, sb, synapseStat.Efficacy);
                    }
                }
                return;
            }

            private void AppendNeuronAnomalies(int margin, StringBuilder sb, ReservoirStat.NeuronsAnomaliesStat nas)
            {
                string leftMargin = margin == 0 ? string.Empty : new string(' ', margin);
                sb.Append(leftMargin + $"Neurons anomalies" + Environment.NewLine);
                sb.Append(leftMargin + $"    {nas.NoResSynapses} neurons have no internal synapses from other reservoir neurons" + Environment.NewLine);
                sb.Append(leftMargin + $"    {nas.NoResStimuli} neurons receive no stimulation from the reservoir" + Environment.NewLine);
                sb.Append(leftMargin + $"    {nas.NoAnalogOutput} neurons generate zero analog signal" + Environment.NewLine);
                sb.Append(leftMargin + $"    {nas.ConstAnalogOutput} neurons generate constant nonzero analog signal" + Environment.NewLine);
                sb.Append(leftMargin + $"    {nas.NotFiring} neurons don't spike" + Environment.NewLine);
                sb.Append(leftMargin + $"    {nas.ConstFiring} neurons constantly fire" + Environment.NewLine);
                return;
            }

            /// <summary>
            /// Builds the text report.
            /// </summary>
            /// <param name="margin">Specifies the text left margin.</param>
            /// <returns>The built text report.</returns>
            public string CreateReport(int margin = 0)
            {
                string leftMargin = margin == 0 ? string.Empty : new string(' ', margin);
                string resWording = ReservoirStatCollection.Count == 1 ? "reservoir" : "reservoirs";
                StringBuilder sb = new StringBuilder();
                sb.Append(leftMargin + $"Neural preprocessor ({ReservoirStatCollection.Count} {resWording}, {TotalNumOfNeurons} neurons)" + Environment.NewLine);
                foreach (ReservoirStat resStat in ReservoirStatCollection)
                {
                    sb.Append(leftMargin + $"    Reservoir: {resStat.InstanceName} (configuration {resStat.StructCfgName}, {resStat.TotalNumOfNeurons} neurons)" + Environment.NewLine);
                    AppendNeuronAnomalies(margin + 8, sb, resStat.NeuronsAnomalies);
                    AppendSynapsesStat(margin + 8, sb, resStat.Synapses);
                    foreach (ReservoirStat.PoolStat poolStat in resStat.Pools)
                    {
                        sb.Append(leftMargin + $"        Pool: {poolStat.PoolName} ({poolStat.NumOfNeurons} neurons)" + Environment.NewLine);
                        AppendNeuronAnomalies(margin + 12, sb, poolStat.NeuronsAnomalies);
                        AppendSynapsesStat(margin + 12, sb, poolStat.Synapses);
                        foreach (ReservoirStat.PoolStat.NeuronGroupStat groupStat in poolStat.NeuronGroups)
                        {
                            sb.Append(leftMargin + $"            Group: {groupStat.GroupName} ({groupStat.NumOfNeurons} neurons)" + Environment.NewLine);
                            AppendNeuronAnomalies(margin + 16, sb, groupStat.NeuronsAnomalies);
                            AppendSynapsesStat(margin + 16, sb, groupStat.Synapses);
                            sb.Append(leftMargin + $"                Stimulation from input neurons" + Environment.NewLine);
                            AppendStandardStatSet(margin + 20, sb, groupStat.Stimuli.Input);
                            sb.Append(leftMargin + $"                Stimulation from reservoir neurons" + Environment.NewLine);
                            AppendStandardStatSet(margin + 20, sb, groupStat.Stimuli.Reservoir);
                            sb.Append(leftMargin + $"                Total stimulation (including Bias)" + Environment.NewLine);
                            AppendStandardStatSet(margin + 20, sb, groupStat.Stimuli.Total);
                            sb.Append(leftMargin + $"                Activation" + Environment.NewLine);
                            AppendStandardStatSet(margin + 20, sb, groupStat.Activation);
                            sb.Append(leftMargin + $"                Analog output" + Environment.NewLine);
                            AppendStandardStatSet(margin + 20, sb, groupStat.Signal.Analog);
                            sb.Append(leftMargin + $"                Firing output" + Environment.NewLine);
                            AppendStandardStatSet(margin + 20, sb, groupStat.Signal.Spiking);
                        }
                    }
                }
                sb.Append(Environment.NewLine);
                sb.Append(leftMargin + $"Total number of predictors: {TotalNumOfPredictors}, suppressed (unused) predictors: {NumOfSuppressedPredictors}, used predictors: {NumOfActivePredictors}" + Environment.NewLine);
                return sb.ToString();
            }

        }//PreprocessingOverview

    }//NeuralPreprocessor

}//Namespace
