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
    /// Implements the neural preprocessor of input data
    /// </summary>
    [Serializable]
    public class NeuralPreprocessor
    {
        //Delegates
        /// <summary>
        /// Delegate of PreprocessingProgressChanged event handler.
        /// </summary>
        /// <param name="totalNumOfInputs">Total number of inputs to be processed</param>
        /// <param name="numOfProcessedInputs">Number of processed inputs</param>
        /// <param name="finalPreprocessingOverview">Final overview of the preprocessing phase</param>
        public delegate void PreprocessingProgressChangedDelegate(int totalNumOfInputs,
                                                                  int numOfProcessedInputs,
                                                                  PreprocessingOverview finalPreprocessingOverview
                                                                  );
        //Events
        /// <summary>
        /// This informative event occurs every time the progress of neural preprocessing has changed
        /// </summary>
        [field: NonSerialized]
        public event PreprocessingProgressChangedDelegate PreprocessingProgressChanged;


        //Attribute properties
        /// <summary>
        /// Collection of reservoir instances.
        /// </summary>
        public List<ReservoirInstance> ReservoirCollection { get; }

        /// <summary>
        /// Number of boot cycles
        /// </summary>
        public int BootCycles { get; }

        /// <summary>
        /// Number of hidden neurons in all reservoirs
        /// </summary>
        public int TotalNumOfHiddenNeurons { get; }

        /// <summary>
        /// Descriptors of all output features in the same order as returns Preprocess method
        /// </summary>
        public List<PredictorDescriptor> OutputFeatureDescriptorCollection { get; private set; }

        /// <summary>
        /// Collection of switches generally enabling/disabling predictors
        /// </summary>
        public bool[] OutputFeatureGeneralSwitchCollection { get; private set; }

        /// <summary>
        /// Number of active output features (predictors + routed input)
        /// </summary>
        public int NumOfActiveOutputFeatures { get; private set; }


        //Attributes
        /// <summary>
        /// Settings used for instance creation.
        /// </summary>
        private readonly NeuralPreprocessorSettings _preprocessorCfg;
        private readonly InputEncoder _inputEncoder;
        private List<int> _predictorsTimePointSlicesPlan;
        private int _totalNumOfReservoirsPredictors;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="preprocessorCfg">Neural Preprocessor's configuration</param>
        /// <param name="randomizerSeek">
        /// A value greater than or equal to 0 will always ensure the same initialization of the internal
        /// random number generator and therefore the same network structure, which is good for tuning
        /// network parameters. A value less than 0 causes a fully random initialization when creating a network instance.
        /// </param>
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
            OutputFeatureDescriptorCollection = null;
            OutputFeatureGeneralSwitchCollection = null;
            NumOfActiveOutputFeatures = 0;
            return;
        }

        //Properties
        /// <summary>
        /// Indicates bidirectional input processing
        /// </summary>
        private bool Bidir { get { return _preprocessorCfg.InputEncoderCfg.FeedingCfg.FeedingType == InputEncoder.InputFeedingType.Patterned && ((FeedingPatternedSettings)_preprocessorCfg.InputEncoderCfg.FeedingCfg).Bidir; } }

        /// <summary>
        /// Number of suppressed output features (exhibits no meaningfully different values or directly reduced by setup parameters)
        /// </summary>
        public int NumOfSuppressedOutputFeatures { get { return OutputFeatureDescriptorCollection.Count - NumOfActiveOutputFeatures; } }

        //Methods
        /// <summary>
        /// Output features comparer (sorts desc by value span)
        /// </summary>
        /// <param name="f1">Feature 1</param>
        /// <param name="f2">Feature 2</param>
        public static int CompareOutputFeature(Tuple<int, double> f1, Tuple<int, double> f2)
        {
            if (f1.Item2 > f2.Item2)
            {
                return -1;
            }
            else if (f1.Item2 < f2.Item2)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Initializes collection of descriptors of output features
        /// </summary>
        private void InitOutputFeaturesDescriptors()
        {
            //Final output features collection
            OutputFeatureDescriptorCollection = new List<PredictorDescriptor>();
            //Routed input values
            if(_inputEncoder.NumOfRoutedValues > 0)
            {
                OutputFeatureDescriptorCollection.AddRange(_inputEncoder.GetInputValuesPredictorsDescriptors());
            }
            //Hidden neurons predictors
            List<PredictorDescriptor> reservoirsPredictorDescriptorCollection = new List<PredictorDescriptor>();
            foreach (ReservoirInstance reservoir in ReservoirCollection)
            {
                reservoirsPredictorDescriptorCollection.AddRange(reservoir.GetPredictorsDescriptors());
            }
            if(_preprocessorCfg.InputEncoderCfg.FeedingCfg.FeedingType == InputEncoder.InputFeedingType.Continuous)
            {
                //Continuous feeding
                OutputFeatureDescriptorCollection.AddRange(reservoirsPredictorDescriptorCollection);
                _totalNumOfReservoirsPredictors = reservoirsPredictorDescriptorCollection.Count;
            }
            else
            {
                //Patterned feeding
                FeedingPatternedSettings patternedCfg = (FeedingPatternedSettings)_preprocessorCfg.InputEncoderCfg.FeedingCfg;
                for (int i = 0; i < (patternedCfg.Bidir ? 2 : 1); i++)
                {
                    for(int j = 0; j < patternedCfg.Slices; j++)
                    {
                        OutputFeatureDescriptorCollection.AddRange(reservoirsPredictorDescriptorCollection);
                        _totalNumOfReservoirsPredictors += reservoirsPredictorDescriptorCollection.Count;
                    }
                }
                //Predictors time-point slices plan
                if (_inputEncoder.NumOfTimePoints != InputEncoder.VariableNumOfTimePoints)
                {
                    //Check correctness
                    if(patternedCfg.Slices > _inputEncoder.NumOfTimePoints)
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
                        if(roundedTimePoint != lastTimePoint)
                        {
                            _predictorsTimePointSlicesPlan.Insert(0, roundedTimePoint);
                            lastTimePoint = roundedTimePoint;
                        }
                        countDown -= avgDistance;
                    }
                    //Second phase - distribution of remaining time-points
                    while (_predictorsTimePointSlicesPlan.Count < patternedCfg.Slices)
                    {
                        for(int i = _predictorsTimePointSlicesPlan.Count - 2; i > -1; i--)
                        {
                            int span = _predictorsTimePointSlicesPlan[i + 1] - (i >= 0 ? _predictorsTimePointSlicesPlan[i] : 1);
                            if (span > 1)
                            {
                                int timePoint = (i >= 0 ? _predictorsTimePointSlicesPlan[i] : 1) + (int)Math.Round(span / 2d, 0, MidpointRounding.AwayFromZero);
                                _predictorsTimePointSlicesPlan.Insert(i + 1, timePoint);
                                if(_predictorsTimePointSlicesPlan.Count == patternedCfg.Slices)
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
        /// Function checks given output features and sets general enabling/disabling switches
        /// </summary>
        /// <param name="predictorsCollection">Collection of regression predictors</param>
        private void InitOutputFeaturesGeneralSwitches(List<double[]> predictorsCollection)
        {
            //Allocate general switches
            OutputFeatureGeneralSwitchCollection = new bool[OutputFeatureDescriptorCollection.Count];
            //Init general predictor switches to false
            OutputFeatureGeneralSwitchCollection.Populate(false);
            //Compute statistics on predictors
            Tuple<int, double>[] predictorValueSpanCollection = new Tuple<int, double>[OutputFeatureDescriptorCollection.Count];
            Parallel.For(0, OutputFeatureDescriptorCollection.Count, i =>
            {
                BasicStat stat = new BasicStat();
                for (int row = 0; row < predictorsCollection.Count; row++)
                {
                    stat.AddSampleValue(predictorsCollection[row][i]);
                }
                //Use predictor's value span as a differentiator
                predictorValueSpanCollection[i] = new Tuple<int, double>(i, stat.Span);
            });
            //Sort collected predictor differentiators
            Array.Sort(predictorValueSpanCollection, CompareOutputFeature);
            //Enable predictors
            int numOfPredictorsToBeRejected = (int)(Math.Round(OutputFeatureDescriptorCollection.Count * _preprocessorCfg.PredictorsReductionRatio));
            int firstIndexToBeRejected = predictorValueSpanCollection.Length - numOfPredictorsToBeRejected;
            NumOfActiveOutputFeatures = 0;
            for (int i = 0; i < predictorValueSpanCollection.Length; i++)
            {
                if (predictorValueSpanCollection[i].Item2 > _preprocessorCfg.PredictorValueMinSpan && i < firstIndexToBeRejected)
                {
                    //Enable predictor
                    OutputFeatureGeneralSwitchCollection[predictorValueSpanCollection[i].Item1] = true;
                    ++NumOfActiveOutputFeatures;
                }
            }
            return;
        }

        /// <summary>
        /// Sets neural preprocessor's internal state to its initial state
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
            NumOfActiveOutputFeatures = 0;
            OutputFeatureDescriptorCollection = null;
            OutputFeatureGeneralSwitchCollection = null;
            return;
        }

        /// <summary>
        /// Sets reservoirs to initial state
        /// </summary>
        /// <param name="resetStatistics">Specifies whether to reset internal statistics</param>
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
        /// Preprocesses all pending input data prepared by InputEncoder and collects reservoirs' predictors
        /// </summary>
        /// <param name="collectStatistics">Indicates whether to update internal statistics</param>
        private double[] ProcessPendingData(bool collectStatistics)
        {
            double[] predictors = new double[_totalNumOfReservoirsPredictors / (Bidir ? 2 : 1)];
            int predictorsIdx = 0;
            int predictorsTimePointSlicesPlanIdx = 0;
            int computationStep = 1;
            //Reset reservoirs in case of patterned feeding
            if (_preprocessorCfg.InputEncoderCfg.FeedingCfg.FeedingType == InputEncoder.InputFeedingType.Patterned)
            {
                ResetReservoirs(false);
            }
            //Loop pending data
            while (_inputEncoder.NumOfRemainingInputs > 0)
            {
                _inputEncoder.EncodeNextInputData(collectStatistics);
                //Compute reservoir(s)
                foreach (ReservoirInstance reservoir in ReservoirCollection)
                {
                    reservoir.Compute(collectStatistics);
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
        /// Pushes external input vector into the input encoder, computes reservoirs and returns output features
        /// </summary>
        /// <param name="inputVector">External input values</param>
        /// <param name="collectStatistics">Indicates whether to update internal statistics</param>
        private double[] PushExtInputVector(double[] inputVector, bool collectStatistics)
        {
            //Output features buffer allocation and index
            double[] outputFeatures = new double[OutputFeatureDescriptorCollection.Count];
            int outputFeaturesIdx = 0;
            //Put new data into the InputEncoder
            _inputEncoder.StoreNewData(inputVector);
            //Collect routed input data
            outputFeaturesIdx += _inputEncoder.CopyRoutedInputDataTo(outputFeatures, outputFeaturesIdx);
            //Process input data in reservoirs and collect predictors
            double[] predictors = ProcessPendingData(collectStatistics);
            predictors.CopyTo(outputFeatures, outputFeaturesIdx);
            outputFeaturesIdx += predictors.Length;
            //Bidirectional input processing?
            if (Bidir)
            {
                //Set reverse mode
                _inputEncoder.SetReverseMode();
                //Process reversed input data in reservoirs
                predictors = ProcessPendingData(collectStatistics);
                predictors.CopyTo(outputFeatures, outputFeaturesIdx);
            }
            return outputFeatures;
        }

        /// <summary>
        /// Pushes input data into the preprocessor and returns output features (predictors)
        /// </summary>
        /// <param name="input">Input values in natural form</param>
        public double[] Preprocess(double[] input)
        {
            if (OutputFeatureGeneralSwitchCollection == null)
            {
                throw new InvalidOperationException($"Preprocessor is not initialized. Call InitializeAndPreprocessBundle method first.");
            }
            return PushExtInputVector(input, false);
        }

        /// <summary>
        /// Prepares input for Readout Layer training.
        /// All input vectors are processed by internal reservoirs and the corresponding network predictors are recorded.
        /// Function also rejects unusable predictors having no reasonable fluctuation of values.
        /// Raises PreprocessingProgressChanged event.
        /// </summary>
        /// <param name="inputBundle">The bundle containing inputs and desired outputs</param>
        /// <param name="preprocessingOverview">Reservoir(s) statistics and other important information as a result of the preprocessing phase.</param>
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
            InitOutputFeaturesDescriptors();
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
                                                              OutputFeatureDescriptorCollection.Count,
                                                              NumOfSuppressedOutputFeatures,
                                                              NumOfActiveOutputFeatures
                                                              );
            //Raise final informative event
            PreprocessingProgressChanged(inputBundle.InputVectorCollection.Count, inputBundle.InputVectorCollection.Count, preprocessingOverview);
            //Return output
            return outputBundle;
        }

        /// <summary>
        /// Collects key statistics of each reservoir instance.
        /// It is very important to follow these statistics to make sure the reservoirs exhibit proper behavior.
        /// </summary>
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
        /// Reservoir(s) statistics and other important information as a result of the preprocessing phase
        /// </summary>
        [Serializable]
        public class PreprocessingOverview
        {
            //Attribute properties
            /// <summary>
            /// Collection of statistics of NeuralPreprocessor's internal reservoirs
            /// </summary>
            public List<ReservoirStat> ReservoirStatCollection { get; }
            /// <summary>
            /// Total number of NeuralPreprocessor's neurons
            /// </summary>
            public int TotalNumOfNeurons { get; }
            /// <summary>
            /// Number of predictors
            /// </summary>
            public int TotalNumOfPredictors { get; }
            /// <summary>
            /// Number of suppressed predictors
            /// </summary>
            public int NumOfSuppressedPredictors { get; }
            /// <summary>
            /// Number of active predictors
            /// </summary>
            public int NumOfActivePredictors { get; }

            //Constructor
            /// <summary>
            /// Creates an initialized instance
            /// </summary>
            /// <param name="reservoirStatCollection">Collection of statistics of NeuralPreprocessor's internal reservoirs</param>
            /// <param name="totalNumOfNeurons">Total number of NeuralPreprocessor's neurons</param>
            /// <param name="totalNumOfPredictors">Number of NeuralPreprocessor's predictors</param>
            /// <param name="numOfSuppressedPredictors">Number of NeuralPreprocessor's suppressed predictors</param>
            /// <param name="numOfActivePredictors">Number of NeuralPreprocessor's active predictors</param>
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
            /// Builds report of key statistics collected from all the NeuralPreprocessor's reservoirs
            /// </summary>
            /// <param name="margin">Specifies how many spaces should be at the begining of each row.</param>
            /// <returns>Built text report</returns>
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
                            AppendStandardStatSet(margin + 20, sb, groupStat.Signal.Firing);
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
