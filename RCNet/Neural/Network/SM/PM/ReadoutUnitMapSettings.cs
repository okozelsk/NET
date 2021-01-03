using RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor;
using System;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.PM
{
    /// <summary>
    /// Configuration of the predictors mapper's readout unit mapping.
    /// </summary>
    [Serializable]
    public class ReadoutUnitMapSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "SMMapperReadoutUnitMapType";

        //Attribute properties
        /// <summary>
        /// The name of the readout unit.
        /// </summary>
        public string ReadoutUnitName { get; }

        /// <summary>
        /// The configuration of the allowed predictors.
        /// </summary>
        public AllowedPredictorsSettings AllowedPredictorsCfg { get; }

        /// <summary>
        /// The configuration of the allowed pools.
        /// </summary>
        public AllowedPoolsSettings AllowedPoolsCfg { get; }

        /// <summary>
        /// The configuration of the allowed input fields.
        /// </summary>
        public AllowedInputFieldsSettings AllowedInputFieldsCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="readoutUnitName">The name of the readout unit.</param>
        /// <param name="allowedPredictorsCfg">The configuration of the allowed predictors.</param>
        /// <param name="allowedPoolsCfg">The configuration of the allowed pools.</param>
        /// <param name="allowedInputFieldsCfg">The configuration of the allowed input fields.</param>
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
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public ReadoutUnitMapSettings(ReadoutUnitMapSettings source)
            : this(source.ReadoutUnitName, source.AllowedPredictorsCfg, source.AllowedPoolsCfg, source.AllowedInputFieldsCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
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
        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return false;
            }
        }


        //Methods
        /// <summary>
        /// Tests whether the predictor's origin is allowed.
        /// </summary>
        /// <param name="resName">The name of the reservoir instance.</param>
        /// <param name="poolName">The name of the pool.</param>
        private bool IsAllowedPredictorOrigin(string resName, string poolName)
        {
            if (AllowedPoolsCfg != null)
            {
                return AllowedPoolsCfg.IsAllowed(resName, poolName);
            }
            return false;
        }

        /// <summary>
        /// Tests whether the predictor is allowed.
        /// </summary>
        /// <param name="predictorID">An identifier of the predictor.</param>
        private bool IsAllowedPredictorID(PredictorsProvider.PredictorID predictorID)
        {
            if (AllowedPredictorsCfg != null)
            {
                return AllowedPredictorsCfg.IsAllowed(predictorID);
            }
            return false;
        }

        /// <summary>
        /// Tests whether the predictor's origin and predictor identifier are both allowed.
        /// </summary>
        /// <param name="resName">The name of the reservoir instance.</param>
        /// <param name="poolName">The name of the pool.</param>
        /// <param name="predictorID">An identifier of the predictor.</param>
        public bool IsAllowedPredictor(string resName, string poolName, PredictorsProvider.PredictorID predictorID)
        {
            if (IsAllowedPredictorOrigin(resName, poolName))
            {
                return IsAllowedPredictorID(predictorID);
            }
            return false;
        }

        /// <summary>
        /// Tests whether the input field is allowed.
        /// </summary>
        /// <param name="inputFieldName">The name of the input field.</param>
        public bool IsAllowedInputField(string inputFieldName)
        {
            if (AllowedInputFieldsCfg != null)
            {
                return AllowedInputFieldsCfg.IsAllowed(inputFieldName);
            }
            return false;
        }

        /// <inheritdoc/>
        protected override void Check()
        {
            if (ReadoutUnitName.Length == 0)
            {
                throw new ArgumentException($"Readout unit name can not be empty.", "ReadoutUnitName");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new ReadoutUnitMapSettings(this);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("map", suppressDefaults);
        }


    }//ReadoutUnitMapSettings

}//Namespace
