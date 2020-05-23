using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Generators
{
    /// <summary>
    /// Setup parameters for the Pulse signal generator
    /// </summary>
    [Serializable]
    public class PulseGeneratorSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PulseGeneratorType";

        //Attribute properties
        /// <summary>
        /// Pulse signal
        /// </summary>
        public double Signal { get; }

        /// <summary>
        /// Average period of the pulse
        /// </summary>
        public double AvgPeriod { get; }

        /// <summary>
        /// Pulse timing mode
        /// </summary>
        public PulseGenerator.TimingMode Mode { get; }

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="signal">Pulse signal</param>
        /// <param name="avgPeriod">Pulse average period</param>
        /// <param name="mode">Pulse timing mode</param>
        public PulseGeneratorSettings(double signal,
                                      double avgPeriod,
                                      PulseGenerator.TimingMode mode
                                      )
        {
            Signal = signal;
            AvgPeriod = Math.Abs(avgPeriod);
            Mode = mode;
            Check();
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public PulseGeneratorSettings(PulseGeneratorSettings source)
        {
            Signal = source.Signal;
            AvgPeriod = source.AvgPeriod;
            Mode = source.Mode;
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public PulseGeneratorSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Signal = double.Parse(settingsElem.Attribute("signal").Value, CultureInfo.InvariantCulture);
            AvgPeriod = Math.Abs(double.Parse(settingsElem.Attribute("avgPeriod").Value, CultureInfo.InvariantCulture));
            Mode = (PulseGenerator.TimingMode)Enum.Parse(typeof(PulseGenerator.TimingMode), settingsElem.Attribute("mode").Value);
            Check();
            return;
        }

        //Properties
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
            if (AvgPeriod < 1)
            {
                throw new ArgumentException($"Invalid AvgPeriod {AvgPeriod.ToString(CultureInfo.InvariantCulture)}. AvgPeriod must be GE to 1.", "AvgPeriod");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new PulseGeneratorSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            return Validate(new XElement(rootElemName, new XAttribute("signal", Signal.ToString(CultureInfo.InvariantCulture)),
                                                       new XAttribute("avgPeriod", AvgPeriod.ToString(CultureInfo.InvariantCulture)),
                                                       new XAttribute("mode", Mode.ToString())),
                                                       XsdTypeName);
        }

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("pulse", suppressDefaults);
        }

    }//PulseGeneratorSettings

}//Namespace
