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
    /// Setup parameters for the two input fields division transformer
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
            :this(source.XInputFieldName, source.YInputFieldName)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// Content of xml element is always validated against the xml schema.
        /// </summary>
        /// <param name="elem">Xml data containing settings</param>
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
        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return false; } }


        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (XInputFieldName.Length == 0)
            {
                throw new Exception($"Name of the input field X must be specified.");
            }
            if (YInputFieldName.Length == 0)
            {
                throw new Exception($"Name of the input field Y must be specified.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new DivTransformerSettings(this);
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
            return GetXml("div", suppressDefaults);
        }

    }//DivTransformerSettings

}//Namespace
