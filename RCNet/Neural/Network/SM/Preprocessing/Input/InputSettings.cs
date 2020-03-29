using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using RCNet.Extensions;
using RCNet.MathTools.Probability;
using RCNet.XmlTools;
using RCNet.RandomValue;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Contains configuration of the preprocessor input
    /// </summary>
    [Serializable]
    public class InputSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPInputType";

        //Attribute properties
        /// <summary>
        /// Input feeding settings
        /// </summary>
        public IFeedingSettings FeedingCfg { get; }

        /// <summary>
        /// Input fields settings
        /// </summary>
        public FieldsSettings FieldsCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="feedingCfg">Input feeding settings</param>
        /// <param name="fieldsCfg">Input fields settings</param>
        public InputSettings(IFeedingSettings feedingCfg, FieldsSettings fieldsCfg)
        {
            FeedingCfg = (IFeedingSettings)feedingCfg.DeepClone();
            FieldsCfg = (FieldsSettings)fieldsCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public InputSettings(InputSettings source)
            :this(source.FeedingCfg, source.FieldsCfg)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing settings.</param>
        public InputSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            XElement feedingElem = settingsElem.Elements().First();
            FeedingCfg = feedingElem.Name.LocalName == "feedingContinuous" ? (IFeedingSettings) new FeedingContinuousSettings(feedingElem) : new FeedingPatternedSettings(feedingElem);
            FieldsCfg = new FieldsSettings(settingsElem.Elements("fields").First());
            Check();
            return;
        }

        //Properties
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
            return;
        }

        /// <summary>
        /// Returns collection of input field names to be routed to readout layer as the predictors
        /// </summary>
        public List<string> GetRoutedFieldNames()
        {
            List<string> names = new List<string>();
            if(FeedingCfg.RouteToReadout)
            {
                foreach(ExternalFieldSettings fieldCfg in FieldsCfg.ExternalFieldsCfg.FieldCfgCollection)
                {
                    if(fieldCfg.RouteToReadout)
                    {
                        names.Add(fieldCfg.Name);
                    }
                }
                if (FieldsCfg.GeneratedFieldsCfg != null)
                {
                    foreach (GeneratedFieldSettings fieldCfg in FieldsCfg.GeneratedFieldsCfg.FieldCfgCollection)
                    {
                        if (fieldCfg.RouteToReadout)
                        {
                            names.Add(fieldCfg.Name);
                        }
                    }
                }
            }
            return names;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new InputSettings(this);
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
                                             FeedingCfg.GetXml(suppressDefaults),
                                             FieldsCfg.GetXml(suppressDefaults)
                                             );
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
            return GetXml("input", suppressDefaults);
        }

    }//InputSettings

}//Namespace
