using System;
using System.IO;
using System.Xml.Linq;
using System.Xml.Schema;

namespace RCNet.XmlTools
{
    /// <summary>
    /// Implements the xml document loader and validator.
    /// </summary>
    public class DocValidator
    {
        //Constants
        //Attributes
        private readonly XmlSchemaSet _schemaSet;

        //Constructor
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        public DocValidator()
        {
            _schemaSet = new XmlSchemaSet();
            return;
        }

        //Methods
        /// <summary>
        /// Adds the specified xml schema into the schema set.
        /// </summary>
        /// <param name="xmlSchema">The xml schema to be added.</param>
        public void AddSchema(XmlSchema xmlSchema)
        {
            //Add the schema into the schema set
            _schemaSet.Add(xmlSchema);
            return;
        }

        /// <summary>
        /// Loads the xml schema from a stream and adds it into the schema set.
        /// </summary>
        /// <param name="schemaStream">The stream to load from.</param>
        public void AddSchema(Stream schemaStream)
        {
            //Load the schema
            XmlSchema schema = XmlSchema.Read(schemaStream, new ValidationEventHandler(XmlValidationCallback));
            //Add the schema into the schema set
            AddSchema(schema);
            return;
        }

        /// <summary>
        /// Loads the xml document from file.
        /// </summary>
        /// <remarks>
        /// The xml document is validated against the internal SchemaSet.
        /// </remarks>
        /// <param name="filename">The name of the xml file.</param>
        public XDocument LoadXDocFromFile(string filename)
        {
            var binDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            XDocument xDoc = XDocument.Load(Path.Combine(binDir, filename));
            xDoc.Validate(_schemaSet, new ValidationEventHandler(XmlValidationCallback), true);
            return xDoc;
        }


        /// <summary>
        /// Loads the xml document from string.
        /// </summary>
        /// <remarks>
        /// The xml document is validated against the internal SchemaSet.
        /// </remarks>
        /// <param name="xmlContent">The xml content.</param>
        public XDocument LoadXDocFromString(string xmlContent)
        {

            XDocument xDoc = XDocument.Parse(xmlContent);
            xDoc.Validate(_schemaSet, new ValidationEventHandler(XmlValidationCallback), true);
            return xDoc;
        }

        /// <summary>
        /// The callback function called during the xml validation.
        /// </summary>
        private void XmlValidationCallback(object sender, ValidationEventArgs args)
        {
            throw new InvalidOperationException($"Validation error: {args.Message}");
        }

    }//DocValidator

}//Namespace

