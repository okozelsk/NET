using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Data;
using RCNet.Neural.Network.SM.Neuron;
using RCNet.Neural.Network.SM.Preprocessing;
using RCNet.Neural.Network.SM.Readout;


namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Implements the State Machine.
    /// </summary>
    [Serializable]
    public class StateMachine
    {
        //Constants
        private const double MinPredictorValueDifference = 1e-6;
        //Attribute properties
        /// <summary>
        /// Neural preprocessor.
        /// </summary>
        public NeuralPreprocessor NP { get; private set; }
        /// <summary>
        /// Number of predictors exhibits useable values (produces meaningfully different values)
        /// </summary>
        public int NumOfValidPredictors { get; private set; }
        /// <summary>
        /// Collection of switches generally enabling/disabling predictors
        /// </summary>
        public bool[] PredictorGeneralSwitchCollection { get; private set; }
        /// <summary>
        /// Readout layer.
        /// </summary>
        public ReadoutLayer RL { get; private set; }

        //Attributes
        /// <summary>
        /// Settings used for instance creation.
        /// </summary>
        private readonly StateMachineSettings _settings;

        //Constructor
        /// <summary>
        /// Constructs an instance of State Machine
        /// </summary>
        /// <param name="settings">State Machine settings</param>
        public StateMachine(StateMachineSettings settings)
        {
            _settings = settings.DeepClone();
            //Neural preprocessor instance
            NP = new NeuralPreprocessor(settings.NeuralPreprocessorConfig, settings.RandomizerSeek);
            NumOfValidPredictors = 0;
            PredictorGeneralSwitchCollection = null;
            //Readout layer
            RL = null;
            return;
        }

        //Properties
        /// <summary>
        /// Number of invalid predictors (exhibits no meaningfully different values)
        /// </summary>
        public int NumOfUnusedPredictors { get { return NP.NumOfPredictors - NumOfValidPredictors; } }

        //Methods
        /// <summary>
        /// Sets State Machine internal state to its initial state
        /// </summary>
        public void Reset()
        {
            //Neural preprocessor reset
            NP.Reset(true);
            //Get rid the ReadoutLayer instance
            RL = null;
            return;
        }

        /// <summary>
        /// Function checks given predictors and set general enabling/disabling switches of predictors
        /// </summary>
        /// <param name="predictorsCollection">Collection of regression predictors</param>
        private void InitPredictorsGeneralSwitches(List<double[]> predictorsCollection)
        {
            PredictorGeneralSwitchCollection = new bool[NP.NumOfPredictors];
            PredictorGeneralSwitchCollection.Populate(false);
            //Test predictors
            NumOfValidPredictors = 0;
            for (int row = 1; row < predictorsCollection.Count; row++)
            {
                for (int i = 0; i < PredictorGeneralSwitchCollection.Length; i++)
                {
                    if (Math.Abs(predictorsCollection[row][i] - predictorsCollection[row - 1][i]) > MinPredictorValueDifference)
                    {
                        //Update number of valid predictors
                        NumOfValidPredictors += PredictorGeneralSwitchCollection[i] ? 0 : 1;
                        //Predictor exhibits different values => enable it
                        PredictorGeneralSwitchCollection[i] = true;
                    }
                }
                if (NumOfValidPredictors == PredictorGeneralSwitchCollection.Length) break;
            }
            return;
        }

        /// <summary>
        /// Prepares input for regression stage of State Machine training.
        /// All input patterns are processed by internal reservoirs and the corresponding network predictors are recorded.
        /// </summary>
        /// <param name="patternBundle">
        /// The bundle containing known sample input patterns and desired output vectors
        /// </param>
        /// <param name="informativeCallback">
        /// Function to be called after each processed input.
        /// </param>
        /// <param name="userObject">
        /// The user object to be passed to informativeCallback.
        /// </param>
        public RegressionInput PrepareRegressionData(PatternBundle patternBundle,
                                                     NeuralPreprocessor.PredictorsCollectionCallbackDelegate informativeCallback = null,
                                                     Object userObject = null
                                                     )
        {
            VectorBundle preprocessedData = NP.InitializeAndPreprocessBundle(patternBundle, informativeCallback, userObject);
            InitPredictorsGeneralSwitches(preprocessedData.InputVectorCollection);
            return new RegressionInput(preprocessedData,
                                       NP.CollectStatatistics(),
                                       NP.NumOfNeurons,
                                       NP.NumOfInternalSynapses,
                                       NumOfUnusedPredictors
                                       );
        }

        /// <summary>
        /// Prepares input for regression stage of State Machine training.
        /// All input vectors are processed by internal reservoirs and the corresponding network predictors are recorded.
        /// </summary>
        /// <param name="vectorBundle">
        /// The bundle containing known sample input and desired output vectors (in time order)
        /// </param>
        /// <param name="informativeCallback">
        /// Function to be called after each processed input.
        /// </param>
        /// <param name="userObject">
        /// The user object to be passed to informativeCallback.
        /// </param>
        public RegressionInput PrepareRegressionData(VectorBundle vectorBundle,
                                                     NeuralPreprocessor.PredictorsCollectionCallbackDelegate informativeCallback = null,
                                                     Object userObject = null
                                                     )
        {
            VectorBundle preprocessedData = NP.InitializeAndPreprocessBundle(vectorBundle, informativeCallback, userObject);
            InitPredictorsGeneralSwitches(preprocessedData.InputVectorCollection);
            return new RegressionInput(preprocessedData,
                                       NP.CollectStatatistics(),
                                       NP.NumOfNeurons,
                                       NP.NumOfInternalSynapses,
                                       NumOfUnusedPredictors
                                       );
        }


        /// <summary>
        /// Creates and trains the State Machine readout layer.
        /// Function uses specific mapping of predictors to readout units, if available.
        /// Function also rejects unusable predictors having no reasonable fluctuation of values.
        /// </summary>
        /// <param name="regressionInput">
        /// RegressionInput object prepared by PrepareRegressionData function
        /// </param>
        /// <param name="regressionController">
        /// Optional. see Regression.RegressionCallbackDelegate
        /// </param>
        /// <param name="regressionControllerData">
        /// Optional custom object to be passed to regressionController together with other standard information
        /// </param>
        public ResultComparativeBundle BuildReadoutLayer(RegressionInput regressionInput,
                                                         ReadoutUnit.RegressionCallbackDelegate regressionController = null,
                                                         Object regressionControllerData = null
                                                         )
        {
            //Readout layer instance
            RL = new ReadoutLayer(_settings.ReadoutLayerConfig);
            //Create empty instance of the mapper
            ReadoutLayer.PredictorsMapper mapper = new ReadoutLayer.PredictorsMapper(PredictorGeneralSwitchCollection);
            if (_settings.MapperConfig != null)
            {
                //Expand list of predicting neurons to array of predictor origin
                StateMachineSettings.MapperSettings.PoolRef[] neuronPoolRefCollection = new StateMachineSettings.MapperSettings.PoolRef[NP.NumOfPredictors];
                int idx = 0;
                foreach(HiddenNeuron neuron in NP.PredictorNeuronCollection)
                {
                    for(int i = 0; i < neuron.PredictorsCfg.NumOfEnabledPredictors; i++)
                    {
                        neuronPoolRefCollection[idx] = new StateMachineSettings.MapperSettings.PoolRef { _reservoirInstanceIdx = neuron.Placement.ReservoirID, _poolIdx = neuron.Placement.PoolID };
                        ++idx;
                    }
                }
                //Iterate all readout units
                foreach (string readoutUnitName in _settings.ReadoutLayerConfig.OutputFieldNameCollection)
                {
                    bool[] switches = new bool[NP.NumOfPredictors];
                    //Exists specific mapping?
                    if(_settings.MapperConfig != null && _settings.MapperConfig.Map.ContainsKey(readoutUnitName))
                    {
                        switches.Populate(false);
                        foreach (StateMachineSettings.MapperSettings.PoolRef allowedPool in _settings.MapperConfig.Map[readoutUnitName])
                        {
                            //Enable specific predictors from allowed pool (origin)
                            for (int i = 0; i < neuronPoolRefCollection.Length; i++)
                            {
                                if (neuronPoolRefCollection[i]._reservoirInstanceIdx == allowedPool._reservoirInstanceIdx && neuronPoolRefCollection[i]._poolIdx == allowedPool._poolIdx)
                                {
                                    //Enable predictor if it is valid
                                    switches[i] = PredictorGeneralSwitchCollection[i];
                                }
                            }
                        }
                    }
                    else
                    {
                        //Initially allow all valid predictors
                        PredictorGeneralSwitchCollection.CopyTo(switches, 0);
                    }
                    //Add mapping to mapper
                    mapper.Add(readoutUnitName, switches);
                }
            }
            //Training
            return RL.Build(regressionInput.PreprocessedData,
                            regressionController,
                            regressionControllerData,
                            mapper
                            );
        }


        /// <summary>
        /// Compute function for a patterned input feeding.
        /// Processes given input pattern and computes the output.
        /// </summary>
        /// <param name="inputPattern">Input pattern</param>
        /// <returns>Computed output values</returns>
        public double[] Compute(List<double[]> inputPattern)
        {
            if (_settings.NeuralPreprocessorConfig.InputConfig.FeedingType == CommonEnums.InputFeedingType.Continuous)
            {
                throw new Exception("This version of Compute function is not useable for continuous input feeding.");
            }
            if (RL == null)
            {
                throw new Exception("Readout layer is not trained.");
            }
            //Compute and return output
            return RL.Compute(NP.Preprocess(inputPattern));
        }

        /// <summary>
        /// Compute fuction for a continuous input feeding.
        /// Processes given input values and computes (predicts) the output.
        /// </summary>
        /// <param name="inputVector">Input values</param>
        /// <returns>Computed output values</returns>
        public double[] Compute(double[] inputVector)
        {
            if (_settings.NeuralPreprocessorConfig.InputConfig.FeedingType == CommonEnums.InputFeedingType.Patterned)
            {
                throw new Exception("This version of Compute function is not useable for patterned input feeding.");
            }
            if (RL == null)
            {
                throw new Exception("Readout layer is not trained.");
            }
            //Compute and return output
            return RL.Compute(NP.Preprocess(inputVector));
        }

        //Inner classes
        /// <summary>
        /// Contains prepared data for the regression stage and important statistics of the reservoir(s)
        /// </summary>
        [Serializable]
        public class RegressionInput
        {
            //Attribute properties
            /// <summary>
            /// Bundle of the NeuralPreprocessor's predictors and desired ideal outputs
            /// </summary>
            public VectorBundle PreprocessedData { get; }
            /// <summary>
            /// Collection of statistics of NeuralPreprocessor's internal reservoirs
            /// </summary>
            public List<ReservoirStat> ReservoirStatCollection { get; }
            /// <summary>
            /// Total number of NeuralPreprocessor's neurons
            /// </summary>
            public int TotalNumOfNeurons { get; }
            /// <summary>
            /// Number of invalid predictors (exhibits no meaningfully different values)
            /// </summary>
            public int NumOfUnusedPredictors { get; }
            /// <summary>
            /// Total number of NeuralPreprocessor's internal synapses
            /// </summary>
            public int TotalNumOfInternalSynapses { get; }

            //Constructor
            /// <summary>
            /// Creates an initialized instance
            /// </summary>
            /// <param name="preprocessedData">Bundle of the NeuralPreprocessor's predictors and desired ideal outputs</param>
            /// <param name="reservoirStatCollection">Collection of statistics of NeuralPreprocessor's internal reservoirs</param>
            /// <param name="totalNumOfNeurons">Total number of NeuralPreprocessor's neurons</param>
            /// <param name="totalNumOfInternalSynapses">Total number of NeuralPreprocessor's internal synapses</param>
            /// <param name="numOfUnusedPredictors">Number of NeuralPreprocessor's invalid predictors</param>
            public RegressionInput(VectorBundle preprocessedData,
                                   List<ReservoirStat> reservoirStatCollection,
                                   int totalNumOfNeurons,
                                   int totalNumOfInternalSynapses,
                                   int numOfUnusedPredictors
                                   )
            {
                PreprocessedData = preprocessedData;
                ReservoirStatCollection = reservoirStatCollection;
                TotalNumOfNeurons = totalNumOfNeurons;
                TotalNumOfInternalSynapses = totalNumOfInternalSynapses;
                NumOfUnusedPredictors = numOfUnusedPredictors;
                return;
            }

            //Methods
            private string FNum(double num)
            {
                return num.ToString("N8", CultureInfo.InvariantCulture).PadLeft(12);
            }

            private string StatLine(BasicStat stat)
            {
                return $"Avg:{FNum(stat.ArithAvg)},  Max:{FNum(stat.Max)},  Min:{FNum(stat.Min)},  SDdev:{FNum(stat.StdDev)}";
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
                sb.Append(leftMargin + $"Neural preprocessor ({ReservoirStatCollection.Count} {resWording}, {TotalNumOfNeurons} neurons, {TotalNumOfInternalSynapses} internal synapses)" + Environment.NewLine);
                foreach (ReservoirStat resStat in ReservoirStatCollection)
                {
                    sb.Append(leftMargin + $"  Reservoir instance: {resStat.ReservoirInstanceName} (configuration {resStat.ReservoirSettingsName}, {resStat.TotalNumOfNeurons} neurons, {Math.Round(resStat.ExcitatoryNeuronsRatio * 100, 1).ToString(CultureInfo.InvariantCulture)}% excitatory neurons, {resStat.TotalNumOfInternalSynapses} internal synapses)" + Environment.NewLine);
                    sb.Append(leftMargin + $"    Zero incoming res. stimuli : {resStat.NumOfNoRStimuliNeurons} neurons" + Environment.NewLine);
                    sb.Append(leftMargin + $"    Zero emitted output signal : {resStat.NumOfNoOutputSignalNeurons} neurons" + Environment.NewLine);
                    sb.Append(leftMargin + $"    Const emitted output signal: {resStat.NumOfConstOutputSignalNeurons} neurons" + Environment.NewLine);
                    foreach (ReservoirStat.PoolStat poolStat in resStat.PoolStatCollection)
                    {
                        sb.Append(leftMargin + $"    Pool: {poolStat.PoolName} ({poolStat.NumOfNeurons} neurons, {Math.Round(poolStat.ExcitatoryNeuronsRatio * 100, 1).ToString(CultureInfo.InvariantCulture)}% excitatory neurons, {poolStat.InternalWeightsStat.NumOfSamples} internal synapses)" + Environment.NewLine);
                        sb.Append(leftMargin + $"      Zero incoming res. stimuli : {poolStat.NumOfNoRStimuliNeurons} neurons" + Environment.NewLine);
                        sb.Append(leftMargin + $"      Zero emitted output signal : {poolStat.NumOfNoOutputSignalNeurons} neurons" + Environment.NewLine);
                        sb.Append(leftMargin + $"      Const emitted output signal: {poolStat.NumOfConstOutputSignalNeurons} neurons" + Environment.NewLine);
                        foreach (ReservoirStat.PoolStat.NeuronGroupStat groupStat in poolStat.NeuronGroupStatCollection)
                        {
                            sb.Append(leftMargin + $"      Group of neurons: {groupStat.GroupName} ({groupStat.AvgOutputSignalStat.NumOfSamples} neurons)" + Environment.NewLine);
                            sb.Append(leftMargin + $"        Zero incoming res. stimuli : {groupStat.NumOfNoRStimuliNeurons} neurons" + Environment.NewLine);
                            sb.Append(leftMargin + $"        Zero emitted output signal : {groupStat.NumOfNoOutputSignalNeurons} neurons" + Environment.NewLine);
                            sb.Append(leftMargin + $"        Const emitted output signal: {groupStat.NumOfConstOutputSignalNeurons} neurons" + Environment.NewLine);
                            sb.Append(leftMargin + $"        Stimulation from Input neurons " + Environment.NewLine);
                            sb.Append(leftMargin + $"          AVG>  {StatLine(groupStat.AvgIStimuliStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"          MAX>  {StatLine(groupStat.MaxIStimuliStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"          MIN>  {StatLine(groupStat.MinIStimuliStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"         SPAN>  {StatLine(groupStat.IStimuliSpansStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"        Stimulation from Reservoir neurons" + Environment.NewLine);
                            sb.Append(leftMargin + $"          AVG>  {StatLine(groupStat.AvgRStimuliStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"          MAX>  {StatLine(groupStat.MaxRStimuliStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"          MIN>  {StatLine(groupStat.MinRStimuliStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"         SPAN>  {StatLine(groupStat.RStimuliSpansStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"        Total stimulation (including Bias)" + Environment.NewLine);
                            sb.Append(leftMargin + $"          AVG>  {StatLine(groupStat.AvgTStimuliStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"          MAX>  {StatLine(groupStat.MaxTStimuliStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"          MIN>  {StatLine(groupStat.MinTStimuliStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"         SPAN>  {StatLine(groupStat.TStimuliSpansStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"        Efficacy of synapses" + Environment.NewLine);
                            sb.Append(leftMargin + $"          AVG>  {StatLine(groupStat.AvgSynEfficacyStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"          MAX>  {StatLine(groupStat.MaxSynEfficacyStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"          MIN>  {StatLine(groupStat.MinSynEfficacyStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"         SPAN>  {StatLine(groupStat.SynEfficacySpansStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"        Activation" + Environment.NewLine);
                            sb.Append(leftMargin + $"          AVG>  {StatLine(groupStat.AvgActivationStatesStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"          MAX>  {StatLine(groupStat.MaxActivationStatesStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"          MIN>  {StatLine(groupStat.MinActivationStatesStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"         SPAN>  {StatLine(groupStat.ActivationStateSpansStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"        Output signal" + Environment.NewLine);
                            sb.Append(leftMargin + $"          AVG>  {StatLine(groupStat.AvgOutputSignalStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"          MAX>  {StatLine(groupStat.MaxOutputSignalStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"          MIN>  {StatLine(groupStat.MinOutputSignalStat)}" + Environment.NewLine);
                        }
                        sb.Append(leftMargin + $"      Initial weights of synapses" + Environment.NewLine);
                        sb.Append(leftMargin + $"        Inp.>  {StatLine(poolStat.InputWeightsStat)}" + Environment.NewLine);
                        sb.Append(leftMargin + $"        Int.>  {StatLine(poolStat.InternalWeightsStat)}" + Environment.NewLine);
                    }
                }
                sb.Append(leftMargin + $"Number of unused (invalid) predictors: {NumOfUnusedPredictors}" + Environment.NewLine);
                return sb.ToString();
            }

        }//RegressionInput

    }//StateMachine
}//Namespace
