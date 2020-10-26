using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Coders.AnalogToSpiking
{
    /// <summary>
    /// Settings of horizontal spike code
    /// </summary>
    [Serializable]
    public class A2SHorizontalMethodSettings : RCNetBaseSettings, IA2SCodingMethodSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "A2SCodingMethodHorizontalType";
        //Default values
        /// <summary>
        /// Default value of parameter specifying length of the half of spike code
        /// </summary>
        public const int DefaultHalfCodeLength = 16;
        /// <summary>
        /// Default value of parameter specifying threshold of the most sensitive spike
        /// </summary>
        public const double DefaultLowestThreshold = 1e-5;

        //Attribute properties
        /// <summary>
        /// Length of the half of spike code
        /// </summary>
        public int HalfCodeLength { get; }

        /// <summary>
        /// Threshold of the most sensitive spike
        /// </summary>
        public double LowestThreshold { get; }


        //Constructors
        /// <summary>
        /// Creates an itialized instance.
        /// </summary>
        /// <param name="halfCodeLength">Length of the half of spike code</param>
        /// <param name="lowestThreshold">Threshold of the most sensitive spike</param>
        public A2SHorizontalMethodSettings(int halfCodeLength = DefaultHalfCodeLength, double lowestThreshold = DefaultLowestThreshold)
        {
            HalfCodeLength = halfCodeLength;
            LowestThreshold = lowestThreshold;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public A2SHorizontalMethodSettings(A2SHorizontalMethodSettings source)
            : this(source.HalfCodeLength, source.LowestThreshold)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance from the given xml element.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public A2SHorizontalMethodSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            HalfCodeLength = int.Parse(settingsElem.Attribute("halfCodeLength").Value, CultureInfo.InvariantCulture);
            LowestThreshold = double.Parse(settingsElem.Attribute("lowestThreshold").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Way to convert an analog value to spikes
        /// </summary>
        public A2SCoder.CodingMethod Method { get { return A2SCoder.CodingMethod.Horizontal; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultHalfCodeLength { get { return (HalfCodeLength == DefaultHalfCodeLength); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultLowestThreshold { get { return (LowestThreshold == DefaultLowestThreshold); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultHalfCodeLength &&
                       IsDefaultLowestThreshold;
            }
        }

        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            if (HalfCodeLength < 1 || HalfCodeLength > 32)
            {
                throw new ArgumentException($"Invalid HalfCodeLength {HalfCodeLength.ToString(CultureInfo.InvariantCulture)}. HalfCodeLength must be GE to 1 and LE to 32.", "HalfCodeLength");
            }
            if (LowestThreshold <= 0 || LowestThreshold >= 1d)
            {
                throw new ArgumentException($"Invalid LowestThreshold {LowestThreshold.ToString(CultureInfo.InvariantCulture)}. LowestThreshold must be GT 0 and LT 1.", "LowestThreshold");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new A2SHorizontalMethodSettings(this);
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
            if (!suppressDefaults || !IsDefaultHalfCodeLength)
            {
                rootElem.Add(new XAttribute("halfCodeLength", HalfCodeLength.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultLowestThreshold)
            {
                rootElem.Add(new XAttribute("lowestThreshold", LowestThreshold.ToString(CultureInfo.InvariantCulture)));
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
            return GetXml("horizontal", suppressDefaults);
        }

    }//SpikeCodeHorizontalSettings

}//Namespace

