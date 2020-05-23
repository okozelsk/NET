using System;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration parameters of a synapse
    /// </summary>
    [Serializable]
    public class SynapseSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "SynapseType";

        //Attribute properties
        /// <summary>
        /// Configuration parameters of a synapse providing signal to hidden spiking neuron
        /// </summary>
        public SynapseSTSettings SpikingTargetCfg { get; }

        /// <summary>
        /// Configuration parameters of a synapse providing signal to hidden analog neuron
        /// </summary>
        public SynapseATSettings AnalogTargetCfg { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="spikingTargetCfg">Configuration parameters of a synapse providing signal to hidden spiking neuron</param>
        /// <param name="analogTargetCfg">Configuration parameters of a synapse providing signal to hidden analog neuron</param>
        public SynapseSettings(SynapseSTSettings spikingTargetCfg = null,
                               SynapseATSettings analogTargetCfg = null
                               )
        {
            SpikingTargetCfg = spikingTargetCfg == null ? new SynapseSTSettings() : (SynapseSTSettings)spikingTargetCfg.DeepClone();
            AnalogTargetCfg = analogTargetCfg == null ? new SynapseATSettings() : (SynapseATSettings)analogTargetCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public SynapseSettings(SynapseSettings source)
            : this(source.SpikingTargetCfg, source.AnalogTargetCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public SynapseSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            XElement cfgElem;
            //Plasticity
            cfgElem = settingsElem.Elements("spikingTarget").FirstOrDefault();
            SpikingTargetCfg = cfgElem == null ? new SynapseSTSettings() : new SynapseSTSettings(cfgElem);
            cfgElem = settingsElem.Elements("analogTarget").FirstOrDefault();
            AnalogTargetCfg = cfgElem == null ? new SynapseATSettings() : new SynapseATSettings(cfgElem);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultSpikingTargetCfg { get { return SpikingTargetCfg.ContainsOnlyDefaults; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultAnalogTargetCfg { get { return AnalogTargetCfg.ContainsOnlyDefaults; } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultSpikingTargetCfg &&
                       IsDefaultAnalogTargetCfg;
            }
        }


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
            return new SynapseSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultSpikingTargetCfg)
            {
                rootElem.Add(SpikingTargetCfg.GetXml("spikingTarget", suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultAnalogTargetCfg)
            {
                rootElem.Add(AnalogTargetCfg.GetXml("analogTarget", suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("synapse", suppressDefaults);
        }

    }//SynapseSettings

}//Namespace

