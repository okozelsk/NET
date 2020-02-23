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

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Synapse
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
        public const string XsdTypeName = "InputSynapseType";
        //Default values
        /// <summary>
        /// Default delay method
        /// </summary>
        const InputSynapse.SynapticDelayMethod DefaultDelayMethod = InputSynapse.SynapticDelayMethod.Random;
        /// <summary>
        /// Default maximum delay
        /// </summary>
        const int DefaultMaxDelay = 0;

        //Attribute properties
        /// <summary>
        /// Specifies how will be decided synaptic delay
        /// </summary>
        public InputSynapse.SynapticDelayMethod DelayMethod { get; }
        /// <summary>
        /// Maximum delay of the input signal
        /// </summary>
        public int MaxDelay { get; }
        /// <summary>
        /// Synapse's settings when targeting spiking neurons
        /// </summary>
        public SpikingTargetSettings SpikingTargetCfg { get; }
        /// <summary>
        /// Synapse's settings when targeting analog neurons
        /// </summary>
        public AnalogTargetSettings AnalogTargetCfg { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="delayMethod">Specifies how will be decided synaptic delay</param>
        /// <param name="maxDelay">Maximum delay of the input signal</param>
        /// <param name="spikingTargetCfg">Synapse's settings when targeting spiking neurons</param>
        /// <param name="analogTargetCfg">Synapse's settings when targeting analog neurons</param>
        public InputSynapseSettings(InputSynapse.SynapticDelayMethod delayMethod = DefaultDelayMethod,
                                    int maxDelay = DefaultMaxDelay,
                                    SpikingTargetSettings spikingTargetCfg = null,
                                    AnalogTargetSettings analogTargetCfg = null
                                    )
        {
            DelayMethod = delayMethod;
            MaxDelay = maxDelay;
            if(spikingTargetCfg != null)
            {
                SpikingTargetCfg = (SpikingTargetSettings)spikingTargetCfg.DeepClone();
            }
            else
            {
                SpikingTargetCfg = new SpikingTargetSettings();
            }
            if (analogTargetCfg != null)
            {
                AnalogTargetCfg = (AnalogTargetSettings)analogTargetCfg.DeepClone();
            }
            else
            {
                AnalogTargetCfg = new AnalogTargetSettings();
            }
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public InputSynapseSettings(InputSynapseSettings source)
        {
            DelayMethod = source.DelayMethod;
            MaxDelay = source.MaxDelay;
            SpikingTargetCfg = (SpikingTargetSettings)source.SpikingTargetCfg.DeepClone();
            AnalogTargetCfg = (AnalogTargetSettings)source.AnalogTargetCfg.DeepClone();
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
            //Delay
            DelayMethod = (InputSynapse.SynapticDelayMethod)Enum.Parse(typeof(InputSynapse.SynapticDelayMethod), settingsElem.Attribute("delayMethod").Value, true);
            MaxDelay = int.Parse(settingsElem.Attribute("maxDelay").Value, CultureInfo.InvariantCulture);
            //Targets
            XElement cfgElem;
            //Spiking target
            cfgElem = settingsElem.Descendants("spikingTarget").FirstOrDefault();
            if(cfgElem != null)
            {
                SpikingTargetCfg = new SpikingTargetSettings(cfgElem);
            }
            else
            {
                SpikingTargetCfg = new SpikingTargetSettings();
            }
            //Analog target
            cfgElem = settingsElem.Descendants("analogTarget").FirstOrDefault();
            if (cfgElem != null)
            {
                AnalogTargetCfg = new AnalogTargetSettings(cfgElem);
            }
            else
            {
                AnalogTargetCfg = new AnalogTargetSettings();
            }
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultDelayMethod { get { return (DelayMethod == DefaultDelayMethod); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultMaxDelay { get { return (MaxDelay == DefaultMaxDelay); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultDelayMethod &&
                       IsDefaultMaxDelay &&
                       SpikingTargetCfg.ContainsOnlyDefaults &&
                       AnalogTargetCfg.ContainsOnlyDefaults;
            }
        }


        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (MaxDelay < 0)
            {
                throw new Exception($"Invalid MaxDelay {MaxDelay.ToString(CultureInfo.InvariantCulture)}. MaxDelay must be GE to 0.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new InputSynapseSettings(this);
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
            if (!suppressDefaults || !IsDefaultDelayMethod)
            {
                rootElem.Add(new XAttribute("delayMethod", DelayMethod.ToString()));
            }
            if (!suppressDefaults || !IsDefaultMaxDelay)
            {
                rootElem.Add(new XAttribute("maxDelay", MaxDelay.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !SpikingTargetCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(SpikingTargetCfg.GetXml("spikingTarget", suppressDefaults));
            }
            if (!suppressDefaults || !AnalogTargetCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(AnalogTargetCfg.GetXml("analogTarget", suppressDefaults));
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
            return GetXml("synapse", suppressDefaults);
        }

    }//InputSynapseSettings

}//Namespace

