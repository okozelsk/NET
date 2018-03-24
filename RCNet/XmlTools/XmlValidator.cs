using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace RCNet.XmlTools
{
    /// <summary>
    /// Provides xml loading/validation functionalities
    /// </summary>
    public class XmlValidator
    {
        //Constants
        //Attributes
        private XmlSchemaSet _schemaSet;

        //Constructor
        public XmlValidator()
        {
            _schemaSet = new XmlSchemaSet();
            return;
        }

        //Properties

        //Methods
        /// <summary>
        /// Loads new xml schema from the given stream. Stream is then closed and disposed.
        /// </summary>
        /// <param name="schemaStream">Stream from which to load xml schema</param>
        public void AddSchema(Stream schemaStream)
        {
            //Load schema
            XmlSchema schema = XmlSchema.Read(schemaStream, new ValidationEventHandler(XmlValidationCallback));
            //Add schema to schema set
            _schemaSet.Add(schema);
            schemaStream.Close();
            schemaStream.Dispose();
            return;
        }

        /// <summary>
        /// Creates new XDocument, content is loaded from given file and validated against stored xml schemaSet
        /// </summary>
        /// <param name="filename">Xml content of document to be loaded</param>
        /// <returns>XDocument</returns>
        public XDocument LoadXDocFromFile(string filename)
        {
            XDocument xDoc = XDocument.Load(filename);
            xDoc.Validate(_schemaSet, new ValidationEventHandler(XmlValidationCallback));
            return xDoc;
        }


        /// <summary>
        /// Creates new XDocument, content is loaded from given string and validated against stored xml schemaSet
        /// </summary>
        /// <param name="xmlContent">Xml content of document to be loaded</param>
        /// <returns>XDocument</returns>
        public XDocument LoadXDocFromString(string xmlContent)
        {

            XDocument xDoc = XDocument.Parse(xmlContent);
            xDoc.Validate(_schemaSet, new ValidationEventHandler(XmlValidationCallback));
            return xDoc;
        }

        /// <summary>
        /// Callback function called during validations.
        /// </summary>
        private void XmlValidationCallback(object sender, ValidationEventArgs args)
        {
            throw new Exception("Validation error: " + args.Message);
        }

    }//XMLValidator
}//Namespace
