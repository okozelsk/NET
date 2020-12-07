using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Data;
using RCNet.Neural.Data.Filter;
using RCNet.Neural.Network.NonRecurrent.FF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Builds computation cluster of trained "One Takes All" networks.
    /// </summary>
    public class TrainedOneTakesAllNetworkClusterBuilder
    {
        //Events
        /// <summary>
        /// This informative event occurs every time the regression epoch is done
        /// </summary>
        [field: NonSerialized]
        public event TrainedOneTakesAllNetworkBuilder.RegressionEpochDoneHandler RegressionEpochDone;

        //Attributes
        private readonly string _clusterName;
        private readonly List<FeedForwardNetworkSettings> _networkSettingsCollection;
        private readonly Random _rand;
        private readonly TrainedOneTakesAllNetworkBuilder.RegressionControllerDelegate _controller;

        //Constructor
        /// <summary>
        /// Creates an instance ready to build probabilistic computation cluster
        /// </summary>
        /// <param name="clusterName">Name of the cluster</param>
        /// <param name="networkSettingsCollection">Collection of network configurations (FeedForwardNetworkSettings)</param>
        /// <param name="rand">Random generator to be used (optional)</param>
        /// <param name="controller">Regression controller (optional)</param>
        public TrainedOneTakesAllNetworkClusterBuilder(string clusterName,
                                                       List<FeedForwardNetworkSettings> networkSettingsCollection,
                                                       Random rand = null,
                                                       TrainedOneTakesAllNetworkBuilder.RegressionControllerDelegate controller = null
                                                       )
        {
            _clusterName = clusterName;
            _networkSettingsCollection = networkSettingsCollection;
            _rand = rand ?? new Random(0);
            _controller = controller;
            return;
        }

        //Properties

        //Methods
        private void OnRegressionEpochDone(TrainedOneTakesAllNetworkBuilder.BuildingState buildingState, bool foundBetter)
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
        public TrainedOneTakesAllNetworkCluster Build(VectorBundle dataBundle,
                                                      CrossvalidationSettings crossvalidationCfg
                                                      )
        {
            //Cluster of trained networks
            TrainedOneTakesAllNetworkCluster cluster = new TrainedOneTakesAllNetworkCluster(_clusterName);
            //Member's training
            for (int repetitionIdx = 0; repetitionIdx < crossvalidationCfg.Repetitions; repetitionIdx++)
            {
                //Data split to folds
                List<VectorBundle> subBundleCollection = dataBundle.Folderize(crossvalidationCfg.FoldDataRatio, Interval.IntZP1.Mid);
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
                        TrainedOneTakesAllNetworkBuilder netBuilder = new TrainedOneTakesAllNetworkBuilder(_clusterName,
                                                                                                           _networkSettingsCollection[netCfgIdx],
                                                                                                           (repetitionIdx * numOfFoldsToBeProcessed) + foldIdx + 1,
                                                                                                           crossvalidationCfg.Repetitions * numOfFoldsToBeProcessed,
                                                                                                           netCfgIdx + 1,
                                                                                                           _networkSettingsCollection.Count,
                                                                                                           trainingData,
                                                                                                           subBundleCollection[foldIdx],
                                                                                                           _rand,
                                                                                                           _controller
                                                                                                           );
                        //Register notification
                        netBuilder.RegressionEpochDone += OnRegressionEpochDone;
                        //Build trained network. Trained network becomes to be the cluster member
                        TrainedOneTakesAllNetwork tn = netBuilder.Build();
                        cluster.AddMember(tn, subBundleCollection[foldIdx]);
                    }//netCfgIdx
                }//foldIdx
                if (repetitionIdx < crossvalidationCfg.Repetitions - 1)
                {
                    //Reshuffle the data
                    dataBundle.Shuffle(_rand);
                }
            }//repetitionIdx
            //Make the cluster operable
            cluster.FinalizeCluster(dataBundle);
            //Return the built cluster
            return cluster;
        }

    }//TrainedOneTakesAllNetworkClusterBuilder

}//Namespace
