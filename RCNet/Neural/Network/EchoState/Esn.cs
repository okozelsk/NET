using System;
using System.Collections.Generic;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Activation;
using RCNet.Neural.Network.Data;
using RCNet.Neural.Network.FF;

namespace RCNet.Neural.Network.EchoState
{
    /// <summary>
    /// Implements the Echo State Network.
    /// </summary>
    [Serializable]
    public class Esn
    {
        //Attributes
        /// <summary>
        /// Esn settings used for instance creation.
        /// </summary>
        private EsnSettings _settings;
        /// <summary>
        /// Random generator
        /// </summary>
        private Random _rand;
        /// <summary>
        /// Collection of reservoir instances within this Esn.
        /// </summary>
        private List<ReservoirInstance> _reservoirInstanceCollection;
        /// <summary>
        /// All Esn reservoirs' predictors are copied here after input processing step.
        /// </summary>
        private double[] _reservoirsReadoutLayer;
        /// <summary>
        /// Trained feed forward network and regression statistics for each Esn output field.
        /// </summary>
        private EsnRegressionResult[] _regressionResultCollection;

        //Delegates
        /// <summary>
        /// Selects testing samples from all available samples.
        /// </summary>
        /// <param name="predictorsCollection">Esn predictors collection</param>
        /// <param name="outputsCollection">Desired outputs collection (desired outputs in the same order as predictors)</param>
        /// <param name="regrOutputFieldIdx">Index of the Esn output field for which the regression will be performed.</param>
        /// <param name="testSampleIdxCollection">Array of test samples indexes to be filled.</param>
        public delegate void TestSamplesSelectorDelegate(List<double[]> predictorsCollection,
                                                         List<double[]> outputsCollection,
                                                         int regrOutputFieldIdx, 
                                                         int[] testSampleIdxCollection
                                                         );
        /// <summary>
        /// This is the control function of the regression process and is called
        /// after the completion of each regression training epoch.
        /// The goal of the regression process is for each Esn output field to train a feed forward network
        /// that will give good results both on the training data and the test data.
        /// Esn.EsnRegressionControlInArgs object passed to the function contains the best error statistics so far
        /// and the latest statistics. The primary purpose of the function is to decide whether the latest statistics
        /// are better than the best statistics so far.
        /// The function can also tell the regression process that it does not make any sense to continue the regression.
        /// It can terminate the current regression attempt or whole output field regression process.
        /// The reservoir statistics are also available in the Esn.EsnRegressionControlInArgs object, which should be
        /// monitored to ensure that the neurons of the reservoirs have not been oversaturated.
        /// </summary>
        /// <param name="inArgs">Contains all the necessary information to control the progress of the regression.</param>
        /// <returns>Instructions for the regression process.</returns>
        public delegate EsnRegressionControlOutArgs EsnRegressionCallbackDelegate(EsnRegressionControlInArgs inArgs);

        //Constructor
        /// <summary>
        /// Constructs an instance of Echo State Network
        /// </summary>
        /// <param name="settings">Echo State Network settings</param>
        public Esn(EsnSettings settings)
        {
            _settings = settings.DeepClone();
            //Random object
            if (_settings.RandomizerSeek < 0) _rand = new Random();
            else _rand = new Random(_settings.RandomizerSeek);
            //Build structure
            //Reservoir instance(s)
            int totalPredictors = 0;
            _reservoirInstanceCollection = new List<ReservoirInstance>(_settings.ReservoirInstanceDefinitionCollection.Count);
            foreach(EsnSettings.ReservoirInstanceDefinition instanceDefinition in _settings.ReservoirInstanceDefinitionCollection)
            {
                ReservoirInstance reservoirInstance = new ReservoirInstance(instanceDefinition, _settings.RandomizerSeek);
                _reservoirInstanceCollection.Add(reservoirInstance);
                totalPredictors += reservoirInstance.ReservoirObj.NumOfOutputPredictors;
            }
            //Reservoir(s) predictors
            _reservoirsReadoutLayer = new double[totalPredictors];
            _reservoirsReadoutLayer.Populate(0);
            //Regression results
            _regressionResultCollection = new EsnRegressionResult[_settings.OutputFieldNameCollection.Count];
            _regressionResultCollection.Populate(null);
            return;
        }

        //Properties

        //Methods
        /// <summary>
        /// Sets Esn internal state to initial state
        /// </summary>
        private void Reset()
        {
            foreach(ReservoirInstance reservoirInstanceData in _reservoirInstanceCollection)
            {
                reservoirInstanceData.Reset();
            }
            _reservoirsReadoutLayer.Populate(0);
            return;
        }

