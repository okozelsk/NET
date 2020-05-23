using RCNet.Neural.Network.SM.Preprocessing.Neuron;
using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
    /// <summary>
    /// Configuration of an input connection
    /// </summary>
    [Serializable]
    public class InputConnSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPResInstanceInputConnectionType";
        //Default values
        /// <summary>
        /// Default spiking target density
        /// </summary>
        public const double DefaultSpikingTargetDensity = 1d;
        /// <summary>
        /// Default analog target density
        /// </summary>
        public const double DefaultAnalogTargetDensity = 1d;
        /// <summary>
        /// Default value of signaling restriction of associated input neuron
        /// </summary>
        public const NeuronCommon.NeuronSignalingRestrictionType DefaultSignalingRestriction = NeuronCommon.NeuronSignalingRestrictionType.AnalogOnly;

        /// <summary>
        /// Name of the input field
        /// </summary>
        public string InputFieldName { get; }

        /// <summary>
        /// Name of target pool
        /// </summary>
        public string PoolName { get; }

        /// <summary>
        /// Spiking target density
        /// </summary>
        public double SpikingTargetDensity { get; }

        /// <summary>
        /// Analog target density
        /// </summary>
        public double AnalogTargetDensity { get; }

        /// <summary>
        /// Signaling restriction of associated input neuron
        /// </summary>
        public NeuronCommon.NeuronSignalingRestrictionType SignalingRestriction { get; }

        //Constructors
        /// <summary>
        /// Creates an itialized instance.
        /// </summary>
        /// <param name="inputFieldName">Name of the input field</param>
        /// <param name="poolName">Name of target pool</param>
        /// <param name="spikingTargetDensity">Spiking target density</param>
        /// <param name="analogTargetDensity">Analog target density</param>
        /// <param name="signalingRestriction">Signaling restriction of associated input neuron</param>
        public InputConnSettings(string inputFieldName,
                                 string poolName,
                                 double spikingTargetDensity = DefaultSpikingTargetDensity,
                                 double analogTargetDensity = DefaultAnalogTargetDensity,
                                 NeuronCommon.NeuronSignalingRestrictionType signalingRestriction = DefaultSignalingRestriction
                                 )
        {
            InputFieldName = inputFieldName;
            PoolName = poolName;
            SpikingTargetDensity = spikingTargetDensity;
            AnalogTargetDensity = analogTargetDensity;
            SignalingRestriction = signalingRestriction;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public InputConnSettings(InputConnSettings source)
            : this(source.InputFieldName, source.PoolName, source.SpikingTargetDensity, source.AnalogTargetDensity, source.SignalingRestriction)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance from the given xml element.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public InputConnSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            InputFieldName = settingsElem.Attribute("inputFieldName").Value;
            PoolName = settingsElem.Attribute("poolName").Value;
            SpikingTargetDensity = double.Parse(settingsElem.Attribute("spikingTargetDensity").Value, CultureInfo.InvariantCulture);
            AnalogTargetDensity = double.Parse(settingsElem.Attribute("analogTargetDensity").Value, CultureInfo.InvariantCulture);
            SignalingRestriction = (NeuronCommon.NeuronSignalingRestrictionType)Enum.Parse(typeof(NeuronCommon.NeuronSignalingRestrictionType), settingsElem.Attribute("signalingRestriction").Value, true);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultSpikingTargetDensity { get { return (SpikingTargetDensity == DefaultSpikingTargetDensity); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultAnalogTargetDensity { get { return (AnalogTargetDensity == DefaultAnalogTargetDensity); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultSignalingRestriction { get { return (SignalingRestriction == DefaultSignalingRestriction); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            if (InputFieldName.Length == 0)
            {
                throw new ArgumentException($"Input field name can not be empty.", "InputFieldName");
            }
            if (PoolName.Length == 0)
            {
                throw new ArgumentException($"Pool name can not be empty.", "PoolName");
            }
            if (SpikingTargetDensity < 0 || SpikingTargetDensity > 1)
            {
                throw new ArgumentException($"Invalid SpikingTargetDensity ({SpikingTargetDensity.ToString(CultureInfo.InvariantCulture)}). SpikingTargetDensity must be GE to 0 and LE to 1.", "SpikingTargetDensity");
            }
            if (AnalogTargetDensity < 0 || AnalogTargetDensity > 1)
            {
                throw new ArgumentException($"Invalid AnalogTargetDensity ({AnalogTargetDensity.ToString(CultureInfo.InvariantCulture)}). AnalogTargetDensity must be GE to 0 and LE to 1.", "AnalogTargetDensity");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new InputConnSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("inputFieldName", InputFieldName),
                                             new XAttribute("poolName", PoolName)
                                             );
            if (!suppressDefaults || !IsDefaultSpikingTargetDensity)
            {
                rootElem.Add(new XAttribute("spikingTargetDensity", SpikingTargetDensity.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultAnalogTargetDensity)
            {
                rootElem.Add(new XAttribute("analogTargetDensity", AnalogTargetDensity.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultSignalingRestriction)
            {
                rootElem.Add(new XAttribute("signalingRestriction", SignalingRestriction.ToString()));
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
            return GetXml("connection", suppressDefaults);
        }

    }//InputConnSettings

}//Namespace

