using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Generators
{
    /// <summary>
    /// Configuration of the SinusoidalGenerator
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
            Check();
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public SinusoidalGeneratorSettings(SinusoidalGeneratorSettings source)
            :this(source.Phase, source.Freq, source.Ampl)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public SinusoidalGeneratorSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Phase = double.Parse(settingsElem.Attribute("phase").Value, CultureInfo.InvariantCulture);
            Freq = double.Parse(settingsElem.Attribute("freq").Value, CultureInfo.InvariantCulture);
            Ampl = double.Parse(settingsElem.Attribute("ampl").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultPhase { get { return (Phase == DefaultPhase); } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultFreq { get { return (Freq == DefaultFreq); } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultAmpl { get { return (Ampl == DefaultAmpl); } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return IsDefaultPhase && IsDefaultFreq && IsDefaultAmpl; } }

        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new SinusoidalGeneratorSettings(this);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("sinusoidal", suppressDefaults);
        }

    }//SinusoidalGeneratorSettings

}//Namespace
