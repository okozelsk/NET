using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OKOSW.Extensions;
using OKOSW.MathTools;
using OKOSW.Neural.Activation;
using OKOSW.Neural.Networks.Data;
using OKOSW.Neural.Networks.FF;
using OKOSW.Neural.Reservoir.Analog;

namespace OKOSW.Neural.Networks.EchoState
{
    /// <summary>
    /// Implements Echo State Network
    /// </summary>
    [Serializable]
    public class Esn
    {
        //Delegates
        /// <summary>
        /// Selects testing samples from all_ samples.
        /// </summary>
        /// <param name="allPredictors">All sample predictors</param>
        /// <param name="allOutputs">All desired sample outputs linked with allPredictors</param>
        /// <param name="regrOutputFieldIdx">Index of ESN output value for which will be network trained.</param>
        /// <param name="testSamplesIdxs">Array to be filled with selected test samples indexes (function output).</param>
        public delegate void EsnTestSamplesSelectorCallbackDelegate(List<double[]> allPredictors, List<double[]> allOutputs, int regrOutputFieldIdx, int[] testSamplesIdxs);

        /// <summary>
        /// Esn regression callback.
        /// Function is continuously called during the regression phase to give the control over regression progress. 
        /// </summary>
        /// <param name="inArgs">Regression control information object</param>
        /// <returns>Instructions for regression</returns>
        public delegate EsnRegressionControlOutArgs EsnRegressionCallbackDelegate(EsnRegressionControlInArgs inArgs);

        //Attributes
        private EsnSettings _settings;
        private Random _rand;
        private List<ReservoirInstance> _reservoirInstances;
        private double[] _unitsStates;
        private EsnRegressionData[] _regression;

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
            int reservoirUnitsOutputTotalLength = 0;
            _reservoirInstances = new List<ReservoirInstance>(_settings.Input2ResSettingsMapCollection.Count);
            foreach(EsnSettings.Input2ResSettingsMap mapping in _settings.Input2ResSettingsMapCollection)
            {
                ReservoirInstance newResInstance = new ReservoirInstance(mapping, _settings.RandomizerSeek);
                _reservoirInstances.Add(newResInstance);
                reservoirUnitsOutputTotalLength += newResInstance.ReservoirObj.OutputPredictorsCount;
            }
            //Reservoir(s) output states holder
            _unitsStates = new double[reservoirUnitsOutputTotalLength];
            _unitsStates.Populate(0);
            //Output fields regressions
            _regression = new EsnRegressionData[_settings.OutputFieldsNames.Count];
            _regression.Populate(null);
            return;
        }

        //Properties

        //Methods
        /// <summary>
        /// Sets network internal state to initial state
        /// </summary>
        private void Reset()
        {
            foreach(ReservoirInstance reservoirInstanceData in _reservoirInstances)
            {
                reservoirInstanceData.Reset();
            }
            _unitsStates.Populate(0);
            return;
        }

        /// <summary>
        /// Pushes input values into the reservoirs and stores reservoirs output (predictors)
        /// </summary>
        /// <param name="inputValues">Input values</param>
        private void PushInput(double[] inputValues, bool afterBoot)
        {
            int reservoirsUnitsOutputIdx = 0;
            //Compute reservoir(s)
            foreach (ReservoirInstance resInstance in _reservoirInstances)
            {
                double[] resInput = new double[resInstance.Mapping.InputFieldsIdxs.Count];
                for(int i = 0; i < resInstance.Mapping.InputFieldsIdxs.Count; i++)
                {
                    resInput[i] = inputValues[resInstance.Mapping.InputFieldsIdxs[i]];
                }
                double[] reservoirOutput = new double[resInstance.ReservoirObj.OutputPredictorsCount];
                //Compute reservoir output
                resInstance.ReservoirObj.Compute(resInput, reservoirOutput, afterBoot);
                //Add output to common all units output
                reservoirOutput.CopyTo(_unitsStates, reservoirsUnitsOutputIdx);
                reservoirsUnitsOutputIdx += reservoirOutput.Length;
            }
            return;
        }

