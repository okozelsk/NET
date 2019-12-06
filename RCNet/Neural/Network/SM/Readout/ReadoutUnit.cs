using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;
using RCNet.Extensions;
using RCNet.Neural.Network.FF;
using RCNet.Neural.Network.PP;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Contains the trained unit associated with output field and related
    /// important error statistics.
    /// </summary>
    [Serializable]
    public class ReadoutUnit
    {
        //Enums
        /// <summary>
        /// Supported task types.
        /// </summary>
        public enum TaskType
        {
            /// <summary>
            /// Forecasting of the next value
            /// </summary>
            Forecast,
            /// <summary>
            /// Classification (Categorization, Pattern recognition) task type
            /// </summary>
            Classification
        }

        //Delegates
        /// <summary>
        /// This is the control function of the regression process and is called
        /// after the completion of each regression training epoch for the readout unit.
        /// The goal of the regression process is to train a unit
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
        /// Trained network
        /// </summary>
        public INonRecurrentNetwork Network { get; set; }
        /// <summary>
        /// Informative message from trainer
        /// </summary>
        public string TrainerInfoMessage { get; set; }
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
        /// Statistics of the network weights
        /// </summary>
        public BasicStat OutputWeightsStat { get; set; }
        /// <summary>
        /// Achieved combined precision error.
        /// </summary>
        public double CombinedPrecisionError { get; set; }
        /// <summary>
        /// Achieved combined binary error.
        /// </summary>
        public double CombinedBinaryError { get; set; }

        //Constructor
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        public ReadoutUnit()
        {
            Network = null;
            TrainingErrorStat = null;
            TrainingBinErrorStat = null;
            TestingErrorStat = null;
            TestingBinErrorStat = null;
            OutputWeightsStat = null;
            CombinedPrecisionError = -1;
            CombinedBinaryError = -1;
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public ReadoutUnit(ReadoutUnit source)
        {
            Network = null;
            if (source.Network != null)
            {
                Network = source.Network.DeepClone();
            }
            TrainerInfoMessage = source.TrainerInfoMessage;
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
            CombinedPrecisionError = source.CombinedPrecisionError;
            CombinedBinaryError = source.CombinedBinaryError;
            return;
        }

        //Methods
        /// <summary>
        /// Parses the task type from a string code
        /// </summary>
        /// <param name="code">Task type code</param>
        public static TaskType ParseTaskType(string code)
        {
            switch (code.ToUpper())
            {
                case "FORECAST": return TaskType.Forecast;
                case "CLASSIFICATION": return TaskType.Classification;
                default:
                    throw new ArgumentException($"Unsupported task type {code}", "code");
            }
        }

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
        public static bool IsBetter(TaskType taskType, ReadoutUnit current, ReadoutUnit best)
        {
            switch(taskType)
            {
                case TaskType.Classification:
                    if(current.CombinedBinaryError < best.CombinedBinaryError)
                    {
                        return true;
                    }
                    else if(current.CombinedBinaryError == best.CombinedBinaryError)
                    {
                        if(current.TestingBinErrorStat.TotalErrStat.Sum < best.TestingBinErrorStat.TotalErrStat.Sum)
                        {
                            return true;
                        }
                        else if (current.TrainingBinErrorStat.TotalErrStat.Sum < best.TrainingBinErrorStat.TotalErrStat.Sum)
                        {
                            return true;
                        }
                        else if (current.CombinedPrecisionError < best.CombinedPrecisionError)
                        {
                            return true;
                        }
                    }
                    return false;
                default:
                    //Forecast task type
                    return (current.CombinedPrecisionError < best.CombinedPrecisionError);
            }
        }

        private static void CreateNetAndTreainer(ReadoutLayerSettings.ReadoutUnitSettings settings,
                                                 List<double[]> trainingPredictorsCollection,
                                                 List<double[]> trainingIdealOutputsCollection,
                                                 Random rand,
                                                 out INonRecurrentNetwork net,
                                                 out INonRecurrentNetworkTrainer trainer
                                                 )
        {
            if(settings.NetType == ReadoutLayerSettings.ReadoutUnitSettings.ReadoutUnitNetworkType.FF)
            {
                FeedForwardNetworkSettings netCfg = (FeedForwardNetworkSettings)settings.NetSettings;
                FeedForwardNetwork ffn = new FeedForwardNetwork(trainingPredictorsCollection[0].Length, 1, netCfg);
                net = ffn;
                if(netCfg.TrainerCfg.GetType() == typeof(QRDRegrTrainerSettings))
                {
                    trainer = new QRDRegrTrainer(ffn, trainingPredictorsCollection, trainingIdealOutputsCollection, (QRDRegrTrainerSettings)netCfg.TrainerCfg, rand);
                }
                else if (netCfg.TrainerCfg.GetType() == typeof(RidgeRegrTrainerSettings))
                {
                    trainer = new RidgeRegrTrainer(ffn, trainingPredictorsCollection, trainingIdealOutputsCollection, (RidgeRegrTrainerSettings)netCfg.TrainerCfg, rand);
                }
                else if(netCfg.TrainerCfg.GetType() == typeof(ElasticRegrTrainerSettings))
                {
                    trainer = new ElasticRegrTrainer(ffn, trainingPredictorsCollection, trainingIdealOutputsCollection, (ElasticRegrTrainerSettings)netCfg.TrainerCfg);
                }
                else if (netCfg.TrainerCfg.GetType() == typeof(RPropTrainerSettings))
                {
                    trainer = new RPropTrainer(ffn, trainingPredictorsCollection, trainingIdealOutputsCollection, (RPropTrainerSettings)netCfg.TrainerCfg, rand);
                }
                else
                {
                    throw new ArgumentException($"Unknown trainer {netCfg.TrainerCfg}");
                }
            }
            else
            {
                ParallelPerceptronSettings netCfg = (ParallelPerceptronSettings)settings.NetSettings;
                ParallelPerceptron ppn = new ParallelPerceptron(trainingPredictorsCollection[0].Length, netCfg);
                net = ppn;
                trainer = new PDeltaRuleTrainer(ppn, trainingPredictorsCollection, trainingIdealOutputsCollection, netCfg.PDeltaRuleTrainerCfg, rand);
            }
            net.RandomizeWeights(rand);
            return;
        }

        /// <summary>
        /// Prepares trained readout unit for specified output field and task.
        /// </summary>
        /// <param name="taskType">Type of the task</param>
        /// <param name="readoutUnitIdx">Index of the readout unit (informative only)</param>
        /// <param name="foldNum">Current fold number</param>
        /// <param name="numOfFolds">Total number of the folds</param>
        /// <param name="binBorder">Binary 0/1 border. Double value LT this border is considered as 0 and GE as 1.
        /// (relevant only if task type is Classification)
        /// </param>
        /// <param name="trainingPredictorsCollection">Collection of the predictors for training</param>
        /// <param name="trainingIdealOutputsCollection">Collection of ideal outputs for training. Note that the double array always has only one member.</param>
        /// <param name="testingPredictorsCollection">Collection of the predictors for testing</param>
        /// <param name="testingIdealOutputsCollection">Collection of ideal outputs for testing. Note that the double array always has only one member.</param>
        /// <param name="rand">Random object to be used</param>
        /// <param name="readoutUnitSettings">Readout unit configuration parameters</param>
        /// <param name="controller">Regression controller</param>
        /// <param name="controllerUserObject">An user object to be passed to controller</param>
        /// <returns>Prepared readout unit</returns>
        public static ReadoutUnit CreateTrained(TaskType taskType,
                                                int readoutUnitIdx,
                                                int foldNum,
                                                int numOfFolds,
                                                double binBorder,
                                                List<double[]> trainingPredictorsCollection,
                                                List<double[]> trainingIdealOutputsCollection,
                                                List<double[]> testingPredictorsCollection,
                                                List<double[]> testingIdealOutputsCollection,
                                                Random rand,
                                                ReadoutLayerSettings.ReadoutUnitSettings readoutUnitSettings,
                                                RegressionCallbackDelegate controller = null,
                                                Object controllerUserObject = null
                                                )
        {
            ReadoutUnit bestReadoutUnit = null;
            //Regression attempts
            bool stopRegression = false;
            //Create network and trainer
            CreateNetAndTreainer(readoutUnitSettings,
                                 trainingPredictorsCollection,
                                 trainingIdealOutputsCollection,
                                 rand,
                                 out INonRecurrentNetwork net,
                                 out INonRecurrentNetworkTrainer trainer
                                 );
            //Iterate training cycles
            while (trainer.Iteration())
            {
                List<double[]> testingComputedOutputsCollection = null;
                //Compute current error statistics after training iteration
                ReadoutUnit currReadoutUnit = new ReadoutUnit
                {
                    Network = net,
                    TrainerInfoMessage = trainer.InfoMessage,
                    TrainingErrorStat = net.ComputeBatchErrorStat(trainingPredictorsCollection, trainingIdealOutputsCollection, out List<double[]> trainingComputedOutputsCollection)
                };
                if (taskType == TaskType.Classification)
                {
                    currReadoutUnit.TrainingBinErrorStat = new BinErrStat(binBorder, trainingComputedOutputsCollection, trainingIdealOutputsCollection);
                    currReadoutUnit.CombinedBinaryError = currReadoutUnit.TrainingBinErrorStat.TotalErrStat.Sum;
                }
                currReadoutUnit.CombinedPrecisionError = currReadoutUnit.TrainingErrorStat.ArithAvg;
                if (testingPredictorsCollection != null && testingPredictorsCollection.Count > 0)
                {
                    currReadoutUnit.TestingErrorStat = net.ComputeBatchErrorStat(testingPredictorsCollection, testingIdealOutputsCollection, out testingComputedOutputsCollection);
                    currReadoutUnit.CombinedPrecisionError = Math.Max(currReadoutUnit.CombinedPrecisionError, currReadoutUnit.TestingErrorStat.ArithAvg);
                    if (taskType == TaskType.Classification)
                    {
                        currReadoutUnit.TestingBinErrorStat = new BinErrStat(binBorder, testingComputedOutputsCollection, testingIdealOutputsCollection);
                        currReadoutUnit.CombinedBinaryError = Math.Max(currReadoutUnit.CombinedBinaryError, currReadoutUnit.TestingBinErrorStat.TotalErrStat.Sum);
                    }
                }
                //Current results processing
                bool better = false, stopAttempt = false;
                //Result first initialization
                if (bestReadoutUnit == null)
                {
                    //Adopt current regression results
                    bestReadoutUnit = currReadoutUnit.DeepClone();
                }
                //Perform call back if it is defined
                if (controller != null)
                {
                    //Evaluation of the improvement is driven externally
                    RegressionControlInArgs cbIn = new RegressionControlInArgs
                    {
                        TaskType = taskType,
                        ReadoutUnitIdx = readoutUnitIdx,
                        OutputFieldName = readoutUnitSettings.Name,
                        FoldNum = foldNum,
                        NumOfFolds = numOfFolds,
                        RegrAttemptNumber = trainer.Attempt,
                        RegrMaxAttempts = trainer.MaxAttempt,
                        Epoch = trainer.AttemptEpoch,
                        MaxEpochs = trainer.MaxAttemptEpoch,
                        TrainingPredictorsCollection = trainingPredictorsCollection,
                        TrainingIdealOutputsCollection = trainingIdealOutputsCollection,
                        TrainingComputedOutputsCollection = trainingComputedOutputsCollection,
                        TestingPredictorsCollection = testingPredictorsCollection,
                        TestingIdealOutputsCollection = testingIdealOutputsCollection,
                        TestingComputedOutputsCollection = testingComputedOutputsCollection,
                        CurrReadoutUnit = currReadoutUnit,
                        BestReadoutUnit = bestReadoutUnit,
                        UserObject = controllerUserObject
                    };
                    //Call external controller
                    RegressionControlOutArgs cbOut = controller(cbIn);
                    //Pick up results
                    better = cbOut.CurrentIsBetter;
                    stopAttempt = cbOut.StopCurrentAttempt;
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
                //Process instructions
                if (stopRegression)
                {
                    break;
                }
                else if(stopAttempt)
                {
                    if(!trainer.NextAttempt())
                    {
                        break;
                    }
                }
            }//while (iteration)
            //Create statistics of the best network weights
            bestReadoutUnit.OutputWeightsStat = bestReadoutUnit.Network.ComputeWeightsStat();
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
            public TaskType TaskType { get; set; } = TaskType.Forecast;
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
            /// Contains the current network and related
            /// important error statistics.
            /// </summary>
            public ReadoutUnit CurrReadoutUnit { get; set; } = null;
            /// <summary>
            /// Contains the best network for now and related
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
