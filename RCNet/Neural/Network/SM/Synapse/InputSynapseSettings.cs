using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.IO;
using RCNet.Extensions;
using RCNet.XmlTools;
using RCNet.RandomValue;
using System.Xml.XPath;

namespace RCNet.Neural.Network.SM.Synapse
{
    /// <summary>
    /// Setup parameters of input synapse
    /// </summary>
    [Serializable]
    public class InputSynapseSettings
    {
        //Attribute properties
        /// <summary>
        /// Synapse's scope when targeting spiking neurons
        /// </summary>
        public CommonEnums.InputSynapseScope SpikingTargetScope { get; set; }
        /// <summary>
        /// Synapse's random weight settings for Input->Spiking connection
        /// </summary>
        public RandomValueSettings SpikingTargetWeightCfg { get; set; }

        /// <summary>
        /// Synapse's scope when targeting analog neurons
        /// </summary>
        public CommonEnums.InputSynapseScope AnalogTargetScope { get; set; }
        /// <summary>
        /// Synapse's random weight settings for Input->Analog connection
        /// </summary>
        public RandomValueSettings AnalogTargetWeightCfg { get; set; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance
        /// </summary>
        public InputSynapseSettings()
        {
            SpikingTargetScope = CommonEnums.InputSynapseScope.Excitatory;
            SpikingTargetWeightCfg = null;
            AnalogTargetScope = CommonEnums.InputSynapseScope.All;
            AnalogTargetWeightCfg = null;
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public InputSynapseSettings(InputSynapseSettings source)
        {
            SpikingTargetScope = source.SpikingTargetScope;
            SpikingTargetWeightCfg = null;
            if (source.SpikingTargetWeightCfg != null)
            {
                SpikingTargetWeightCfg = source.SpikingTargetWeightCfg.DeepClone();
            }
            AnalogTargetScope = source.AnalogTargetScope;
            AnalogTargetWeightCfg = null;
            if (source.AnalogTargetWeightCfg != null)
            {
                AnalogTargetWeightCfg = source.AnalogTargetWeightCfg.DeepClone();
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
        public InputSynapseSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.SM.Synapse.InputSynapseSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement settingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            XElement cfgElem;
            //Spiking target
            //Scope
            cfgElem = settingsElem.XPathSelectElement("./spikingTarget");
            if(cfgElem != null)
            {
                SpikingTargetScope = CommonEnums.ParseInputSynapseScope(cfgElem.Attribute("scope").Value);
            }
            else
            {
                SpikingTargetScope = CommonEnums.InputSynapseScope.Excitatory;
            }
            //Spiking target Weight
            cfgElem = settingsElem.XPathSelectElement("./spikingTarget/weight");
            if(cfgElem != null)
            {
                SpikingTargetWeightCfg = new RandomValueSettings(cfgElem);
            }
            else
            {
                SpikingTargetWeightCfg = new RandomValueSettings(0, 1);
            }
            //Analog target
            //Scope
            cfgElem = settingsElem.XPathSelectElement("./analogTarget");
            if (cfgElem != null)
            {
                AnalogTargetScope = CommonEnums.ParseInputSynapseScope(cfgElem.Attribute("scope").Value);
            }
            else
            {
                AnalogTargetScope = CommonEnums.InputSynapseScope.All;
            }
            //Analog target Weight
            cfgElem = settingsElem.XPathSelectElement("./analogTarget/weight");
            if (cfgElem != null)
            {
                AnalogTargetWeightCfg = new RandomValueSettings(cfgElem);
            }
            else
            {
                AnalogTargetWeightCfg = new RandomValueSettings(0, 1);
            }
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            InputSynapseSettings cmpSettings = obj as InputSynapseSettings;
            if (SpikingTargetScope != cmpSettings.SpikingTargetScope ||
                !Equals(SpikingTargetWeightCfg, cmpSettings.SpikingTargetWeightCfg) ||
                AnalogTargetScope != cmpSettings.AnalogTargetScope ||
                !Equals(AnalogTargetWeightCfg, cmpSettings.AnalogTargetWeightCfg)
                )
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// See the base.
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public InputSynapseSettings DeepClone()
        {
            InputSynapseSettings clone = new InputSynapseSettings(this);
            return clone;
        }

    }//InputSynapseSettings

}//Namespace

