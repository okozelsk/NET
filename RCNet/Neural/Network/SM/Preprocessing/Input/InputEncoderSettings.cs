using RCNet.Neural.Data.Coders.AnalogToSpiking;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Space3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Contains configuration of the preprocessor's input encoder
    /// </summary>
    [Serializable]
    public class InputEncoderSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPInputEncoderType";

        //Attribute properties
        /// <summary>
        /// Input feeding settings
        /// </summary>
        public IFeedingSettings FeedingCfg { get; }
        
        /// <summary>
        /// Varying input fields settings
        /// </summary>
        public VaryingFieldsSettings VaryingFieldsCfg { get; }

        /// <summary>
        /// Input placement in 3D space
        /// </summary>
        public CoordinatesSettings CoordinatesCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="feedingCfg">Input feeding settings</param>
        /// <param name="varyingFieldsCfg">Varying input fields settings</param>
        /// <param name="coordinatesCfg">Input placement in 3D space</param>
        public InputEncoderSettings(IFeedingSettings feedingCfg,
                                    VaryingFieldsSettings varyingFieldsCfg,
                                    CoordinatesSettings coordinatesCfg = null
                                    )
        {
            FeedingCfg = (IFeedingSettings)feedingCfg.DeepClone();
            VaryingFieldsCfg = (VaryingFieldsSettings)varyingFieldsCfg.DeepClone();
            CoordinatesCfg = coordinatesCfg == null ? new CoordinatesSettings() : (CoordinatesSettings)coordinatesCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public InputEncoderSettings(InputEncoderSettings source)
            : this(source.FeedingCfg, source.VaryingFieldsCfg, source.CoordinatesCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings.</param>
        public InputEncoderSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            XElement feedingElem = settingsElem.Elements().First();
            FeedingCfg = feedingElem.Name.LocalName == "feedingContinuous" ? (IFeedingSettings)new FeedingContinuousSettings(feedingElem) : new FeedingPatternedSettings(feedingElem);
            VaryingFieldsCfg = new VaryingFieldsSettings(settingsElem.Elements("varyingFields").First());
            XElement coordinatesElem = settingsElem.Elements("coordinates").FirstOrDefault();
            CoordinatesCfg = coordinatesElem == null ? new CoordinatesSettings() : new CoordinatesSettings(coordinatesElem);
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
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            //Uniqueness of all input field names
            if(FeedingCfg.FeedingType == InputEncoder.InputFeedingType.Patterned)
            {
                FeedingPatternedSettings patternedCfg = (FeedingPatternedSettings)FeedingCfg;
                if(patternedCfg.SteadyFieldsCfg != null)
                {
                    //There are steady fields defined
                    foreach(SteadyFieldSettings sf in patternedCfg.SteadyFieldsCfg.FieldCfgCollection)
                    {
                        if (VaryingFieldsCfg.GetFieldID(sf.Name, false) != -1)
                        {
                            throw new InvalidOperationException($"Steady field name {sf.Name} found among varying fields.");
                        }
                    }
                }
            }

            return;
        }

        /// <summary>
        /// Returns collection of input field names to be routed to readout layer as the predictors
        /// </summary>
        public List<string> GetRoutedFieldNames()
        {
            List<string> names = new List<string>();
            //Steady fields
            if (FeedingCfg.FeedingType == InputEncoder.InputFeedingType.Patterned)
            {
                FeedingPatternedSettings patternedCfg = (FeedingPatternedSettings)FeedingCfg;
                if (patternedCfg.SteadyFieldsCfg != null)
                {
                    //There are steady fields defined
                    foreach (SteadyFieldSettings sf in patternedCfg.SteadyFieldsCfg.FieldCfgCollection)
                    {
                        if (sf.RouteToReadout)
                        {
                            names.Add(sf.Name);
                        }
                    }
                }
            }
            //Varying fields
            if (VaryingFieldsCfg.RouteToReadout)
            {
                foreach (ExternalFieldSettings fieldCfg in VaryingFieldsCfg.ExternalFieldsCfg.FieldCfgCollection)
                {
                    if (fieldCfg.RouteToReadout)
                    {
                        names.Add(fieldCfg.Name);
                    }
                }
                if (VaryingFieldsCfg.TransformedFieldsCfg != null)
                {
                    foreach (TransformedFieldSettings fieldCfg in VaryingFieldsCfg.TransformedFieldsCfg.FieldCfgCollection)
                    {
                        if (fieldCfg.RouteToReadout)
                        {
                            names.Add(fieldCfg.Name);
                        }
                    }
                }
                if (VaryingFieldsCfg.GeneratedFieldsCfg != null)
                {
                    foreach (GeneratedFieldSettings fieldCfg in VaryingFieldsCfg.GeneratedFieldsCfg.FieldCfgCollection)
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
            return new InputEncoderSettings(this);
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
                                             FeedingCfg.GetXml(suppressDefaults),
                                             VaryingFieldsCfg.GetXml(suppressDefaults)
                                             );
            if (!suppressDefaults || !CoordinatesCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(CoordinatesCfg.GetXml(suppressDefaults));
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
            return GetXml("inputEncoder", suppressDefaults);
        }

    }//InputEncoderSettings

}//Namespace
