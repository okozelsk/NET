using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;
using RCNet.Neural.Activation;
using RCNet.Neural.Network.FF;

namespace RCNet.Neural.Network.ReservoirComputing.Readout
{
    /// <summary>
    /// Contains the trained feed forward network associated with output field and related
    /// important error statistics.
    /// </summary>
    [Serializable]
    public class ReadoutUnit
    {
        //Delegates
        /// <summary>
        /// This is the control function of the regression process and is called
        /// after the completion of each regression training epoch for the readout unit.
        /// The goal of the regression process is to train a feed forward network
        /// that will give good results both on the training data and the test data.
        /// RegressionControlInArgs object passed to the callback function contains the best error statistics so far
        /// and the latest statistics. The primary purpose of this function is to decide whether the latest statistics
        /// are better than the best statistics so far.
        /// The function can also tell the regression process that it does not make any sense to continue the regression.
        /// It can terminate the current regression attempt or whole regression process.
        /// </summary>
        /// <param name="inArgs">Contains all the necessary information to control the progress of the regression.</param>
        /// <returns>Instructions for the regression process.</returns>
        public delegate RegressionControlOutArgs RegressionCallbackDelegate(RegressionControlInArgs inArgs);

        //Attribute properties
        /// <summary>
        /// Trained feed forward network
        /// </summary>
        public FeedForwardNetwork FFNet { get; set; }
        /// <summary>
        /// Training error statistics
        /// </summary>
        public BasicStat TrainingErrorStat { get; set; }
        /// <summary>
        /// Training binary error statistics
        /// </summary>
        public BinErrStat TrainingBinErrorStat { get; set; }
        /// <summary>
        /// Testing error statistics
        /// </summary>
        public BasicStat TestingErrorStat { get; set; }
        /// <summary>
        /// Testing binary error statistics
        /// </summary>
        public BinErrStat TestingBinErrorStat { get; set; }
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
        /// Achieved combined binary score.
        /// Formula for combined binary score calculation is Min(training bin error stat.Score, testing bin error stat.Score)
        /// </summary>
        public double CombinedBinScore { get; set; }

