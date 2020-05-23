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
    /// Setup parameters for the power transformer
    /// </summary>
    [Serializable]
    public class PowerTransformerSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PowerTransformerType";
        //Default values
        /// <summary>
        /// Default exponent
        /// </summary>
        public const double DefaultExponent = 0.5d;
        /// <summary>
        /// Default keep sign
        /// </summary>
        public const bool DefaultKeepSign = true;

        //Attribute properties
        /// <summary>
        /// Name of the input field to be transformed
        /// </summary>
        public string InputFieldName { get; }

        /// <summary>
        /// Exponent
        /// </summary>
        public double Exponent { get; }

        /// <summary>
        /// Specifies if to keep original sign
        /// </summary>
        public bool KeepSign { get; }


        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="inputFieldName">Name of the input field to be transformed</param>
        /// <param name="exponent">Exponent</param>
        /// <param name="keepSign">Specifies if to keep original sign</param>
        public PowerTransformerSettings(string inputFieldName, double exponent = DefaultExponent, bool keepSign = DefaultKeepSign)
        {
            InputFieldName = inputFieldName;
            Exponent = exponent;
            KeepSign = keepSign;
            Check();
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public PowerTransformerSettings(PowerTransformerSettings source)
            :this(source.InputFieldName, source.Exponent, source.KeepSign)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// Content of xml element is always validated against the xml schema.
        /// </summary>
        /// <param name="elem">Xml data containing settings</param>
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
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultExponent { get { return Exponent == DefaultExponent; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultKeepSign { get { return KeepSign == DefaultKeepSign; } }

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

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new PowerTransformerSettings(this);
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

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("power", suppressDefaults);
        }

    }//PowerTransformerSettings

}//Namespace
