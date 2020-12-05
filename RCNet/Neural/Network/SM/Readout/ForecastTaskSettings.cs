using RCNet.Neural.Data.Filter;
using RCNet.Neural.Network.NonRecurrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Configuration of the forecast task
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
        /// <inheritdoc />
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
        /// <inheritdoc />
        public ReadoutUnit.TaskType Type { get { return ReadoutUnit.TaskType.Forecast; } }

        /// <inheritdoc />
        public List<INonRecurrentNetworkSettings> NetworkCfgCollection { get { return NetworksCfg.NetworkCfgCollection; } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return false;
            }
        }


        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new ForecastTaskSettings(this);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("forecast", suppressDefaults);
        }

    }//ForecastTaskSettings

}//Namespace
