using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Xml.Linq;
using System.Reflection;
using RCNet.Extensions;
using RCNet.Neural.Analog.Reservoir;
using RCNet.Neural.Analog.Readout;
using RCNet.XmlTools;

namespace RCNet.Neural.Analog.Network.EchoState
{
    /// <summary>
    /// The class contains Esn (Echo State Network) configuration parameters and also contains
    /// internal logic so it is not just a container of parameters. Creating an proper instance by hand is not
    /// a trivial task.
    /// The easiest and safest way to create an instance is to use the xml constructor.
    /// </summary>
    [Serializable]
    public class EsnSettings
    {
        //Attribute properties
        /// <summary>
        /// Type of the task for which is Esn designed
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
        /// The collection of Esn input field names in order of how they will be pushed to the network.
        /// </summary>
        public List<string> InputFieldNameCollection { get; set; }
        /// <summary>
        /// Collection of definitions for future instances of internal reservoirs.
        /// Each definition contains a specific setting for the reservoir and maps the input
        /// fields of the reservoir to the input fields of the Esn network.
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
        public EsnSettings()
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
        public EsnSettings(EsnSettings source)
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
        /// This is the preferred way to instantiate Esn settings.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing Esn settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public EsnSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Analog.Network.EchoState.EsnSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement esnSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            //Task type
            TaskType = CommonEnums.ParseTaskType(esnSettingsElem.Attribute("taskType").Value);
            //Randomizer seek
            RandomizerSeek = int.Parse(esnSettingsElem.Attribute("randomizerSeek").Value);
            //Input fields
            XElement inputFieldsElem = esnSettingsElem.Descendants("inputFields").First();
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
            List<AnalogReservoirSettings> availableResSettings = new List<AnalogReservoirSettings>();
            XElement reservoirSettingsContainerElem = esnSettingsElem.Descendants("reservoirCfgContainer").First();
            foreach (XElement reservoirSettingsElem in reservoirSettingsContainerElem.Descendants("reservoirCfg"))
            {
                availableResSettings.Add(new AnalogReservoirSettings(reservoirSettingsElem));
            }
            //Readout layer
            XElement readoutLayerElem = esnSettingsElem.Descendants("readoutLayer").First();
            ReadoutLayerConfig = new ReadoutLayerSettings(readoutLayerElem);
            //Mapping of input fields to reservoir settings (future reservoir instance)
            ReservoirInstanceDefinitionCollection = new List<ReservoirInstanceDefinition>();
            XElement reservoirInstancesContainerElem = esnSettingsElem.Descendants("reservoirInstanceContainer").First();
            foreach (XElement reservoirInstanceElem in reservoirInstancesContainerElem.Descendants("reservoirInstance"))
            {
                ReservoirInstanceDefinition newMap = new ReservoirInstanceDefinition();
                newMap.InstanceName = reservoirInstanceElem.Attribute("name").Value;
                newMap.AugmentedStates = bool.Parse(reservoirInstanceElem.Attribute("augmentedStates").Value);
                //Select reservoir settings
                newMap.ReservoirSettings = (from settings in availableResSettings
                                            where settings.SettingsName == reservoirInstanceElem.Attribute("cfg").Value
                                            select settings).FirstOrDefault();
                if(newMap.ReservoirSettings == null)
                {
                    throw new Exception($"Reservoir settings '{reservoirInstanceElem.Attribute("cfg").Value}' was not found among available settings.");
                }
                //Associated Esn input fields
                foreach (XElement inputFieldElem in reservoirInstanceElem.Descendants("inputFields").First().Descendants("field"))
                {
                    string inputFieldName = inputFieldElem.Attribute("name").Value;
                    //Index in InputFieldsNames
                    int inputFieldIdx = InputFieldNameCollection.IndexOf(inputFieldName);
                    //Found?
                    if (inputFieldIdx < 0)
                    {
                        //Not found
                        throw new Exception($"Reservoir instance {newMap.InstanceName}: input field {inputFieldName} is not defined among Esn input fields.");
                    }
                    newMap.InputFieldMappingCollection.Add(inputFieldIdx);
                }
                //Associated Esn output fields tor feedback
                if (newMap.ReservoirSettings.FeedbackFeature)
                {
                    foreach (string feedbackFieldName in newMap.ReservoirSettings.FeedbackFieldNameCollection)
                    {
                        if (TaskType != CommonEnums.TaskType.Prediction)
                        {
                            throw new Exception($"Reservoir instance {newMap.InstanceName}: feedback fields are allowed for prediction task type only.");
                        }
                        //Index in OutputFieldsNames
                        int feedbackFieldIdx = ReadoutLayerConfig.OutputFieldNameCollection.IndexOf(feedbackFieldName);
                        //Found?
                        if (feedbackFieldIdx < 0)
                        {
                            //Not found
                            throw new Exception($"Reservoir instance {newMap.InstanceName}: feedback field {feedbackFieldName} is not defined among Esn output fields.");
                        }
                        newMap.FeedbackFieldMappingCollection.Add(feedbackFieldIdx);
                    }
                }
                ReservoirInstanceDefinitionCollection.Add(newMap);
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
            EsnSettings cmpSettings = obj as EsnSettings;
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
        public EsnSettings DeepClone()
        {
            EsnSettings clone = new EsnSettings(this);
            return clone;
        }

        //Inner classes
        /// <summary>
        /// Definition of future instance of Esn internal reservoir.
        /// Definition contains a specific setting for the reservoir and maps the input
        /// fields of the reservoir to the input fields of the Esn network.
        /// </summary>
        [Serializable]
        public class ReservoirInstanceDefinition
        {
            //Attribute properties
            /// <summary>
            /// Maps the reservoir input field to the input field of the Esn.
            /// Each collection entry means reservoir input field and int value
            /// is the index of the field within EsnSettings.InputFieldNameCollection
            /// </summary>
            public List<int> InputFieldMappingCollection {get; set;}
            /// <summary>
            /// Maps the reservoir feedback field to the output field of the Esn.
            /// Each collection entry means reservoir feedback field and int value
            /// is the index of the field within EsnSettings.OutputFieldNameCollection
            /// </summary>
            public List<int> FeedbackFieldMappingCollection { get; set; }
            /// <summary>
            /// Name of the reservoir instance. It is useful for logging and visualization
            /// purposes so instance name should be unique within the Esn.
            /// </summary>
            public string InstanceName { get; set; }
            /// <summary>
            /// ReservoirSettings of the reservoir instance.
            /// </summary>
            public AnalogReservoirSettings ReservoirSettings { get; set; }
            /// <summary>
            /// The parameter specifies whether, in addition to the standard neuron states,
            /// augmented states of reservoir neurons will be added to the reservoir output predictors.
            /// Augmented states double the number of output predictors of the reservoir.
            /// The augmented state of the neuron is its squared state.
            /// </summary>
            public bool AugmentedStates { get; set; }

            //Constructors
            /// <summary>
            /// Creates an unitialized instance.
            /// </summary>
            public ReservoirInstanceDefinition()
            {
                InputFieldMappingCollection = new List<int>();
                FeedbackFieldMappingCollection = new List<int>();
                InstanceName = string.Empty;
                ReservoirSettings = null;
                AugmentedStates = false;
                return;
            }

            /// <summary>
            /// The deep copy constructor.
            /// </summary>
            /// <param name="source">Source instance</param>
            public ReservoirInstanceDefinition(ReservoirInstanceDefinition source)
            {
                InputFieldMappingCollection = new List<int>(source.InputFieldMappingCollection);
                FeedbackFieldMappingCollection = new List<int>(source.FeedbackFieldMappingCollection);
                InstanceName = source.InstanceName;
                ReservoirSettings = source.ReservoirSettings.DeepClone();
                AugmentedStates = source.AugmentedStates;
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
                if (InputFieldMappingCollection.Count != cmpSettings.InputFieldMappingCollection.Count ||
                    InstanceName != cmpSettings.InstanceName ||
                    !Equals(ReservoirSettings, cmpSettings.ReservoirSettings) ||
                    AugmentedStates != cmpSettings.AugmentedStates ||
                    !InputFieldMappingCollection.ToArray().ContainsEqualValues(cmpSettings.InputFieldMappingCollection.ToArray()) ||
                    !FeedbackFieldMappingCollection.ToArray().ContainsEqualValues(cmpSettings.FeedbackFieldMappingCollection.ToArray())
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
                return InstanceName.GetHashCode();
            }

        }//ReservoirInstanceDefinition

    }//EsnSettings

}//Namespace
