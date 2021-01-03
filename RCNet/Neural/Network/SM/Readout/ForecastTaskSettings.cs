using RCNet.Neural.Data.Filter;
using RCNet.Neural.Network.NonRecurrent;
using System;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Configuration of the readout unit's forecast task.
    /// </summary>
    [Serializable]
    public class ForecastTaskSettings : RCNetBaseSettings, ITaskSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ROutUnitForecastTaskType";

        //Static members
        /// <summary>
        /// The shared instance of RealFeatureFilterSettings.
        /// </summary>
        private static readonly IFeatureFilterSettings _sharedRealFeatureFilterCfg = new RealFeatureFilterSettings();

        //Attribute properties
        /// <summary>
        /// The cluster chain configuration.
        /// </summary>
        public TNRNetClusterChainRealSettings ClusterChainCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="clusterChainCfg">The cluster chain configuration.</param>
        public ForecastTaskSettings(TNRNetClusterChainRealSettings clusterChainCfg = null)
        {
            ClusterChainCfg = clusterChainCfg == null ? null : (TNRNetClusterChainRealSettings)clusterChainCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public ForecastTaskSettings(ForecastTaskSettings source)
            : this(source.ClusterChainCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public ForecastTaskSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            //Result
            XElement clusterChainElem = settingsElem.Elements("clusterChain").FirstOrDefault();
            ClusterChainCfg = clusterChainElem == null ? null : new TNRNetClusterChainRealSettings(clusterChainElem);
            Check();
            return;
        }

        //Properties
        /// <inheritdoc />
        public ReadoutUnit.TaskType Type { get { return ReadoutUnit.TaskType.Forecast; } }

        /// <inheritdoc />
        public IFeatureFilterSettings FeatureFilterCfg { get { return _sharedRealFeatureFilterCfg; } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return ClusterChainCfg == null;
            }
        }

        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new ForecastTaskSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (ClusterChainCfg != null && !ClusterChainCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(ClusterChainCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("forecast", suppressDefaults);
        }

    }//ForecastTaskSettings

}//Namespace
