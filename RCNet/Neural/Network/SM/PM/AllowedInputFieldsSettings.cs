using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.PM
{
    /// <summary>
    /// Configuration of the predictors mapper's allowed input fields.
    /// </summary>
    [Serializable]
    public class AllowedInputFieldsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "SMMapperAllowedInputFieldsType";

        //Attribute properties
        /// <summary>
        /// The collection of the allowed input field configurations.
        /// </summary>
        public List<AllowedInputFieldSettings> AllowedInputFieldCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        private AllowedInputFieldsSettings()
        {
            AllowedInputFieldCfgCollection = new List<AllowedInputFieldSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="allowedInputFieldCfgCollection">The collection of the allowed input field configurations.</param>
        public AllowedInputFieldsSettings(IEnumerable<AllowedInputFieldSettings> allowedInputFieldCfgCollection)
            : this()
        {
            AddAllowedInputFields(allowedInputFieldCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="allowedInputFieldCfgCollection">The allowed input field configurations.</param>
        public AllowedInputFieldsSettings(params AllowedInputFieldSettings[] allowedInputFieldCfgCollection)
            : this()
        {
            AddAllowedInputFields(allowedInputFieldCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">The source instance.</param>
        public AllowedInputFieldsSettings(AllowedInputFieldsSettings source)
            : this()
        {
            AddAllowedInputFields(source.AllowedInputFieldCfgCollection);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public AllowedInputFieldsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            AllowedInputFieldCfgCollection = new List<AllowedInputFieldSettings>();
            foreach (XElement fieldElem in settingsElem.Elements("field"))
            {
                AllowedInputFieldCfgCollection.Add(new AllowedInputFieldSettings(fieldElem));
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
            if (AllowedInputFieldCfgCollection.Count == 0)
            {
                throw new ArgumentException($"At least one allowed input field configuration must be specified.", "AllowedInputFieldCfgCollection");
            }
            //Uniqueness of the field name
            string[] names = new string[AllowedInputFieldCfgCollection.Count];
            names[0] = AllowedInputFieldCfgCollection[0].Name;
            for (int i = 1; i < AllowedInputFieldCfgCollection.Count; i++)
            {
                if (names.Contains(AllowedInputFieldCfgCollection[i].Name))
                {
                    throw new ArgumentException($"Input field name {AllowedInputFieldCfgCollection[i].Name} is not unique.", "AllowedInputFieldCfgCollection");
                }
                names[i] = AllowedInputFieldCfgCollection[i].Name;
            }
            return;
        }

        /// <summary>
        /// Adds allowed input field configurations from the specified collection into the internal collection.
        /// </summary>
        /// <param name="allowedInputFieldCfgCollection">The collection of the allowed input field configurations.</param>
        private void AddAllowedInputFields(IEnumerable<AllowedInputFieldSettings> allowedInputFieldCfgCollection)
        {
            foreach (AllowedInputFieldSettings allowedInputFieldCfg in allowedInputFieldCfgCollection)
            {
                AllowedInputFieldCfgCollection.Add((AllowedInputFieldSettings)allowedInputFieldCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Checks whether the specified input field is allowed.
        /// </summary>
        /// <param name="inputFieldName">The name of the input field.</param>
        public bool IsAllowed(string inputFieldName)
        {
            foreach (AllowedInputFieldSettings fieldCfg in AllowedInputFieldCfgCollection)
            {
                if (fieldCfg.Name == inputFieldName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new AllowedInputFieldsSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach (AllowedInputFieldSettings allowedInputFieldCfg in AllowedInputFieldCfgCollection)
            {
                rootElem.Add(allowedInputFieldCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("allowedInputFields", suppressDefaults);
        }

    }//AllowedInputFieldsSettings

}//Namespace
