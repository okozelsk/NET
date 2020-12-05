using RCNet.Neural.Network.NonRecurrent;
using System;
using System.Collections.Generic;
using System.Xml.Linq;


namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Collection of the classification networks configurations
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
            foreach (INonRecurrentNetworkSettings netCfg in source.NetworkCfgCollection)
            {
                NetworkCfgCollection.Add((INonRecurrentNetworkSettings)netCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public ClassificationNetworksSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            NetworkCfgCollection = NonRecurrentNetUtils.LoadSettingsCollection(settingsElem);
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
            return new ClassificationNetworksSettings(this);
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

    }//ClassificationNetworksSettings

}//Namespace