        /// <summary>
        /// Predicts next output values
        /// </summary>
        /// <param name="inputValues">Input values</param>
        /// <returns>Computed output values</returns>
        public double[] PredictNext(double[] inputValues)
        {
            PushInput(inputValues, true);
            int predictorsCount = _unitsStates.Length;
            if (_settings.RouteInputToReadout)
            {
                predictorsCount += inputValues.Length;
            }
            //Readout predictors
            double[] predictors = new double[predictorsCount];
            _unitsStates.CopyTo(predictors, 0);
            if (_settings.RouteInputToReadout)
            {
                inputValues.CopyTo(predictors, _unitsStates.Length);
            }
            double[] esnOutput = new double[_regression.Length];
            for (int i = 0; i < _regression.Length; i++)
            {
                double[] outputValue;
                outputValue = _regression[i].FFNet.Compute(predictors);
                esnOutput[i] = outputValue[0];
            }
            return esnOutput;
        }

        /// <summary>
        /// Could be called before next prediction to tell the network right previous outputs (feedback)
        /// </summary>
        /// <param name="lastRealValues">Last real values (or last esn outputs)</param>
        public void PushFeedback(double[] lastRealValues)
        {
            foreach (ReservoirInstance resInstance in _reservoirInstances)
            {
                if (resInstance.Mapping.ReservoirSettings.FeedbackFeature)
                {
                    double[] feedbackValues = new double[resInstance.Mapping.FeedbackFieldsNames.Count];
                    for (int i = 0; i < resInstance.Mapping.FeedbackFieldsIdxs.Count; i++)
                    {
                        feedbackValues[i] = lastRealValues[resInstance.Mapping.FeedbackFieldsIdxs[i]];
                    }
                    resInstance.ReservoirObj.SetFeedback(feedbackValues);
                }
            }
            return;
        }

        /// <summary>
        /// Selects testing samples to be the lasting sequence in all_ samples.
        /// </summary>
        /// <param name="allPredictors">All sample predictors</param>
        /// <param name="allOutputs">All desired sample outputs corresponding with allPredictors</param>
        /// <param name="regrOutputFieldIdx">Index of ESN output value for which will be network trained. Not used here.</param>
        /// <param name="testSamplesIdxs">Array to be filled with selected test samples indexes</param>
        public void SelectSequentialTestSamples(List<double[]> allPredictors, List<double[]> allOutputs, int regrOutputFieldIdx, int[] testSamplesIdxs)
        {
            //Sequential selection
            for (int srcIdx = allPredictors.Count - testSamplesIdxs.Length, i = 0; srcIdx < allPredictors.Count; srcIdx++, i++)
            {
                testSamplesIdxs[i] = srcIdx;
            }
            return;
        }

        /// <summary>
        /// Selects testing samples randomly from all_ samples.
        /// </summary>
        /// <param name="allPredictors">All sample predictors</param>
        /// <param name="allOutputs">All desired sample outputs corresponding with allPredictors</param>
        /// <param name="regrOutputFieldIdx">Index of ESN output value for which will be network trained. Not used here.</param>
        /// <param name="testSamplesIdxs">Array to be filled with selected test samples indexes</param>
        public void SelectRandomTestSamples(List<double[]> allPredictors, List<double[]> allOutputs, int regrOutputFieldIdx, int[] testSamplesIdxs)
        {
            //Random selection
            int[] randIndexes = new int[allPredictors.Count];
            randIndexes.ShuffledIndices(_rand);
            for (int i = 0; i < testSamplesIdxs.Length; i++)
            {
                testSamplesIdxs[i] = randIndexes[i];
            }
            return;
        }

        /// <summary>
        /// Collects key statistics of each reservoir instance
        /// </summary>
        /// <returns></returns>
        public List<AnalogReservoirStat> CollectReservoirsStats()
        {
            List<AnalogReservoirStat> stats = new List<AnalogReservoirStat>();
            foreach(ReservoirInstance resInstance in _reservoirInstances)
            {
                stats.Add(resInstance.ReservoirObj.CollectStateStatistics());
            }
            return stats;
        }


