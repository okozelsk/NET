﻿using RCNet.MathTools;
using RCNet.Neural.Activation;
using RCNet.Neural.Data;
using RCNet.Neural.Network.NonRecurrent.FF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Builds trained "One Takes All" network.
    /// Supported is Feed Forward Network having multiple probability outputs and SoftMax output activation.
    /// </summary>
    public class TrainedOneTakesAllNetworkBuilder
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
        private readonly FeedForwardNetworkSettings _networkSettings;
        private readonly int _foldNum;
        private readonly int _numOfFolds;
        private readonly int _foldNetworkNum;
        private readonly int _numOfFoldNetworks;
        private readonly VectorBundle _trainingBundle;
        private readonly VectorBundle _testingBundle;
        private readonly Random _rand;
        private readonly RegressionControllerDelegate _controller;

        //Constructor
        /// <summary>
        /// Creates an instance ready to start building trained "One Takes All" network
        /// </summary>
        /// <param name="networkName">Name of the network to be built</param>
        /// <param name="networkSettings">Feed Forward Network configuration (required is SoftMax output activation)</param>
        /// <param name="foldNum">Current fold number</param>
        /// <param name="numOfFolds">Total number of the folds</param>
        /// <param name="foldNetworkNum">Current fold network number</param>
        /// <param name="numOfFoldNetworks">Total number of the fold networks</param>
        /// <param name="trainingBundle">Bundle of predictors and ideal values to be used for training purposes. Ideal values must be 0 or 1 and only one 1 must be set in the ideal vector.</param>
        /// <param name="testingBundle">Bundle of predictors and ideal values to be used for testing purposes. Ideal values must be 0 or 1 and only one 1 must be set in the ideal vector.</param>
        /// <param name="rand">Random generator to be used (optional)</param>
        /// <param name="controller">Regression controller (optional)</param>
        public TrainedOneTakesAllNetworkBuilder(string networkName,
                                                FeedForwardNetworkSettings networkSettings,
                                                int foldNum,
                                                int numOfFolds,
                                                int foldNetworkNum,
                                                int numOfFoldNetworks,
                                                VectorBundle trainingBundle,
                                                VectorBundle testingBundle,
                                                Random rand = null,
                                                RegressionControllerDelegate controller = null
                                                )
        {
            //Check network config
            CheckNetCfg(networkSettings);
            //Check the data
            CheckBinOutput(trainingBundle);
            CheckBinOutput(testingBundle);
            //Checking passed, continue
            _networkName = networkName;
            _networkSettings = networkSettings;
            _foldNum = foldNum;
            _numOfFolds = numOfFolds;
            _foldNetworkNum = foldNetworkNum;
            _numOfFoldNetworks = numOfFoldNetworks;
            _trainingBundle = trainingBundle;
            _testingBundle = testingBundle;
            _rand = rand ?? new Random(0);
            _controller = controller ?? DefaultRegressionController;
            return;
        }

        //Properties

        //Static methods
        /// <summary>
        /// Checks the output vector contains only 0 and one 1.
        /// </summary>
        /// <param name="data">Data to be checked</param>
        public void CheckBinOutput(VectorBundle data)
        {
            foreach(double[] outputVector in data.OutputVectorCollection)
            {
                if(outputVector.Length <= 1)
                {
                    throw new InvalidOperationException($"Number of output vector values must be GT 1.");
                }
                int bin1Counter = 0;
                int bin0Counter = 0;
                for (int i = 0; i < outputVector.Length; i++)
                {
                    if(outputVector[i] == 0d)
                    {
                        ++bin0Counter;
                    }
                    else if(outputVector[i] == 1d)
                    {
                        ++bin1Counter;
                    }
                    else
                    {
                        throw new ArgumentException($"Output data vectors contain different values tha 0 or 1.", "data");
                    }
                }
                if (bin1Counter != 1)
                {
                    throw new ArgumentException($"Output data vector contains more than one 1.", "data");
                }
            }
            return;
        }

        /// <summary>
        /// Function checks that network has SoftMax output activation and associated RProp trainer.
        /// </summary>
        /// <param name="netCfg">Feed Forward Network configuration</param>
        public static void CheckNetCfg(FeedForwardNetworkSettings netCfg)
        {
            if (netCfg.OutputActivationCfg.GetType() != typeof(AFAnalogSoftMaxSettings))
            {
                throw new ArgumentException($"Feed forward network must have SoftMax output activation.", "netCfg");
            }
            if (netCfg.TrainerCfg.GetType() != typeof(RPropTrainerSettings))
            {
                throw new ArgumentException($"Feed forward network must have associated RProp trainer.", "netCfg");
            }
            return;
        }

        /// <summary>
        /// This is a default implementation of an evaluation whether the "candidate" network
        /// achieved a better result than the best network so far
        /// </summary>
        /// <param name="candidate">Network to be evaluated</param>
        /// <param name="currentBest">The best network so far</param>
        public static bool IsBetter(TrainedOneTakesAllNetwork candidate, TrainedOneTakesAllNetwork currentBest)
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

        //Instance methods
        private BuildingInstr DefaultRegressionController(BuildingState buildingState)
        {
            BuildingInstr instructions = new BuildingInstr
            {
                CurrentIsBetter = IsBetter(buildingState.CurrNetwork,
                                           buildingState.BestNetwork
                                           ),
                StopProcess = (buildingState.BestNetwork.CombinedBinaryError == 0 &&
                               buildingState.CurrNetwork.CombinedPrecisionError > buildingState.BestNetwork.CombinedPrecisionError
                               )
            };
            return instructions;
        }

        /// <summary>
        /// Builds trained network
        /// </summary>
        /// <returns>Trained network</returns>
        public TrainedOneTakesAllNetwork Build()
        {
            TrainedOneTakesAllNetwork bestNetwork = null;
            int bestNetworkAttempt = 0;
            int currNetworkLastImprovementEpoch = 0;
            double currNetworkLastImprovementCombinedPrecisionError = 0d;
            double currNetworkLastImprovementCombinedBinaryError = 0d;
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
                TrainedOneTakesAllNetwork currNetwork = new TrainedOneTakesAllNetwork
                {
                    NetworkName = _networkName,
                    Network = (FeedForwardNetwork)net,
                    TrainingErrorStat = net.ComputeBatchErrorStat(_trainingBundle.InputVectorCollection, _trainingBundle.OutputVectorCollection, out List<double[]> trainingComputedOutputsCollection)
                };
                currNetwork.TrainingBinErrorStat = new BinErrStat(Interval.IntZP1.Mid, trainingComputedOutputsCollection, _trainingBundle.OutputVectorCollection);
                currNetwork.CombinedBinaryError = currNetwork.TrainingBinErrorStat.TotalErrStat.Sum;
                currNetwork.CombinedPrecisionError = currNetwork.TrainingErrorStat.ArithAvg;
                //Testing data part
                currNetwork.TestingErrorStat = net.ComputeBatchErrorStat(_testingBundle.InputVectorCollection, _testingBundle.OutputVectorCollection, out List<double[]> testingComputedOutputsCollection);
                currNetwork.CombinedPrecisionError = Math.Max(currNetwork.CombinedPrecisionError, currNetwork.TestingErrorStat.ArithAvg);
                currNetwork.TestingBinErrorStat = new BinErrStat(Interval.IntZP1.Mid, testingComputedOutputsCollection, _testingBundle.OutputVectorCollection);
                currNetwork.CombinedBinaryError = Math.Max(currNetwork.CombinedBinaryError, currNetwork.TestingBinErrorStat.TotalErrStat.Sum);
                //Restart lastImprovementEpoch when new trainer's attempt started
                if (trainer.AttemptEpoch == 1)
                {
                    currNetworkLastImprovementEpoch = trainer.AttemptEpoch;
                    currNetworkLastImprovementCombinedPrecisionError = currNetwork.CombinedPrecisionError;
                    currNetworkLastImprovementCombinedBinaryError = currNetwork.CombinedBinaryError;
                }
                //First initialization of the best network
                if(bestNetwork == null)
                {
                    bestNetwork = currNetwork.DeepClone();
                    bestNetworkAttempt = trainer.Attempt;
                }
                //RegrState instance
                BuildingState regrState = new BuildingState(_networkName, _foldNum, _numOfFolds, _foldNetworkNum, _numOfFoldNetworks, trainer.Attempt, trainer.MaxAttempt, trainer.AttemptEpoch, trainer.MaxAttemptEpoch, currNetwork, currNetworkLastImprovementEpoch, bestNetwork, bestNetworkAttempt);
                //Call controller
                BuildingInstr instructions = _controller(regrState);
                //Better?
                if (instructions.CurrentIsBetter)
                {
                    //Adopt current regression unit as a best one
                    bestNetwork = currNetwork.DeepClone();
                    regrState.BestNetwork = bestNetwork;
                    bestNetworkAttempt = trainer.Attempt;
                }
                if (currNetwork.CombinedBinaryError < currNetworkLastImprovementCombinedBinaryError || currNetwork.CombinedPrecisionError < currNetworkLastImprovementCombinedPrecisionError)
                {
                    currNetworkLastImprovementEpoch = trainer.AttemptEpoch;
                    currNetworkLastImprovementCombinedPrecisionError = currNetwork.CombinedPrecisionError;
                    currNetworkLastImprovementCombinedBinaryError = currNetwork.CombinedBinaryError;
                }
                //Raise notification event
                RegressionEpochDone?.Invoke(regrState, instructions.CurrentIsBetter);
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
            public TrainedOneTakesAllNetwork CurrNetwork { get; }
            /// <summary>
            /// Specifies when was lastly found an improvement of current network within the current attempt
            /// </summary>
            public int CurrNetworkLastImprovementEpoch { get; set; }
            /// <summary>
            /// Contains the best network for now and related important error statistics.
            /// </summary>
            public TrainedOneTakesAllNetwork BestNetwork { get; set; }
            /// <summary>
            /// Number of attempt in which was recognized the best network
            /// </summary>
            public int BestNetworkAttempt { get; set; }

            /// <summary>
            /// Creates an initialized instance
            /// </summary>
            /// <param name="networkName">Name of the network</param>
            /// <param name="foldNum">Current fold number</param>
            /// <param name="numOfFolds">Total number of the folds</param>
            /// <param name="foldNetworkNum">Current fold network number</param>
            /// <param name="numOfFoldNetworks">Total number of the fold networks</param>
            /// <param name="regrAttemptNumber">Current regression attempt number</param>
            /// <param name="regrMaxAttempts">Maximum number of regression attempts</param>
            /// <param name="epoch">Current epoch number within the current regression attempt</param>
            /// <param name="maxEpochs">Maximum number of epochs</param>
            /// <param name="currNetwork">Current network and related important error statistics.</param>
            /// <param name="currNetworkLastImprovementEpoch">Specifies when was lastly found an improvement of current network within the current attempt.</param>
            /// <param name="bestNetwork">The best network for now and related important error statistics.</param>
            /// <param name="bestNetworkAttempt">Number of attempt in which was recognized the best network.</param>
            public BuildingState(string networkName,
                                 int foldNum,
                                 int numOfFolds,
                                 int foldNetworkNum,
                                 int numOfFoldNetworks,
                                 int regrAttemptNumber,
                                 int regrMaxAttempts,
                                 int epoch,
                                 int maxEpochs,
                                 TrainedOneTakesAllNetwork currNetwork,
                                 int currNetworkLastImprovementEpoch,
                                 TrainedOneTakesAllNetwork bestNetwork,
                                 int bestNetworkAttempt
                                 )
            {
                NetworkName = networkName;
                FoldNum = foldNum;
                NumOfFolds = numOfFolds;
                FoldNetworkNum = foldNetworkNum;
                NumOfFoldNetworks = numOfFoldNetworks;
                RegrAttemptNumber = regrAttemptNumber;
                RegrMaxAttempts = regrMaxAttempts;
                Epoch = epoch;
                MaxEpochs = maxEpochs;
                CurrNetwork = currNetwork;
                CurrNetworkLastImprovementEpoch = currNetworkLastImprovementEpoch;
                BestNetwork = bestNetwork;
                BestNetworkAttempt = bestNetworkAttempt;
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
                progressText.Append("Network [");
                progressText.Append(NetworkName);
                progressText.Append("] Fold/Net/Attempt/Epoch: ");
                progressText.Append(FoldNum.ToString().PadLeft(NumOfFolds.ToString().Length, '0') + "/");
                progressText.Append(FoldNetworkNum.ToString().PadLeft(NumOfFoldNetworks.ToString().Length, '0') + "/");
                progressText.Append(RegrAttemptNumber.ToString().PadLeft(RegrMaxAttempts.ToString().Length, '0') + "/");
                progressText.Append(Epoch.ToString().PadLeft(MaxEpochs.ToString().Length, '0'));
                progressText.Append(", Samples: ");
                progressText.Append((CurrNetwork.TrainingErrorStat.NumOfSamples / CurrNetwork.Network.NumOfOutputValues).ToString() + "/");
                progressText.Append((CurrNetwork.TestingErrorStat.NumOfSamples / CurrNetwork.Network.NumOfOutputValues).ToString());
                progressText.Append(", Best-Train: ");
                progressText.Append(BestNetwork.TrainingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture));
                progressText.Append("/" + BestNetwork.TrainingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                progressText.Append("/" + BestNetwork.TrainingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
                progressText.Append(", Best-Test: ");
                progressText.Append(BestNetwork.TestingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture));
                progressText.Append("/" + BestNetwork.TestingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                progressText.Append("/" + BestNetwork.TestingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
                progressText.Append(", Curr-Train: ");
                progressText.Append(CurrNetwork.TrainingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture));
                progressText.Append("/" + CurrNetwork.TrainingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                progressText.Append("/" + CurrNetwork.TrainingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
                progressText.Append(", Curr-Test: ");
                progressText.Append(CurrNetwork.TestingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture));
                progressText.Append("/" + CurrNetwork.TestingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                progressText.Append("/" + CurrNetwork.TestingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
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

    }//TrainedOneTakesAllNetworkBuilder

}//Namespace
