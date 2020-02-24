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

namespace RCNet.Neural.Network.SM.PM
{
    /// <summary>
    /// Configuration of the predictors mapper's readout unit allowed map
    /// </summary>
    [Serializable]
    public class ReadoutUnitMapSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "SMMapperReadoutUnitMapType";

        //Attribute properties
        /// <summary>
        /// Name of the input field
        /// </summary>
        public string ReadoutUnitName { get; }

        /// <summary>
        /// Allowed predictors
        /// </summary>
        public AllowedPredictorsSettings AllowedPredictorsCfg { get; }

        /// <summary>
        /// Allowed pools
        /// </summary>
        public AllowedPoolsSettings AllowedPoolsCfg { get; }

        /// <summary>
        /// Allowed input fields
        /// </summary>
        public AllowedInputFieldsSettings AllowedInputFieldsCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="readoutUnitName">Name of the readout unit</param>
        /// <param name="allowedPredictorsCfg">Allowed predictors</param>
        /// <param name="allowedPoolsCfg">Allowed pools</param>
        /// <param name="allowedInputFieldsCfg">Allowed input fields</param>
        public ReadoutUnitMapSettings(string readoutUnitName,
                                      AllowedPredictorsSettings allowedPredictorsCfg,
                                      AllowedPoolsSettings allowedPoolsCfg,
                                      AllowedInputFieldsSettings allowedInputFieldsCfg)
        {
            ReadoutUnitName = readoutUnitName;
            AllowedPredictorsCfg = allowedPredictorsCfg == null ? null : (AllowedPredictorsSettings)allowedPredictorsCfg.DeepClone();
            AllowedPoolsCfg = allowedPoolsCfg == null ? null : (AllowedPoolsSettings)allowedPoolsCfg.DeepClone();
            AllowedInputFieldsCfg = allowedInputFieldsCfg == null ? null : (AllowedInputFieldsSettings)allowedInputFieldsCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ReadoutUnitMapSettings(ReadoutUnitMapSettings source)
            :this(source.ReadoutUnitName, source.AllowedPredictorsCfg, source.AllowedPoolsCfg, source.AllowedInputFieldsCfg)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public ReadoutUnitMapSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            ReadoutUnitName = settingsElem.Attribute("readoutUnitName").Value;
            XElement allowedPredictorsElem = settingsElem.Descendants("allowedPredictors").FirstOrDefault();
            AllowedPredictorsCfg = allowedPredictorsElem == null ? null : new AllowedPredictorsSettings(allowedPredictorsElem);
            XElement allowedPoolsElem = settingsElem.Descendants("allowedPools").FirstOrDefault();
            AllowedPoolsCfg = allowedPoolsElem == null ? null : new AllowedPoolsSettings(allowedPoolsElem);
            XElement allowedInputFieldsElem = settingsElem.Descendants("allowedInputFields").FirstOrDefault();
            AllowedInputFieldsCfg = allowedInputFieldsElem == null ? null : new AllowedInputFieldsSettings(allowedInputFieldsElem);
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
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (ReadoutUnitName.Length == 0)
            {
                throw new Exception($"Readout unit name can not be empty.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new ReadoutUnitMapSettings(this);
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
                                             new XAttribute("readoutUnitName", ReadoutUnitName)
                                             );
            if (AllowedPredictorsCfg != null && (!suppressDefaults || !AllowedPredictorsCfg.ContainsOnlyDefaults))
            {
                rootElem.Add(AllowedPredictorsCfg.GetXml(suppressDefaults));
            }
            if (AllowedPoolsCfg != null && (!suppressDefaults || !AllowedPoolsCfg.ContainsOnlyDefaults))
            {
                rootElem.Add(AllowedPoolsCfg.GetXml(suppressDefaults));
            }
            if (AllowedInputFieldsCfg != null && (!suppressDefaults || !AllowedInputFieldsCfg.ContainsOnlyDefaults))
            {
                rootElem.Add(AllowedInputFieldsCfg.GetXml(suppressDefaults));
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
            return GetXml("map", suppressDefaults);
        }


    }//ReadoutUnitMapSettings

}//Namespace
