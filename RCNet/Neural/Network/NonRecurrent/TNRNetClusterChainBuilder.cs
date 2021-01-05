using RCNet.Extensions;
using RCNet.Neural.Data;
using RCNet.Neural.Data.Filter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Implements the builder of the chain of cooperating non-recurrent network clusters.
    /// </summary>
    public class TNRNetClusterChainBuilder
    {
        //Events
        /// <inheritdoc cref="TNRNetBuilder.EpochDone"/>
        public event TNRNetBuilder.EpochDoneHandler EpochDone;

        //Attributes
        private readonly string _buildContext;
        private readonly string _chainName;
        private readonly ITNRNetClusterChainSettings _clusterChainCfg;
        private readonly Random _rand;
        private readonly TNRNetBuilder.BuildControllerDelegate _controller;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="buildContext">The build context.</param>
        /// <param name="chainName">The name of the cluster chain.</param>
        /// <param name="clusterChainCfg">The configuration of the cluster chain.</param>
        /// <param name="rand">The random generator to be used (optional).</param>
        /// <param name="controller">The network build process controller (optional).</param>
        public TNRNetClusterChainBuilder(string buildContext,
                                         string chainName,
                                         ITNRNetClusterChainSettings clusterChainCfg,
                                         Random rand = null,
                                         TNRNetBuilder.BuildControllerDelegate controller = null
                                         )
        {
            _buildContext = buildContext;
            _chainName = chainName;
            _clusterChainCfg = clusterChainCfg;
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

        private List<VectorBundle> CopyFolds(List<VectorBundle> sourceFolds)
        {
            List<VectorBundle> copyFolds = new List<VectorBundle>(sourceFolds.Count);
            foreach (VectorBundle fold in sourceFolds)
            {
                copyFolds.Add(fold.CreateShallowCopy());
            }
            return copyFolds;
        }

        /// <summary>
        /// Builds the cluster chain.
        /// </summary>
        /// <param name="dataBundle">The data bundle for training.</param>
        /// <param name="filters">The filters to be used to denormalize outputs.</param>
        public TNRNetClusterChain Build(VectorBundle dataBundle, FeatureFilterBase[] filters)
        {
            //The chain to be built
            TNRNetClusterChain chain = new TNRNetClusterChain(_chainName, _clusterChainCfg.Output);
            //Instantiate chained clusters
            List<TNRNetCluster> chainClusters = new List<TNRNetCluster>(_clusterChainCfg.ClusterCfgCollection.Count);
            for (int clusterIdx = 0; clusterIdx < _clusterChainCfg.ClusterCfgCollection.Count; clusterIdx++)
            {
                //Cluster
                chainClusters.Add(new TNRNetCluster(_chainName,
                                                    _clusterChainCfg.ClusterCfgCollection[clusterIdx].Output,
                                                    _clusterChainCfg.ClusterCfgCollection[clusterIdx].TrainingGroupWeight,
                                                    _clusterChainCfg.ClusterCfgCollection[clusterIdx].TestingGroupWeight,
                                                    _clusterChainCfg.ClusterCfgCollection[clusterIdx].SamplesWeight,
                                                    _clusterChainCfg.ClusterCfgCollection[clusterIdx].NumericalPrecisionWeight,
                                                    _clusterChainCfg.ClusterCfgCollection[clusterIdx].MisrecognizedFalseWeight,
                                                    _clusterChainCfg.ClusterCfgCollection[clusterIdx].UnrecognizedTrueWeight
                                                    )
                                   );
            }
            //Common crossvalidation configuration
            double boolBorder = _clusterChainCfg.Output == TNRNet.OutputType.Real ? double.NaN : chain.OutputDataRange.Mid;

            VectorBundle localDataBundle = dataBundle.CreateShallowCopy();
            //Member's training
            for (int repetitionIdx = 0; repetitionIdx < _clusterChainCfg.CrossvalidationCfg.Repetitions; repetitionIdx++)
            {
                //Split data to folds
                List<VectorBundle> foldCollection = localDataBundle.Folderize(_clusterChainCfg.CrossvalidationCfg.FoldDataRatio, boolBorder);
                int numOfFoldsToBeProcessed = Math.Min(_clusterChainCfg.CrossvalidationCfg.Folds <= 0 ? foldCollection.Count : _clusterChainCfg.CrossvalidationCfg.Folds, foldCollection.Count);

                List<VectorBundle> currentClusterFoldCollection = CopyFolds(foldCollection);
                List<VectorBundle> nextClusterFoldCollection = new List<VectorBundle>(foldCollection.Count);
                //For each cluster
                for (int clusterIdx = 0; clusterIdx < chainClusters.Count; clusterIdx++)
                {
                    //Train networks for each testing fold.
                    for (int testingFoldIdx = 0; testingFoldIdx < numOfFoldsToBeProcessed; testingFoldIdx++)
                    {
                        //Prepare training data bundle
                        VectorBundle trainingData = new VectorBundle();
                        for (int foldIdx = 0; foldIdx < currentClusterFoldCollection.Count; foldIdx++)
                        {
                            if (foldIdx != testingFoldIdx)
                            {
                                trainingData.Add(currentClusterFoldCollection[foldIdx]);
                            }
                        }
                        VectorBundle nextClusterUpdatedDataFold = foldCollection[testingFoldIdx].CreateShallowCopy();
                        for (int netCfgIdx = 0; netCfgIdx < _clusterChainCfg.ClusterCfgCollection[clusterIdx].ClusterNetConfigurations.Count; netCfgIdx++)
                        {
                            TNRNetBuilder netBuilder = new TNRNetBuilder(_buildContext,
                                                                         _chainName + "#C" + (clusterIdx + 1).ToString(CultureInfo.InvariantCulture) + "R" + (repetitionIdx + 1).ToString(CultureInfo.InvariantCulture),
                                                                         _clusterChainCfg.ClusterCfgCollection[clusterIdx].ClusterNetConfigurations[netCfgIdx],
                                                                         _clusterChainCfg.ClusterCfgCollection[clusterIdx].Output,
                                                                         (repetitionIdx * numOfFoldsToBeProcessed) + testingFoldIdx + 1,
                                                                         _clusterChainCfg.CrossvalidationCfg.Repetitions * numOfFoldsToBeProcessed,
                                                                         netCfgIdx + 1,
                                                                         _clusterChainCfg.ClusterCfgCollection[clusterIdx].ClusterNetConfigurations.Count,
                                                                         trainingData,
                                                                         currentClusterFoldCollection[testingFoldIdx],
                                                                         _rand,
                                                                         _controller
                                                                         );
                            //Register notification
                            netBuilder.EpochDone += OnEpochDone;
                            //Build trained network. Trained network becomes to be the cluster member
                            TNRNet tn = netBuilder.Build();
                            int netScopeID = repetitionIdx * 1000000 + testingFoldIdx;
                            chainClusters[clusterIdx].AddMember(tn, netScopeID, currentClusterFoldCollection[testingFoldIdx], filters);
                            //Update input data in the data fold for the next cluster
                            for (int sampleIdx = 0; sampleIdx < currentClusterFoldCollection[testingFoldIdx].InputVectorCollection.Count; sampleIdx++)
                            {
                                double[] computedNetData = tn.Network.Compute(currentClusterFoldCollection[testingFoldIdx].InputVectorCollection[sampleIdx]);
                                nextClusterUpdatedDataFold.InputVectorCollection[sampleIdx] = nextClusterUpdatedDataFold.InputVectorCollection[sampleIdx].Concat(computedNetData);
                            }
                        }//netCfgIdx
                        //Add updated data fold for the next cluster
                        nextClusterFoldCollection.Add(nextClusterUpdatedDataFold);
                    }//testingFoldIdx
                    //Switch fold collection
                    currentClusterFoldCollection = nextClusterFoldCollection;
                    nextClusterFoldCollection = new List<VectorBundle>(currentClusterFoldCollection.Count);
                }//clusterIdx
                if (repetitionIdx < _clusterChainCfg.CrossvalidationCfg.Repetitions - 1)
                {
                    //Reshuffle the data
                    localDataBundle.Shuffle(_rand);
                }
            }//repetitionIdx
            //Make the clusters operable and add them into the chain
            for (int clusterIdx = 0; clusterIdx < chainClusters.Count; clusterIdx++)
            {
                chainClusters[clusterIdx].FinalizeCluster();
                chain.AddCluster(chainClusters[clusterIdx]);
            }
            //Return the built chain
            return chain;
        }

    }//TNRNetClusterChainBuilder

}//Namespace
