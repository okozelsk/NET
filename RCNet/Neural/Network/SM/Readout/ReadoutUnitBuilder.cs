using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;
using RCNet.Extensions;
using RCNet.Neural.Network.FF;
using RCNet.Neural.Network.PP;
using RCNet.Neural.Data;
using System.Globalization;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Builds trained readout unit
    /// </summary>
    public class ReadoutUnitBuilder
    {
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
        /// It can terminate the current regression attempt or whole regression process of the readout unit.
        /// </summary>
        /// <param name="regrState">Contains all the necessary information to control the regression.</param>
        /// <returns>Instructions for the regression process.</returns>
        public delegate RegrInstr RegressionControllerDelegate(RegrState regrState);

        /// <summary>
        /// Delegate of RegressionEpochDone event handler.
        /// </summary>
        /// <param name="regrState">Current state of the regression process</param>
        /// <param name="bestUnitChanged">Indicates that the best readout unit was changed as a result of the performed epoch</param>
        public delegate void RegressionEpochDoneHandler(RegrState regrState, bool bestUnitChanged);

        /// <summary>
        /// This informative event occurs every time the regression epoch is done
        /// </summary>
        public event RegressionEpochDoneHandler RegressionEpochDone;

        //Constants

        //Attributes
        private readonly ReadoutLayerSettings.ReadoutUnitSettings _readoutUnitSettings;
        private readonly int _foldNum;
        private readonly int _numOfFolds;
        private readonly VectorBundle _trainingBundle;
        private readonly VectorBundle _testingBundle;
        private readonly Random _rand;
        private readonly RegressionControllerDelegate _controller;

        //Constructor
        /// <summary>
        /// Creates an instance ready to build trained ReadoutUnit
        /// </summary>
        /// <param name="readoutUnitConfig">Readout unit configuration parameters</param>
        /// <param name="foldNum">Current fold number</param>
        /// <param name="numOfFolds">Total number of the folds</param>
        /// <param name="trainingBundle">Bundle of predictors and ideal values to be used for training purposes</param>
        /// <param name="testingBundle">Bundle of predictors and ideal values to be used for testing purposes</param>
        /// <param name="rand">Random generator to be used (optional)</param>
        /// <param name="controller">Regression controller (optional)</param>
        public ReadoutUnitBuilder(ReadoutLayerSettings.ReadoutUnitSettings readoutUnitConfig,
                                  int foldNum,
                                  int numOfFolds,
                                  VectorBundle trainingBundle,
                                  VectorBundle testingBundle,
                                  Random rand = null,
                                  RegressionControllerDelegate controller = null
                                  )
        {
            _readoutUnitSettings = readoutUnitConfig;
            _foldNum = foldNum;
            _numOfFolds = numOfFolds;
            _trainingBundle = trainingBundle;
            _testingBundle = testingBundle;
            _rand = rand ?? new Random(0);
            _controller = controller ?? DefaultRegressionController;
            return;
        }

        //Static methods
        /// <summary>
        /// Default implementation of a decision if the tested readout unit is better than currently the best readout unit
        /// </summary>
        /// <param name="taskType">Type of the task</param>
        /// <param name="candidateUnit">Tested readout unit</param>
        /// <param name="currentBestUnit">For now the best readout unit</param>
        public static bool IsBetter(ReadoutUnit.TaskType taskType, ReadoutUnit candidateUnit, ReadoutUnit currentBestUnit)
        {
            switch (taskType)
            {
                case ReadoutUnit.TaskType.Classification:
                    if (candidateUnit.CombinedBinaryError < currentBestUnit.CombinedBinaryError ||
                        (candidateUnit.CombinedBinaryError == currentBestUnit.CombinedBinaryError &&
                         candidateUnit.CombinedPrecisionError < currentBestUnit.CombinedPrecisionError)
                       )
                    {
                        return true;
                    }
                    return false;
                default:
                    //Forecast task type
                    return (candidateUnit.CombinedPrecisionError < currentBestUnit.CombinedPrecisionError);
            }
        }

        //Methods
        private RegrInstr DefaultRegressionController(RegrState regrState)
        {
            RegrInstr instructions = new RegrInstr
            {
                CurrentIsBetter = IsBetter(regrState.ReadoutUnitConfig.TaskType,
                                           regrState.CurrReadoutUnit,
                                           regrState.BestReadoutUnit
                                           ),
                /*
                StopReadoutUnitRegression = (regrState.ReadoutUnitConfig.TaskType == ReadoutUnit.TaskType.Classification &&
                                             regrState.CurrReadoutUnit.TrainingBinErrorStat.TotalErrStat.Sum == 0 &&
                                             regrState.CurrReadoutUnit.TestingBinErrorStat.TotalErrStat.Sum == 0
                                             )
                                             */
                StopReadoutUnitRegression = (regrState.ReadoutUnitConfig.TaskType == ReadoutUnit.TaskType.Classification &&
                                             regrState.BestReadoutUnit.TrainingBinErrorStat.TotalErrStat.Sum == 0 &&
                                             regrState.BestReadoutUnit.TestingBinErrorStat.TotalErrStat.Sum == 0 &&
                                             regrState.CurrReadoutUnit.CombinedPrecisionError > regrState.BestReadoutUnit.CombinedPrecisionError
                                             )
            };
            return instructions;
        }

        /// <summary>
        /// Creates new network and associated trainer.
        /// </summary>
        /// <param name="net">Created network</param>
        /// <param name="trainer">Created associated trainer</param>
        private void NewNetworkAndTrainer(out INonRecurrentNetwork net, out INonRecurrentNetworkTrainer trainer)
        {
            if (_readoutUnitSettings.NetType == ReadoutLayerSettings.ReadoutUnitSettings.ReadoutUnitNetworkType.FF)
            {
                //Feed forward network
                FeedForwardNetworkSettings netCfg = (FeedForwardNetworkSettings)_readoutUnitSettings.NetSettings;
                FeedForwardNetwork ffn = new FeedForwardNetwork(_trainingBundle.InputVectorCollection[0].Length, 1, netCfg);
                net = ffn;
                if (netCfg.TrainerCfg.GetType() == typeof(QRDRegrTrainerSettings))
                {
                    trainer = new QRDRegrTrainer(ffn, _trainingBundle.InputVectorCollection, _trainingBundle.OutputVectorCollection, (QRDRegrTrainerSettings)netCfg.TrainerCfg, _rand);
                }
                else if (netCfg.TrainerCfg.GetType() == typeof(RidgeRegrTrainerSettings))
                {
                    trainer = new RidgeRegrTrainer(ffn, _trainingBundle.InputVectorCollection, _trainingBundle.OutputVectorCollection, (RidgeRegrTrainerSettings)netCfg.TrainerCfg, _rand);
                }
                else if (netCfg.TrainerCfg.GetType() == typeof(ElasticRegrTrainerSettings))
                {
                    trainer = new ElasticRegrTrainer(ffn, _trainingBundle.InputVectorCollection, _trainingBundle.OutputVectorCollection, (ElasticRegrTrainerSettings)netCfg.TrainerCfg);
                }
                else if (netCfg.TrainerCfg.GetType() == typeof(RPropTrainerSettings))
                {
                    trainer = new RPropTrainer(ffn, _trainingBundle.InputVectorCollection, _trainingBundle.OutputVectorCollection, (RPropTrainerSettings)netCfg.TrainerCfg, _rand);
                }
                else
                {
                    throw new ArgumentException($"Unknown trainer {netCfg.TrainerCfg}");
                }
            }
            else
            {
                //Parallel perceptron network
                ParallelPerceptronSettings netCfg = (ParallelPerceptronSettings)_readoutUnitSettings.NetSettings;
                ParallelPerceptron ppn = new ParallelPerceptron(_trainingBundle.InputVectorCollection[0].Length, netCfg);
                net = ppn;
                trainer = new PDeltaRuleTrainer(ppn, _trainingBundle.InputVectorCollection, _trainingBundle.OutputVectorCollection, netCfg.PDeltaRuleTrainerCfg, _rand);
            }
            net.RandomizeWeights(_rand);
            return;
        }

        /// <summary>
        /// Builds trained readout unit
        /// </summary>
        /// <returns>Trained readout unit</returns>
        public ReadoutUnit Build()
        {
            ReadoutUnit bestReadoutUnit = null;
            //Create network and trainer
            NewNetworkAndTrainer(out INonRecurrentNetwork net, out INonRecurrentNetworkTrainer trainer);
            //Iterate training cycles
            while (trainer.Iteration())
            {
                //Compute current error statistics after training iteration
                //Training data part
                ReadoutUnit currReadoutUnit = new ReadoutUnit
                {
                    Network = net,
                    TrainerInfoMessage = trainer.InfoMessage,
                    TrainingErrorStat = net.ComputeBatchErrorStat(_trainingBundle.InputVectorCollection, _trainingBundle.OutputVectorCollection, out List<double[]> trainingComputedOutputsCollection)
                };
                if (_readoutUnitSettings.TaskType == ReadoutUnit.TaskType.Classification)
                {
                    currReadoutUnit.TrainingBinErrorStat = new BinErrStat(_readoutUnitSettings.BinBorder, trainingComputedOutputsCollection, _trainingBundle.OutputVectorCollection);
                    currReadoutUnit.CombinedBinaryError = currReadoutUnit.TrainingBinErrorStat.TotalErrStat.Sum;
                }
                currReadoutUnit.CombinedPrecisionError = currReadoutUnit.TrainingErrorStat.ArithAvg;
                //Testing data part
                currReadoutUnit.TestingErrorStat = net.ComputeBatchErrorStat(_testingBundle.InputVectorCollection, _testingBundle.OutputVectorCollection, out List<double[]> testingComputedOutputsCollection);
                currReadoutUnit.CombinedPrecisionError = Math.Max(currReadoutUnit.CombinedPrecisionError, currReadoutUnit.TestingErrorStat.ArithAvg);
                if (_readoutUnitSettings.TaskType == ReadoutUnit.TaskType.Classification)
                {
                    currReadoutUnit.TestingBinErrorStat = new BinErrStat(_readoutUnitSettings.BinBorder, testingComputedOutputsCollection, _testingBundle.OutputVectorCollection);
                    currReadoutUnit.CombinedBinaryError = Math.Max(currReadoutUnit.CombinedBinaryError, currReadoutUnit.TestingBinErrorStat.TotalErrStat.Sum);
                }
                //First initialization of the best readout unit
                bestReadoutUnit = bestReadoutUnit ?? currReadoutUnit.DeepClone();
                //RegrState instance
                RegrState regrState = new RegrState(_readoutUnitSettings, _foldNum, _numOfFolds, trainer.Attempt, trainer.MaxAttempt, trainer.AttemptEpoch, trainer.MaxAttemptEpoch, currReadoutUnit, bestReadoutUnit);
                //Call controller
                RegrInstr instructions = _controller(regrState);
                //Better?
                if (instructions.CurrentIsBetter)
                {
                    //Adopt current regression unit as a best one
                    bestReadoutUnit = currReadoutUnit.DeepClone();
                    regrState.BestReadoutUnit = bestReadoutUnit;
                }
                //Raise notification event
                RegressionEpochDone(regrState, instructions.CurrentIsBetter);
                //Process instructions
                if (instructions.StopReadoutUnitRegression)
                {
                    break;
                }
                else if (instructions.StopCurrentAttempt)
                {
                    if (!trainer.NextAttempt())
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
        /// The class contains information needed to control regression process.
        /// This class is also used for progeress changed event.
        /// </summary>
        [Serializable]
        public class RegrState
        {
            //Attribute properties
            /// <summary>
            /// Configuration of the readout unit
            /// </summary>
            public ReadoutLayerSettings.ReadoutUnitSettings ReadoutUnitConfig { get; }
            /// <summary>
            /// Current fold number
            /// </summary>
            public int FoldNum { get; }
            /// <summary>
            /// Total number of the folds
            /// </summary>
            public int NumOfFolds { get; }
            /// <summary>
            /// Current regression attempt number 
            /// </summary>
            public int RegrAttemptNumber { get; }
            /// <summary>
            /// Maximum number of regression attempts
            /// </summary>
            public int RegrMaxAttempts { get; }
            /// <summary>
            /// Current epoch number
            /// </summary>
            public int Epoch { get; }
            /// <summary>
            /// Maximum nuber of epochs
            /// </summary>
            public int MaxEpochs { get; }
            /// <summary>
            /// Contains current network and related important error statistics.
            /// </summary>
            public ReadoutUnit CurrReadoutUnit { get; }
            /// <summary>
            /// Contains the best network for now and related important error statistics.
            /// </summary>
            public ReadoutUnit BestReadoutUnit { get; set; }

            /// <summary>
            /// Creates an initialized instance
            /// </summary>
            /// <param name="readoutUnitConfig">Configuration of the readout unit</param>
            /// <param name="foldNum">Current fold number</param>
            /// <param name="numOfFolds">Total number of the folds</param>
            /// <param name="regrAttemptNumber">Current regression attempt number</param>
            /// <param name="regrMaxAttempts">Maximum number of regression attempts</param>
            /// <param name="epoch">Current epoch number</param>
            /// <param name="maxEpochs">Maximum nuber of epochs</param>
            /// <param name="currReadoutUnit">Contains current network and related important error statistics.</param>
            /// <param name="bestReadoutUnit">Contains the best network for now and related important error statistics.</param>
            public RegrState(ReadoutLayerSettings.ReadoutUnitSettings readoutUnitConfig,
                             int foldNum,
                             int numOfFolds,
                             int regrAttemptNumber,
                             int regrMaxAttempts,
                             int epoch,
                             int maxEpochs,
                             ReadoutUnit currReadoutUnit,
                             ReadoutUnit bestReadoutUnit
                             )
            {
                ReadoutUnitConfig = readoutUnitConfig;
                FoldNum = foldNum;
                NumOfFolds = numOfFolds;
                RegrAttemptNumber = regrAttemptNumber;
                RegrMaxAttempts = regrMaxAttempts;
                Epoch = epoch;
                MaxEpochs = maxEpochs;
                CurrReadoutUnit = currReadoutUnit;
                BestReadoutUnit = bestReadoutUnit;
                return;
            }

            //Methods
            /// <summary>
            /// Builds string containing information about the regression progress.
            /// </summary>
            /// <param name="margin">Specifies how many spaces to be at the begining of the line.</param>
            /// <returns>Built text line</returns>
            public string GetProgressInfo(int margin = 0)
            {
                //Build progress text message
                StringBuilder progressText = new StringBuilder();
                progressText.Append(new string(' ', margin));
                progressText.Append("Regression: ");
                progressText.Append(ReadoutUnitConfig.Name);
                progressText.Append(", Fold/Attempt/Epoch: ");
                progressText.Append(FoldNum.ToString().PadLeft(NumOfFolds.ToString().Length, '0') + "/");
                progressText.Append(RegrAttemptNumber.ToString().PadLeft(RegrMaxAttempts.ToString().Length, '0') + "/");
                progressText.Append(Epoch.ToString().PadLeft(MaxEpochs.ToString().Length, '0'));
                progressText.Append(", DSet-Sizes: (");
                progressText.Append(CurrReadoutUnit.TrainingErrorStat.NumOfSamples.ToString() + ", ");
                progressText.Append(CurrReadoutUnit.TestingErrorStat.NumOfSamples.ToString() + ")");
                progressText.Append(", Best-Train: ");
                progressText.Append(BestReadoutUnit.TrainingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture));
                if (ReadoutUnitConfig.TaskType == ReadoutUnit.TaskType.Classification)
                {
                    //Append binary errors
                    progressText.Append("/" + BestReadoutUnit.TrainingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                    progressText.Append("/" + BestReadoutUnit.TrainingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
                }
                progressText.Append(", Best-Test: ");
                progressText.Append(BestReadoutUnit.TestingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture));
                if (ReadoutUnitConfig.TaskType == ReadoutUnit.TaskType.Classification)
                {
                    //Append binary errors
                    progressText.Append("/" + BestReadoutUnit.TestingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                    progressText.Append("/" + BestReadoutUnit.TestingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
                }
                progressText.Append(", Curr-Train: ");
                progressText.Append(CurrReadoutUnit.TrainingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture));
                if (ReadoutUnitConfig.TaskType == ReadoutUnit.TaskType.Classification)
                {
                    //Append binary errors
                    progressText.Append("/" + CurrReadoutUnit.TrainingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                    progressText.Append("/" + CurrReadoutUnit.TrainingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
                }
                progressText.Append(", Curr-Test: ");
                progressText.Append(CurrReadoutUnit.TestingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture));
                if (ReadoutUnitConfig.TaskType == ReadoutUnit.TaskType.Classification)
                {
                    //Append binary errors
                    progressText.Append("/" + CurrReadoutUnit.TestingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                    progressText.Append("/" + CurrReadoutUnit.TestingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
                }
                progressText.Append($" [{BestReadoutUnit.TrainerInfoMessage}]");
                return progressText.ToString();
            }


        }//RegrState

        /// <summary>
        /// Contains instructions for the regression process
        /// </summary>
        public class RegrInstr
        {
            //Attribute properties
            /// <summary>
            /// Indicates whether to terminate the current regression attempt
            /// </summary>
            public bool StopCurrentAttempt { get; set; } = false;
            /// <summary>
            /// Indicates whether to terminate the entire regression process for the readout unit
            /// </summary>
            public bool StopReadoutUnitRegression { get; set; } = false;
            /// <summary>
            /// This is the most important switch indicating whether the CurrReadoutUnit is better than
            /// the BestReadoutUnit
            /// </summary>
            public bool CurrentIsBetter { get; set; } = false;

        }//RegrInstr

    }//ReadoutUnitBuilder

}//Namespace
