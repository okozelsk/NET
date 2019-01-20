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

namespace RCNet.Neural.Network.SM.Preprocessing
{
    /// <summary>
    /// The class contains Neural Preprocessor configuration parameters and also contains
    /// internal logic so it is not just a container of parameters. To create a proper instance by hand is not
    /// a trivial task.
    /// The easiest and safest way to create an instance is to use the xml constructor.
    /// </summary>
    [Serializable]
    public class NeuralPreprocessorSettings
    {
        //Attribute properties
        /// <summary>
        /// Settings of external and internal inputs
        /// </summary>
        public InputSettings InputConfig { get; set; }
        /// <summary>
        /// Collection of definitions for future instances of internal reservoirs.
        /// Each definition contains a specific setting for the reservoir and mapping of the input fields
        /// </summary>
        public List<ReservoirInstanceDefinition> ReservoirInstanceDefinitionCollection { get; set; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        public NeuralPreprocessorSettings()
        {
            //Default settings
            InputConfig = new InputSettings();
            ReservoirInstanceDefinitionCollection = new List<ReservoirInstanceDefinition>();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public NeuralPreprocessorSettings(NeuralPreprocessorSettings source)
        {
            //Copy
            InputConfig = source.InputConfig.DeepClone();
            ReservoirInstanceDefinitionCollection = new List<ReservoirInstanceDefinition>(source.ReservoirInstanceDefinitionCollection.Count);
            foreach (ReservoirInstanceDefinition mapping in source.ReservoirInstanceDefinitionCollection)
            {
                ReservoirInstanceDefinitionCollection.Add(mapping.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// This is the preferred way to instantiate Neural Preprocessor settings.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing the settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public NeuralPreprocessorSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.SM.Preprocessing.NeuralPreprocessorSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement neuralPreprocessorSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            //Input
            InputConfig = new InputSettings(neuralPreprocessorSettingsElem.Descendants("input").First());
            //Collect available reservoir settings
            List<ReservoirSettings> availableResSettings = new List<ReservoirSettings>();
            XElement reservoirSettingsContainerElem = neuralPreprocessorSettingsElem.Descendants("reservoirCfgContainer").First();
            foreach (XElement reservoirSettingsElem in reservoirSettingsContainerElem.Descendants("reservoirCfg"))
            {
                availableResSettings.Add(new ReservoirSettings(reservoirSettingsElem));
            }
            //Mapping of input fields to reservoir settings (future reservoir instance)
            ReservoirInstanceDefinitionCollection = new List<ReservoirInstanceDefinition>();
            XElement reservoirInstancesContainerElem = neuralPreprocessorSettingsElem.Descendants("reservoirInstanceContainer").First();
            int numOfNeuronsInLargestReservoir = 0;
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
                //Update number of neurons in the largest reservoir
                int numOfReservoirNeurons = 0;
                foreach(PoolSettings ps in reservoirInstanceDefinition.Settings.PoolSettingsCollection)
                {
                    numOfReservoirNeurons += ps.Dim.Size;
                }
                if(numOfNeuronsInLargestReservoir < numOfReservoirNeurons)
                {
                    numOfNeuronsInLargestReservoir = numOfReservoirNeurons;
                }

                //Distinct input field names and corresponding indexes using by the reservoir instance
                List<string> resInpFieldNameCollection = new List<string>();
                foreach (XElement inputFieldConnectionElem in reservoirInstanceElem.Descendants("inputConnections").First().Descendants("inputConnection"))
                {
                    //Input field name
                    string inputFieldName = inputFieldConnectionElem.Attribute("fieldName").Value;
                    //Index in InputConfig
                    int inputFieldIdx = InputConfig.IndexOf(inputFieldName);
                    //Found?
                    if (inputFieldIdx < 0)
                    {
                        //Not found
                        throw new Exception($"Reservoir instance {reservoirInstanceDefinition.InstanceName}: field name {inputFieldName} is not defined among input fields.");
                    }
                    //Add distinct name to the collection
                    if(resInpFieldNameCollection.IndexOf(inputFieldName) < 0)
                    {
                        reservoirInstanceDefinition.NPInputFieldIdxCollection.Add(inputFieldIdx);
                        resInpFieldNameCollection.Add(inputFieldName);
                    }
                }

                //Connections of the reservoir's input fields to the pools
                foreach (XElement inputConnectionElem in reservoirInstanceElem.Descendants("inputConnections").First().Descendants("inputConnection"))
                {
                    //Input field
                    string inputFieldName = inputConnectionElem.Attribute("fieldName").Value;
                    //Index in resInpFieldNameCollection
                    int resInputFieldIdx = resInpFieldNameCollection.IndexOf(inputFieldName);
                    //Found?
                    if (resInputFieldIdx < 0)
                    {
                        //Not found
                        throw new Exception($"Reservoir instance {reservoirInstanceDefinition.InstanceName}: input field {inputFieldName} is not defined among Reservoir's input fields.");
                    }
                    //Target pool
                    string targetPoolName = inputConnectionElem.Attribute("poolName").Value;
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
                    double density = double.Parse(inputConnectionElem.Attribute("density").Value, CultureInfo.InvariantCulture);
                    //Static synapse settings
                    StaticSynapseSettings synapseCfg = new StaticSynapseSettings(inputConnectionElem.Descendants("staticSynapse").First());
                    //Add new assignment
                    reservoirInstanceDefinition.InputConnectionCollection.Add(new ReservoirInstanceDefinition.InputConnection(resInputFieldIdx, targetPoolID, density, synapseCfg));
                }
                ReservoirInstanceDefinitionCollection.Add(reservoirInstanceDefinition);
            }
            //Finalize boot cycles if necessary
            if(InputConfig.BootCycles == -1 && InputConfig.FeedingType == CommonEnums.InputFeedingType.Continuous)
            {
                InputConfig.BootCycles = numOfNeuronsInLargestReservoir;
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
            NeuralPreprocessorSettings cmpSettings = obj as NeuralPreprocessorSettings;
            if (!Equals(InputConfig, cmpSettings.InputConfig) ||
                ReservoirInstanceDefinitionCollection.Count != cmpSettings.ReservoirInstanceDefinitionCollection.Count
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
        public NeuralPreprocessorSettings DeepClone()
        {
            NeuralPreprocessorSettings clone = new NeuralPreprocessorSettings(this);
            return clone;
        }

        //Inner classes
        /// <summary>
        /// Encapsulates Neural Preprocessor input definition
        /// </summary>
        [Serializable]
        public class InputSettings
        {
            //Attribute properties
            /// <summary>
            /// Type of input feeding
            /// </summary>
            public CommonEnums.InputFeedingType FeedingType { get; set; }

            /// <summary>
            /// Number of booting cycles (-1 means Auto)
            /// </summary>
            public int BootCycles { get; set; }

            /// <summary>
            /// The parameter specifies whether the external input values will be included among predictors
            /// together with the predictors from the reservoirs.
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
                FeedingType = CommonEnums.InputFeedingType.Continuous;
                BootCycles = -1;
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
                FeedingType = source.FeedingType;
                BootCycles = source.BootCycles;
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
            /// <param name="settingsElem">Xml element containing settings</param>
            public InputSettings(XElement settingsElem)
                :this()
            {
                //Feeding type
                XElement feedingElem = settingsElem.Descendants().First();
                if(feedingElem.Name.LocalName == "feedingContinuous")
                {
                    FeedingType = CommonEnums.InputFeedingType.Continuous;
                    //Number of booting cycles
                    string bootCyclesAttrValue = feedingElem.Attribute("bootCycles").Value;
                    if (bootCyclesAttrValue == "Auto")
                    {
                        //Automatic - will be set later
                        BootCycles = -1;
                    }
                    else
                    {
                        BootCycles = int.Parse(bootCyclesAttrValue, CultureInfo.InvariantCulture);
                    }
                    //Routing of external input to readout layer
                    RouteExternalInputToReadout = bool.Parse(feedingElem.Attribute("routeToReadout").Value);
                }
                else
                {
                    FeedingType = CommonEnums.InputFeedingType.Patterned;
                    BootCycles = 0;
                }
                //Fields
                Dictionary<string, string> uniquenessChecker = new Dictionary<string, string>();
                //External fields
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
            /// Total number of Neural Preprocessor input fields
            /// </summary>
            public int NumOfFields { get { return ExternalFieldCollection.Count + InternalFieldCollection.Count; } }

            //Methods
            /// <summary>
            /// Function searches for index of the specified field among all Neural Preprocessor input fields
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
                if (FeedingType != cmpSettings.FeedingType ||
                    BootCycles != cmpSettings.BootCycles ||
                    RouteExternalInputToReadout != cmpSettings.RouteExternalInputToReadout ||
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
                /// Signal generator configuration
                /// </summary>
                public Object GeneratorSettings { get; set; }

                //Constructors
                /// <summary>
                /// Creates an initialized instance
                /// </summary>
                /// <param name="name">Field name</param>
                /// <param name="settingsElem">Xml element containing associated signal generator settings</param>
                public InternalField(string name, XElement settingsElem)
                    :base(name)
                {
                    switch(settingsElem.Name.LocalName)
                    {
                        case "constGenerator":
                            GeneratorSettings = new ConstGeneratorSettings(settingsElem);
                            break;
                        case "randomGenerator":
                            GeneratorSettings = new RandomValueSettings(settingsElem);
                            break;
                        case "sinusoidalGenerator":
                            GeneratorSettings = new SinusoidalGeneratorSettings(settingsElem);
                            break;
                        case "mackeyGlassGenerator":
                            GeneratorSettings = new MackeyGlassGeneratorSettings(settingsElem);
                            break;
                        default:
                            throw new Exception($"Unknown generator settings {settingsElem.Name.LocalName}");
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
                    if(source.GeneratorSettings.GetType() == typeof(ConstGeneratorSettings))
                    {
                        GeneratorSettings = ((ConstGeneratorSettings)source.GeneratorSettings).DeepClone();
                    }
                    else if(source.GeneratorSettings.GetType() == typeof(RandomValueSettings))
                    {
                        GeneratorSettings = ((RandomValueSettings)source.GeneratorSettings).DeepClone();
                    }
                    else if (source.GeneratorSettings.GetType() == typeof(SinusoidalGeneratorSettings))
                    {
                        GeneratorSettings = ((SinusoidalGeneratorSettings)source.GeneratorSettings).DeepClone();
                    }
                    else if (source.GeneratorSettings.GetType() == typeof(MackeyGlassGeneratorSettings))
                    {
                        GeneratorSettings = ((MackeyGlassGeneratorSettings)source.GeneratorSettings).DeepClone();
                    }
                    else
                    {
                        throw new Exception($"Unknown generator settings {source.GeneratorSettings.ToString()}");
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
                        !Equals(GeneratorSettings, cmpSettings.GeneratorSettings)
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
        /// Definition of future instance of internal reservoir.
        /// Definition contains a specific setting for the reservoir and maps the input
        /// fields of the reservoir to the input fields of the Neural Preprocessor.
        /// </summary>
        [Serializable]
        public class ReservoirInstanceDefinition
        {
            //Attribute properties
            /// <summary>
            /// Name of the reservoir instance. It is useful for logging and visualization
            /// purposes so instance name should be unique within the Neural Preprocessor.
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
            /// Reservoir's input fields indexes in Neural Preprocessor input fields.
            /// </summary>
            public List<int> NPInputFieldIdxCollection { get; set; }
            /// <summary>
            /// Connections of the Reservoir's input fields to the Reservoir's pools.
            /// </summary>
            public List<InputConnection> InputConnectionCollection { get; set; }

            //Constructors
            /// <summary>
            /// Creates an unitialized instance.
            /// </summary>
            public ReservoirInstanceDefinition()
            {
                InstanceName = string.Empty;
                Settings = null;
                AugmentedStates = false;
                NPInputFieldIdxCollection = new List<int>();
                InputConnectionCollection = new List<InputConnection>();
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
                NPInputFieldIdxCollection = new List<int>(source.NPInputFieldIdxCollection);
                InputConnectionCollection = new List<InputConnection>(source.InputConnectionCollection.Count);
                foreach(InputConnection ifa in source.InputConnectionCollection)
                {
                    InputConnectionCollection.Add(ifa.DeepClone());
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
                    NPInputFieldIdxCollection.Count != cmpSettings.NPInputFieldIdxCollection.Count ||
                    !NPInputFieldIdxCollection.ToArray().ContainsEqualValues(cmpSettings.NPInputFieldIdxCollection.ToArray()) ||
                    InputConnectionCollection.Count != cmpSettings.InputConnectionCollection.Count
                    )
                {
                    return false;
                }
                for(int i = 0; i < InputConnectionCollection.Count; i++)
                {
                    if(!Equals(InputConnectionCollection[i], cmpSettings.InputConnectionCollection[i]))
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
            /// Connection of input field to pool
            /// </summary>
            [Serializable]
            public class InputConnection
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
                public InputConnection(int fieldIdx, int poolID, double density, Object synapseCfg)
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
                public InputConnection(InputConnection source)
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
                public InputConnection DeepClone()
                {
                    return new InputConnection(this);
                }

                /// <summary>
                /// See the base.
                /// </summary>
                public override bool Equals(object obj)
                {
                    if (obj == null) return false;
                    InputConnection cmpSettings = obj as InputConnection;
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

            }//InputConnection

        }//ReservoirInstanceDefinition

    }//NeuralPreprocessorSettings

}//Namespace
