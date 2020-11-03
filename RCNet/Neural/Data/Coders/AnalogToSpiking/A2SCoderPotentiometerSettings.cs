using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Coders.AnalogToSpiking
{
    /// <summary>
    /// Settings of A2SCoderPotentiometer coder
    /// </summary>
    [Serializable]
    public class A2SCoderPotentiometerSettings : RCNetBaseSettings, IA2SCoderSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "A2SCoderPotentiometerType";
        //Default values
        /// <summary>
        /// Default value of parameter specifying the length of the spike-code of the analog absolute value
        /// </summary>
        public const int DefaultAbsValCodeLength = 16;
        /// <summary>
        /// Default value of parameter specifying threshold of the most sensitive spike
        /// </summary>
        public const double DefaultLowestThreshold = 1e-5;
        /// <summary>
        /// Default value of parameter specifying if to generate halved spike-code where one half is dedicated for bellow average values (-)
        /// and second half for above average values (+)
        /// </summary>
        public const bool DefaultHalved = true;

        //Attribute properties
        /// <summary>
        /// Length of the spike-code of the analog absolute value
        /// </summary>
        public int AbsValCodeLength { get; }

        /// <summary>
        /// Threshold of the most sensitive spike
        /// </summary>
        public double LowestThreshold { get; }

        /// <summary>
        /// Specifies if to generate halved spike-code where one half is dedicated for bellow average values (-)
        /// and second half for above average values (+)
        /// </summary>
        public bool Halved { get; }

        //Constructors
        /// <summary>
        /// Creates an itialized instance.
        /// </summary>
        /// <param name="absValCodeLength">Length of the spike-code of the analog absolute value</param>
        /// <param name="lowestThreshold">Threshold of the most sensitive spike</param>
        /// <param name="halved">Specifies if to generate halved spike-code where one half is dedicated for bellow average values (-) and second half for above average values (+)</param>
        public A2SCoderPotentiometerSettings(int absValCodeLength = DefaultAbsValCodeLength,
                                             double lowestThreshold = DefaultLowestThreshold,
                                             bool halved = DefaultHalved)
        {
            AbsValCodeLength = absValCodeLength;
            LowestThreshold = lowestThreshold;
            Halved = halved;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public A2SCoderPotentiometerSettings(A2SCoderPotentiometerSettings source)
            : this(source.AbsValCodeLength, source.LowestThreshold, source.Halved)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance from the given xml element.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public A2SCoderPotentiometerSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            AbsValCodeLength = int.Parse(settingsElem.Attribute("absValCodeLength").Value, CultureInfo.InvariantCulture);
            LowestThreshold = double.Parse(settingsElem.Attribute("lowestThreshold").Value, CultureInfo.InvariantCulture);
            Halved = bool.Parse(settingsElem.Attribute("halved").Value);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultAbsValCodeLength { get { return (AbsValCodeLength == DefaultAbsValCodeLength); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultLowestThreshold { get { return (LowestThreshold == DefaultLowestThreshold); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultHalved { get { return (Halved == DefaultHalved); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultAbsValCodeLength &&
                       IsDefaultLowestThreshold &&
                       IsDefaultHalved;
            }
        }

        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            if (AbsValCodeLength < 1 || AbsValCodeLength > 128)
            {
                throw new ArgumentException($"Invalid AbsValCodeLength {AbsValCodeLength.ToString(CultureInfo.InvariantCulture)}. AbsValCodeLength must be GE to 1 and LE to 128.", "AbsValCodeLength");
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
            return new A2SCoderPotentiometerSettings(this);
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
            if (!suppressDefaults || !IsDefaultAbsValCodeLength)
            {
                rootElem.Add(new XAttribute("absValCodeLength", AbsValCodeLength.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultLowestThreshold)
            {
                rootElem.Add(new XAttribute("lowestThreshold", LowestThreshold.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultHalved)
            {
                rootElem.Add(new XAttribute("halved", Halved.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
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
            return GetXml("potentiometer", suppressDefaults);
        }

    }//A2SCoderPotentiometerSettings

}//Namespace

