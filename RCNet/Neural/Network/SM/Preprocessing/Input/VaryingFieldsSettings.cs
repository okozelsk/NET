using RCNet.Neural.Data.Transformers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Contains configuration of external and generated varying input fields and input spikes coder
    /// </summary>
    [Serializable]
    public class VaryingFieldsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPVaryingInpFieldsType";
        //Default values
        /// <summary>
        /// Default value of parameter specifying if to route input fields to readout layer together with other predictors 
        /// </summary>
        public const bool DefaultRouteToReadout = false;

        //Attribute properties
        /// <summary>
        /// Input spikes coder settings
        /// </summary>
        public InputSpikesCoderSettings SpikesCoderCfg { get; }

        /// <summary>
        /// External input fields settings
        /// </summary>
        public ExternalFieldsSettings ExternalFieldsCfg { get; }

        /// <summary>
        /// Transformed input fields settings
        /// </summary>
        public TransformedFieldsSettings TransformedFieldsCfg { get; }

        /// <summary>
        /// Generated input fields settings
        /// </summary>
        public GeneratedFieldsSettings GeneratedFieldsCfg { get; }

        /// <summary>
        /// Specifies whether to route input fields to readout layer together with other predictors
        /// </summary>
        public bool RouteToReadout { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="spikesCoderCfg">Input spikes coder settings</param>
        /// <param name="externalFieldsCfg">External input fields settings</param>
        /// <param name="transformedFieldsCfg">Transformed input fields settings</param>
        /// <param name="generatedFieldsCfg">Generated input fields settings</param>
        /// <param name="routeToReadout">Specifies whether to route input fields to readout layer together with other predictors</param>
        public VaryingFieldsSettings(InputSpikesCoderSettings spikesCoderCfg,
                                     ExternalFieldsSettings externalFieldsCfg,
                                     TransformedFieldsSettings transformedFieldsCfg = null,
                                     GeneratedFieldsSettings generatedFieldsCfg = null,
                                     bool routeToReadout = DefaultRouteToReadout
                                     )
        {
            SpikesCoderCfg = (InputSpikesCoderSettings)spikesCoderCfg.DeepClone();
            ExternalFieldsCfg = (ExternalFieldsSettings)externalFieldsCfg.DeepClone();
            TransformedFieldsCfg = transformedFieldsCfg == null ? null : (TransformedFieldsSettings)transformedFieldsCfg.DeepClone();
            GeneratedFieldsCfg = generatedFieldsCfg == null ? null : (GeneratedFieldsSettings)generatedFieldsCfg.DeepClone();
            RouteToReadout = routeToReadout;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public VaryingFieldsSettings(VaryingFieldsSettings source)
            : this(source.SpikesCoderCfg, source.ExternalFieldsCfg, source.TransformedFieldsCfg, source.GeneratedFieldsCfg, source.RouteToReadout)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings.</param>
        public VaryingFieldsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            SpikesCoderCfg = new InputSpikesCoderSettings(settingsElem.Elements("spikesCoder").First());
            ExternalFieldsCfg = new ExternalFieldsSettings(settingsElem.Elements("externalFields").First());
            XElement transformedFieldsElem = settingsElem.Elements("transformedFields").FirstOrDefault();
            TransformedFieldsCfg = transformedFieldsElem == null ? null : new TransformedFieldsSettings(transformedFieldsElem);
            XElement generatedFieldsElem = settingsElem.Elements("generatedFields").FirstOrDefault();
            GeneratedFieldsCfg = generatedFieldsElem == null ? null : new GeneratedFieldsSettings(generatedFieldsElem);
            RouteToReadout = bool.Parse(settingsElem.Attribute("routeToReadout").Value);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Total number of fields (external + transformed + generated)
        /// </summary>
        public int TotalNumOfFields
        {
            get
            {
                return ExternalFieldsCfg.FieldCfgCollection.Count +
                       (TransformedFieldsCfg == null ? 0 : TransformedFieldsCfg.FieldCfgCollection.Count) +
                       (GeneratedFieldsCfg == null ? 0 : GeneratedFieldsCfg.FieldCfgCollection.Count);
            }
        }

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
            List<string> names = new List<string>(TotalNumOfFields);
            int index = 0;
            foreach (ExternalFieldSettings externalFieldCfg in ExternalFieldsCfg.FieldCfgCollection)
            {
                names.Add(externalFieldCfg.Name);
                ++index;
            }
            if (TransformedFieldsCfg != null)
            {
                foreach (TransformedFieldSettings transformedFieldCfg in TransformedFieldsCfg.FieldCfgCollection)
                {
                    if (names.IndexOf(transformedFieldCfg.Name) != -1)
                    {
                        throw new ArgumentException($"Field name {transformedFieldCfg.Name} is not unique.", "TransformedFieldsCfg");
                    }
                    foreach (string name in TransformerFactory.GetAssociatedNames(transformedFieldCfg.TransformerCfg))
                    {
                        if (names.IndexOf(name) == -1)
                        {
                            throw new ArgumentException($"Inconsistent input field name {name} as an input for transformed field. Field {name} is used before its definition or does not exist.", "TransformedFieldsCfg");
                        }
                    }
                    names.Add(transformedFieldCfg.Name);
                    ++index;
                }
            }
            if (GeneratedFieldsCfg != null)
            {
                foreach (GeneratedFieldSettings generatedFieldCfg in GeneratedFieldsCfg.FieldCfgCollection)
                {
                    if (names.IndexOf(generatedFieldCfg.Name) != -1)
                    {
                        throw new ArgumentException($"Field name {generatedFieldCfg.Name} is not unique.", "GeneratedFieldsCfg");
                    }
                    names.Add(generatedFieldCfg.Name);
                    ++index;
                }
            }
            return;
        }

        /// <summary>
        /// Returns names of all defined input fields
        /// </summary>
        public List<string> GetNames()
        {
            List<string> names = new List<string>(TotalNumOfFields);
            foreach (ExternalFieldSettings externalFieldCfg in ExternalFieldsCfg.FieldCfgCollection)
            {
                names.Add(externalFieldCfg.Name);
            }
            if (TransformedFieldsCfg != null)
            {
                foreach (TransformedFieldSettings transformedFieldCfg in TransformedFieldsCfg.FieldCfgCollection)
                {
                    names.Add(transformedFieldCfg.Name);
                }
            }
            if (GeneratedFieldsCfg != null)
            {
                foreach (GeneratedFieldSettings generatedFieldCfg in GeneratedFieldsCfg.FieldCfgCollection)
                {
                    names.Add(generatedFieldCfg.Name);
                }
            }
            return names;
        }

        /// <summary>
        /// Returns the zero-based index of the field among concated external and generated fields or -1 if name was not found.
        /// </summary>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="ex">Specifies whether to throw exception when not found</param>
        public int GetFieldID(string fieldName, bool ex = true)
        {
            int index = 0;
            foreach (ExternalFieldSettings externalFieldCfg in ExternalFieldsCfg.FieldCfgCollection)
            {
                if (fieldName == externalFieldCfg.Name)
                {
                    return index;
                }
                ++index;
            }
            if (TransformedFieldsCfg != null)
            {
                foreach (TransformedFieldSettings transformedFieldCfg in TransformedFieldsCfg.FieldCfgCollection)
                {
                    if (fieldName == transformedFieldCfg.Name)
                    {
                        return index;
                    }
                    ++index;
                }
            }
            if (GeneratedFieldsCfg != null)
            {
                foreach (GeneratedFieldSettings generatedFieldCfg in GeneratedFieldsCfg.FieldCfgCollection)
                {
                    if (fieldName == generatedFieldCfg.Name)
                    {
                        return index;
                    }
                    ++index;
                }
            }
            if (ex)
            {
                throw new InvalidOperationException($"Field name {fieldName} not found.");
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Returns configuration of the given field
        /// </summary>
        /// <param name="fieldName">Field name</param>
        public RCNetBaseSettings GetFieldCfg(string fieldName)
        {
            int index = ExternalFieldsCfg.GetFieldID(fieldName, false);
            if (index != -1)
            {
                return ExternalFieldsCfg.FieldCfgCollection[index];
            }
            if (GeneratedFieldsCfg != null)
            {
                index = GeneratedFieldsCfg.GetFieldID(fieldName, false);
                if (index != -1)
                {
                    return GeneratedFieldsCfg.FieldCfgCollection[index];
                }
            }
            throw new InvalidOperationException($"Field name {fieldName} not found.");
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new VaryingFieldsSettings(this);
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
                                             SpikesCoderCfg.GetXml(suppressDefaults),
                                             ExternalFieldsCfg.GetXml(suppressDefaults)
                                             );
            if (TransformedFieldsCfg != null)
            {
                rootElem.Add(TransformedFieldsCfg.GetXml(suppressDefaults));
            }
            if (GeneratedFieldsCfg != null)
            {
                rootElem.Add(GeneratedFieldsCfg.GetXml(suppressDefaults));
            }
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
            return GetXml("varyingFields", suppressDefaults);
        }

    }//VaryingFieldsSettings

}//Namespace
