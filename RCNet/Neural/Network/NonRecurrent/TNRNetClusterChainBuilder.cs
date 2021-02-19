using RCNet.Extensions;
using RCNet.MiscTools;
using RCNet.Neural.Data;
using RCNet.Neural.Data.Filter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Implements the builder of the chain of cooperating non-recurrent network clusters.
    /// </summary>
    public class TNRNetClusterChainBuilder
    {
        //Constants
        private const int NetScopeDelimiterCoeff = 1000000;

        //Events
        /// <summary>
        /// This informative event occurs each time the progress of the build process takes a step forward.
        /// </summary>
        public event ChainBuildProgressChangedHandler ChainBuildProgressChanged;

        /// <summary>
        /// The delegate of the ChainBuildProgressChanged event handler.
        /// </summary>
        /// <param name="buildProgress">The current state of the build process.</param>
        public delegate void ChainBuildProgressChangedHandler(BuildProgress buildProgress);

        //Attributes
        private readonly string _chainName;
        private readonly ITNRNetClusterChainSettings _clusterChainCfg;
        private readonly Random _rand;
        private readonly TNRNetBuilder.BuildControllerDelegate _controller;

        //Progress tracking attributes
        private int _repetitionIdx;
        private int _clusterIdx;
        private int _numOfFoldsPerRepetition;
        private int _testingFoldIdx;
        private int _netCfgIdx;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="chainName">The name of the cluster chain.</param>
        /// <param name="clusterChainCfg">The configuration of the cluster chain.</param>
        /// <param name="rand">The random generator to be used (optional).</param>
        /// <param name="controller">The network build process controller (optional).</param>
        public TNRNetClusterChainBuilder(string chainName,
                                         ITNRNetClusterChainSettings clusterChainCfg,
                                         Random rand = null,
                                         TNRNetBuilder.BuildControllerDelegate controller = null
                                         )
        {
            _chainName = chainName;
            _clusterChainCfg = clusterChainCfg;
            _rand = rand ?? new Random(0);
            _controller = controller;
            ResetProgressTracking();
            return;
        }

        //Methods
        private void ResetProgressTracking()
        {
            _repetitionIdx = 0;
            _clusterIdx = 0;
            _numOfFoldsPerRepetition = 0;
            _testingFoldIdx = 0;
            _netCfgIdx = 0;
            return;
        }

        private void OnNetworkBuildProgressChanged(TNRNetBuilder.BuildProgress netBuildProgress)
        {
            //Prepare chain version
            BuildProgress buildProgress = new BuildProgress(_chainName,
                                                            Math.Min(_repetitionIdx + 1, _clusterChainCfg.CrossvalidationCfg.Repetitions),
                                                            _clusterChainCfg.CrossvalidationCfg.Repetitions,
                                                            Math.Min(_clusterIdx + 1, _clusterChainCfg.ClusterCfgCollection.Count),
                                                            _clusterChainCfg.ClusterCfgCollection.Count,
                                                            Math.Min(_testingFoldIdx + 1, _numOfFoldsPerRepetition),
                                                            _numOfFoldsPerRepetition,
                                                            Math.Min(_netCfgIdx + 1, _clusterChainCfg.ClusterCfgCollection[_clusterIdx].ClusterNetConfigurations.Count),
                                                            _clusterChainCfg.ClusterCfgCollection[_clusterIdx].ClusterNetConfigurations.Count,
                                                            netBuildProgress
                                                            );
            //Raise event
            ChainBuildProgressChanged?.Invoke(buildProgress);
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
            ResetProgressTracking();
            for (_repetitionIdx = 0; _repetitionIdx < _clusterChainCfg.CrossvalidationCfg.Repetitions; _repetitionIdx++)
            {
                //Split data to folds
                List<VectorBundle> foldCollection = localDataBundle.Folderize(_clusterChainCfg.CrossvalidationCfg.FoldDataRatio, boolBorder);
                _numOfFoldsPerRepetition = Math.Min(_clusterChainCfg.CrossvalidationCfg.Folds <= 0 ? foldCollection.Count : _clusterChainCfg.CrossvalidationCfg.Folds, foldCollection.Count);

                List<VectorBundle> currentClusterFoldCollection = CopyFolds(foldCollection);
                List<VectorBundle> nextClusterFoldCollection = new List<VectorBundle>(foldCollection.Count);
                //For each cluster
                for (_clusterIdx = 0; _clusterIdx < chainClusters.Count; _clusterIdx++)
                {
                    //Train networks for each testing fold.
                    for (_testingFoldIdx = 0; _testingFoldIdx < _numOfFoldsPerRepetition; _testingFoldIdx++)
                    {
                        //Prepare training data bundle
                        VectorBundle trainingData = new VectorBundle();
                        for (int foldIdx = 0; foldIdx < currentClusterFoldCollection.Count; foldIdx++)
                        {
                            if (foldIdx != _testingFoldIdx)
                            {
                                trainingData.Add(currentClusterFoldCollection[foldIdx]);
                            }
                        }
                        VectorBundle nextClusterUpdatedDataFold = foldCollection[_testingFoldIdx].CreateShallowCopy();
                        for (_netCfgIdx = 0; _netCfgIdx < _clusterChainCfg.ClusterCfgCollection[_clusterIdx].ClusterNetConfigurations.Count; _netCfgIdx++)
                        {
                            TNRNetBuilder netBuilder = new TNRNetBuilder(_chainName,
                                                                         _clusterChainCfg.ClusterCfgCollection[_clusterIdx].ClusterNetConfigurations[_netCfgIdx],
                                                                         _clusterChainCfg.ClusterCfgCollection[_clusterIdx].Output,
                                                                         trainingData,
                                                                         currentClusterFoldCollection[_testingFoldIdx],
                                                                         _rand,
                                                                         _controller
                                                                         );
                            //Register notification
                            netBuilder.NetworkBuildProgressChanged += OnNetworkBuildProgressChanged;
                            //Build trained network. Trained network becomes to be the cluster member
                            TNRNet tn = netBuilder.Build();
                            int netScopeID = _repetitionIdx * NetScopeDelimiterCoeff + _testingFoldIdx;
                            chainClusters[_clusterIdx].AddMember(tn, netScopeID, currentClusterFoldCollection[_testingFoldIdx], filters);
                            //Update input data in the data fold for the next cluster
                            for (int sampleIdx = 0; sampleIdx < currentClusterFoldCollection[_testingFoldIdx].InputVectorCollection.Count; sampleIdx++)
                            {
                                double[] computedNetData = tn.Network.Compute(currentClusterFoldCollection[_testingFoldIdx].InputVectorCollection[sampleIdx]);
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
                if (_repetitionIdx < _clusterChainCfg.CrossvalidationCfg.Repetitions - 1)
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

        //Inner classes
        /// <summary>
        /// Implements the holder of the cluster chain build progress information.
        /// </summary>
        public class BuildProgress : IBuildProgress
        {
            //Attribute properties
            /// <summary>
            /// Name of the cluster chain.
            /// </summary>
            public string ChainName { get; }

            /// <summary>
            /// Information about the folds processing repetitions progress.
            /// </summary>
            public ProgressTracker RepetitionsTracker { get; }

            /// <summary>
            /// Information about the clusters processing progress.
            /// </summary>
            public ProgressTracker ClustersTracker { get; }

            /// <summary>
            /// Information about the folds processing progress.
            /// </summary>
            public ProgressTracker FoldsTracker { get; }

            /// <summary>
            /// Information about the fold networks processing progress.
            /// </summary>
            public ProgressTracker FoldNetworksTracker { get; }

            /// <summary>
            /// Information about the network build progress.
            /// </summary>
            public TNRNetBuilder.BuildProgress NetBuildProgress { get; }

            /// <summary>
            /// Creates an initialized instance.
            /// </summary>
            /// <param name="chainName">Name of the cluster chain.</param>
            /// <param name="repetitionNum">The current folds processing repetition number.</param>
            /// <param name="maxNumOfRepetitions">The maximum number of folds processing repetitions.</param>
            /// <param name="clusterNum">The current cluster number within the repetition.</param>
            /// <param name="maxNumOfClusters">The maximum number of clusters within the repetition.</param>
            /// <param name="foldNum">The current fold number within the cluster processing.</param>
            /// <param name="maxNumOfFolds">The maximum number of folds within the cluster processing.</param>
            /// <param name="netNum">The current network number within the current fold processing.</param>
            /// <param name="maxNumOfNets">The maximum number of networks within the current fold processing.</param>
            /// <param name="netBuildProgress">The holder of the network build progress information.</param>
            public BuildProgress(string chainName,
                                 int repetitionNum,
                                 int maxNumOfRepetitions,
                                 int clusterNum,
                                 int maxNumOfClusters,
                                 int foldNum,
                                 int maxNumOfFolds,
                                 int netNum,
                                 int maxNumOfNets,
                                 TNRNetBuilder.BuildProgress netBuildProgress
                                 )
            {
                ChainName = chainName;
                RepetitionsTracker = new ProgressTracker((uint)maxNumOfRepetitions, (uint)repetitionNum);
                ClustersTracker = new ProgressTracker((uint)maxNumOfClusters, (uint)clusterNum);
                FoldsTracker = new ProgressTracker((uint)maxNumOfFolds, (uint)foldNum);
                FoldNetworksTracker = new ProgressTracker((uint)maxNumOfNets, (uint)netNum);
                NetBuildProgress = netBuildProgress;
                return;
            }

            //Properties
            /// <inheritdoc/>
            public bool NewEndNetwork
            {
                get
                {
                    return NetBuildProgress.NewEndNetwork;
                }
            }

            /// <inheritdoc/>
            public bool ShouldBeReported
            {
                get
                {
                    return NetBuildProgress.ShouldBeReported;
                }
            }

            /// <inheritdoc/>
            public int EndNetworkEpochNum
            {
                get
                {
                    return NetBuildProgress.EndNetworkEpochNum;
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
                    if (RepetitionsTracker.Target > 1)
                    {
                        text.Append($"Repetition {RepetitionsTracker.Current.ToString(CultureInfo.InvariantCulture).PadLeft(RepetitionsTracker.Target.ToString(CultureInfo.InvariantCulture).Length)}");
                        text.Append($", ");
                    }
                    if (ClustersTracker.Target > 1)
                    {
                        text.Append($"Cluster {ClustersTracker.Current.ToString(CultureInfo.InvariantCulture).PadLeft(ClustersTracker.Target.ToString(CultureInfo.InvariantCulture).Length)}");
                        text.Append($", ");
                    }
                    text.Append($"Fold {FoldsTracker.Current.ToString(CultureInfo.InvariantCulture).PadLeft(FoldsTracker.Target.ToString(CultureInfo.InvariantCulture).Length)}");
                    if (FoldNetworksTracker.Target > 1)
                    {
                        text.Append($", Net {FoldNetworksTracker.Current.ToString(CultureInfo.InvariantCulture).PadLeft(FoldNetworksTracker.Target.ToString(CultureInfo.InvariantCulture).Length)}");
                    }
                }
                else
                {
                    text.Append($"Repetition {RepetitionsTracker}");
                    text.Append($", Cluster {ClustersTracker}");
                    text.Append($", Fold {FoldsTracker}");
                    text.Append($", Net {FoldNetworksTracker}");
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
                    progressText.Append(ChainName);
                    progressText.Append("] ");
                }
                progressText.Append(GetBasicProgressInfoText(true));
                progressText.Append(", ");
                progressText.Append(NetBuildProgress.GetInfoText(0, false));
                return progressText.ToString();
            }

        }//BuildProgress



    }//TNRNetClusterChainBuilder

}//Namespace
