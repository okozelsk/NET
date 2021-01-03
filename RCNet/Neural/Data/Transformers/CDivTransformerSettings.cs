using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Configuration of the CDivTransformer.
    /// </summary>
    [Serializable]
    public class CDivTransformerSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "CDivTransformerType";
        //Default values
        /// <summary>
        /// The default value of the constant numerator.
        /// </summary>
        public const double DefaultC = 1d;

        //Attribute properties
        /// <summary>
        /// The name of the input field to be transformed.
        /// </summary>
        public string InputFieldName { get; }

        /// <summary>
        /// The constant numerator.
        /// </summary>
        public double C { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="inputFieldName">The name of the input field to be transformed.</param>
        /// <param name="c">The constant numerator.</param>
        public CDivTransformerSettings(string inputFieldName, double c)
        {
            InputFieldName = inputFieldName;
            C = c;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public CDivTransformerSettings(CDivTransformerSettings source)
            : this(source.InputFieldName, source.C)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public CDivTransformerSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            InputFieldName = settingsElem.Attribute("fieldName").Value;
            C = double.Parse(settingsElem.Attribute("c").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultC { get { return (C == DefaultC); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }


        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (InputFieldName.Length == 0)
            {
                throw new ArgumentException($"Name of the input field must be specified.", "InputFieldName");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new CDivTransformerSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("fieldName", InputFieldName)
                                             );
            if (!suppressDefaults || !IsDefaultC)
            {
                rootElem.Add(new XAttribute("c", C.ToString(CultureInfo.InvariantCulture)));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("cdiv", suppressDefaults);
        }

    }//CDivTransformerSettings

}//Namespace
