using System;
using System.Collections.Generic;
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
            //Readout layer deletion
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
                PreprocessedData = NP.PreprocessBundle(patternBundle, informativeCallback, userObject),
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
                PreprocessedData = NP.PreprocessBundle(vectorBundle, informativeCallback, userObject),
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
            RL = new ReadoutLayer(_settings.ReadoutLayerConfig, NeuralPreprocessor.DataRange);
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
            return RL.Build(regressionInput.PreprocessedData.InputVectorCollection,
                            regressionInput.PreprocessedData.OutputVectorCollection,
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
            double[] predictors = NP.PushInput(inputPattern);
            //Compute output
            return RL.Compute(predictors);
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
            //Push input into the network
            double[] predictors = NP.PushInput(inputVector, false);
            //Compute output
            return RL.Compute(predictors);
        }

        //Inner classes
        /// <summary>
        /// Contains prepared data for regression stage and statistics of the reservoir(s)
        /// </summary>
        [Serializable]
        public class RegressionInput
        {
            /// <summary>
            /// Collection of the predictors and ideal outputs
            /// </summary>
            public VectorBundle PreprocessedData { get; set; } = null;
            /// <summary>
            /// Collection of statistics of the State Machine's reservoir(s)
            /// </summary>
            public List<ReservoirStat> ReservoirStatCollection { get; set; } = null;

        }//RegressionInput

    }//StateMachine
}//Namespace
