using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Configuration of the external steady input fields.
    /// </summary>
    [Serializable]
    public class SteadyFieldsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "NPSteadyInpFieldsType";

        //Attribute properties
        /// <summary>
        /// The collection of the external steady input field configurations.
        /// </summary>
        public List<SteadyFieldSettings> FieldCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        private SteadyFieldsSettings()
        {
            FieldCfgCollection = new List<SteadyFieldSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="fieldCfgCollection">The collection of the external steady input field configurations.</param>
        public SteadyFieldsSettings(IEnumerable<SteadyFieldSettings> fieldCfgCollection)
            : this()
        {
            AddFields(fieldCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="fieldCfgCollection">The external steady input field configurations.</param>
        public SteadyFieldsSettings(params SteadyFieldSettings[] fieldCfgCollection)
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
        public SteadyFieldsSettings(SteadyFieldsSettings source)
            : this()
        {
            AddFields(source.FieldCfgCollection);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public SteadyFieldsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            FieldCfgCollection = new List<SteadyFieldSettings>();
            foreach (XElement fieldElem in settingsElem.Elements("field"))
            {
                FieldCfgCollection.Add(new SteadyFieldSettings(fieldElem));
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
                throw new ArgumentException($"At least one steady external field configuration must be specified.", "FieldCfgCollection");
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
        /// Gets the collection of the external steady input field names.
        /// </summary>
        public List<string> GetFieldNames()
        {
            List<string> names = new List<string>();
            foreach (SteadyFieldSettings fieldCfg in FieldCfgCollection)
            {
                names.Add(fieldCfg.Name);
            }
            return names;
        }

        /// <summary>
        /// Adds the external steady input field configurations from the specified collection into the internal collection.
        /// </summary>
        /// <param name="fieldCfgCollection">The collection of the external stedy input field configurations.</param>
        private void AddFields(IEnumerable<SteadyFieldSettings> fieldCfgCollection)
        {
            foreach (SteadyFieldSettings fieldCfg in fieldCfgCollection)
            {
                FieldCfgCollection.Add((SteadyFieldSettings)fieldCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Gets the zero-based index of the external steady input field.
        /// </summary>
        /// <param name="fieldName">The name of the external steady input field.</param>
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
        /// Gets the configuration of the specified external steady input field.
        /// </summary>
        /// <param name="fieldName">The name of the external steady input field.</param>
        public SteadyFieldSettings GetFieldCfg(string fieldName)
        {
            return FieldCfgCollection[GetFieldID(fieldName)];
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new SteadyFieldsSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach (SteadyFieldSettings fieldCfg in FieldCfgCollection)
            {
                rootElem.Add(fieldCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("steadyFields", suppressDefaults);
        }

    }//SteadyFieldsSettings

}//Namespace
