using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Generators
{
    /// <summary>
    /// Configuration of the SinusoidalGenerator.
    /// </summary>
    [Serializable]
    public class SinusoidalGeneratorSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "SinusoidalGeneratorType";
        //Default values
        /// <summary>
        /// The default value of the phase shift.
        /// </summary>
        public const double DefaultPhase = 0d;
        /// <summary>
        /// The default value of the frequency.
        /// </summary>
        public const double DefaultFreq = 1d;
        /// <summary>
        /// The default value of the amplitude.
        /// </summary>
        public const double DefaultAmpl = 1d;


        //Attribute properties
        /// <summary>
        /// The phase shift.
        /// </summary>
        public double Phase { get; }

        /// <summary>
        /// The frequency.
        /// </summary>
        public double Freq { get; }

        /// <summary>
        /// The amplitude.
        /// </summary>
        public double Ampl { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="phase">The phase shift.</param>
        /// <param name="freq">The frequency.</param>
        /// <param name="ampl">The amplitude.</param>
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
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public SinusoidalGeneratorSettings(SinusoidalGeneratorSettings source)
            : this(source.Phase, source.Freq, source.Ampl)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
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
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultPhase { get { return (Phase == DefaultPhase); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultFreq { get { return (Freq == DefaultFreq); } }

        /// <summary>
        /// Checks the defaults.
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
