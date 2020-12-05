using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Configuration of the LogTransformer
    /// </summary>
    [Serializable]
    public class LogTransformerSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "LogTransformerType";
        //Default values
        /// <summary>
        /// Default base
        /// </summary>
        public const double DefaultBase = Math.E;

        //Attribute properties
        /// <summary>
        /// Name of the input field to be transformed
        /// </summary>
        public string InputFieldName { get; }

        /// <summary>
        /// Base
        /// </summary>
        public double Base { get; }

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="inputFieldName">Name of the input field to be transformed</param>
        /// <param name="b">Base</param>
        public LogTransformerSettings(string inputFieldName, double b = DefaultBase)
        {
            Base = b;
            InputFieldName = inputFieldName;
            Check();
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public LogTransformerSettings(LogTransformerSettings source)
            : this(source.InputFieldName, source.Base)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public LogTransformerSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            InputFieldName = settingsElem.Attribute("fieldName").Value;
            Base = double.Parse(settingsElem.Attribute("base").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultBase { get { return Base == DefaultBase; } }

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
            if (Base <= 0d)
            {
                throw new ArgumentException($"Invalid base {Base.ToString(CultureInfo.InvariantCulture)}. Base must be GT 0.", "Base");
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new LogTransformerSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("fieldName", InputFieldName)
                                             );
            if (!suppressDefaults || !IsDefaultBase)
            {
                rootElem.Add(new XAttribute("base", Base.ToString(CultureInfo.InvariantCulture)));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("log", suppressDefaults);
        }

    }//LogTransformerSettings

}//Namespace
