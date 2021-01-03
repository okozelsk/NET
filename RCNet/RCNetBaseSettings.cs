using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace RCNet
{
    /// <summary>
    /// The base class of all configurations classes.
    /// </summary>
    [Serializable]
    public abstract class RCNetBaseSettings
    {
        //Static attributes
        /// <summary>
        /// The shared schema instance of RCNetTypes.
        /// </summary>
        protected static readonly XmlSchema _RCNetTypesSchema;
        /// <summary>
        /// The shared compiled schema set.
        /// </summary>
        protected static readonly XmlSchemaSet _validationSchemaSet;

        //Attributes

        //Constructors
        /// <summary>
        /// The static constructor.
        /// </summary>
        static RCNetBaseSettings()
        {
            _RCNetTypesSchema = LoadRCNetTypesSchema();
            _validationSchemaSet = new XmlSchemaSet();
            _validationSchemaSet.Add(_RCNetTypesSchema);
            _validationSchemaSet.Compile();
            return;
        }

        /// <summary>
        /// The protected constructor.
        /// </summary>
        protected RCNetBaseSettings()
        {
            return;
        }

        //Properties
        /// <summary>
        /// Indicates the fully default configuration.
        /// </summary>
        public abstract bool ContainsOnlyDefaults { get; }

        //Static methods
        /// <summary>
        /// Loads an instance of the RCNetTypes schema.
        /// </summary>
        public static XmlSchema LoadRCNetTypesSchema()
        {
            //Load an instance of RCNetTypes.xsd
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            using (Stream schemaStream = assemblyRCNet.GetManifestResourceStream("RCNet.RCNetTypes.xsd"))
            {
                return XmlSchema.Read(schemaStream, null);
            }
        }

        /// <summary>
        /// Validates and completes the xml element against the specified xsd type defined in RCNetTypes schema.
        /// </summary>
        /// <param name="elem">The xml element to be validated and completed.</param>
        /// <param name="xsdTypeName">The name of the xsd type defined in RCNetTypes schema.</param>
        /// <param name="newElemInstance">Specifies whether to create the new xml element instance.</param>
        /// <returns>The validated and completed xml element.</returns>
        public static XElement Validate(XElement elem, string xsdTypeName, bool newElemInstance = true)
        {
            XmlQualifiedName xmlQualifiedName = new XmlQualifiedName(xsdTypeName, "RCNetTypes");
            if (newElemInstance)
            {
                XElement validatedElem = new XElement(elem);
                validatedElem.Validate(_RCNetTypesSchema.SchemaTypes[xmlQualifiedName], _validationSchemaSet, null, true);
                return validatedElem;
            }
            else
            {
                elem.Validate(_RCNetTypesSchema.SchemaTypes[xmlQualifiedName], _validationSchemaSet, null, true);
                return elem;
            }
        }

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance.
        /// </summary>
        public abstract RCNetBaseSettings DeepClone();

        /// <summary>
        /// Checks the correctness of the configuration.
        /// </summary>
        protected abstract void Check();

        /// <summary>
        /// Gets the generated xml element containing the entire configuration.
        /// </summary>
        /// <param name="rootElemName">The name of the root xml element.</param>
        /// <param name="suppressDefaults">Specifies whether to omit optional nodes containing only defaults.</param>
        /// <returns>The xml element containing the entire configuration.</returns>
        public abstract XElement GetXml(string rootElemName, bool suppressDefaults);

        /// <summary>
        /// Gets the generated defaultly named xml element containing the entire configuration.
        /// </summary>
        /// <param name="suppressDefaults">Specifies whether to omit optional nodes containing only defaults.</param>
        /// <returns>The xml element containing the entire configuration.</returns>
        public virtual XElement GetXml(bool suppressDefaults)
        {
            return GetXml("config", suppressDefaults);
        }

    }//RCNetBaseSettings

}//Namespace