        //Constructor
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        public ReadoutUnit()
        {
            FFNet = null;
            TrainingErrorStat = null;
            TrainingBinErrorStat = null;
            TestingErrorStat = null;
            TestingBinErrorStat = null;
            OutputWeightsStat = null;
            CombinedError = -1;
            CombinedBinScore = -1;
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public ReadoutUnit(ReadoutUnit source)
        {
            FFNet = null;
            if (source.FFNet != null)
            {
                FFNet = source.FFNet.Clone();
            }
            TrainingErrorStat = null;
            if (source.TrainingErrorStat != null)
            {
                TrainingErrorStat = new BasicStat(source.TrainingErrorStat);
            }
            TrainingBinErrorStat = null;
            if (source.TrainingBinErrorStat != null)
            {
                TrainingBinErrorStat = new BinErrStat(source.TrainingBinErrorStat);
            }
            TestingErrorStat = null;
            if (source.TestingErrorStat != null)
            {
                TestingErrorStat = new BasicStat(source.TestingErrorStat);
            }
            TestingBinErrorStat = null;
            if (source.TestingBinErrorStat != null)
            {
                TestingBinErrorStat = new BinErrStat(source.TestingBinErrorStat);
            }
            OutputWeightsStat = null;
            if (source.OutputWeightsStat != null)
            {
                OutputWeightsStat = new BasicStat(source.OutputWeightsStat);
            }
            CombinedError = source.CombinedError;
            CombinedBinScore = source.CombinedBinScore;
            return;
        }

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public ReadoutUnit DeepClone()
        {
            return new ReadoutUnit(this);
        }

        /// <summary>
        /// The default implementation of a judgement if the current readout unit is better than for now the best readout unit
        /// </summary>
        /// <param name="taskType">Type of the task</param>
        /// <param name="current">Current readout unit</param>
        /// <param name="best">For now the best readout unit</param>
        public static bool IsBetter(CommonTypes.TaskType taskType, ReadoutUnit current, ReadoutUnit best)
        {
            switch(taskType)
            {
                case CommonTypes.TaskType.Prediction:
                    return (current.CombinedError < best.CombinedError);
                case CommonTypes.TaskType.Classification:
                    if(current.CombinedBinScore > best.CombinedBinScore)
                    {
                        return true;
                    }
                    else if(current.CombinedBinScore == best.CombinedBinScore && current.CombinedError < best.CombinedError)
                    {
                        return true;
                    }
                    break;
                default:
                    break;
            }
            return false;
        }

        /// <summary>
        /// Prepares trained readout unit for specified output field and task.
        /// </summary>
        /// <param name="taskType">Type of the task</param>
        /// <param name="readoutUnitIdx">Index of the readout unit (informative only)</param>
        /// <param name="outputFieldName">Name of the corresponding output field (informative only)</param>
        /// <param name="foldNum">Current fold number</param>
        /// <param name="numOfFolds">Total number of the folds</param>
        /// <param name="refBinDistr">Reference bin distribution (if task type is Classification)</param>
        /// <param name="trainingPredictorsCollection">Collection of the predictors for training</param>
        /// <param name="trainingIdealOutputsCollection">Collection of ideal outputs for training. Note that the double array always has only one member.</param>
        /// <param name="testingPredictorsCollection">Collection of the predictors for testing</param>
        /// <param name="testingIdealOutputsCollection">Collection of ideal outputs for testing. Note that the double array always has only one member.</param>
        /// <param name="rand">Random object to be used</param>
        /// <param name="hiddenLayerCollection">Collection of output FF network hidden layer settings</param>
        /// <param name="outputNeuronActivation">Activation type of the FF output layer neuron</param>
        /// <param name="regressionMethod">Regression method to be used</param>
        /// <param name="regrAttempts">Number of regression attempts</param>
        /// <param name="attemptEpochs">Number of epochs for each regression attempt</param>
        /// <param name="stopMSE">The achieved training MSE when to terminate regression attempt</param>
        /// <param name="controller">Regression controller</param>
        /// <param name="controllerUserObject">An user object to be passed to controller</param>
        /// <returns>Prepared readout unit</returns>
        public static ReadoutUnit CreateTrained(CommonTypes.TaskType taskType,
                                                int readoutUnitIdx,
                                                string outputFieldName,
                                                int foldNum,
                                                int numOfFolds,
                                                BinDistribution refBinDistr,
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
                //Reference binary distribution
                //Iterate training cycles
                for (int epoch = 1; epoch <= attemptEpochs; epoch++)
                {
                    trainer.Iteration();
                    List<double[]> trainingComputedOutputsCollection = null;
                    List<double[]> testingComputedOutputsCollection = null;
                    //Compute current error statistics after training iteration
                    ReadoutUnit currReadoutUnit = new ReadoutUnit();
                    currReadoutUnit.FFNet = ffn;
                    currReadoutUnit.TrainingErrorStat = ffn.ComputeBatchErrorStat(trainingPredictorsCollection, trainingIdealOutputsCollection, out trainingComputedOutputsCollection);
                    if(taskType == CommonTypes.TaskType.Classification)
                    {
                        currReadoutUnit.TrainingBinErrorStat = new BinErrStat(refBinDistr, trainingComputedOutputsCollection, trainingIdealOutputsCollection);
                        currReadoutUnit.CombinedBinScore = currReadoutUnit.TrainingBinErrorStat.Score;
                    }
                    currReadoutUnit.CombinedError = currReadoutUnit.TrainingErrorStat.ArithAvg;
                    if (testingPredictorsCollection != null && testingPredictorsCollection.Count > 0)
                    {
                        currReadoutUnit.TestingErrorStat = ffn.ComputeBatchErrorStat(testingPredictorsCollection, testingIdealOutputsCollection, out testingComputedOutputsCollection);
                        currReadoutUnit.CombinedError = Math.Max(currReadoutUnit.CombinedError, currReadoutUnit.TestingErrorStat.ArithAvg);
                        if (taskType == CommonTypes.TaskType.Classification)
                        {
                            currReadoutUnit.TestingBinErrorStat = new BinErrStat(refBinDistr, testingComputedOutputsCollection, testingIdealOutputsCollection);
                            currReadoutUnit.CombinedBinScore = Math.Min(currReadoutUnit.CombinedBinScore, currReadoutUnit.TestingBinErrorStat.Score);
                        }
                    }
                    //Current results processing
                    bool better = false, stopTrainingCycle = false;
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
                        cbIn.TaskType = taskType;
                        cbIn.ReadoutUnitIdx = readoutUnitIdx;
                        cbIn.OutputFieldName = outputFieldName;
                        cbIn.FoldNum = foldNum;
                        cbIn.NumOfFolds = numOfFolds;
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
                        better = cbOut.CurrentIsBetter;
                        stopTrainingCycle = cbOut.StopCurrentAttempt;
                        stopRegression = cbOut.StopRegression;
                    }
                    else
                    {
                        //Default implementation
                        better = IsBetter(taskType, currReadoutUnit, bestReadoutUnit);
                    }
                    //Best?
                    if (better)
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
            /// Type of the neural task
            /// </summary>
            public CommonTypes.TaskType TaskType { get; set; } = CommonTypes.TaskType.Prediction;
            /// <summary>
            /// Readout unit index for which the regression is performing (corresponds with output field index)
            /// </summary>
            public int ReadoutUnitIdx { get; set; } = 0;
            /// <summary>
            /// Current fold number
            /// </summary>
            public int FoldNum { get; set; } = 0;
            /// <summary>
            /// Total number of the folds
            /// </summary>
            public int NumOfFolds { get; set; } = 0;
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
            /// This is the most important switch indicating whether the CurrReadoutUnit is better than
            /// the existing BestReadoutUnit
            /// </summary>
            public bool CurrentIsBetter { get; set; } = false;
        }//RegressionControlOutArgs






    }//ReadoutUnit
}//Namespace
