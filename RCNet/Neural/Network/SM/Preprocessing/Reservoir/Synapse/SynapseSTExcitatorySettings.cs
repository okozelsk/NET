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
using System.Xml.XPath;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration parameters of a synapse providing excitatory signal to hidden spiking neuron
    /// </summary>
    [Serializable]
    public class SynapseSTExcitatorySettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "SynapseSTExcitatoryType";

        //Default values
        /// <summary>
        /// Default delay method
        /// </summary>
        const Synapse.SynapticDelayMethod DefaultDelayMethod = Synapse.SynapticDelayMethod.Random;
        /// <summary>
        /// Default maximum delay
        /// </summary>
        const int DefaultMaxDelay = 0;
        /// <summary>
        /// Default relative share
        /// </summary>
        const double DefaultRelShare = 4;

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
        /// Relative share
        /// </summary>
        public double RelShare { get; }

        /// <summary>
        /// Configuration of synapse having analog source neuron
        /// </summary>
        public AnalogSourceSettings AnalogSourceCfg { get; }

        /// <summary>
        /// Configuration of synapse having spiking source neuron
        /// </summary>
        public SpikingSourceSTExcitatorySettings SpikingSourceCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="delayMethod">Specifies how to decide synaptic delay</param>
        /// <param name="maxDelay">Maximum delay of the signal</param>
        /// <param name="relShare">Relative share</param>
        /// <param name="analogSourceCfg">Configuration of synapse having analog source neuron</param>
        /// <param name="spikingSourceCfg">Configuration of synapse having spiking source neuron</param>
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
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public SynapseSTExcitatorySettings(SynapseSTExcitatorySettings source)
            :this(source.DelayMethod, source.MaxDelay, source.RelShare, source.AnalogSourceCfg, source.SpikingSourceCfg)
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
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultDelayMethod { get { return (DelayMethod == DefaultDelayMethod); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultMaxDelay { get { return (MaxDelay == DefaultMaxDelay); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultRelShare { get { return (RelShare == DefaultRelShare); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultAnalogSourceCfg { get { return AnalogSourceCfg.ContainsOnlyDefaults; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultSpikingSourceCfg { get { return SpikingSourceCfg.ContainsOnlyDefaults; } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
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
        /// <summary>
        /// Checks consistency
        /// </summary>
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

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new SynapseSTExcitatorySettings(this);
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

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("excitatory", suppressDefaults);
        }

    }//SynapseSTExcitatorySettings

}//Namespace

