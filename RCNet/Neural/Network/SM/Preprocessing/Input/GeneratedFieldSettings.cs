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
using RCNet.Neural.Data.Generators;
using RCNet.RandomValue;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Configuration of a generated input field
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
        /// Specifies if to route generated field to readout layer together with other predictors
        /// </summary>
        public bool RouteToReadout { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="name">Generated field name</param>
        /// <param name="generatorCfg">Configuration of associated generator</param>
        /// <param name="routeToReadout">Specifies if to route generated field to readout layer together with other predictors</param>
        public GeneratedFieldSettings(string name,
                                      RCNetBaseSettings generatorCfg,
                                      bool routeToReadout = DefaultRouteToReadout
                                      )
        {
            Name = name;
            GeneratorCfg = generatorCfg.DeepClone();
            RouteToReadout = routeToReadout;
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public GeneratedFieldSettings(GeneratedFieldSettings source)
            :this(source.Name, source.GeneratorCfg, source.RouteToReadout)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
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
            Type genType = GeneratorCfg.GetType();
            if(genType != typeof(PulseGeneratorSettings) &&
               genType != typeof(RandomValueSettings) &&
               genType != typeof(SinusoidalGeneratorSettings) &&
               genType != typeof(MackeyGlassGeneratorSettings))
            {
                throw new Exception($"Unsupported generator configuration {genType.ToString()}.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance.
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new GeneratedFieldSettings(this);
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
                                             GeneratorCfg.GetXml(suppressDefaults)
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
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("field", suppressDefaults);
        }

    }//GeneratedFieldSettings

}//Namespace
