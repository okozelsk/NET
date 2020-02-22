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
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Synapse;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
    /// <summary>
    /// Configuration of the inter-pool connection
    /// </summary>
    [Serializable]
    public class InterPoolConnSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ResStructInterPoolConnectionType";
        //Default values
        /// <summary>
        /// Default keep constant number of incoming synapses
        /// </summary>
        public const bool DefaultConstantNumOfConnections = false;


        //Attribute properties
        /// <summary>
        /// Connection probabilities settings
        /// </summary>
        public IConnDistrSettings ConnDistrCfg { get; }
        /// <summary>
        /// Name of the source pool
        /// </summary>
        public string SourcePoolName { get; }
        /// <summary>
        /// Determines how many neurons from the source pool will be connected to neurons in target pool
        /// </summary>
        public double SourceConnectionDensity { get; }
        /// <summary>
        /// Name of the target pool
        /// </summary>
        public string TargetPoolName { get; }
        /// <summary>
        /// Determines how many neurons from the target pool will be connected to one neuron from source pool
        /// </summary>
        public double TargetConnectionDensity { get; }
        /// <summary>
        /// Specifies whether to keep for each neuron from source pool constant number of synapses
        /// </summary>
        public bool ConstantNumOfConnections { get; }
        /// <summary>
        /// Synapse settings
        /// </summary>
        public InternalSynapseSettings SynapseCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="connDistrCfg">Connection probabilities settings</param>
        /// <param name="sourcePoolName">Name of the source pool</param>
        /// <param name="sourceConnectionDensity">Determines how many neurons from the source pool will be connected to neurons in target pool</param>
        /// <param name="targetPoolName">Name of the target pool</param>
        /// <param name="targetConnectionDensity">Determines how many neurons from the target pool will be connected to one neuron from source pool</param>
        /// <param name="constantNumOfConnections">Specifies whether to keep for each neuron constant number of synapses</param>
        /// <param name="synapseCfg">Synapse settings</param>
        public InterPoolConnSettings(IConnDistrSettings connDistrCfg,
                                                             string sourcePoolName,
                                                             double sourceConnectionDensity,
                                                             string targetPoolName,
                                                             double targetConnectionDensity,
                                                             bool constantNumOfConnections = DefaultConstantNumOfConnections,
                                                             InternalSynapseSettings synapseCfg = null
                                                             )
        {
            ConnDistrCfg = (IConnDistrSettings)connDistrCfg.DeepClone();
            SourcePoolName = sourcePoolName;
            SourceConnectionDensity = sourceConnectionDensity;
            TargetPoolName = targetPoolName;
            TargetConnectionDensity = targetConnectionDensity;
            ConstantNumOfConnections = constantNumOfConnections;
            SynapseCfg = synapseCfg == null ? new InternalSynapseSettings() : (InternalSynapseSettings)synapseCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public InterPoolConnSettings(InterPoolConnSettings source)
            :this(source.ConnDistrCfg, source.SourcePoolName, source.SourceConnectionDensity, source.TargetPoolName,
                  source.TargetConnectionDensity, source.ConstantNumOfConnections, source.SynapseCfg)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public InterPoolConnSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            SourcePoolName = settingsElem.Attribute("sourcePool").Value;
            SourceConnectionDensity = double.Parse(settingsElem.Attribute("sourceConnDensity").Value, CultureInfo.InvariantCulture);
            TargetPoolName = settingsElem.Attribute("targetPool").Value;
            TargetConnectionDensity = double.Parse(settingsElem.Attribute("targetConnDensity").Value, CultureInfo.InvariantCulture);
            ConstantNumOfConnections = bool.Parse(settingsElem.Attribute("constantNumOfConnections").Value);
            //Connection probabilities
            XElement connDistrSettings = settingsElem.Descendants().First();
            switch(connDistrSettings.Name.LocalName)
            {
                case "distributionCustom":
                    ConnDistrCfg = new ConnDistrCustomSettings(connDistrSettings);
                    break;
                case "distributionLSM":
                    ConnDistrCfg = new ConnDistrLSMSettings(connDistrSettings);
                    break;
                case "distributionFlat":
                    ConnDistrCfg = new ConnDistrFlatSettings(connDistrSettings);
                    break;
                default:
                    throw new Exception($"Unknown connection distribution {connDistrSettings.Name.LocalName}.");
            }
            //Synapse
            XElement synapseSettingsElem = settingsElem.Descendants("synapse").FirstOrDefault();
            SynapseCfg = synapseSettingsElem == null ? new InternalSynapseSettings() : new InternalSynapseSettings(settingsElem.Descendants("synapse").First());
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultConstantNumOfConnections { get { return (ConstantNumOfConnections == DefaultConstantNumOfConnections); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return false;
            }
        }


        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (SourcePoolName.Length == 0)
            {
                throw new Exception($"SourcePoolName can not be empty.");
            }
            if (SourceConnectionDensity < 0 || SourceConnectionDensity > 1)
            {
                throw new Exception($"Invalid SourceConnectionDensity {SourceConnectionDensity.ToString(CultureInfo.InvariantCulture)}. Density must be GE to 0 and LE to 1.");
            }
            if (TargetPoolName.Length == 0)
            {
                throw new Exception($"TargetPoolName can not be empty.");
            }
            if (TargetConnectionDensity < 0 || TargetConnectionDensity > 1)
            {
                throw new Exception($"Invalid TargetConnectionDensity {TargetConnectionDensity.ToString(CultureInfo.InvariantCulture)}. Density must be GE to 0 and LE to 1.");
            }
            if(SourcePoolName == TargetPoolName)
            {
                throw new Exception($"SourcePoolName and TargetPoolName can not be the same.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new InterPoolConnSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("sourcePool", SourcePoolName),
                                             new XAttribute("sourceConnDensity", SourceConnectionDensity.ToString(CultureInfo.InvariantCulture)),
                                             new XAttribute("targetPool", TargetPoolName),
                                             new XAttribute("targetConnDensity", TargetConnectionDensity.ToString(CultureInfo.InvariantCulture))
                                             );
            if (!suppressDefaults || !IsDefaultConstantNumOfConnections)
            {
                rootElem.Add(new XAttribute("constantNumOfConnections", ConstantNumOfConnections.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
            }
            rootElem.Add(ConnDistrCfg.GetXml(suppressDefaults));
            if (!suppressDefaults || !SynapseCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(SynapseCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("interPoolConnection", suppressDefaults);
        }


    }//ReservoirStructureInterPoolConnectionSettings

}//Namespace
