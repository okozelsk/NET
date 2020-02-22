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

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron.Predictor
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
        /// Creates initialized instance as a result of neuron group, pool and reservoir instance predictors settings
        /// </summary>
        /// <param name="groupPredictorsSettings">Neuron group predictors settings</param>
        /// <param name="poolPredictorsSettings">Pool predictors settings</param>
        /// <param name="reservoirPredictorsSettings">Reservoir predictors settings</param>
        public PredictorsSettings(PredictorsSettings groupPredictorsSettings,
                                  PredictorsSettings poolPredictorsSettings,
                                  PredictorsSettings reservoirPredictorsSettings
                                  )
        {
            EnablingSwitchCollection = new bool[PredictorsProvider.NumOfPredictors];
            //Params
            ParamsCfg = groupPredictorsSettings?.ParamsCfg != null ? (PredictorsParamsSettings)groupPredictorsSettings.ParamsCfg.DeepClone() : (poolPredictorsSettings?.ParamsCfg != null ? (PredictorsParamsSettings)poolPredictorsSettings.ParamsCfg.DeepClone() : (reservoirPredictorsSettings?.ParamsCfg != null ? (PredictorsParamsSettings)reservoirPredictorsSettings.ParamsCfg.DeepClone() : new PredictorsParamsSettings()));
            //Enabling switches
            foreach(PredictorsProvider.PredictorID predictorID in typeof(PredictorsProvider.PredictorID).GetEnumValues())
            {
                EnablingSwitchCollection[(int)predictorID] = ((groupPredictorsSettings == null ? true : groupPredictorsSettings.EnablingSwitchCollection[(int)predictorID]) &&
                              (poolPredictorsSettings == null ? true : poolPredictorsSettings.EnablingSwitchCollection[(int)predictorID]) &&
                              (reservoirPredictorsSettings == null ? true : reservoirPredictorsSettings.EnablingSwitchCollection[(int)predictorID])
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
            EnablingSwitchCollection = new bool[PredictorsProvider.NumOfPredictors];
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
            EnablingSwitchCollection = new bool[PredictorsProvider.NumOfPredictors];
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
            EnablingSwitchCollection = new bool[PredictorsProvider.NumOfPredictors];
            foreach (PredictorsProvider.PredictorID predictorID in typeof(PredictorsProvider.PredictorID).GetEnumValues())
            {
                EnablingSwitchCollection[(int)predictorID] = bool.Parse(predictorsElem.Attribute(GetXmlName(predictorID)).Value);
            }
            //Parsing of params
            ParamsCfg = null;
            XElement paramsElem = predictorsElem.Descendants("params").FirstOrDefault();
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
                return (NumOfEnabledPredictors == PredictorsProvider.NumOfPredictors && (ParamsCfg == null || ParamsCfg.ContainsOnlyDefaults));
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

        //Methods
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

