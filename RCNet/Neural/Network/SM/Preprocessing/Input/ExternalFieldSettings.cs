using RCNet.Neural.Data.Filter;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

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
        /// Specifies whether to route input field to readout layer together with other predictors
        /// </summary>
        public bool RouteToReadout { get; }

        //Constructors
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        /// <param name="name">Input field name</param>
        /// <param name="featureFilterCfg">Configuration of feature filter associated with the input field</param>
        /// <param name="routeToReadout">Specifies whether to route input field to readout layer together with other predictors</param>
        public ExternalFieldSettings(string name,
                                     IFeatureFilterSettings featureFilterCfg,
                                     bool routeToReadout = DefaultRouteToReadout
                                     )
        {
            Name = name;
            FeatureFilterCfg = (IFeatureFilterSettings)featureFilterCfg.DeepClone();
            RouteToReadout = routeToReadout;
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ExternalFieldSettings(ExternalFieldSettings source)
            : this(source.Name, source.FeatureFilterCfg, source.RouteToReadout)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
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
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
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

    }//ExternalFieldSettings

}//Namespace
