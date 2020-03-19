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
    public class MWStatTransformerSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "MWStatTransformerType";

        //Attribute properties
        /// <summary>
        /// Name of the input field to be transformed
        /// </summary>
        public string InputFieldName { get; }

        /// <summary>
        /// Window size
        /// </summary>
        public int Window { get; }

        /// <summary>
        /// Requiered output
        /// </summary>
        public MWStatTransformer.OutputValue Output { get; }


        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="inputFieldName">Name of the input field to be transformed</param>
        /// <param name="window">Difference interval</param>
        /// <param name="output">Requiered output</param>
        public MWStatTransformerSettings(string inputFieldName, int window, MWStatTransformer.OutputValue output)
        {
            InputFieldName = inputFieldName;
            Window = window;
            Output = output;
            Check();
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public MWStatTransformerSettings(MWStatTransformerSettings source)
            :this(source.InputFieldName, source.Window, source.Output)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// Content of xml element is always validated against the xml schema.
        /// </summary>
        /// <param name="elem">Xml data containing settings</param>
        public MWStatTransformerSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            InputFieldName = settingsElem.Attribute("fieldName").Value;
            Window = int.Parse(settingsElem.Attribute("window").Value, CultureInfo.InvariantCulture);
            Output = (MWStatTransformer.OutputValue)Enum.Parse(typeof(MWStatTransformer.OutputValue), settingsElem.Attribute("output").Value, true);
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
            if (InputFieldName.Length == 0)
            {
                throw new Exception($"Name of the input field must be specified.");
            }
            if (Window < 1)
            {
                throw new Exception($"Invalid window size {Window.ToString(CultureInfo.InvariantCulture)}. Window size must be GE to 2.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new MWStatTransformerSettings(this);
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
                                             new XAttribute("fieldName", InputFieldName),
                                             new XAttribute("window", Window.ToString(CultureInfo.InvariantCulture)),
                                             new XAttribute("output", Output.ToString())
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
            return GetXml("stat", suppressDefaults);
        }

    }//MWStatTransformerSettings

}//Namespace
