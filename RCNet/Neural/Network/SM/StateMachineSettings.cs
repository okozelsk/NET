using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Xml.Linq;
using System.Reflection;
using RCNet.Extensions;
using RCNet.XmlTools;
using RCNet.RandomValue;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// The class contains State Machine configuration parameters and also contains
    /// internal logic so it is not just a container of parameters. To create a proper instance by hand is not
    /// a trivial task.
    /// The easiest and safest way to create an instance is to use the xml constructor.
    /// </summary>
    [Serializable]
    public class StateMachineSettings
    {
        //Attribute properties
        /// <summary>
        /// Type of the task for which is Machine designed
        /// </summary>
        public CommonEnums.TaskType TaskType { get; set; }
        /// <summary>
        /// A value greater than or equal to 0 will always ensure the same initialization of the internal
        /// random number generator and therefore the same network structure, which is good for tuning
        /// other network parameters.
        /// A value less than 0 causes a fully random initialization when creating a network instance.
        /// </summary>
        public int RandomizerSeek { get; set; }
        /// <summary>
        /// The collection of input field names in order of how they will be pushed to the network.
        /// </summary>
        public List<string> InputFieldNameCollection { get; set; }
        /// <summary>
        /// Collection of definitions for future instances of internal reservoirs.
        /// Each definition contains a specific setting for the reservoir and mapping of the input fields
        /// </summary>
        public List<ReservoirInstanceDefinition> ReservoirInstanceDefinitionCollection { get; set; }
        /// <summary>
        /// The parameter specifies whether the input values will be forwarded to the regression
        /// along with the predictors from the reservoirs.
        /// </summary>
        public bool RouteInputToReadout { get; set; }
        /// <summary>
        /// Configuration of the readout layer
        /// </summary>
        public ReadoutLayerSettings ReadoutLayerConfig { get; set; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        public StateMachineSettings()
        {
            //Default settings
            TaskType = CommonEnums.TaskType.Prediction;
            RandomizerSeek = 0;
            InputFieldNameCollection = new List<string>();
            ReservoirInstanceDefinitionCollection = new List<ReservoirInstanceDefinition>();
            RouteInputToReadout = false;
            ReadoutLayerConfig = new ReadoutLayerSettings();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public StateMachineSettings(StateMachineSettings source)
        {
            //Copy
            TaskType = source.TaskType;
            RandomizerSeek = source.RandomizerSeek;
            InputFieldNameCollection = new List<string>(source.InputFieldNameCollection);
            ReservoirInstanceDefinitionCollection = new List<ReservoirInstanceDefinition>(source.ReservoirInstanceDefinitionCollection.Count);
            foreach (ReservoirInstanceDefinition mapping in source.ReservoirInstanceDefinitionCollection)
            {
                ReservoirInstanceDefinitionCollection.Add(mapping.DeepClone());
            }
            RouteInputToReadout = source.RouteInputToReadout;
            ReadoutLayerConfig = new ReadoutLayerSettings(source.ReadoutLayerConfig);
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
            //Task type
            TaskType = CommonEnums.ParseTaskType(stateMachineSettingsElem.Attribute("taskType").Value);
            //Randomizer seek
            RandomizerSeek = int.Parse(stateMachineSettingsElem.Attribute("randomizerSeek").Value);
            //Input fields
            XElement inputFieldsElem = stateMachineSettingsElem.Descendants("inputFields").First();
            RouteInputToReadout = (inputFieldsElem.Attribute("routeToReadout") == null) ? false : bool.Parse(inputFieldsElem.Attribute("routeToReadout").Value);
            if(TaskType != CommonEnums.TaskType.Prediction && RouteInputToReadout)
            {
                throw new Exception("Routing input to readout is allowed for prediction task only.");
            }
            InputFieldNameCollection = new List<string>();
            foreach(XElement inputFieldElem in inputFieldsElem.Descendants("field"))
            {
                InputFieldNameCollection.Add(inputFieldElem.Attribute("name").Value);
            }
            //Collect available reservoir settings
            List<ReservoirSettings> availableResSettings = new List<ReservoirSettings>();
            XElement reservoirSettingsContainerElem = stateMachineSettingsElem.Descendants("reservoirCfgContainer").First();
            foreach (XElement reservoirSettingsElem in reservoirSettingsContainerElem.Descendants("reservoirCfg"))
            {
                availableResSettings.Add(new ReservoirSettings(reservoirSettingsElem));
            }
            //Readout layer
            XElement readoutLayerElem = stateMachineSettingsElem.Descendants("readoutLayer").First();
            ReadoutLayerConfig = new ReadoutLayerSettings(readoutLayerElem);
            //Mapping of input fields to reservoir settings (future reservoir instance)
            ReservoirInstanceDefinitionCollection = new List<ReservoirInstanceDefinition>();
            XElement reservoirInstancesContainerElem = stateMachineSettingsElem.Descendants("reservoirInstanceContainer").First();
            foreach (XElement reservoirInstanceElem in reservoirInstancesContainerElem.Descendants("reservoirInstance"))
            {
                ReservoirInstanceDefinition reservoirInstanceDefinition = new ReservoirInstanceDefinition
                {
                    InstanceName = reservoirInstanceElem.Attribute("name").Value,
                    AugmentedStates = bool.Parse(reservoirInstanceElem.Attribute("augmentedStates").Value),
                    //Select reservoir settings
                    Settings = (from settings in availableResSettings
                                         where settings.SettingsName == reservoirInstanceElem.Attribute("cfg").Value
                                         select settings).FirstOrDefault()
                };
                if (reservoirInstanceDefinition.Settings == null)
                {
                    throw new Exception($"Reservoir settings '{reservoirInstanceElem.Attribute("cfg").Value}' was not found among available settings.");
                }

                //Reservoir's input fields aggregation
                List<string> resInpFieldNameCollection = new List<string>();
                foreach (XElement inputFieldAssignmentElem in reservoirInstanceElem.Descendants("inputFieldAssignments").First().Descendants("inputFieldAssignment"))
                {
                    //Input field name
                    string inputFieldName = inputFieldAssignmentElem.Attribute("inputFieldName").Value;
                    //Index in InputFieldNameCollection
                    int inputFieldIdx = InputFieldNameCollection.IndexOf(inputFieldName);
                    //Found?
                    if (inputFieldIdx < 0)
                    {
                        //Not found
                        throw new Exception($"Reservoir instance {reservoirInstanceDefinition.InstanceName}: input field {inputFieldName} is not defined among State Machine input fields.");
                    }
                    //Add distinct name to the collection
                    if(resInpFieldNameCollection.IndexOf(inputFieldName) < 0)
                    {
                        reservoirInstanceDefinition.InputFieldIdxCollection.Add(inputFieldIdx);
                        resInpFieldNameCollection.Add(inputFieldName);
                    }
                }

                //Assignments of the reservoir's input fields to the pools
                foreach (XElement inputFieldAssignmentElem in reservoirInstanceElem.Descendants("inputFieldAssignments").First().Descendants("inputFieldAssignment"))
                {
                    //Input field
                    string inputFieldName = inputFieldAssignmentElem.Attribute("inputFieldName").Value;
                    //Index in resInpFieldNameCollection
                    int inputFieldIdx = resInpFieldNameCollection.IndexOf(inputFieldName);
                    //Found?
                    if (inputFieldIdx < 0)
                    {
                        //Not found
                        throw new Exception($"Reservoir instance {reservoirInstanceDefinition.InstanceName}: input field {inputFieldName} is not defined among Reservoir's input fields.");
                    }
                    //Target pool
                    string targetPoolName = inputFieldAssignmentElem.Attribute("poolName").Value;
                    int targetPoolID = -1;
                    //Find target pool ID (index)
                    for (int idx = 0; idx < reservoirInstanceDefinition.Settings.PoolSettingsCollection.Count; idx++)
                    {
                        if (reservoirInstanceDefinition.Settings.PoolSettingsCollection[idx].Name == targetPoolName)
                        {
                            targetPoolID = idx;
                            break;
                        }
                    }
                    //Found?
                    if (targetPoolID < 0)
                    {
                        //Not found
                        throw new Exception($"Reservoir instance {reservoirInstanceDefinition.InstanceName}: pool {targetPoolName} is not defined among Reservoir pools.");
                    }
                    //Density
                    double density = double.Parse(inputFieldAssignmentElem.Attribute("density").Value, CultureInfo.InvariantCulture);
                    //Static synapse settings
                    StaticSynapseSettings synapseCfg = new StaticSynapseSettings(inputFieldAssignmentElem.Descendants("staticSynapse").First());
                    //Add new assignment
                    reservoirInstanceDefinition.InputFieldAssignmentCollection.Add(new ReservoirInstanceDefinition.InputFieldAssignment(inputFieldIdx, targetPoolID, density, synapseCfg));
                }
                ReservoirInstanceDefinitionCollection.Add(reservoirInstanceDefinition);
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
            if (TaskType != cmpSettings.TaskType ||
                RandomizerSeek != cmpSettings.RandomizerSeek ||
                !InputFieldNameCollection.ToArray().ContainsEqualValues(cmpSettings.InputFieldNameCollection.ToArray()) ||
                ReservoirInstanceDefinitionCollection.Count != cmpSettings.ReservoirInstanceDefinitionCollection.Count ||
                RouteInputToReadout != cmpSettings.RouteInputToReadout ||
                !Equals(ReadoutLayerConfig, cmpSettings.ReadoutLayerConfig)
                )
            {
                return false;
            }
            for (int i = 0; i < ReservoirInstanceDefinitionCollection.Count; i++)
            {
                if (!ReservoirInstanceDefinitionCollection[i].Equals(cmpSettings.ReservoirInstanceDefinitionCollection[i]))
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
        public StateMachineSettings DeepClone()
        {
            StateMachineSettings clone = new StateMachineSettings(this);
            return clone;
        }

        //Inner classes
        /// <summary>
        /// Definition of future instance of State Machine internal reservoir.
        /// Definition contains a specific setting for the reservoir and maps the input
        /// fields of the reservoir to the input fields of the State Machine network.
        /// </summary>
        [Serializable]
        public class ReservoirInstanceDefinition
        {
            //Attribute properties
            /// <summary>
            /// Name of the reservoir instance. It is useful for logging and visualization
            /// purposes so instance name should be unique within the State Machine.
            /// </summary>
            public string InstanceName { get; set; }
            /// <summary>
            /// ReservoirSettings of the reservoir instance.
            /// </summary>
            public ReservoirSettings Settings { get; set; }
            /// <summary>
            /// The parameter specifies whether, in addition to the standard neuron states,
            /// augmented states of reservoir neurons will be added to the reservoir output predictors.
            /// Augmented states double the number of output predictors of the reservoir.
            /// The augmented state of the neuron is its squared state.
            /// </summary>
            public bool AugmentedStates { get; set; }
            /// <summary>
            /// Reservoir's input fields indexes in State Machine input fields.
            /// </summary>
            public List<int> InputFieldIdxCollection { get; set; }
            /// <summary>
            /// Mapping of the Reservoir's input fields to the Reservoir's pools.
            /// </summary>
            public List<InputFieldAssignment> InputFieldAssignmentCollection { get; set; }

            //Constructors
            /// <summary>
            /// Creates an unitialized instance.
            /// </summary>
            public ReservoirInstanceDefinition()
            {
                InstanceName = string.Empty;
                Settings = null;
                AugmentedStates = false;
                InputFieldIdxCollection = new List<int>();
                InputFieldAssignmentCollection = new List<InputFieldAssignment>();
                return;
            }

            /// <summary>
            /// The deep copy constructor.
            /// </summary>
            /// <param name="source">Source instance</param>
            public ReservoirInstanceDefinition(ReservoirInstanceDefinition source)
            {
                InstanceName = source.InstanceName;
                Settings = source.Settings.DeepClone();
                AugmentedStates = source.AugmentedStates;
                InputFieldIdxCollection = new List<int>(source.InputFieldIdxCollection);
                InputFieldAssignmentCollection = new List<InputFieldAssignment>(source.InputFieldAssignmentCollection.Count);
                foreach(InputFieldAssignment ifa in source.InputFieldAssignmentCollection)
                {
                    InputFieldAssignmentCollection.Add(ifa.DeepClone());
                }
                return;
            }
            
            //Methods
            /// <summary>
            /// Creates the deep copy instance of this instance
            /// </summary>
            /// <returns></returns>
            public ReservoirInstanceDefinition DeepClone()
            {
                return new ReservoirInstanceDefinition(this);
            }

            /// <summary>
            /// See the base.
            /// </summary>
            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                ReservoirInstanceDefinition cmpSettings = obj as ReservoirInstanceDefinition;
                if (InstanceName != cmpSettings.InstanceName ||
                    !Equals(Settings, cmpSettings.Settings) ||
                    AugmentedStates != cmpSettings.AugmentedStates ||
                    InputFieldIdxCollection.Count != cmpSettings.InputFieldIdxCollection.Count ||
                    !InputFieldIdxCollection.ToArray().ContainsEqualValues(cmpSettings.InputFieldIdxCollection.ToArray()) ||
                    InputFieldAssignmentCollection.Count != cmpSettings.InputFieldAssignmentCollection.Count
                    )
                {
                    return false;
                }
                for(int i = 0; i < InputFieldAssignmentCollection.Count; i++)
                {
                    if(!Equals(InputFieldAssignmentCollection[i], cmpSettings.InputFieldAssignmentCollection[i]))
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
                return InstanceName.GetHashCode();
            }

            //Inner classes
            /// <summary>
            /// Assigmnment of input field to pool (connections)
            /// </summary>
            [Serializable]
            public class InputFieldAssignment
            {
                //Attribute properties
                /// <summary>
                /// Index of the reservoir input field
                /// </summary>
                public int FieldIdx;
                /// <summary>
                /// ID of the pool
                /// </summary>
                public int PoolID;
                /// <summary>
                /// Each reservoir input neuron will be connected by the synapse to the number of
                /// pool neurons = (Dim.Size * Density).
                /// Typical InputConnectionDensity = 1 (it means the full connectivity).
                /// </summary>
                public double Density { get; set; }
                /// <summary>
                /// Input neuron to pool's neuron synapse settings
                /// </summary>
                public Object SynapseCfg { get; set; }

                //Constructors
                /// <summary>
                /// Creates an itialized instance.
                /// </summary>
                /// <param name="fieldIdx">Index of the reservoir input field</param>
                /// <param name="poolID">ID of the target pool</param>
                /// <param name="density">Input field connection density</param>
                /// <param name="synapseCfg">Input neuron to pool's neuron synapse settings</param>
                public InputFieldAssignment(int fieldIdx, int poolID, double density, Object synapseCfg)
                {
                    FieldIdx = fieldIdx;
                    PoolID = poolID;
                    Density = density;
                    SynapseCfg = null;
                    if (synapseCfg != null)
                    {
                        if (synapseCfg.GetType() == typeof(StaticSynapseSettings))
                        {
                            SynapseCfg = ((StaticSynapseSettings)synapseCfg).DeepClone();
                        }
                        else
                        {
                            SynapseCfg = ((DynamicSynapseSettings)synapseCfg).DeepClone();
                        }
                    }
                    return;
                }

                /// <summary>
                /// The deep copy constructor.
                /// </summary>
                /// <param name="source">Source instance</param>
                public InputFieldAssignment(InputFieldAssignment source)
                {
                    FieldIdx = source.FieldIdx;
                    PoolID = source.PoolID;
                    Density = source.Density;
                    SynapseCfg = null;
                    if (source.SynapseCfg != null)
                    {
                        if (source.SynapseCfg.GetType() == typeof(StaticSynapseSettings))
                        {
                            SynapseCfg = ((StaticSynapseSettings)source.SynapseCfg).DeepClone();
                        }
                        else
                        {
                            SynapseCfg = ((DynamicSynapseSettings)source.SynapseCfg).DeepClone();
                        }
                    }
                    return;
                }

                //Methods
                /// <summary>
                /// Creates the deep copy instance of this instance
                /// </summary>
                /// <returns></returns>
                public InputFieldAssignment DeepClone()
                {
                    return new InputFieldAssignment(this);
                }

                /// <summary>
                /// See the base.
                /// </summary>
                public override bool Equals(object obj)
                {
                    if (obj == null) return false;
                    InputFieldAssignment cmpSettings = obj as InputFieldAssignment;
                    if (FieldIdx != cmpSettings.FieldIdx ||
                        PoolID != cmpSettings.PoolID ||
                        Density != cmpSettings.Density ||
                        !Equals(SynapseCfg, cmpSettings.SynapseCfg)
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

            }//InputFieldAssignment

        }//ReservoirInstanceDefinition

    }//StateMachineSettings

}//Namespace
