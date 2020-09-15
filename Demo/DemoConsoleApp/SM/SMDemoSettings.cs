using RCNet;
using RCNet.Neural.Network.SM;
using RCNet.XmlTools;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Demo.DemoConsoleApp.SM
{
    /// <summary>
    /// Encapsulates StateMachine demo cases configurations.
    /// One and only way to create an instance is to use the xml constructor.
    /// </summary>
    public class SMDemoSettings
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
        /// This is the only way to instantiate StateMachine demo settings.
        /// </summary>
        /// <param name="fileName">Xml file consisting of demo cases definitions</param>
        public SMDemoSettings(string fileName)
        {
            //Validate xml file and load the document
            DocValidator validator = new DocValidator();
            //Add RCNetTypes.xsd
            validator.AddSchema(RCNetBaseSettings.LoadRCNetTypesSchema());
            //Add SMDemoSettings.xsd
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream schemaStream = assembly.GetManifestResourceStream("Demo.DemoConsoleApp.SM.SMDemoSettings.xsd"))
            {
                validator.AddSchema(schemaStream);
            }
            //Load the xml
            XDocument xmlDoc = validator.LoadXDocFromFile(fileName);
            //Parsing
            //Data folder
            XElement root = xmlDoc.Elements("demo").First();
            DataFolder = root.Attribute("dataFolder").Value;
            //Demo cases definitions
            CaseCfgCollection = new List<CaseSettings>();
            foreach (XElement demoCaseParamsElem in root.Elements("case"))
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
            /// Demo case training data file (appropriate csv format)
            /// </summary>
            public string TrainingDataFileName { get; }
            /// <summary>
            /// Demo case verification data file (appropriate csv format)
            /// </summary>
            public string VerificationDataFileName { get; }
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
                XElement samplesElem = demoCaseElem.Elements("samples").First();
                //Full path to training csv file
                TrainingDataFileName = Path.Combine(dir, samplesElem.Attribute("trainingData").Value);
                //Verification data file
                if (samplesElem.Attribute("verificationData").Value.Trim().Length > 0)
                {
                    //Full path to verification csv file
                    VerificationDataFileName = Path.Combine(dir, samplesElem.Attribute("verificationData").Value);
                }
                else
                {
                    //Empty - no verification data
                    VerificationDataFileName = string.Empty;
                }
                //State Machine configuration
                StateMachineCfg = new StateMachineSettings(demoCaseElem.Elements("stateMachine").First());
                return;
            }

        }//CaseSettings

    }//DemoSettings

}//Namespace
