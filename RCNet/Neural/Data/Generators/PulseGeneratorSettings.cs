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
    /// Setup parameters for the Pulse signal generator
    /// </summary>
    [Serializable]
    public class PulseGeneratorSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PulseGeneratorCfgType";

        //Enums
        /// <summary>
        /// Method of the pulse timing
        /// </summary>
        public enum TimingMode
        {
            /// <summary>
            /// Period of the pulses is constant
            /// </summary>
            Constant,
            /// <summary>
            /// Period of the pulses follows the Uniform distribution
            /// </summary>
            Uniform,
            /// <summary>
            /// Period of the pulses follows the Gaussian distribution
            /// </summary>
            Gaussian,
            /// <summary>
            /// Period of the pulses follows the Poisson (Exponential) distribution
            /// </summary>
            Poisson
        }

        //Attribute properties
        /// <summary>
        /// Pulse signal
        /// </summary>
        public double Signal { get; set; }

        /// <summary>
        /// Average period of the pulse
        /// </summary>
        public double AvgPeriod { get; set; }

        /// <summary>
        /// Pulse timing mode
        /// </summary>
        public TimingMode Mode { get; set; }

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="signal">Pulse signal</param>
        /// <param name="avgPeriod">Pulse average period</param>
        /// <param name="mode">Pulse timing mode</param>
        public PulseGeneratorSettings(double signal, double avgPeriod, TimingMode mode)
        {
            Signal = signal;
            AvgPeriod = Math.Abs(avgPeriod);
            Mode = mode;
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
        /// Creates the instance and initializes it from given xml element.
        /// Content of xml element is always validated against the xml schema.
        /// </summary>
        /// <param name="elem">Xml data containing settings</param>
        public PulseGeneratorSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Signal = double.Parse(settingsElem.Attribute("signal").Value, CultureInfo.InvariantCulture);
            AvgPeriod = Math.Abs(double.Parse(settingsElem.Attribute("avgPeriod").Value, CultureInfo.InvariantCulture));
            Mode = ParseTimingMode(settingsElem.Attribute("mode").Value);
            return;
        }

        //Methods
        //Static methods
        /// <summary>
        /// Parses the timing mode from a string code
        /// </summary>
        /// <param name="code">Mode code</param>
        public static TimingMode ParseTimingMode(string code)
        {
            switch (code.ToUpper())
            {
                case "CONSTANT": return TimingMode.Constant;
                case "UNIFORM": return TimingMode.Uniform;
                case "GAUSSIAN": return TimingMode.Gaussian;
                case "POISSON": return TimingMode.Poisson;
                default:
                    throw new ArgumentException($"Unsupported mode code {code}", "code");
            }
        }

        //Instance methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public PulseGeneratorSettings DeepClone()
        {
            return new PulseGeneratorSettings(this);
        }

    }//PulseGeneratorSettings

}//Namespace
