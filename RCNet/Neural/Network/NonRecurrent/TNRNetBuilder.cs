using RCNet.MathTools;
using RCNet.Neural.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Implements the builder of the trained non-recurrent network.
    /// </summary>
    public class TNRNetBuilder
    {

        //Delegates
        /// <summary>
        /// The delegate of the controller of the network building process.
        /// It is invoked after the completion of each training epoch.
        /// </summary>
        /// <remarks>
        /// The goal of the build process is to train a network that gives good results both on the training data and the testing data.
        /// BuildProgress object contains the best trained network so far and the current one. The primary purpose of controller is
        /// to make decision whether the current network is better than the best network so far.
        /// The controller can also tell the build process that it does not make any sense to continue. It can
        /// stop the current regression attempt or the whole build process.
        /// </remarks>
        /// <param name="buildProgress">The BuildProgress object containing all the necessary information to control the build process.</param>
        /// <returns>The instructions for the build process.</returns>
        public delegate BuildInstr BuildControllerDelegate(BuildProgress buildProgress);

        /// <summary>
        /// The delegate of the EpochDone event handler.
        /// </summary>
        /// <param name="buildProgress">The current state of the build process.</param>
        /// <param name="foundBetter">Indicates that the best network so far was found during the last performed epoch.</param>
        public delegate void EpochDoneHandler(BuildProgress buildProgress, bool foundBetter);

        /// <summary>
        /// This informative event occurs every time the epoch of the network build process is done.
        /// </summary>
        public event EpochDoneHandler EpochDone;


        //Attributes
        private readonly string _buildContext;
        private readonly string _networkName;
        private readonly INonRecurrentNetworkSettings _networkCfg;
        private readonly TNRNet.OutputType _networkOutput;
        private readonly int _foldNum;
        private readonly int _numOfFolds;
        private readonly int _foldNetworkNum;
        private readonly int _numOfFoldNetworks;
        private readonly VectorBundle _trainingBundle;
        private readonly VectorBundle _testingBundle;
        private readonly Random _rand;
        private readonly BuildControllerDelegate _controller;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="buildContext">The context of the build process.</param>
        /// <param name="networkName">The name of the network to be built.</param>
        /// <param name="networkCfg">The configuration of the network to be built.</param>
        /// <param name="networkOutput">The type of output of the network to be built.</param>
        /// <param name="foldNum">The current fold number.</param>
        /// <param name="numOfFolds">The total number of the folds.</param>
        /// <param name="foldNetworkNum">The current fold network number.</param>
        /// <param name="numOfFoldNetworks">The total number of the fold networks.</param>
        /// <param name="trainingBundle">The bundle of input and ideal vectors to be used for the network training.</param>
        /// <param name="testingBundle">The bundle of input and ideal vectors to be used for the network testing.</param>
        /// <param name="rand">The random generator to be used (optional).</param>
        /// <param name="controller">The build process controller (optional).</param>
        public TNRNetBuilder(string buildContext,
                             string networkName,
                             INonRecurrentNetworkSettings networkCfg,
                             TNRNet.OutputType networkOutput,
                             int foldNum,
                             int numOfFolds,
                             int foldNetworkNum,
                             int numOfFoldNetworks,
                             VectorBundle trainingBundle,
                             VectorBundle testingBundle,
                             Random rand = null,
                             BuildControllerDelegate controller = null
                             )
        {
            _buildContext = buildContext;
            _networkName = networkName;
            NonRecurrentNetUtils.CheckNetCfg(networkOutput, networkCfg);
            _networkCfg = networkCfg;
            _networkOutput = networkOutput;
            _foldNum = foldNum;
            _numOfFolds = numOfFolds;
            _foldNetworkNum = foldNetworkNum;
            _numOfFoldNetworks = numOfFoldNetworks;
            NonRecurrentNetUtils.CheckData(_networkOutput, trainingBundle);
            _trainingBundle = trainingBundle;
            NonRecurrentNetUtils.CheckData(_networkOutput, testingBundle);
            _testingBundle = testingBundle;
            _rand = rand ?? new Random(0);
            _controller = controller ?? DefaultNetworkBuildController;
            return;
        }

        //Properties
        /// <summary>
        /// Gets the boolean border of the network.
        /// </summary>
        private double BoolBorder
        {
            get
            {
                if (_networkOutput == TNRNet.OutputType.Probabilistic)
                {
                    return Interval.IntZP1.Mid;
                }
                else
                {
                    return Interval.IntN1P1.Mid;
                }
            }
        }

        //Static methods
        /// <summary>
        /// Evaluates whether the "candidate" network achieved a better result than the best network so far.
        /// </summary>
        /// <remarks>
        /// The default implementation.
        /// </remarks>
        /// <param name="candidate">The candidate network to be evaluated.</param>
        /// <param name="currentBest">The best network so far.</param>
        public static bool IsBetter(TNRNet candidate, TNRNet currentBest)
        {
            //Binary decisions comparison
            if (candidate.HasBinErrorStats)
            {
                if (candidate.CombinedBinaryError > currentBest.CombinedBinaryError)
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
                else if (candidate.TestingBinErrorStat.BinValErrStat[0].Sum < currentBest.TestingBinErrorStat.BinValErrStat[0].Sum)
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
            }
            //Numerical precision comparison
            if (candidate.CombinedPrecisionError < currentBest.CombinedPrecisionError)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //Methods
        /// <summary>
        /// The default implementation of the build process controler.
        /// </summary>
        protected BuildInstr DefaultNetworkBuildController(BuildProgress buildProgress)
        {
            BuildInstr instructions = new BuildInstr
            {
                CurrentIsBetter = IsBetter(buildProgress.CurrNetwork,
                                           buildProgress.BestNetwork
                                           ),
                StopProcess = (buildProgress.CurrNetwork.HasBinErrorStats &&
                               buildProgress.BestNetwork.TrainingBinErrorStat.TotalErrStat.Sum == 0 &&
                               buildProgress.BestNetwork.TestingBinErrorStat.TotalErrStat.Sum == 0 &&
                               buildProgress.CurrNetwork.CombinedPrecisionError > buildProgress.BestNetwork.CombinedPrecisionError
                               )
            };
            return instructions;
        }

        /// <summary>
        /// Builds the trained network.
        /// </summary>
        /// <returns>The trained network.</returns>
        public TNRNet Build()
        {
            TNRNet bestNetwork = null;
            int bestNetworkAttempt = 0;
            int currNetworkLastImprovementEpoch = 0;
            double currNetworkLastImprovementCombinedPrecisionError = 0d;
            double currNetworkLastImprovementCombinedBinaryError = 0d;
            //Create network and trainer
            NonRecurrentNetUtils.CreateNetworkAndTrainer(_networkCfg,
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
                TNRNet currNetwork = new TNRNet(_networkName, _networkOutput)
                {
                    Network = net,
                    TrainerInfoMessage = trainer.InfoMessage,
                    TrainingErrorStat = net.ComputeBatchErrorStat(_trainingBundle.InputVectorCollection, _trainingBundle.OutputVectorCollection, out List<double[]> trainingComputedOutputsCollection)
                };
                if (TNRNet.IsBinErrorStatsOutputType(_networkOutput))
                {
                    currNetwork.TrainingBinErrorStat = new BinErrStat(BoolBorder, trainingComputedOutputsCollection, _trainingBundle.OutputVectorCollection);
                    currNetwork.CombinedBinaryError = currNetwork.TrainingBinErrorStat.TotalErrStat.Sum;
                }
                currNetwork.CombinedPrecisionError = currNetwork.TrainingErrorStat.ArithAvg;
                //Testing data part
                currNetwork.TestingErrorStat = net.ComputeBatchErrorStat(_testingBundle.InputVectorCollection, _testingBundle.OutputVectorCollection, out List<double[]> testingComputedOutputsCollection);
                currNetwork.CombinedPrecisionError = Math.Max(currNetwork.CombinedPrecisionError, currNetwork.TestingErrorStat.ArithAvg);
                if (TNRNet.IsBinErrorStatsOutputType(_networkOutput))
                {
                    currNetwork.TestingBinErrorStat = new BinErrStat(BoolBorder, testingComputedOutputsCollection, _testingBundle.OutputVectorCollection);
                    currNetwork.CombinedBinaryError = Math.Max(currNetwork.CombinedBinaryError, currNetwork.TestingBinErrorStat.TotalErrStat.Sum);
                }
                //Restart lastImprovementEpoch when new trainer's attempt started
                if (trainer.AttemptEpoch == 1)
                {
                    currNetworkLastImprovementEpoch = trainer.AttemptEpoch;
                    currNetworkLastImprovementCombinedPrecisionError = currNetwork.CombinedPrecisionError;
                    if (TNRNet.IsBinErrorStatsOutputType(_networkOutput))
                    {
                        currNetworkLastImprovementCombinedBinaryError = currNetwork.CombinedBinaryError;
                    }
                }
                //First initialization of the best network
                if (bestNetwork == null)
                {
                    bestNetwork = currNetwork.DeepClone();
                    bestNetworkAttempt = trainer.Attempt;
                }
                //RegrState instance
                BuildProgress buildProgress = new BuildProgress(_buildContext, _networkName, _foldNum, _numOfFolds, _foldNetworkNum, _numOfFoldNetworks, trainer.Attempt, trainer.MaxAttempt, trainer.AttemptEpoch, trainer.MaxAttemptEpoch, currNetwork, currNetworkLastImprovementEpoch, bestNetwork, bestNetworkAttempt);
                //Call controller
                BuildInstr instructions = _controller(buildProgress);
                //Better?
                if (instructions.CurrentIsBetter)
                {
                    //Adopt current regression unit as a best one
                    bestNetwork = currNetwork.DeepClone();
                    buildProgress.BestNetwork = bestNetwork;
                    bestNetworkAttempt = trainer.Attempt;
                }
                if ((TNRNet.IsBinErrorStatsOutputType(_networkOutput) && currNetwork.CombinedBinaryError < currNetworkLastImprovementCombinedBinaryError) ||
                    currNetwork.CombinedPrecisionError < currNetworkLastImprovementCombinedPrecisionError
                    )
                {
                    currNetworkLastImprovementEpoch = trainer.AttemptEpoch;
                    currNetworkLastImprovementCombinedPrecisionError = currNetwork.CombinedPrecisionError;
                    if (TNRNet.IsBinErrorStatsOutputType(_networkOutput))
                    {
                        currNetworkLastImprovementCombinedBinaryError = currNetwork.CombinedBinaryError;
                    }
                }
                //Raise notification event
                EpochDone?.Invoke(buildProgress, instructions.CurrentIsBetter);
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
            bestNetwork.NetworkWeightsStat = bestNetwork.Network.ComputeWeightsStat();
            return bestNetwork;
        }

        //Inner classes
        /// <summary>
        /// Implements the holder of the build progress information.
        /// </summary>
        public class BuildProgress
        {
            //Attribute properties
            /// <summary>
            /// The context of the build process.
            /// </summary>
            public string BuildContext { get; }

            /// <summary>
            /// The name of the network.
            /// </summary>
            public string NetworkName { get; }

            /// <summary>
            /// The current fold number.
            /// </summary>
            public int FoldNum { get; }

            /// <summary>
            /// The total number of the folds.
            /// </summary>
            public int NumOfFolds { get; }

            /// <summary>
            /// The current fold network number.
            /// </summary>
            public int FoldNetworkNum { get; }

            /// <summary>
            /// The total number of the fold networks.
            /// </summary>
            public int NumOfFoldNetworks { get; }

            /// <summary>
            /// The current attempt number.
            /// </summary>
            public int AttemptNumber { get; }

            /// <summary>
            /// The maximum number of attempts.
            /// </summary>
            public int MaxAttempts { get; }

            /// <summary>
            /// The current epoch number.
            /// </summary>
            public int Epoch { get; }

            /// <summary>
            /// The maximum number of epochs.
            /// </summary>
            public int MaxEpochs { get; }

            /// <summary>
            /// The current network and its error statistics.
            /// </summary>
            public TNRNet CurrNetwork { get; }

            /// <summary>
            /// Specifies when was lastly found an improvement of the current network within the current attempt.
            /// </summary>
            public int CurrNetworkLastImprovementEpoch { get; set; }

            /// <summary>
            /// The best network so far and its error statistics.
            /// </summary>
            public TNRNet BestNetwork { get; set; }

            /// <summary>
            /// The attempt number in which was found the best network so far.
            /// </summary>
            public int BestNetworkAttempt { get; set; }

            /// <summary>
            /// Creates an initialized instance.
            /// </summary>
            /// <param name="buildContext">The context of the build process.</param>
            /// <param name="networkName">The name of the network.</param>
            /// <param name="foldNum">The current fold number.</param>
            /// <param name="numOfFolds">The total number of the folds.</param>
            /// <param name="foldNetworkNum">The current fold network number.</param>
            /// <param name="numOfFoldNetworks">The total number of the fold networks.</param>
            /// <param name="regrAttemptNumber">The current attempt number.</param>
            /// <param name="regrMaxAttempts">The maximum number of attempts.</param>
            /// <param name="epoch">The current epoch number within the current attempt.</param>
            /// <param name="maxEpochs">The maximum number of epochs.</param>
            /// <param name="currNetwork">The current network and its error statistics.</param>
            /// <param name="currNetworkLastImprovementEpoch">Specifies when was lastly found an improvement of the current network within the current attempt.</param>
            /// <param name="bestNetwork">The best network so far and its error statistics.</param>
            /// <param name="bestNetworkAttempt">The attempt number in which was found the best network so far.</param>
            public BuildProgress(string buildContext,
                                 string networkName,
                                 int foldNum,
                                 int numOfFolds,
                                 int foldNetworkNum,
                                 int numOfFoldNetworks,
                                 int regrAttemptNumber,
                                 int regrMaxAttempts,
                                 int epoch,
                                 int maxEpochs,
                                 TNRNet currNetwork,
                                 int currNetworkLastImprovementEpoch,
                                 TNRNet bestNetwork,
                                 int bestNetworkAttempt
                                 )
            {
                BuildContext = buildContext;
                NetworkName = networkName;
                FoldNum = foldNum;
                NumOfFolds = numOfFolds;
                FoldNetworkNum = foldNetworkNum;
                NumOfFoldNetworks = numOfFoldNetworks;
                AttemptNumber = regrAttemptNumber;
                MaxAttempts = regrMaxAttempts;
                Epoch = epoch;
                MaxEpochs = maxEpochs;
                CurrNetwork = currNetwork;
                CurrNetworkLastImprovementEpoch = currNetworkLastImprovementEpoch;
                BestNetwork = bestNetwork;
                BestNetworkAttempt = bestNetworkAttempt;
                return;
            }

            //Properties
            /// <inheritdoc cref="TNRNet.OutputType"/>
            public TNRNet.OutputType NetworkOutputType { get { return CurrNetwork.Output; } }

            //Methods
            /// <summary>
            /// Builds the text message with the information about the network build progress.
            /// </summary>
            /// <param name="margin">The left margin (number of spaces).</param>
            /// <returns>The built text message.</returns>
            public string GetInfo(int margin = 0)
            {
                //Build the progress text message
                StringBuilder progressText = new StringBuilder();
                progressText.Append(new string(' ', margin));
                progressText.Append("[");
                if (BuildContext.Length > 0)
                {
                    progressText.Append(BuildContext);
                    progressText.Append(" - ");
                }
                progressText.Append(NetworkName);
                progressText.Append("] Fold/Net/Attempt/Epoch: ");
                progressText.Append(FoldNum.ToString().PadLeft(NumOfFolds.ToString().Length, '0') + "/");
                progressText.Append(FoldNetworkNum.ToString().PadLeft(NumOfFoldNetworks.ToString().Length, '0') + "/");
                progressText.Append(AttemptNumber.ToString().PadLeft(MaxAttempts.ToString().Length, '0') + "/");
                progressText.Append(Epoch.ToString().PadLeft(MaxEpochs.ToString().Length, '0'));
                progressText.Append(", Samples: ");
                progressText.Append((CurrNetwork.TrainingErrorStat.NumOfSamples / CurrNetwork.Network.NumOfOutputValues).ToString() + "/");
                progressText.Append((CurrNetwork.TestingErrorStat.NumOfSamples / CurrNetwork.Network.NumOfOutputValues).ToString());
                progressText.Append(", Best-Train: ");
                progressText.Append(BestNetwork.TrainingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture));
                if (BestNetwork.HasBinErrorStats)
                {
                    progressText.Append("/" + BestNetwork.TrainingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                    progressText.Append("/" + BestNetwork.TrainingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
                }
                progressText.Append(", Best-Test: ");
                progressText.Append(BestNetwork.TestingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture));
                if (BestNetwork.HasBinErrorStats)
                {
                    progressText.Append("/" + BestNetwork.TestingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                    progressText.Append("/" + BestNetwork.TestingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
                }
                progressText.Append(", Curr-Train: ");
                progressText.Append(CurrNetwork.TrainingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture));
                if (CurrNetwork.HasBinErrorStats)
                {
                    progressText.Append("/" + CurrNetwork.TrainingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                    progressText.Append("/" + CurrNetwork.TrainingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
                }
                progressText.Append(", Curr-Test: ");
                progressText.Append(CurrNetwork.TestingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture));
                if (CurrNetwork.HasBinErrorStats)
                {
                    progressText.Append("/" + CurrNetwork.TestingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                    progressText.Append("/" + CurrNetwork.TestingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
                }
                if (BestNetwork.TrainerInfoMessage.Length > 0)
                {
                    progressText.Append($" [{BestNetwork.TrainerInfoMessage}]");
                }
                return progressText.ToString();
            }

        }//BuildProgress

        /// <summary>
        /// Implements the holder of instructions for the network build process.
        /// </summary>
        public class BuildInstr
        {
            //Attribute properties
            /// <summary>
            /// Instructs whether to terminate the current regression attempt.
            /// </summary>
            public bool StopCurrentAttempt { get; set; } = false;

            /// <summary>
            /// Instructs whether to terminate the entire build process.
            /// </summary>
            public bool StopProcess { get; set; } = false;

            /// <summary>
            /// Informs the build process whether the current network is better than the best network so far.
            /// </summary>
            public bool CurrentIsBetter { get; set; } = false;

        }//BuildInstr

    }//TNRNetBuilder

}//Namespace
