using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration of an excitatory synapse connecting postsynaptic hidden spiking neuron.
    /// </summary>
    [Serializable]
    public class SynapseSTExcitatorySettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "SynapseSTExcitatoryType";

        //Default values
        /// <summary>
        /// The default delay method.
        /// </summary>
        const Synapse.SynapticDelayMethod DefaultDelayMethod = Synapse.SynapticDelayMethod.Random;
        /// <summary>
        /// The default maximum delay.
        /// </summary>
        const int DefaultMaxDelay = 0;
        /// <summary>
        /// The default relative share.
        /// </summary>
        const double DefaultRelShare = 4;

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
        /// The relative share.
        /// </summary>
        public double RelShare { get; }

        /// <summary>
        /// The configuration of the synapse connecting analog presynaptic neuron.
        /// </summary>
        public AnalogSourceSettings AnalogSourceCfg { get; }

        /// <summary>
        /// The configuration of the synapse connecting spiking presynaptic neuron.
        /// </summary>
        public SpikingSourceSTExcitatorySettings SpikingSourceCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="delayMethod">The synaptic delay method.</param>
        /// <param name="maxDelay">The maximum synaptic delay.</param>
        /// <param name="relShare">The relative share.</param>
        /// <param name="analogSourceCfg">The configuration of the synapse connecting analog presynaptic neuron.</param>
        /// <param name="spikingSourceCfg">The configuration of the synapse connecting spiking presynaptic neuron.</param>
        public SynapseSTExcitatorySettings(Synapse.SynapticDelayMethod delayMethod = DefaultDelayMethod,
                                           int maxDelay = DefaultMaxDelay,
                                           double relShare = DefaultRelShare,
                                           AnalogSourceSettings analogSourceCfg = null,
                                           SpikingSourceSTExcitatorySettings spikingSourceCfg = null
                                           )
        {
            DelayMethod = delayMethod;
            MaxDelay = maxDelay;
            RelShare = relShare;
            AnalogSourceCfg = analogSourceCfg == null ? new AnalogSourceSettings() : (AnalogSourceSettings)analogSourceCfg.DeepClone();
            SpikingSourceCfg = spikingSourceCfg == null ? new SpikingSourceSTExcitatorySettings() : (SpikingSourceSTExcitatorySettings)spikingSourceCfg.DeepClone();
            Check();
            return;
        }


        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public SynapseSTExcitatorySettings(SynapseSTExcitatorySettings source)
            : this(source.DelayMethod, source.MaxDelay, source.RelShare, source.AnalogSourceCfg, source.SpikingSourceCfg)
        {

            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public SynapseSTExcitatorySettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            DelayMethod = (Synapse.SynapticDelayMethod)Enum.Parse(typeof(Synapse.SynapticDelayMethod), settingsElem.Attribute("delayMethod").Value, true);
            MaxDelay = int.Parse(settingsElem.Attribute("maxDelay").Value, CultureInfo.InvariantCulture);
            RelShare = double.Parse(settingsElem.Attribute("relShare").Value, CultureInfo.InvariantCulture);
            XElement analogSourceElem = settingsElem.Elements("analogSource").FirstOrDefault();
            AnalogSourceCfg = analogSourceElem == null ? new AnalogSourceSettings() : new AnalogSourceSettings(analogSourceElem);
            XElement spikingSourceElem = settingsElem.Elements("spikingSource").FirstOrDefault();
            SpikingSourceCfg = spikingSourceElem == null ? new SpikingSourceSTExcitatorySettings() : new SpikingSourceSTExcitatorySettings(spikingSourceElem);
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
        public bool IsDefaultRelShare { get { return (RelShare == DefaultRelShare); } }

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
                       IsDefaultRelShare &&
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
            if (RelShare <= 0)
            {
                throw new ArgumentException($"Invalid RelShare {RelShare.ToString(CultureInfo.InvariantCulture)}. RelShare must be GT 0.", "RelShare");
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new SynapseSTExcitatorySettings(this);
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
            if (!suppressDefaults || !IsDefaultRelShare)
            {
                rootElem.Add(new XAttribute("relShare", RelShare.ToString(CultureInfo.InvariantCulture)));
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
            return GetXml("excitatory", suppressDefaults);
        }

    }//SynapseSTExcitatorySettings

}//Namespace

