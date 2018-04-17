using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Globalization;
using System.Reflection;
using System.IO;
using RCNet.XmlTools;
using RCNet.Neural.Analog.Network.EchoState;

namespace RCNet.Demo
{
    /// <summary>
    /// The class implements Esn demo configuration parameters.
    /// One and only way to create an instance is to use the xml constructor.
    /// </summary>
    public class EsnDemoSettings
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
        /// This is the only way to instantiate Esn demo settings.
        /// </summary>
        /// <param name="fileName">Xml file containing definitions of demo cases to be prformed</param>
        public EsnDemoSettings(string fileName)
        {
            //Validate xml file and load the document 
            DocValidator validator = new DocValidator();
            Assembly esnDemoAssembly = Assembly.GetExecutingAssembly();
            Assembly assemblyRCNet = Assembly.Load("RCNet");
            using (Stream schemaStream = esnDemoAssembly.GetManifestResourceStream("RCNet.Demo.EsnDemoSettings.xsd"))
            {
                validator.AddSchema(schemaStream);
            }
            using (Stream schemaStream = assemblyRCNet.GetManifestResourceStream("RCNet.RCNetTypes.xsd"))
            {
                validator.AddSchema(schemaStream);
            }
            XDocument xmlDoc = validator.LoadXDocFromFile(fileName);
            //Parse DataDir
            XElement root = xmlDoc.Descendants("esnDemo").First();
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
        /// Holds the configuration of the single Esn demo case.
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
            public EsnSettings EsnCfg { get; }

            //Constructor
            public CaseSettings(XElement demoCaseElem, string dir)
            {
                Name = demoCaseElem.Attribute("name").Value;
                XElement samplesElem = demoCaseElem.Descendants("samples").First();
                FileName = dir + "\\" + samplesElem.Attribute("fileName").Value;
                NumOfBootSamples = (samplesElem.Attribute("bootSamples") == null) ? 0 : int.Parse(samplesElem.Attribute("bootSamples").Value);
                SingleNormalizer = bool.Parse(samplesElem.Attribute("singleNormalizer").Value);
                NormalizerReserveRatio = double.Parse(samplesElem.Attribute("normalizerReserve").Value, CultureInfo.InvariantCulture);
                EsnCfg = new EsnSettings(demoCaseElem.Descendants("esnCfg").First());
                return;
            }

        }//CaseSettings

    }//EsnDemoSettings

}//Namespace
