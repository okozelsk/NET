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
            RegressionInput regrInput = new RegressionInput
            {
                PreprocessedData = NP.InitializeAndPreprocessBundle(patternBundle, informativeCallback, userObject),
                ReservoirStatCollection = NP.CollectStatatistics()
            };
            return regrInput;
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
            RegressionInput regrInput = new RegressionInput
            {
                PreprocessedData = NP.InitializeAndPreprocessBundle(vectorBundle, informativeCallback, userObject),
                ReservoirStatCollection = NP.CollectStatatistics()
            };
            return regrInput;
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
        /// Contains prepared data for regression stage and statistics of the reservoir(s)
        /// </summary>
        [Serializable]
        public class RegressionInput
        {
            //Attribute properties
            /// <summary>
            /// Collection of the predictors and ideal outputs
            /// </summary>
            public VectorBundle PreprocessedData { get; set; } = null;
            /// <summary>
            /// Collection of statistics of the State Machine's reservoir(s)
            /// </summary>
            public List<ReservoirStat> ReservoirStatCollection { get; set; } = null;

            //Methods
            /// <summary>
            /// Builds report of key statistics collected from NeuralPreprocessor's reservoir(s)
            /// </summary>
            /// <param name="margin">Specifies how many spaces should be at the begining of each row.</param>
            /// <returns>Built text report</returns>
            public string CreateReport(int margin = 0)
            {
                string leftMargin = margin == 0 ? string.Empty : new string(' ', margin);
                StringBuilder sb = new StringBuilder();
                sb.Append(leftMargin + "Reservoir(s) info:" + Environment.NewLine);
                foreach (ReservoirStat resStat in ReservoirStatCollection)
                {
                    sb.Append(leftMargin + $"  Reservoir instance: {resStat.ReservoirInstanceName} ({resStat.ReservoirSettingsName})" + Environment.NewLine);
                    foreach (ReservoirStat.PoolStat poolStat in resStat.PoolStatCollection)
                    {
                        sb.Append(leftMargin + $"    Pool: {poolStat.PoolName}" + Environment.NewLine);
                        foreach (ReservoirStat.PoolStat.NeuronGroupStat groupStat in poolStat.NeuronGroupStatCollection)
                        {
                            sb.Append(leftMargin + $"      Group of neurons: {groupStat.GroupName}" + Environment.NewLine);
                            sb.Append(leftMargin + "        Stimulation (Input + Reservoir synapses)" + Environment.NewLine);
                            sb.Append(leftMargin + "          AVG Avg, Max, Min, SDdev: " + groupStat.AvgTStimuliStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.AvgTStimuliStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.AvgTStimuliStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.AvgTStimuliStat.StdDev.ToString("N4", CultureInfo.InvariantCulture) + Environment.NewLine);
                            sb.Append(leftMargin + "          MAX Avg, Max, Min, SDdev: " + groupStat.MaxTStimuliStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.MaxTStimuliStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.MaxTStimuliStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.MaxTStimuliStat.StdDev.ToString("N4", CultureInfo.InvariantCulture) + Environment.NewLine);
                            sb.Append(leftMargin + "          MIN Avg, Max, Min, SDdev: " + groupStat.MinTStimuliStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.MinTStimuliStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.MinTStimuliStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.MinTStimuliStat.StdDev.ToString("N4", CultureInfo.InvariantCulture) + Environment.NewLine);
                            sb.Append(leftMargin + "         SPAN Avg, Max, Min, SDdev: " + groupStat.TStimuliSpansStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.TStimuliSpansStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.TStimuliSpansStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.TStimuliSpansStat.StdDev.ToString("N4", CultureInfo.InvariantCulture) + Environment.NewLine);
                            /*
                            sb.Append(leftMargin + "        Stimulation (Reservoir synapses only)" + Environment.NewLine);
                            sb.Append(leftMargin + "          AVG Avg, Max, Min, SDdev: " + groupStat.AvgRStimuliStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.AvgRStimuliStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.AvgRStimuliStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.AvgRStimuliStat.StdDev.ToString("N4", CultureInfo.InvariantCulture) + Environment.NewLine);
                            sb.Append(leftMargin + "          MAX Avg, Max, Min, SDdev: " + groupStat.MaxRStimuliStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.MaxRStimuliStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.MaxRStimuliStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.MaxRStimuliStat.StdDev.ToString("N4", CultureInfo.InvariantCulture) + Environment.NewLine);
                            sb.Append(leftMargin + "          MIN Avg, Max, Min, SDdev: " + groupStat.MinRStimuliStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.MinRStimuliStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.MinRStimuliStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.MinRStimuliStat.StdDev.ToString("N4", CultureInfo.InvariantCulture) + Environment.NewLine);
                            sb.Append(leftMargin + "         SPAN Avg, Max, Min, SDdev: " + groupStat.RStimuliSpansStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.RStimuliSpansStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.RStimuliSpansStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.RStimuliSpansStat.StdDev.ToString("N4", CultureInfo.InvariantCulture) + Environment.NewLine);
                            */
                            sb.Append(leftMargin + "        Reservoir synapses efficacy" + Environment.NewLine);
                            sb.Append(leftMargin + "          AVG Avg, Max, Min, SDdev: " + groupStat.AvgSynEfficacyStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.AvgSynEfficacyStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.AvgSynEfficacyStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.AvgSynEfficacyStat.StdDev.ToString("N4", CultureInfo.InvariantCulture) + Environment.NewLine);
                            sb.Append(leftMargin + "          MAX Avg, Max, Min, SDdev: " + groupStat.MaxSynEfficacyStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.MaxSynEfficacyStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.MaxSynEfficacyStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.MaxSynEfficacyStat.StdDev.ToString("N4", CultureInfo.InvariantCulture) + Environment.NewLine);
                            sb.Append(leftMargin + "          MIN Avg, Max, Min, SDdev: " + groupStat.MinSynEfficacyStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.MinSynEfficacyStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.MinSynEfficacyStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.MinSynEfficacyStat.StdDev.ToString("N4", CultureInfo.InvariantCulture) + Environment.NewLine);
                            sb.Append(leftMargin + "         SPAN Avg, Max, Min, SDdev: " + groupStat.SynEfficacySpansStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.SynEfficacySpansStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.SynEfficacySpansStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.SynEfficacySpansStat.StdDev.ToString("N4", CultureInfo.InvariantCulture) + Environment.NewLine);
                            sb.Append(leftMargin + "        Activation" + Environment.NewLine);
                            sb.Append(leftMargin + "          AVG Avg, Max, Min, SDdev: " + groupStat.AvgActivationStatesStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.AvgActivationStatesStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.AvgActivationStatesStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.AvgActivationStatesStat.StdDev.ToString("N4", CultureInfo.InvariantCulture) + Environment.NewLine);
                            sb.Append(leftMargin + "          MAX Avg, Max, Min, SDdev: " + groupStat.MaxActivationStatesStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.MaxActivationStatesStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.MaxActivationStatesStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.MaxActivationStatesStat.StdDev.ToString("N4", CultureInfo.InvariantCulture) + Environment.NewLine);
                            sb.Append(leftMargin + "         SPAN Avg, Max, Min, SDdev: " + groupStat.ActivationStateSpansStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.ActivationStateSpansStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.ActivationStateSpansStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.ActivationStateSpansStat.StdDev.ToString("N4", CultureInfo.InvariantCulture) + Environment.NewLine);
                            sb.Append(leftMargin + "        Signal" + Environment.NewLine);
                            sb.Append(leftMargin + "          AVG Avg, Max, Min, SDdev: " + groupStat.AvgOutputSignalStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.AvgOutputSignalStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.AvgOutputSignalStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.AvgOutputSignalStat.StdDev.ToString("N4", CultureInfo.InvariantCulture) + Environment.NewLine);
                            sb.Append(leftMargin + "          MAX Avg, Max, Min, SDdev: " + groupStat.MaxOutputSignalStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.MaxOutputSignalStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.MaxOutputSignalStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.MaxOutputSignalStat.StdDev.ToString("N4", CultureInfo.InvariantCulture) + Environment.NewLine);
                            sb.Append(leftMargin + "          MIN Avg, Max, Min, SDdev: " + groupStat.MinOutputSignalStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.MinOutputSignalStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.MinOutputSignalStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                          + groupStat.MinOutputSignalStat.StdDev.ToString("N4", CultureInfo.InvariantCulture) + Environment.NewLine);
                        }
                        /*
                        sb.Append(leftMargin + "      Weights" + Environment.NewLine);
                        sb.Append(leftMargin + "        Input Avg, Max, Min, SDdev: " + poolStat.InputWeightsStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                      + poolStat.InputWeightsStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                      + poolStat.InputWeightsStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                      + poolStat.InputWeightsStat.StdDev.ToString("N4", CultureInfo.InvariantCulture) + Environment.NewLine);
                        sb.Append(leftMargin + "     Internal Avg, Max, Min, SDdev: " + poolStat.InternalWeightsStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                      + poolStat.InternalWeightsStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                      + poolStat.InternalWeightsStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                                      + poolStat.InternalWeightsStat.StdDev.ToString("N4", CultureInfo.InvariantCulture) + Environment.NewLine);
                        */
                    }
                }
                return sb.ToString();
            }

        }//RegressionInput

    }//StateMachine
}//Namespace
