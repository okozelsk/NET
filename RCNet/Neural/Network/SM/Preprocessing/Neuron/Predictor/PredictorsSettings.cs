using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.IO;
using RCNet.Extensions;
using RCNet.XmlTools;
using RCNet.RandomValue;
using RCNet.Neural.Activation;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Configuration of the availble predictors
    /// </summary>
    [Serializable]
    public class PredictorsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PredictorsType";

        //Attribute properties
        //Configuration
        /// <summary>
        ///Enabling switches 
        /// </summary>
        public bool[] EnablingSwitchCollection { get; }
        /// <summary>
        /// Parameters of the predictors
        /// </summary>
        public PredictorsParamsSettings ParamsCfg { get; private set; }

        //Constructors
        /// <summary>
        /// Creates initialized instance as a result of L1, L2 and L3 predictors settings.
        /// (L1 overrides L2 overiddes L3)
        /// </summary>
        /// <param name="level1PredictorsCfg">Level1 (lowest level) predictors settings</param>
        /// <param name="level2PredictorsCfg">Level2 predictors settings</param>
        /// <param name="level3PredictorsCfg">Level3 (highest level) predictors settings</param>
        public PredictorsSettings(PredictorsSettings level1PredictorsCfg,
                                  PredictorsSettings level2PredictorsCfg,
                                  PredictorsSettings level3PredictorsCfg
                                  )
        {
            EnablingSwitchCollection = new bool[PredictorsProvider.NumOfSupportedPredictors];
            //Params
            ParamsCfg = level1PredictorsCfg?.ParamsCfg != null ? (PredictorsParamsSettings)level1PredictorsCfg.ParamsCfg.DeepClone() : (level2PredictorsCfg?.ParamsCfg != null ? (PredictorsParamsSettings)level2PredictorsCfg.ParamsCfg.DeepClone() : (level3PredictorsCfg?.ParamsCfg != null ? (PredictorsParamsSettings)level3PredictorsCfg.ParamsCfg.DeepClone() : new PredictorsParamsSettings()));
            //Enabling switches
            foreach(PredictorsProvider.PredictorID predictorID in typeof(PredictorsProvider.PredictorID).GetEnumValues())
            {
                EnablingSwitchCollection[(int)predictorID] = ((level1PredictorsCfg == null ? true : level1PredictorsCfg.EnablingSwitchCollection[(int)predictorID]) &&
                              (level2PredictorsCfg == null ? true : level2PredictorsCfg.EnablingSwitchCollection[(int)predictorID]) &&
                              (level3PredictorsCfg == null ? true : level3PredictorsCfg.EnablingSwitchCollection[(int)predictorID])
                              );
            }
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="enablingSwitchCollection">Enabling switches</param>
        /// <param name="paramsCfg">Parameters of the predictors</param>
        public PredictorsSettings(bool[] enablingSwitchCollection = null, PredictorsParamsSettings paramsCfg = null)
        {
            EnablingSwitchCollection = new bool[PredictorsProvider.NumOfSupportedPredictors];
            if(enablingSwitchCollection != null)
            {
                enablingSwitchCollection.CopyTo(EnablingSwitchCollection, 0);
            }
            else
            {
                EnablingSwitchCollection.Populate(true);
            }
            ParamsCfg = (PredictorsParamsSettings)paramsCfg?.DeepClone();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="activation">Activation predictor enabling switch</param>
        /// <param name="activationSquare">ActivationSquare predictor enabling switch</param>
        /// <param name="activationFadingSum">ActivationFadingSum predictor enabling switch</param>
        /// <param name="activationMWAvg">ActivationMWAvg predictor enabling switch</param>
        /// <param name="firingFadingSum">FiringFadingSum predictor enabling switch</param>
        /// <param name="firingMWAvg">FiringMWAvg predictor enabling switch</param>
        /// <param name="firingCount">FiringCount predictor enabling switch</param>
        /// <param name="firingBinPattern">FiringBinPattern predictor enabling switch</param>
        /// <param name="paramsCfg">Parameters of the predictors</param>
        public PredictorsSettings(bool activation,
                                  bool activationSquare,
                                  bool activationFadingSum,
                                  bool activationMWAvg,
                                  bool firingFadingSum,
                                  bool firingMWAvg,
                                  bool firingCount,
                                  bool firingBinPattern,
                                  PredictorsParamsSettings paramsCfg = null
                                  )
        {
            EnablingSwitchCollection = new bool[PredictorsProvider.NumOfSupportedPredictors];
            EnablingSwitchCollection[(int)PredictorsProvider.PredictorID.Activation] = activation;
            EnablingSwitchCollection[(int)PredictorsProvider.PredictorID.ActivationSquare] = activationSquare;
            EnablingSwitchCollection[(int)PredictorsProvider.PredictorID.ActivationFadingSum] = activationFadingSum;
            EnablingSwitchCollection[(int)PredictorsProvider.PredictorID.ActivationMWAvg] = activationMWAvg;
            EnablingSwitchCollection[(int)PredictorsProvider.PredictorID.FiringFadingSum] = firingFadingSum;
            EnablingSwitchCollection[(int)PredictorsProvider.PredictorID.FiringMWAvg] = firingMWAvg;
            EnablingSwitchCollection[(int)PredictorsProvider.PredictorID.FiringCount] = firingCount;
            EnablingSwitchCollection[(int)PredictorsProvider.PredictorID.FiringBinPattern] = firingBinPattern;
            ParamsCfg = (PredictorsParamsSettings)paramsCfg?.DeepClone();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public PredictorsSettings(PredictorsSettings source)
        {
            EnablingSwitchCollection = (bool[])source.EnablingSwitchCollection.Clone();
            //Params
            ParamsCfg = (PredictorsParamsSettings)source.ParamsCfg?.DeepClone();
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml element containing the settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public PredictorsSettings(XElement elem)
        {
            //Validation
            XElement predictorsElem = Validate(elem, XsdTypeName);
            //Parsing of enabling switches
            EnablingSwitchCollection = new bool[PredictorsProvider.NumOfSupportedPredictors];
            foreach (PredictorsProvider.PredictorID predictorID in typeof(PredictorsProvider.PredictorID).GetEnumValues())
            {
                EnablingSwitchCollection[(int)predictorID] = bool.Parse(predictorsElem.Attribute(GetXmlName(predictorID)).Value);
            }
            //Parsing of params
            ParamsCfg = null;
            XElement paramsElem = predictorsElem.Elements("params").FirstOrDefault();
            if (paramsElem != null)
            {
                ParamsCfg = new PredictorsParamsSettings(paramsElem);
            }
            return;
        }

        //Properties
        /// <summary>
        /// Total number of enabled predictors
        /// </summary>
        public int NumOfEnabledPredictors
        {
            get
            {
                int count = 0;
                foreach (PredictorsProvider.PredictorID predictorID in typeof(PredictorsProvider.PredictorID).GetEnumValues())
                {
                    count += EnablingSwitchCollection[(int)predictorID] ? 1 : 0;
                }
                return count;
            }
        }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return (NumOfEnabledPredictors == PredictorsProvider.NumOfSupportedPredictors && (ParamsCfg == null || ParamsCfg.ContainsOnlyDefaults));
            }
        }

        //Static methods
        /// <summary>
        /// Constructs name of predictor used in xml setup
        /// </summary>
        /// <param name="predictorID">Enumerated predictor identifier</param>
        public static string GetXmlName(PredictorsProvider.PredictorID predictorID)
        {
            string name = predictorID.ToString();
            return name.Substring(0, 1).ToLowerInvariant() + name.Substring(1);
        }

        /// <summary>
        /// Creates instance having all predictors disabled
        /// </summary>
        public static PredictorsSettings CreateDisabledInstance()
        {
            return new PredictorsSettings(false, false, false, false, false, false, false, false);
        }

        //Methods
        /// <summary>
        /// Disables activation based predictors
        /// </summary>
        public void DisableActivationPredictors()
        {
            EnablingSwitchCollection[(int)PredictorsProvider.PredictorID.Activation] = false;
            EnablingSwitchCollection[(int)PredictorsProvider.PredictorID.ActivationSquare] = false;
            EnablingSwitchCollection[(int)PredictorsProvider.PredictorID.ActivationFadingSum] = false;
            EnablingSwitchCollection[(int)PredictorsProvider.PredictorID.ActivationMWAvg] = false;
            return;
        }

        /// <summary>
        /// Disables firing based predictors
        /// </summary>
        public void DisableFiringPredictors()
        {
            EnablingSwitchCollection[(int)PredictorsProvider.PredictorID.FiringMWAvg] = false;
            EnablingSwitchCollection[(int)PredictorsProvider.PredictorID.FiringFadingSum] = false;
            EnablingSwitchCollection[(int)PredictorsProvider.PredictorID.FiringCount] = false;
            EnablingSwitchCollection[(int)PredictorsProvider.PredictorID.FiringBinPattern] = false;
            return;
        }

        /// <summary>
        /// Disables all predictors
        /// </summary>
        public void DisableAllPredictors()
        {
            DisableActivationPredictors();
            DisableFiringPredictors();
            return;
        }

        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new PredictorsSettings(this);
        }

        /// <summary>
        /// Checks if given predictor is enabled
        /// </summary>
        /// <param name="predictorID">Enumerated predictor identifier</param>
        public bool IsEnabled(PredictorsProvider.PredictorID predictorID)
        {
            return EnablingSwitchCollection[(int)predictorID];
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
            foreach(PredictorsProvider.PredictorID predictorID in typeof(PredictorsProvider.PredictorID).GetEnumValues())
            {
                if (!suppressDefaults || !EnablingSwitchCollection[(int)predictorID])
                {
                    rootElem.Add(new XAttribute(GetXmlName(predictorID), EnablingSwitchCollection[(int)predictorID].ToString().ToLowerInvariant()));
                }
            }
            if(ParamsCfg != null)
            {
                if (!suppressDefaults || !ParamsCfg.ContainsOnlyDefaults)
                {
                    rootElem.Add(ParamsCfg.GetXml(suppressDefaults));
                }
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
            return GetXml("predictors", suppressDefaults);
        }

    }//PredictorsSettings

}//Namespace