        /// <summary>
        /// Pushes input values into the reservoirs and stores reservoirs predictors
        /// </summary>
        /// <param name="inputValues">Esn input values</param>
        /// <param name="collectStatesStatistics">
        /// The parameter indicates whether the internal states may be included into the statistics
        /// </param>
        private void PushInput(double[] inputValues, bool collectStatesStatistics)
        {
            int reservoirsReadoutLayerIdx = 0;
            //Compute reservoir(s)
            foreach (ReservoirInstance resInstance in _reservoirInstanceCollection)
            {
                double[] reservoirInput = new double[resInstance.InstanceDefinition.InputFieldMappingCollection.Count];
                for(int i = 0; i < resInstance.InstanceDefinition.InputFieldMappingCollection.Count; i++)
                {
                    reservoirInput[i] = inputValues[resInstance.InstanceDefinition.InputFieldMappingCollection[i]];
                }
                double[] reservoirPredictors = new double[resInstance.ReservoirObj.NumOfOutputPredictors];
                //Compute reservoir output
                resInstance.ReservoirObj.Compute(reservoirInput, reservoirPredictors, collectStatesStatistics);
                //Add predictors to common readout layer
                reservoirPredictors.CopyTo(_reservoirsReadoutLayer, reservoirsReadoutLayerIdx);
                reservoirsReadoutLayerIdx += reservoirPredictors.Length;
            }
            return;
        }

        /// <summary>
        /// Pushes input pattern into the reservoirs and stores reservoirs predictors
        /// </summary>
        /// <param name="inputPattern">Input pattern</param>
        /// <param name="collectStatesStatistics">
        /// The parameter indicates whether the internal states may be included into the statistics
        /// </param>
        private void PushInput(List<double[]> inputPattern)
        {
            int reservoirsReadoutLayerIdx = 0;
            //Compute reservoir(s)
            foreach (ReservoirInstance resInstance in _reservoirInstanceCollection)
            {
                double[] reservoirPredictors = new double[resInstance.ReservoirObj.NumOfOutputPredictors];
                foreach (double[] inputVector in inputPattern)
                {
                    double[] reservoirInput = new double[resInstance.InstanceDefinition.InputFieldMappingCollection.Count];
                    for (int i = 0; i < resInstance.InstanceDefinition.InputFieldMappingCollection.Count; i++)
                    {
                        reservoirInput[i] = inputVector[resInstance.InstanceDefinition.InputFieldMappingCollection[i]];
                    }
                    //Compute reservoir output
                    resInstance.ReservoirObj.Compute(reservoirInput, reservoirPredictors, true);
                }
                //Add predictors to common readout layer
                reservoirPredictors.CopyTo(_reservoirsReadoutLayer, reservoirsReadoutLayerIdx);
                reservoirsReadoutLayerIdx += reservoirPredictors.Length;
            }
            return;
        }

        /// <summary>
        /// Compute fuction for pattern recognition tasks.
        /// Processes given input pattern and computes output.
        /// </summary>
        /// <param name="inputPattern">Input values</param>
        /// <returns>Computed output values</returns>
        public double[] Compute(List<double[]> inputPattern)
        {
            //There is no continuity, each pattern is independed so internal states has to be reseted
            Reset();
            PushInput(inputPattern);
            //Collect all predictors
            double[] predictors = new double[_reservoirsReadoutLayer.Length];
            _reservoirsReadoutLayer.CopyTo(predictors, 0);
            //Compute output
            double[] output = new double[_regressionResultCollection.Length];
            for (int i = 0; i < _regressionResultCollection.Length; i++)
            {
                double[] outputValue;
                outputValue = _regressionResultCollection[i].FFNet.Compute(predictors);
                output[i] = outputValue[0];
            }
            return output;
        }

        /// <summary>
        /// Compute fuction for time series prediction tasks.
        /// Processes given input values and computes (predicts) output values.
        /// </summary>
        /// <param name="inputVector">Input values</param>
        /// <returns>Computed output values</returns>
        public double[] Compute(double[] inputVector)
        {
            //Push input into the Esn
            PushInput(inputVector, true);
            //Collect all predictors
            int totalNumOfPredictors = _reservoirsReadoutLayer.Length;
            if (_settings.RouteInputToReadout)
            {
                totalNumOfPredictors += inputVector.Length;
            }
            double[] predictors = new double[totalNumOfPredictors];
            _reservoirsReadoutLayer.CopyTo(predictors, 0);
            if (_settings.RouteInputToReadout)
            {
                inputVector.CopyTo(predictors, _reservoirsReadoutLayer.Length);
            }
            //Compute output
            double[] output = new double[_regressionResultCollection.Length];
            for (int i = 0; i < _regressionResultCollection.Length; i++)
            {
                double[] outputValue;
                outputValue = _regressionResultCollection[i].FFNet.Compute(predictors);
                output[i] = outputValue[0];
            }
            return output;
        }

