using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Generators
{
    /// <summary>
    /// Configuration of the PulseGenerator.
    /// </summary>
    [Serializable]
    public class PulseGeneratorSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "PulseGeneratorType";

        //Attribute properties
        /// <summary>
        /// The pulse signal value.
        /// </summary>
        public double Signal { get; }

        /// <summary>
        /// The pulse average leak.
        /// </summary>
        public double AvgPeriod { get; }

        /// <summary>
        /// The pulse timing mode.
        /// </summary>
        public PulseGenerator.TimingMode Mode { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="signal">The pulse signal value.</param>
        /// <param name="avgPeriod">The pulse average leak.</param>
        /// <param name="mode">The pulse timing mode.</param>
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
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public PulseGeneratorSettings(PulseGeneratorSettings source)
            : this(source.Signal, source.AvgPeriod, source.Mode)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
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
        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            if (AvgPeriod < 1)
            {
                throw new ArgumentException($"Invalid AvgPeriod {AvgPeriod.ToString(CultureInfo.InvariantCulture)}. AvgPeriod must be GE to 1.", "AvgPeriod");
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new PulseGeneratorSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            return Validate(new XElement(rootElemName, new XAttribute("signal", Signal.ToString(CultureInfo.InvariantCulture)),
                                                       new XAttribute("avgPeriod", AvgPeriod.ToString(CultureInfo.InvariantCulture)),
                                                       new XAttribute("mode", Mode.ToString())),
                                                       XsdTypeName);
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("pulse", suppressDefaults);
        }

    }//PulseGeneratorSettings

}//Namespace
