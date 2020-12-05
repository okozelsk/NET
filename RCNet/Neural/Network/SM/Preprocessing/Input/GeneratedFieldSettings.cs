using RCNet.Neural.Data.Filter;
using RCNet.Neural.Data.Generators;
using RCNet.RandomValue;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Configuration of the generated input field
    /// </summary>
    [Serializable]
    public class GeneratedFieldSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPGeneratedInpFieldType";
        //Default values
        /// <summary>
        /// Default value of parameter specifying if to route generated field to readout layer together with other predictors 
        /// </summary>
        public const bool DefaultRouteToReadout = false;

        //Attribute properties
        /// <summary>
        /// Generated field name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Configuration of associated generator
        /// </summary>
        public RCNetBaseSettings GeneratorCfg { get; }

        /// <summary>
        /// Specifies whether to route generated field to readout layer together with other predictors
        /// </summary>
        public bool RouteToReadout { get; }

        /// <summary>
        /// Configuration of real feature filter
        /// </summary>
        public RealFeatureFilterSettings FeatureFilterCfg { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="name">Generated field name</param>
        /// <param name="generatorCfg">Configuration of associated generator</param>
        /// <param name="routeToReadout">Specifies whether to route generated field to readout layer together with other predictors</param>
        /// <param name="featureFilterCfg">Configuration of real feature filter</param>
        public GeneratedFieldSettings(string name,
                                      RCNetBaseSettings generatorCfg,
                                      bool routeToReadout = DefaultRouteToReadout,
                                      RealFeatureFilterSettings featureFilterCfg = null
                                      )
        {
            Name = name;
            GeneratorCfg = generatorCfg.DeepClone();
            RouteToReadout = routeToReadout;
            FeatureFilterCfg = featureFilterCfg == null ? null : (RealFeatureFilterSettings)featureFilterCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public GeneratedFieldSettings(GeneratedFieldSettings source)
            : this(source.Name, source.GeneratorCfg, source.RouteToReadout, source.FeatureFilterCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml data containing the settings.</param>
        public GeneratedFieldSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Name = settingsElem.Attribute("name").Value;
            RouteToReadout = bool.Parse(settingsElem.Attribute("routeToReadout").Value);
            XElement genElem = settingsElem.Elements().First();
            GeneratorCfg = GeneratorFactory.LoadSettings(genElem);
            XElement realFeatureFilterElem = settingsElem.Elements("realFeatureFilter").FirstOrDefault();
            FeatureFilterCfg = realFeatureFilterElem == null ? new RealFeatureFilterSettings() : new RealFeatureFilterSettings(realFeatureFilterElem);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultRouteToReadout { get { return (RouteToReadout == DefaultRouteToReadout); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (Name.Length == 0)
            {
                throw new ArgumentException($"Name can not be empty.", "Name");
            }
            Type genType = GeneratorCfg.GetType();
            if (genType != typeof(PulseGeneratorSettings) &&
               genType != typeof(RandomValueSettings) &&
               genType != typeof(SinusoidalGeneratorSettings) &&
               genType != typeof(MackeyGlassGeneratorSettings))
            {
                throw new ArgumentException($"Unsupported generator configuration {genType}.", "GeneratorCfg");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new GeneratedFieldSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("name", Name),
                                             GeneratorCfg.GetXml(suppressDefaults)
                                             );
            if (!suppressDefaults || !IsDefaultRouteToReadout)
            {
                rootElem.Add(new XAttribute("routeToReadout", RouteToReadout.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
            }
            if (!suppressDefaults || !FeatureFilterCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(FeatureFilterCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("field", suppressDefaults);
        }

    }//GeneratedFieldSettings

}//Namespace
