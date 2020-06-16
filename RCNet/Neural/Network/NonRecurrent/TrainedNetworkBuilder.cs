using RCNet.MathTools;
using RCNet.Neural.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Builds trained non-recurrent network.
    /// Supported is only single output network.
    /// </summary>
    public class TrainedNetworkBuilder
    {
        //Delegates
        /// <summary>
        /// This is the control function of the regression process and it is called after the completion of each regression training epoch.
        /// The goal of the regression process is to train a network that will give good results both on the training data and the test data.
        /// BuildingState object contains the best error statistics so far and the latest statistics. The primary purpose of this function is
        /// to decide whether the latest statistics are better than the best statistics so far.
        /// Function can also tell the regression process that it does not make any sense to continue the regression. It can
        /// terminate the current regression attempt or whole regression process.
        /// </summary>
        /// <param name="buildingState">Contains all the necessary information to control the regression.</param>
        /// <returns>Instructions for the regression process.</returns>
        public delegate BuildingInstr RegressionControllerDelegate(BuildingState buildingState);

        /// <summary>
        /// Delegate of RegressionEpochDone event handler.
        /// </summary>
        /// <param name="buildingState">Current state of the regression process</param>
        /// <param name="foundBetter">Indicates that the best network was found as a result of the performed epoch</param>
        public delegate void RegressionEpochDoneHandler(BuildingState buildingState, bool foundBetter);

        /// <summary>
        /// This informative event occurs every time the regression epoch is done
        /// </summary>
        [field: NonSerialized]
        public event RegressionEpochDoneHandler RegressionEpochDone;

        //Attributes
        private readonly string _networkName;
        private readonly INonRecurrentNetworkSettings _networkSettings;
        private readonly int _foldNum;
        private readonly int _numOfFolds;
        private readonly int _foldNetworkNum;
        private readonly int _numOfFoldNetworks;
        private readonly VectorBundle _trainingBundle;
        private readonly VectorBundle _testingBundle;
        private readonly double _binBorder;
        private readonly Random _rand;
        private readonly RegressionControllerDelegate _controller;

        //Constructor
        /// <summary>
        /// Creates an instance ready to start building trained non-recurrent network
        /// </summary>
        /// <param name="networkName">Name of the network to be built</param>
        /// <param name="networkSettings">Network configuration (FeedForwardNetworkSettings or ParallelPerceptronSettings object)</param>
        /// <param name="foldNum">Current fold number</param>
        /// <param name="numOfFolds">Total number of the folds</param>
        /// <param name="foldNetworkNum">Current fold network number</param>
        /// <param name="numOfFoldNetworks">Total number of the fold networks</param>
        /// <param name="trainingBundle">Bundle of predictors and ideal values to be used for training purposes</param>
        /// <param name="testingBundle">Bundle of predictors and ideal values to be used for testing purposes</param>
        /// <param name="binBorder">If specified, it indicates that the whole network output is binary and specifies numeric border where GE network output is decided as a 1 and LT output as a 0.</param>
        /// <param name="rand">Random generator to be used (optional)</param>
        /// <param name="controller">Regression controller (optional)</param>
        public TrainedNetworkBuilder(string networkName,
                                     INonRecurrentNetworkSettings networkSettings,
                                     int foldNum,
                                     int numOfFolds,
                                     int foldNetworkNum,
                                     int numOfFoldNetworks,
                                     VectorBundle trainingBundle,
                                     VectorBundle testingBundle,
                                     double binBorder = double.NaN,
                                     Random rand = null,
                                     RegressionControllerDelegate controller = null
                                     )
        {
            _networkName = networkName;
            _networkSettings = networkSettings;
            _foldNum = foldNum;
            _numOfFolds = numOfFolds;
            _foldNetworkNum = foldNetworkNum;
            _numOfFoldNetworks = numOfFoldNetworks;
            //Check num of output values is 1
            if (trainingBundle.OutputVectorCollection[0].Length != 1)
            {
                throw new InvalidOperationException($"Only single output value is allowed.");
            }
            _trainingBundle = trainingBundle;
            _testingBundle = testingBundle;
            _binBorder = binBorder;
            _rand = rand ?? new Random(0);
            _controller = controller ?? DefaultRegressionController;
            return;
        }

        //Properties
        /// <summary>
        /// Indicates that the network ideal output is binary
        /// </summary>
        private bool BinaryOutput { get { return !double.IsNaN(_binBorder); } }

        //Static methods
        /// <summary>
        /// This is a default implementation of an evaluation whether the "candidate" network
        /// achieved a better result than the best network so far
        /// </summary>
        /// <param name="binaryOutput">Indicates the whole network output is binary</param>
        /// <param name="candidate">Network to be evaluated</param>
        /// <param name="currentBest">The best network so far</param>
        public static bool IsBetter(bool binaryOutput, TrainedNetwork candidate, TrainedNetwork currentBest)
        {
            if (binaryOutput)
            {
                if(candidate.CombinedBinaryError > currentBest.CombinedBinaryError)
                {
                    return false;
                }
                else if (candidate.CombinedBinaryError < currentBest.CombinedBinaryError)
                {
                    return true;
                }
                //CombinedBinaryError is the same
                else if (candidate.TestingBinErrorStat.BinValErrStat[0].Sum > currentBest.TestingBinErrorStat.BinValErrStat[0].Sum)
                {
                    return false;
                }
                else if(candidate.TestingBinErrorStat.BinValErrStat[0].Sum < currentBest.TestingBinErrorStat.BinValErrStat[0].Sum)
                {
                    return true;
                }
                //CombinedBinaryError is the same
                //TestingBinErrorStat.BinValErrStat[0].Sum is the same
                else if (candidate.TrainingBinErrorStat.BinValErrStat[0].Sum > currentBest.TrainingBinErrorStat.BinValErrStat[0].Sum)
                {
                    return false;
                }
                else if (candidate.TrainingBinErrorStat.BinValErrStat[0].Sum < currentBest.TrainingBinErrorStat.BinValErrStat[0].Sum)
                {
                    return true;
                }
                //CombinedBinaryError is the same
                //TestingBinErrorStat.BinValErrStat[0].Sum is the same
                //TrainingBinErrorStat.BinValErrStat[0].Sum is the same
                else if (candidate.CombinedPrecisionError < currentBest.CombinedPrecisionError)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return (candidate.CombinedPrecisionError < currentBest.CombinedPrecisionError);
            }
        }

        //Methods
        private BuildingInstr DefaultRegressionController(BuildingState buildingState)
        {
            BuildingInstr instructions = new BuildingInstr
            {
                CurrentIsBetter = IsBetter(buildingState.BinaryOutput,
                                           buildingState.CurrNetwork,
                                           buildingState.BestNetwork
                                           ),
                StopProcess = (BinaryOutput &&
                               buildingState.BestNetwork.TrainingBinErrorStat.TotalErrStat.Sum == 0 &&
                               buildingState.BestNetwork.TestingBinErrorStat.TotalErrStat.Sum == 0 &&
                               buildingState.CurrNetwork.CombinedPrecisionError > buildingState.BestNetwork.CombinedPrecisionError
                               )
            };
            return instructions;
        }

        /// <summary>
        /// Builds trained network
        /// </summary>
        /// <returns>Trained network</returns>
        public TrainedNetwork Build()
        {
            TrainedNetwork bestNetwork = null;
            int lastImprovementEpoch = 0;
            double lastImprovementCombinedPrecisionError = 0d;
            double lastImprovementCombinedBinaryError = 0d;
            //Create network and trainer
            NonRecurrentNetUtils.CreateNetworkAndTrainer(_networkSettings,
                                                         _trainingBundle.InputVectorCollection,
                                                         _trainingBundle.OutputVectorCollection,
                                                         _rand,
                                                         out INonRecurrentNetwork net,
                                                         out INonRecurrentNetworkTrainer trainer
                                                         );
            //Iterate training cycles
            while (trainer.Iteration())
            {
                //Compute current error statistics after training iteration
                //Training data part
                TrainedNetwork currNetwork = new TrainedNetwork
                {
                    NetworkName = _networkName,
                    BinBorder = _binBorder,
                    Network = net,
                    TrainerInfoMessage = trainer.InfoMessage,
                    TrainingErrorStat = net.ComputeBatchErrorStat(_trainingBundle.InputVectorCollection, _trainingBundle.OutputVectorCollection, out List<double[]> trainingComputedOutputsCollection)
                };
                if (BinaryOutput)
                {
                    currNetwork.TrainingBinErrorStat = new BinErrStat(_binBorder, trainingComputedOutputsCollection, _trainingBundle.OutputVectorCollection);
                    currNetwork.CombinedBinaryError = currNetwork.TrainingBinErrorStat.TotalErrStat.Sum;
                }
                currNetwork.CombinedPrecisionError = currNetwork.TrainingErrorStat.ArithAvg;
                //Testing data part
                currNetwork.TestingErrorStat = net.ComputeBatchErrorStat(_testingBundle.InputVectorCollection, _testingBundle.OutputVectorCollection, out List<double[]> testingComputedOutputsCollection);
                currNetwork.CombinedPrecisionError = Math.Max(currNetwork.CombinedPrecisionError, currNetwork.TestingErrorStat.ArithAvg);
                if (BinaryOutput)
                {
                    currNetwork.TestingBinErrorStat = new BinErrStat(_binBorder, testingComputedOutputsCollection, _testingBundle.OutputVectorCollection);
                    currNetwork.CombinedBinaryError = Math.Max(currNetwork.CombinedBinaryError, currNetwork.TestingBinErrorStat.TotalErrStat.Sum);
                }
                //Expected precision accuracy
                currNetwork.ExpectedPrecisionAccuracy = Math.Min((1d - (currNetwork.TrainingErrorStat.ArithAvg / currNetwork.Network.OutputRange.Span)), (1d - (currNetwork.TestingErrorStat.ArithAvg / currNetwork.Network.OutputRange.Span)));
                //Expected binary accuracy
                if (BinaryOutput)
                {
                    currNetwork.ExpectedBinaryAccuracy = Math.Min((1d - currNetwork.TrainingBinErrorStat.TotalErrStat.ArithAvg), (1d - currNetwork.TestingBinErrorStat.TotalErrStat.ArithAvg));
                }
                else
                {
                    currNetwork.ExpectedBinaryAccuracy = double.NaN;
                }

                //Restart lastImprovementEpoch when new trainer's attempt started
                if (trainer.AttemptEpoch == 1)
                {
                    lastImprovementEpoch = trainer.AttemptEpoch;
                    lastImprovementCombinedPrecisionError = currNetwork.CombinedPrecisionError;
                    lastImprovementCombinedBinaryError = currNetwork.CombinedBinaryError;
                }
                //First initialization of the best network
                bestNetwork = bestNetwork ?? currNetwork.DeepClone();
                //RegrState instance
                BuildingState regrState = new BuildingState(_networkName, _binBorder, _foldNum, _numOfFolds, _foldNetworkNum, _numOfFoldNetworks, trainer.Attempt, trainer.MaxAttempt, trainer.AttemptEpoch, trainer.MaxAttemptEpoch, currNetwork, bestNetwork, lastImprovementEpoch);
                //Call controller
                BuildingInstr instructions = _controller(regrState);
                //Better?
                if (instructions.CurrentIsBetter)
                {
                    //Adopt current regression unit as a best one
                    bestNetwork = currNetwork.DeepClone();
                    regrState.BestNetwork = bestNetwork;
                    lastImprovementEpoch = trainer.AttemptEpoch;
                    lastImprovementCombinedPrecisionError = currNetwork.CombinedPrecisionError;
                    lastImprovementCombinedBinaryError = currNetwork.CombinedBinaryError;
                }
                if (currNetwork.CombinedBinaryError < lastImprovementCombinedBinaryError || currNetwork.CombinedPrecisionError < lastImprovementCombinedPrecisionError)
                {
                    lastImprovementEpoch = trainer.AttemptEpoch;
                    lastImprovementCombinedPrecisionError = currNetwork.CombinedPrecisionError;
                    lastImprovementCombinedBinaryError = currNetwork.CombinedBinaryError;
                }
                //Raise notification event
                RegressionEpochDone(regrState, instructions.CurrentIsBetter);
                //Process instructions
                if (instructions.StopProcess)
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
            bestNetwork.OutputWeightsStat = bestNetwork.Network.ComputeWeightsStat();
            return bestNetwork;
        }


        //Inner classes
        /// <summary>
        /// The class contains information needed to control network building (regression) process.
        /// This class is also used for progeress changed event.
        /// </summary>
        [Serializable]
        public class BuildingState
        {
            //Attribute properties
            /// <summary>
            /// Name of the network
            /// </summary>
            public string NetworkName { get; }
            /// <summary>
            /// If specified, it indicates that the whole network output is binary and specifies numeric border where GE network output is decided as a 1 and LT output as a 0.
            /// </summary>
            public double BinBorder { get; }
            /// <summary>
            /// Current fold number
            /// </summary>
            public int FoldNum { get; }
            /// <summary>
            /// Total number of the folds
            /// </summary>
            public int NumOfFolds { get; }
            /// <summary>
            /// Current fold network number
            /// </summary>
            public int FoldNetworkNum { get; }
            /// <summary>
            /// Total number of the fold networks
            /// </summary>
            public int NumOfFoldNetworks { get; }
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
            public TrainedNetwork CurrNetwork { get; }
            /// <summary>
            /// Contains the best network for now and related important error statistics.
            /// </summary>
            public TrainedNetwork BestNetwork { get; set; }
            /// <summary>
            /// Specifies when was lastly found an improvement
            /// </summary>
            public int LastImprovementEpoch { get; set; }

            /// <summary>
            /// Creates an initialized instance
            /// </summary>
            /// <param name="networkName">Name of the network</param>
            /// <param name="binBorder">If specified, it indicates that the whole network output is binary and specifies numeric border where GE network output is decided as a 1 and LT output as a 0.</param>
            /// <param name="foldNum">Current fold number</param>
            /// <param name="numOfFolds">Total number of the folds</param>
            /// <param name="foldNetworkNum">Current fold network number</param>
            /// <param name="numOfFoldNetworks">Total number of the fold networks</param>
            /// <param name="regrAttemptNumber">Current regression attempt number</param>
            /// <param name="regrMaxAttempts">Maximum number of regression attempts</param>
            /// <param name="epoch">Current epoch number within the current regression attempt</param>
            /// <param name="maxEpochs">Maximum number of epochs</param>
            /// <param name="currNetwork">Current network and related important error statistics.</param>
            /// <param name="bestNetwork">The best network for now and related important error statistics.</param>
            /// <param name="lastImprovementEpoch">Specifies when was lastly found an improvement (bestNetwork=currNetwork).</param>
            public BuildingState(string networkName,
                                 double binBorder,
                                 int foldNum,
                                 int numOfFolds,
                                 int foldNetworkNum,
                                 int numOfFoldNetworks,
                                 int regrAttemptNumber,
                                 int regrMaxAttempts,
                                 int epoch,
                                 int maxEpochs,
                                 TrainedNetwork currNetwork,
                                 TrainedNetwork bestNetwork,
                                 int lastImprovementEpoch
                                 )
            {
                NetworkName = networkName;
                BinBorder = binBorder;
                FoldNum = foldNum;
                NumOfFolds = numOfFolds;
                FoldNetworkNum = foldNetworkNum;
                NumOfFoldNetworks = numOfFoldNetworks;
                RegrAttemptNumber = regrAttemptNumber;
                RegrMaxAttempts = regrMaxAttempts;
                Epoch = epoch;
                MaxEpochs = maxEpochs;
                CurrNetwork = currNetwork;
                BestNetwork = bestNetwork;
                LastImprovementEpoch = lastImprovementEpoch;
                return;
            }

            //Properties
            /// <summary>
            /// Indicates that the whole network output is binary
            /// </summary>
            public bool BinaryOutput { get { return !double.IsNaN(BinBorder); } }

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
                progressText.Append("Unit [");
                progressText.Append(NetworkName);
                progressText.Append("] Fold/Net/Attempt/Epoch: ");
                progressText.Append(FoldNum.ToString().PadLeft(NumOfFolds.ToString().Length, '0') + "/");
                progressText.Append(FoldNetworkNum.ToString().PadLeft(NumOfFoldNetworks.ToString().Length, '0') + "/");
                progressText.Append(RegrAttemptNumber.ToString().PadLeft(RegrMaxAttempts.ToString().Length, '0') + "/");
                progressText.Append(Epoch.ToString().PadLeft(MaxEpochs.ToString().Length, '0'));
                progressText.Append(", Samples: ");
                progressText.Append(CurrNetwork.TrainingErrorStat.NumOfSamples.ToString() + "/");
                progressText.Append(CurrNetwork.TestingErrorStat.NumOfSamples.ToString());
                progressText.Append(", Best-Train: ");
                progressText.Append(BestNetwork.TrainingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture));
                if (BinaryOutput)
                {
                    //Append binary errors
                    progressText.Append("/" + BestNetwork.TrainingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                    progressText.Append("/" + BestNetwork.TrainingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
                }
                progressText.Append(", Best-Test: ");
                progressText.Append(BestNetwork.TestingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture));
                if (BinaryOutput)
                {
                    //Append binary errors
                    progressText.Append("/" + BestNetwork.TestingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                    progressText.Append("/" + BestNetwork.TestingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
                }
                progressText.Append(", Curr-Train: ");
                progressText.Append(CurrNetwork.TrainingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture));
                if (BinaryOutput)
                {
                    //Append binary errors
                    progressText.Append("/" + CurrNetwork.TrainingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                    progressText.Append("/" + CurrNetwork.TrainingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
                }
                progressText.Append(", Curr-Test: ");
                progressText.Append(CurrNetwork.TestingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture));
                if (BinaryOutput)
                {
                    //Append binary errors
                    progressText.Append("/" + CurrNetwork.TestingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                    progressText.Append("/" + CurrNetwork.TestingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
                }
                if (BestNetwork.TrainerInfoMessage.Length > 0)
                {
                    progressText.Append($" [{BestNetwork.TrainerInfoMessage}]");
                }
                return progressText.ToString();
            }


        }//BuildingState

        /// <summary>
        /// Contains instructions for the network building (regression) process
        /// </summary>
        public class BuildingInstr
        {
            //Attribute properties
            /// <summary>
            /// Indicates whether to terminate the current regression attempt
            /// </summary>
            public bool StopCurrentAttempt { get; set; } = false;
            /// <summary>
            /// Indicates whether to terminate the entire regression process
            /// </summary>
            public bool StopProcess { get; set; } = false;
            /// <summary>
            /// This is the most important switch indicating whether the CurrNetwork is better than
            /// the BestNetwork
            /// </summary>
            public bool CurrentIsBetter { get; set; } = false;

        }//BuildingInstr

    }//TrainedNetworkBuilder

}//Namespace
