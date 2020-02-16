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
using RCNet.Neural.Network.SM.Synapse;
using RCNet.Neural.Data.Filter;
using RCNet.Neural.Data;

namespace RCNet.Neural.Network.SM.Preprocessing
{
    /// <summary>
    /// Neural Preprocessor configuration parameters
    /// </summary>
    [Serializable]
    public class NeuralPreprocessorSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPCfgType";
        /// <summary>
        /// Value indicates to decide Neural Preprocessor's boot cycles automatically based on number of neurons within the resrvoirs
        /// </summary>
        public const int AutomaticBootCycles = -1;
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
        /// <summary>
        /// Specifies how many predictors having smallest standard deviation to be disabled 
        /// </summary>
        public double PredictorsReductionRatio { get; set; }

        //Constructors
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
            PredictorsReductionRatio = source.PredictorsReductionRatio;
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
            XElement neuralPreprocessorSettingsElem = Validate(elem, XsdTypeName);
            //Parsing
            //Predictors reduction ratio
            PredictorsReductionRatio = double.Parse(neuralPreprocessorSettingsElem.Attribute("predictorsReductionRatio").Value, CultureInfo.InvariantCulture);
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
            int reservoirInstanceID = 0;
            int numOfNeuronsInLargestReservoir = 0;
            foreach (XElement reservoirInstanceElem in reservoirInstancesContainerElem.Descendants("reservoirInstance"))
            {
                ReservoirInstanceDefinition reservoirInstanceDefinition = new ReservoirInstanceDefinition
                {
                    InstanceID = reservoirInstanceID,
                    InstanceName = reservoirInstanceElem.Attribute("name").Value,
                    //Select reservoir settings
                    Settings = (from settings in availableResSettings
                                         where settings.SettingsName == reservoirInstanceElem.Attribute("cfg").Value
                                         select settings).FirstOrDefault(),
                    PredictorsCfg = new PredictorsSettings(reservoirInstanceElem.Descendants("predictors").First())
                };
                if (reservoirInstanceDefinition.Settings == null)
                {
                    throw new Exception($"Reservoir settings '{reservoirInstanceElem.Attribute("cfg").Value}' was not found among available settings.");
                }
                //Number of neurons of the largest reservoir
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
                        //Try to find related InputUnit settings elem
                        XElement inputUnitElem = null;
                        XElement inputUnitsElem = reservoirInstanceElem.Descendants("inputUnits").FirstOrDefault();
                        if(inputUnitsElem != null)
                        {
                            foreach(XElement candidateElem in inputUnitsElem.Descendants("inputUnit"))
                            {
                                if(candidateElem.Attribute("fieldName").Value == inputFieldName)
                                {
                                    inputUnitElem = candidateElem;
                                    break;
                                }
                            }
                        }
                        InputUnitSettings ius = null;
                        if(inputUnitElem == null)
                        {
                            ius = new InputUnitSettings(inputFieldName);
                        }
                        else
                        {
                            ius = new InputUnitSettings(inputUnitElem);
                        }
                        //Add new InputFieldInfo
                        reservoirInstanceDefinition.InputUnitCfgCollection.Add(ius);
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
                    //Input coding
                    InputUnit.AnalogCodingMethod coding = InputUnit.ParseAnalogCodingMethod(inputConnectionElem.Attribute("coding").Value);
                    //Use opposite amplitude?
                    bool oppositeAmplitude = bool.Parse(inputConnectionElem.Attribute("oppositeAmplitude").Value);
                    //Signaling restriction
                    NeuronCommon.NeuronSignalingRestrictionType signalingRestriction = NeuronCommon.ParseNeuronSignalingRestriction(inputConnectionElem.Attribute("signalingRestriction").Value);
                    //Input synapse settings
                    InputSynapseSettings synapseCfg = new InputSynapseSettings(inputConnectionElem.Descendants("synapse").First());
                    //Add new assignment
                    reservoirInstanceDefinition.InputConnectionCollection.Add(new ReservoirInstanceDefinition.InputConnection(resInputFieldIdx, targetPoolID, density, coding, oppositeAmplitude, signalingRestriction, synapseCfg));
                }
                ReservoirInstanceDefinitionCollection.Add(reservoirInstanceDefinition);
                ++reservoirInstanceID;
            }
            //Finalize boot cycles if necessary
            if(InputConfig.BootCycles == AutomaticBootCycles && InputConfig.FeedingType == NeuralPreprocessor.InputFeedingType.Continuous)
            {
                InputConfig.BootCycles = numOfNeuronsInLargestReservoir;
            }

            return;
        }

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
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
            public NeuralPreprocessor.InputFeedingType FeedingType { get; set; }

            /// <summary>
            /// Type of attributes organization in the input data in case of patterned input feeding
            /// </summary>
            public InputPattern.TimeOrderVarDataOrganization PatternedInputDataOrganization { get; set; }

            /// <summary>
            /// Specifies if to unify amlitudes of input data in case of patterned input feeding
            /// </summary>
            public bool UnifyAmplitude { get; set; }

            /// <summary>
            /// Specifies if to detrend input data in case of patterned input feeding
            /// </summary>
            public bool Detrend { get; set; }

            /// <summary>
            /// Sensitivity of signal begin detection in case of patterned input feeding
            /// </summary>
            public double ThresholdOfSignalBeginDetection { get; set; }

            /// <summary>
            /// Sensitivity of signal end detection in case of patterned input feeding
            /// </summary>
            public double ThresholdOfSignalEndDetection { get; set; }

            /// <summary>
            /// Specifies if to keep common resampling time-scale in case of patterned input feeding
            /// </summary>
            public bool KeepCommonTimeScale { get; set; }

            /// <summary>
            /// Number of target timepoints of the resampled pattern in case of patterned input feeding
            /// </summary>
            public int TargetTimePoints { get; set; }

            /// <summary>
            /// Parameter is relevant only for patterned feeding and specifies, if to preprocess time series in both time directions.
            /// </summary>
            public bool Bidirectional { get; set; }

            /// <summary>
            /// Number of booting cycles (-1 means Auto)
            /// </summary>
            public int BootCycles { get; set; }

            /// <summary>
            /// The parameter specifies whether the (allowed) input fields will be included among predictors
            /// together with the predictors from the reservoirs.
            /// </summary>
            public bool RouteInputToReadout { get; set; }

            /// <summary>
            /// External input data
            /// </summary>
            public List<ExternalField> ExternalFieldCollection { get; set; }

            /// <summary>
            /// Internal (augmented) input data
            /// </summary>
            public List<InternalField> InternalFieldCollection { get; set; }

            //Constructors
            /// <summary>
            /// Creates an uninitialized instance.
            /// </summary>
            private InputSettings()
            {
                FeedingType = NeuralPreprocessor.InputFeedingType.Continuous;
                UnifyAmplitude = false;
                Detrend = false;
                ThresholdOfSignalBeginDetection = 0d;
                ThresholdOfSignalEndDetection = 0d;
                KeepCommonTimeScale = true;
                TargetTimePoints = -1;
                Bidirectional = false;
                BootCycles = -1;
                RouteInputToReadout = false;
                ExternalFieldCollection = new List<ExternalField>();
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
                UnifyAmplitude = source.UnifyAmplitude;
                Detrend = source.Detrend;
                ThresholdOfSignalBeginDetection = source.ThresholdOfSignalBeginDetection;
                ThresholdOfSignalEndDetection = source.ThresholdOfSignalEndDetection;
                KeepCommonTimeScale = source.KeepCommonTimeScale;
                TargetTimePoints = source.TargetTimePoints;
                Bidirectional = source.Bidirectional;
                BootCycles = source.BootCycles;
                RouteInputToReadout = source.RouteInputToReadout;
                foreach (ExternalField field in source.ExternalFieldCollection)
                {
                    ExternalFieldCollection.Add((ExternalField)field.DeepClone());
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
                    FeedingType = NeuralPreprocessor.InputFeedingType.Continuous;
                    Bidirectional = false;
                    PatternedInputDataOrganization = InputPattern.TimeOrderVarDataOrganization.Groupped;
                    //Number of booting cycles
                    string bootCyclesAttrValue = feedingElem.Attribute("bootCycles").Value;
                    if (bootCyclesAttrValue == "Auto")
                    {
                        //Boot cycles will be set automatically later
                        BootCycles = NeuralPreprocessorSettings.AutomaticBootCycles;
                    }
                    else
                    {
                        BootCycles = int.Parse(bootCyclesAttrValue, CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    FeedingType = NeuralPreprocessor.InputFeedingType.Patterned;
                    Bidirectional = bool.Parse(feedingElem.Attribute("bidir").Value);
                    PatternedInputDataOrganization = InputPattern.ParseTimeOrderVarDataOrganization(feedingElem.Attribute("dataOrganization").Value);
                    BootCycles = 0;
                    XElement unificationElem = feedingElem.Descendants("unification").First();
                    UnifyAmplitude = bool.Parse(unificationElem.Attribute("unifyAmplitude").Value);
                    Detrend = bool.Parse(unificationElem.Attribute("detrend").Value);
                    XElement resamplingElem = unificationElem.Descendants("resampling").First();
                    ThresholdOfSignalBeginDetection = double.Parse(resamplingElem.Attribute("thresholdOfSignalBeginDetection").Value, CultureInfo.InvariantCulture);
                    ThresholdOfSignalEndDetection = double.Parse(resamplingElem.Attribute("thresholdOfSignalEndDetection").Value, CultureInfo.InvariantCulture);
                    KeepCommonTimeScale = bool.Parse(resamplingElem.Attribute("keepCommonTimeScale").Value);
                    TargetTimePoints = -1;
                    if (resamplingElem.Attribute("targetTimePoints").Value != "Auto")
                    {
                        TargetTimePoints = int.Parse(resamplingElem.Attribute("targetTimePoints").Value, CultureInfo.InvariantCulture);
                    }
                }
                //Routing of input to readout layer
                RouteInputToReadout = bool.Parse(feedingElem.Attribute("routeToReadout").Value);
                //Fields
                Dictionary<string, string> uniquenessChecker = new Dictionary<string, string>();
                //External fields
                foreach (XElement fieldElem in settingsElem.Descendants("external").First().Descendants("field"))
                {
                    string fieldName = fieldElem.Attribute("name").Value;
                    bool allowRoutingToReadout = bool.Parse(fieldElem.Attribute("allowRoutingToReadout").Value);
                    if (uniquenessChecker.ContainsKey(fieldName))
                    {
                        throw new Exception($"Duplicit input field name {fieldName}");
                    }
                    uniquenessChecker.Add(fieldName, fieldName);
                    ExternalFieldCollection.Add(new ExternalField(fieldName,
                                                                  allowRoutingToReadout,
                                                                  fieldElem.Descendants().FirstOrDefault()
                                                                  ));
                }
                //Internal fields
                XElement intFieldsElem = settingsElem.Descendants("internal").FirstOrDefault();
                if (intFieldsElem != null)
                {
                    foreach (XElement fieldElem in intFieldsElem.Descendants("field"))
                    {
                        string fieldName = fieldElem.Attribute("name").Value;
                        bool allowRoutingToReadout = bool.Parse(fieldElem.Attribute("allowRoutingToReadout").Value);
                        if (uniquenessChecker.ContainsKey(fieldName))
                        {
                            throw new Exception($"Duplicit input field name: {fieldName}");
                        }
                        uniquenessChecker.Add(fieldName, fieldName);
                        InternalFieldCollection.Add(new InternalField(fieldName,
                                                                      allowRoutingToReadout,
                                                                      fieldElem.Descendants().First()
                                                                      ));
                    }
                }
                return;
            }

            //Properties
            /// <summary>
            /// Total number of Neural Preprocessor input fields
            /// </summary>
            public int NumOfFields { get { return ExternalFieldCollection.Count + InternalFieldCollection.Count; } }

            //Static methods

            //Methods
            /// <summary>
            /// Function searches for index of the specified field among all Neural Preprocessor input fields
            /// </summary>
            /// <param name="fieldName">Name of the searched field</param>
            /// <returns>Zero based index of the field.</returns>
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
                throw new Exception($"Input field {fieldName} not found among NP input fields.");
            }

            /// <summary>
            /// Function searches for the specified field among all Neural Preprocessor input fields
            /// </summary>
            /// <param name="fieldName">Name of the searched field</param>
            public Field GetField(string fieldName)
            {
                for (int i = 0; i < ExternalFieldCollection.Count; i++)
                {
                    if (ExternalFieldCollection[i].Name == fieldName)
                    {
                        return ExternalFieldCollection[i];
                    }
                }
                for (int i = 0; i < InternalFieldCollection.Count; i++)
                {
                    if (InternalFieldCollection[i].Name == fieldName)
                    {
                        return InternalFieldCollection[i];
                    }
                }
                return null;
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
            /// Returns collection of input field names to be routed to readout layer
            /// </summary>
            /// <returns></returns>
            public List<string> RoutedFieldNameCollection()
            {
                List<string> result = new List<string>();
                if(RouteInputToReadout)
                {
                    result.AddRange(from item in ExternalFieldCollection where item.AllowRoutingToReadout select item.Name);
                    result.AddRange(from item in InternalFieldCollection where item.AllowRoutingToReadout select item.Name);
                }
                return result;
            }

            /// <summary>
            /// Creates the deep copy instance of this instance
            /// </summary>
            /// <returns></returns>
            public InputSettings DeepClone()
            {
                return new InputSettings(this);
            }

            //Inner classes
            /// <summary>
            /// Base class of fields
            /// </summary>
            [Serializable]
            public class Field
            {
                //Constants

                //Attribute properties
                /// <summary>
                /// Field name
                /// </summary>
                public string Name { get; set; }

                /// <summary>
                /// The parameter specifies whether the field can be included among predictors
                /// together with the predictors from the reservoirs.
                /// </summary>
                public bool AllowRoutingToReadout { get; set; }

                //Constructors
                /// <summary>
                /// Creates an initialized instance
                /// </summary>
                /// <param name="name">Field name</param>
                /// <param name="allowRoutingToReadout">Specifies whether the field can be included among predictors</param>
                public Field(string name, bool allowRoutingToReadout)
                {
                    Name = name;
                    AllowRoutingToReadout = allowRoutingToReadout;
                    return;
                }

                /// <summary>
                /// The deep copy constructor.
                /// </summary>
                /// <param name="source">Source instance</param>
                public Field(Field source)
                {
                    Name = source.Name;
                    AllowRoutingToReadout = source.AllowRoutingToReadout;
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

            }//Field

            /// <summary>
            /// Represents external input field
            /// </summary>
            [Serializable]
            public class ExternalField : Field
            {
                //Attribute properties
                /// <summary>
                /// Feature filter configuration
                /// </summary>
                public IFeatureFilterSettings FeatureFilterCfg { get; set; }

                //Constructors
                /// <summary>
                /// Creates an initialized instance
                /// </summary>
                /// <param name="name">Field name</param>
                /// <param name="allowRoutingToReadout">Specifies whether the field can be included among predictors</param>
                /// <param name="settingsElem">Xml element containing associated feature filter settings</param>
                public ExternalField(string name,
                                     bool allowRoutingToReadout,
                                     XElement settingsElem
                                     )
                    : base(name, allowRoutingToReadout)
                {
                    FeatureFilterCfg = FeatureFilterFactory.LoadSettings(settingsElem);
                    return;
                }

                /// <summary>
                /// The deep copy constructor.
                /// </summary>
                /// <param name="source">Source instance</param>
                public ExternalField(ExternalField source)
                    : base(source)
                {
                    FeatureFilterCfg = FeatureFilterFactory.DeepClone(source.FeatureFilterCfg);
                    return;
                }

                //Methods
                /// <summary>
                /// Creates the deep copy instance of this instance
                /// </summary>
                /// <returns></returns>
                public override Field DeepClone()
                {
                    return new ExternalField(this);
                }

            }//ExternalField


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
                /// <param name="allowRoutingToReadout">Specifies whether the field can be included among predictors</param>
                /// <param name="settingsElem">Xml element containing associated signal generator settings</param>
                public InternalField(string name,
                                     bool allowRoutingToReadout,
                                     XElement settingsElem
                                     )
                    :base(name, allowRoutingToReadout)
                {
                    switch(settingsElem.Name.LocalName)
                    {
                        case "pulse":
                            GeneratorSettings = new PulseGeneratorSettings(settingsElem);
                            break;
                        case "random":
                            GeneratorSettings = new RandomValueSettings(settingsElem);
                            break;
                        case "sinusoidal":
                            GeneratorSettings = new SinusoidalGeneratorSettings(settingsElem);
                            break;
                        case "mackeyGlass":
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
                    if(source.GeneratorSettings.GetType() == typeof(PulseGeneratorSettings))
                    {
                        GeneratorSettings = ((PulseGeneratorSettings)source.GeneratorSettings).DeepClone();
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
            /// Index of the reservoir instance within the Neural Preprocessor
            /// </summary>
            public int InstanceID { get; set; }
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
            /// Collection of input unit configurations
            /// </summary>
            public List<InputUnitSettings> InputUnitCfgCollection { get; set; }
            /// <summary>
            /// Connections of the Reservoir's input fields to the Reservoir's pools.
            /// </summary>
            public List<InputConnection> InputConnectionCollection { get; set; }
            /// <summary>
            /// Configuration of the predictors
            /// </summary>
            public PredictorsSettings PredictorsCfg { get; set; }

            //Constructors
            /// <summary>
            /// Creates an unitialized instance.
            /// </summary>
            public ReservoirInstanceDefinition()
            {
                InstanceID = 0;
                InstanceName = string.Empty;
                Settings = null;
                PredictorsCfg = null;
                InputUnitCfgCollection = new List<InputUnitSettings>();
                InputConnectionCollection = new List<InputConnection>();
                return;
            }

            /// <summary>
            /// The deep copy constructor.
            /// </summary>
            /// <param name="source">Source instance</param>
            public ReservoirInstanceDefinition(ReservoirInstanceDefinition source)
            {
                InstanceID = source.InstanceID;
                InstanceName = source.InstanceName;
                Settings = (ReservoirSettings)source.Settings.DeepClone();
                InputUnitCfgCollection = new List<InputUnitSettings>(source.InputUnitCfgCollection.Count);
                foreach(InputUnitSettings ifi in source.InputUnitCfgCollection)
                {
                    InputUnitCfgCollection.Add((InputUnitSettings)ifi.DeepClone());
                }
                InputConnectionCollection = new List<InputConnection>(source.InputConnectionCollection.Count);
                foreach(InputConnection ifa in source.InputConnectionCollection)
                {
                    InputConnectionCollection.Add(ifa.DeepClone());
                }
                PredictorsCfg = (PredictorsSettings)source.PredictorsCfg?.DeepClone();
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
                /// Analog coding method
                /// </summary>
                public InputUnit.AnalogCodingMethod Coding { get; set; }
                /// <summary>
                /// Use opposite amplitude?
                /// </summary>
                public bool OppositeAmplitude { get; set; }
                /// <summary>
                /// Restriction of signaling of associated synapses
                /// </summary>
                public NeuronCommon.NeuronSignalingRestrictionType SignalingRestriction { get; set; }
                /// <summary>
                /// Input neuron to pool's neuron synapse settings
                /// </summary>
                public InputSynapseSettings SynapseCfg { get; set; }

                //Constructors
                /// <summary>
                /// Creates an itialized instance.
                /// </summary>
                /// <param name="fieldIdx">Index of the reservoir input field</param>
                /// <param name="poolID">ID of the target pool</param>
                /// <param name="density">Input field connection density</param>
                /// <param name="coding">Analog coding method</param>
                /// <param name="oppositeAmplitude">Specifies if to use opposite amplitude</param>
                /// <param name="signalingRestriction">Restriction of signaling of associated synapses</param>
                /// <param name="synapseCfg">Input neuron to pool's neuron synapse settings</param>
                public InputConnection(int fieldIdx,
                                       int poolID,
                                       double density,
                                       InputUnit.AnalogCodingMethod coding,
                                       bool oppositeAmplitude,
                                       NeuronCommon.NeuronSignalingRestrictionType signalingRestriction,
                                       InputSynapseSettings synapseCfg
                                       )
                {
                    FieldIdx = fieldIdx;
                    PoolID = poolID;
                    Density = density;
                    Coding = coding;
                    OppositeAmplitude = oppositeAmplitude;
                    SignalingRestriction = signalingRestriction;
                    SynapseCfg = (InputSynapseSettings)synapseCfg.DeepClone();
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
                    Coding = source.Coding;
                    OppositeAmplitude = source.OppositeAmplitude;
                    SignalingRestriction = source.SignalingRestriction;
                    SynapseCfg = (InputSynapseSettings)source.SynapseCfg.DeepClone();
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

            }//InputConnection

        }//ReservoirInstanceDefinition

    }//NeuralPreprocessorSettings

}//Namespace
