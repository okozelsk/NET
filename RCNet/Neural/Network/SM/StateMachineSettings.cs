using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Xml.Linq;
using System.Reflection;
using RCNet.Extensions;
using RCNet.XmlTools;
using RCNet.RandomValue;
using RCNet.Neural.Data.Generators;
using RCNet.Neural.Network.SM.Synapse;
using RCNet.Neural.Network.SM.Preprocessing;
using RCNet.Neural.Network.SM.Readout;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// The class contains State Machine configuration.
    /// The easiest and safest way to create an instance is to use the xml constructor.
    /// </summary>
    [Serializable]
    public class StateMachineSettings
    {
        //Attribute properties
        /// <summary>
        /// A value greater than or equal to 0 will always ensure the same initialization of the internal
        /// random number generator and therefore the same network structure, which is good for tuning
        /// other parameters.
        /// A value less than 0 causes a fully random initialization when creating a network instance.
        /// </summary>
        public int RandomizerSeek { get; set; }
        /// <summary>
        /// Settings of Neural Preprocessor
        /// </summary>
        public NeuralPreprocessorSettings NeuralPreprocessorConfig { get; set; }
        /// <summary>
        /// Configuration of the readout layer
        /// </summary>
        public ReadoutLayerSettings ReadoutLayerConfig { get; set; }
        /// <summary>
        /// Configuration of mapper of predictors to readout units
        /// </summary>
        public MapperSettings MapperConfig { get; set; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        public StateMachineSettings()
        {
            //Default settings
            RandomizerSeek = 0;
            NeuralPreprocessorConfig = new NeuralPreprocessorSettings();
            ReadoutLayerConfig = new ReadoutLayerSettings();
            MapperConfig = null;
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public StateMachineSettings(StateMachineSettings source)
        {
            //Copy
            RandomizerSeek = source.RandomizerSeek;
            NeuralPreprocessorConfig = source.NeuralPreprocessorConfig.DeepClone();
            ReadoutLayerConfig = new ReadoutLayerSettings(source.ReadoutLayerConfig);
            MapperConfig = null;
            if(source.MapperConfig != null)
            {
                MapperConfig = source.MapperConfig.DeepClone();
            }
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// This is the preferred way to instantiate State Machine settings.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing State Machine settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public StateMachineSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.SM.StateMachineSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement stateMachineSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            //Randomizer seek
            RandomizerSeek = int.Parse(stateMachineSettingsElem.Attribute("randomizerSeek").Value);
            //Neural preprocessor
            NeuralPreprocessorConfig = new NeuralPreprocessorSettings(stateMachineSettingsElem.Descendants("neuralPreprocessor").First());
            //Readout layer
            ReadoutLayerConfig = new ReadoutLayerSettings(stateMachineSettingsElem.Descendants("readoutLayer").First());
            //Mapper
            XElement mapperSettingsElem = stateMachineSettingsElem.Descendants("mapper").FirstOrDefault();
            if(mapperSettingsElem != null)
            {
                //Create mapper object
                MapperConfig = new MapperSettings();
                //Loop through mappings
                foreach(XElement mapElem in mapperSettingsElem.Descendants("map"))
                {
                    //Readout unit name
                    string readoutUnitName = mapElem.Attribute("readoutUnitName").Value;
                    int readoutUnitIdx = -1;
                    for (int i = 0; i < ReadoutLayerConfig.ReadoutUnitCfgCollection.Count; i++)
                    {
                        if(ReadoutLayerConfig.ReadoutUnitCfgCollection[i].Name == readoutUnitName)
                        {
                            readoutUnitIdx = i;
                            break;
                        }
                        else if(i == ReadoutLayerConfig.ReadoutUnitCfgCollection.Count - 1)
                        {
                            throw new Exception($"Name {readoutUnitName} not found among readout units.");
                        }
                    }
                    //Allowed pools
                    List<MapperSettings.PoolRef> allowedPools = new List<MapperSettings.PoolRef>();
                    foreach(XElement allowedPoolElem in mapElem.Descendants("allowed"))
                    {
                        //Reservoir instance
                        string reservoirInstanceName = allowedPoolElem.Attribute("reservoirInstanceName").Value;
                        int reservoirInstanceIdx = -1;
                        for(int i = 0; i < NeuralPreprocessorConfig.ReservoirInstanceDefinitionCollection.Count; i++)
                        {
                            if(NeuralPreprocessorConfig.ReservoirInstanceDefinitionCollection[i].InstanceName == reservoirInstanceName)
                            {
                                reservoirInstanceIdx = i;
                                break;
                            }
                            else if(i == NeuralPreprocessorConfig.ReservoirInstanceDefinitionCollection.Count - 1)
                            {
                                throw new Exception($"Name {reservoirInstanceName} not found among resevoir instances.");
                            }
                        }
                        //Pool
                        string reservoirCfgName = NeuralPreprocessorConfig.ReservoirInstanceDefinitionCollection[reservoirInstanceIdx].Settings.SettingsName;
                        ReservoirSettings reservoirSettings = NeuralPreprocessorConfig.ReservoirInstanceDefinitionCollection[reservoirInstanceIdx].Settings;
                        string poolName = allowedPoolElem.Attribute("poolName").Value;
                        int poolIdx = -1;
                        for (int i = 0; i < reservoirSettings.PoolSettingsCollection.Count; i++)
                        {
                            if(reservoirSettings.PoolSettingsCollection[i].Name == poolName)
                            {
                                poolIdx = i;
                                break;
                            }
                            else if(i == reservoirSettings.PoolSettingsCollection.Count - 1)
                            {
                                throw new Exception($"Name {poolName} not found among resevoir's pools.");
                            }
                        }
                        allowedPools.Add(new MapperSettings.PoolRef { _reservoirInstanceIdx = reservoirInstanceIdx, _poolIdx = poolIdx });
                    }
                    MapperConfig.Map.Add(readoutUnitName, allowedPools);
                }
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
            StateMachineSettings cmpSettings = obj as StateMachineSettings;
            if (RandomizerSeek != cmpSettings.RandomizerSeek ||
                !Equals(NeuralPreprocessorConfig, cmpSettings.NeuralPreprocessorConfig) ||
                !Equals(ReadoutLayerConfig, cmpSettings.ReadoutLayerConfig) ||
                (MapperConfig == null && cmpSettings.MapperConfig != null) ||
                (MapperConfig != null && cmpSettings.MapperConfig == null) ||
                (MapperConfig != null && !Equals(MapperConfig, cmpSettings.MapperConfig))
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
        public StateMachineSettings DeepClone()
        {
            StateMachineSettings clone = new StateMachineSettings(this);
            return clone;
        }

        //Inner classes
        /// <summary>
        /// Configuration of mapper of predictors to readout units
        /// </summary>
        [Serializable]
        public class MapperSettings
        {
            /// <summary>
            /// Mapping of readout unit and allowed predictors pools
            /// </summary>
            public Dictionary<string, List<PoolRef>> Map { get; }

            /// <summary>
            /// Creates an unitialized instance
            /// </summary>
            public MapperSettings()
            {
                Map = new Dictionary<string, List<PoolRef>>();
                return;
            }

            //Methods
            /// <summary>
            /// See the base.
            /// </summary>
            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                MapperSettings cmpSettings = obj as MapperSettings;
                if (Map.Count != cmpSettings.Map.Count)
                {
                    return false;
                }
                foreach(string key in Map.Keys)
                {
                    List<PoolRef> myAllowedPools = Map[key];
                    List<PoolRef> cmpAllowedPools = null;
                    try
                    {
                        cmpAllowedPools = cmpSettings.Map[key];
                    }
                    catch
                    {
                        return false;
                    }
                    if(myAllowedPools.Count != cmpAllowedPools.Count)
                    {
                        return false;
                    }
                    foreach(PoolRef ap in myAllowedPools)
                    {
                        for(int i = 0; i < cmpAllowedPools.Count; i++)
                        {
                            if(cmpAllowedPools[i]._reservoirInstanceIdx == ap._reservoirInstanceIdx && cmpAllowedPools[i]._poolIdx == ap._poolIdx)
                            {
                                break;
                            }
                            else if(i == cmpAllowedPools.Count - 1)
                            {
                                return false;
                            }
                        }
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
            public MapperSettings DeepClone()
            {
                MapperSettings clonnedMapper = new MapperSettings();
                foreach (KeyValuePair<string, List<PoolRef>> keyValuePair in Map)
                {
                    List<PoolRef> clonnedList = new List<PoolRef>(keyValuePair.Value.Count);
                    foreach (PoolRef ap in keyValuePair.Value)
                    {
                        PoolRef clonnedAP = new PoolRef { _reservoirInstanceIdx = ap._reservoirInstanceIdx, _poolIdx = ap._poolIdx };
                        clonnedList.Add(clonnedAP);
                    }
                    clonnedMapper.Map.Add(keyValuePair.Key, clonnedList);
                }
                return clonnedMapper;
            }

            //Inner classes
            /// <summary>
            /// Identification of the pool from which are allowed predictors
            /// </summary>
            [Serializable]
            public class PoolRef
            {
                /// <summary>
                /// Index of the reservoir instance
                /// </summary>
                public int _reservoirInstanceIdx;
                /// <summary>
                /// Index of the pool within the reservoir instance
                /// </summary>
                public int _poolIdx;

                //Methods
                /// <summary>
                /// See the base.
                /// </summary>
                public override bool Equals(object obj)
                {
                    if (obj == null) return false;
                    PoolRef cmpSettings = obj as PoolRef;
                    if (_reservoirInstanceIdx != cmpSettings._reservoirInstanceIdx ||
                        _poolIdx != cmpSettings._poolIdx
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

            }//AllowedPool

        }//MapperSettings

    }//StateMachineSettings

}//Namespace
