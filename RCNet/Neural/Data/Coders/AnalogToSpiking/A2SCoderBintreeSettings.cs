using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Coders.AnalogToSpiking
{
    /// <summary>
    /// Settings of A2SCoderBintree coder
    /// </summary>
    [Serializable]
    public class A2SCoderBintreeSettings : RCNetBaseSettings, IA2SCoderSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "A2SCoderBintreeType";
        //Default values
        /// <summary>
        /// Default value of parameter specifying the length of the spike-code of the analog absolute value
        /// </summary>
        public const int DefaultAbsValCodeLength = 8;
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
        /// Specifies if to generate halved spike-code where one half is dedicated for bellow average values (-)
        /// and second half for above average values (+)
        /// </summary>
        public bool Halved { get; }

        //Constructors
        /// <summary>
        /// Creates an itialized instance.
        /// </summary>
        /// <param name="absValCodeLength">Length of the spike-code of the analog absolute value</param>
        /// <param name="halved">Specifies if to generate halved spike-code where one half is dedicated for bellow average values (-) and second half for above average values (+)</param>
        public A2SCoderBintreeSettings(int absValCodeLength = DefaultAbsValCodeLength,
                                             bool halved = DefaultHalved)
        {
            AbsValCodeLength = absValCodeLength;
            Halved = halved;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public A2SCoderBintreeSettings(A2SCoderBintreeSettings source)
            : this(source.AbsValCodeLength, source.Halved)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance from the given xml element.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public A2SCoderBintreeSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            AbsValCodeLength = int.Parse(settingsElem.Attribute("absValCodeLength").Value, CultureInfo.InvariantCulture);
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
        public bool IsDefaultHalved { get { return (Halved == DefaultHalved); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultAbsValCodeLength &&
                       IsDefaultHalved;
            }
        }

        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            if (AbsValCodeLength < 1 || AbsValCodeLength > 32)
            {
                throw new ArgumentException($"Invalid AbsValCodeLength {AbsValCodeLength.ToString(CultureInfo.InvariantCulture)}. AbsValCodeLength must be GE to 1 and LE to 32.", "AbsValCodeLength");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new A2SCoderBintreeSettings(this);
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
            return GetXml("bintree", suppressDefaults);
        }

    }//A2SCoderBintreeSettings

}//Namespace

