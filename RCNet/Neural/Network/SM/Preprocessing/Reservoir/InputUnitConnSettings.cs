using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using RCNet.Extensions;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Synapse;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
    /// <summary>
    /// Configuration of an input unit connection
    /// </summary>
    [Serializable]
    public class InputUnitConnSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPResInstanceInputUnitConnectionType";
        /// <summary>
        /// Default analog coding method
        /// </summary>
        public const InputUnit.AnalogCodingMethod DefaultAnalogCoding = InputUnit.AnalogCodingMethod.Actual;
        /// <summary>
        /// Default value of parameter specifying if to opposite amplitude of the input
        /// </summary>
        public const bool DefaultOppositeAmplitude = false;
        /// <summary>
        /// Default value of signaling restriction of associated input neuron
        /// </summary>
        public const NeuronCommon.NeuronSignalingRestrictionType DefaultSignalingRestriction = NeuronCommon.NeuronSignalingRestrictionType.AnalogOnly;

        /// <summary>
        /// Name of target pool
        /// </summary>
        public string PoolName { get; }

        /// <summary>
        /// Specifies what part of trgeted neurons will be connected
        /// </summary>
        public double Density { get; }

        /// <summary>
        /// Analog coding method to be used
        /// </summary>
        public InputUnit.AnalogCodingMethod AnalogCoding { get; }

        /// <summary>
        /// Specifies if to opposite amplitude of the input
        /// </summary>
        public bool OppositeAmplitude { get; }

        /// <summary>
        /// Signaling restriction of associated input neuron
        /// </summary>
        public NeuronCommon.NeuronSignalingRestrictionType SignalingRestriction { get; }

        /// <summary>
        /// Input neuron to target pool's neuron synapse settings
        /// </summary>
        public InputSynapseSettings SynapseCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an itialized instance.
        /// </summary>
        /// <param name="poolName">Name of target pool</param>
        /// <param name="density">Specifies what part of trgeted neurons will be connected</param>
        /// <param name="analogCoding">Analog coding method to be used</param>
        /// <param name="oppositeAmplitude">Specifies if to opposite amplitude of the input</param>
        /// <param name="signalingRestriction">Signaling restriction of associated input neuron</param>
        /// <param name="synapseCfg">Input neuron to target pool's neuron synapse settings</param>
        public InputUnitConnSettings(string poolName,
                                 double density,
                                 InputUnit.AnalogCodingMethod analogCoding = DefaultAnalogCoding,
                                 bool oppositeAmplitude = DefaultOppositeAmplitude,
                                 NeuronCommon.NeuronSignalingRestrictionType signalingRestriction = DefaultSignalingRestriction,
                                 InputSynapseSettings synapseCfg = null
                                 )
        {
            PoolName = poolName;
            Density = density;
            AnalogCoding = analogCoding;
            OppositeAmplitude = oppositeAmplitude;
            SignalingRestriction = signalingRestriction;
            SynapseCfg = synapseCfg == null ? new InputSynapseSettings() : (InputSynapseSettings)synapseCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public InputUnitConnSettings(InputUnitConnSettings source)
            : this(source.PoolName, source.Density, source.AnalogCoding, source.OppositeAmplitude,
                  source.SignalingRestriction, source.SynapseCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance from the given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing the settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public InputUnitConnSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            PoolName = settingsElem.Attribute("poolName").Value;
            Density = double.Parse(settingsElem.Attribute("density").Value, CultureInfo.InvariantCulture);
            AnalogCoding = InputUnit.ParseAnalogCodingMethod(settingsElem.Attribute("coding").Value);
            OppositeAmplitude = bool.Parse(settingsElem.Attribute("oppositeAmplitude").Value);
            SignalingRestriction = NeuronCommon.ParseNeuronSignalingRestriction(settingsElem.Attribute("signalingRestriction").Value);
            XElement synapseElem = settingsElem.Descendants("synapse").FirstOrDefault();
            SynapseCfg = synapseElem == null ? new InputSynapseSettings() : new InputSynapseSettings(synapseElem);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultAnalogCoding { get { return (AnalogCoding == DefaultAnalogCoding); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultOppositeAmplitude { get { return (OppositeAmplitude == DefaultOppositeAmplitude); } }

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
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if(PoolName.Length == 0)
            {
                throw new Exception($"Pool name can not be empty.");
            }
            if (Density < 0)
            {
                throw new Exception($"Invalid Density {Density.ToString(CultureInfo.InvariantCulture)}. Density must be GE to 0 and LE to 1.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new InputUnitConnSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName, new XAttribute("poolName", PoolName),
                                                           new XAttribute("density", Density.ToString(CultureInfo.InvariantCulture)));
            if (!suppressDefaults || !IsDefaultAnalogCoding)
            {
                rootElem.Add(new XAttribute("coding", AnalogCoding.ToString()));
            }
            if (!suppressDefaults || !IsDefaultOppositeAmplitude)
            {
                rootElem.Add(new XAttribute("oppositeAmplitude", OppositeAmplitude.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
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
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("connection", suppressDefaults);
        }

    }//InputUnitConnSettings

}//Namespace