        /// <summary>
        /// If feedback is defined in one or more of the reservoirs, the PushFeedback function must be called
        /// before calling the "Compute" function. The previous real values should be passed to the function, which is
        /// then processed in a similar way as the input values.
        /// The exception is the first call to Compute after network training. Before this first call, PushFeedback
        /// does not have to be called because it has already been called in the training.
        /// </summary>
        /// <param name="lastRealValues">
        /// Previous real values in the same order as the Esn output fields are defined.
        /// If you want to do more forward predictions, the previous real values are of course not available, and in
        /// this case use the previous values calculated by the network. However, it is very likely that the network
        /// error will grow steeply over time and the prediction ability will decrease.
        /// </param>
        public void PushFeedback(double[] lastRealValues)
        {
            foreach (ReservoirInstance resInstance in _reservoirInstanceCollection)
            {
                if (resInstance.InstanceDefinition.ReservoirSettings.FeedbackFeature)
                {
                    double[] feedbackValues = new double[resInstance.InstanceDefinition.FeedbackFieldMappingCollection.Count];
                    for (int i = 0; i < resInstance.InstanceDefinition.FeedbackFieldMappingCollection.Count; i++)
                    {
                        feedbackValues[i] = lastRealValues[resInstance.InstanceDefinition.FeedbackFieldMappingCollection[i]];
                    }
                    resInstance.ReservoirObj.SetFeedback(feedbackValues);
                }
            }
            return;
        }

        /// <summary>
        /// Selects the testing samples as the lasting sequence from given samples.
        /// see EsnTestTimeSeriesSamplesSelectorCallbackDelegate.
        /// </summary>
        public void SelectSequentialTestSamples(List<double[]> predictorsCollection,
                                                List<double[]> outputsCollection,
                                                int regrOutputFieldIdx,
                                                int[] testSampleIdxCollection
                                                )
        {
            //Sequential selection
            for (int srcIdx = predictorsCollection.Count - testSampleIdxCollection.Length, i = 0; srcIdx < predictorsCollection.Count; srcIdx++, i++)
            {
                testSampleIdxCollection[i] = srcIdx;
            }
            return;
        }

        /// <summary>
        /// Selects testing samples randomly from given samples.
        /// see EsnTestTimeSeriesSamplesSelectorCallbackDelegate.
        /// </summary>
        public void SelectRandomTestSamples(List<double[]> predictorsCollection,
                                            List<double[]> outputsCollection,
                                            int regrOutputFieldIdx,
                                            int[] testSampleIdxCollection
                                            )
        {
            //Random selection
            int[] randIndexes = new int[predictorsCollection.Count];
            randIndexes.ShuffledIndices(_rand);
            for (int i = 0; i < testSampleIdxCollection.Length; i++)
            {
                testSampleIdxCollection[i] = randIndexes[i];
            }
            return;
        }

        /// <summary>
        /// Collects the key statistics of each reservoir instance.
        /// It is very important to follow these statistics and adjust the weights in the reservoirs so that the neurons
        /// in the reservoir are not oversaturated.
        /// </summary>
        /// <returns>Collection of key statistics of each reservoir instance</returns>
        private List<AnalogReservoirStat> CollectReservoirInstancesStatatistics()
        {
            List<AnalogReservoirStat> stats = new List<AnalogReservoirStat>();
            foreach(ReservoirInstance resInstance in _reservoirInstanceCollection)
            {
                stats.Add(resInstance.ReservoirObj.CollectStatistics());
            }
            return stats;
        }


