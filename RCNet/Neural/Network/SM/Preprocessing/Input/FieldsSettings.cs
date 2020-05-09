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
using RCNet.Neural.Data.Transformers;
using RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Contains configuration of external and generated input fields
    /// </summary>
    [Serializable]
    public class FieldsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPInpFieldsType";

        //Attribute properties
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
        /// Predictors settings
        /// </summary>
        public PredictorsSettings PredictorsCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="externalFieldsCfg">External input fields settings</param>
        /// <param name="transformedFieldsCfg">Transformed input fields settings</param>
        /// <param name="generatedFieldsCfg">generated input fields settings</param>
        public FieldsSettings(ExternalFieldsSettings externalFieldsCfg,
                              TransformedFieldsSettings transformedFieldsCfg = null,
                              GeneratedFieldsSettings generatedFieldsCfg = null,
                              PredictorsSettings predictorsCfg = null
                              )
        {
            ExternalFieldsCfg = (ExternalFieldsSettings)externalFieldsCfg.DeepClone();
            TransformedFieldsCfg = transformedFieldsCfg == null ? null : (TransformedFieldsSettings)transformedFieldsCfg.DeepClone();
            GeneratedFieldsCfg = generatedFieldsCfg == null ? null : (GeneratedFieldsSettings)generatedFieldsCfg.DeepClone();
            PredictorsCfg = predictorsCfg == null ? null : (PredictorsSettings)predictorsCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public FieldsSettings(FieldsSettings source)
            :this(source.ExternalFieldsCfg, source.TransformedFieldsCfg, source.GeneratedFieldsCfg, source.PredictorsCfg)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing settings.</param>
        public FieldsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            ExternalFieldsCfg = new ExternalFieldsSettings(settingsElem.Elements("externalFields").First());
            XElement transformedFieldsElem = settingsElem.Elements("transformedFields").FirstOrDefault();
            TransformedFieldsCfg = transformedFieldsElem == null ? null : new TransformedFieldsSettings(transformedFieldsElem);
            XElement generatedFieldsElem = settingsElem.Elements("generatedFields").FirstOrDefault();
            GeneratedFieldsCfg = generatedFieldsElem == null ? null : new GeneratedFieldsSettings(generatedFieldsElem);
            XElement predictorsElem = settingsElem.Elements("predictors").FirstOrDefault();
            PredictorsCfg = predictorsElem == null ? null : new PredictorsSettings(predictorsElem);
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
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
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
                    if(names.IndexOf(transformedFieldCfg.Name) != -1)
                    {
                        throw new Exception($"Field name {transformedFieldCfg.Name} is not unique.");
                    }
                    foreach(string name in TransformerFactory.GetAssociatedNames(transformedFieldCfg.TransformerCfg))
                    {
                        if (names.IndexOf(name) == -1)
                        {
                            throw new Exception($"Inconsistent input field name {name} as an input for transformed field. Field {name} is used before its definition or does not exist.");
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
                        throw new Exception($"Field name {generatedFieldCfg.Name} is not unique.");
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
            foreach(ExternalFieldSettings externalFieldCfg in ExternalFieldsCfg.FieldCfgCollection)
            {
                names.Add(externalFieldCfg.Name);
            }
            if(TransformedFieldsCfg != null)
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
        /// <param name="ex">Specifies if to throw exception when not found</param>
        public int GetFieldID(string fieldName, bool ex = true)
        {
            int index = 0;
            foreach (ExternalFieldSettings externalFieldCfg in ExternalFieldsCfg.FieldCfgCollection)
            {
                if(fieldName == externalFieldCfg.Name)
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
                throw new Exception($"Field name {fieldName} not found.");
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
            throw new Exception($"Field name {fieldName} not found.");
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new FieldsSettings(this);
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
            if (PredictorsCfg != null && (!PredictorsCfg.ContainsOnlyDefaults || !suppressDefaults))
            {
                rootElem.Add(PredictorsCfg.GetXml(suppressDefaults));
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
            return GetXml("fields", suppressDefaults);
        }

    }//FieldsSettings

}//Namespace
