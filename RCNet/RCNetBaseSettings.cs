using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Xml.Linq;
using System.Reflection;
using System.Xml.Schema;
using System.IO;
using System.Xml;
using System.Text;

namespace RCNet
{
    /// <summary>
    /// Base class for standard settings classes used in RCNet library
    /// </summary>
    [Serializable]
    public abstract class RCNetBaseSettings
    {
        //Static attributes
        /// <summary>
        /// Shared schema instance of RCNetTypes.xsd
        /// </summary>
        protected static readonly XmlSchema _RCNetTypesSchema;
        /// <summary>
        /// Shared compiled schema set
        /// </summary>
        protected static readonly XmlSchemaSet _validationSchemaSet;

        //Attributes

        //Constructors
        /// <summary>
        /// Static constructor
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
        /// Protected constructor.
        /// </summary>
        protected RCNetBaseSettings()
        {
            return;
        }

        //Properties
        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public abstract bool ContainsOnlyDefaults { get; }

        //Static methods
        /// <summary>
        /// Loads schema instance of RCNetTypes.xsd
        /// </summary>
        public static XmlSchema LoadRCNetTypesSchema()
        {
            //Load instance of RCNetTypes.xsd
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            using (Stream schemaStream = assemblyRCNet.GetManifestResourceStream("RCNet.RCNetTypes.xsd"))
            {
                return XmlSchema.Read(schemaStream, null);
            }
        }
        
        /// <summary>
        /// Validates and completes given xml element against specified xsd type defined in RCNetTypes.xsd
        /// </summary>
        /// <param name="elem">Xml element to be validated and completed</param>
        /// <param name="xsdTypeName">Name of the xsd type defined in RCNetTypes.xsd to be used for xml element validation and completion</param>
        /// <param name="newElemInstance">If true, new XElement instance will be returned and elem will stay unchanged. If false, the same instance of elem will be returned.</param>
        /// <returns>Validated and completed xml element</returns>
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
        /// Creates the deep copy instance of this instance
        /// </summary>
        public abstract RCNetBaseSettings DeepClone();

        /// <summary>
        /// Checks consistency
        /// </summary>
        protected abstract void Check();

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public virtual XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            throw new NotImplementedException("GetXml method is not implemented.");
        }

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public virtual XElement GetXml(bool suppressDefaults)
        {
            throw new NotImplementedException("GetXml method is not implemented.");
        }

    }//RCNetBaseSettings

}//Namespace
