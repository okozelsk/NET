using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using RCNet.Extensions;
using RCNet.XmlTools;
using RCNet.RandomValue;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Synapse's plasticity configuration
    /// </summary>
    [Serializable]
    public class PlasticitySettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "SynapsePlasticityType";

        //Attribute properties
        /// <summary>
        /// Synapse's dynamics configuration when excitatory-excitatory neurons are connected
        /// </summary>
        public DynamicsEESettings DynamicsEECfg { get; }
        /// <summary>
        /// Synapse's dynamics configuration when excitatory-inhibitory neurons are connected
        /// </summary>
        public DynamicsEISettings DynamicsEICfg { get; }
        /// <summary>
        /// Synapse's dynamics configuration when inhibitory-excitatory neurons are connected
        /// </summary>
        public DynamicsIESettings DynamicsIECfg { get; }
        /// <summary>
        /// Synapse's dynamics configuration when inhibitory-inhibitory neurons are connected
        /// </summary>
        public DynamicsIISettings DynamicsIICfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public PlasticitySettings()
        {
            DynamicsEECfg = new DynamicsEESettings();
            DynamicsEICfg = new DynamicsEISettings();
            DynamicsIECfg = new DynamicsIESettings();
            DynamicsIICfg = new DynamicsIISettings();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="dynamicSettings">Specific dynamics settings</param>
        public PlasticitySettings(IEnumerable<DynamicsSettings> dynamicSettings)
            : this()
        {
            if (dynamicSettings != null)
            {
                foreach (DynamicsSettings ds in dynamicSettings)
                {
                    if (ds.GetType() == typeof(DynamicsEESettings))
                    {
                        DynamicsEECfg = (DynamicsEESettings)ds.DeepClone();
                    }
                    else if (ds.GetType() == typeof(DynamicsEISettings))
                    {
                        DynamicsEICfg = (DynamicsEISettings)ds.DeepClone();
                    }
                    else if (ds.GetType() == typeof(DynamicsIESettings))
                    {
                        DynamicsIECfg = (DynamicsIESettings)ds.DeepClone();
                    }
                    else if (ds.GetType() == typeof(DynamicsIISettings))
                    {
                        DynamicsIICfg = (DynamicsIISettings)ds.DeepClone();
                    }
                }
            }
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="dynamicSettings">Specific dynamics settings</param>
        public PlasticitySettings(params DynamicsSettings[] dynamicSettings)
            :this((IEnumerable<DynamicsSettings>) dynamicSettings)
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public PlasticitySettings(PlasticitySettings source)
            :this(source.DynamicsEECfg, source.DynamicsEICfg, source.DynamicsIECfg, source.DynamicsIICfg)
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
        public PlasticitySettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            XElement dynamicsCfgElem;
            //DynamicsEECfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsEE");
            DynamicsEECfg = dynamicsCfgElem == null ? new DynamicsEESettings() : new DynamicsEESettings(dynamicsCfgElem);
            //DynamicsEICfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsEI");
            DynamicsEICfg = dynamicsCfgElem == null ? new DynamicsEISettings() : new DynamicsEISettings(dynamicsCfgElem);
            //DynamicsIECfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsIE");
            DynamicsIECfg = dynamicsCfgElem == null ? new DynamicsIESettings() : new DynamicsIESettings(dynamicsCfgElem);
            //DynamicsIICfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsII");
            DynamicsIICfg = dynamicsCfgElem == null ? new DynamicsIISettings() : new DynamicsIISettings(dynamicsCfgElem);
            return;
        }

        //Properties
        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return DynamicsEECfg.ContainsOnlyDefaults &&
                       DynamicsEICfg.ContainsOnlyDefaults &&
                       DynamicsIECfg.ContainsOnlyDefaults &&
                       DynamicsIICfg.ContainsOnlyDefaults;
            }
        }

        //Methods
        /// <summary>
        /// Returns appropriate dynamics settings
        /// </summary>
        /// <param name="sourceNeuronRole">Role of the source neuron</param>
        /// <param name="targetNeuronRole">Role of the target neuron</param>
        public DynamicsSettings GetDynamicsSettings(NeuronCommon.NeuronRole sourceNeuronRole,
                                                    NeuronCommon.NeuronRole targetNeuronRole
                                                    )
        {
            //Input role is considered as the excitatory role
            sourceNeuronRole = sourceNeuronRole == NeuronCommon.NeuronRole.Input ? NeuronCommon.NeuronRole.Excitatory : sourceNeuronRole;
            //Choose appropriate dynamics
            if (sourceNeuronRole == NeuronCommon.NeuronRole.Excitatory &&
               targetNeuronRole == NeuronCommon.NeuronRole.Excitatory
               )
            {
                return DynamicsEECfg;
            }
            else if (sourceNeuronRole == NeuronCommon.NeuronRole.Excitatory &&
                     targetNeuronRole == NeuronCommon.NeuronRole.Inhibitory
                     )
            {
                return DynamicsEICfg;
            }
            else if (sourceNeuronRole == NeuronCommon.NeuronRole.Inhibitory &&
                     targetNeuronRole == NeuronCommon.NeuronRole.Excitatory
                     )
            {
                return DynamicsIECfg;
            }
            else if (sourceNeuronRole == NeuronCommon.NeuronRole.Inhibitory &&
                     targetNeuronRole == NeuronCommon.NeuronRole.Inhibitory
                     )
            {
                return DynamicsIICfg;
            }
            return null;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new PlasticitySettings(this);
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
            if (!suppressDefaults || !DynamicsEECfg.ContainsOnlyDefaults)
            {
                rootElem.Add(DynamicsEECfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !DynamicsEICfg.ContainsOnlyDefaults)
            {
                rootElem.Add(DynamicsEICfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !DynamicsIECfg.ContainsOnlyDefaults)
            {
                rootElem.Add(DynamicsIECfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !DynamicsIICfg.ContainsOnlyDefaults)
            {
                rootElem.Add(DynamicsIICfg.GetXml(suppressDefaults));
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
            return GetXml("plasticity", suppressDefaults);
        }

    }//PlasticitySettings

}//Namespace

