using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Coders.AnalogToSpiking
{
    /// <summary>
    /// Settings of an analog value to spikes coder
    /// </summary>
    [Serializable]
    public class A2SCoderSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "A2SCoderType";
        //Default values

        //Attribute properties
        /// <summary>
        /// Configuration of the method to be used for coding
        /// </summary>
        public IA2SCodingMethodSettings CodingMethodCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an itialized instance.
        /// </summary>
        /// <param name="codingMethodCfg">Configuration of the method to be used for coding</param>
        public A2SCoderSettings(IA2SCodingMethodSettings codingMethodCfg)
        {
            CodingMethodCfg = codingMethodCfg;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public A2SCoderSettings(A2SCoderSettings source)
            : this(source.CodingMethodCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance from the given xml element.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public A2SCoderSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            XElement codingMethodElem = settingsElem.Elements().First();
            switch(codingMethodElem.Name.LocalName)
            {
                case "horizontal":
                    CodingMethodCfg = new A2SHorizontalMethodSettings(codingMethodElem);
                    break;
                case "vertical":
                    CodingMethodCfg = new A2SVerticalMethodSettings(codingMethodElem);
                    break;
                case "none":
                    CodingMethodCfg = new A2SNoneMethodSettings(codingMethodElem);
                    break;
                default:
                    throw new ArgumentException($"Unexpected child element name {codingMethodElem.Name.LocalName.ToString(CultureInfo.InvariantCulture)}.", "elem");
            }
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return false;
            }
        }

        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            if (CodingMethodCfg == null)
            {
                throw new ArgumentNullException($"Coding method must be specified.", "CodingMethodCfg");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new A2SCoderSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName, ((RCNetBaseSettings)CodingMethodCfg).GetXml(suppressDefaults));
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("codingA2S", suppressDefaults);
        }

    }//A2SCoderSettings

}//Namespace

