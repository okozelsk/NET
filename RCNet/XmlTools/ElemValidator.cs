using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace RCNet.XmlTools
{
    /// <summary>
    /// Class applies ugly and unefficient method to validate element against xsd type. This is
    /// unfortunatelly missing functionality in standard .net xml support functionality.
    /// Alone xsd schema has to be prepared for the xsd type.
    /// </summary>
    public class ElemValidator
    {
        //Attributes
        private DocValidator _validator;

        //Constructor
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        public ElemValidator()
        {
            _validator = new DocValidator();
            return;
        }

        //Methods
        /// <summary>
        /// Adds xml schema to validator's schema set
        /// </summary>
        /// <param name="assembly">Assembly object from which to load resource</param>
        /// <param name="manifestFullResourceName">Fully qualified name of the resource holding xsd.</param>
        public void AddXsdFromResources(Assembly assembly, string manifestFullResourceName)
        {
            using (Stream schemaStream = assembly.GetManifestResourceStream(manifestFullResourceName))
            {
                _validator.AddSchema(schemaStream);
            }
            return;
        }

        /// <summary>
        /// Validates given xml element against the schema.
        /// Function creates copy of the content of source element (to be validated) as an alone document, then
        /// renames root element to the specified name, then calls validation and if succeed, returns
        /// renamed, validated and completed root element.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="xsdRootElemName"></param>
        /// <returns></returns>
        public XElement Validate(XElement source, string xsdRootElemName = "rootElem")
        {
            XDocument doc = XDocument.Parse(source.ToString());
            XDocument validationSourceDoc = new XDocument(new XElement("rootElem", doc.Root.Attributes(), doc.Root.Nodes()));
            XDocument validaatedDoc = _validator.LoadXDocFromString(validationSourceDoc.ToString());
            return validaatedDoc.Root;
        }


    }//XmlElemValidator
}//Namespace
