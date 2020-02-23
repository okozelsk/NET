using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using RCNet.Extensions;
using RCNet.MathTools.Probability;
using RCNet.XmlTools;
using RCNet.RandomValue;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool;

namespace RCNet.Neural.Network.SM.PM
{
    /// <summary>
    /// Collection of predictors mapper's allowed input field settings
    /// </summary>
    [Serializable]
    public class AllowedInputFieldsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "SMMapperAllowedInputFieldsType";

        //Attribute properties
        /// <summary>
        /// Collection of pools settings
        /// </summary>
        public List<AllowedInputFieldSettings> AllowedInputFieldCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        private AllowedInputFieldsSettings()
        {
            AllowedInputFieldCfgCollection = new List<AllowedInputFieldSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="allowedInputFieldCfgCollection">Allowed input field settings collection</param>
        public AllowedInputFieldsSettings(IEnumerable<AllowedInputFieldSettings> allowedInputFieldCfgCollection)
            : this()
        {
            AddAllowedInputFields(allowedInputFieldCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="allowedInputFieldCfgCollection">Allowed input field settings collection</param>
        public AllowedInputFieldsSettings(params AllowedInputFieldSettings[] allowedInputFieldCfgCollection)
            : this()
        {
            AddAllowedInputFields(allowedInputFieldCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public AllowedInputFieldsSettings(AllowedInputFieldsSettings source)
            : this()
        {
            AddAllowedInputFields(source.AllowedInputFieldCfgCollection);
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing settings.</param>
        public AllowedInputFieldsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            AllowedInputFieldCfgCollection = new List<AllowedInputFieldSettings>();
            foreach (XElement fieldElem in settingsElem.Descendants("field"))
            {
                AllowedInputFieldCfgCollection.Add(new AllowedInputFieldSettings(fieldElem));
            }
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
            if (AllowedInputFieldCfgCollection.Count == 0)
            {
                throw new Exception($"At least one allowed input field configuration must be specified.");
            }
            //Uniqueness of pool references
            string[] names = new string[AllowedInputFieldCfgCollection.Count];
            names[0] = AllowedInputFieldCfgCollection[0].Name;
            for(int i = 1; i < AllowedInputFieldCfgCollection.Count; i++)
            {
                if (names.Contains(AllowedInputFieldCfgCollection[i].Name))
                {
                    throw new Exception($"Input field name {AllowedInputFieldCfgCollection[i].Name} is not unique.");
                }
                names[i] = AllowedInputFieldCfgCollection[i].Name;
            }
            return;
        }

        /// <summary>
        /// Adds cloned allowed input field configurations from given collection into the internal collection
        /// </summary>
        /// <param name="allowedInputFieldCfgCollection">Allowed input field settings collection</param>
        private void AddAllowedInputFields(IEnumerable<AllowedInputFieldSettings> allowedInputFieldCfgCollection)
        {
            foreach (AllowedInputFieldSettings allowedInputFieldCfg in allowedInputFieldCfgCollection)
            {
                AllowedInputFieldCfgCollection.Add((AllowedInputFieldSettings)allowedInputFieldCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new AllowedInputFieldsSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach (AllowedInputFieldSettings allowedInputFieldCfg in AllowedInputFieldCfgCollection)
            {
                rootElem.Add(allowedInputFieldCfg.GetXml(suppressDefaults));
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
            return GetXml("allowedInputFields", suppressDefaults);
        }

    }//AllowedInputFieldsSettings

}//Namespace
