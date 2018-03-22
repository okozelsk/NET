using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;
using System.Globalization;
using OKOSW.Extensions;
using OKOSW.XMLTools;
using OKOSW.Neural;
using OKOSW.Neural.Networks.EchoState;
using System.Reflection;

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
            XmlValidator validator = new XmlValidator();
            Assembly esnDemoAssembly = Assembly.GetExecutingAssembly();
            Assembly neuralAssembly = Assembly.Load("Neural");
            validator.AddSchema(esnDemoAssembly.GetManifestResourceStream("OKOSW.Demo.ESNDemoSettings.xsd"));
            validator.AddSchema(neuralAssembly.GetManifestResourceStream("OKOSW.Neural.OKOSWNeuralSettingsTypes.xsd"));
            XDocument xmlDoc = validator.LoadXDocFromFile(xmlFile);
            XElement root = xmlDoc.Descendants("ESNDemoSettings").First();
            DataDir = root.Attribute("DataDir").Value;
            DemoCases = new List<DemoCaseParams>();
            foreach (XElement demoCaseParamsElem in root.Descendants("DemoCase"))
            {
                DemoCases.Add(new DemoCaseParams(demoCaseParamsElem, DataDir));
            }

            return;
        }
        //Properties
        //Methods

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
            public DemoCaseParams(XElement demoCaseElem, string dir)
            {
                Name = demoCaseElem.Attribute("Name").Value;
                XElement samplesElem = demoCaseElem.Descendants("Samples").First();
                CSVDataFileName = dir + "\\" + samplesElem.Attribute("CSVDataFileName").Value;
                BootSeqMinLength = int.Parse(samplesElem.Attribute("BootSeqMinLength").Value);
                TrainingSeqMaxLength = int.Parse(samplesElem.Attribute("TrainingSeqMaxLength").Value);
                TestingSeqLength = int.Parse(samplesElem.Attribute("TestingSeqLength").Value);
                TestSamplesSelection = samplesElem.Attribute("TestSamplesSelection").Value;
                SingleNormalizer = bool.Parse(samplesElem.Attribute("SingleNormalizer").Value);
                NormalizerReserveRatio = double.Parse(samplesElem.Attribute("NormalizerReserveRatio").Value, CultureInfo.InvariantCulture);
                OutputFieldsNames = new List<string>();
                XElement outputElem = demoCaseElem.Descendants("Output").First();
                foreach (XElement outputFieldElem in outputElem.Descendants("Field"))
                {
                    OutputFieldsNames.Add(outputFieldElem.Attribute("Name").Value);
                }
                ESNCfg = new ESNSettings(demoCaseElem.Descendants("ESN").First());
                return;
            }
        }//DemoCaseParams

    }//ESNDemoSettings
}//Namespace
