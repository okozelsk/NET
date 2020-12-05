using RCNet.Neural.Network.NonRecurrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;


namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Configuration of the default task-dependent networks
    /// </summary>
    [Serializable]
    public class DefaultNetworksSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ROutLayerUnitDefaultNetworksType";

        //Attribute properties
        /// <summary>
        /// Classification networks settings
        /// </summary>
        public ClassificationNetworksSettings ClassificationNetworksCfg { get; }
        /// <summary>
        /// Forecast networks settings
        /// </summary>
        public ForecastNetworksSettings ForecastNetworksCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="classificationNetworksCfg">Default classification networks settings</param>
        /// <param name="forecastNetworksCfg">Default forecast networks settings</param>
        public DefaultNetworksSettings(ClassificationNetworksSettings classificationNetworksCfg = null,
                                       ForecastNetworksSettings forecastNetworksCfg = null
                                       )
        {
            ClassificationNetworksCfg = classificationNetworksCfg == null ? new ClassificationNetworksSettings() : (ClassificationNetworksSettings)classificationNetworksCfg.DeepClone();
            ForecastNetworksCfg = forecastNetworksCfg == null ? new ForecastNetworksSettings() : (ForecastNetworksSettings)forecastNetworksCfg.DeepClone();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public DefaultNetworksSettings(DefaultNetworksSettings source)
            : this(source.ClassificationNetworksCfg, source.ForecastNetworksCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public DefaultNetworksSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            XElement classificationNetworksSettingsElem = settingsElem.Elements("classification").FirstOrDefault();
            ClassificationNetworksCfg = classificationNetworksSettingsElem == null ? new ClassificationNetworksSettings() : new ClassificationNetworksSettings(classificationNetworksSettingsElem);
            XElement forecastNetworksSettingsElem = settingsElem.Elements("forecast").FirstOrDefault();
            ForecastNetworksCfg = forecastNetworksSettingsElem == null ? new ForecastNetworksSettings() : new ForecastNetworksSettings(forecastNetworksSettingsElem);
            return;
        }

        //Properties
        /// <inheritdoc />
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return ClassificationNetworksCfg.ContainsOnlyDefaults &&
                       ForecastNetworksCfg.ContainsOnlyDefaults;
            }
        }


        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            return;
        }

        /// <summary>
        /// Returns task dependent collection of networks settings
        /// </summary>
        /// <param name="task">Task type</param>
        public List<INonRecurrentNetworkSettings> GetTaskNetworksCfgs(ReadoutUnit.TaskType task)
        {
            return task == ReadoutUnit.TaskType.Classification ? ClassificationNetworksCfg.NetworkCfgCollection : ForecastNetworksCfg.NetworkCfgCollection;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new DefaultNetworksSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!ClassificationNetworksCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(ClassificationNetworksCfg.GetXml("classification", suppressDefaults));
            }
            if (!ForecastNetworksCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(ForecastNetworksCfg.GetXml("forecast", suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("defaultNetworks", suppressDefaults);
        }

    }//DefaultNetworksSettings

}//Namespace
