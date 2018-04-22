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

namespace RCNet.Neural.Network.SM
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
        /// Readout unit configuration
        /// </summary>
        public ReadoutUnitSettings ReadoutUnitCfg { get; set; }
        /// <summary>
        /// The collection of output field names in order of how they will be computed.
        /// </summary>
        public List<string> OutputFieldNameCollection { get; set; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        public ReadoutLayerSettings()
        {
            //Default settings
            TestDataRatio = 0;
            NumOfFolds = 0;
            ReadoutUnitCfg = new ReadoutUnitSettings();
            OutputFieldNameCollection = new List<string>();
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
            ReadoutUnitCfg = source.ReadoutUnitCfg.DeepClone();
            OutputFieldNameCollection = new List<string>(source.OutputFieldNameCollection);
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
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.SM.ReadoutLayerSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement readoutLayerSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            TestDataRatio = double.Parse(readoutLayerSettingsElem.Attribute("testDataRatio").Value, CultureInfo.InvariantCulture);
            NumOfFolds = readoutLayerSettingsElem.Attribute("folds").Value == "Auto" ? 0 : int.Parse(readoutLayerSettingsElem.Attribute("folds").Value);
            //Readout unit
            XElement readoutUnitElem = readoutLayerSettingsElem.Descendants("readoutUnit").First();
            ReadoutUnitCfg = new ReadoutUnitSettings(readoutUnitElem);
            //Output fields
            XElement outputFieldsElem = readoutLayerSettingsElem.Descendants("outputFields").First();
            OutputFieldNameCollection = new List<string>();
            foreach (XElement outputFieldElem in outputFieldsElem.Descendants("field"))
            {
                OutputFieldNameCollection.Add(outputFieldElem.Attribute("name").Value);
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
                !ReadoutUnitCfg.Equals(cmpSettings.ReadoutUnitCfg) ||
                !OutputFieldNameCollection.ToArray().ContainsEqualValues(cmpSettings.OutputFieldNameCollection.ToArray())
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
            /// Type of readout unit network
            /// </summary>
            public ReadoutUnitNetworkType NetType { get; set; }
            /// <summary>
            /// Settings of readout unit network
            /// </summary>
            public object NetSettings { get; set; }
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
                NetType = ReadoutUnitNetworkType.FF;
                NetSettings = null;
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
                NetType = source.NetType;
                NetSettings = null;
                if (source.NetSettings != null)
                {
                    if (source.NetSettings.GetType() == typeof(FeedForwardNetworkSettings))
                    {
                        NetSettings = ((FeedForwardNetworkSettings)source.NetSettings).DeepClone();
                    }
                    else
                    {
                        NetSettings = ((ParallelPerceptronSettings)source.NetSettings).DeepClone();
                    }
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
                if (netSettingsElem.Name == "ff")
                {
                    NetType = ReadoutUnitNetworkType.FF;
                    NetSettings = new FeedForwardNetworkSettings(netSettingsElem);
                }
                else
                {
                    //PP
                    NetType = ReadoutUnitNetworkType.PP;
                    NetSettings = new ParallelPerceptronSettings(netSettingsElem);
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
                if (NetType != cmpSettings.NetType ||
                    !Equals(NetSettings, cmpSettings.NetSettings) ||
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
