using System;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.PM
{
    /// <summary>
    /// Configuration of the predictors mapper's allowed input field
    /// </summary>
    [Serializable]
    public class AllowedInputFieldSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "SMMapperAllowedInputFieldType";

        //Attribute properties
        /// <summary>
        /// Name of the input field
        /// </summary>
        public string Name { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="name">Name of the input field</param>
        public AllowedInputFieldSettings(string name)
        {
            Name = name;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public AllowedInputFieldSettings(AllowedInputFieldSettings source)
            : this(source.Name)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public AllowedInputFieldSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Name = settingsElem.Attribute("name").Value;
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return false;
            }
        }


        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (Name.Length == 0)
            {
                throw new ArgumentException($"Name can not be empty.", "Name");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new AllowedInputFieldSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("name", Name)
                                             );
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("field", suppressDefaults);
        }


    }//AllowedInputFieldSettings

}//Namespace
