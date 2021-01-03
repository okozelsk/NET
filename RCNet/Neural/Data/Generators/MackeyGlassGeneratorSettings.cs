using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Generators
{
    /// <summary>
    /// Configuration of the MackeyGlassGenerator.
    /// </summary>
    [Serializable]
    public class MackeyGlassGeneratorSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "MackeyGlassGeneratorType";
        //Default values
        /// <summary>
        /// The default value of the tau parameter.
        /// </summary>
        public const int DefaultTau = 18;
        /// <summary>
        /// The default value of the b coefficient.
        /// </summary>
        public const double DefaultB = 0.1d;
        /// <summary>
        /// The default value of the c coefficient.
        /// </summary>
        public const double DefaultC = 0.2d;



        //Attribute properties
        /// <summary>
        /// The tau (the backward deepness 2..18).
        /// </summary>
        public int Tau { get; }

        /// <summary>
        /// The b coefficient.
        /// </summary>
        public double B { get; }

        /// <summary>
        /// The c coefficient.
        /// </summary>
        public double C { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="tau">The tau (the backward deepness 2..18).</param>
        /// <param name="b">The b coefficient.</param>
        /// <param name="c">The c coefficient.</param>
        public MackeyGlassGeneratorSettings(int tau = DefaultTau,
                                            double b = DefaultB,
                                            double c = DefaultC
                                            )
        {
            Tau = tau;
            B = b;
            C = c;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public MackeyGlassGeneratorSettings(MackeyGlassGeneratorSettings source)
            : this(source.Tau, source.B, source.C)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public MackeyGlassGeneratorSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Tau = int.Parse(settingsElem.Attribute("tau").Value, CultureInfo.InvariantCulture);
            B = double.Parse(settingsElem.Attribute("b").Value, CultureInfo.InvariantCulture);
            C = double.Parse(settingsElem.Attribute("c").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultTau { get { return (Tau == DefaultTau); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultB { get { return (B == DefaultB); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultC { get { return (C == DefaultC); } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return IsDefaultTau && IsDefaultB && IsDefaultC; } }


        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            if (Tau < 2 || Tau > 18)
            {
                throw new ArgumentException($"Invalid Tau {Tau.ToString(CultureInfo.InvariantCulture)}. Tau must be GE to 2 and LE to 18.", "Tau");
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new MackeyGlassGeneratorSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultTau)
            {
                rootElem.Add(new XAttribute("tau", Tau.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultB)
            {
                rootElem.Add(new XAttribute("b", B.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultC)
            {
                rootElem.Add(new XAttribute("c", C.ToString(CultureInfo.InvariantCulture)));
            }

            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("mackeyGlass", suppressDefaults);
        }

    }//MackeyGlassGeneratorSettings

}//Namespace