        /// <summary>
        /// Trains network (computes output weights).
        /// </summary>
        /// <param name="dataSet">Bundle containing all known input and desired output samples (in time order)</param>
        /// <param name="bootSamplesCount">How many of starting items from dataSet will be used for booting of reservoirs (to ensure reservoir neurons states consistently corresponding to input data)</param>
        /// <param name="testSamplesCount">How many samples from dataSet to use as the test samples</param>
        /// <param name="testSamplesSelector">Function to be called to select testing samples (use SelectSequenceTestSamples, SelectTestSamples_Rnd or implement your own)</param>
        /// <param name="RegressionController">EsnRegressionCallbackDelegate</param>
        /// <param name="regressionControllerData">Custom object to be passed to RegressionController together with other standard information</param>
        /// <returns>Array of regression outputs</returns>
        public EsnRegressionData[] Train(SamplesDataBundle dataSet,
                                 int bootSamplesCount,
                                 int testSamplesCount,
                                 EsnTestSamplesSelectorCallbackDelegate testSamplesSelector,
                                 EsnRegressionCallbackDelegate RegressionController = null,
                                 Object regressionControllerData = null
                                 )
        {
            EsnRegressionData[] results = new EsnRegressionData[_regression.Length];
            int predictorsCount = _unitsStates.Length;
            if(_settings.RouteInputToReadout)
            {
                predictorsCount += _settings.InputFieldsCount;
            }
            //All predictors and outputs collection
            int dataSetLength = dataSet.Inputs.Count;
            List<double[]> all_predictors = new List<double[]>(dataSetLength - bootSamplesCount);
            List<double[]> all_outputs = new List<double[]>(dataSetLength - bootSamplesCount);
            //ESN Reset
            Reset();
            //Collection
            for (int dataSetIdx = 0; dataSetIdx < dataSetLength; dataSetIdx++)
            {
                PushInput(dataSet.Inputs[dataSetIdx], (dataSetIdx >= bootSamplesCount));
                //Is boot sequence passed? Collect predictors?
                if (dataSetIdx >= bootSamplesCount)
                {
                    //YES
                    //Predictors
                    double[] predictors = new double[predictorsCount];
                    //Reservoir units states to predictors
                    _unitsStates.CopyTo(predictors, 0);
                    if (_settings.RouteInputToReadout)
                    {
                        //Input to predictors
                        dataSet.Inputs[dataSetIdx].CopyTo(predictors, _unitsStates.Length);
                    }
                    all_predictors.Add(predictors);
                    //Desired outputs
                    all_outputs.Add(dataSet.Outputs[dataSetIdx]);
                }
                PushFeedback(dataSet.Outputs[dataSetIdx]);
            }
            //Statistics
            List<AnalogReservoirStat> reservoirsStats = CollectReservoirsStats();
            
            //Alone regression for each ESN output field
            for (int outputIdx = 0; outputIdx < _regression.Length; outputIdx++)
            {
                //Testing data indexes
                int[] testIndexes = new int[testSamplesCount];
                testSamplesSelector(all_predictors, all_outputs, outputIdx, testIndexes);
                HashSet<int> testIndexesHashSet = new HashSet<int>(testIndexes);
                //Dividing to training and testing sets
                int trainingSeqLength = all_predictors.Count - testSamplesCount;
                List<double[]> trainingPredictors = new List<double[]>(trainingSeqLength);
                List<double[]> trainingOutputs = new List<double[]>(trainingSeqLength);
                List<double[]> testingPredictors = new List<double[]>(testSamplesCount);
                List<double[]> testingOutputs = new List<double[]>(testSamplesCount);
                for (int i = 0; i < all_predictors.Count; i++)
                {
                    if (!testIndexesHashSet.Contains(i))
                    {
                        //Training sample
                        trainingPredictors.Add(all_predictors[i]);
                        trainingOutputs.Add(new double[1]);
                        trainingOutputs[trainingOutputs.Count - 1][0] = all_outputs[i][outputIdx];
                    }
                    else
                    {
                        //Testing sample
                        testingPredictors.Add(all_predictors[i]);
                        testingOutputs.Add(new double[1]);
                        testingOutputs[testingOutputs.Count - 1][0] = all_outputs[i][outputIdx];
                    }
                }
                //Regression
                results[outputIdx] = PerformRegression(outputIdx,
                                                       _settings.OutputFieldsNames[outputIdx],
                                                       reservoirsStats,
                                                       trainingPredictors,
                                                       trainingOutputs,
                                                       testingPredictors,
                                                       testingOutputs,
                                                       RegressionController,
                                                       regressionControllerData
                                                       );
                //Store regression results and FF network
                _regression[outputIdx] = results[outputIdx];
            }

            return results;
        }

