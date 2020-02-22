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
    /// Collection of classification networks settings
    /// </summary>
    [Serializable]
    public class ClassificationNetworksSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ROutLayerUnitClassificationNetworksType";

        //Attribute properties
        /// <summary>
        /// Collection of classification networks settings
        /// </summary>
        public List<INonRecurrentNetworkSettings> NetworkCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public ClassificationNetworksSettings()
        {
            NetworkCfgCollection = new List<INonRecurrentNetworkSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="networkCfgs">Network settings</param>
        public ClassificationNetworksSettings(IEnumerable<INonRecurrentNetworkSettings> networkCfgs)
            : this()
        {
            foreach (INonRecurrentNetworkSettings netCfg in networkCfgs)
            {
                NetworkCfgCollection.Add((INonRecurrentNetworkSettings)netCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="networkCfgs">Network settings</param>
        public ClassificationNetworksSettings(params INonRecurrentNetworkSettings[] networkCfgs)
            : this()
        {
            foreach (INonRecurrentNetworkSettings netCfg in networkCfgs)
            {
                NetworkCfgCollection.Add((INonRecurrentNetworkSettings)netCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ClassificationNetworksSettings(ClassificationNetworksSettings source)
            : this()
        {
            foreach(INonRecurrentNetworkSettings netCfg in source.NetworkCfgCollection)
            {
                NetworkCfgCollection.Add((INonRecurrentNetworkSettings)netCfg.DeepClone());
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
        public ClassificationNetworksSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            NetworkCfgCollection = NonRecurrentNetUtils.LoadSettingsCollection(settingsElem);
            return;
        }

        //Properties
        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return NetworkCfgCollection.Count == 0; } }


        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new ClassificationNetworksSettings(this);
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

    }//ClassificationNetworksSettings

}//Namespace
