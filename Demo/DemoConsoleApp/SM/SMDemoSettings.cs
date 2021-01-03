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
    /// Configuration of the StateMachine demo cases.
    /// </summary>
    public class SMDemoSettings
    {
        //Constants
        //Attribute properties
        /// <summary>
        /// The data location
        /// </summary>
        public string DataFolder { get; }
        /// <summary>
        /// The collection of the demo case configurations.
        /// </summary>
        public List<CaseSettings> CaseCfgCollection { get; }

        //Constructor
        /// <summary>
        /// Creates initialized instance from the specified xml file.
        /// </summary>
        /// <param name="fileName">The name of the xml file consisting of demo cases configurations.</param>
        public SMDemoSettings(string fileName)
        {
            //Validate xml file and load the document
            DocValidator validator = new DocValidator();
            //Add RCNetTypes.xsd
            validator.AddSchema(RCNetBaseSettings.LoadRCNetTypesSchema());
            //Add SMDemoSettings.xsd
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream schemaStream = assembly.GetManifestResourceStream("DemoConsoleApp.SMDemoSettings.xsd"))
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
        /// Implements the configuration of the single demo case.
        /// </summary>
        public class CaseSettings
        {
            //Constants
            //Attribute properties
            /// <summary>
            /// The name of the demo case.
            /// </summary>
            public string Name { get; }
            /// <summary>
            /// The name of the file containing the training data.
            /// </summary>
            public string TrainingDataFileName { get; }
            /// <summary>
            /// The name of the file containing the verification data.
            /// </summary>
            public string VerificationDataFileName { get; }
            /// <summary>
            /// The configuration of the State Machine.
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
                //Training data file (full path)
                TrainingDataFileName = Path.Combine(dir, samplesElem.Attribute("trainingData").Value);
                //Verification data file
                if (samplesElem.Attribute("verificationData").Value.Trim().Length > 0)
                {
                    //Full path
                    VerificationDataFileName = Path.Combine(dir, samplesElem.Attribute("verificationData").Value);
                }
                else
                {
                    //Empty - no verification data specified
                    VerificationDataFileName = string.Empty;
                }
                //State Machine configuration
                StateMachineCfg = new StateMachineSettings(demoCaseElem.Elements("stateMachine").First());
                return;
            }

        }//CaseSettings

    }//DemoSettings

}//Namespace
