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
    /// Configuration of an internal input field
    /// </summary>
    [Serializable]
    public class InternalFieldSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPInternalInpFieldType";
        //Default values
        /// <summary>
        /// Default value of parameter specifying if to route input field to readout layer together with other predictors 
        /// </summary>
        public const bool DefaultRouteToReadout = false;

        //Attribute properties
        /// <summary>
        /// Input field name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Configuration of the generator associated with the input field
        /// </summary>
        public RCNetBaseSettings GeneratorCfg { get; }

        /// <summary>
        /// Specifies if to route input field to readout layer together with other predictors
        /// </summary>
        public bool RouteToReadout { get; }


        //Constructors
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        /// <param name="name">Input field name</param>
        /// <param name="generatorCfg">Configuration of the generator associated with the input field</param>
        /// <param name="routeToReadout">Specifies if to route input field to readout layer together with other predictors</param>
        public InternalFieldSettings(string name,
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
        public InternalFieldSettings(InternalFieldSettings source)
            :this(source.Name, source.GeneratorCfg, source.RouteToReadout)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing the settings.</param>
        public InternalFieldSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Name = settingsElem.Attribute("name").Value;
            RouteToReadout = bool.Parse(settingsElem.Attribute("routeToReadout").Value);
            XElement genElem = settingsElem.Descendants().First();
            switch(genElem.Name.LocalName)
            {
                case "pulse":
                    GeneratorCfg = new PulseGeneratorSettings(genElem);
                    break;
                case "random":
                    GeneratorCfg = new RandomValueSettings(genElem);
                    break;
                case "sinusoidal":
                    GeneratorCfg = new SinusoidalGeneratorSettings(genElem);
                    break;
                case "mackeyGlass":
                    GeneratorCfg = new MackeyGlassGeneratorSettings(genElem);
                    break;
                default:
                    throw new Exception($"Unsupported generator configuration element {genElem.Name.LocalName}.");
            }
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
            return new InternalFieldSettings(this);
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

    }//InternalFieldSettings

}//Namespace
