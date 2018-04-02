using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Activation;
using RCNet.Neural.Network.FF;

namespace RCNet.Neural.Network.RCReadout
{
    /// <summary>
    /// Class encaptulates common readout-layer regression logic.
    /// </summary>
    public static class Regression
    {

        //Delegates
        /// <summary>
        /// Selects testing samples from all available samples.
        /// </summary>
        /// <param name="predictorsCollection">Predictors collection</param>
        /// <param name="idealOutputsCollection">Desired outputs collection (desired outputs in the same order as predictors)</param>
        /// <param name="readoutUnitIdx">Index of the readout unit (output field) for which the regression will be performed.</param>
        /// <param name="testSampleIdxCollection">Array of test sample indexes to be filled by this function.</param>
        public delegate void TestSamplesSelectorDelegate(List<double[]> predictorsCollection,
                                                         List<double[]> idealOutputsCollection,
                                                         int readoutUnitIdx,
                                                         int[] testSampleIdxCollection
                                                         );

        /// <summary>
        /// This is the control function of the regression process and is called
        /// after the completion of each regression training epoch.
        /// The goal of the regression process is for each readout unit to train a feed forward network
        /// that will give good results both on the training data and the test data.
        /// RegressionControlInArgs object passed to the function contains the best error statistics so far
        /// and the latest statistics. The primary purpose of the function is to decide whether the latest statistics
        /// are better than the best statistics so far.
        /// The function can also tell the regression process that it does not make any sense to continue the regression.
        /// It can terminate the current regression attempt or whole readout unit regression process.
        /// </summary>
        /// <param name="inArgs">Contains all the necessary information to control the progress of the regression.</param>
        /// <returns>Instructions for the regression process.</returns>
        public delegate RegressionControlOutArgs RegressionCallbackDelegate(RegressionControlInArgs inArgs);

