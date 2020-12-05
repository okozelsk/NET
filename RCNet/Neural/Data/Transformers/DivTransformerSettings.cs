using System;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Configuration of the DivTransformer
    /// </summary>
    [Serializable]
    public class DivTransformerSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "DivTransformerType";

        //Attribute properties
        /// <summary>
        /// Name of the first (X) input field (numerator)
        /// </summary>
        public string XInputFieldName { get; }

        /// <summary>
        /// Name of the second (Y) input field (denominator)
        /// </summary>
        public string YInputFieldName { get; }

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="xInputFieldName">Name of the first (X) input field (numerator)</param>
        /// <param name="yInputFieldName">Name of the second (Y) input field (denominator)</param>
        public DivTransformerSettings(string xInputFieldName, string yInputFieldName)
        {
            XInputFieldName = xInputFieldName;
            YInputFieldName = yInputFieldName;
            Check();
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public DivTransformerSettings(DivTransformerSettings source)
            : this(source.XInputFieldName, source.YInputFieldName)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public DivTransformerSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            XInputFieldName = settingsElem.Attribute("xFieldName").Value;
            YInputFieldName = settingsElem.Attribute("yFieldName").Value;
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
            return new DivTransformerSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("xFieldName", XInputFieldName),
                                             new XAttribute("yFieldName", YInputFieldName)
                                             );
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("div", suppressDefaults);
        }

    }//DivTransformerSettings

}//Namespace
