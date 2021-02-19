using RCNet.Neural.Data;
using RCNet.Neural.Data.Filter;
using RCNet.Neural.Network.NonRecurrent;
using System;
using System.Collections.Generic;
using System.Text;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Implements the readout unit of the readout layer.
    /// </summary>
    [Serializable]
    public class ReadoutUnit
    {
        //Enums
        /// <summary>
        /// The type of task.
        /// </summary>
        public enum TaskType
        {
            /// <summary>
            /// The forecast task.
            /// </summary>
            Forecast,
            /// <summary>
            /// The classification task.
            /// </summary>
            Classification
        }

        /// <summary>
        /// This informative event occurs each time the progress of the build process takes a step forward.
        /// </summary>
        [field: NonSerialized]
        public event ReadoutUnitBuildProgressChangedHandler ReadoutUnitBuildProgressChanged;

        /// <summary>
        /// The delegate of the ReadoutUnitBuildProgressChanged event handler.
        /// </summary>
        /// <param name="buildProgress">The current state of the build process.</param>
        public delegate void ReadoutUnitBuildProgressChangedHandler(BuildProgress buildProgress);

        //Attribute properties
        /// <summary>
        /// An index of this readout unit within the readout layer.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// The name of this readout unit.
        /// </summary>
        public string Name { get; }

        /// <inheritdoc cref="TaskType"/>
        public TaskType Task;

        //Attributes
        private readonly ITNRNetClusterChainSettings _clusterChainCfg;
        private TNRNetClusterChain _clusterChain;

        //Constructors
        /// <summary>
        /// Creates an itialized instance.
        /// </summary>
        /// <param name="index">An index of this readout unit within the readout layer.</param>
        /// <param name="readoutUnitCfg">The configuration of the readout unit.</param>
        /// <param name="taskDefaultsCfg">The tasks defaults configuration.</param>
        public ReadoutUnit(int index,
                           ReadoutUnitSettings readoutUnitCfg,
                           TaskDefaultsSettings taskDefaultsCfg
                           )
        {
            Index = index;
            Name = readoutUnitCfg.Name;
            Task = readoutUnitCfg.TaskCfg.Type;
            if (readoutUnitCfg.TaskCfg.GetType() == typeof(ForecastTaskSettings))
            {
                _clusterChainCfg = (ITNRNetClusterChainSettings)((ForecastTaskSettings)readoutUnitCfg.TaskCfg).ClusterChainCfg?.DeepClone();
                if (_clusterChainCfg == null)
                {
                    _clusterChainCfg = taskDefaultsCfg.ForecastClusterChainCfg;
                }
            }
            else
            {
                _clusterChainCfg = (ITNRNetClusterChainSettings)((ClassificationTaskSettings)readoutUnitCfg.TaskCfg).ClusterChainCfg?.DeepClone();
                {
                    if (_clusterChainCfg == null)
                    {
                        _clusterChainCfg = taskDefaultsCfg.ClassificationClusterChainCfg;
                    }
                }
            }
            _clusterChain = null;
            return;
        }

        //Properties
        /// <summary>
        /// Indicates the readiness to operate.
        /// </summary>
        public bool Ready { get { return _clusterChain != null; } }

        //Methods
        private void OnChainBuildProgressChanged(TNRNetClusterChainBuilder.BuildProgress chainBuildProgress)
        {
            //Prepare readout unit version
            BuildProgress buildProgress = new BuildProgress(Name, chainBuildProgress);
            //Raise event
            ReadoutUnitBuildProgressChanged?.Invoke(buildProgress);
            return;
        }

        /// <summary>
        /// Builds the inner cluster chain.
        /// </summary>
        /// <param name="dataBundle">The data to be used for training.</param>
        /// <param name="filter">The feature filter to be used to denormalize output.</param>
        /// <param name="rand">The random object to be used (optional).</param>
        /// <param name="controller">The build process controller (optional).</param>
        public void Build(VectorBundle dataBundle,
                          FeatureFilterBase filter,
                          Random rand = null,
                          TNRNetBuilder.BuildControllerDelegate controller = null
                          )
        {
            rand = rand ?? new Random(0);
            TNRNetClusterChainBuilder builder = new TNRNetClusterChainBuilder(Name,
                                                                              _clusterChainCfg,
                                                                              rand,
                                                                              controller
                                                                              );
            builder.ChainBuildProgressChanged += OnChainBuildProgressChanged;
            _clusterChain = builder.Build(dataBundle, new FeatureFilterBase[] { filter });
            return;
        }

        /// <summary>
        /// Computes the readout unit.
        /// </summary>
        /// <param name="predictors">The predictors.</param>
        /// <returns>The readout unit's composite result.</returns>
        public CompositeResult Compute(double[] predictors)
        {
            if (!Ready)
            {
                throw new InvalidOperationException($"Readout unit {Name} is not ready to operate. Call the Build method first.");
            }
            double[] result = _clusterChain.Compute(predictors, out List<Tuple<int, double[]>> mainClusterSubResults);
            return new CompositeResult(result, NonRecurrentNetUtils.ExtractVectors(mainClusterSubResults));
        }

        /// <summary>
        /// Gets the clone of error statistics of the readout unit.
        /// </summary>
        public TNRNetCluster.ClusterErrStatistics GetErrStat()
        {
            return _clusterChain.MainCluster.ErrorStats.DeepClone();
        }


        //Inner classes
        /// <summary>
        /// Implements the holder of the readout unit build progress information.
        /// </summary>
        public class BuildProgress : IBuildProgress
        {
            //Attribute properties
            /// <summary>
            /// Name of the readout unit.
            /// </summary>
            public string UnitName { get; }

            /// <summary>
            /// Information about the cluster chain build progress.
            /// </summary>
            public TNRNetClusterChainBuilder.BuildProgress ChainBuildProgress { get; }

            /// <summary>
            /// Creates an initialized instance.
            /// </summary>
            /// <param name="unitName">Name of the readout unit.</param>
            /// <param name="chainBuildProgress">The holder of the cluster chain build progress information.</param>
            public BuildProgress(string unitName,
                                 TNRNetClusterChainBuilder.BuildProgress chainBuildProgress
                                 )
            {
                UnitName = unitName;
                ChainBuildProgress = chainBuildProgress;
                return;
            }

            //Properties
            /// <inheritdoc/>
            public bool NewEndNetwork
            {
                get
                {
                    return ChainBuildProgress.NewEndNetwork;
                }
            }

            /// <inheritdoc/>
            public bool ShouldBeReported
            {
                get
                {
                    return ChainBuildProgress.ShouldBeReported;
                }
            }

            /// <inheritdoc/>
            public int EndNetworkEpochNum
            {
                get
                {
                    return ChainBuildProgress.EndNetworkEpochNum;
                }
            }

            //Methods
            /// <inheritdoc/>
            public string GetInfoText(int margin = 0, bool includeName = true)
            {
                //Build the progress text message
                StringBuilder progressText = new StringBuilder();
                progressText.Append(new string(' ', margin));
                if (includeName)
                {
                    progressText.Append("[");
                    progressText.Append(UnitName);
                    progressText.Append("] ");
                }
                progressText.Append(ChainBuildProgress.GetInfoText(0, false));
                return progressText.ToString();
            }

        }//BuildProgress



    }//ReadoutUnit


}//Namespace