        /// <summary>
        /// Trains the Esn network.
        /// All input vectors are processed by internal reservoirs and the corresponding Esn predictors are recorded.
        /// Predictors are then subdivided into training data and test data.
        /// Training data is used to teach the output feed forward networks (regression phase).
        /// The degree of generalization is tested on test data.
        /// The goal is to select a network where there is not a big difference between
        /// the overall error on the training data and the test data.
        /// </summary>
        /// <param name="dataSet">
        /// The bundle containing known input and desired output sample vectors (in time order)
        /// </param>
        /// <param name="numOfBootSamples">
        /// How many of starting items from dataSet will be used for booting of reservoirs to ensure
        /// reservoir neurons states be affected by input data only. Boot sequence length of the reservoir should be greater
        /// or equal to reservoir neurons count. So for this Esn parameter use the boot sequence length of the largest
        /// reservoir in the Esn.
        /// </param>
        /// <param name="numOfTestSamples">
        /// How many samples from the dataSet will be used as the Esn test samples.
        /// </param>
        /// <param name="testSamplesSelector">
        /// Function to be called to select testing samples
        /// (use SelectSequentialTestSamples, SelectRandomTestSamples or implement your own method)
        /// </param>
        /// <param name="regressionController">
        /// Optional. see EsnRegressionCallbackDelegate
        /// </param>
        /// <param name="regressionControllerData">
        /// Optional custom object to be passed to regressionController together with other standard information
        /// </param>
        /// <returns>
        /// Array of regression outputs
        /// </returns>
        public EsnRegressionResult[] Train(PatternVectorPairBundle dataSet,
                                           int numOfTestSamples,
                                           TestSamplesSelectorDelegate testSamplesSelector,
                                           EsnRegressionCallbackDelegate regressionController = null,
                                           Object regressionControllerData = null
                                           )
        {
            EsnRegressionResult[] results = new EsnRegressionResult[_regressionResultCollection.Length];
            int numOfPredictors = _reservoirsReadoutLayer.Length;
            //All predictors to be used for training/testing
            int dataSetLength = dataSet.InputPatternCollection.Count;
            List<double[]> allPredictorsCollection = new List<double[]>(dataSetLength);
            List<double[]> allOutputsCollection = new List<double[]>(dataSetLength);
            //Collection
            for (int dataSetIdx = 0; dataSetIdx < dataSetLength; dataSetIdx++)
            {
                //Push input data into the Esn
                PushInput(dataSet.InputPatternCollection[dataSetIdx]);
                //Predictors
                double[] predictors = new double[numOfPredictors];
                //Reservoir units states to predictors
                _reservoirsReadoutLayer.CopyTo(predictors, 0);
                allPredictorsCollection.Add(predictors);
                //Desired outputs
                allOutputsCollection.Add(dataSet.OutputVectorCollection[dataSetIdx]);
            }

            //Collect reservoirs statistics
            List<AnalogReservoirStat> reservoirsStats = CollectReservoirInstancesStatatistics();

            //Alone regression for each Esn output field
            for (int outputIdx = 0; outputIdx < _regressionResultCollection.Length; outputIdx++)
            {
                //Testing data indexes
                int[] testIndexes = new int[numOfTestSamples];
                testSamplesSelector(allPredictorsCollection, allOutputsCollection, outputIdx, testIndexes);
                HashSet<int> testSampleIndexCollection = new HashSet<int>(testIndexes);
                //Dividing to training and testing sets
                int numOfTrainingSamples = allPredictorsCollection.Count - numOfTestSamples;
                List<double[]> trainingPredictorsCollection = new List<double[]>(numOfTrainingSamples);
                List<double[]> trainingOutputsCollection = new List<double[]>(numOfTrainingSamples);
                List<double[]> testingPredictorsCollection = new List<double[]>(numOfTestSamples);
                List<double[]> testingOutputsCollection = new List<double[]>(numOfTestSamples);
                for (int i = 0; i < allPredictorsCollection.Count; i++)
                {
                    if (!testSampleIndexCollection.Contains(i))
                    {
                        //Training sample
                        trainingPredictorsCollection.Add(allPredictorsCollection[i]);
                        trainingOutputsCollection.Add(new double[1]);
                        trainingOutputsCollection[trainingOutputsCollection.Count - 1][0] = allOutputsCollection[i][outputIdx];
                    }
                    else
                    {
                        //Testing sample
                        testingPredictorsCollection.Add(allPredictorsCollection[i]);
                        testingOutputsCollection.Add(new double[1]);
                        testingOutputsCollection[testingOutputsCollection.Count - 1][0] = allOutputsCollection[i][outputIdx];
                    }
                }
                //Regression
                results[outputIdx] = PerformRegression(outputIdx,
                                                       _settings.OutputFieldNameCollection[outputIdx],
                                                       reservoirsStats,
                                                       trainingPredictorsCollection,
                                                       trainingOutputsCollection,
                                                       testingPredictorsCollection,
                                                       testingOutputsCollection,
                                                       regressionController,
                                                       regressionControllerData
                                                       );
                //Store regression results and trained FF network
                _regressionResultCollection[outputIdx] = results[outputIdx];
            }

            return results;
        }


