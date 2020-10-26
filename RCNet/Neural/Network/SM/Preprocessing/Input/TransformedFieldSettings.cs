using RCNet.Neural.Data.Filter;
using RCNet.Neural.Data.Transformers;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Configuration of a transformed input field
    /// </summary>
    [Serializable]
    public class TransformedFieldSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPTransformedInpFieldType";
        //Default values
        /// <summary>
        /// Default value of parameter specifying if to route transformed field to readout layer together with other predictors 
        /// </summary>
        public const bool DefaultRouteToReadout = true;

        //Attribute properties
        /// <summary>
        /// Transformed field name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Configuration of the associated transformer
        /// </summary>
        public RCNetBaseSettings TransformerCfg { get; }

        /// <summary>
        /// Specifies whether to route transformed field to readout layer together with other predictors
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
        /// <param name="name">Transformed field name</param>
        /// <param name="transformerCfg">Configuration of associated transformer</param>
        /// <param name="routeToReadout">Specifies whether to route transformed field to readout layer together with other predictors</param>
        /// <param name="featureFilterCfg">Configuration of real feature filter</param>
        public TransformedFieldSettings(string name,
                                        RCNetBaseSettings transformerCfg,
                                        bool routeToReadout = DefaultRouteToReadout,
                                        RealFeatureFilterSettings featureFilterCfg = null
                                        )
        {
            Name = name;
            TransformerCfg = transformerCfg.DeepClone();
            RouteToReadout = routeToReadout;
            FeatureFilterCfg = featureFilterCfg == null ? null : (RealFeatureFilterSettings)featureFilterCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public TransformedFieldSettings(TransformedFieldSettings source)
            : this(source.Name, source.TransformerCfg, source.RouteToReadout, source.FeatureFilterCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml data containing the settings.</param>
        public TransformedFieldSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Name = settingsElem.Attribute("name").Value;
            RouteToReadout = bool.Parse(settingsElem.Attribute("routeToReadout").Value);
            XElement transElem = settingsElem.Elements().First();
            TransformerCfg = TransformerFactory.LoadSettings(transElem);
            XElement realFeatureFilterElem = settingsElem.Elements("realFeatureFilter").FirstOrDefault();
            FeatureFilterCfg = realFeatureFilterElem == null ? new RealFeatureFilterSettings() : new RealFeatureFilterSettings(realFeatureFilterElem);
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
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            if (Name.Length == 0)
            {
                throw new ArgumentException($"Name can not be empty.", "Name");
            }
            Type transType = TransformerCfg.GetType();
            if (transType != typeof(DiffTransformerSettings) &&
               transType != typeof(CDivTransformerSettings) &&
               transType != typeof(LogTransformerSettings) &&
               transType != typeof(ExpTransformerSettings) &&
               transType != typeof(PowerTransformerSettings) &&
               transType != typeof(YeoJohnsonTransformerSettings) &&
               transType != typeof(MWStatTransformerSettings) &&
               transType != typeof(MulTransformerSettings) &&
               transType != typeof(DivTransformerSettings) &&
               transType != typeof(LinearTransformerSettings))
            {
                throw new ArgumentException($"Unsupported transformer configuration {transType.Name}.", "TransformerCfg");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance.
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new TransformedFieldSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("name", Name),
                                             TransformerCfg.GetXml(suppressDefaults)
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

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("field", suppressDefaults);
        }

    }//TransformedFieldSettings

}//Namespace
