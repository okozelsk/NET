using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Space3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Configuration of the InputEncoder.
    /// </summary>
    [Serializable]
    public class InputEncoderSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "NPInputEncoderType";

        //Attribute properties
        /// <summary>
        /// The configuration of the input feeding.
        /// </summary>
        public IFeedingSettings FeedingCfg { get; }

        /// <summary>
        /// The configuration of the varying input fields.
        /// </summary>
        public VaryingFieldsSettings VaryingFieldsCfg { get; }

        /// <summary>
        /// Configuration of the input placement.
        /// </summary>
        public CoordinatesSettings CoordinatesCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="feedingCfg">The configuration of the input feeding.</param>
        /// <param name="varyingFieldsCfg">The configuration of the varying input fields.</param>
        /// <param name="coordinatesCfg">Configuration of the input placement.</param>
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
        /// <param name="source">The source instance.</param>
        public InputEncoderSettings(InputEncoderSettings source)
            : this(source.FeedingCfg, source.VaryingFieldsCfg, source.CoordinatesCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
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
        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            //Uniqueness of all input field names
            if (FeedingCfg.FeedingType == InputEncoder.InputFeedingType.Patterned)
            {
                FeedingPatternedSettings patternedCfg = (FeedingPatternedSettings)FeedingCfg;
                if (patternedCfg.SteadyFieldsCfg != null)
                {
                    //There are steady fields defined
                    foreach (SteadyFieldSettings sf in patternedCfg.SteadyFieldsCfg.FieldCfgCollection)
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
        /// Gets the collection of input field names being routed to readout layer.
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

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new InputEncoderSettings(this);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("inputEncoder", suppressDefaults);
        }

    }//InputEncoderSettings

}//Namespace
