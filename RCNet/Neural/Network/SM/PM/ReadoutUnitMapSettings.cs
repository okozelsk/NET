using RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor;
using System;
using System.Linq;
using System.Xml.Linq;

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
            : this(source.ReadoutUnitName, source.AllowedPredictorsCfg, source.AllowedPoolsCfg, source.AllowedInputFieldsCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public ReadoutUnitMapSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            ReadoutUnitName = settingsElem.Attribute("readoutUnitName").Value;
            XElement allowedPredictorsElem = settingsElem.Elements("allowedPredictors").FirstOrDefault();
            AllowedPredictorsCfg = allowedPredictorsElem == null ? null : new AllowedPredictorsSettings(allowedPredictorsElem);
            XElement allowedPoolsElem = settingsElem.Elements("allowedPools").FirstOrDefault();
            AllowedPoolsCfg = allowedPoolsElem == null ? null : new AllowedPoolsSettings(allowedPoolsElem);
            XElement allowedInputFieldsElem = settingsElem.Elements("allowedInputFields").FirstOrDefault();
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
        /// Determines whether the predictor's origin is allowed
        /// </summary>
        /// <param name="resName">Reservoir instance name</param>
        /// <param name="poolName">Pool name</param>
        private bool IsAllowedPredictorOrigin(string resName, string poolName)
        {
            if(AllowedPoolsCfg != null)
            {
                return AllowedPoolsCfg.IsAllowed(resName, poolName);
            }
            return false;
        }

        /// <summary>
        /// Determines whether Predictor is allowed
        /// </summary>
        /// <param name="predictorID">Predictor identificator</param>
        private bool IsAllowedPredictorID(PredictorsProvider.PredictorID predictorID)
        {
            if(AllowedPredictorsCfg != null)
            {
                return AllowedPredictorsCfg.IsAllowed(predictorID);
            }
            return false;
        }

        /// <summary>
        /// Determines whether the Predictor's origin and Predictor are both allowed
        /// </summary>
        /// <param name="resName">Reservoir instance name</param>
        /// <param name="poolName">Pool name</param>
        /// <param name="predictorID">Predictor identificator</param>
        public bool IsAllowedPredictor(string resName, string poolName, PredictorsProvider.PredictorID predictorID)
        {
            if(IsAllowedPredictorOrigin(resName, poolName))
            {
                return IsAllowedPredictorID(predictorID);
            }
            return false;
        }

        /// <summary>
        /// Determines whether given input field is allowed
        /// </summary>
        /// <param name="inputFieldName">Name of the input field</param>
        public bool IsAllowedInputField(string inputFieldName)
        {
            if(AllowedInputFieldsCfg != null)
            {
                return AllowedInputFieldsCfg.IsAllowed(inputFieldName);
            }
            return false;
        }

        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            if (ReadoutUnitName.Length == 0)
            {
                throw new ArgumentException($"Readout unit name can not be empty.", "ReadoutUnitName");
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
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
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
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("map", suppressDefaults);
        }


    }//ReadoutUnitMapSettings

}//Namespace
