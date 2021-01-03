using RCNet.Neural.Data.Transformers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Configuration of all the varying input fields.
    /// </summary>
    [Serializable]
    public class VaryingFieldsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "NPVaryingInpFieldsType";
        //Default values
        /// <summary>
        /// The default value of the parameter specifying whether to route the varying input fields to the readout layer.
        /// </summary>
        public const bool DefaultRouteToReadout = false;

        //Attribute properties
        /// <summary>
        /// The configuration of the input spikes coder.
        /// </summary>
        public InputSpikesCoderSettings InputSpikesCoderCfg { get; }

        /// <summary>
        /// The configuration of the external input fields.
        /// </summary>
        public ExternalFieldsSettings ExternalFieldsCfg { get; }

        /// <summary>
        /// The configuration of the transformed input fields.
        /// </summary>
        public TransformedFieldsSettings TransformedFieldsCfg { get; }

        /// <summary>
        /// The configuration of the generated input fields.
        /// </summary>
        public GeneratedFieldsSettings GeneratedFieldsCfg { get; }

        /// <summary>
        /// Specifies whether to route the varying input fields to the readout layer.
        /// </summary>
        public bool RouteToReadout { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="spikesCoderCfg">The configuration of the input spikes coder.</param>
        /// <param name="externalFieldsCfg">The configuration of the external input fields.</param>
        /// <param name="transformedFieldsCfg">The configuration of the transformed input fields.</param>
        /// <param name="generatedFieldsCfg">The configuration of the generated input fields.</param>
        /// <param name="routeToReadout">Specifies whether to route the varying input fields to the readout layer.</param>
        public VaryingFieldsSettings(InputSpikesCoderSettings spikesCoderCfg,
                                     ExternalFieldsSettings externalFieldsCfg,
                                     TransformedFieldsSettings transformedFieldsCfg = null,
                                     GeneratedFieldsSettings generatedFieldsCfg = null,
                                     bool routeToReadout = DefaultRouteToReadout
                                     )
        {
            InputSpikesCoderCfg = (InputSpikesCoderSettings)spikesCoderCfg.DeepClone();
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
        /// <param name="source">The source instance.</param>
        public VaryingFieldsSettings(VaryingFieldsSettings source)
            : this(source.InputSpikesCoderCfg, source.ExternalFieldsCfg, source.TransformedFieldsCfg, source.GeneratedFieldsCfg, source.RouteToReadout)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public VaryingFieldsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            InputSpikesCoderCfg = new InputSpikesCoderSettings(settingsElem.Elements("spikesCoder").First());
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
        /// Gets the total number of varying input fields (external + transformed + generated).
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
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultRouteToReadout { get { return (RouteToReadout == DefaultRouteToReadout); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc/>
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
        /// Gets the collection of all defined varying input fields.
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
        /// Gets the zero-based index of the varying input field.
        /// </summary>
        /// <param name="fieldName">The name of the varying input field.</param>
        /// <param name="ex">Specifies whether to throw an exception or return -1 in case the varying input field not found.</param>
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
        /// Gets the configuration of the varying input field.
        /// </summary>
        /// <param name="fieldName">The name of the varying input field.</param>
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

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new VaryingFieldsSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             InputSpikesCoderCfg.GetXml(suppressDefaults),
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

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("varyingFields", suppressDefaults);
        }

    }//VaryingFieldsSettings

}//Namespace
