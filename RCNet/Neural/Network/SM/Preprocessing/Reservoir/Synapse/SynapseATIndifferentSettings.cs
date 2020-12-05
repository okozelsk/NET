using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration of a synapse providing indifferent signal to hidden analog neuron
    /// </summary>
    [Serializable]
    public class SynapseATIndifferentSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "SynapseATIndifferentType";

        //Default values
        /// <summary>
        /// Default delay method
        /// </summary>
        const Synapse.SynapticDelayMethod DefaultDelayMethod = Synapse.SynapticDelayMethod.Random;
        /// <summary>
        /// Default maximum delay
        /// </summary>
        const int DefaultMaxDelay = 0;

        //Attribute properties
        /// <summary>
        /// Specifies how to decide synaptic delay
        /// </summary>
        public Synapse.SynapticDelayMethod DelayMethod { get; }

        /// <summary>
        /// Maximum delay of the signal
        /// </summary>
        public int MaxDelay { get; }

        /// <summary>
        /// Configuration of synapse having analog source neuron
        /// </summary>
        public AnalogSourceSettings AnalogSourceCfg { get; }

        /// <summary>
        /// Configuration of synapse having spiking source neuron
        /// </summary>
        public SpikingSourceATIndifferentSettings SpikingSourceCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="delayMethod">Specifies how to decide synaptic delay</param>
        /// <param name="maxDelay">Maximum delay of the signal</param>
        /// <param name="analogSourceCfg">Configuration of synapse having analog source neuron</param>
        /// <param name="spikingSourceCfg">Configuration of synapse having spiking source neuron</param>
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
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public SynapseATIndifferentSettings(SynapseATIndifferentSettings source)
            : this(source.DelayMethod, source.MaxDelay, source.AnalogSourceCfg, source.SpikingSourceCfg)
        {

            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
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
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultDelayMethod { get { return (DelayMethod == DefaultDelayMethod); } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultMaxDelay { get { return (MaxDelay == DefaultMaxDelay); } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultAnalogSourceCfg { get { return AnalogSourceCfg.ContainsOnlyDefaults; } }

        /// <summary>
        /// Checks the defaults
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

