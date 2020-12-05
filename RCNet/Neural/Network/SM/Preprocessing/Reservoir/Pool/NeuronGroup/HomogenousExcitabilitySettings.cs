using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool.NeuronGroup
{
    /// <summary>
    /// Configuration of spiking neuron's homogenous excitability
    /// </summary>
    [Serializable]
    public class HomogenousExcitabilitySettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "HomogenousExcitabilityType";
        //Default values
        /// <summary>
        /// Default excitatory strength
        /// </summary>
        public const double DefaultExcitatoryStrength = 0.75d;
        /// <summary>
        /// Default input strength ratio
        /// </summary>
        public const double DefaultInputRatio = 0.67d;
        /// <summary>
        /// Default inhibitory ratio
        /// </summary>
        public const double DefaultInhibitoryRatio = 0.25d;

        //Attribute properties
        /// <summary>
        /// Total excitatory strength (sum of inner excitatory synapses + sum of input synapses)
        /// </summary>
        public double ExcitatoryStrength { get; }

        /// <summary>
        /// Determines input strength. (input strength = InputRatio * ExcitatoryStrength)
        /// </summary>
        public double InputRatio { get; }

        /// <summary>
        /// Determines inhibitory strength (inhibitory strength = InhibitoryRatio * ExcitatoryStrength)
        /// </summary>
        public double InhibitoryRatio { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="excitatoryStrength">Total excitatory strength (sum of inner excitatory synapses + sum of input synapses)</param>
        /// <param name="inputRatio">Determines input strength. (input strength = InputRatio * ExcitatoryStrength)</param>
        /// <param name="inhibitoryRatio">Determines inhibitory strength (inhibitory strength = InhibitoryRatio * ExcitatoryStrength)</param>
        public HomogenousExcitabilitySettings(double excitatoryStrength = DefaultExcitatoryStrength,
                                              double inputRatio = DefaultInputRatio,
                                              double inhibitoryRatio = DefaultInhibitoryRatio
                                              )
        {
            ExcitatoryStrength = excitatoryStrength;
            InputRatio = inputRatio;
            InhibitoryRatio = inhibitoryRatio;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public HomogenousExcitabilitySettings(HomogenousExcitabilitySettings source)
            : this(source.ExcitatoryStrength, source.InputRatio, source.InhibitoryRatio)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings.</param>
        public HomogenousExcitabilitySettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            ExcitatoryStrength = double.Parse(settingsElem.Attribute("excitatoryStrength").Value, CultureInfo.InvariantCulture);
            InputRatio = double.Parse(settingsElem.Attribute("inputRatio").Value, CultureInfo.InvariantCulture);
            InhibitoryRatio = double.Parse(settingsElem.Attribute("inhibitoryRatio").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultExcitatoryStrength { get { return (ExcitatoryStrength == DefaultExcitatoryStrength); } }
        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultInputRatio { get { return (InputRatio == DefaultInputRatio); } }
        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultInhibitoryRatio { get { return (InhibitoryRatio == DefaultInhibitoryRatio); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultExcitatoryStrength &&
                       IsDefaultInputRatio &&
                       IsDefaultInhibitoryRatio;
            }
        }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (ExcitatoryStrength <= 0)
            {
                throw new ArgumentException($"Invalid total ExcitatoryStrength {ExcitatoryStrength.ToString(CultureInfo.InvariantCulture)}. ExcitatoryStrength must be GT 0.", "ExcitatoryStrength");
            }
            if (InputRatio < 0 || InputRatio > 1)
            {
                throw new ArgumentException($"Invalid InputRatio {InputRatio.ToString(CultureInfo.InvariantCulture)}. InputRatio must be GE to 0 and LE to 1.", "InputRatio");
            }
            if (InhibitoryRatio < 0 || InhibitoryRatio > 1)
            {
                throw new ArgumentException($"Invalid InhibitoryRatio {InhibitoryRatio.ToString(CultureInfo.InvariantCulture)}. InhibitoryRatio must be GE to 0 and LE to 1.", "InhibitoryRatio");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new HomogenousExcitabilitySettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultExcitatoryStrength)
            {
                rootElem.Add(new XAttribute("excitatoryStrength", ExcitatoryStrength.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultInputRatio)
            {
                rootElem.Add(new XAttribute("inputRatio", InputRatio.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultInhibitoryRatio)
            {
                rootElem.Add(new XAttribute("inhibitoryRatio", InhibitoryRatio.ToString(CultureInfo.InvariantCulture)));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("homogenousExcitability", suppressDefaults);
        }

    }//HomogenousExcitabilitySettings

}//Namespace
