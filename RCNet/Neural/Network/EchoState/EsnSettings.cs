using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Xml.Linq;
using System.Reflection;
using System.IO;
using RCNet.Extensions;
using RCNet.Neural.Activation;
using RCNet.Neural.Network.FF;
using RCNet.XmlTools;

namespace RCNet.Neural.Network.EchoState
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
        /// <summary>
        /// Supported task types which can be solved by Esn.
        /// </summary>
        public enum Purpose
        {
            /// <summary>
            /// Time series prediction
            /// </summary>
            TimeSeriesPrediction,
            /// <summary>
            /// Pattern recognition
            /// </summary>
            Categorization
        }

        //Attribute properties
        /// <summary>
        /// Type of the task for which is Esn designed
        /// </summary>
        public Purpose TaskType { get; set; }
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
        /// Collection of hidden layer definitions. Hidden layers are optional and can be used in the output
        /// feed forward network that process predictors from reservoirs and computes Esn output field
        /// value (for each output field is instantiated and trained alone feed forward network).
        /// Defining hidden layers is useful in some specific cases.
        /// In most cases, hidden layers are not defined and the output feed forward network
        /// contains only the output neuron.
        /// </summary>
        public List<HiddenLayerSettings> HiddenLayerCollection { get; set; }
        /// <summary>
        /// Activation function of the output neuron calculating the output value of the Esn network.
        /// This neuron is the output layer of the feed forward network processing predictors
        /// from the reservoirs (for each output field is instantiated and trained alone output feed forward network).
        /// </summary>
        public ActivationFactory.ActivationType OutputNeuronActivation { get; set; }
        /// <summary>
        /// The parameter specifies what method will be used for training
        /// the output feed forward network.
        /// </summary>
        public TrainingMethodType RegressionMethod { get; set; }
        /// <summary>
        /// Number of regression attempts.
        /// </summary>
        public int RegressionAttempts { get; set; }
        /// <summary>
        /// Number of iterations (epochs) during regression attempt.
        /// </summary>
        public int RegressionAttemptEpochs { get; set; }
        /// <summary>
        /// Regression attempt will be stopped after the specified
        /// MSE on training dataset will be reached.
        /// </summary>
        public double RegressionAttemptStopMSE { get; set; }
        /// <summary>
        /// The collection of Esn output field names in order of how they will be predicted by Esn.
        /// </summary>
        public List<string> OutputFieldNameCollection { get; set; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        public EsnSettings()
        {
            //Default settings
            TaskType = Purpose.TimeSeriesPrediction;
            RandomizerSeek = 0;
            InputFieldNameCollection = new List<string>();
            ReservoirInstanceDefinitionCollection = new List<ReservoirInstanceDefinition>();
            RouteInputToReadout = false;
            HiddenLayerCollection = new List<HiddenLayerSettings>();
            OutputNeuronActivation = ActivationFactory.ActivationType.Identity;
            RegressionMethod = TrainingMethodType.Linear;
            RegressionAttempts = 0;
            RegressionAttemptEpochs = 0;
            RegressionAttemptStopMSE = 0;
            OutputFieldNameCollection = new List<string>();
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
            HiddenLayerCollection = new List<HiddenLayerSettings>(source.HiddenLayerCollection.Count);
            foreach (HiddenLayerSettings hiddenLayerSettings in source.HiddenLayerCollection)
            {
                HiddenLayerCollection.Add(hiddenLayerSettings.DeepClone());
            }
            OutputNeuronActivation = source.OutputNeuronActivation;
            RegressionMethod = source.RegressionMethod;
            RegressionAttempts = source.RegressionAttempts;
            RegressionAttemptEpochs = source.RegressionAttemptEpochs;
            RegressionAttemptStopMSE = source.RegressionAttemptStopMSE;
            OutputFieldNameCollection = new List<string>(source.OutputFieldNameCollection);
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// This is the preferred way to instantiate Esn settings.
        /// </summary>
        /// <param name="esnSettingsElem">
        /// Xml data containing Esn settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public EsnSettings(XElement esnSettingsElem)
        {
            //Validation
            //A very ugly validation. Xml schema does not support validation of the xml fragment against specific type.
            XmlValidator validator = new XmlValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            using (Stream schemaStream = assemblyRCNet.GetManifestResourceStream("RCNet.Neural.Network.EchoState.EsnSettings.xsd"))
            {
                validator.AddSchema(schemaStream);
            }
            using (Stream schemaStream = assemblyRCNet.GetManifestResourceStream("RCNet.NeuralSettingsTypes.xsd"))
            {
                validator.AddSchema(schemaStream);
            }
            validator.LoadXDocFromString(esnSettingsElem.ToString());
            //Parsing
            //Task type
            TaskType = ParsePurpose(esnSettingsElem.Attribute("TaskType").Value);
            //Randomizer seek
            RandomizerSeek = int.Parse(esnSettingsElem.Attribute("RandomizerSeek").Value);
            //Input fields
            XElement inputFieldsElem = esnSettingsElem.Descendants("InputFields").First();
            RouteInputToReadout = bool.Parse(inputFieldsElem.Attributes("RouteToReadout").FirstOrDefault().Value);
            if(TaskType == Purpose.Categorization && RouteInputToReadout)
            {
                throw new Exception("For categorization task setup is not allowed to route input to readout because of possible variable length of the input.");
            }
            InputFieldNameCollection = new List<string>();
            foreach(XElement inputFieldElem in inputFieldsElem.Descendants("Field"))
            {
                InputFieldNameCollection.Add(inputFieldElem.Attribute("Name").Value);
            }
            //Collect available reservoir settings
            List<AnalogReservoirSettings> availableResSettings = new List<AnalogReservoirSettings>();
            XElement reservoirSettingsContainerElem = esnSettingsElem.Descendants("ReservoirSettingsContainer").First();
            foreach (XElement reservoirSettingsElem in reservoirSettingsContainerElem.Descendants("AnalogReservoirSettings"))
            {
                availableResSettings.Add(new AnalogReservoirSettings(reservoirSettingsElem));
            }
            //Readout
            XElement readoutElem = esnSettingsElem.Descendants("Readout").First();
            OutputNeuronActivation = ActivationFactory.ParseActivation(readoutElem.Attribute("OutputActivation").Value);
            RegressionMethod = FeedForwardNetwork.ParseTrainingMethodType(readoutElem.Attribute("RegressionMethod").Value);
            RegressionAttempts = int.Parse(readoutElem.Attribute("Attempts").Value);
            RegressionAttemptEpochs = int.Parse(readoutElem.Attribute("AttemptEpochs").Value);
            RegressionAttemptStopMSE = double.Parse(readoutElem.Attribute("AttemptStopMSE").Value, CultureInfo.InvariantCulture);
            //Hidden layers
            HiddenLayerCollection = new List<HiddenLayerSettings>();
            foreach (XElement hiddenLayerElem in readoutElem.Descendants("Layer"))
            {
                HiddenLayerCollection.Add(new HiddenLayerSettings(hiddenLayerElem));
            }
            //Output fields
            XElement outputFieldsElem = esnSettingsElem.Descendants("OutputFields").First();
            OutputFieldNameCollection = new List<string>();
            foreach (XElement outputFieldElem in outputFieldsElem.Descendants("Field"))
            {
                OutputFieldNameCollection.Add(outputFieldElem.Attribute("Name").Value);
            }
            //Mapping of input fields to reservoir settings (future reservoir instance)
            ReservoirInstanceDefinitionCollection = new List<ReservoirInstanceDefinition>();
            XElement reservoirInstancesContainerElem = esnSettingsElem.Descendants("ReservoirInstancesContainer").First();
            foreach (XElement reservoirInstanceElem in reservoirInstancesContainerElem.Descendants("ReservoirInstance"))
            {
                ReservoirInstanceDefinition newMap = new ReservoirInstanceDefinition();
                newMap.InstanceName = reservoirInstanceElem.Attribute("InstanceName").Value;
                newMap.AugmentedStates = bool.Parse(reservoirInstanceElem.Attribute("AugmentedStates").Value);
                //Select reservoir settings
                newMap.ReservoirSettings = (from settings in availableResSettings
                                            where settings.SettingsName == reservoirInstanceElem.Attribute("SettingsName").Value
                                            select settings).First();
                //Associated Esn input fields
                foreach (XElement inputFieldElem in reservoirInstanceElem.Descendants("InputField"))
                {
                    string inputFieldName = inputFieldElem.Attribute("Name").Value;
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
                foreach (string feedbackFieldName in newMap.ReservoirSettings.FeedbackFieldNameCollection)
                {
                    if(TaskType == Purpose.Categorization)
                    {
                        throw new Exception($"Reservoir instance {newMap.InstanceName}: feedback fields are not allowed for categorization task type.");
                    }
                    //Index in OutputFieldsNames
                    int feedbackFieldIdx = OutputFieldNameCollection.IndexOf(feedbackFieldName);
                    //Found?
                    if (feedbackFieldIdx < 0)
                    {
                        //Not found
                        throw new Exception($"Reservoir instance {newMap.InstanceName}: feedback field {feedbackFieldName} is not defined among Esn output fields.");
                    }
                    newMap.FeedbackFieldMappingCollection.Add(feedbackFieldIdx);
                }
                ReservoirInstanceDefinitionCollection.Add(newMap);
            }

            return;
        }

        //Methods
        public Purpose ParsePurpose(string code)
        {
            switch(code.ToUpper())
            {
                case "TIMESERIESPREDICTION": return Purpose.TimeSeriesPrediction;
                case "CATEGORIZATION": return Purpose.Categorization;
                default:
                    throw new ArgumentException($"Unknown task type {code}", "code");
            }
        }
        
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            EsnSettings cmpSettings = obj as EsnSettings;
            if (RandomizerSeek != cmpSettings.RandomizerSeek ||
               !InputFieldNameCollection.ToArray().ContainsEqualValues(cmpSettings.InputFieldNameCollection.ToArray()) ||
               ReservoirInstanceDefinitionCollection.Count != cmpSettings.ReservoirInstanceDefinitionCollection.Count ||
               RouteInputToReadout != cmpSettings.RouteInputToReadout ||
               OutputNeuronActivation != cmpSettings.OutputNeuronActivation ||
               RegressionMethod != cmpSettings.RegressionMethod ||
               RegressionAttempts != cmpSettings.RegressionAttempts ||
               RegressionAttemptEpochs != cmpSettings.RegressionAttemptEpochs ||
               RegressionAttemptStopMSE != cmpSettings.RegressionAttemptStopMSE ||
               HiddenLayerCollection.Count != cmpSettings.HiddenLayerCollection.Count
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
            for (int i = 0; i < HiddenLayerCollection.Count; i++)
            {
                if (!HiddenLayerCollection[i].Equals(cmpSettings.HiddenLayerCollection[i]))
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
                    FeedbackFieldMappingCollection.Count != cmpSettings.FeedbackFieldMappingCollection.Count ||
                    InstanceName != cmpSettings.InstanceName ||
                    !ReservoirSettings.Equals(cmpSettings.ReservoirSettings) ||
                    AugmentedStates != cmpSettings.AugmentedStates
                    )
                {
                    return false;
                }
                for (int i = 0; i < InputFieldMappingCollection.Count; i++)
                {
                    if (InputFieldMappingCollection[i] != cmpSettings.InputFieldMappingCollection[i])
                    {
                        return false;
                    }
                }
                for (int i = 0; i < FeedbackFieldMappingCollection.Count; i++)
                {
                    if (FeedbackFieldMappingCollection[i] != cmpSettings.FeedbackFieldMappingCollection[i])
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

        }//ReservoirInstanceDefinition

    }//ESNSettings

}//Namespace
