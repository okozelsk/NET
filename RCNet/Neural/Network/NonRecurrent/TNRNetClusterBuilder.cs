using RCNet.MiscTools;
using RCNet.Neural.Data;
using RCNet.Neural.Data.Filter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Implements the builder of trained non-recurrent networks cluster based on the cross-validation approach.
    /// </summary>
    public class TNRNetClusterBuilder
    {
        //Constants
        private const int NetScopeDelimiterCoeff = 1000000;

        //Events
        /// <summary>
        /// This informative event occurs each time the progress of the build process takes a step forward.
        /// </summary>
        public event ClusterBuildProgressChangedHandler ClusterBuildProgressChanged;

        /// <summary>
        /// The delegate of the ClusterBuildProgressChanged event handler.
        /// </summary>
        /// <param name="buildProgress">The current state of the build process.</param>
        public delegate void ClusterBuildProgressChangedHandler(BuildProgress buildProgress);

        //Attributes
        private readonly string _clusterName;
        private readonly CrossvalidationSettings _crossvalidationCfg;
        private readonly ITNRNetClusterSettings _clusterCfg;
        private readonly Random _rand;
        private readonly TNRNetBuilder.BuildControllerDelegate _controller;

        //Progress tracking attributes
        private int _numOfFoldsPerRepetition;
        private int _repetitionIdx;
        private int _testingFoldIdx;
        private int _netCfgIdx;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="clusterName">The name of the cluster to be built.</param>
        /// <param name="crossvalidationCfg">The crossvalidation configuration.</param>
        /// <param name="clusterCfg">The configuration of the cluster to be built.</param>
        /// <param name="rand">The random generator to be used (optional).</param>
        /// <param name="controller">The network build process controller (optional).</param>
        public TNRNetClusterBuilder(string clusterName,
                                    CrossvalidationSettings crossvalidationCfg,
                                    ITNRNetClusterSettings clusterCfg,
                                    Random rand = null,
                                    TNRNetBuilder.BuildControllerDelegate controller = null
                                    )
        {
            _clusterName = clusterName;
            _crossvalidationCfg = crossvalidationCfg;
            _clusterCfg = clusterCfg;
            _rand = rand ?? new Random(0);
            _controller = controller;
            ResetProgressTracking();
            return;
        }

        //Methods
        private void ResetProgressTracking()
        {
            _numOfFoldsPerRepetition = 0;
            _repetitionIdx = 0;
            _testingFoldIdx = 0;
            _netCfgIdx = 0;
            return;
        }

        private void OnNetworkBuildProgressChanged(TNRNetBuilder.BuildProgress netBuildProgress)
        {
            //Prepare cluster version
            BuildProgress buildProgress = new BuildProgress(_clusterName,
                                                            Math.Min(_repetitionIdx + 1, _crossvalidationCfg.Repetitions),
                                                            _crossvalidationCfg.Repetitions,
                                                            Math.Min(_testingFoldIdx + 1, _numOfFoldsPerRepetition),
                                                            _numOfFoldsPerRepetition,
                                                            Math.Min(_netCfgIdx + 1, _clusterCfg.ClusterNetConfigurations.Count),
                                                            _clusterCfg.ClusterNetConfigurations.Count,
                                                            netBuildProgress
                                                            );
            //Raise event
            ClusterBuildProgressChanged?.Invoke(buildProgress);
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
            ResetProgressTracking();
            for (_repetitionIdx = 0; _repetitionIdx < _crossvalidationCfg.Repetitions; _repetitionIdx++)
            {
                //Data split to folds
                List<VectorBundle> foldCollection = localDataBundle.Folderize(_crossvalidationCfg.FoldDataRatio, _clusterCfg.Output == TNRNet.OutputType.Real ? double.NaN : cluster.OutputDataRange.Mid);
                _numOfFoldsPerRepetition = Math.Min(_crossvalidationCfg.Folds <= 0 ? foldCollection.Count : _crossvalidationCfg.Folds, foldCollection.Count);
                //Train the collection of networks for each processing fold.
                for (_testingFoldIdx = 0; _testingFoldIdx < _numOfFoldsPerRepetition; _testingFoldIdx++)
                {
                    //Prepare training data bundle
                    VectorBundle trainingData = new VectorBundle();
                    for (int foldIdx = 0; foldIdx < foldCollection.Count; foldIdx++)
                    {
                        if (foldIdx != _testingFoldIdx)
                        {
                            trainingData.Add(foldCollection[foldIdx]);
                        }
                    }
                    for (_netCfgIdx = 0; _netCfgIdx < _clusterCfg.ClusterNetConfigurations.Count; _netCfgIdx++)
                    {
                        TNRNetBuilder netBuilder = new TNRNetBuilder(_clusterName,
                                                                     _clusterCfg.ClusterNetConfigurations[_netCfgIdx],
                                                                     _clusterCfg.Output,
                                                                     trainingData,
                                                                     foldCollection[_testingFoldIdx],
                                                                     _rand,
                                                                     _controller
                                                                     );
                        //Register notification
                        netBuilder.NetworkBuildProgressChanged += OnNetworkBuildProgressChanged;
                        //Build trained network. Trained network becomes to be the cluster member
                        TNRNet tn = netBuilder.Build();
                        //Build an unique network scope identifier
                        int netScopeID = _repetitionIdx * NetScopeDelimiterCoeff + _testingFoldIdx;
                        //Add trained network to a cluster
                        cluster.AddMember(tn, netScopeID, foldCollection[_testingFoldIdx], filters);
                    }//netCfgIdx
                }//testingFoldIdx
                if (_repetitionIdx < _crossvalidationCfg.Repetitions - 1)
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


        //Inner classes
        /// <summary>
        /// Implements the holder of the cluster build progress information.
        /// </summary>
        public class BuildProgress : IBuildProgress
        {
            //Attribute properties
            /// <summary>
            /// Name of the cluster.
            /// </summary>
            public string ClusterName { get; }

            /// <summary>
            /// Information about the folds processing repetitions progress.
            /// </summary>
            public ProgressTracker RepetitionsTracker { get; }

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
            /// <param name="clusterName">Name of the cluster.</param>
            /// <param name="repetitionNum">The current folds processing repetition number.</param>
            /// <param name="maxNumOfRepetitions">The maximum number of folds processing repetitions.</param>
            /// <param name="foldNum">The current fold number within the folds processing repetition.</param>
            /// <param name="maxNumOfFolds">The maximum number of folds within the folds processing repetition.</param>
            /// <param name="netNum">The current network number within the current fold processing.</param>
            /// <param name="maxNumOfNets">The maximum number of networks within the current fold processing.</param>
            /// <param name="netBuildProgress">The holder of the network build progress information.</param>
            public BuildProgress(string clusterName,
                                 int repetitionNum,
                                 int maxNumOfRepetitions,
                                 int foldNum,
                                 int maxNumOfFolds,
                                 int netNum,
                                 int maxNumOfNets,
                                 TNRNetBuilder.BuildProgress netBuildProgress
                                 )
            {
                ClusterName = clusterName;
                RepetitionsTracker = new ProgressTracker((uint)maxNumOfRepetitions, (uint)repetitionNum);
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
                    text.Append($"Fold {FoldsTracker.Current.ToString(CultureInfo.InvariantCulture).PadLeft(FoldsTracker.Target.ToString(CultureInfo.InvariantCulture).Length)}");
                    if (FoldNetworksTracker.Target > 1)
                    {
                        text.Append($", Net {FoldNetworksTracker.Current.ToString(CultureInfo.InvariantCulture).PadLeft(FoldNetworksTracker.Target.ToString(CultureInfo.InvariantCulture).Length)}");
                    }
                }
                else
                {
                    text.Append($"Repetition {RepetitionsTracker}");
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
                    progressText.Append(ClusterName);
                    progressText.Append("] ");
                }
                progressText.Append(GetBasicProgressInfoText(true));
                progressText.Append(", ");
                progressText.Append(NetBuildProgress.GetInfoText(0, false));
                return progressText.ToString();
            }

        }//BuildProgress




    }//TNRNetClusterBuilder

}//Namespace
