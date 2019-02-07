using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Data;
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
        //Attributes
        /// <summary>
        /// Settings used for instance creation.
        /// </summary>
        private StateMachineSettings _settings;

        //Attribute properties
        /// <summary>
        /// Neural preprocessor.
        /// </summary>
        public NeuralPreprocessor NP { get; private set; }
        /// <summary>
        /// Readout layer.
        /// </summary>
        public ReadoutLayer RL { get; private set; }

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
            //Readout layer
            RL = null;
            return;
        }

        //Properties

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
            return new RegressionInput(NP.InitializeAndPreprocessBundle(patternBundle, informativeCallback, userObject),
                                       NP.CollectStatatistics(),
                                       NP.NumOfNeurons,
                                       NP.NumOfInternalSynapses
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
            return new RegressionInput(NP.InitializeAndPreprocessBundle(vectorBundle, informativeCallback, userObject),
                                       NP.CollectStatatistics(),
                                       NP.NumOfNeurons,
                                       NP.NumOfInternalSynapses
                                       );
        }


        /// <summary>
        /// Creates and trains the State Machine readout layer.
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
            //Optional mapper of predictors to readout units
            ReadoutLayer.PredictorsMapper mapper = null;
            if(_settings.MapperConfig != null)
            {
                //Create empty instance of the mapper
                mapper = new ReadoutLayer.PredictorsMapper(NP.NumOfPredictors);
                //Expand list of predicting neurons to array of predictor origin
                StateMachineSettings.MapperSettings.PoolRef[] neuronPoolRefCollection = new StateMachineSettings.MapperSettings.PoolRef[NP.NumOfPredictors];
                int idx = 0;
                foreach(Reservoir.PredictorNeuron pn in NP.PredictorNeuronCollection)
                {
                    neuronPoolRefCollection[idx] = new StateMachineSettings.MapperSettings.PoolRef { _reservoirInstanceIdx = pn.Neuron.Placement.ReservoirID, _poolIdx = pn.Neuron.Placement.PoolID };
                    ++idx;
                    if(pn.UseSecondaryPredictor)
                    {
                        neuronPoolRefCollection[idx] = neuronPoolRefCollection[idx - 1];
                        ++idx;
                    }
                }
                //Iterate readout units having specific predictors mapping
                foreach (string readoutUnitName in _settings.MapperConfig.Map.Keys)
                {
                    bool[] switches = new bool[NP.NumOfPredictors];
                    switches.Populate(false);
                    foreach(StateMachineSettings.MapperSettings.PoolRef allowedPool in _settings.MapperConfig.Map[readoutUnitName])
                    {
                        //Enable specific predictors from allowed pool (origin)
                        for(int i = 0; i < neuronPoolRefCollection.Length; i++)
                        {
                            if(neuronPoolRefCollection[i]._reservoirInstanceIdx == allowedPool._reservoirInstanceIdx && neuronPoolRefCollection[i]._poolIdx == allowedPool._poolIdx)
                            {
                                switches[i] = true;
                            }
                        }
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
            public RegressionInput(VectorBundle preprocessedData,
                                   List<ReservoirStat> reservoirStatCollection,
                                   int totalNumOfNeurons,
                                   int totalNumOfInternalSynapses
                                   )
            {
                PreprocessedData = preprocessedData;
                ReservoirStatCollection = reservoirStatCollection;
                TotalNumOfNeurons = totalNumOfNeurons;
                TotalNumOfInternalSynapses = totalNumOfInternalSynapses;
                return;
            }

            //Methods
            private string FNum(double num)
            {
                return num.ToString("N4", CultureInfo.InvariantCulture).PadLeft(7);
            }

            private string StatLine(BasicStat stat)
            {
                return $"Avg:{FNum(stat.ArithAvg)},  Max:{FNum(stat.Max)},  Min:{FNum(stat.Min)},  SDdev:{FNum(stat.StdDev)}";
            }

            /// <summary>
            /// Builds report of key statistics collected from NeuralPreprocessor's reservoir(s)
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
                    sb.Append(leftMargin + $"  Reservoir instance: {resStat.ReservoirInstanceName} (configuration {resStat.ReservoirSettingsName}, {resStat.TotalNumOfNeurons} neurons, {resStat.TotalNumOfInternalSynapses} internal synapses)" + Environment.NewLine);
                    foreach (ReservoirStat.PoolStat poolStat in resStat.PoolStatCollection)
                    {
                        sb.Append(leftMargin + $"    Pool: {poolStat.PoolName} ({poolStat.NumOfNeurons} neurons)" + Environment.NewLine);
                        foreach (ReservoirStat.PoolStat.NeuronGroupStat groupStat in poolStat.NeuronGroupStatCollection)
                        {
                            sb.Append(leftMargin + $"      Group of neurons: {groupStat.GroupName} ({groupStat.AvgOutputSignalStat.NumOfSamples} neurons)" + Environment.NewLine);
                            sb.Append(leftMargin + $"        Stimulation" + Environment.NewLine);
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
                            sb.Append(leftMargin + $"         SPAN>  {StatLine(groupStat.ActivationStateSpansStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"        Output signal" + Environment.NewLine);
                            sb.Append(leftMargin + $"          AVG>  {StatLine(groupStat.AvgOutputSignalStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"          MAX>  {StatLine(groupStat.MaxOutputSignalStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"          MIN>  {StatLine(groupStat.MinOutputSignalStat)}" + Environment.NewLine);
                        }
                        sb.Append(leftMargin + $"      Weights of synapses" + Environment.NewLine);
                        sb.Append(leftMargin + $"        Inp.>  {StatLine(poolStat.InputWeightsStat)}" + Environment.NewLine);
                        sb.Append(leftMargin + $"        Int.>  {StatLine(poolStat.InternalWeightsStat)}" + Environment.NewLine);
                    }
                }
                return sb.ToString();
            }

        }//RegressionInput

    }//StateMachine
}//Namespace
