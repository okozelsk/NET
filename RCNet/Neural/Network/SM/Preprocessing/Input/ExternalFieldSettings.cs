﻿using RCNet.Neural.Data.Filter;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Configuration of the external input field.
    /// </summary>
    [Serializable]
    public class ExternalFieldSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "NPExternalInpFieldType";
        //Default values
        /// <summary>
        /// The default value of the parameter specifying whether to route the input field to the readout layer.
        /// </summary>
        public const bool DefaultRouteToReadout = true;

        //Attribute properties
        /// <summary>
        /// The name of the input field.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The configuration of the feature filter associated with the input field.
        /// </summary>
        public IFeatureFilterSettings FeatureFilterCfg { get; }

        /// <summary>
        /// Specifies whether to route the input field to the readout layer.
        /// </summary>
        public bool RouteToReadout { get; }

        //Constructors
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        /// <param name="name">The name of the input field.</param>
        /// <param name="featureFilterCfg">The configuration of the feature filter associated with the input field.</param>
        /// <param name="routeToReadout">Specifies whether to route the input field to the readout layer.</param>
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
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public ExternalFieldSettings(ExternalFieldSettings source)
            : this(source.Name, source.FeatureFilterCfg, source.RouteToReadout)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
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
        /// Checks the defaults.
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
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new ExternalFieldSettings(this);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("field", suppressDefaults);
        }

    }//ExternalFieldSettings

}//Namespace
