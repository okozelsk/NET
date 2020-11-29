using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Data;
using RCNet.Neural.Data.Filter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Builds computation cluster of trained networks.
    /// Supported is only single output.
    /// </summary>
    public class TrainedNetworkClusterBuilder
    {
        //Events
        /// <summary>
        /// This informative event occurs every time the regression epoch is done
        /// </summary>
        [field: NonSerialized]
        public event TrainedNetworkBuilder.RegressionEpochDoneHandler RegressionEpochDone;

        //Attributes
        private readonly string _clusterName;
        private readonly List<INonRecurrentNetworkSettings> _networkSettingsCollection;
        private readonly double _binBorder;
        private readonly Random _rand;
        private readonly TrainedNetworkBuilder.RegressionControllerDelegate _controller;
        private readonly Interval _dataRange;
        private readonly NetworkClusterSecondLevelCompSettings _secondLevelCompCfg;

        //Constructor
        /// <summary>
        /// Creates an instance ready to build computation cluster
        /// </summary>
        /// <param name="clusterName">Name of the cluster</param>
        /// <param name="networkSettingsCollection">Collection of network configurations (FeedForwardNetworkSettings or ParallelPerceptronSettings objects)</param>
        /// <param name="dataRange">Range of input and output data</param>
        /// <param name="binBorder">If specified, it indicates that the network ideal output is binary and specifies numeric border where GE network output is decided as a 1 and LT output as a 0.</param>
        /// <param name="rand">Random generator to be used (optional)</param>
        /// <param name="controller">Regression controller (optional)</param>
        /// <param name="secondLevelCompCfg">Configuration of the network cluster 2nd level computation (optional)</param>
        public TrainedNetworkClusterBuilder(string clusterName,
                                            List<INonRecurrentNetworkSettings> networkSettingsCollection,
                                            Interval dataRange,
                                            double binBorder = double.NaN,
                                            Random rand = null,
                                            TrainedNetworkBuilder.RegressionControllerDelegate controller = null,
                                            NetworkClusterSecondLevelCompSettings secondLevelCompCfg = null
                                            )
        {
            _clusterName = clusterName;
            _networkSettingsCollection = networkSettingsCollection;
            _dataRange = dataRange;
            _binBorder = binBorder;
            _rand = rand ?? new Random(0);
            _controller = controller;
            _secondLevelCompCfg = secondLevelCompCfg;
            return;
        }

        //Properties
        /// <summary>
        /// Indicates that the cluster ideal output is binary
        /// </summary>
        public bool BinaryOutput { get { return !double.IsNaN(_binBorder); } }

        //Methods
        private void OnRegressionEpochDone(TrainedNetworkBuilder.BuildingState buildingState, bool foundBetter)
        {
            //Only raise up
            RegressionEpochDone(buildingState, foundBetter);
            return;
        }

        /// <summary>
        /// Builds computation cluster of trained networks
        /// </summary>
        /// <param name="dataBundle">Data to be used for training. Take into the account that rows in this bundle may be in random order after the Build call.</param>
        /// <param name="crossvalidationCfg">Crossvalidation configuration</param>
        /// <param name="outputFeatureFilter">Output feature filter to be used for output data denormalization.</param>
        public TrainedNetworkCluster Build(VectorBundle dataBundle,
                                           CrossvalidationSettings crossvalidationCfg,
                                           FeatureFilterBase outputFeatureFilter
                                           )
        {
            //Cluster of trained networks
            TrainedNetworkCluster cluster = new TrainedNetworkCluster(_clusterName, _dataRange, _binBorder, _secondLevelCompCfg);
            //Member's training
            for (int repetitionIdx = 0; repetitionIdx < crossvalidationCfg.Repetitions; repetitionIdx++)
            {
                //Data split to folds
                List<VectorBundle> subBundleCollection = dataBundle.CreateFolds(crossvalidationCfg.FoldDataRatio, _binBorder);
                int numOfFoldsToBeProcessed = Math.Min(crossvalidationCfg.Folds <= 0 ? subBundleCollection.Count : crossvalidationCfg.Folds, subBundleCollection.Count);
                //Train collection of networks for each processing fold.
                for (int foldIdx = 0; foldIdx < numOfFoldsToBeProcessed; foldIdx++)
                {
                    for (int netCfgIdx = 0; netCfgIdx < _networkSettingsCollection.Count; netCfgIdx++)
                    {
                        //Prepare training data bundle
                        VectorBundle trainingData = new VectorBundle();
                        for (int bundleIdx = 0; bundleIdx < subBundleCollection.Count; bundleIdx++)
                        {
                            if (bundleIdx != foldIdx)
                            {
                                trainingData.Add(subBundleCollection[bundleIdx]);
                            }
                        }
                        TrainedNetworkBuilder netBuilder = new TrainedNetworkBuilder(_clusterName,
                                                                                     _networkSettingsCollection[netCfgIdx],
                                                                                     (repetitionIdx * numOfFoldsToBeProcessed) + foldIdx + 1,
                                                                                     crossvalidationCfg.Repetitions * numOfFoldsToBeProcessed,
                                                                                     netCfgIdx + 1,
                                                                                     _networkSettingsCollection.Count,
                                                                                     trainingData,
                                                                                     subBundleCollection[foldIdx],
                                                                                     _binBorder,
                                                                                     _rand,
                                                                                     _controller
                                                                                     );
                        //Register notification
                        netBuilder.RegressionEpochDone += OnRegressionEpochDone;
                        //Build trained network. Trained network becomes to be the cluster member
                        TrainedNetwork tn = netBuilder.Build();
                        cluster.AddMember(tn, subBundleCollection[foldIdx], outputFeatureFilter);
                    }//netCfgIdx
                }//foldIdx
                if (repetitionIdx < crossvalidationCfg.Repetitions - 1)
                {
                    //Reshuffle the data
                    dataBundle.Shuffle(_rand);
                }
            }//repetitionIdx
            //Make the cluster operable
            //Register notification
            cluster.RegressionEpochDone += OnRegressionEpochDone;
            cluster.FinalizeCluster(dataBundle);
            //Return the built cluster
            return cluster;
        }

    }//TrainedNetworkClusterBuilder

}//Namespace