        /// <summary>
        /// Builds and trains FF network for specified Esn output field
        /// </summary>
        private EsnRegressionData PerformRegression(int outputFieldIdx,
                                                    string outputFieldName,
                                                    List<AnalogReservoirStat> reservoirsStatistics,
                                                    List<double[]> trainingPredictors,
                                                    List<double[]> trainingOutputs,
                                                    List<double[]> testingPredictors,
                                                    List<double[]> testingOutputs,
                                                    EsnRegressionCallbackDelegate Controller = null,
                                                    Object controllerData = null
                                                    )
        {
            EsnRegressionData bestRegrData = new EsnRegressionData();
            //Create FF network
            FeedForwardNetwork network = new FeedForwardNetwork(trainingPredictors[0].Length, testingOutputs[0].Length);
            for (int i = 0; i < _settings.ReadOutHiddenLayers.Count; i++)
            {
                network.AddLayer(_settings.ReadOutHiddenLayers[i].NeuronsCount,
                                 ActivationFactory.CreateActivationFunction(_settings.ReadOutHiddenLayers[i].ActivationType)
                                 );
            }
            network.FinalizeStructure(ActivationFactory.CreateActivationFunction(_settings.OutputNeuronActivation));
            //Regression attempts
            bool stopRegression = false;
            for (int regrAttemptNumber = 1; regrAttemptNumber <= _settings.RegressionAttempts; regrAttemptNumber++)
            {
                network.RandomizeWeights(_rand);
                //Create trainer object
                IFeedForwardNetworkTrainer trainer = null;
                switch (_settings.RegressionMethod)
                {
                    case TrainingMethodType.Linear:
                        trainer = new LinRegrTrainer(network, trainingPredictors, trainingOutputs, _settings.RegressionAttemptEpochs, _rand);
                        break;
                    case TrainingMethodType.Resilient:
                        trainer = new RPropTrainer(network, trainingPredictors, trainingOutputs);
                        break;
                    default:
                        throw new ArgumentException($"Unknown regression method {_settings.RegressionMethod}");
                }
                //Iterate training cycles
                for (int epoch = 1; epoch <= _settings.RegressionAttemptEpochs; epoch++)
                {
                    trainer.Iteration();
                    //Compute current error statistics after training iteration
                    EsnRegressionData currRegrData = new EsnRegressionData();
                    currRegrData.OutputFieldName = outputFieldName;
                    currRegrData.FFNet = network;
                    currRegrData.TrainingErrorStat = network.ComputeBatchErrorStat(trainingPredictors, trainingOutputs);
                    currRegrData.CombinedError = currRegrData.TrainingErrorStat.ArithAvg;
                    if (testingPredictors != null)
                    {
                        currRegrData.TestingErrorStat = network.ComputeBatchErrorStat(testingPredictors, testingOutputs);
                        currRegrData.CombinedError = Math.Max(currRegrData.CombinedError, currRegrData.TestingErrorStat.ArithAvg);
                    }
                    //Current results processing
                    bool best = false, stopTrainingCycle = false;
                    //Result first initialization
                    if (bestRegrData.CombinedError == -1)
                    {
                        //Adopt current regression results
                        bestRegrData = currRegrData.DeepClone();
                        ++bestRegrData.BestUpdatesCount;
                    }
                    //Perform call back if it is defined
                    EsnRegressionControlOutArgs cbOut = null;
                    if (Controller != null)
                    {
                        //Improvement evaluation is driven externaly
                        EsnRegressionControlInArgs cbIn = new EsnRegressionControlInArgs();
                        cbIn.ReservoirsStatistics = reservoirsStatistics;
                        cbIn.OutputFieldIdx = outputFieldIdx;
                        cbIn.OutputFieldName = outputFieldName;
                        cbIn.RegrAttemptNumber = regrAttemptNumber;
                        cbIn.Epoch = epoch;
                        cbIn.TrainingPredictors = trainingPredictors;
                        cbIn.TrainingOutputs = trainingOutputs;
                        cbIn.TestingPredictors = testingPredictors;
                        cbIn.TestingOutputs = testingOutputs;
                        cbIn.CurrRegrData = currRegrData;
                        cbIn.BestRegrData = bestRegrData;
                        cbIn.ControllerData = controllerData;
                        cbOut = Controller(cbIn);
                        best = cbOut.Best;
                        stopTrainingCycle = cbOut.StopCurrentAttempt;
                        stopRegression = cbOut.StopRegression;
                    }
                    else
                    {
                        //Default implementation
                        if (currRegrData.CombinedError < bestRegrData.CombinedError)
                        {
                            best = true;
                        }
                    }
                    //Best?
                    if (best)
                    {
                        //Adopt current regression results
                        bestRegrData = currRegrData.DeepClone();
                        ++bestRegrData.BestUpdatesCount;
                    }
                    //Training stop conditions
                    if (currRegrData.TrainingErrorStat.RootMeanSquare <= _settings.RegressionAttemptStopMSE || stopTrainingCycle || stopRegression)
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
            //Statistics of best network weights
            bestRegrData.OutputWeightsStat = bestRegrData.FFNet.ComputeWeightsStat();
            return bestRegrData;
        }




        //Inner classes
        /// <summary>
        /// Contains trained feed forward network and key statistics
        /// </summary>
        [Serializable]
        public class EsnRegressionData
        {
            //Attribute properties
            public string OutputFieldName;
            public FeedForwardNetwork FFNet { get; set; }
            public BasicStat TrainingErrorStat { get; set; }
            public BasicStat TestingErrorStat { get; set; }
            public BasicStat OutputWeightsStat { get; set; }
            public double CombinedError { get; set; }
            public int BestUpdatesCount { get; set; }

            //Constructor
            public EsnRegressionData()
            {
                OutputFieldName = string.Empty;
                FFNet = null;
                TrainingErrorStat = null;
                TestingErrorStat = null;
                OutputWeightsStat = null;
                CombinedError = -1;
                BestUpdatesCount = 0;
                return;
            }

            public EsnRegressionData(EsnRegressionData source)
            {
                OutputFieldName = source.OutputFieldName;
                FFNet = source.FFNet.Clone();
                TrainingErrorStat = new BasicStat(source.TrainingErrorStat);
                TestingErrorStat = new BasicStat(source.TestingErrorStat);
                CombinedError = source.CombinedError;
                return;
            }
            
            //Methods
            public EsnRegressionData DeepClone()
            {
                return new EsnRegressionData(this);
            }
        }

        [Serializable]
        public class EsnRegressionControlInArgs
        {
            //Attribute properties
            public List<AnalogReservoirStat> ReservoirsStatistics { get; set; } = null;
            public int OutputFieldIdx { get; set; } = 0;
            public string OutputFieldName;
            public int RegrAttemptNumber { get; set; } = -1;
            public int Epoch { get; set; } = -1;
            public List<double[]> TrainingPredictors { get; set; } = null;
            public List<double[]> TrainingOutputs { get; set; } = null;
            public List<double[]> TestingPredictors { get; set; } = null;
            public List<double[]> TestingOutputs { get; set; } = null;
            public EsnRegressionData CurrRegrData { get; set; } = null;
            public EsnRegressionData BestRegrData { get; set; } = null;
            public Object ControllerData { get; set; } = null;
        }

        [Serializable]
        public class EsnRegressionControlOutArgs
        {
            //Attribute properties
            public bool StopCurrentAttempt { get; set; } = false;
            public bool StopRegression { get; set; } = false;
            public bool Best { get; set; } = false;
        }


        [Serializable]
        public class ReservoirInstance
        {
            //Attribute properties
            public EsnSettings.Input2ResSettingsMap Mapping { get; }
            public AnalogReservoir ReservoirObj { get; }

            //Constructor
            public ReservoirInstance(EsnSettings.Input2ResSettingsMap mapping, int randomizerSeek)
            {
                Mapping = mapping;
                ReservoirObj = new AnalogReservoir(Mapping.InstanceName, Mapping.InputFieldsIdxs.Count, Mapping.ReservoirSettings, Mapping.AugmentedStates, randomizerSeek);
                return;
            }

            //Methods
            public void Reset()
            {
                ReservoirObj.Reset();
                return;
            }

        }//ReservoirInstance


    }//ESN
}//Namespace
