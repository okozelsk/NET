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
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultCompMode { get { return (CompMode == DefaultCompMode); } }
        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance.
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new NetworkClusterSecondLevelCompSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
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

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("clusterSecondLevelComputation", suppressDefaults);
        }

    }//NetworkClusterSecondLevelCompSettings

}//Namespace
