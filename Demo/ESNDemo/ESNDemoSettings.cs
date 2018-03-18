using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Globalization;
using OKOSW.Extensions;
using OKOSW.CSVTools;
using OKOSW.Neural.Networks.EchoState;


namespace OKOSW.Demo
{
    public class ESNDemoSettings
    {
        //Constants
        //Attributes
        public string DataDir { get; }
        public List<DemoCaseParams> DemoCases { get; }

        //Constructor
        public ESNDemoSettings(string xmlFile)
        {
            Validate(xmlFile);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFile);
            XmlNode paramsNode= xmlDoc.SelectSingleNode("//ESNDemoSettings");
            DataDir = paramsNode.Attributes["DataDir"].Value;
            XmlNodeList nodes = paramsNode.SelectNodes("DemoCase");
            DemoCases = new List<DemoCaseParams>();
            foreach (XmlNode node in nodes)
            {
                DemoCases.Add(new DemoCaseParams(node, DataDir));
            }
            return;
        }
        //Properties
        //Methods
        private void Validate(String filename)
        {
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.Add("", "ESNDemoSettings.xsd");
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Schemas.Add(schemaSet);
            settings.ValidationEventHandler += new ValidationEventHandler(XmlValidationCallBack);
            settings.ValidationType = ValidationType.Schema;
            //Create the schema validating reader.
            XmlReader vreader = XmlReader.Create(filename, settings);
            while (vreader.Read()) { }
            //Close the reader.
            vreader.Close();
            return;
        }

        private void XmlValidationCallBack(object sender, ValidationEventArgs args)
        {
            throw new Exception("Validation error: " + args.Message);
        }

        //Inner classes
        //////////////////////////////////////////////////////////////////////////////////////
        public class DemoCaseParams
        {
            //Constants
            //Attributes
            public string Name { get; }
            public string CSVDataFileName { get; }
            public List<string> OutputFieldsNames { get; }
            public int BootSeqMinLength { get; }
            public int TrainingSeqMaxLength { get; }
            public int TestingSeqLength { get; }
            public string TestSamplesSelection { get; }
            public bool SingleNormalizer { get; }
            public double NormalizerReserveRatio { get; }
            public ESNSettings ESNCfg { get; }

            //Constructor
            public DemoCaseParams(XmlNode paramsNode, string dir)
            {
                Name = paramsNode.Attributes["Name"].Value;
                XmlNode samplesNode = paramsNode.SelectSingleNode("Samples");
                CSVDataFileName = dir + "\\" + samplesNode.Attributes["CSVDataFileName"].Value;
                BootSeqMinLength = int.Parse(samplesNode.Attributes["BootSeqMinLength"].Value);
                TrainingSeqMaxLength = int.Parse(samplesNode.Attributes["TrainingSeqMaxLength"].Value);
                TestingSeqLength = int.Parse(samplesNode.Attributes["TestingSeqLength"].Value);
                TestSamplesSelection = samplesNode.Attributes["TestSamplesSelection"].Value;
                SingleNormalizer = bool.Parse(samplesNode.Attributes["SingleNormalizer"].Value);
                NormalizerReserveRatio = double.Parse(samplesNode.Attributes["NormalizerReserveRatio"].Value, CultureInfo.InvariantCulture);
                OutputFieldsNames = new List<string>();
                XmlNode outputNode = paramsNode.SelectSingleNode("Output");
                foreach (XmlNode outputField in outputNode.SelectNodes("Field"))
                {
                    OutputFieldsNames.Add(outputField.Attributes["Name"].Value);
                }
                ESNCfg = new ESNSettings(paramsNode.SelectSingleNode("ESN"));
                return;
            }
        }//DemoCaseParams

    }//ESNDemoSettings
}//Namespace
