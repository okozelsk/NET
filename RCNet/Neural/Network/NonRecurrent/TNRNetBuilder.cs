using RCNet.MathTools;
using RCNet.MiscTools;
using RCNet.Neural.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Implements the builder of trained non-recurrent network.
    /// </summary>
    public class TNRNetBuilder
    {

        //Delegates
        /// <summary>
        /// The delegate of a network build process controller invoked during each training epoch.
        /// </summary>
        /// <remarks>
        /// The goal of the build process is to build a network that gives good results both on the training data and the testing data.
        /// BuildProgress object contains the best trained network so far and the current one. The primary purpose of controller is
        /// to make decision whether the current network is better than the best network so far.
        /// The controller can also tell the build process that it does not make any sense to continue. It can
        /// stop the current build attempt or the whole build process.
        /// </remarks>
        /// <param name="buildProgress">The BuildProgress object containing all the necessary information to control the build process.</param>
        /// <returns>Instructions for the network build process.</returns>
        public delegate BuildInstr BuildControllerDelegate(BuildProgress buildProgress);

        /// <summary>
        /// The delegate of the NetworkBuildProgressChanged event handler.
        /// </summary>
        /// <param name="buildProgress">The current state of the build process.</param>
        public delegate void NetworkBuildProgressChangedHandler(BuildProgress buildProgress);

        /// <summary>
        /// This informative event occurs every time the epoch of the network build process is done.
        /// </summary>
        public event NetworkBuildProgressChangedHandler NetworkBuildProgressChanged;


        //Attributes
        private readonly string _networkName;
        private readonly INonRecurrentNetworkSettings _networkCfg;
        private readonly TNRNet.OutputType _networkOutput;
        private readonly VectorBundle _trainingBundle;
        private readonly VectorBundle _testingBundle;
        private readonly Random _rand;
        private readonly BuildControllerDelegate _controller;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="networkName">The name of the network to be built.</param>
        /// <param name="networkCfg">The configuration of the network to be built.</param>
        /// <param name="networkOutput">The type of output of the network to be built.</param>
        /// <param name="trainingBundle">The bundle of input and ideal vectors to be used for the network training.</param>
        /// <param name="testingBundle">The bundle of input and ideal vectors to be used for the network testing.</param>
        /// <param name="rand">The random generator to be used (optional).</param>
        /// <param name="controller">The build process controller (optional).</param>
        public TNRNetBuilder(string networkName,
                             INonRecurrentNetworkSettings networkCfg,
                             TNRNet.OutputType networkOutput,
                             VectorBundle trainingBundle,
                             VectorBundle testingBundle,
                             Random rand = null,
                             BuildControllerDelegate controller = null
                             )
        {
            _networkName = networkName;
            NonRecurrentNetUtils.CheckNetCfg(networkOutput, networkCfg);
            _networkCfg = networkCfg;
            _networkOutput = networkOutput;
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
            int bestNetworkAttemptEpoch = 0;
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
                if ((TNRNet.IsBinErrorStatsOutputType(_networkOutput) && currNetwork.CombinedBinaryError < currNetworkLastImprovementCombinedBinaryError) ||
                    currNetwork.CombinedPrecisionError < currNetworkLastImprovementCombinedPrecisionError
                    )
                {
                    currNetworkLastImprovementCombinedPrecisionError = currNetwork.CombinedPrecisionError;
                    if (TNRNet.IsBinErrorStatsOutputType(_networkOutput))
                    {
                        currNetworkLastImprovementCombinedBinaryError = currNetwork.CombinedBinaryError;
                    }
                    currNetworkLastImprovementEpoch = trainer.AttemptEpoch;
                }
                //BuildProgress instance
                BuildProgress buildProgress = new BuildProgress(_networkName,
                                                                trainer.Attempt,
                                                                trainer.MaxAttempt,
                                                                trainer.AttemptEpoch,
                                                                trainer.MaxAttemptEpoch,
                                                                currNetwork,
                                                                currNetworkLastImprovementEpoch,
                                                                bestNetwork,
                                                                bestNetworkAttempt,
                                                                bestNetworkAttemptEpoch
                                                                );
                //Call controller
                BuildInstr instructions = _controller(buildProgress);
                //Better?
                if (instructions.CurrentIsBetter)
                {
                    //Adopt current regression unit as a best one
                    bestNetwork = currNetwork.DeepClone();
                    bestNetworkAttempt = trainer.Attempt;
                    bestNetworkAttemptEpoch = trainer.AttemptEpoch;
                    //Update build progress
                    buildProgress.BestNetwork = bestNetwork;
                    buildProgress.BestNetworkAttemptNum = bestNetworkAttempt;
                    buildProgress.BestNetworkAttemptEpochNum = bestNetworkAttemptEpoch;
                }
                //Raise notification event
                NetworkBuildProgressChanged?.Invoke(buildProgress);
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
        /// Implements the holder of the network build progress information.
        /// </summary>
        public class BuildProgress : IBuildProgress
        {
            //Attribute properties
            /// <summary>
            /// Name of the network.
            /// </summary>
            public string NetworkName { get; }

            /// <summary>
            /// Information about the progress of network build attempts.
            /// </summary>
            public ProgressTracker AttemptsTracker { get; }

            /// <summary>
            /// Information about the progress of network build epochs within the current attempt.
            /// </summary>
            public ProgressTracker AttemptEpochsTracker { get; }

            /// <summary>
            /// The current network and its error statistics.
            /// </summary>
            public TNRNet CurrNetwork { get; }

            /// <summary>
            /// An epoch number within the current build attempt when was found an improvement of the current network.
            /// </summary>
            public int CurrNetworkLastImprovementAttemptEpochNum { get; }

            /// <summary>
            /// The best network so far and its error statistics.
            /// </summary>
            public TNRNet BestNetwork { get; set; }

            /// <summary>
            /// The attempt number in which was found the best network so far.
            /// </summary>
            public int BestNetworkAttemptNum { get; set; }

            /// <summary>
            /// The epoch number within the BestNetworkAttempt in which was found the best network so far.
            /// </summary>
            public int BestNetworkAttemptEpochNum { get; set; }

            /// <summary>
            /// Creates an initialized instance.
            /// </summary>
            /// <param name="networkName">Name of the network.</param>
            /// <param name="attemptNum">The current attempt number.</param>
            /// <param name="maxNumOfAttempts">The maximum number of attempts.</param>
            /// <param name="attemptEpochNum">The current epoch number within the current attempt.</param>
            /// <param name="maxNumOfAttemptEpochs">The maximum number of epochs.</param>
            /// <param name="currNetwork">The current network and its error statistics.</param>
            /// <param name="currNetworkLastImprovementEpochNum">An epoch number within the current build attempt when was found an improvement of the current network.</param>
            /// <param name="bestNetwork">The best network so far and its error statistics.</param>
            /// <param name="bestNetworkAttemptNum">The attempt number in which was found the best network so far.</param>
            /// <param name="bestNetworkAttemptEpochNum">The epoch number within the bestNetworkAttemptNum in which was found the best network so far.</param>
            public BuildProgress(string networkName,
                                 int attemptNum,
                                 int maxNumOfAttempts,
                                 int attemptEpochNum,
                                 int maxNumOfAttemptEpochs,
                                 TNRNet currNetwork,
                                 int currNetworkLastImprovementEpochNum,
                                 TNRNet bestNetwork,
                                 int bestNetworkAttemptNum,
                                 int bestNetworkAttemptEpochNum
                                 )
            {
                NetworkName = networkName;
                AttemptsTracker = new ProgressTracker((uint)maxNumOfAttempts, (uint)attemptNum);
                AttemptEpochsTracker = new ProgressTracker((uint)maxNumOfAttemptEpochs, (uint)attemptEpochNum);
                CurrNetwork = currNetwork;
                CurrNetworkLastImprovementAttemptEpochNum = currNetworkLastImprovementEpochNum;
                BestNetwork = bestNetwork;
                BestNetworkAttemptNum = bestNetworkAttemptNum;
                BestNetworkAttemptEpochNum = bestNetworkAttemptEpochNum;
                return;
            }

            //Properties
            /// <inheritdoc cref="TNRNet.OutputType"/>
            public TNRNet.OutputType NetworkOutputType { get { return CurrNetwork.Output; } }

            /// <summary>
            /// Indicates the current network is also the best network so far.
            /// </summary>
            public bool CurrentIsBest { get { return (BestNetworkAttemptNum == AttemptsTracker.Current && BestNetworkAttemptEpochNum == AttemptEpochsTracker.Current); } }

            /// <inheritdoc/>
            public bool NewEndNetwork
            {
                get
                {
                    return AttemptsTracker.Current == 1 && AttemptEpochsTracker.Current == 1;
                }
            }

            /// <inheritdoc/>
            public bool ShouldBeReported
            {
                get
                {
                    return NewEndNetwork || CurrentIsBest || AttemptEpochsTracker.Last;
                }
            }

            /// <inheritdoc/>
            public int EndNetworkEpochNum
            {
                get
                {
                    return (int)AttemptEpochsTracker.Current;
                }
            }

            //Methods
            /// <summary>
            /// Gets textual information about the build basic progress.
            /// </summary>
            /// <param name="shortVersion">Specifies whether to build short version of the informative text.</param>
            public string GetBasicProgressInfoText(bool shortVersion = true)
            {
                StringBuilder text = new StringBuilder();
                if (shortVersion)
                {
                    text.Append($"Attempt {AttemptsTracker.Current.ToString(CultureInfo.InvariantCulture).PadLeft(AttemptsTracker.Target.ToString(CultureInfo.InvariantCulture).Length)}");
                    text.Append($", Epoch {AttemptEpochsTracker.Current.ToString(CultureInfo.InvariantCulture).PadLeft(AttemptEpochsTracker.Target.ToString(CultureInfo.InvariantCulture).Length)}");
                }
                else
                {
                    text.Append($"Attempt {AttemptsTracker}");
                    text.Append($", Epoch {AttemptEpochsTracker}");
                }
                return text.ToString();
            }

            /// <summary>
            /// Gets textual information about the training and test samples.
            /// </summary>
            /// <param name="shortVersion">Specifies whether to build short version of the informative text.</param>
            public string GetSamplesInfoText(bool shortVersion = true)
            {
                StringBuilder text = new StringBuilder();
                int numOfTrainingSamples = CurrNetwork.TrainingErrorStat.NumOfSamples / CurrNetwork.Network.NumOfOutputValues;
                int numOfTestingSamples = CurrNetwork.TestingErrorStat.NumOfSamples / CurrNetwork.Network.NumOfOutputValues;
                if (shortVersion)
                {
                    text.Append($"Samples {numOfTrainingSamples.ToString(CultureInfo.InvariantCulture)}");
                    text.Append($"/{numOfTestingSamples.ToString(CultureInfo.InvariantCulture)}");
                }
                else
                {
                    text.Append($"Training samples {numOfTrainingSamples.ToString(CultureInfo.InvariantCulture)}");
                    text.Append($", Testing samples {numOfTestingSamples.ToString(CultureInfo.InvariantCulture)}");
                }
                return text.ToString();
            }

            /// <summary>
            /// Gets textual information about the specified trained network instance.
            /// </summary>
            /// <param name="network">An instance of trained network.</param>
            /// <param name="shortVersion">Specifies whether to build short version of the informative text.</param>
            public string GetNetworkInfoText(TNRNet network, bool shortVersion = true)
            {
                StringBuilder text = new StringBuilder();
                if (shortVersion)
                {
                    text.Append("TrainErr ");
                    text.Append(network.TrainingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture));
                    if (network.HasBinErrorStats)
                    {
                        text.Append("/" + network.TrainingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                        text.Append("/" + network.TrainingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
                    }
                    text.Append(", TestErr ");
                    text.Append(network.TestingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture));
                    if (network.HasBinErrorStats)
                    {
                        text.Append("/" + network.TestingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                        text.Append("/" + network.TestingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
                    }
                }
                else
                {
                    text.Append("Training numerical error ");
                    text.Append(network.TrainingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture));
                    if (network.HasBinErrorStats)
                    {
                        text.Append(", total bad classifications " + network.TrainingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                        text.Append(", false positive classifications " + network.TrainingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
                    }
                    text.Append(", Testing numerical error ");
                    text.Append(network.TestingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture));
                    if (network.HasBinErrorStats)
                    {
                        text.Append(", total incorrect classifications " + network.TestingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                        text.Append(", false positive classifications " + network.TestingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
                    }
                }
                return text.ToString();
            }

            /// <inheritdoc/>
            public string GetInfoText(int margin = 0, bool includeName = true)
            {
                //Build the progress text message
                StringBuilder progressText = new StringBuilder();
                progressText.Append(new string(' ', margin));
                if (includeName)
                {
                    progressText.Append("[");
                    progressText.Append(NetworkName);
                    progressText.Append("] ");
                }
                progressText.Append(GetBasicProgressInfoText(true));
                progressText.Append(", ");
                progressText.Append(GetSamplesInfoText(true));
                progressText.Append(", BestNet-{");
                progressText.Append(GetNetworkInfoText(BestNetwork, true));
                progressText.Append("}, CurrNet-{");
                progressText.Append(GetNetworkInfoText(CurrNetwork, true));
                progressText.Append("}");
                if (BestNetwork.TrainerInfoMessage.Length > 0)
                {
                    progressText.Append(", BestParam-{");
                    progressText.Append(BestNetwork.TrainerInfoMessage);
                    progressText.Append("}");
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
