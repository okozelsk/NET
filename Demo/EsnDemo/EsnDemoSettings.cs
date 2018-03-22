using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;
using System.Globalization;
using OKOSW.Extensions;
using OKOSW.XmlTools;
using OKOSW.Neural;
using OKOSW.Neural.Networks.EchoState;
using System.Reflection;

namespace OKOSW.Demo
{
    public class EsnDemoSettings
    {
        //Constants
        //Attributes
        public string DataDir { get; }
        public List<DemoCaseParams> DemoCases { get; }

        //Constructor
        public EsnDemoSettings(string xmlFile)
        {
            XmlValidator validator = new XmlValidator();
            Assembly esnDemoAssembly = Assembly.GetExecutingAssembly();
            Assembly neuralAssembly = Assembly.Load("Neural");
            validator.AddSchema(esnDemoAssembly.GetManifestResourceStream("OKOSW.Demo.EsnDemoSettings.xsd"));
            validator.AddSchema(neuralAssembly.GetManifestResourceStream("OKOSW.Neural.NeuralSettingsTypes.xsd"));
            XDocument xmlDoc = validator.LoadXDocFromFile(xmlFile);
            XElement root = xmlDoc.Descendants("EsnDemoSettings").First();
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
            public EsnSettings ESNCfg { get; }

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
                ESNCfg = new EsnSettings(demoCaseElem.Descendants("Esn").First());
                return;
            }
        }//DemoCaseParams

    }//ESNDemoSettings
}//Namespace
