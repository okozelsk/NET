using System;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration of a synapse.
    /// </summary>
    [Serializable]
    public class SynapseSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "SynapseType";

        //Attribute properties
        /// <summary>
        /// The configuration of a synapse connecting postsynaptic hidden spiking neuron.
        /// </summary>
        public SynapseSTSettings SpikingTargetCfg { get; }

        /// <summary>
        /// The configuration of a synapse connecting postsynaptic hidden analog neuron.
        /// </summary>
        public SynapseATSettings AnalogTargetCfg { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="spikingTargetCfg">The configuration of a synapse connecting postsynaptic hidden spiking neuron.</param>
        /// <param name="analogTargetCfg">The configuration of a synapse connecting postsynaptic hidden analog neuron.</param>
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
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public SynapseSettings(SynapseSettings source)
            : this(source.SpikingTargetCfg, source.AnalogTargetCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
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
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultSpikingTargetCfg { get { return SpikingTargetCfg.ContainsOnlyDefaults; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultAnalogTargetCfg { get { return AnalogTargetCfg.ContainsOnlyDefaults; } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultSpikingTargetCfg &&
                       IsDefaultAnalogTargetCfg;
            }
        }


        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new SynapseSettings(this);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("synapse", suppressDefaults);
        }

    }//SynapseSettings

}//Namespace

