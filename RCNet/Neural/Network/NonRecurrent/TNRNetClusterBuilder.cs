using RCNet.Neural.Data;
using RCNet.Neural.Data.Filter;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Implements the builder of the cluster of the trained non-recurrent networks.
    /// </summary>
    public class TNRNetClusterBuilder
    {
        //Events
        /// <inheritdoc cref="TNRNetBuilder.EpochDone"/>
        public event TNRNetBuilder.EpochDoneHandler EpochDone;

        //Attributes
        private readonly string _buildContext;
        private readonly string _clusterName;
        private readonly CrossvalidationSettings _crossvalidationCfg;
        private readonly ITNRNetClusterSettings _clusterCfg;
        private readonly Random _rand;
        private readonly TNRNetBuilder.BuildControllerDelegate _controller;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="buildContext">The build context.</param>
        /// <param name="clusterName">The name of the cluster to be built.</param>
        /// <param name="crossvalidationCfg">The crossvalidation configuration.</param>
        /// <param name="clusterCfg">The configuration of the cluster to be built.</param>
        /// <param name="rand">The random generator to be used (optional).</param>
        /// <param name="controller">The network build process controller (optional).</param>
        public TNRNetClusterBuilder(string buildContext,
                                    string clusterName,
                                    CrossvalidationSettings crossvalidationCfg,
                                    ITNRNetClusterSettings clusterCfg,
                                    Random rand = null,
                                    TNRNetBuilder.BuildControllerDelegate controller = null
                                    )
        {
            _buildContext = buildContext;
            _clusterName = clusterName;
            _crossvalidationCfg = crossvalidationCfg;
            _clusterCfg = clusterCfg;
            _rand = rand ?? new Random(0);
            _controller = controller;
            return;
        }

        //Methods
        private void OnEpochDone(TNRNetBuilder.BuildProgress buildingProgress, bool foundBetter)
        {
            //Only raise up
            EpochDone(buildingProgress, foundBetter);
            return;
        }

        /// <summary>
        /// Builds the cluster.
        /// </summary>
        /// <param name="dataBundle">The data bundle for training.</param>
        /// <param name="filters">The filters to be used to denormalize outputs.</param>
        public TNRNetCluster Build(VectorBundle dataBundle, FeatureFilterBase[] filters)
        {
            VectorBundle localDataBundle = dataBundle.CreateShallowCopy();
            //Cluster of trained networks
            TNRNetCluster cluster = new TNRNetCluster(_clusterName,
                                                      _clusterCfg.Output,
                                                      _clusterCfg.TrainingGroupWeight,
                                                      _clusterCfg.TestingGroupWeight,
                                                      _clusterCfg.SamplesWeight,
                                                      _clusterCfg.NumericalPrecisionWeight,
                                                      _clusterCfg.MisrecognizedFalseWeight,
                                                      _clusterCfg.UnrecognizedTrueWeight
                                                      );
            //Member's training
            for (int repetitionIdx = 0; repetitionIdx < _crossvalidationCfg.Repetitions; repetitionIdx++)
            {
                //Data split to folds
                List<VectorBundle> foldCollection = localDataBundle.Folderize(_crossvalidationCfg.FoldDataRatio, _clusterCfg.Output == TNRNet.OutputType.Real ? double.NaN : cluster.OutputDataRange.Mid);
                int numOfFoldsToBeProcessed = Math.Min(_crossvalidationCfg.Folds <= 0 ? foldCollection.Count : _crossvalidationCfg.Folds, foldCollection.Count);
                //Train the collection of networks for each processing fold.
                for (int testingFoldIdx = 0; testingFoldIdx < numOfFoldsToBeProcessed; testingFoldIdx++)
                {
                    //Prepare training data bundle
                    VectorBundle trainingData = new VectorBundle();
                    for (int foldIdx = 0; foldIdx < foldCollection.Count; foldIdx++)
                    {
                        if (foldIdx != testingFoldIdx)
                        {
                            trainingData.Add(foldCollection[foldIdx]);
                        }
                    }
                    for (int netCfgIdx = 0; netCfgIdx < _clusterCfg.ClusterNetConfigurations.Count; netCfgIdx++)
                    {
                        TNRNetBuilder netBuilder = new TNRNetBuilder(_buildContext,
                                                                     _clusterName + "#R" + (repetitionIdx + 1).ToString(CultureInfo.InvariantCulture),
                                                                     _clusterCfg.ClusterNetConfigurations[netCfgIdx],
                                                                     _clusterCfg.Output,
                                                                     (repetitionIdx * numOfFoldsToBeProcessed) + testingFoldIdx + 1,
                                                                     _crossvalidationCfg.Repetitions * numOfFoldsToBeProcessed,
                                                                     netCfgIdx + 1,
                                                                     _clusterCfg.ClusterNetConfigurations.Count,
                                                                     trainingData,
                                                                     foldCollection[testingFoldIdx],
                                                                     _rand,
                                                                     _controller
                                                                     );
                        //Register notification
                        netBuilder.EpochDone += OnEpochDone;
                        //Build trained network. Trained network becomes to be the cluster member
                        TNRNet tn = netBuilder.Build();
                        int netScopeID = repetitionIdx * 1000000 + testingFoldIdx;
                        cluster.AddMember(tn, netScopeID, foldCollection[testingFoldIdx], filters);
                    }//netCfgIdx
                }//testingFoldIdx
                if (repetitionIdx < _crossvalidationCfg.Repetitions - 1)
                {
                    //Reshuffle the data
                    localDataBundle.Shuffle(_rand);
                }
            }//repetitionIdx
            //Make the cluster operable
            cluster.FinalizeCluster();
            //Return the built cluster
            return cluster;
        }

    }//TNRNetClusterBuilder

}//Namespace
