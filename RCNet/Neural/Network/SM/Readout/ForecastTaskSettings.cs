using RCNet.Neural.Data.Filter;
using RCNet.Neural.Network.NonRecurrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Readout unit - forecast task settings
    /// </summary>
    [Serializable]
    public class ForecastTaskSettings : RCNetBaseSettings, ITaskSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ROutLayerUnitForecastTaskType";

        //Attribute properties
        /// <summary>
        /// Output feature filter settings
        /// </summary>
        public IFeatureFilterSettings FeatureFilterCfg { get; }
        /// <summary>
        /// Forecast networks settings
        /// </summary>
        public ForecastNetworksSettings NetworksCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="featureFilterCfg">Output feature filter settings</param>
        /// <param name="networksCfg">Forecasting networks settings</param>
        public ForecastTaskSettings(IFeatureFilterSettings featureFilterCfg,
                                    ForecastNetworksSettings networksCfg = null
                                    )
        {
            FeatureFilterCfg = (IFeatureFilterSettings)featureFilterCfg.DeepClone();
            NetworksCfg = networksCfg == null ? new ForecastNetworksSettings() : (ForecastNetworksSettings)networksCfg.DeepClone();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ForecastTaskSettings(ForecastTaskSettings source)
            : this(source.FeatureFilterCfg, source.NetworksCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public ForecastTaskSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            //Filter
            XElement filterSettingsElem = settingsElem.Elements().First();
            switch (filterSettingsElem.Name.LocalName)
            {
                case "binFeature":
                    FeatureFilterCfg = new BinFeatureFilterSettings(filterSettingsElem);
                    break;
                case "enumFeature":
                    FeatureFilterCfg = new EnumFeatureFilterSettings(filterSettingsElem);
                    break;
                case "realFeature":
                    FeatureFilterCfg = new RealFeatureFilterSettings(filterSettingsElem);
                    break;
                default:
                    throw new InvalidOperationException($"Feature filter element not found.");
            }
            //Networks
            XElement forecastNetworksSettingsElem = settingsElem.Elements("networks").FirstOrDefault();
            NetworksCfg = forecastNetworksSettingsElem == null ? new ForecastNetworksSettings() : new ForecastNetworksSettings(forecastNetworksSettingsElem);
            return;
        }

        //Properties
        /// <summary>
        /// Identifies forecast task
        /// </summary>
        public ReadoutUnit.TaskType Type { get { return ReadoutUnit.TaskType.Forecast; } }

        /// <summary>
        /// Associated networks settings
        /// </summary>
        public List<INonRecurrentNetworkSettings> NetworkCfgCollection { get { return NetworksCfg.NetworkCfgCollection; } }

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
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new ForecastTaskSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            rootElem.Add(FeatureFilterCfg.GetXml(suppressDefaults));
            if (!NetworksCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(NetworksCfg.GetXml("networks", suppressDefaults));
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
            return GetXml("forecast", suppressDefaults);
        }

    }//ForecastTaskSettings

}//Namespace
