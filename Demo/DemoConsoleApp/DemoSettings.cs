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
    /// Encapsulates demo cases configurations.
    /// One and only way to create an instance is to use the xml constructor.
    /// </summary>
    public class DemoSettings
    {
        //Constants
        //Attribute properties
        /// <summary>
        /// Location where the csv sample data files are stored.
        /// </summary>
        public string DataFolder { get; }
        /// <summary>
        /// Collection of demo case definitions.
        /// </summary>
        public List<CaseSettings> CaseCfgCollection { get; }

        //Constructor
        /// <summary>
        /// Creates initialized instance based on given xml file.
        /// This is the only way to instantiate demo settings.
        /// </summary>
        /// <param name="fileName">Xml file consisting of demo cases definitions</param>
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
            //Parsing
            //Data folder
            XElement root = xmlDoc.Descendants("demo").First();
            DataFolder = root.Attribute("dataFolder").Value;
            //Demo cases definitions
            CaseCfgCollection = new List<CaseSettings>();
            foreach (XElement demoCaseParamsElem in root.Descendants("case"))
            {
                CaseCfgCollection.Add(new CaseSettings(demoCaseParamsElem, DataFolder));
            }
            return;
        }

        //Inner classes
        /// <summary>
        /// Holds the configuration of the single demo case.
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
            /// State machine configuration
            /// </summary>
            public StateMachineSettings StateMachineCfg { get; }

            //Constructor
            public CaseSettings(XElement demoCaseElem, string dir)
            {
                //Parsing
                //Demo case name
                Name = demoCaseElem.Attribute("name").Value;
                //Samples
                XElement samplesElem = demoCaseElem.Descendants("samples").First();
                //Full path to csv file
                FileName = dir + "\\" + samplesElem.Attribute("fileName").Value;
                //State Machine configuration
                StateMachineCfg = new StateMachineSettings(demoCaseElem.Descendants("stateMachineCfg").First());
                return;
            }

        }//CaseSettings

    }//DemoSettings

}//Namespace
