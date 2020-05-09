using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Xml.Linq;
using System.Reflection;
using System.IO;
using RCNet.Extensions;
using RCNet.XmlTools;
using RCNet.MathTools;
using RCNet.Neural.Data.Filter;
using RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Configuration of an external input field
    /// </summary>
    [Serializable]
    public class ExternalFieldSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPExternalInpFieldType";
        //Default values
        /// <summary>
        /// Default value of parameter specifying if to route input field to readout layer together with other predictors 
        /// </summary>
        public const bool DefaultRouteToReadout = true;

        //Attribute properties
        /// <summary>
        /// Input field name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Configuration of feature filter associated with the input field
        /// </summary>
        public IFeatureFilterSettings FeatureFilterCfg { get; }

        /// <summary>
        /// Specifies if to route input field to readout layer together with other predictors
        /// </summary>
        public bool RouteToReadout { get; }

        /// <summary>
        /// Configuration of spiking coding neurons
        /// </summary>
        public SpikingCodingSettings SpikingCodingCfg { get; }

        /// <summary>
        /// Predictors settings
        /// </summary>
        public PredictorsSettings PredictorsCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        /// <param name="name">Input field name</param>
        /// <param name="featureFilterCfg">Configuration of feature filter associated with the input field</param>
        /// <param name="routeToReadout">Specifies if to route input field to readout layer together with other predictors</param>
        /// <param name="spikingCodingCfg">Configuration of spiking coding neurons</param>
        /// <param name="predictorsCfg">Predictors settings</param>
        public ExternalFieldSettings(string name,
                                     IFeatureFilterSettings featureFilterCfg,
                                     bool routeToReadout = DefaultRouteToReadout,
                                     SpikingCodingSettings spikingCodingCfg = null,
                                     PredictorsSettings predictorsCfg = null
                                     )
        {
            Name = name;
            FeatureFilterCfg = (IFeatureFilterSettings)featureFilterCfg.DeepClone();
            RouteToReadout = routeToReadout;
            SpikingCodingCfg = (SpikingCodingSettings)spikingCodingCfg?.DeepClone();
            if (featureFilterCfg.Type == BaseFeatureFilter.FeatureType.Real && spikingCodingCfg == null)
            {
                SpikingCodingCfg = new SpikingCodingSettings();
            }
            PredictorsCfg = predictorsCfg == null ? null : (PredictorsSettings)predictorsCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ExternalFieldSettings(ExternalFieldSettings source)
            :this(source.Name, source.FeatureFilterCfg, source.RouteToReadout, source.SpikingCodingCfg, source.PredictorsCfg)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing the settings.</param>
        public ExternalFieldSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Name = settingsElem.Attribute("name").Value;
            RouteToReadout = bool.Parse(settingsElem.Attribute("routeToReadout").Value);
            FeatureFilterCfg = FeatureFilterFactory.LoadSettings(settingsElem.Elements().First());
            XElement spikingCodingElem = settingsElem.Elements("spikingCoding").FirstOrDefault();
            SpikingCodingCfg = spikingCodingElem == null ? null : new SpikingCodingSettings(spikingCodingElem);
            XElement predictorsElem = settingsElem.Elements("predictors").FirstOrDefault();
            PredictorsCfg = predictorsElem == null ? null : new PredictorsSettings(predictorsElem);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultRouteToReadout { get { return (RouteToReadout == DefaultRouteToReadout); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (Name.Length == 0)
            {
                throw new Exception($"Name can not be empty.");
            }
            if (FeatureFilterCfg.Type != BaseFeatureFilter.FeatureType.Real && SpikingCodingCfg != null)
            {
                throw new Exception("Spiking coding configuration is relevant for real-feature only.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance.
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new ExternalFieldSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("name", Name),
                                             FeatureFilterCfg.GetXml(suppressDefaults)
                                             );
            if (!suppressDefaults || !IsDefaultRouteToReadout)
            {
                rootElem.Add(new XAttribute("routeToReadout", RouteToReadout.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
            }
            if (SpikingCodingCfg != null && (!SpikingCodingCfg.ContainsOnlyDefaults || !suppressDefaults))
            {
                rootElem.Add(SpikingCodingCfg.GetXml(suppressDefaults));
            }
            if (PredictorsCfg != null && (!PredictorsCfg.ContainsOnlyDefaults || !suppressDefaults))
            {
                rootElem.Add(PredictorsCfg.GetXml(suppressDefaults));
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
            return GetXml("field", suppressDefaults);
        }

    }//ExternalFieldSettings

}//Namespace
