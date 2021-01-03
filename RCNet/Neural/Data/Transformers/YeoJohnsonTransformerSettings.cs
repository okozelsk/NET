using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Configuration of the YeoJohnsonTransformer.
    /// </summary>
    [Serializable]
    public class YeoJohnsonTransformerSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "YeoJohnsonTransformerType";

        //Attribute properties
        /// <summary>
        /// The name of the input field to be transformed.
        /// </summary>
        public string InputFieldName { get; }

        /// <summary>
        /// The lambda exponent.
        /// </summary>
        public double Lambda { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="inputFieldName">The name of the input field to be transformed.</param>
        /// <param name="lambda">The lambda exponent.</param>
        public YeoJohnsonTransformerSettings(string inputFieldName, double lambda)
        {
            Lambda = lambda;
            InputFieldName = inputFieldName;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public YeoJohnsonTransformerSettings(YeoJohnsonTransformerSettings source)
            : this(source.InputFieldName, source.Lambda)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public YeoJohnsonTransformerSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            InputFieldName = settingsElem.Attribute("fieldName").Value;
            Lambda = double.Parse(settingsElem.Attribute("lambda").Value, CultureInfo.InvariantCulture);
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
            if (InputFieldName.Length == 0)
            {
                throw new ArgumentException($"Name of the input field must be specified.", "InputFieldName");
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new YeoJohnsonTransformerSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("fieldName", InputFieldName),
                                             new XAttribute("lambda", Lambda.ToString(CultureInfo.InvariantCulture))
                                             );
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("yeoJohnson", suppressDefaults);
        }

    }//YeoJohnsonTransformerSettings

}//Namespace
