using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration of an indifferent synapse connecting a postsynaptic hidden analog neuron.
    /// </summary>
    [Serializable]
    public class SynapseATIndifferentSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "SynapseATIndifferentType";

        //Default values
        /// <summary>
        /// The default delay method.
        /// </summary>
        const Synapse.SynapticDelayMethod DefaultDelayMethod = Synapse.SynapticDelayMethod.Random;
        /// <summary>
        /// The default maximum delay.
        /// </summary>
        const int DefaultMaxDelay = 0;

        //Attribute properties
        /// <summary>
        /// The synaptic delay method.
        /// </summary>
        public Synapse.SynapticDelayMethod DelayMethod { get; }

        /// <summary>
        /// The maximum synaptic delay.
        /// </summary>
        public int MaxDelay { get; }

        /// <summary>
        /// The configuration of the synapse connecting analog presynaptic neuron.
        /// </summary>
        public AnalogSourceSettings AnalogSourceCfg { get; }

        /// <summary>
        /// The configuration of the synapse connecting spiking presynaptic neuron.
        /// </summary>
        public SpikingSourceATIndifferentSettings SpikingSourceCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="delayMethod">The synaptic delay method.</param>
        /// <param name="maxDelay">The maximum synaptic delay.</param>
        /// <param name="analogSourceCfg">The configuration of the synapse connecting analog presynaptic neuron.</param>
        /// <param name="spikingSourceCfg">The configuration of the synapse connecting spiking presynaptic neuron.</param>
        public SynapseATIndifferentSettings(Synapse.SynapticDelayMethod delayMethod = DefaultDelayMethod,
                                            int maxDelay = DefaultMaxDelay,
                                            AnalogSourceSettings analogSourceCfg = null,
                                            SpikingSourceATIndifferentSettings spikingSourceCfg = null
                                            )
        {
            DelayMethod = delayMethod;
            MaxDelay = maxDelay;
            AnalogSourceCfg = analogSourceCfg == null ? new AnalogSourceSettings() : (AnalogSourceSettings)analogSourceCfg.DeepClone();
            SpikingSourceCfg = spikingSourceCfg == null ? new SpikingSourceATIndifferentSettings() : (SpikingSourceATIndifferentSettings)spikingSourceCfg.DeepClone();
            Check();
            return;
        }


        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public SynapseATIndifferentSettings(SynapseATIndifferentSettings source)
            : this(source.DelayMethod, source.MaxDelay, source.AnalogSourceCfg, source.SpikingSourceCfg)
        {

            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public SynapseATIndifferentSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            DelayMethod = (Synapse.SynapticDelayMethod)Enum.Parse(typeof(Synapse.SynapticDelayMethod), settingsElem.Attribute("delayMethod").Value, true);
            MaxDelay = int.Parse(settingsElem.Attribute("maxDelay").Value, CultureInfo.InvariantCulture);
            XElement analogSourceElem = settingsElem.Elements("analogSource").FirstOrDefault();
            AnalogSourceCfg = analogSourceElem == null ? new AnalogSourceSettings() : new AnalogSourceSettings(analogSourceElem);
            XElement spikingSourceElem = settingsElem.Elements("spikingSource").FirstOrDefault();
            SpikingSourceCfg = spikingSourceElem == null ? new SpikingSourceATIndifferentSettings() : new SpikingSourceATIndifferentSettings(spikingSourceElem);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultDelayMethod { get { return (DelayMethod == DefaultDelayMethod); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultMaxDelay { get { return (MaxDelay == DefaultMaxDelay); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultAnalogSourceCfg { get { return AnalogSourceCfg.ContainsOnlyDefaults; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultSpikingSourceCfg { get { return SpikingSourceCfg.ContainsOnlyDefaults; } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultDelayMethod &&
                       IsDefaultMaxDelay &&
                       IsDefaultAnalogSourceCfg &&
                       IsDefaultSpikingSourceCfg;
            }
        }


        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            if (MaxDelay < 0)
            {
                throw new ArgumentException($"Invalid MaxDelay {MaxDelay.ToString(CultureInfo.InvariantCulture)}. MaxDelay must be GE to 0.", "MaxDelay");
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new SynapseATIndifferentSettings(this);
        }

        /// <inheritdoc />
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
            if (!suppressDefaults || !IsDefaultAnalogSourceCfg)
            {
                rootElem.Add(AnalogSourceCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultSpikingSourceCfg)
            {
                rootElem.Add(SpikingSourceCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("indifferent", suppressDefaults);
        }

    }//SynapseATIndifferentSettings

}//Namespace

