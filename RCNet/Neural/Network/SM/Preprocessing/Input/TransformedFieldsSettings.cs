using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Configuration of the transformed input fields.
    /// </summary>
    [Serializable]
    public class TransformedFieldsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "NPTransformedInpFieldsType";

        //Attribute properties
        /// <summary>
        /// The collection of the transformed input field configurations.
        /// </summary>
        public List<TransformedFieldSettings> FieldCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        private TransformedFieldsSettings()
        {
            FieldCfgCollection = new List<TransformedFieldSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="fieldCfgCollection">The collection of the transformed input field configurations.</param>
        public TransformedFieldsSettings(IEnumerable<TransformedFieldSettings> fieldCfgCollection)
            : this()
        {
            AddFields(fieldCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="fieldCfgCollection">The transformed input field configurations.</param>
        public TransformedFieldsSettings(params TransformedFieldSettings[] fieldCfgCollection)
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
        public TransformedFieldsSettings(TransformedFieldsSettings source)
            : this()
        {
            AddFields(source.FieldCfgCollection);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public TransformedFieldsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            FieldCfgCollection = new List<TransformedFieldSettings>();
            foreach (XElement fieldElem in settingsElem.Elements("field"))
            {
                FieldCfgCollection.Add(new TransformedFieldSettings(fieldElem));
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
                throw new ArgumentException($"At least one transformed field configuration must be specified.", "FieldCfgCollection");
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
        /// Gets the collection of the transformed input field names.
        /// </summary>
        public List<string> GetFieldNames()
        {
            List<string> names = new List<string>();
            foreach (TransformedFieldSettings fieldCfg in FieldCfgCollection)
            {
                names.Add(fieldCfg.Name);
            }
            return names;
        }

        /// <summary>
        /// Adds the transformed input field configurations from the specified collection into the internal collection.
        /// </summary>
        /// <param name="fieldCfgCollection">The collection of the transformed input field configurations.</param>
        private void AddFields(IEnumerable<TransformedFieldSettings> fieldCfgCollection)
        {
            foreach (TransformedFieldSettings fieldCfg in fieldCfgCollection)
            {
                FieldCfgCollection.Add((TransformedFieldSettings)fieldCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Gets the zero-based index of the transformed input field.
        /// </summary>
        /// <param name="fieldName">The name of the transformed input field.</param>
        /// <param name="ex">Specifies whether to throw an exception or return -1 in case the external steady input field not found.</param>
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
        /// Gets the configuration of the specified transformed input field.
        /// </summary>
        /// <param name="fieldName">The name of the transformed input field.</param>
        public TransformedFieldSettings GetFieldCfg(string fieldName)
        {
            return FieldCfgCollection[GetFieldID(fieldName)];
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new TransformedFieldsSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach (TransformedFieldSettings fieldCfg in FieldCfgCollection)
            {
                rootElem.Add(fieldCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("transformedFields", suppressDefaults);
        }

    }//TransformedFieldsSettings

}//Namespace
