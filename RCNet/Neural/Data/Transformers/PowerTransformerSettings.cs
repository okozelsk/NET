using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Configuration of the PowerTransformer.
    /// </summary>
    [Serializable]
    public class PowerTransformerSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "PowerTransformerType";
        //Default values
        /// <summary>
        /// The default exponent.
        /// </summary>
        public const double DefaultExponent = 0.5d;
        /// <summary>
        /// The default value of the parameter specifying whether to keep the original value sign.
        /// </summary>
        public const bool DefaultKeepSign = true;

        //Attribute properties
        /// <summary>
        /// The name of the input field to be transformed.
        /// </summary>
        public string InputFieldName { get; }

        /// <summary>
        /// The exponent.
        /// </summary>
        public double Exponent { get; }

        /// <summary>
        /// Specifies whether to keep the original value sign.
        /// </summary>
        public bool KeepSign { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="inputFieldName">The name of the input field to be transformed.</param>
        /// <param name="exponent">The exponent.</param>
        /// <param name="keepSign">Specifies whether to keep the original value sign.</param>
        public PowerTransformerSettings(string inputFieldName, double exponent = DefaultExponent, bool keepSign = DefaultKeepSign)
        {
            InputFieldName = inputFieldName;
            Exponent = exponent;
            KeepSign = keepSign;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public PowerTransformerSettings(PowerTransformerSettings source)
            : this(source.InputFieldName, source.Exponent, source.KeepSign)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public PowerTransformerSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            InputFieldName = settingsElem.Attribute("fieldName").Value;
            Exponent = double.Parse(settingsElem.Attribute("exponent").Value, CultureInfo.InvariantCulture);
            KeepSign = bool.Parse(settingsElem.Attribute("keepSign").Value);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultExponent { get { return Exponent == DefaultExponent; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultKeepSign { get { return KeepSign == DefaultKeepSign; } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return false; } }


        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            if (InputFieldName.Length == 0)
            {
                throw new ArgumentException($"Name of the input field must be specified.", "InputFieldName");
            }
            if (Exponent <= 0d)
            {
                throw new ArgumentException($"Invalid exponent {Exponent.ToString(CultureInfo.InvariantCulture)}. Exponent must be GT 0.", "Exponent");
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new PowerTransformerSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("fieldName", InputFieldName)
                                             );
            if (!suppressDefaults || !IsDefaultExponent)
            {
                rootElem.Add(new XAttribute("exponent", Exponent.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultKeepSign)
            {
                rootElem.Add(new XAttribute("keepSign", KeepSign.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("power", suppressDefaults);
        }

    }//PowerTransformerSettings

}//Namespace
