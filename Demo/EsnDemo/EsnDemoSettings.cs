using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Globalization;
using System.Reflection;
using System.IO;
using RCNet.XmlTools;
using RCNet.Neural.Network.ReservoirComputing.EchoState;

namespace RCNet.Demo
{
    /// <summary>
    /// The class implements Esn demo configuration parameters.
    /// The only way to create an instance is to use the xml constructor.
    /// </summary>
    public class EsnDemoSettings
    {
        //Constants
        //Attribute properties
        /// <summary>
        /// File system directory where the sample data files are stored.
        /// </summary>
        public string DataDir { get; }
        /// <summary>
        /// Collection of demo case definitions.
        /// </summary>
        public List<EsnDemoCaseSettings> DemoCaseParamsCollection { get; }

        //Constructor
        /// <summary>
        /// Creates instance and initialize it from given xml file.
        /// This is the only way to instantiate Esn demo settings.
        /// </summary>
        /// <param name="demoSettingsXmlFile">Xml file containing definitions of demo cases to be prformed</param>
        public EsnDemoSettings(string demoSettingsXmlFile)
        {
            //Validate xml file and load the document 
            XmlValidator validator = new XmlValidator();
            Assembly esnDemoAssembly = Assembly.GetExecutingAssembly();
            Assembly assemblyRCNet = Assembly.Load("RCNet");
            using (Stream schemaStream = esnDemoAssembly.GetManifestResourceStream("RCNet.Demo.EsnDemoSettings.xsd"))
            {
                validator.AddSchema(schemaStream);
            }
            using (Stream schemaStream = assemblyRCNet.GetManifestResourceStream("RCNet.NeuralSettingsTypes.xsd"))
            {
                validator.AddSchema(schemaStream);
            }
            XDocument xmlDoc = validator.LoadXDocFromFile(demoSettingsXmlFile);
            //Parse DataDir
            XElement root = xmlDoc.Descendants("EsnDemoSettings").First();
            DataDir = root.Attribute("DataDir").Value;
            //Parse demo cases definitions
            DemoCaseParamsCollection = new List<EsnDemoCaseSettings>();
            foreach (XElement demoCaseParamsElem in root.Descendants("DemoCase"))
            {
                DemoCaseParamsCollection.Add(new EsnDemoCaseSettings(demoCaseParamsElem, DataDir));
            }

            return;
        }

        //Inner classes
        /// <summary>
        /// Holds the configuration of the single Esn demo case.
        /// </summary>
        public class EsnDemoCaseSettings
        {
            //Constants
            //Attribute properties
            /// <summary>
            /// Demo case descriptive name
            /// </summary>
            public string Name { get; }
            /// <summary>
            /// File name of the csv file containing the time series data to be used.
            /// </summary>
            public string CsvDataFileName { get; }
            /// <summary>
            /// How many of starting samples will be used for booting of reservoirs to ensure
            /// reservoir neurons states be affected by input data only. Boot sequence length of the reservoir should be greater
            /// or equal to reservoir neurons count. So use the boot sequence length of the largest defined reservoir.
            /// </summary>
            public int NumOfBootSamples { get; }
            /// <summary>
            /// Use true if all input and output Esn fields are about the same range of values.
            /// </summary>
            public bool SingleNormalizer { get; }
            /// <summary>
            /// Reserve held by a normalizer to cover cases where future data exceeds a known range of sample data.
            /// </summary>
            public double NormalizerReserveRatio { get; }
            /// <summary>
            /// 
            /// </summary>
            public EsnSettings EsnConfiguration { get; }

            //Constructor
            public EsnDemoCaseSettings(XElement demoCaseElem, string dir)
            {
                //Simple parsing of demo case parameters
                Name = demoCaseElem.Attribute("Name").Value;
                XElement samplesElem = demoCaseElem.Descendants("Samples").First();
                CsvDataFileName = dir + "\\" + samplesElem.Attribute("CsvDataFileName").Value;
                NumOfBootSamples = (samplesElem.Attribute("NumOfBootSamples") == null) ? 0 : int.Parse(samplesElem.Attribute("NumOfBootSamples").Value);
                SingleNormalizer = bool.Parse(samplesElem.Attribute("SingleNormalizer").Value);
                NormalizerReserveRatio = double.Parse(samplesElem.Attribute("NormalizerReserveRatio").Value, CultureInfo.InvariantCulture);
                //Instantiating of the EsnSettings
                EsnConfiguration = new EsnSettings(demoCaseElem.Descendants("EsnSettings").First());
                return;
            }

        }//EsnDemoCaseSettings

    }//ESNDemoSettings

}//Namespace
