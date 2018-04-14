using System;
using System.IO;
using System.Xml.Linq;
using System.Xml.Schema;

namespace RCNet.XmlTools
{
    /// <summary>
    /// The class provides the xml loading/validation functionalities
    /// </summary>
    public class DocValidator
    {
        //Constants
        //Attributes
        private XmlSchemaSet _schemaSet;

        //Constructor
        /// <summary>
        /// Instantiates a XmlValidator
        /// </summary>
        public DocValidator()
        {
            _schemaSet = new XmlSchemaSet();
            return;
        }

        //Methods
        /// <summary>
        /// Loads and adds the new xml schema from a given stream.
        /// </summary>
        /// <param name="schemaStream">A stream from which to load the xml schema</param>
        public void AddSchema(Stream schemaStream)
        {
            //Load the schema
            XmlSchema schema = XmlSchema.Read(schemaStream, new ValidationEventHandler(XmlValidationCallback));
            //Add the schema into the schema set
            _schemaSet.Add(schema);
            return;
        }

        /// <summary>
        /// Creates a new XDocument and loads its content from a given file.
        /// Xml document is validated against the stored SchemaSet
        /// </summary>
        /// <param name="filename">File containing the xml content</param>
        public XDocument LoadXDocFromFile(string filename)
        {
            XDocument xDoc = XDocument.Load(filename);
            xDoc.Validate(_schemaSet, new ValidationEventHandler(XmlValidationCallback), true);
            return xDoc;
        }


        /// <summary>
        /// Creates a new XDocument and loads its content from a given string.
        /// Xml document is validated against the stored SchemaSet
        /// </summary>
        /// <param name="xmlContent">A string containing the xml content</param>
        public XDocument LoadXDocFromString(string xmlContent)
        {

            XDocument xDoc = XDocument.Parse(xmlContent);
            xDoc.Validate(_schemaSet, new ValidationEventHandler(XmlValidationCallback), true);
            return xDoc;
        }

        /// <summary>
        /// Callback function called during validations.
        /// </summary>
        private void XmlValidationCallback(object sender, ValidationEventArgs args)
        {
            throw new Exception($"Validation error: {args.Message}");
        }

    }//XMLValidator

}//Namespace

