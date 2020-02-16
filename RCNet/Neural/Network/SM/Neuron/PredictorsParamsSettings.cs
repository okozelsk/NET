using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Neuron
{
    /// <summary>
    /// Predictors' parameters
    /// </summary>
    [Serializable]
    public class PredictorsParamsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PredictorsParamsCfgType";

        //Attribute properties
        /// <summary>
        /// Configuration of ActivationFadingSum predictor
        /// </summary>
        public PredictorActivationFadingSumSettings ActivationFadingSumCfg { get; }
        /// <summary>
        /// Configuration of ActivationMWAvg predictor
        /// </summary>
        public PredictorActivationMWAvgSettings ActivationMWAvgCfg { get; }
        /// <summary>
        /// Configuration of FiringFadingSum predictor
        /// </summary>
        public PredictorFiringFadingSumSettings FiringFadingSumCfg { get; }
        /// <summary>
        /// Configuration of FiringMWAvg predictor
        /// </summary>
        public PredictorFiringMWAvgSettings FiringMWAvgCfg { get; }
        /// <summary>
        /// Configuration of FiringCount predictor
        /// </summary>
        public PredictorFiringCountSettings FiringCountCfg { get; }
        /// <summary>
        /// Configuration of FiringBinPattern predictor
        /// </summary>
        public PredictorFiringBinPatternSettings FiringBinPatternCfg { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public PredictorsParamsSettings()
        {
            ActivationFadingSumCfg = new PredictorActivationFadingSumSettings();
            ActivationMWAvgCfg = new PredictorActivationMWAvgSettings();
            FiringFadingSumCfg = new PredictorFiringFadingSumSettings();
            FiringMWAvgCfg = new PredictorFiringMWAvgSettings();
            FiringCountCfg = new PredictorFiringCountSettings();
            FiringBinPatternCfg = new PredictorFiringBinPatternSettings();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="predictorParamsSettings">Predictor specific parameters settings</param>
        public PredictorsParamsSettings(params IPredictorParamsSettings[] predictorParamsSettings)
            :this()
        {

            foreach (IPredictorParamsSettings settings in predictorParamsSettings)
            {
                switch (settings.ID)
                {
                    case Predictors.PredictorID.ActivationFadingSum:
                        ActivationFadingSumCfg = (PredictorActivationFadingSumSettings)settings;
                        break;
                    case Predictors.PredictorID.ActivationMWAvg:
                        ActivationMWAvgCfg = (PredictorActivationMWAvgSettings)settings;
                        break;
                    case Predictors.PredictorID.FiringFadingSum:
                        FiringFadingSumCfg = (PredictorFiringFadingSumSettings)settings;
                        break;
                    case Predictors.PredictorID.FiringMWAvg:
                        FiringMWAvgCfg = (PredictorFiringMWAvgSettings)settings;
                        break;
                    case Predictors.PredictorID.FiringCount:
                        FiringCountCfg = (PredictorFiringCountSettings)settings;
                        break;
                    case Predictors.PredictorID.FiringBinPattern:
                        FiringBinPatternCfg = (PredictorFiringBinPatternSettings)settings;
                        break;
                    default:
                        break;
                }
            }
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public PredictorsParamsSettings(PredictorsParamsSettings source)
        {
            ActivationFadingSumCfg = (PredictorActivationFadingSumSettings)source.ActivationFadingSumCfg.DeepClone();
            ActivationMWAvgCfg = (PredictorActivationMWAvgSettings)source.ActivationMWAvgCfg.DeepClone();
            FiringFadingSumCfg = (PredictorFiringFadingSumSettings)source.FiringFadingSumCfg.DeepClone();
            FiringMWAvgCfg = (PredictorFiringMWAvgSettings)source.FiringMWAvgCfg.DeepClone();
            FiringCountCfg = (PredictorFiringCountSettings)source.FiringCountCfg.DeepClone();
            FiringBinPatternCfg = (PredictorFiringBinPatternSettings)source.FiringBinPatternCfg.DeepClone();
            return;
        }

        /// <summary>
        /// Creates initialized instance using xml element
        /// </summary>
        /// <param name="elem">Xml element containing settings</param>
        public PredictorsParamsSettings(XElement elem)
            :this()
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            foreach(Predictors.PredictorID predictorID in typeof(Predictors.PredictorID).GetEnumValues())
            {
                XElement predictorElem = settingsElem.Descendants(PredictorsSettings.GetXmlName(predictorID)).FirstOrDefault();
                if(predictorElem != null)
                {
                    switch(predictorID)
                    {
                        case Predictors.PredictorID.ActivationFadingSum:
                            ActivationFadingSumCfg = new PredictorActivationFadingSumSettings(predictorElem);
                            break;
                        case Predictors.PredictorID.ActivationMWAvg:
                            ActivationMWAvgCfg = new PredictorActivationMWAvgSettings(predictorElem);
                            break;
                        case Predictors.PredictorID.FiringFadingSum:
                            FiringFadingSumCfg = new PredictorFiringFadingSumSettings(predictorElem);
                            break;
                        case Predictors.PredictorID.FiringMWAvg:
                            FiringMWAvgCfg = new PredictorFiringMWAvgSettings(predictorElem);
                            break;
                        case Predictors.PredictorID.FiringCount:
                            FiringCountCfg = new PredictorFiringCountSettings(predictorElem);
                            break;
                        case Predictors.PredictorID.FiringBinPattern:
                            FiringBinPatternCfg = new PredictorFiringBinPatternSettings(predictorElem);
                            break;
                        default:
                            break;
                    }
                }
            }
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
                return ActivationFadingSumCfg.ContainsOnlyDefaults &&
                       ActivationMWAvgCfg.ContainsOnlyDefaults &&
                       FiringFadingSumCfg.ContainsOnlyDefaults &&
                       FiringMWAvgCfg.ContainsOnlyDefaults &&
                       FiringCountCfg.ContainsOnlyDefaults &&
                       FiringBinPatternCfg.ContainsOnlyDefaults;
            }
        }

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new PredictorsParamsSettings(this);
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
            if (!suppressDefaults || !ActivationFadingSumCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(ActivationFadingSumCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !ActivationMWAvgCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(ActivationMWAvgCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !FiringFadingSumCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(FiringFadingSumCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !FiringMWAvgCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(FiringMWAvgCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !FiringCountCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(FiringCountCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !FiringBinPatternCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(FiringBinPatternCfg.GetXml(suppressDefaults));
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
            return GetXml("params", suppressDefaults);
        }

    }//PredictorsParamsSettings

}//Namespace
