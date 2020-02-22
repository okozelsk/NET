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
    /// Setup parameters for the Mackey-Glass signal generator
    /// </summary>
    [Serializable]
    public class MackeyGlassGeneratorSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "MackeyGlassGeneratorType";
        //Default values
        /// <summary>
        /// Default value of tau argument
        /// </summary>
        public const int DefaultTau = 18;
        /// <summary>
        /// Default value of b argument
        /// </summary>
        public const double DefaultB = 0.1d;
        /// <summary>
        /// Default value of c argument
        /// </summary>
        public const double DefaultC = 0.2d;



        //Attribute properties
        /// <summary>
        /// Tau (backward deepness 2->18)
        /// </summary>
        public int Tau { get; }

        /// <summary>
        /// b coefficient
        /// </summary>
        public double B { get; }

        /// <summary>
        /// c coefficient
        /// </summary>
        public double C { get; }


        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="tau">Tau (backward deepness 2-18)</param>
        /// <param name="b">b coefficient</param>
        /// <param name="c">c coefficient</param>
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
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public MackeyGlassGeneratorSettings(MackeyGlassGeneratorSettings source)
        {
            Tau = source.Tau;
            B = source.B;
            C = source.C;
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// Content of xml element is always validated against the xml schema.
        /// </summary>
        /// <param name="elem">Xml data containing settings</param>
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
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultTau { get { return (Tau == DefaultTau); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultB { get { return (B == DefaultB); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultC { get { return (C == DefaultC); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return IsDefaultTau && IsDefaultB && IsDefaultC; } }


        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (Tau < 2 || Tau > 18)
            {
                throw new Exception($"Invalid Tau {Tau.ToString(CultureInfo.InvariantCulture)}. Tau must be GE to 2 and LE to 18.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new MackeyGlassGeneratorSettings(this);
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

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("mackeyGlass", suppressDefaults);
        }

    }//MackeyGlassGeneratorSettings

}//Namespace
