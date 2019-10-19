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
using RCNet.Neural.Network.SM.Neuron;
using RCNet.Neural.Network.SM.Preprocessing;
using RCNet.Neural.Network.SM.Readout;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// State Machine configuration.
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
        public MapperSettings MapperCfg { get; set; }

        //Constructors
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
            MapperCfg = null;
            if(source.MapperCfg != null)
            {
                MapperCfg = source.MapperCfg.DeepClone();
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
                MapperCfg = new MapperSettings();
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
                    List<MapperSettings.AllowedPool> allowedPools = new List<MapperSettings.AllowedPool>();
                    XElement allowedPoolsElem = mapElem.Descendants("allowedPools").FirstOrDefault();
                    if (allowedPoolsElem != null)
                    {
                        foreach (XElement allowedPoolElem in allowedPoolsElem.Descendants("pool"))
                        {
                            //Reservoir instance name
                            string reservoirInstanceName = allowedPoolElem.Attribute("reservoirInstanceName").Value;
                            int reservoirInstanceIdx = -1;
                            for (int i = 0; i < NeuralPreprocessorConfig.ReservoirInstanceDefinitionCollection.Count; i++)
                            {
                                if (NeuralPreprocessorConfig.ReservoirInstanceDefinitionCollection[i].InstanceName == reservoirInstanceName)
                                {
                                    reservoirInstanceIdx = i;
                                    break;
                                }
                                else if (i == NeuralPreprocessorConfig.ReservoirInstanceDefinitionCollection.Count - 1)
                                {
                                    throw new Exception($"Name {reservoirInstanceName} not found among resevoir instances.");
                                }
                            }
                            //Pool name
                            string reservoirCfgName = NeuralPreprocessorConfig.ReservoirInstanceDefinitionCollection[reservoirInstanceIdx].Settings.SettingsName;
                            ReservoirSettings reservoirSettings = NeuralPreprocessorConfig.ReservoirInstanceDefinitionCollection[reservoirInstanceIdx].Settings;
                            string poolName = allowedPoolElem.Attribute("poolName").Value;
                            int poolIdx = -1;
                            for (int i = 0; i < reservoirSettings.PoolSettingsCollection.Count; i++)
                            {
                                if (reservoirSettings.PoolSettingsCollection[i].Name == poolName)
                                {
                                    poolIdx = i;
                                    break;
                                }
                                else if (i == reservoirSettings.PoolSettingsCollection.Count - 1)
                                {
                                    throw new Exception($"Name {poolName} not found among resevoir's pools.");
                                }
                            }
                            allowedPools.Add(new MapperSettings.AllowedPool { _reservoirInstanceIdx = reservoirInstanceIdx, _poolIdx = poolIdx });
                        }
                        MapperCfg.PoolsMap.Add(readoutUnitName, allowedPools);
                    }

                    //Allowed routed input fields
                    List<int> allowedRoutedFieldsIdxs = new List<int>();
                    XElement allowedInputFieldsElem = mapElem.Descendants("allowedInputFields").FirstOrDefault();
                    List<string> routedInputFieldNames = NeuralPreprocessorConfig.InputConfig.RoutedFieldNameCollection();
                    if (allowedInputFieldsElem != null)
                    {
                        foreach (XElement allowedInputFieldElem in allowedInputFieldsElem.Descendants("field"))
                        {
                            //Input field name
                            string inputFieldName = allowedInputFieldElem.Attribute("name").Value;
                            int routedFieldIdx = routedInputFieldNames.IndexOf(inputFieldName);
                            if (routedFieldIdx == -1)
                            {
                                throw new Exception($"Name {inputFieldName} not found among input fields allowed to be routed to readout.");
                            }
                            allowedRoutedFieldsIdxs.Add(routedFieldIdx);
                        }
                        MapperCfg.RoutedInputFieldsMap.Add(readoutUnitName, allowedRoutedFieldsIdxs);
                    }

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
                (MapperCfg == null && cmpSettings.MapperCfg != null) ||
                (MapperCfg != null && cmpSettings.MapperCfg == null) ||
                (MapperCfg != null && !Equals(MapperCfg, cmpSettings.MapperCfg))
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
            public Dictionary<string, List<AllowedPool>> PoolsMap { get; }

            /// <summary>
            /// Mapping of readout unit and allowed routed input fields indexes
            /// </summary>
            public Dictionary<string, List<int>> RoutedInputFieldsMap { get; }

            /// <summary>
            /// Creates an empty initialized instance
            /// </summary>
            public MapperSettings()
            {
                PoolsMap = new Dictionary<string, List<AllowedPool>>();
                RoutedInputFieldsMap = new Dictionary<string, List<int>>();
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
                //Pools map
                if (PoolsMap.Count != cmpSettings.PoolsMap.Count)
                {
                    return false;
                }
                foreach(string key in PoolsMap.Keys)
                {
                    List<AllowedPool> myAllowedPools = PoolsMap[key];
                    List<AllowedPool> cmpAllowedPools = null;
                    try
                    {
                        cmpAllowedPools = cmpSettings.PoolsMap[key];
                    }
                    catch
                    {
                        return false;
                    }
                    if(myAllowedPools.Count != cmpAllowedPools.Count)
                    {
                        return false;
                    }
                    foreach(AllowedPool allowedPool in myAllowedPools)
                    {
                        for(int i = 0; i < cmpAllowedPools.Count; i++)
                        {
                            if(cmpAllowedPools[i]._reservoirInstanceIdx == allowedPool._reservoirInstanceIdx && cmpAllowedPools[i]._poolIdx == allowedPool._poolIdx)
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


                //Input fields map
                if (RoutedInputFieldsMap.Count != cmpSettings.RoutedInputFieldsMap.Count)
                {
                    return false;
                }
                foreach (string key in RoutedInputFieldsMap.Keys)
                {
                    List<int> myAllowedRoutedInputFieldsIdxs = RoutedInputFieldsMap[key];
                    List<int> cmpAllowedRoutedInputFieldsIdxs = null;
                    try
                    {
                        cmpAllowedRoutedInputFieldsIdxs = cmpSettings.RoutedInputFieldsMap[key];
                    }
                    catch
                    {
                        return false;
                    }
                    if (myAllowedRoutedInputFieldsIdxs.Count != cmpAllowedRoutedInputFieldsIdxs.Count)
                    {
                        return false;
                    }
                    foreach (int allowedInputField in myAllowedRoutedInputFieldsIdxs)
                    {
                        for (int i = 0; i < cmpAllowedRoutedInputFieldsIdxs.Count; i++)
                        {
                            if (cmpAllowedRoutedInputFieldsIdxs[i] == allowedInputField)
                            {
                                break;
                            }
                            else if (i == cmpAllowedRoutedInputFieldsIdxs.Count - 1)
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
                //Allowed pools
                foreach (KeyValuePair<string, List<AllowedPool>> keyValuePair in PoolsMap)
                {
                    List<AllowedPool> clonnedAllowedPoolsList = new List<AllowedPool>(keyValuePair.Value.Count);
                    foreach (AllowedPool allowedPool in keyValuePair.Value)
                    {
                        AllowedPool clonnedAllowedPool = new AllowedPool { _reservoirInstanceIdx = allowedPool._reservoirInstanceIdx, _poolIdx = allowedPool._poolIdx };
                        clonnedAllowedPoolsList.Add(clonnedAllowedPool);
                    }
                    clonnedMapper.PoolsMap.Add(keyValuePair.Key, clonnedAllowedPoolsList);
                }
                //Allowed input fields
                foreach (KeyValuePair<string, List<int>> keyValuePair in RoutedInputFieldsMap)
                {
                    List<int> clonnedAllowedRoutedInputFieldsIdxs = new List<int>(keyValuePair.Value.Count);
                    foreach (int allowedRoutedInputFieldIdx in keyValuePair.Value)
                    {
                        clonnedAllowedRoutedInputFieldsIdxs.Add(allowedRoutedInputFieldIdx);
                    }
                    clonnedMapper.RoutedInputFieldsMap.Add(keyValuePair.Key, clonnedAllowedRoutedInputFieldsIdxs);
                }
                return clonnedMapper;
            }

            //Inner classes
            /// <summary>
            /// Identification of the pool from which are allowed predictors
            /// </summary>
            [Serializable]
            public class AllowedPool
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
                    AllowedPool cmpSettings = obj as AllowedPool;
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
