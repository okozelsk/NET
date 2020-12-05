using RCNet.Neural.Network.NonRecurrent;
using RCNet.Neural.Network.NonRecurrent.FF;
using System;
using System.Collections.Generic;
using System.Xml.Linq;


namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// The collection of the forecast networks configurations
    /// </summary>
    [Serializable]
    public class ForecastNetworksSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ROutLayerUnitForecastNetworksType";

        //Attribute properties
        /// <summary>
        /// Collection of forecast networks settings
        /// </summary>
        public List<INonRecurrentNetworkSettings> NetworkCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public ForecastNetworksSettings()
        {
            NetworkCfgCollection = new List<INonRecurrentNetworkSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="networkCfgs">Network settings</param>
        public ForecastNetworksSettings(IEnumerable<FeedForwardNetworkSettings> networkCfgs)
            : this()
        {
            foreach (FeedForwardNetworkSettings netCfg in networkCfgs)
            {
                NetworkCfgCollection.Add((FeedForwardNetworkSettings)netCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="networkCfgs">Network settings</param>
        public ForecastNetworksSettings(params FeedForwardNetworkSettings[] networkCfgs)
            : this()
        {
            foreach (FeedForwardNetworkSettings netCfg in networkCfgs)
            {
                NetworkCfgCollection.Add((FeedForwardNetworkSettings)netCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ForecastNetworksSettings(ForecastNetworksSettings source)
            : this()
        {
            foreach (FeedForwardNetworkSettings netCfg in source.NetworkCfgCollection)
            {
                NetworkCfgCollection.Add((FeedForwardNetworkSettings)netCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public ForecastNetworksSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            NetworkCfgCollection = new List<INonRecurrentNetworkSettings>();
            foreach (XElement ffSettingsElem in settingsElem.Elements("ff"))
            {
                NetworkCfgCollection.Add(new FeedForwardNetworkSettings(ffSettingsElem));
            }
            return;
        }

        //Properties
        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return NetworkCfgCollection.Count == 0; } }


        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new ForecastNetworksSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach (INonRecurrentNetworkSettings netCfg in NetworkCfgCollection)
            {
                rootElem.Add(netCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

    }//ForecastNetworksSettings

}//Namespace
