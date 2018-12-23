using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Xml.Linq;
using System.Reflection;
using RCNet.Extensions;
using RCNet.XmlTools;
using RCNet.RandomValue;
using RCNet.Neural.Data.Modulation;
using RCNet.Neural.Network.SM.Synapse;
using RCNet.Neural.Network.SM.ReservoirStructure;
using RCNet.Neural.Network.SM.Readout;

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
        /// Settings of this State Machine external and internal input
        /// </summary>
        public InputSettings InputConfig { get; set; }
        /// <summary>
        /// Collection of definitions for future instances of internal reservoirs.
        /// Each definition contains a specific setting for the reservoir and mapping of the input fields
        /// </summary>
        public List<ReservoirInstanceDefinition> ReservoirInstanceDefinitionCollection { get; set; }
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
            InputConfig = new InputSettings();
            ReservoirInstanceDefinitionCollection = new List<ReservoirInstanceDefinition>();
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
            InputConfig = source.InputConfig.DeepClone();
            ReservoirInstanceDefinitionCollection = new List<ReservoirInstanceDefinition>(source.ReservoirInstanceDefinitionCollection.Count);
            foreach (ReservoirInstanceDefinition mapping in source.ReservoirInstanceDefinitionCollection)
            {
                ReservoirInstanceDefinitionCollection.Add(mapping.DeepClone());
            }
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
            //Input
            InputConfig = new InputSettings(stateMachineSettingsElem.Descendants("input").First());
            if(TaskType != CommonEnums.TaskType.Prediction && InputConfig.RouteExternalInputToReadout)
            {
                throw new Exception("Routing input to readout is allowed for prediction task only.");
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

                //Distinct input field names and corresponding indexes using by the reservoir instance
                List<string> resInpFieldNameCollection = new List<string>();
                foreach (XElement inputFieldAssignmentElem in reservoirInstanceElem.Descendants("inputFieldAssignments").First().Descendants("inputFieldAssignment"))
                {
                    //Input field name
                    string inputFieldName = inputFieldAssignmentElem.Attribute("inputFieldName").Value;
                    //Index in InputConfig
                    int inputFieldIdx = InputConfig.IndexOf(inputFieldName);
                    //Found?
                    if (inputFieldIdx < 0)
                    {
                        //Not found
                        throw new Exception($"Reservoir instance {reservoirInstanceDefinition.InstanceName}: input field {inputFieldName} is not defined among State Machine input fields.");
                    }
                    //Add distinct name to the collection
                    if(resInpFieldNameCollection.IndexOf(inputFieldName) < 0)
                    {
                        reservoirInstanceDefinition.SMInputFieldIdxCollection.Add(inputFieldIdx);
                        resInpFieldNameCollection.Add(inputFieldName);
                    }
                }

                //Assignments of the reservoir's input fields to the pools
                foreach (XElement inputFieldAssignmentElem in reservoirInstanceElem.Descendants("inputFieldAssignments").First().Descendants("inputFieldAssignment"))
                {
                    //Input field
                    string inputFieldName = inputFieldAssignmentElem.Attribute("inputFieldName").Value;
                    //Index in resInpFieldNameCollection
                    int resInputFieldIdx = resInpFieldNameCollection.IndexOf(inputFieldName);
                    //Found?
                    if (resInputFieldIdx < 0)
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
                    reservoirInstanceDefinition.InputFieldAssignmentCollection.Add(new ReservoirInstanceDefinition.InputFieldAssignment(resInputFieldIdx, targetPoolID, density, synapseCfg));
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
                !Equals(InputConfig, cmpSettings.InputConfig) ||
                ReservoirInstanceDefinitionCollection.Count != cmpSettings.ReservoirInstanceDefinitionCollection.Count ||
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
        /// Encapsulates State Machine input definition
        /// </summary>
        [Serializable]
        public class InputSettings
        {
            //Attribute properties
            /// <summary>
            /// The parameter specifies whether the external input values will be forwarded to the regression
            /// along with the predictors from the reservoirs.
            /// </summary>
            public bool RouteExternalInputToReadout { get; set; }

            /// <summary>
            /// External input data
            /// </summary>
            public List<Field> ExternalFieldCollection { get; set; }

            /// <summary>
            /// Internal (augmented) input data
            /// </summary>
            public List<InternalField> InternalFieldCollection { get; set; }

            //Constructors
            /// <summary>
            /// Creates an uninitialized instance.
            /// </summary>
            public InputSettings()
            {
                RouteExternalInputToReadout = false;
                ExternalFieldCollection = new List<Field>();
                InternalFieldCollection = new List<InternalField>();
                return;
            }

            /// <summary>
            /// The deep copy constructor.
            /// </summary>
            /// <param name="source">Source instance</param>
            public InputSettings(InputSettings source)
                :this()
            {
                RouteExternalInputToReadout = source.RouteExternalInputToReadout;
                foreach (Field field in source.ExternalFieldCollection)
                {
                    ExternalFieldCollection.Add(field.DeepClone());
                }
                foreach (InternalField field in source.InternalFieldCollection)
                {
                    InternalFieldCollection.Add((InternalField)field.DeepClone());
                }
                return;
            }

            /// <summary>
            /// Creates an initialized instance
            /// </summary>
            /// <param name="settingsElem">Xml element containing associated signal modulator settings</param>
            public InputSettings(XElement settingsElem)
                :this()
            {
                Dictionary<string, string> uniquenessChecker = new Dictionary<string, string>();
                //External fields
                RouteExternalInputToReadout = bool.Parse(settingsElem.Descendants("external").First().Attribute("routeToReadout").Value);
                foreach (XElement extFieldElem in settingsElem.Descendants("external").First().Descendants())
                {
                    string fieldName = extFieldElem.Attribute("name").Value;
                    if(uniquenessChecker.ContainsKey(fieldName))
                    {
                        throw new Exception($"Duplicit input field name {fieldName}");
                    }
                    uniquenessChecker.Add(fieldName, fieldName);
                    ExternalFieldCollection.Add(new Field(fieldName));
                }
                //Internal fields
                XElement intFieldsElem = settingsElem.Descendants("internal").FirstOrDefault();
                if (intFieldsElem != null)
                {
                    foreach (XElement intFieldElem in intFieldsElem.Descendants("field"))
                    {
                        string fieldName = intFieldElem.Attribute("name").Value;
                        if (uniquenessChecker.ContainsKey(fieldName))
                        {
                            throw new Exception($"Duplicit input field name: {fieldName}");
                        }
                        uniquenessChecker.Add(fieldName, fieldName);
                        InternalFieldCollection.Add(new InternalField(fieldName, intFieldElem.Descendants().First()));
                    }
                }
                return;
            }

            //Properties
            /// <summary>
            /// Total number of SM input fields
            /// </summary>
            public int NumOfFields { get { return ExternalFieldCollection.Count + InternalFieldCollection.Count; } }

            //Methods
            /// <summary>
            /// Function searches for index of the specified field among all SM input fields
            /// </summary>
            /// <param name="fieldName">Name of the searched field</param>
            /// <returns>Zero based index of the field or -1 if searching fails</returns>
            public int IndexOf(string fieldName)
            {
                for(int i = 0; i < ExternalFieldCollection.Count; i++)
                {
                    if(ExternalFieldCollection[i].Name == fieldName)
                    {
                        return i;
                    }
                }
                for (int i = 0; i < InternalFieldCollection.Count; i++)
                {
                    if (InternalFieldCollection[i].Name == fieldName)
                    {
                        return ExternalFieldCollection.Count + i;
                    }
                }
                return -1;
            }

            /// <summary>
            /// Returns collection of external fields names
            /// </summary>
            /// <returns></returns>
            public List<string> ExternalFieldNameCollection()
            {
                return (from item in ExternalFieldCollection select item.Name).ToList();
            }

            /// <summary>
            /// Creates the deep copy instance of this instance
            /// </summary>
            /// <returns></returns>
            public InputSettings DeepClone()
            {
                return new InputSettings(this);
            }

            /// <summary>
            /// See the base.
            /// </summary>
            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                InputSettings cmpSettings = obj as InputSettings;
                if (RouteExternalInputToReadout != cmpSettings.RouteExternalInputToReadout ||
                    ExternalFieldCollection.Count != cmpSettings.ExternalFieldCollection.Count ||
                    InternalFieldCollection.Count != cmpSettings.InternalFieldCollection.Count)
                {
                    return false;
                }
                for (int i = 0; i < ExternalFieldCollection.Count; i++)
                {
                    if(!Equals(ExternalFieldCollection[i], cmpSettings.ExternalFieldCollection[i]))
                    {
                        return false;
                    }
                }
                for (int i = 0; i < InternalFieldCollection.Count; i++)
                {
                    if (!Equals(InternalFieldCollection[i], cmpSettings.InternalFieldCollection[i]))
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


            //Inner classes
            /// <summary>
            /// Represents simple input field
            /// </summary>
            [Serializable]
            public class Field
            {
                //Attribute properties
                /// <summary>
                /// Field name
                /// </summary>
                public string Name { get; set; }

                //Constructors
                /// <summary>
                /// Creates an initialized instance
                /// </summary>
                /// <param name="name">Field name</param>
                public Field(string name)
                {
                    Name = name;
                    return;
                }

                /// <summary>
                /// The deep copy constructor.
                /// </summary>
                /// <param name="source">Source instance</param>
                public Field(Field source)
                {
                    Name = source.Name;
                    return;
                }

                //Methods
                /// <summary>
                /// Creates the deep copy instance of this instance
                /// </summary>
                /// <returns></returns>
                public virtual Field DeepClone()
                {
                    return new Field(this);
                }

                /// <summary>
                /// See the base.
                /// </summary>
                public override bool Equals(object obj)
                {
                    if (obj == null) return false;
                    Field cmpSettings = obj as Field;
                    if (Name != cmpSettings.Name)
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
                    return Name.GetHashCode();
                }

            }//Field

            /// <summary>
            /// Represents input internal field
            /// </summary>
            [Serializable]
            public class InternalField : Field
            {
                //Attribute properties
                /// <summary>
                /// Signal modualtor configuration
                /// </summary>
                public Object ModulatorSettings { get; set; }

                //Constructors
                /// <summary>
                /// Creates an initialized instance
                /// </summary>
                /// <param name="name">Field name</param>
                /// <param name="settingsElem">Xml element containing associated signal modulator settings</param>
                public InternalField(string name, XElement settingsElem)
                    :base(name)
                {
                    switch(settingsElem.Name.LocalName)
                    {
                        case "constModulator":
                            ModulatorSettings = new ConstModulatorSettings(settingsElem);
                            break;
                        case "randomModulator":
                            ModulatorSettings = new RandomValueSettings(settingsElem);
                            break;
                        case "sinusoidalModulator":
                            ModulatorSettings = new SinusoidalModulatorSettings(settingsElem);
                            break;
                        case "mackeyGlassModulator":
                            ModulatorSettings = new MackeyGlassModulatorSettings(settingsElem);
                            break;
                        default:
                            throw new Exception($"Unknown modulator settings {settingsElem.Name.LocalName}");
                    }
                    return;
                }

                /// <summary>
                /// The deep copy constructor.
                /// </summary>
                /// <param name="source">Source instance</param>
                public InternalField(InternalField source)
                    :base(source)
                {
                    if(source.ModulatorSettings.GetType() == typeof(ConstModulatorSettings))
                    {
                        ModulatorSettings = ((ConstModulatorSettings)source.ModulatorSettings).DeepClone();
                    }
                    else if(source.ModulatorSettings.GetType() == typeof(RandomValueSettings))
                    {
                        ModulatorSettings = ((RandomValueSettings)source.ModulatorSettings).DeepClone();
                    }
                    else if (source.ModulatorSettings.GetType() == typeof(SinusoidalModulatorSettings))
                    {
                        ModulatorSettings = ((SinusoidalModulatorSettings)source.ModulatorSettings).DeepClone();
                    }
                    else if (source.ModulatorSettings.GetType() == typeof(MackeyGlassModulatorSettings))
                    {
                        ModulatorSettings = ((MackeyGlassModulatorSettings)source.ModulatorSettings).DeepClone();
                    }
                    else
                    {
                        throw new Exception($"Unknown modulator settings {source.ModulatorSettings.ToString()}");
                    }
                    return;
                }

                //Methods
                /// <summary>
                /// Creates the deep copy instance of this instance
                /// </summary>
                /// <returns></returns>
                public override Field DeepClone()
                {
                    return new InternalField(this);
                }

                /// <summary>
                /// See the base.
                /// </summary>
                public override bool Equals(object obj)
                {
                    if (obj == null) return false;
                    InternalField cmpSettings = obj as InternalField;
                    if (!base.Equals(obj) || 
                        !Equals(ModulatorSettings, cmpSettings.ModulatorSettings)
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

            }//InternalField

        }//InputSettings

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
            public List<int> SMInputFieldIdxCollection { get; set; }
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
                SMInputFieldIdxCollection = new List<int>();
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
                SMInputFieldIdxCollection = new List<int>(source.SMInputFieldIdxCollection);
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
                    SMInputFieldIdxCollection.Count != cmpSettings.SMInputFieldIdxCollection.Count ||
                    !SMInputFieldIdxCollection.ToArray().ContainsEqualValues(cmpSettings.SMInputFieldIdxCollection.ToArray()) ||
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
                /// Index of the reservoir's input field
                /// </summary>
                public int FieldIdx { get; set; }
                /// <summary>
                /// ID of the pool
                /// </summary>
                public int PoolID { get; set; }
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
