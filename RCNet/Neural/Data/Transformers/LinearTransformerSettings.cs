using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Configuration of the LinearTransformer
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
            : this(source.XInputFieldName, source.YInputFieldName, source.A, source.B)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
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
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultA { get { return (A == DefaultA); } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultB { get { return (B == DefaultB); } }


        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return false; } }


        //Methods
        /// <inheritdoc />
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

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new LinearTransformerSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("xFieldName", XInputFieldName),
                                             new XAttribute("yFieldName", YInputFieldName)
                                             );
            if (!suppressDefaults || !IsDefaultA)
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

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("linear", suppressDefaults);
        }

    }//LinearTransformerSettings

}//Namespace
