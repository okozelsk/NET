using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.IO;
using RCNet.Extensions;
using RCNet.MathTools.Probability;
using RCNet.XmlTools;
using RCNet.RandomValue;
using RCNet.Neural.Network.SM.Preprocessing;
using System.Xml.XPath;

namespace RCNet.Neural.Network.SM.Synapse
{
    /// <summary>
    /// Configuration parameters of an input synapse
    /// </summary>
    [Serializable]
    public class InputSynapseSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "InputSynapseCfgType";

        //Attribute properties
        /// <summary>
        /// Synapse's scope when targeting spiking neurons
        /// </summary>
        public BaseSynapse.SynapticTargetScope SpikingTargetScope { get; set; }
        /// <summary>
        /// Synapse's random weight settings for Input->Spiking connection
        /// </summary>
        public RandomValueSettings SpikingTargetWeightCfg { get; set; }
        /// <summary>
        /// Synapse's scope when targeting analog neurons
        /// </summary>
        public BaseSynapse.SynapticTargetScope AnalogTargetScope { get; set; }
        /// <summary>
        /// Synapse's random weight settings for Input->Analog connection
        /// </summary>
        public RandomValueSettings AnalogTargetWeightCfg { get; set; }
        /// <summary>
        /// Specifies how will be decided synaptic delay
        /// </summary>
        public InputSynapse.SynapticDelayMethod DelayMethod { get; set; }
        /// <summary>
        /// Maximum delay of the input signal
        /// </summary>
        public int MaxDelay { get; set; }


        //Constructors
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
            DelayMethod = source.DelayMethod;
            MaxDelay = source.MaxDelay;
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
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            XElement cfgElem;
            //Spiking target
            //Scope
            cfgElem = settingsElem.XPathSelectElement("./spikingTarget");
            if(cfgElem != null)
            {
                SpikingTargetScope = BaseSynapse.ParseSynapticTargetScope(cfgElem.Attribute("scope").Value);
            }
            else
            {
                SpikingTargetScope = BaseSynapse.SynapticTargetScope.Excitatory;
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
                AnalogTargetScope = BaseSynapse.ParseSynapticTargetScope(cfgElem.Attribute("scope").Value);
            }
            else
            {
                AnalogTargetScope = BaseSynapse.SynapticTargetScope.All;
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
            //Delay
            DelayMethod = InputSynapse.ParseSynapticDelayMethod(settingsElem.Attribute("delayMethod").Value);
            MaxDelay = int.Parse(settingsElem.Attribute("maxDelay").Value, CultureInfo.InvariantCulture);
            return;
        }

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public InputSynapseSettings DeepClone()
        {
            return new InputSynapseSettings(this);
        }

    }//InputSynapseSettings

}//Namespace

