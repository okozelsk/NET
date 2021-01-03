using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// The configuration of the generated input fields.
    /// </summary>
    [Serializable]
    public class GeneratedFieldsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "NPGeneratedInpFieldsType";

        //Attribute properties
        /// <summary>
        /// The collection of the generated input field configurations.
        /// </summary>
        public List<GeneratedFieldSettings> FieldCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        private GeneratedFieldsSettings()
        {
            FieldCfgCollection = new List<GeneratedFieldSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="fieldCfgCollection">The collection of the generated input field configurations.</param>
        public GeneratedFieldsSettings(IEnumerable<GeneratedFieldSettings> fieldCfgCollection)
            : this()
        {
            AddFields(fieldCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="fieldCfgCollection">The generated input field configurations.</param>
        public GeneratedFieldsSettings(params GeneratedFieldSettings[] fieldCfgCollection)
            : this()
        {
            AddFields(fieldCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">The source instance.</param>
        public GeneratedFieldsSettings(GeneratedFieldsSettings source)
            : this()
        {
            AddFields(source.FieldCfgCollection);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public GeneratedFieldsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            FieldCfgCollection = new List<GeneratedFieldSettings>();
            foreach (XElement fieldElem in settingsElem.Elements("field"))
            {
                FieldCfgCollection.Add(new GeneratedFieldSettings(fieldElem));
            }
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
            if (FieldCfgCollection.Count == 0)
            {
                throw new ArgumentException($"At least one internal field configuration must be specified.", "FieldCfgCollection");
            }
            //Uniqueness of field names
            string[] names = new string[FieldCfgCollection.Count];
            names[0] = FieldCfgCollection[0].Name;
            for (int i = 1; i < FieldCfgCollection.Count; i++)
            {
                if (names.Contains(FieldCfgCollection[i].Name))
                {
                    throw new ArgumentException($"Field name {FieldCfgCollection[i].Name} is not unique.", "FieldCfgCollection");
                }
                names[i] = FieldCfgCollection[i].Name;
            }
            return;
        }

        /// <summary>
        /// Adds the generated input field configurations from the specified collection into the internal collection.
        /// </summary>
        /// <param name="fieldCfgCollection">The collection of the generated input field configurations.</param>
        private void AddFields(IEnumerable<GeneratedFieldSettings> fieldCfgCollection)
        {
            foreach (GeneratedFieldSettings fieldCfg in fieldCfgCollection)
            {
                FieldCfgCollection.Add((GeneratedFieldSettings)fieldCfg.DeepClone());
            }
            return;
        }


        /// <summary>
        /// Gets the zero-based index of the generated input field.
        /// </summary>
        /// <param name="fieldName">The name of the generated input field.</param>
        /// <param name="ex">Specifies whether to throw an exception or return -1 in case the generated input field not found.</param>
        public int GetFieldID(string fieldName, bool ex = true)
        {
            for (int i = 0; i < FieldCfgCollection.Count; i++)
            {
                if (FieldCfgCollection[i].Name == fieldName)
                {
                    return i;
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
        /// Gets the configuration of the specified generated input field.
        /// </summary>
        /// <param name="fieldName">The name of the generated input field.</param>
        public GeneratedFieldSettings GetFieldCfg(string fieldName)
        {
            return FieldCfgCollection[GetFieldID(fieldName)];
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new GeneratedFieldsSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach (GeneratedFieldSettings fieldCfg in FieldCfgCollection)
            {
                rootElem.Add(fieldCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("generatedFields", suppressDefaults);
        }

    }//GeneratedFieldsSettings

}//Namespace
