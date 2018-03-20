using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace OKOSW.XMLTools
{
    /// <summary>
    /// Provides XML loading/validation functionalities
    /// </summary>
    public class XMLValidator
    {
        //Constants
        //Attributes
        private XmlSchemaSet m_schemas;

        //Constructor
        public XMLValidator()
        {
            m_schemas = new XmlSchemaSet();
            return;
        }

        //Properties

        //Methods
        public void AddSchema(Stream schemaStream)
        {
            //Load schema
            XmlSchema schema = XmlSchema.Read(schemaStream, new ValidationEventHandler(XmlValidationCallBack));
            //Add schema to schema set
            m_schemas.Add(schema);
            schemaStream.Close();
            schemaStream.Dispose();
            return;
        }

        public XDocument LoadXDocFromFile(string filename)
        {
            XDocument xDoc = XDocument.Load(filename);
            xDoc.Validate(m_schemas, new ValidationEventHandler(XmlValidationCallBack));
            return xDoc;
        }

        public XDocument LoadXDocFromString(string xmlContent)
        {

            XDocument xDoc = XDocument.Parse(xmlContent);
            xDoc.Validate(m_schemas, new ValidationEventHandler(XmlValidationCallBack));
            return xDoc;
        }

        private void XmlValidationCallBack(object sender, ValidationEventArgs args)
        {
            throw new Exception("Validation error: " + args.Message);
        }

    }//XMLValidator
}//Namespace