        /// <summary>
        /// Trains the Esn network.
        /// All input vectors are processed by internal reservoirs and the corresponding Esn predictors are recorded.
        /// Predictors are then subdivided into training data and test data.
        /// Training data is used to teach the output feed forward networks (regression phase).
        /// The degree of generalization is tested on test data.
        /// The goal is to select a network where there is not a big difference between
        /// the overall error on the training data and the test data.
        /// </summary>
        /// <param name="dataSet">
        /// The bundle containing known input and desired output sample vectors (in time order)
        /// </param>
        /// <param name="numOfBootSamples">
        /// How many of starting items from dataSet will be used for booting of reservoirs to ensure
        /// reservoir neurons states be affected by input data only. Boot sequence length of the reservoir should be greater
        /// or equal to reservoir neurons count. So for this Esn parameter use the boot sequence length of the largest
        /// reservoir in the Esn.
        /// </param>
        /// <param name="numOfTestSamples">
        /// How many samples from the dataSet will be used as the Esn test samples.
        /// </param>
        /// <param name="testSamplesSelector">
        /// Function to be called to select testing samples
        /// (use SelectSequentialTestSamples, SelectRandomTestSamples or implement your own method)
        /// </param>
        /// <param name="regressionController">
        /// Optional. see EsnRegressionCallbackDelegate
        /// </param>
        /// <param name="regressionControllerData">
        /// Optional custom object to be passed to regressionController together with other standard information
        /// </param>
        /// <returns>
        /// Array of regression outputs
        /// </returns>
        public EsnRegressionResult[] Train(VectorsPairBundle dataSet,
                                           int numOfBootSamples,
                                           int numOfTestSamples,
                                           TestSamplesSelectorDelegate testSamplesSelector,
                                           EsnRegressionCallbackDelegate regressionController = null,
                                           Object regressionControllerData = null
                                           )
        {
            EsnRegressionResult[] results = new EsnRegressionResult[_regressionResultCollection.Length];
            int numOfPredictors = _reservoirsReadoutLayer.Length;
            if(_settings.RouteInputToReadout)
            {
                numOfPredictors += _settings.InputFieldNameCollection.Count;
            }
            //All predictors to be used for training/testing
            int dataSetLength = dataSet.InputVectorCollection.Count;
            List<double[]> allPredictorsCollection = new List<double[]>(dataSetLength - numOfBootSamples);
            List<double[]> allOutputsCollection = new List<double[]>(dataSetLength - numOfBootSamples);
            //ESN Reset
            Reset();
            //Collection
            for (int dataSetIdx = 0; dataSetIdx < dataSetLength; dataSetIdx++)
            {
                bool afterBoot = (dataSetIdx >= numOfBootSamples);
                //Push input data into the Esn
                PushInput(dataSet.InputVectorCollection[dataSetIdx], afterBoot);
                //Is boot sequence passed? Collect predictors?
                if (afterBoot)
                {
                    //YES
                    //Predictors
                    double[] predictors = new double[numOfPredictors];
                    //Reservoir units states to predictors
                    _reservoirsReadoutLayer.CopyTo(predictors, 0);
                    if (_settings.RouteInputToReadout)
                    {
                        //Input to predictors
                        dataSet.InputVectorCollection[dataSetIdx].CopyTo(predictors, _reservoirsReadoutLayer.Length);
                    }
                    allPredictorsCollection.Add(predictors);
                    //Desired outputs
                    allOutputsCollection.Add(dataSet.OutputVectorCollection[dataSetIdx]);
                }
                PushFeedback(dataSet.OutputVectorCollection[dataSetIdx]);
            }

            //Collect reservoirs statistics
            List<AnalogReservoirStat> reservoirsStats = CollectReservoirInstancesStatatistics();
            
            //Alone regression for each Esn output field
            for (int outputIdx = 0; outputIdx < _regressionResultCollection.Length; outputIdx++)
            {
                //Testing data indexes
                int[] testIndexes = new int[numOfTestSamples];
                testSamplesSelector(allPredictorsCollection, allOutputsCollection, outputIdx, testIndexes);
                HashSet<int> testSampleIndexCollection = new HashSet<int>(testIndexes);
                //Dividing to training and testing sets
                int numOfTrainingSamples = allPredictorsCollection.Count - numOfTestSamples;
                List<double[]> trainingPredictorsCollection = new List<double[]>(numOfTrainingSamples);
                List<double[]> trainingOutputsCollection = new List<double[]>(numOfTrainingSamples);
                List<double[]> testingPredictorsCollection = new List<double[]>(numOfTestSamples);
                List<double[]> testingOutputsCollection = new List<double[]>(numOfTestSamples);
                for (int i = 0; i < allPredictorsCollection.Count; i++)
                {
                    if (!testSampleIndexCollection.Contains(i))
                    {
                        //Training sample
                        trainingPredictorsCollection.Add(allPredictorsCollection[i]);
                        trainingOutputsCollection.Add(new double[1]);
                        trainingOutputsCollection[trainingOutputsCollection.Count - 1][0] = allOutputsCollection[i][outputIdx];
                    }
                    else
                    {
                        //Testing sample
                        testingPredictorsCollection.Add(allPredictorsCollection[i]);
                        testingOutputsCollection.Add(new double[1]);
                        testingOutputsCollection[testingOutputsCollection.Count - 1][0] = allOutputsCollection[i][outputIdx];
                    }
                }
                //Regression
                results[outputIdx] = PerformRegression(outputIdx,
                                                       _settings.OutputFieldNameCollection[outputIdx],
                                                       reservoirsStats,
                                                       trainingPredictorsCollection,
                                                       trainingOutputsCollection,
                                                       testingPredictorsCollection,
                                                       testingOutputsCollection,
                                                       regressionController,
                                                       regressionControllerData
                                                       );
                //Store regression results and trained FF network
                _regressionResultCollection[outputIdx] = results[outputIdx];
            }

            return results;
        }

