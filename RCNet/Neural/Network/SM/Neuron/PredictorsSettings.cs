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
using RCNet.Neural.Network.SM.Synapse;

namespace RCNet.Neural.Network.SM.Neuron
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
        public const string XsdTypeName = "PredictorsCfgType";

        //Attribute properties
        //Configuration
        /// <summary>
        ///Enabling switches 
        /// </summary>
        public bool[] EnablingSwitches { get; }
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
            EnablingSwitches = new bool[Predictors.NumOfPredictors];
            //Params
            ParamsCfg = groupPredictorsSettings?.ParamsCfg != null ? (PredictorsParamsSettings)groupPredictorsSettings.ParamsCfg.DeepClone() : (poolPredictorsSettings?.ParamsCfg != null ? (PredictorsParamsSettings)poolPredictorsSettings.ParamsCfg.DeepClone() : (reservoirPredictorsSettings?.ParamsCfg != null ? (PredictorsParamsSettings)reservoirPredictorsSettings.ParamsCfg.DeepClone() : new PredictorsParamsSettings()));
            //Enabling switches
            foreach(Predictors.PredictorID predictorID in typeof(Predictors.PredictorID).GetEnumValues())
            {
                EnablingSwitches[(int)predictorID] = ((groupPredictorsSettings == null ? true : groupPredictorsSettings.EnablingSwitches[(int)predictorID]) &&
                              (poolPredictorsSettings == null ? true : poolPredictorsSettings.EnablingSwitches[(int)predictorID]) &&
                              (reservoirPredictorsSettings == null ? true : reservoirPredictorsSettings.EnablingSwitches[(int)predictorID])
                              );
            }
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="enablingSwitches">Enabling switches</param>
        /// <param name="paramsCfg">Parameters of the predictors</param>
        public PredictorsSettings(bool[] enablingSwitches = null, PredictorsParamsSettings paramsCfg = null)
        {
            EnablingSwitches = new bool[Predictors.NumOfPredictors];
            if(enablingSwitches != null)
            {
                enablingSwitches.CopyTo(EnablingSwitches, 0);
            }
            else
            {
                EnablingSwitches.Populate(true);
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
            EnablingSwitches = new bool[Predictors.NumOfPredictors];
            EnablingSwitches[(int)Predictors.PredictorID.Activation] = activation;
            EnablingSwitches[(int)Predictors.PredictorID.ActivationSquare] = activationSquare;
            EnablingSwitches[(int)Predictors.PredictorID.ActivationFadingSum] = activationFadingSum;
            EnablingSwitches[(int)Predictors.PredictorID.ActivationMWAvg] = activationMWAvg;
            EnablingSwitches[(int)Predictors.PredictorID.FiringFadingSum] = firingFadingSum;
            EnablingSwitches[(int)Predictors.PredictorID.FiringMWAvg] = firingMWAvg;
            EnablingSwitches[(int)Predictors.PredictorID.FiringCount] = firingCount;
            EnablingSwitches[(int)Predictors.PredictorID.FiringBinPattern] = firingBinPattern;
            ParamsCfg = (PredictorsParamsSettings)paramsCfg?.DeepClone();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public PredictorsSettings(PredictorsSettings source)
        {
            EnablingSwitches = (bool[])source.EnablingSwitches.Clone();
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
            EnablingSwitches = new bool[Predictors.NumOfPredictors];
            foreach (Predictors.PredictorID predictorID in typeof(Predictors.PredictorID).GetEnumValues())
            {
                EnablingSwitches[(int)predictorID] = bool.Parse(predictorsElem.Attribute(GetXmlName(predictorID)).Value);
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
                foreach (Predictors.PredictorID predictorID in typeof(Predictors.PredictorID).GetEnumValues())
                {
                    count += EnablingSwitches[(int)predictorID] ? 1 : 0;
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
                return (NumOfEnabledPredictors == Predictors.NumOfPredictors && (ParamsCfg == null || ParamsCfg.ContainsOnlyDefaults));
            }
        }

        //Static methods
        /// <summary>
        /// Constructs name of predictor used in xml setup
        /// </summary>
        /// <param name="predictorID">Enumerated predictor identifier</param>
        public static string GetXmlName(Predictors.PredictorID predictorID)
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
        public bool IsEnabled(Predictors.PredictorID predictorID)
        {
            return EnablingSwitches[(int)predictorID];
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
            foreach(Predictors.PredictorID predictorID in typeof(Predictors.PredictorID).GetEnumValues())
            {
                if (!suppressDefaults || !EnablingSwitches[(int)predictorID])
                {
                    rootElem.Add(new XAttribute(GetXmlName(predictorID), EnablingSwitches[(int)predictorID].ToString().ToLowerInvariant()));
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

