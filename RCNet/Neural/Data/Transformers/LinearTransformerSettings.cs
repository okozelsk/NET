using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Globalization;
using System.Reflection;
using RCNet.XmlTools;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Setup parameters for the two input fields linear transformer (a*X + b*Y)
    /// </summary>
    [Serializable]
    public class LinearTransformerSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "LinearTransformerType";
        //Default values
        /// <summary>
        /// Default value of the A coefficient
        /// </summary>
        public const double DefaultA = 1d;
        /// <summary>
        /// Default value of the B coefficient
        /// </summary>
        public const double DefaultB = 1d;


        //Attribute properties
        /// <summary>
        /// Name of the first (X) input field
        /// </summary>
        public string XInputFieldName { get; }

        /// <summary>
        /// Name of the second (Y) input field
        /// </summary>
        public string YInputFieldName { get; }

        /// <summary>
        /// The A coefficient
        /// </summary>
        public double A { get; }

        /// <summary>
        /// The B coefficient
        /// </summary>
        public double B { get; }


        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="xInputFieldName">Name of the first (X) input field</param>
        /// <param name="yInputFieldName">Name of the second (Y) input field</param>
        /// <param name="a">The A coefficient</param>
        /// <param name="b">The B coefficient</param>
        public LinearTransformerSettings(string xInputFieldName, string yInputFieldName, double a = DefaultA, double b = DefaultB)
        {
            XInputFieldName = xInputFieldName;
            YInputFieldName = yInputFieldName;
            A = a;
            B = b;
            Check();
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public LinearTransformerSettings(LinearTransformerSettings source)
            :this(source.XInputFieldName, source.YInputFieldName, source.A, source.B)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// Content of xml element is always validated against the xml schema.
        /// </summary>
        /// <param name="elem">Xml data containing settings</param>
        public LinearTransformerSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            XInputFieldName = settingsElem.Attribute("xFieldName").Value;
            YInputFieldName = settingsElem.Attribute("yFieldName").Value;
            A = double.Parse(settingsElem.Attribute("a").Value, CultureInfo.InvariantCulture);
            B = double.Parse(settingsElem.Attribute("b").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultA { get { return (A == DefaultA); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultB { get { return (B == DefaultB); } }


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
            if (XInputFieldName.Length == 0)
            {
                throw new ArgumentException($"Name of the input field X must be specified.", "XInputFieldName");
            }
            if (YInputFieldName.Length == 0)
            {
                throw new ArgumentException($"Name of the input field Y must be specified.", "YInputFieldName");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new LinearTransformerSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("xFieldName", XInputFieldName),
                                             new XAttribute("yFieldName", YInputFieldName)
                                             );
            if(!suppressDefaults || !IsDefaultA)
            {
                rootElem.Add(new XAttribute("a", A.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultB)
            {
                rootElem.Add(new XAttribute("b", B.ToString(CultureInfo.InvariantCulture)));
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
            return GetXml("linear", suppressDefaults);
        }

    }//LinearTransformerSettings

}//Namespace
