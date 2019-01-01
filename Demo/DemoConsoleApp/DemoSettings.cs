using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Globalization;
using System.Reflection;
using System.IO;
using RCNet.XmlTools;
using RCNet.Neural;
using RCNet.Neural.Network.SM;
using RCNet.MathTools;

namespace RCNet.DemoConsoleApp
{
    /// <summary>
    /// The class implements State Machine demo configuration parameters.
    /// One and only way to create an instance is to use the xml constructor.
    /// </summary>
    public class DemoSettings
    {
        //Constants
        //Attribute properties
        /// <summary>
        /// File system directory where the sample data files are stored.
        /// </summary>
        public string DataFolder { get; }
        /// <summary>
        /// Collection of demo case definitions.
        /// </summary>
        public List<CaseSettings> CaseCfgCollection { get; }

        //Constructor
        /// <summary>
        /// Creates instance and initialize it from given xml file.
        /// This is the only way to instantiate State Machine demo settings.
        /// </summary>
        /// <param name="fileName">Xml file containing definitions of demo cases to be prformed</param>
        public DemoSettings(string fileName)
        {
            //Validate xml file and load the document 
            DocValidator validator = new DocValidator();
            Assembly demoAssembly = Assembly.GetExecutingAssembly();
            Assembly assemblyRCNet = Assembly.Load("RCNet");
            using (Stream schemaStream = demoAssembly.GetManifestResourceStream("RCNet.DemoConsoleApp.DemoSettings.xsd"))
            {
                validator.AddSchema(schemaStream);
            }
            using (Stream schemaStream = assemblyRCNet.GetManifestResourceStream("RCNet.RCNetTypes.xsd"))
            {
                validator.AddSchema(schemaStream);
            }
            XDocument xmlDoc = validator.LoadXDocFromFile(fileName);
            //Parse DataDir
            XElement root = xmlDoc.Descendants("demo").First();
            DataFolder = root.Attribute("dataFolder").Value;
            //Parse demo cases definitions
            CaseCfgCollection = new List<CaseSettings>();
            foreach (XElement demoCaseParamsElem in root.Descendants("case"))
            {
                CaseCfgCollection.Add(new CaseSettings(demoCaseParamsElem, DataFolder));
            }

            return;
        }

        //Inner classes
        /// <summary>
        /// Holds the configuration of the single State Machine demo case.
        /// </summary>
        public class CaseSettings
        {
            //Constants
            //Attribute properties
            /// <summary>
            /// Demo case descriptive name
            /// </summary>
            public string Name { get; }
            /// <summary>
            /// Demo case data file (appropriate csv format)
            /// </summary>
            public string FileName { get; }
            /// <summary>
            /// Use true if all input and output State Machine fields are about the same range of values.
            /// </summary>
            public bool SingleNormalizer { get; }
            /// <summary>
            /// Reserve held by a normalizer to cover cases where future data exceeds a known range of sample data.
            /// </summary>
            public double NormalizerReserveRatio { get; }
            /// <summary>
            /// State machine configuration
            /// </summary>
            public StateMachineSettings stateMachineCfg { get; }

            //Constructor
            public CaseSettings(XElement demoCaseElem, string dir)
            {
                Name = demoCaseElem.Attribute("name").Value;
                XElement samplesElem = demoCaseElem.Descendants("samples").First();
                FileName = dir + "\\" + samplesElem.Attribute("fileName").Value;
                SingleNormalizer = bool.Parse(samplesElem.Attribute("singleNormalizer").Value);
                NormalizerReserveRatio = double.Parse(samplesElem.Attribute("normalizerReserve").Value, CultureInfo.InvariantCulture);
                stateMachineCfg = new StateMachineSettings(demoCaseElem.Descendants("stateMachineCfg").First());
                return;
            }

        }//CaseSettings

    }//DemoSettings

}//Namespace
