using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Xml.Linq;
using System.Reflection;
using System.IO;
using RCNet.Extensions;
using RCNet.Neural.Network.FF;
using RCNet.Neural.Network.PP;
using RCNet.XmlTools;
using RCNet.MathTools;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// The class contains readout layer configuration parameters.
    /// The easiest and safest way to create an instance is to use the xml constructor.
    /// </summary>
    [Serializable]
    public class ReadoutLayerSettings
    {
        //Constants

        //Attributes
        /// <summary>
        /// Parameter specifies how big part of available samples will be used for testing.
        /// </summary>
        public double TestDataRatio { get; set; }
        /// <summary>
        /// Number of predicting readout units for each output field.
        /// It also detemines how many data sets for testing will be prepared.
        /// (x-fold cross-validation)
        /// https://en.wikipedia.org/wiki/Cross-validation_(statistics)
        /// Parameter has two options.
        /// LE 0 - means auto setup to achieve full cross-validation if it is possible (related to specified TestDataRatio)
        /// GT 0 - means exact number of the folds
        /// </summary>
        public int NumOfFolds { get; set; }
        /// <summary>
        /// Readout unit configurations
        /// </summary>
        public List<ReadoutUnitSettings> ReadoutUnitCfgCollection { get; set; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        public ReadoutLayerSettings()
        {
            //Default settings
            TestDataRatio = 0;
            NumOfFolds = 0;
            ReadoutUnitCfgCollection = new List<ReadoutUnitSettings>();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ReadoutLayerSettings(ReadoutLayerSettings source)
        {
            //Copy
            TestDataRatio = source.TestDataRatio;
            NumOfFolds = source.NumOfFolds;
            ReadoutUnitCfgCollection = new List<ReadoutUnitSettings>();
            foreach(ReadoutUnitSettings rus in source.ReadoutUnitCfgCollection)
            {
                ReadoutUnitCfgCollection.Add(rus.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// This is the preferred way to instantiate ReadoutLayer settings.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing ReadoutLayer settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public ReadoutLayerSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.SM.Readout.ReadoutLayerSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement readoutLayerSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            TestDataRatio = double.Parse(readoutLayerSettingsElem.Attribute("testDataRatio").Value, CultureInfo.InvariantCulture);
            NumOfFolds = readoutLayerSettingsElem.Attribute("folds").Value == "Auto" ? 0 : int.Parse(readoutLayerSettingsElem.Attribute("folds").Value);
            //Readout units
            ReadoutUnitCfgCollection = new List<ReadoutUnitSettings>();
            foreach (XElement readoutUnitElem in readoutLayerSettingsElem.Descendants("readoutUnit"))
            {
                ReadoutUnitCfgCollection.Add(new ReadoutUnitSettings(readoutUnitElem));
            }
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            ReadoutLayerSettings cmpSettings = obj as ReadoutLayerSettings;
            if (TestDataRatio != cmpSettings.TestDataRatio ||
                NumOfFolds != cmpSettings.NumOfFolds ||
                ReadoutUnitCfgCollection.Count != cmpSettings.ReadoutUnitCfgCollection.Count
                )
            {
                return false;
            }
            for(int i = 0; i < ReadoutUnitCfgCollection.Count; i++)
            {
                if(!ReadoutUnitCfgCollection[i].Equals(cmpSettings.ReadoutUnitCfgCollection[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// See the base.
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public ReadoutLayerSettings DeepClone()
        {
            ReadoutLayerSettings clone = new ReadoutLayerSettings(this);
            return clone;
        }

        //Inner classes
        /// <summary>
        /// Readout unit settings
        /// </summary>
        [Serializable]
        public class ReadoutUnitSettings
        {
            //Constants
            /// <summary>
            /// Supported types of readout unit networks
            /// </summary>
            public enum ReadoutUnitNetworkType
            {
                /// <summary>
                /// Readout unit with feed forward network
                /// </summary>
                FF,
                /// <summary>
                /// Readout unit with parallel perceptron
                /// </summary>
                PP
            }//ReadoutUnitNetworkType

            //Attributes
            /// <summary>
            /// Output field name
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// Neural task type
            /// </summary>
            public CommonEnums.TaskType TaskType;
            /// <summary>
            /// Type of readout unit network
            /// </summary>
            public ReadoutUnitNetworkType NetType { get; set; }
            /// <summary>
            /// Settings of readout unit network
            /// </summary>
            public object NetSettings { get; set; }
            /// <summary>
            /// Unit's output values range.
            /// </summary>
            public Interval OutputRange { get; }
            /// <summary>
            /// Number of regression attempts.
            /// </summary>
            public int RegressionAttempts { get; set; }
            /// <summary>
            /// Number of iterations (epochs) during regression attempt.
            /// </summary>
            public int RegressionAttemptEpochs { get; set; }

            //Constructors
            /// <summary>
            /// Creates an unitialized instance
            /// </summary>
            public ReadoutUnitSettings()
            {
                Name = "";
                TaskType = CommonEnums.TaskType.Forecast;
                NetType = ReadoutUnitNetworkType.FF;
                NetSettings = null;
                OutputRange = null;
                RegressionAttempts = 0;
                RegressionAttemptEpochs = 0;
                return;
            }

            /// <summary>
            /// Copy constructor
            /// </summary>
            /// <param name="source">Source instance</param>
            public ReadoutUnitSettings(ReadoutUnitSettings source)
            {
                Name = source.Name;
                TaskType = source.TaskType;
                NetType = source.NetType;
                NetSettings = null;
                OutputRange = null;
                if (source.NetSettings != null)
                {
                    if (source.NetSettings.GetType() == typeof(FeedForwardNetworkSettings))
                    {
                        NetSettings = ((FeedForwardNetworkSettings)(source.NetSettings)).DeepClone();
                    }
                    else
                    {
                        NetSettings = ((ParallelPerceptronSettings)(source.NetSettings)).DeepClone();
                    }
                    OutputRange = source.OutputRange.DeepClone();
                }
                RegressionAttempts = source.RegressionAttempts;
                RegressionAttemptEpochs = source.RegressionAttemptEpochs;
                return;
            }

            /// <summary>
            /// Creates the instance and initializes it from given xml element.
            /// </summary>
            /// <param name="readoutUnitElem">
            /// Xml data containing the settings.
            /// </param>
            public ReadoutUnitSettings(XElement readoutUnitElem)
            {
                Name = readoutUnitElem.Attribute("name").Value;
                TaskType = CommonEnums.ParseTaskType(readoutUnitElem.Attribute("task").Value);
                RegressionAttempts = int.Parse(readoutUnitElem.Attribute("attempts").Value);
                RegressionAttemptEpochs = int.Parse(readoutUnitElem.Attribute("attemptEpochs").Value);
                //Net settings
                List<XElement> netSettingsElems = new List<XElement>();
                netSettingsElems.AddRange(readoutUnitElem.Descendants("ff"));
                netSettingsElems.AddRange(readoutUnitElem.Descendants("pp"));
                if (netSettingsElems.Count != 1)
                {
                    throw new Exception("Only one network configuration can be specified in readout unit settings.");
                }
                if (netSettingsElems.Count == 0)
                {
                    throw new Exception("Network configuration is not specified in readout unit settings.");
                }
                XElement netSettingsElem = netSettingsElems[0];
                //FF?
                if (netSettingsElem.Name.LocalName == "ff")
                {
                    NetType = ReadoutUnitNetworkType.FF;
                    NetSettings = new FeedForwardNetworkSettings(netSettingsElem);
                    OutputRange = ((FeedForwardNetworkSettings)NetSettings).OutputRange.DeepClone();
                }
                else
                {
                    //PP
                    NetType = ReadoutUnitNetworkType.PP;
                    NetSettings = new ParallelPerceptronSettings(netSettingsElem);
                    OutputRange = ((ParallelPerceptronSettings)NetSettings).OutputRange.DeepClone();
                }
                return;
            }

            //Methods
            /// <summary>
            /// See the base.
            /// </summary>
            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                ReadoutUnitSettings cmpSettings = obj as ReadoutUnitSettings;
                if (Name != cmpSettings.Name ||
                    TaskType != cmpSettings.TaskType ||
                    NetType != cmpSettings.NetType ||
                    !Equals(NetSettings, cmpSettings.NetSettings) ||
                    !Equals(OutputRange, cmpSettings.OutputRange) ||
                    RegressionAttempts != cmpSettings.RegressionAttempts ||
                    RegressionAttemptEpochs != cmpSettings.RegressionAttemptEpochs
                    )
                {
                    return false;
                }
                return true;
            }

            /// <summary>
            /// See the base.
            /// </summary>
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            /// <summary>
            /// Creates the deep copy instance of this instance.
            /// </summary>
            public ReadoutUnitSettings DeepClone()
            {
                return new ReadoutUnitSettings(this);
            }

        }//ReadoutUnitSettings

    }//ReadoutLayerSettings

}//Namespace
