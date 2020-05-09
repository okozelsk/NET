using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
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
        public const string XsdTypeName = "PredictorsParamsType";

        //Attribute properties
        /// <summary>
        /// Configuration of ActivationFadingSum predictor
        /// </summary>
        public ActivationFadingSumSettings ActivationFadingSumCfg { get; }
        /// <summary>
        /// Configuration of ActivationMWAvg predictor
        /// </summary>
        public ActivationMWAvgSettings ActivationMWAvgCfg { get; }
        /// <summary>
        /// Configuration of FiringFadingSum predictor
        /// </summary>
        public FiringFadingSumSettings FiringFadingSumCfg { get; }
        /// <summary>
        /// Configuration of FiringMWAvg predictor
        /// </summary>
        public FiringMWAvgSettings FiringMWAvgCfg { get; }
        /// <summary>
        /// Configuration of FiringCount predictor
        /// </summary>
        public FiringCountSettings FiringCountCfg { get; }
        /// <summary>
        /// Configuration of FiringBinPattern predictor
        /// </summary>
        public FiringBinPatternSettings FiringBinPatternCfg { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public PredictorsParamsSettings()
        {
            ActivationFadingSumCfg = new ActivationFadingSumSettings();
            ActivationMWAvgCfg = new ActivationMWAvgSettings();
            FiringFadingSumCfg = new FiringFadingSumSettings();
            FiringMWAvgCfg = new FiringMWAvgSettings();
            FiringCountCfg = new FiringCountSettings();
            FiringBinPatternCfg = new FiringBinPatternSettings();
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
                    case PredictorsProvider.PredictorID.ActivationFadingSum:
                        ActivationFadingSumCfg = (ActivationFadingSumSettings)settings;
                        break;
                    case PredictorsProvider.PredictorID.ActivationMWAvg:
                        ActivationMWAvgCfg = (ActivationMWAvgSettings)settings;
                        break;
                    case PredictorsProvider.PredictorID.FiringFadingSum:
                        FiringFadingSumCfg = (FiringFadingSumSettings)settings;
                        break;
                    case PredictorsProvider.PredictorID.FiringMWAvg:
                        FiringMWAvgCfg = (FiringMWAvgSettings)settings;
                        break;
                    case PredictorsProvider.PredictorID.FiringCount:
                        FiringCountCfg = (FiringCountSettings)settings;
                        break;
                    case PredictorsProvider.PredictorID.FiringBinPattern:
                        FiringBinPatternCfg = (FiringBinPatternSettings)settings;
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
            ActivationFadingSumCfg = (ActivationFadingSumSettings)source.ActivationFadingSumCfg.DeepClone();
            ActivationMWAvgCfg = (ActivationMWAvgSettings)source.ActivationMWAvgCfg.DeepClone();
            FiringFadingSumCfg = (FiringFadingSumSettings)source.FiringFadingSumCfg.DeepClone();
            FiringMWAvgCfg = (FiringMWAvgSettings)source.FiringMWAvgCfg.DeepClone();
            FiringCountCfg = (FiringCountSettings)source.FiringCountCfg.DeepClone();
            FiringBinPatternCfg = (FiringBinPatternSettings)source.FiringBinPatternCfg.DeepClone();
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
            foreach(PredictorsProvider.PredictorID predictorID in typeof(PredictorsProvider.PredictorID).GetEnumValues())
            {
                XElement predictorElem = settingsElem.Elements(PredictorsSettings.GetXmlName(predictorID)).FirstOrDefault();
                if(predictorElem != null)
                {
                    switch(predictorID)
                    {
                        case PredictorsProvider.PredictorID.ActivationFadingSum:
                            ActivationFadingSumCfg = new ActivationFadingSumSettings(predictorElem);
                            break;
                        case PredictorsProvider.PredictorID.ActivationMWAvg:
                            ActivationMWAvgCfg = new ActivationMWAvgSettings(predictorElem);
                            break;
                        case PredictorsProvider.PredictorID.FiringFadingSum:
                            FiringFadingSumCfg = new FiringFadingSumSettings(predictorElem);
                            break;
                        case PredictorsProvider.PredictorID.FiringMWAvg:
                            FiringMWAvgCfg = new FiringMWAvgSettings(predictorElem);
                            break;
                        case PredictorsProvider.PredictorID.FiringCount:
                            FiringCountCfg = new FiringCountSettings(predictorElem);
                            break;
                        case PredictorsProvider.PredictorID.FiringBinPattern:
                            FiringBinPatternCfg = new FiringBinPatternSettings(predictorElem);
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