        /// <summary>
        /// Builds and trains FF network for specified Esn output field
        /// </summary>
        private EsnRegressionResult PerformRegression(int outputFieldIdx,
                                                      string outputFieldName,
                                                      List<AnalogReservoirStat> reservoirStatisticsCollection,
                                                      List<double[]> trainingPredictorsCollection,
                                                      List<double[]> trainingOutputsCollection,
                                                      List<double[]> testingPredictorsCollection,
                                                      List<double[]> testingOutputsCollection,
                                                      EsnRegressionCallbackDelegate Controller = null,
                                                      Object controllerData = null
                                                      )
        {
            EsnRegressionResult regrBestResult = new EsnRegressionResult();
            //Create FF network
            FeedForwardNetwork ffn = new FeedForwardNetwork(trainingPredictorsCollection[0].Length, testingOutputsCollection[0].Length);
            for (int i = 0; i < _settings.HiddenLayerCollection.Count; i++)
            {
                ffn.AddLayer(_settings.HiddenLayerCollection[i].NumOfNeurons,
                             ActivationFactory.CreateActivationFunction(_settings.HiddenLayerCollection[i].ActivationType)
                             );
            }
            ffn.FinalizeStructure(ActivationFactory.CreateActivationFunction(_settings.OutputNeuronActivation));
            //Regression attempts
            bool stopRegression = false;
            for (int regrAttemptNumber = 1; regrAttemptNumber <= _settings.RegressionAttempts; regrAttemptNumber++)
            {
                //Reset feed forward network's weights to random values
                ffn.RandomizeWeights(_rand);
                //Create trainer object
                IFeedForwardNetworkTrainer trainer = null;
                switch (_settings.RegressionMethod)
                {
                    case TrainingMethodType.Linear:
                        trainer = new LinRegrTrainer(ffn, trainingPredictorsCollection, trainingOutputsCollection, _settings.RegressionAttemptEpochs, _rand);
                        break;
                    case TrainingMethodType.Resilient:
                        trainer = new RPropTrainer(ffn, trainingPredictorsCollection, trainingOutputsCollection);
                        break;
                    default:
                        throw new ArgumentException($"Unknown regression method {_settings.RegressionMethod}");
                }
                //Iterate training cycles
                for (int epoch = 1; epoch <= _settings.RegressionAttemptEpochs; epoch++)
                {
                    trainer.Iteration();
                    //Compute current error statistics after training iteration
                    EsnRegressionResult regrCurrResult = new EsnRegressionResult();
                    regrCurrResult.OutputFieldName = outputFieldName;
                    regrCurrResult.FFNet = ffn;
                    regrCurrResult.TrainingErrorStat = ffn.ComputeBatchErrorStat(trainingPredictorsCollection, trainingOutputsCollection);
                    regrCurrResult.CombinedError = regrCurrResult.TrainingErrorStat.ArithAvg;
                    if (testingPredictorsCollection != null)
                    {
                        regrCurrResult.TestingErrorStat = ffn.ComputeBatchErrorStat(testingPredictorsCollection, testingOutputsCollection);
                        regrCurrResult.CombinedError = Math.Max(regrCurrResult.CombinedError, regrCurrResult.TestingErrorStat.ArithAvg);
                    }
                    //Current results processing
                    bool best = false, stopTrainingCycle = false;
                    //Result first initialization
                    if (regrBestResult.CombinedError == -1)
                    {
                        //Adopt current regression results
                        regrBestResult = regrCurrResult.DeepClone();
                        ++regrBestResult.WeightsUpdateCounter;
                    }
                    //Perform call back if it is defined
                    EsnRegressionControlOutArgs cbOut = null;
                    if (Controller != null)
                    {
                        //Evaluation of the improvement is driven externaly
                        EsnRegressionControlInArgs cbIn = new EsnRegressionControlInArgs();
                        cbIn.ReservoirStatisticsCollection = reservoirStatisticsCollection;
                        cbIn.OutputFieldIdx = outputFieldIdx;
                        cbIn.OutputFieldName = outputFieldName;
                        cbIn.RegrAttemptNumber = regrAttemptNumber;
                        cbIn.RegrMaxAttempt = _settings.RegressionAttempts;
                        cbIn.Epoch = epoch;
                        cbIn.MaxEpoch = _settings.RegressionAttemptEpochs;
                        cbIn.TrainingPredictorsCollection = trainingPredictorsCollection;
                        cbIn.TrainingOutputsCollection = trainingOutputsCollection;
                        cbIn.TestingPredictorsCollection = testingPredictorsCollection;
                        cbIn.TestingOutputsCollection = testingOutputsCollection;
                        cbIn.RegrCurrResult = regrCurrResult;
                        cbIn.RegrBestResult = regrBestResult;
                        cbIn.ControllerData = controllerData;
                        cbOut = Controller(cbIn);
                        best = cbOut.Best;
                        stopTrainingCycle = cbOut.StopCurrentAttempt;
                        stopRegression = cbOut.StopRegression;
                    }
                    else
                    {
                        //Default implementation
                        if (regrCurrResult.CombinedError < regrBestResult.CombinedError)
                        {
                            best = true;
                        }
                    }
                    //Best?
                    if (best)
                    {
                        //Adopt current regression results
                        regrBestResult = regrCurrResult.DeepClone();
                        ++regrBestResult.WeightsUpdateCounter;
                    }
                    //Training stop conditions
                    if (regrCurrResult.TrainingErrorStat.RootMeanSquare <= _settings.RegressionAttemptStopMSE ||
                        stopTrainingCycle ||
                        stopRegression
                        )
                    {
                        break;
                    }
                }
                //Regression stop conditions
                if (stopRegression)
                {
                    break;
                }
            }
            //Create statistics of the best feed forward network weights
            regrBestResult.OutputWeightsStat = regrBestResult.FFNet.ComputeWeightsStat();
            return regrBestResult;
        }


