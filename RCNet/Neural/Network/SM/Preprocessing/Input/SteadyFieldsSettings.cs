using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// The collection of the steady external fields configurations
    /// </summary>
    [Serializable]
    public class SteadyFieldsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPSteadyInpFieldsType";

        //Attribute properties
        /// <summary>
        /// Collection of steady external input field settings
        /// </summary>
        public List<SteadyFieldSettings> FieldCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        private SteadyFieldsSettings()
        {
            FieldCfgCollection = new List<SteadyFieldSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="fieldCfgCollection">Collection of steady external input field settings</param>
        public SteadyFieldsSettings(IEnumerable<SteadyFieldSettings> fieldCfgCollection)
            : this()
        {
            AddFields(fieldCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="fieldCfgCollection">Collection of steady external input field settings</param>
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
        /// <param name="source">Source instance</param>
        public SteadyFieldsSettings(SteadyFieldsSettings source)
            : this()
        {
            AddFields(source.FieldCfgCollection);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings.</param>
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
        /// Returns collection of field names
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
        /// Adds cloned settings from given collection into the internal collection
        /// </summary>
        /// <param name="fieldCfgCollection">Collection of steady external input field settings</param>
        private void AddFields(IEnumerable<SteadyFieldSettings> fieldCfgCollection)
        {
            foreach (SteadyFieldSettings fieldCfg in fieldCfgCollection)
            {
                FieldCfgCollection.Add((SteadyFieldSettings)fieldCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Returns the zero-based index of the field or -1 if given name not found
        /// </summary>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="ex">Specifies whether to throw exception when not found</param>
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
        /// Returns configuration of the given field
        /// </summary>
        /// <param name="fieldName">Field name</param>
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
