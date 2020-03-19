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
    /// Synapse's plasticity configuration (excitatory spiking to hidden spiking neuron)
    /// </summary>
    [Serializable]
    public class PlasticitySTExcitatorySettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "SynapsePlasticitySTExcitatoryType";

        //Attribute properties
        /// <summary>
        /// Synapse's dynamics configuration
        /// </summary>
        public IDynamicsSettings DynamicsCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public PlasticitySTExcitatorySettings(IDynamicsSettings dynamicsCfg = null)
        {
            if (dynamicsCfg == null)
            {
                DynamicsCfg = new LinearDynamicsSTExcitatorySettings();
            }
            else if (dynamicsCfg.Application != PlasticityCommon.DynApplication.STExcitatory)
            {
                throw new Exception($"Dynamics application must be STExcitatory.");
            }
            else
            {
                DynamicsCfg = (IDynamicsSettings)(((RCNetBaseSettings)dynamicsCfg).DeepClone());
            }
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public PlasticitySTExcitatorySettings(PlasticitySTExcitatorySettings source)
            :this(source.DynamicsCfg)
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
        public PlasticitySTExcitatorySettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            XElement dynamicsCfgElem = settingsElem.Descendants().FirstOrDefault();
            if (dynamicsCfgElem == null)
            {
                DynamicsCfg = new ConstantDynamicsSTExcitatorySettings();
            }
            else
            {
                switch (dynamicsCfgElem.Name.LocalName)
                {
                    case "constantDynamics":
                        DynamicsCfg = new ConstantDynamicsSTExcitatorySettings(dynamicsCfgElem);
                        break;
                    case "linearDynamics":
                        DynamicsCfg = new LinearDynamicsSTExcitatorySettings(dynamicsCfgElem);
                        break;
                    case "nonlinearDynamics":
                        DynamicsCfg = new NonlinearDynamicsSTExcitatorySettings(dynamicsCfgElem);
                        break;
                    default:
                        throw new Exception($"Unexpected element name {dynamicsCfgElem.Name.LocalName}.");
                }
            }
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
                return DynamicsCfg.Type == PlasticityCommon.DynType.Linear &&
                       ((RCNetBaseSettings)DynamicsCfg).ContainsOnlyDefaults;
            }
        }

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new PlasticitySTExcitatorySettings(this);
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
            if (!suppressDefaults || !((RCNetBaseSettings)DynamicsCfg).ContainsOnlyDefaults)
            {
                rootElem.Add(DynamicsCfg.GetXml(suppressDefaults));
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

    }//PlasticitySTExcitatorySettings

}//Namespace

