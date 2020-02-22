using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Globalization;
using System.Reflection;
using RCNet.XmlTools;

namespace RCNet.Neural.Data.Generators
{
    /// <summary>
    /// Setup parameters for the Sinusoidal signal generator
    /// </summary>
    [Serializable]
    public class SinusoidalGeneratorSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "SinusoidalGeneratorType";
        //Default values
        /// <summary>
        /// Default value of phase argument
        /// </summary>
        public const double DefaultPhase = 0d;
        /// <summary>
        /// Default value of freq argument
        /// </summary>
        public const double DefaultFreq = 1d;
        /// <summary>
        /// Default value of ampl argument
        /// </summary>
        public const double DefaultAmpl = 1d;


        //Attribute properties
        /// <summary>
        /// Phase shift
        /// </summary>
        public double Phase { get; }

        /// <summary>
        /// Frequency coefficient
        /// </summary>
        public double Freq { get; }

        /// <summary>
        /// Amplitude coefficient
        /// </summary>
        public double Ampl { get; }


        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="phase">Phase shift</param>
        /// <param name="freq">Frequency coefficient</param>
        /// <param name="ampl">Amplitude coefficient</param>
        public SinusoidalGeneratorSettings(double phase = DefaultPhase,
                                           double freq = DefaultFreq,
                                           double ampl = DefaultAmpl
                                           )
        {
            Phase = phase;
            Freq = freq;
            Ampl = ampl;
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public SinusoidalGeneratorSettings(SinusoidalGeneratorSettings source)
        {
            Phase = source.Phase;
            Freq = source.Freq;
            Ampl = source.Ampl;
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// Content of xml element is always validated against the xml schema.
        /// </summary>
        /// <param name="elem">Xml data containing settings</param>
        public SinusoidalGeneratorSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Phase = double.Parse(settingsElem.Attribute("phase").Value, CultureInfo.InvariantCulture);
            Freq = double.Parse(settingsElem.Attribute("freq").Value, CultureInfo.InvariantCulture);
            Ampl = double.Parse(settingsElem.Attribute("ampl").Value, CultureInfo.InvariantCulture);
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultPhase { get { return (Phase == DefaultPhase); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultFreq { get { return (Freq == DefaultFreq); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultAmpl { get { return (Ampl == DefaultAmpl); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return IsDefaultPhase && IsDefaultFreq && IsDefaultAmpl; } }

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new SinusoidalGeneratorSettings(this);
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
            if (!suppressDefaults || !IsDefaultPhase)
            {
                rootElem.Add(new XAttribute("phase", Phase.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultFreq)
            {
                rootElem.Add(new XAttribute("freq", Freq.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultAmpl)
            {
                rootElem.Add(new XAttribute("ampl", Ampl.ToString(CultureInfo.InvariantCulture)));
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
            return GetXml("sinusoidal", suppressDefaults);
        }

    }//SinusoidalGeneratorSettings

}//Namespace