        //Methods
        /// <summary>
        /// Selects the testing samples as the lasting sequence from given samples.
        /// See the TestSamplesSelectorDelegate.
        /// </summary>
        public static void SelectSequentialTestSamples(List<double[]> predictorsCollection,
                                                       List<double[]> idealOutputsCollection,
                                                       int readoutUnitIdx,
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
        /// See the TestSamplesSelectorDelegate.
        /// </summary>
        public static void SelectRandomTestSamples(List<double[]> predictorsCollection,
                                                   List<double[]> idealOutputsCollection,
                                                   int readoutUnitIdx,
                                                   int[] testSampleIdxCollection
                                                   )
        {
            Random rand = new Random(0);
            //Random selection
            int[] randIndexes = new int[predictorsCollection.Count];
            randIndexes.ShuffledIndices(rand);
            for (int i = 0; i < testSampleIdxCollection.Length; i++)
            {
                testSampleIdxCollection[i] = randIndexes[i];
            }
            return;
        }

        /// <summary>
        /// Function prepares readout layer. One readout unit for each output field.
        /// </summary>
        /// <param name="outputFieldNameCollection">Collection of the output field names</param>
        /// <param name="predictorsCollection">Collection of all available Esn predictors</param>
        /// <param name="idealOutputsCollection">Collection of all available desired outputs related to predictors</param>
        /// <param name="numOfTestSamples">Required number of test samples</param>
        /// <param name="testSamplesSelector">Test samples selector delegate</param>
        /// <param name="rand">Random object to be used</param>
        /// <param name="hiddenLayerCollection">Collection of output FF network hidden layer settings</param>
        /// <param name="outputNeuronActivation">Activation type of the FF output layer neuron</param>
        /// <param name="regressionMethod">Regression method to be used</param>
        /// <param name="regrAttempts">Number of regression attempts</param>
        /// <param name="attemptEpochs">Number of epochs for each regression attempt</param>
        /// <param name="stopMSE">The achieved training MSE when to terminate regression attempt</param>
        /// <param name="regressionController">Regression controller delegate</param>
        /// <param name="regressionControllerData">An user object</param>
        /// <returns>Array of prepared readout units</returns>
        public static ReadoutUnit[] LayerRegressions(List<string> outputFieldNameCollection,
                                                     List<double[]> predictorsCollection,
                                                     List<double[]> idealOutputsCollection,
                                                     int numOfTestSamples,
                                                     TestSamplesSelectorDelegate testSamplesSelector,
                                                     Random rand,
                                                     List<HiddenLayerSettings> hiddenLayerCollection,
                                                     ActivationFactory.ActivationType outputNeuronActivation,
                                                     TrainingMethodType regressionMethod,
                                                     int regrAttempts,
                                                     int attemptEpochs,
                                                     double stopMSE,
                                                     RegressionCallbackDelegate regressionController,
                                                     Object regressionControllerData
                                                     )
        {
            ReadoutUnit[] readoutUnits = new ReadoutUnit[outputFieldNameCollection.Count];
            //Alone regression for each output field
            for (int outputIdx = 0; outputIdx < outputFieldNameCollection.Count; outputIdx++)
            {
                //Testing data indexes
                int[] testIndexes = new int[numOfTestSamples];
                testSamplesSelector(predictorsCollection, idealOutputsCollection, outputIdx, testIndexes);
                HashSet<int> testSampleIndexCollection = new HashSet<int>(testIndexes);
                //Division to training and testing sets
                int numOfTrainingSamples = predictorsCollection.Count - numOfTestSamples;
                List<double[]> trainingPredictorsCollection = new List<double[]>(numOfTrainingSamples);
                List<double[]> trainingOutputsCollection = new List<double[]>(numOfTrainingSamples);
                List<double[]> testingPredictorsCollection = new List<double[]>(numOfTestSamples);
                List<double[]> testingOutputsCollection = new List<double[]>(numOfTestSamples);
                for (int i = 0; i < predictorsCollection.Count; i++)
                {
                    if (!testSampleIndexCollection.Contains(i))
                    {
                        //Training sample
                        trainingPredictorsCollection.Add(predictorsCollection[i]);
                        trainingOutputsCollection.Add(new double[1]);
                        trainingOutputsCollection[trainingOutputsCollection.Count - 1][0] = idealOutputsCollection[i][outputIdx];
                    }
                    else
                    {
                        //Testing sample
                        testingPredictorsCollection.Add(predictorsCollection[i]);
                        testingOutputsCollection.Add(new double[1]);
                        testingOutputsCollection[testingOutputsCollection.Count - 1][0] = idealOutputsCollection[i][outputIdx];
                    }
                }
                //Readout unit regression
                readoutUnits[outputIdx] = UnitRegression(outputIdx,
                                                         outputFieldNameCollection[outputIdx],
                                                         trainingPredictorsCollection,
                                                         trainingOutputsCollection,
                                                         testingPredictorsCollection,
                                                         testingOutputsCollection,
                                                         rand,
                                                         hiddenLayerCollection,
                                                         outputNeuronActivation,
                                                         regressionMethod,
                                                         regrAttempts,
                                                         attemptEpochs,
                                                         stopMSE,
                                                         regressionController,
                                                         regressionControllerData
                                                         );
            }
            return readoutUnits;
        }

        /// <summary>
        /// Prepares the readout unit for specified output field.
        /// </summary>
        /// <param name="readoutUnitIdx">Index of the readout unit</param>
        /// <param name="outputFieldName">Name of the corresponding output field</param>
        /// <param name="trainingPredictorsCollection">Collection of the predictors for training</param>
        /// <param name="trainingIdealOutputsCollection">Collection of ideal outputs for training</param>
        /// <param name="testingPredictorsCollection">Collection of the predictors for testing</param>
        /// <param name="testingIdealOutputsCollection">Collection of ideal outputs for testing</param>
        /// <param name="rand">Random object to be used</param>
        /// <param name="hiddenLayerCollection">Collection of output FF network hidden layer settings</param>
        /// <param name="outputNeuronActivation">Activation type of the FF output layer neuron</param>
        /// <param name="regressionMethod">Regression method to be used</param>
        /// <param name="regrAttempts">Number of regression attempts</param>
        /// <param name="attemptEpochs">Number of epochs for each regression attempt</param>
        /// <param name="stopMSE">The achieved training MSE when to terminate regression attempt</param>
        /// <param name="controller">Regression controller delegate</param>
        /// <param name="controllerUserObject">An user object to be passed to controller</param>
        /// <returns>Prepared readout unit</returns>
        private static ReadoutUnit UnitRegression(int readoutUnitIdx,
                                                  string outputFieldName,
                                                  List<double[]> trainingPredictorsCollection,
                                                  List<double[]> trainingIdealOutputsCollection,
                                                  List<double[]> testingPredictorsCollection,
                                                  List<double[]> testingIdealOutputsCollection,
                                                  Random rand,
                                                  List<HiddenLayerSettings> hiddenLayerCollection,
                                                  ActivationFactory.ActivationType outputNeuronActivation,
                                                  TrainingMethodType regressionMethod,
                                                  int regrAttempts,
                                                  int attemptEpochs,
                                                  double stopMSE,
                                                  RegressionCallbackDelegate controller = null,
                                                  Object controllerUserObject = null
                                                  )
        {
            ReadoutUnit bestReadoutUnit = new ReadoutUnit();
            //Regression attempts
            bool stopRegression = false;
            for (int regrAttemptNumber = 1; regrAttemptNumber <= regrAttempts; regrAttemptNumber++)
            {
                //Create FF network
                FeedForwardNetwork ffn = new FeedForwardNetwork(trainingPredictorsCollection[0].Length, 1);
                for (int i = 0; i < hiddenLayerCollection.Count; i++)
                {
                    ffn.AddLayer(hiddenLayerCollection[i].NumOfNeurons,
                                 ActivationFactory.CreateActivationFunction(hiddenLayerCollection[i].ActivationType)
                                 );
                }
                ffn.FinalizeStructure(ActivationFactory.CreateActivationFunction(outputNeuronActivation));
                //Reset feed forward network's weights to random values
                ffn.RandomizeWeights(rand);
                //Create trainer object
                IFeedForwardNetworkTrainer trainer = null;
                switch (regressionMethod)
                {
                    case TrainingMethodType.Linear:
                        trainer = new LinRegrTrainer(ffn, trainingPredictorsCollection, trainingIdealOutputsCollection, attemptEpochs, rand);
                        break;
                    case TrainingMethodType.Resilient:
                        trainer = new RPropTrainer(ffn, trainingPredictorsCollection, trainingIdealOutputsCollection);
                        break;
                    default:
                        throw new ArgumentException($"Not supported regression method {regressionMethod}");
                }
                //Iterate training cycles
                for (int epoch = 1; epoch <= attemptEpochs; epoch++)
                {
                    trainer.Iteration();
                    List<double[]> trainingComputedOutputsCollection = null;
                    List<double[]> testingComputedOutputsCollection = null;
                    //Compute current error statistics after training iteration
                    ReadoutUnit currReadoutUnit = new ReadoutUnit();
                    currReadoutUnit.OutputFieldName = outputFieldName;
                    currReadoutUnit.FFNet = ffn;
                    currReadoutUnit.TrainingErrorStat = ffn.ComputeBatchErrorStat(trainingPredictorsCollection, trainingIdealOutputsCollection, out trainingComputedOutputsCollection);
                    currReadoutUnit.TrainingBinErrorStat = new BinErrStat(ffn.LayerCollection[ffn.LayerCollection.Count - 1].Activation.Range.Mid, trainingComputedOutputsCollection, trainingIdealOutputsCollection);
                    currReadoutUnit.CombinedError = currReadoutUnit.TrainingErrorStat.ArithAvg;
                    if (testingPredictorsCollection != null && testingPredictorsCollection.Count > 0)
                    {
                        currReadoutUnit.TestingErrorStat = ffn.ComputeBatchErrorStat(testingPredictorsCollection, testingIdealOutputsCollection, out testingComputedOutputsCollection);
                        currReadoutUnit.TestingBinErrorStat = new BinErrStat(ffn.LayerCollection[ffn.LayerCollection.Count - 1].Activation.Range.Mid, testingComputedOutputsCollection, testingIdealOutputsCollection);
                        currReadoutUnit.CombinedError = Math.Max(currReadoutUnit.CombinedError, currReadoutUnit.TestingErrorStat.ArithAvg);
                    }
                    //Current results processing
                    bool best = false, stopTrainingCycle = false;
                    //Result first initialization
                    if (bestReadoutUnit.CombinedError == -1)
                    {
                        //Adopt current regression results
                        bestReadoutUnit = currReadoutUnit.DeepClone();
                    }
                    //Perform call back if it is defined
                    RegressionControlOutArgs cbOut = null;
                    if (controller != null)
                    {
                        //Evaluation of the improvement is driven externaly
                        RegressionControlInArgs cbIn = new RegressionControlInArgs();
                        cbIn.ReadoutUnitIdx = readoutUnitIdx;
                        cbIn.OutputFieldName = outputFieldName;
                        cbIn.RegrAttemptNumber = regrAttemptNumber;
                        cbIn.RegrMaxAttempts = regrAttempts;
                        cbIn.Epoch = epoch;
                        cbIn.MaxEpochs = attemptEpochs;
                        cbIn.TrainingPredictorsCollection = trainingPredictorsCollection;
                        cbIn.TrainingIdealOutputsCollection = trainingIdealOutputsCollection;
                        cbIn.TrainingComputedOutputsCollection = trainingComputedOutputsCollection;
                        cbIn.TestingPredictorsCollection = testingPredictorsCollection;
                        cbIn.TestingIdealOutputsCollection = testingIdealOutputsCollection;
                        cbIn.TestingComputedOutputsCollection = testingComputedOutputsCollection;
                        cbIn.CurrReadoutUnit = currReadoutUnit;
                        cbIn.BestReadoutUnit = bestReadoutUnit;
                        cbIn.UserObject = controllerUserObject;
                        cbOut = controller(cbIn);
                        best = cbOut.Best;
                        stopTrainingCycle = cbOut.StopCurrentAttempt;
                        stopRegression = cbOut.StopRegression;
                    }
                    else
                    {
                        //Default implementation
                        if (currReadoutUnit.CombinedError < bestReadoutUnit.CombinedError)
                        {
                            best = true;
                        }
                    }
                    //Best?
                    if (best)
                    {
                        //Adopt current regression results
                        bestReadoutUnit = currReadoutUnit.DeepClone();
                    }
                    //Training stop conditions
                    if (currReadoutUnit.TrainingErrorStat.RootMeanSquare <= stopMSE ||
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
            bestReadoutUnit.OutputWeightsStat = bestReadoutUnit.FFNet.ComputeWeightsStat();
            return bestReadoutUnit;
        }


        //Inner classes
        /// <summary>
        /// The class contains information needed to control the progress of the regression process.
        /// </summary>
        [Serializable]
        public class RegressionControlInArgs
        {
            //Attribute properties
            /// <summary>
            /// Readout unit index for which the regression is performing (corresponds with output field index)
            /// </summary>
            public int ReadoutUnitIdx { get; set; } = 0;
            /// <summary>
            /// Output field name for which the regression is performing
            /// </summary>
            public string OutputFieldName;
            /// <summary>
            /// Current regression attempt number 
            /// </summary>
            public int RegrAttemptNumber { get; set; } = -1;
            /// <summary>
            /// Maximum number of regression attempts
            /// </summary>
            public int RegrMaxAttempts { get; set; } = -1;
            /// <summary>
            /// Current epoch number
            /// </summary>
            public int Epoch { get; set; } = -1;
            /// <summary>
            /// Maximum nuber of epochs
            /// </summary>
            public int MaxEpochs { get; set; } = -1;
            /// <summary>
            /// Predictors collection for training
            /// </summary>
            public List<double[]> TrainingPredictorsCollection { get; set; } = null;
            /// <summary>
            /// Desired outputs collection for training
            /// </summary>
            public List<double[]> TrainingIdealOutputsCollection { get; set; } = null;
            /// <summary>
            /// Computed outputs collection for training
            /// </summary>
            public List<double[]> TrainingComputedOutputsCollection { get; set; } = null;
            /// <summary>
            /// Predictors collection for testing
            /// </summary>
            public List<double[]> TestingPredictorsCollection { get; set; } = null;
            /// <summary>
            /// Desired outputs collection for testing
            /// </summary>
            public List<double[]> TestingIdealOutputsCollection { get; set; } = null;
            /// <summary>
            /// Computed outputs collection for testing
            /// </summary>
            public List<double[]> TestingComputedOutputsCollection { get; set; } = null;
            /// <summary>
            /// Contains the current feed forward network and related
            /// important error statistics.
            /// </summary>
            public ReadoutUnit CurrReadoutUnit { get; set; } = null;
            /// <summary>
            /// Contains the best feed forward network for now and related
            /// important error statistics.
            /// </summary>
            public ReadoutUnit BestReadoutUnit { get; set; } = null;
            /// <summary>
            /// An user object
            /// </summary>
            public Object UserObject { get; set; } = null;
        }//RegressionControlInArgs

        /// <summary>
        /// Contains instructions for the regression process
        /// </summary>
        [Serializable]
        public class RegressionControlOutArgs
        {
            //Attribute properties
            /// <summary>
            /// Indicates whether to end the current regression attempt
            /// </summary>
            public bool StopCurrentAttempt { get; set; } = false;
            /// <summary>
            /// Indicates whether to end the entire regression process for the current output field
            /// </summary>
            public bool StopRegression { get; set; } = false;
            /// <summary>
            /// This is the most important switch indicating whether the RegrCurrResult is better than
            /// the existing RegrBestResult
            /// </summary>
            public bool Best { get; set; } = false;
        }//RegressionControlOutArgs


    }//ReadoutUnitRegression

}//Namespace
