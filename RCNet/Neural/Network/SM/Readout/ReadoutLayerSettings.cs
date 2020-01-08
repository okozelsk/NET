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
using RCNet.Neural.Data.Filter;

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

        //Attribute properties
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
        /// <summary>
        /// Dictionary of names of "one winner" groups
        /// </summary>
        public Dictionary<string, string> OneWinnerGroupNameCollection { get; set; }


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
            OneWinnerGroupNameCollection = new Dictionary<string, string>();
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
            OneWinnerGroupNameCollection = new Dictionary<string, string>(source.OneWinnerGroupNameCollection);
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
            OneWinnerGroupNameCollection = new Dictionary<string, string>();
            int unitIndex = 0;
            foreach (XElement readoutUnitElem in readoutLayerSettingsElem.Descendants("readoutUnit"))
            {
                ReadoutUnitSettings rus = new ReadoutUnitSettings(unitIndex, readoutUnitElem);
                ReadoutUnitCfgCollection.Add(rus);
                if(rus.OneWinnerGroupName != ReadoutUnitSettings.NoOneWinnerGroupName)
                {
                    if(!OneWinnerGroupNameCollection.TryGetValue(rus.OneWinnerGroupName, out string value))
                    {
                        OneWinnerGroupNameCollection.Add(rus.OneWinnerGroupName, rus.OneWinnerGroupName);
                    }
                }
                ++unitIndex;
            }
            return;
        }

        //Properties
        /// <summary>
        /// Collection of names of output fields
        /// </summary>
        public List<string> OutputFieldNameCollection { get { return (from rus in ReadoutUnitCfgCollection select rus.Name).ToList(); } }

        //Methods
        /// <summary>
        /// Returns settings of readout units belonging into the specified one-winner group.
        /// </summary>
        /// <param name="groupName">One-winner group name</param>
        public List<ReadoutUnitSettings> GetOneWinnerGroupMembers(string groupName)
        {
            return (from rus in ReadoutUnitCfgCollection where rus.OneWinnerGroupName == groupName select rus).ToList();
        }

        
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            ReadoutLayerSettings cmpSettings = obj as ReadoutLayerSettings;
            if (TestDataRatio != cmpSettings.TestDataRatio ||
                NumOfFolds != cmpSettings.NumOfFolds ||
                ReadoutUnitCfgCollection.Count != cmpSettings.ReadoutUnitCfgCollection.Count ||
                OneWinnerGroupNameCollection.Count != cmpSettings.OneWinnerGroupNameCollection.Count
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
            foreach(string name in OneWinnerGroupNameCollection.Keys)
            {
                if(!cmpSettings.OneWinnerGroupNameCollection.TryGetValue(name, out string value))
                {
                    return false;
                }
                if(OneWinnerGroupNameCollection[name] != cmpSettings.OneWinnerGroupNameCollection[name])
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
            /// Code indicating no membership in a One-winner group
            /// </summary>
            public const string NoOneWinnerGroupName = "NA";
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
            /// Readout unit zero-based index
            /// </summary>
            public int Index { get; set; }
            /// <summary>
            /// Neural task type
            /// </summary>
            public ReadoutUnit.TaskType TaskType;
            /// <summary>
            /// Specifies membership in the group of possible classes where only one can win.
            /// </summary>
            public readonly string OneWinnerGroupName;
            /// <summary>
            /// Feature filter configuration
            /// </summary>
            public BaseFeatureFilterSettings FeatureFilterCfg;
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

            //Constructors
            /// <summary>
            /// Creates an unitialized instance
            /// </summary>
            public ReadoutUnitSettings()
            {
                Name = string.Empty;
                Index = -1;
                TaskType = ReadoutUnit.TaskType.Forecast;
                OneWinnerGroupName = NoOneWinnerGroupName;
                FeatureFilterCfg = null;
                NetType = ReadoutUnitNetworkType.FF;
                NetSettings = null;
                OutputRange = null;
                return;
            }

            /// <summary>
            /// Copy constructor
            /// </summary>
            /// <param name="source">Source instance</param>
            public ReadoutUnitSettings(ReadoutUnitSettings source)
            {
                Name = source.Name;
                Index = source.Index;
                TaskType = source.TaskType;
                OneWinnerGroupName = source.OneWinnerGroupName;
                FeatureFilterCfg = FeatureFilterFactory.DeepClone(source.FeatureFilterCfg);
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
                return;
            }

            /// <summary>
            /// Creates the instance and initializes it from given xml element.
            /// </summary>
            /// <param name="index">Zero-based index of this readout unit</param>
            /// <param name="readoutUnitElem">Xml data containing the settings.</param>
            public ReadoutUnitSettings(int index, XElement readoutUnitElem)
            {
                //Name
                Name = readoutUnitElem.Attribute("name").Value;
                Index = index;
                //Task and filter
                XElement taskElem = readoutUnitElem.Descendants().First();
                if(taskElem.Name.LocalName == "forecast")
                {
                    TaskType = ReadoutUnit.TaskType.Forecast;
                    OneWinnerGroupName = NoOneWinnerGroupName;
                }
                else
                {
                    TaskType = ReadoutUnit.TaskType.Classification;
                    //One winner group name
                    OneWinnerGroupName = taskElem.Attribute("oneWinnerGroupName").Value;
                    if(OneWinnerGroupName.ToUpper().Trim() == NoOneWinnerGroupName)
                    {
                        OneWinnerGroupName = NoOneWinnerGroupName;
                    }
                }
                //Feature filter
                FeatureFilterCfg = FeatureFilterFactory.LoadSettings(taskElem.Descendants().First());
                //Net settings
                List<XElement> netSettingsElems = new List<XElement>();
                netSettingsElems.AddRange(readoutUnitElem.Descendants("ff"));
                netSettingsElems.AddRange(readoutUnitElem.Descendants("pp"));
                if (netSettingsElems.Count != 1)
                {
                    throw new Exception("Only one network configuration can be specified within readout unit settings.");
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

            //Properties
            /// <summary>
            /// Binary border (for classification purposes only)
            /// </summary>
            public double BinBorder { get { return OutputRange.Mid; } }
            
            //Methods
            /// <summary>
            /// See the base.
            /// </summary>
            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                ReadoutUnitSettings cmpSettings = obj as ReadoutUnitSettings;
                if (Name != cmpSettings.Name ||
                    Index != cmpSettings.Index ||
                    TaskType != cmpSettings.TaskType ||
                    OneWinnerGroupName != cmpSettings.OneWinnerGroupName ||
                    !Equals(FeatureFilterCfg, cmpSettings.FeatureFilterCfg) ||
                    NetType != cmpSettings.NetType ||
                    !Equals(NetSettings, cmpSettings.NetSettings) ||
                    !Equals(OutputRange, cmpSettings.OutputRange)
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
