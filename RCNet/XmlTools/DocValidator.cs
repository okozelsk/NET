using System;
using System.IO;
using System.Xml.Linq;
using System.Xml.Schema;

namespace RCNet.XmlTools
{
    /// <summary>
    /// The class provides helper xml document loading/validation functionalities
    /// </summary>
    public class DocValidator
    {
        //Constants
        //Attributes
        private readonly XmlSchemaSet _schemaSet;

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
        /// Adds given xml schema into the schema set.
        /// </summary>
        /// <param name="xmlSchema">Xml schema to be added</param>
        public void AddSchema(XmlSchema xmlSchema)
        {
            //Add the schema into the schema set
            _schemaSet.Add(xmlSchema);
            return;
        }

        /// <summary>
        /// Loads xml schema from a given stream and adds it into the schema set.
        /// </summary>
        /// <param name="schemaStream">A stream from which to load the xml schema</param>
        public void AddSchema(Stream schemaStream)
        {
            //Load the schema
            XmlSchema schema = XmlSchema.Read(schemaStream, new ValidationEventHandler(XmlValidationCallback));
            //Add the schema into the schema set
            AddSchema(schema);
            return;
        }

        /// <summary>
        /// Creates a new XDocument and loads its content from a given file.
        /// Xml document is validated against the stored SchemaSet
        /// </summary>
        /// <param name="filename">File containing the xml content</param>
        public XDocument LoadXDocFromFile(string filename)
        {
            var binDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            XDocument xDoc = XDocument.Load(Path.Combine(binDir, filename));
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
            throw new InvalidOperationException($"Validation error: {args.Message}");
        }

    }//DocValidator

}//Namespace