        //Inner classes
        /// <summary>
        /// Holds the instantiated reservoir together with its definition.
        /// </summary>
        [Serializable]
        private class ReservoirInstance
        {
            //Attribute properties
            /// <summary>
            /// Instance definition.
            /// </summary>
            public EsnSettings.ReservoirInstanceDefinition InstanceDefinition { get; }
            /// <summary>
            /// Instantiated reservoir.
            /// </summary>
            public AnalogReservoir ReservoirObj { get; }

            //Constructor
            public ReservoirInstance(EsnSettings.ReservoirInstanceDefinition instanceDefinition, int randomizerSeek)
            {
                //Store definition
                InstanceDefinition = instanceDefinition;
                //Create reservoir
                ReservoirObj = new AnalogReservoir(InstanceDefinition.InstanceName,
                                                   InstanceDefinition.InputFieldMappingCollection.Count,
                                                   InstanceDefinition.ReservoirSettings,
                                                   InstanceDefinition.AugmentedStates,
                                                   randomizerSeek
                                                   );
                return;
            }

            //Methods
            /// <summary>
            /// Resets reservoir internal state to the initial state.
            /// </summary>
            public void Reset()
            {
                ReservoirObj.Reset();
                return;
            }

        }//ReservoirInstance

        /// <summary>
        /// Contains the best trained feed forward network and related
        /// important error statistics.
        /// </summary>
        [Serializable]
        public class EsnRegressionResult
        {
            //Attribute properties
            /// <summary>
            /// Esn output field name for which the regression was performed
            /// </summary>
            public string OutputFieldName;
            /// <summary>
            /// Trained feed forward network
            /// </summary>
            public FeedForwardNetwork FFNet { get; set; }
            /// <summary>
            /// Training error statistics
            /// </summary>
            public BasicStat TrainingErrorStat { get; set; }
            /// <summary>
            /// Testing error statistics
            /// </summary>
            public BasicStat TestingErrorStat { get; set; }
            /// <summary>
            /// Statistics of the FF network weights
            /// </summary>
            public BasicStat OutputWeightsStat { get; set; }
            /// <summary>
            /// Achieved combined error.
            /// Formula for combined error calculation is Max(training error, testing error)
            /// </summary>
            public double CombinedError { get; set; }
            /// <summary>
            /// Number of updates of the best weights
            /// </summary>
            public int WeightsUpdateCounter { get; set; }

