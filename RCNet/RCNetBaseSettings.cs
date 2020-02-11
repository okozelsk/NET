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

        //Attributes

        //Constructors
        /// <summary>
        /// Static constructor
        /// </summary>
        static RCNetBaseSettings()
        {
            _RCNetTypesSchema = LoadRCNetTypesSchema();
            return;
        }

        /// <summary>
        /// Protected constructor.
        protected RCNetBaseSettings()
        {
            return;
        }

        //Static methods
        /// <summary>
        /// Loads schema instance of RCNetTypes.xsd
        /// </summary>
        public static XmlSchema LoadRCNetTypesSchema()
        {
            //Load shared schema instance of RCNetTypes.xsd
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
        /// <returns>Valid completed xml element</returns>
        public static XElement Validate(XElement elem, string xsdTypeName)
        {
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            //Add shared RCNetTypes schema
            schemaSet.Add(_RCNetTypesSchema);
            //Create in memory schema for given xsd type and add it to a schema set
            string specificXsdContent = $"<xs:schema attributeFormDefault=\"unqualified\" elementFormDefault=\"unqualified\" xmlns:rcn=\"RCNetTypes\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"> <xs:element name=\"{elem.Name.LocalName}\" type=\"rcn:{xsdTypeName}\"/> </xs:schema>";
            using (Stream byteStream = new MemoryStream(Encoding.UTF8.GetBytes(specificXsdContent)))
            {
                schemaSet.Add(XmlSchema.Read(byteStream, null));
            }
            //Validation and completion of the element
            XDocument elemDocument = new XDocument(elem);
            elemDocument.Validate(schemaSet, null, true);
            return elemDocument.Root;
        }

        //Methods

    }//RCNetBaseSettings

}//Namespace
