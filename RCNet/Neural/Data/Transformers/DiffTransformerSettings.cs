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
    /// Setup parameters for the Difference transformer
    /// </summary>
    [Serializable]
    public class DiffTransformerSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "DiffTransformerType";
        //Default values
        /// <summary>
        /// Default value of interval argument
        /// </summary>
        public const int DefaultInterval = 1;



        //Attribute properties
        /// <summary>
        /// Name of the input field to be transformed
        /// </summary>
        public string InputFieldName { get; }

        /// <summary>
        /// Difference interval
        /// </summary>
        public int Interval { get; }


        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="inputFieldName">Name of the input field to be transformed</param>
        /// <param name="interval">Difference interval</param>
        public DiffTransformerSettings(string inputFieldName, int interval = DefaultInterval)
        {
            InputFieldName = inputFieldName;
            Interval = interval;
            Check();
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public DiffTransformerSettings(DiffTransformerSettings source)
            :this(source.InputFieldName, source.Interval)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// Content of xml element is always validated against the xml schema.
        /// </summary>
        /// <param name="elem">Xml data containing settings</param>
        public DiffTransformerSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            InputFieldName = settingsElem.Attribute("fieldName").Value;
            Interval = int.Parse(settingsElem.Attribute("interval").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultInterval { get { return (Interval == DefaultInterval); } }

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
            if (InputFieldName.Length == 0)
            {
                throw new Exception($"Name of the input field must be specified.");
            }
            if (Interval < 1)
            {
                throw new Exception($"Invalid difference interval {Interval.ToString(CultureInfo.InvariantCulture)}. Interval must be GE to 1.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new DiffTransformerSettings(this);
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
            if (!suppressDefaults || !IsDefaultInterval)
            {
                rootElem.Add(new XAttribute("interval", Interval.ToString(CultureInfo.InvariantCulture)));
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
            return GetXml("diff", suppressDefaults);
        }

    }//DiffTransformerSettings

}//Namespace