            //Constructor
            /// <summary>
            /// Creates an unitialized instance
            /// </summary>
            public EsnRegressionResult()
            {
                OutputFieldName = string.Empty;
                FFNet = null;
                TrainingErrorStat = null;
                TestingErrorStat = null;
                OutputWeightsStat = null;
                CombinedError = -1;
                WeightsUpdateCounter = 0;
                return;
            }

            /// <summary>
            /// The deep copy constructor.
            /// </summary>
            /// <param name="source">Source instance</param>
            public EsnRegressionResult(EsnRegressionResult source)
            {
                OutputFieldName = source.OutputFieldName;
                FFNet = source.FFNet.Clone();
                TrainingErrorStat = new BasicStat(source.TrainingErrorStat);
                TestingErrorStat = new BasicStat(source.TestingErrorStat);
                CombinedError = source.CombinedError;
                return;
            }
            
            //Methods
            /// <summary>
            /// Creates the deep copy instance of this instance
            /// </summary>
            public EsnRegressionResult DeepClone()
            {
                return new EsnRegressionResult(this);
            }

        }//EsnRegressionResult

        /// <summary>
        /// The class contains information needed to control the progress of the regression process.
        /// </summary>
        [Serializable]
        public class EsnRegressionControlInArgs
        {
            //Attribute properties
            /// <summary>
            /// Collection of important statistics for each instance of the reservoir
            /// </summary>
            public List<AnalogReservoirStat> ReservoirStatisticsCollection { get; set; } = null;
            /// <summary>
            /// Esn output field index for which the regression is performing
            /// </summary>
            public int OutputFieldIdx { get; set; } = 0;
            /// <summary>
            /// Esn output field name for which the regression is performing
            /// </summary>
            public string OutputFieldName;
            /// <summary>
            /// Current regression attempt number 
            /// </summary>
            public int RegrAttemptNumber { get; set; } = -1;
            /// <summary>
            /// Maximum number of regression attempts
            /// </summary>
            public int RegrMaxAttempt { get; set; } = -1;
            /// <summary>
            /// Current epoch number
            /// </summary>
            public int Epoch { get; set; } = -1;
            /// <summary>
            /// Maximum nuber of epochs
            /// </summary>
            public int MaxEpoch { get; set; } = -1;
            /// <summary>
            /// Esn predictors collection for training
            /// </summary>
            public List<double[]> TrainingPredictorsCollection { get; set; } = null;
            /// <summary>
            /// Desired outputs collection for training
            /// </summary>
            public List<double[]> TrainingOutputsCollection { get; set; } = null;
            /// <summary>
            /// Esn predictors collection for testing
            /// </summary>
            public List<double[]> TestingPredictorsCollection { get; set; } = null;
            /// <summary>
            /// Desired outputs collection for testing
            /// </summary>
            public List<double[]> TestingOutputsCollection { get; set; } = null;
            /// <summary>
            /// Contains the current feed forward network and related
            /// important error statistics.
            /// </summary>
            public EsnRegressionResult RegrCurrResult { get; set; } = null;
            /// <summary>
            /// Contains the best feed forward network for now and related
            /// important error statistics.
            /// </summary>
            public EsnRegressionResult RegrBestResult { get; set; } = null;
            /// <summary>
            /// The custom user object that was passed to the Train method
            /// </summary>
            public Object ControllerData { get; set; } = null;
        }//EsnRegressionControlInArgs

        /// <summary>
        /// Contains instructions for the regression process
        /// </summary>
        [Serializable]
        public class EsnRegressionControlOutArgs
        {
            //Attribute properties
            /// <summary>
            /// Indicates whether to end the current regression attempt
            /// </summary>
            public bool StopCurrentAttempt { get; set; } = false;
            /// <summary>
            /// Indicates whether to end the entire regression process for the current Esn output field
            /// </summary>
            public bool StopRegression { get; set; } = false;
            /// <summary>
            /// This is the most important switch indicating whether the RegrCurrResult is better than
            /// the existing RegrBestResult
            /// </summary>
            public bool Best { get; set; } = false;
        }//EsnRegressionControlOutArgs

    }//Esn
}//Namespace
