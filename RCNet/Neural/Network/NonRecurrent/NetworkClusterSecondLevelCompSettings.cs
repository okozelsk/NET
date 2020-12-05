using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using RCNet.Neural.Network.NonRecurrent.FF;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Configuration of the 2nd level computation of the network cluster
    /// </summary>
    [Serializable]
    public class NetworkClusterSecondLevelCompSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NetworkClusterSecondLevelCompType";
        //Default values
        /// <summary>
        /// Default value of the parameter specifying computation mode of the cluster
        /// </summary>
        public const TrainedNetworkCluster.SecondLevelCompMode DefaultCompMode = TrainedNetworkCluster.SecondLevelCompMode.AveragedOutputs;

        //Attribute properties
        /// <summary>
        /// Crossvalidation configuration
        /// </summary>
        public CrossvalidationSettings CrossvalidationCfg { get; }

        /// <summary>
        /// 2nd level network configuration
        /// </summary>
        public FeedForwardNetworkSettings NetCfg { get; }

        /// <summary>
        /// Computation mode of the cluster
        /// </summary>
        public TrainedNetworkCluster.SecondLevelCompMode CompMode { get; }


        //Constructors
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        /// <param name="crossvalidationCfg">Crossvalidation configuration</param>
        /// <param name="netCfg">2nd level network configuration</param>
        /// <param name="compMode">Computation mode</param>
        public NetworkClusterSecondLevelCompSettings(CrossvalidationSettings crossvalidationCfg,
                                                     FeedForwardNetworkSettings netCfg,
                                                     TrainedNetworkCluster.SecondLevelCompMode compMode = DefaultCompMode
                                                     )
        {
            CrossvalidationCfg = (CrossvalidationSettings)crossvalidationCfg.DeepClone();
            NetCfg = (FeedForwardNetworkSettings)netCfg.DeepClone();
            CompMode = compMode;
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public NetworkClusterSecondLevelCompSettings(NetworkClusterSecondLevelCompSettings source)
            : this(source.CrossvalidationCfg, source.NetCfg, source.CompMode)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml data containing the settings.</param>
        public NetworkClusterSecondLevelCompSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            CrossvalidationCfg = new CrossvalidationSettings(settingsElem.Element("crossvalidation"));
            NetCfg = new FeedForwardNetworkSettings(settingsElem.Element("ff"));
            CompMode = (TrainedNetworkCluster.SecondLevelCompMode)Enum.Parse(typeof(TrainedNetworkCluster.SecondLevelCompMode), settingsElem.Attribute("mode").Value, true);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultCompMode { get { return (CompMode == DefaultCompMode); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new NetworkClusterSecondLevelCompSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName, CrossvalidationCfg.GetXml(suppressDefaults), NetCfg.GetXml(suppressDefaults));
            if (!suppressDefaults || !IsDefaultCompMode)
            {
                rootElem.Add(new XAttribute("mode", CompMode.ToString()));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("clusterSecondLevelComputation", suppressDefaults);
        }

    }//NetworkClusterSecondLevelCompSettings

}//Namespace
