using System;
using System.Collections.Generic;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Data;
using RCNet.Neural.Data.Generators;
using RCNet.RandomValue;
using RCNet.Neural.Network.SM.ReservoirStructure;
using RCNet.Neural.Network.SM.Readout;


namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Implements the State Machine Network.
    /// </summary>
    [Serializable]
    public class StateMachine
    {
        //Delegates
        /// <summary>
        /// Informative callback function to inform about predictors collection progress.
        /// </summary>
        /// <param name="totalNumOfInputs">Total number of inputs to be processed</param>
        /// <param name="numOfProcessedInputs">Number of processed inputs</param>
        /// <param name="userObject">An user object</param>
        public delegate void PredictorsCollectionCallbackDelegate(int totalNumOfInputs,
                                                                  int numOfProcessedInputs,
                                                                  Object userObject
                                                                  );
        //Attributes
        /// <summary>
        /// Settings used for instance creation.
        /// </summary>
        private StateMachineSettings _settings;
        /// <summary>
        /// Data range. Data range has to be always between -1 and 1
        /// </summary>
        private readonly Interval _dataRange;
        /// <summary>
        /// Collection of the internal input generators associated with the internal input fields
        /// </summary>
        private readonly List<IGenerator> _internalInputGeneratorCollection;
        /// <summary>
        /// Collection of reservoir instances.
        /// </summary>
        private List<Reservoir> _reservoirCollection;
        /// <summary>
        /// Number of State Machine predictors
        /// </summary>
        private readonly int _numOfPredictors;
        /// <summary>
        /// Readout layer.
        /// </summary>
        private ReadoutLayer _readoutLayer;

        //Constructor
        /// <summary>
        /// Constructs an instance of State Machine
        /// </summary>
        /// <param name="settings">State Machine settings</param>
        public StateMachine(StateMachineSettings settings)
        {
            _settings = settings.DeepClone();
            //Data range has to be always <-1,1>
            _dataRange = CommonEnums.GetDataNormalizationRange(CommonEnums.DataNormalizationRange.Inclusive_Neg1_Pos1);
            //Internal input generators
            _internalInputGeneratorCollection = new List<IGenerator>();
            foreach(StateMachineSettings.InputSettings.InternalField field in _settings.InputConfig.InternalFieldCollection)
            {
                if(field.GeneratorSettings.GetType() == typeof(ConstGeneratorSettings))
                {
                    _internalInputGeneratorCollection.Add(new ConstGenerator((ConstGeneratorSettings)field.GeneratorSettings));
                }
                else if (field.GeneratorSettings.GetType() == typeof(RandomValueSettings))
                {
                    _internalInputGeneratorCollection.Add(new RandomGenerator((RandomValueSettings)field.GeneratorSettings));
                }
                else if (field.GeneratorSettings.GetType() == typeof(SinusoidalGeneratorSettings))
                {
                    _internalInputGeneratorCollection.Add(new SinusoidalGenerator((SinusoidalGeneratorSettings)field.GeneratorSettings));
                }
                else if (field.GeneratorSettings.GetType() == typeof(MackeyGlassGeneratorSettings))
                {
                    _internalInputGeneratorCollection.Add(new MackeyGlassGenerator((MackeyGlassGeneratorSettings)field.GeneratorSettings));
                }
                else
                {
                    throw new Exception($"Unsupported internal signal generator for field {field.Name}");
                }
            }
            //Reservoir instance(s)
            //Random generator used for reservoir structure initialization
            Random rand = (_settings.RandomizerSeek < 0 ? new Random() : new Random(_settings.RandomizerSeek));
            _numOfPredictors = 0;
            _reservoirCollection = new List<Reservoir>(_settings.ReservoirInstanceDefinitionCollection.Count);
            foreach(StateMachineSettings.ReservoirInstanceDefinition instanceDefinition in _settings.ReservoirInstanceDefinitionCollection)
            {
                Reservoir reservoir = new Reservoir(instanceDefinition, _dataRange, rand);
                _reservoirCollection.Add(reservoir);
                _numOfPredictors += reservoir.NumOfOutputPredictors;
            }
            if(_settings.InputConfig.RouteExternalInputToReadout)
            {
                _numOfPredictors += _settings.InputConfig.ExternalFieldCollection.Count;
            }
            //Readout layer
            _readoutLayer = null;
            return;
        }

        //Properties
        /// <summary>
        /// Collection of the error statistics.
        /// Each cluster of readout units related to output field  has one summary error statistics in the collection.
        /// Order is the same as the order of output fields.
        /// </summary>
        public List<ReadoutLayer.ClusterErrStatistics> ClusterErrStatisticsCollection { get { return _readoutLayer.ClusterErrStatisticsCollection; } }

        //Methods
        /// <summary>
        /// Sets State Machine internal state to initial state
        /// </summary>
        /// <param name="resetStatistics">Specifies whether to reset internal statistics</param>
        private void Reset(bool resetStatistics)
        {
            foreach(IGenerator generator in _internalInputGeneratorCollection)
            {
                generator.Reset();
            }
            foreach(Reservoir reservoir in _reservoirCollection)
            {
                reservoir.Reset(resetStatistics);
            }
            return;
        }

        /// <summary>
        /// Adds inputs from internal generators to be used in reservoirs.
        /// </summary>
        /// <param name="externalInputVector">External input values</param>
        /// <returns></returns>
        private double[] AddInternalInputVector(double[] externalInputVector)
        {
            double[] smInput = new double[_settings.InputConfig.NumOfFields];
            externalInputVector.CopyTo(smInput, 0);
            for(int i = 0; i < _internalInputGeneratorCollection.Count; i++)
            {
                smInput[_settings.InputConfig.ExternalFieldCollection.Count + i] = _internalInputGeneratorCollection[i].Next();
            }
            return smInput;
        }

        /// <summary>
        /// Pushes input values into the reservoirs and returns the predictors
        /// </summary>
        /// <param name="externalInputVector">Input values</param>
        /// <param name="collectStatesStatistics">
        /// The parameter indicates whether to update internal statistics
        /// </param>
        private double[] PushInput(double[] externalInputVector, bool collectStatesStatistics)
        {
            double[] completedInputVector = AddInternalInputVector(externalInputVector);
            double[] predictors = new double[_numOfPredictors];
            int predictorsIdx = 0;
            //Compute reservoir(s)
            foreach (Reservoir reservoir in _reservoirCollection)
            {
                double[] reservoirInput = new double[reservoir.InstanceDefinition.SMInputFieldIdxCollection.Count];
                for(int i = 0; i < reservoir.InstanceDefinition.SMInputFieldIdxCollection.Count; i++)
                {
                    reservoirInput[i] = completedInputVector[reservoir.InstanceDefinition.SMInputFieldIdxCollection[i]];
                }
                //Compute reservoir
                reservoir.Compute(reservoirInput, collectStatesStatistics);
                reservoir.CopyPredictorsTo(predictors, predictorsIdx);
                predictorsIdx += reservoir.NumOfOutputPredictors;
            }
            if(_settings.InputConfig.RouteExternalInputToReadout)
            {
                completedInputVector.CopyTo(predictors, predictorsIdx);
            }
            return predictors;
        }

        /// <summary>
        /// Pushes input pattern into the reservoirs and returns the predictors
        /// </summary>
        /// <param name="externalInputPattern">Input pattern</param>
        private double[] PushInput(List<double[]> externalInputPattern)
        {
            double[] predictors = new double[_numOfPredictors];
            int predictorsIdx = 0;
            //Reset SM but keep statistics
            Reset(false);
            //Add internal input
            List<double[]> completedInputPattern = new List<double[]>(externalInputPattern.Count);
            foreach(double[] externalInputVector in externalInputPattern)
            {
                completedInputPattern.Add(AddInternalInputVector(externalInputVector));
            }
            //Compute reservoir(s)
            foreach (Reservoir reservoir in _reservoirCollection)
            {
                double[] reservoirInput = new double[reservoir.InstanceDefinition.SMInputFieldIdxCollection.Count];
                foreach (double[] inputVector in completedInputPattern)
                {
                    for (int i = 0; i < reservoir.InstanceDefinition.SMInputFieldIdxCollection.Count; i++)
                    {
                        reservoirInput[i] = inputVector[reservoir.InstanceDefinition.SMInputFieldIdxCollection[i]];
                    }
                    //Compute the reservoir
                    reservoir.Compute(reservoirInput, true);
                }
                reservoir.CopyPredictorsTo(predictors, predictorsIdx);
                predictorsIdx += reservoir.NumOfOutputPredictors;
            }
            return predictors;
        }

        /// <summary>
        /// Compute function for classification or hybrid tasks.
        /// Processes given input pattern and computes the output.
        /// </summary>
        /// <param name="inputPattern">Input pattern</param>
        /// <returns>Computed output values</returns>
        public double[] Compute(List<double[]> inputPattern)
        {
            if(_settings.TaskType == CommonEnums.TaskType.Prediction)
            {
                throw new Exception("This version of Compute function is useable only for the classification or hybrid task type.");
            }
            double[] predictors = PushInput(inputPattern);
            //Compute output
            return _readoutLayer.Compute(predictors);
        }

        /// <summary>
        /// Compute fuction for time series prediction tasks.
        /// Processes given input values and computes (predicts) the output.
        /// </summary>
        /// <param name="inputVector">Input values</param>
        /// <returns>Computed output values</returns>
        public double[] Compute(double[] inputVector)
        {
            if (_settings.TaskType != CommonEnums.TaskType.Prediction)
            {
                throw new Exception("This version of Compute function is useable only for the prediction task type.");
            }
            //Push input into the network
            double[] predictors = PushInput(inputVector, true);
            //Compute output
            return _readoutLayer.Compute(predictors);
        }

        /// <summary>
        /// Collects the key statistics of each reservoir instance.
        /// It is very important to follow these statistics and adjust the weights in the reservoirs so that the neurons
        /// in the reservoir are not oversaturated or inactive.
        /// </summary>
        /// <returns>Collection of key statistics for each reservoir instance</returns>
        private List<ReservoirStat> CollectReservoirInstancesStatatistics()
        {
            List<ReservoirStat> stats = new List<ReservoirStat>();
            foreach(Reservoir reservoir in _reservoirCollection)
            {
                stats.Add(reservoir.CollectStatistics());
            }
            return stats;
        }

        /// <summary>
        /// Prepares input for regression stage of State Machine training for the classification or hybrid task type.
        /// All input patterns are processed by internal reservoirs and the corresponding network predictors are recorded.
        /// </summary>
        /// <param name="dataSet">
        /// The bundle containing known sample input patterns and desired output vectors
        /// </param>
        /// <param name="informativeCallback">
        /// Function to be called after each processed input.
        /// </param>
        /// <param name="userObject">
        /// The user object to be passed to informativeCallback.
        /// </param>
        public RegressionStageInput PrepareRegressionStageInput(PatternBundle dataSet,
                                                                PredictorsCollectionCallbackDelegate informativeCallback = null,
                                                                Object userObject = null
                                                                )
        {
            if (_settings.TaskType == CommonEnums.TaskType.Prediction)
            {
                throw new Exception("This version of PrepareRegressionStageInput function is useable only for the classification or hybrid task type.");
            }
            //RegressionStageInput allocation
            RegressionStageInput rsi = new RegressionStageInput
            {
                PredictorsCollection = new List<double[]>(dataSet.InputPatternCollection.Count),
                IdealOutputsCollection = new List<double[]>(dataSet.OutputVectorCollection.Count)
            };
            //Reset the internal states and statistics
            Reset(true);
            //Collection
            for (int dataSetIdx = 0; dataSetIdx < dataSet.InputPatternCollection.Count; dataSetIdx++)
            {
                //Push input data into the network
                double[] predictors = PushInput(dataSet.InputPatternCollection[dataSetIdx]);
                rsi.PredictorsCollection.Add(predictors);
                //Add desired outputs
                rsi.IdealOutputsCollection.Add(dataSet.OutputVectorCollection[dataSetIdx]);
                //Informative callback
                informativeCallback?.Invoke(dataSet.InputPatternCollection.Count, dataSetIdx + 1, userObject);
            }
            //Collect reservoirs statistics
            rsi.ReservoirStatCollection = CollectReservoirInstancesStatatistics();
            return rsi;
        }

        /// <summary>
        /// Prepares input for regression stage of State Machine training for the time series prediction task.
        /// All input vectors are processed by internal reservoirs and the corresponding network predictors are recorded.
        /// </summary>
        /// <param name="dataSet">
        /// The bundle containing known sample input and desired output vectors (in time order)
        /// </param>
        /// <param name="numOfBootSamples">
        /// Number of boot samples from the beginning of all samples.
        /// The purpose of the boot samples is to ensure that the states of the neurons in the reservoir
        /// depend only on the time series data and not on the initial state of the neurons in the reservoir.
        /// The number of boot samples depends on the size and configuration of the reservoirs.
        /// It is usually sufficient to set the number of boot samples equal to the number of neurons in the largest reservoir.
        /// </param>
        /// <param name="informativeCallback">
        /// Function to be called after each processed input.
        /// </param>
        /// <param name="userObject">
        /// The user object to be passed to informativeCallback.
        /// </param>
        public RegressionStageInput PrepareRegressionStageInput(TimeSeriesBundle dataSet,
                                                                int numOfBootSamples,
                                                                PredictorsCollectionCallbackDelegate informativeCallback = null,
                                                                Object userObject = null
                                                                )
        {
            if (_settings.TaskType != CommonEnums.TaskType.Prediction)
            {
                throw new Exception("This version of PrepareRegressionStageInput function is useable only for the prediction task type.");
            }
            int dataSetLength = dataSet.InputVectorCollection.Count;
            //RegressionStageInput allocation
            RegressionStageInput rsi = new RegressionStageInput
            {
                PredictorsCollection = new List<double[]>(dataSetLength - numOfBootSamples),
                IdealOutputsCollection = new List<double[]>(dataSetLength - numOfBootSamples)
            };
            //Reset the internal states and statistics
            Reset(true);
            //Collection
            for (int dataSetIdx = 0; dataSetIdx < dataSetLength; dataSetIdx++)
            {
                bool afterBoot = (dataSetIdx >= numOfBootSamples);
                //Push input data into the network
                double[] predictors = PushInput(dataSet.InputVectorCollection[dataSetIdx], afterBoot);
                //Is boot sequence passed? Collect predictors?
                if (afterBoot)
                {
                    //YES
                    rsi.PredictorsCollection.Add(predictors);
                    //Desired outputs
                    rsi.IdealOutputsCollection.Add(dataSet.OutputVectorCollection[dataSetIdx]);
                }
                //An informative callback
                informativeCallback?.Invoke(dataSetLength, dataSetIdx + 1, userObject);
            }

            //Collect reservoirs statistics
            rsi.ReservoirStatCollection = CollectReservoirInstancesStatatistics();
            return rsi;
        }

        /// <summary>
        /// Trains the State Machine readout layer.
        /// </summary>
        /// <param name="rsi">
        /// RegressionStageInput object prepared by PrepareRegressionStageInput function
        /// </param>
        /// <param name="regressionController">
        /// Optional. see Regression.RegressionCallbackDelegate
        /// </param>
        /// <param name="regressionControllerData">
        /// Optional custom object to be passed to regressionController together with other standard information
        /// </param>
        public ValidationBundle RegressionStage(RegressionStageInput rsi,
                                                ReadoutUnit.RegressionCallbackDelegate regressionController = null,
                                                Object regressionControllerData = null
                                                )
        {
            //Readout layer instance
            _readoutLayer = new ReadoutLayer(_settings.TaskType, _settings.ReadoutLayerConfig, _dataRange);
            //Training
            return _readoutLayer.Build(rsi.PredictorsCollection,
                                       rsi.IdealOutputsCollection,
                                       regressionController,
                                       regressionControllerData
                                       );
        }


        //Inner classes
        /// <summary>
        /// Contains prepared data for regression stage and statistics of the reservoir(s)
        /// </summary>
        [Serializable]
        public class RegressionStageInput
        {
            /// <summary>
            /// Collection of State Machine predictors
            /// </summary>
            public List<double[]> PredictorsCollection { get; set; } = null;
            /// <summary>
            /// Collection of the ideal outputs
            /// </summary>
            public List<double[]> IdealOutputsCollection { get; set; } = null;
            /// <summary>
            /// Collection of statistics of the State Machine's reservoir(s)
            /// </summary>
            public List<ReservoirStat> ReservoirStatCollection { get; set; } = null;

        }//RegressionStageInput

    }//StateMachine
}//Namespace
