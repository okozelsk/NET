using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Xml.Linq;
using System.Reflection;
using System.IO;
using RCNet.Extensions;
using RCNet.Neural.Network.NonRecurrent.FF;
using RCNet.Neural.Network.NonRecurrent.PP;
using RCNet.XmlTools;
using RCNet.MathTools;
using RCNet.Neural.Data.Filter;
using RCNet.Neural.Network.NonRecurrent;
using RCNet.Neural.Activation;

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
        /// The x in the x-fold cross-validation
        /// https://en.wikipedia.org/wiki/Cross-validation_(statistics)
        /// Parameter has two options.
        /// LE 0 - means auto setup to achieve full cross-validation if it is possible (related to specified TestDataRatio)
        /// GT 0 - means exact number of the folds
        /// </summary>
        public int NumOfFolds { get; set; }
        /// <summary>
        /// Defines how many times the generation of whole folds will be repeated
        /// </summary>
        public int Repetitions { get; set; }
        /// <summary>
        /// Collection of networks to be applied when specific networks are not specified.
        /// Relevant for classification tasks.
        /// </summary>
        public List<object> DefaultClassificationNetworkCfgCollection { get; set; }
        /// <summary>
        /// Collection of networks to be applied when specific networks are not specified.
        /// Relevant for forecast tasks.
        /// </summary>
        public List<object> DefaultForecastNetworkCfgCollection { get; set; }
        /// <summary>
        /// Readout unit configurations
        /// </summary>
        public List<ReadoutUnitSettings> ReadoutUnitCfgCollection { get; set; }
        /// <summary>
        /// Dictionary of names of "one winner" groups
        /// </summary>
        public Dictionary<string, OneWinnerGroupSettings> OneWinnerGroupCfgCollection { get; set; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        public ReadoutLayerSettings()
        {
            //Default settings
            TestDataRatio = 0;
            NumOfFolds = 0;
            Repetitions = 0;
            DefaultClassificationNetworkCfgCollection = new List<object>();
            DefaultForecastNetworkCfgCollection = new List<object>();
            ReadoutUnitCfgCollection = new List<ReadoutUnitSettings>();
            OneWinnerGroupCfgCollection = new Dictionary<string, OneWinnerGroupSettings>();
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
            Repetitions = source.Repetitions;
            DefaultClassificationNetworkCfgCollection = new List<object>();
            foreach(object networkCfg in source.DefaultClassificationNetworkCfgCollection)
            {
                DefaultClassificationNetworkCfgCollection.Add(NonRecurrentNetUtils.CloneSettings(networkCfg));
            }
            DefaultForecastNetworkCfgCollection = new List<object>();
            foreach (object networkCfg in source.DefaultForecastNetworkCfgCollection)
            {
                DefaultForecastNetworkCfgCollection.Add(NonRecurrentNetUtils.CloneSettings(networkCfg));
            }
            ReadoutUnitCfgCollection = new List<ReadoutUnitSettings>();
            foreach(ReadoutUnitSettings rus in source.ReadoutUnitCfgCollection)
            {
                ReadoutUnitCfgCollection.Add(rus.DeepClone());
            }
            OneWinnerGroupCfgCollection = new Dictionary<string, OneWinnerGroupSettings>();
            foreach(OneWinnerGroupSettings wwgs in source.OneWinnerGroupCfgCollection.Values)
            {
                OneWinnerGroupCfgCollection.Add(wwgs.Name, wwgs.DeepClone());
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
            Repetitions = int.Parse(readoutLayerSettingsElem.Attribute("repetitions").Value);
            //Default networks settings
            XElement defaultNetworksElem = readoutLayerSettingsElem.Descendants("defaultNetworks").First();
            DefaultClassificationNetworkCfgCollection = NonRecurrentNetUtils.LoadSettingsCollection(defaultNetworksElem.Descendants("classificationNetworksCfg").FirstOrDefault());
            if(DefaultClassificationNetworkCfgCollection.Count == 0)
            {
                DefaultClassificationNetworkCfgCollection.Add(GetSuperDefaultNetworkSettings());
            }
            DefaultForecastNetworkCfgCollection = NonRecurrentNetUtils.LoadSettingsCollection(defaultNetworksElem.Descendants("forecastNetworksCfg").FirstOrDefault());
            if (DefaultForecastNetworkCfgCollection.Count == 0)
            {
                DefaultForecastNetworkCfgCollection.Add(GetSuperDefaultNetworkSettings());
            }
            //Readout units
            XElement readoutUnitsElem = readoutLayerSettingsElem.Descendants("readoutUnits").First();
            ReadoutUnitCfgCollection = new List<ReadoutUnitSettings>();
            int unitIndex = 0;
            List<string> owgs = new List<string>();
            foreach (XElement readoutUnitElem in readoutUnitsElem.Descendants("readoutUnit"))
            {
                ReadoutUnitSettings rus = new ReadoutUnitSettings(unitIndex, readoutUnitElem);
                ReadoutUnitCfgCollection.Add(rus);
                if(rus.OneWinnerGroupName != ReadoutUnitSettings.NoOneWinnerGroupName)
                {
                    if(owgs.IndexOf(rus.OneWinnerGroupName) == -1)
                    {
                        owgs.Add(rus.OneWinnerGroupName);
                    }
                }
                ++unitIndex;
            }
            OneWinnerGroupCfgCollection = new Dictionary<string, OneWinnerGroupSettings>(owgs.Count);
            foreach (string oneWinnerGroupName in owgs)
            {
                OneWinnerGroupCfgCollection.Add(oneWinnerGroupName, new OneWinnerGroupSettings(oneWinnerGroupName, GetOneWinnerGroupMembers(oneWinnerGroupName)));
            }
            return;
        }

        //Properties
        /// <summary>
        /// Collection of names of output fields
        /// </summary>
        public List<string> OutputFieldNameCollection { get { return (from rus in ReadoutUnitCfgCollection select rus.Name).ToList(); } }

        //Methods
        private object GetSuperDefaultNetworkSettings()
        {
            FeedForwardNetworkSettings cfg = new FeedForwardNetworkSettings();
            ElliotSettings elliotCfg = new ElliotSettings();
            IActivationFunction af = ActivationFactory.Create(elliotCfg, new Random(0));
            cfg.OutputLayerActivation = elliotCfg;
            cfg.OutputRange = af.OutputRange;
            cfg.TrainerCfg = new RPropTrainerSettings(3, 1000);
            return cfg;
        }

        /// <summary>
        /// Returns settings of readout units belonging into the specified one-winner group.
        /// </summary>
        /// <param name="groupName">One-winner group name</param>
        private List<ReadoutUnitSettings> GetOneWinnerGroupMembers(string groupName)
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
                Repetitions != cmpSettings.Repetitions ||
                DefaultClassificationNetworkCfgCollection.Count != cmpSettings.DefaultClassificationNetworkCfgCollection.Count ||
                DefaultForecastNetworkCfgCollection.Count != cmpSettings.DefaultForecastNetworkCfgCollection.Count ||
                ReadoutUnitCfgCollection.Count != cmpSettings.ReadoutUnitCfgCollection.Count ||
                OneWinnerGroupCfgCollection.Count != cmpSettings.OneWinnerGroupCfgCollection.Count
                )
            {
                return false;
            }
            for(int i = 0; i < DefaultClassificationNetworkCfgCollection.Count; i++)
            {
                if(!Equals(DefaultClassificationNetworkCfgCollection[i], cmpSettings.DefaultClassificationNetworkCfgCollection[i]))
                {
                    return false;
                }
            }
            for (int i = 0; i < DefaultForecastNetworkCfgCollection.Count; i++)
            {
                if (!Equals(DefaultForecastNetworkCfgCollection[i], cmpSettings.DefaultForecastNetworkCfgCollection[i]))
                {
                    return false;
                }
            }
            for (int i = 0; i < ReadoutUnitCfgCollection.Count; i++)
            {
                if(!Equals(ReadoutUnitCfgCollection[i], cmpSettings.ReadoutUnitCfgCollection[i]))
                {
                    return false;
                }
            }
            foreach(string name in OneWinnerGroupCfgCollection.Keys)
            {
                if(!cmpSettings.OneWinnerGroupCfgCollection.TryGetValue(name, out _))
                {
                    return false;
                }
                if(!Equals(OneWinnerGroupCfgCollection[name], cmpSettings.OneWinnerGroupCfgCollection[name]))
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
            /// Collection of readout unit networks
            /// </summary>
            public List<object> NetCfgCollection { get; set; }

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
                NetCfgCollection = new List<object>();
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
                NetCfgCollection = new List<object>();
                foreach(object netCfg in source.NetCfgCollection)
                {
                    NetCfgCollection.Add(NonRecurrentNetUtils.CloneSettings(netCfg));
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
                //Task
                XElement taskElem = readoutUnitElem.Descendants().First();
                if(taskElem.Name.LocalName == "forecast")
                {
                    TaskType = ReadoutUnit.TaskType.Forecast;
                    FeatureFilterCfg = FeatureFilterFactory.LoadSettings(taskElem.Descendants().First());
                    OneWinnerGroupName = NoOneWinnerGroupName;
                }
                else
                {
                    TaskType = ReadoutUnit.TaskType.Classification;
                    FeatureFilterCfg = new BinFeatureFilterSettings();
                    //One winner group name
                    OneWinnerGroupName = taskElem.Attribute("oneWinnerGroupName").Value;
                    if(OneWinnerGroupName.ToUpper().Trim() == NoOneWinnerGroupName)
                    {
                        OneWinnerGroupName = NoOneWinnerGroupName;
                    }
                }
                //Networks settings
                NetCfgCollection = NonRecurrentNetUtils.LoadSettingsCollection(taskElem.Descendants().FirstOrDefault());
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
                    Index != cmpSettings.Index ||
                    TaskType != cmpSettings.TaskType ||
                    OneWinnerGroupName != cmpSettings.OneWinnerGroupName ||
                    !Equals(FeatureFilterCfg, cmpSettings.FeatureFilterCfg) ||
                    NetCfgCollection.Count != cmpSettings.NetCfgCollection.Count
                    )
                {
                    return false;
                }
                for(int i = 0; i < NetCfgCollection.Count; i++)
                {
                    if(!Equals(NetCfgCollection[i], cmpSettings.NetCfgCollection[i]))
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
            /// Creates the deep copy instance of this instance.
            /// </summary>
            public ReadoutUnitSettings DeepClone()
            {
                return new ReadoutUnitSettings(this);
            }

        }//ReadoutUnitSettings

        /// <summary>
        /// Configuration of "one winner: group
        /// </summary>
        [Serializable]
        public class OneWinnerGroupSettings
        {
            /// <summary>
            /// Name of the group
            /// </summary>
            public string Name { get; }
            /// <summary>
            /// Indexes of member readout units
            /// </summary>
            public List<ReadoutUnitSettings> Members { get; }

            //Constructors
            /// <summary>
            /// Creates initialized instance
            /// </summary>
            /// <param name="name">One winner group name</param>
            /// <param name="members">Member readout units</param>
            public OneWinnerGroupSettings(string name, List<ReadoutUnitSettings> members)
            {
                Name = name;
                Members = members;
                return;
            }

            /// <summary>
            /// Copy constructor
            /// </summary>
            /// <param name="source">Source instance</param>
            public OneWinnerGroupSettings(OneWinnerGroupSettings source)
            {
                Name = source.Name;
                Members = new List<ReadoutUnitSettings>(source.Members.Count);
                foreach(ReadoutUnitSettings rus in source.Members)
                {
                    Members.Add(rus.DeepClone());
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
                OneWinnerGroupSettings cmpSettings = obj as OneWinnerGroupSettings;
                if (Name != cmpSettings.Name ||
                    Members.Count != cmpSettings.Members.Count
                    )
                {
                    return false;
                }
                for(int i = 0; i < cmpSettings.Members.Count; i++)
                {
                    if(!Equals(Members[i], cmpSettings.Members[i]))
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
            /// Creates the deep copy instance of this instance.
            /// </summary>
            public OneWinnerGroupSettings DeepClone()
            {
                return new OneWinnerGroupSettings(this);
            }


        }//OneWinnerGroupSettings

    }//ReadoutLayerSettings

}//Namespace
