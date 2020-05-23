using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using RCNet.Extensions;
using RCNet.MathTools.Probability;
using RCNet.XmlTools;
using RCNet.RandomValue;
using RCNet.Neural.Network.NonRecurrent;
using RCNet.Neural.Network.NonRecurrent.FF;
using RCNet.Neural.Network.NonRecurrent.PP;


namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Collection of forecast networks settings
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
            foreach(FeedForwardNetworkSettings netCfg in source.NetworkCfgCollection)
            {
                NetworkCfgCollection.Add((FeedForwardNetworkSettings)netCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
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
        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return NetworkCfgCollection.Count == 0; } }


        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new ForecastNetworksSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach(INonRecurrentNetworkSettings netCfg in NetworkCfgCollection)
            {
                rootElem.Add(netCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

    }//ForecastNetworksSettings

}//Namespace
