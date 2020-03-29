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
using RCNet.Neural.Data.Transformers;

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
        /// Specifies if to route transformed field to readout layer together with other predictors
        /// </summary>
        public bool RouteToReadout { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="name">Transformed field name</param>
        /// <param name="transformerCfg">Configuration of associated transformer</param>
        /// <param name="routeToReadout">Specifies if to route transformed field to readout layer together with other predictors</param>
        public TransformedFieldSettings(string name,
                                        RCNetBaseSettings transformerCfg,
                                        bool routeToReadout = DefaultRouteToReadout
                                        )
        {
            Name = name;
            TransformerCfg = transformerCfg.DeepClone();
            RouteToReadout = routeToReadout;
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public TransformedFieldSettings(TransformedFieldSettings source)
            :this(source.Name, source.TransformerCfg, source.RouteToReadout)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
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
            Type transType = TransformerCfg.GetType();
            if(transType != typeof(DiffTransformerSettings) &&
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
                throw new Exception($"Unsupported transformer configuration {transType.ToString()}.");
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
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
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

    }//TransformedFieldSettings

}//Namespace
