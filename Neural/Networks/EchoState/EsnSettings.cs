using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Xml.Linq;
using System.Reflection;
using RCNet.Extensions;
using RCNet.Neural.Activation;
using RCNet.Neural.Networks.FF;
using RCNet.XmlTools;

namespace RCNet.Neural.Networks.EchoState
{
    /// <summary>Echo State Network general settings</summary>
    [Serializable]
    public sealed class EsnSettings
    {
        //Attribute properties
        /// <summary>
        /// RandomizerSeek greater or equal to 0 causes the same "Random" initialization (useful for parameters tuning).
        /// Specify randomizerSeek less than 0 for different "Random" initialization every time the Esn is instatiating.
        /// </summary>
        public int RandomizerSeek { get; set; }
        /// <summary>Input fields names.</summary>
        public List<string> InputFieldsNames { get; set; }
        /// <summary>List of future reservoir instances</summary>
        public List<Input2ResSettingsMap> Input2ResSettingsMapCollection { get; set; }
        /// <summary>If true, unmodified input values will be added as a part of the Esn regression predictors</summary>
        public bool RouteInputToReadout { get; set; }
        /// <summary>Readout hidden layers</summary>
        public List<HiddenLayerSettings> ReadOutHiddenLayers { get; set; }
        /// <summary>Readout output neuron activation function.</summary>
        public ActivationFactory.ActivationType OutputNeuronActivation { get; set; }
        /// <summary>Readout regression method to be used.</summary>
        public TrainingMethodType RegressionMethod { get; set; }
        /// <summary>Maximum number of regression attempts.</summary>
        public int RegressionAttempts { get; set; }
        /// <summary>Maximum number of iterations during regression attempt.</summary>
        public int RegressionAttemptEpochs { get; set; }
        /// <summary>Regression attempt will be stopped after the specified MSE on training dataset will be reached.</summary>
        public double RegressionAttemptStopMSE { get; set; }
        /// <summary>Esn output fields names (predictions).</summary>
        public List<string> OutputFieldsNames { get; set; }

        //Constructors
        /// <summary>Creates uninitialized Esn settings instance</summary>
        public EsnSettings()
        {
            //Default settings
            RandomizerSeek = 0;
            InputFieldsNames = new List<string>();
            Input2ResSettingsMapCollection = new List<Input2ResSettingsMap>();
            RouteInputToReadout = false;
            ReadOutHiddenLayers = new List<HiddenLayerSettings>();
            OutputNeuronActivation = ActivationFactory.ActivationType.Identity;
            RegressionMethod = TrainingMethodType.Linear;
            RegressionAttempts = 0;
            RegressionAttemptEpochs = 0;
            RegressionAttemptStopMSE = 0;
            OutputFieldsNames = new List<string>();
            return;
        }

        /// <summary>
        /// Creates this instance as a deep copy of source instance
        /// </summary>
        /// <param name="source">Source settings</param>
        public EsnSettings(EsnSettings source)
        {
            //Copy
            RandomizerSeek = source.RandomizerSeek;
            InputFieldsNames = new List<string>(source.InputFieldsNames);
            Input2ResSettingsMapCollection = new List<Input2ResSettingsMap>(source.Input2ResSettingsMapCollection.Count);
            foreach (Input2ResSettingsMap mapping in source.Input2ResSettingsMapCollection)
            {
                Input2ResSettingsMapCollection.Add(mapping.DeepClone());
            }
            RouteInputToReadout = source.RouteInputToReadout;
            ReadOutHiddenLayers = new List<HiddenLayerSettings>(source.ReadOutHiddenLayers.Count);
            foreach (HiddenLayerSettings hiddenLayerSettings in source.ReadOutHiddenLayers)
            {
                ReadOutHiddenLayers.Add(hiddenLayerSettings.DeepClone());
            }
            OutputNeuronActivation = source.OutputNeuronActivation;
            RegressionMethod = source.RegressionMethod;
            RegressionAttempts = source.RegressionAttempts;
            RegressionAttemptEpochs = source.RegressionAttemptEpochs;
            RegressionAttemptStopMSE = source.RegressionAttemptStopMSE;
            OutputFieldsNames = new List<string>(source.OutputFieldsNames);
            return;
        }

        /// <summary>
        /// Creates instance and initialize it from given xml element
        /// </summary>
        /// <param name="esnSettingsElem">Xml element containing esn settings</param>
        public EsnSettings(XElement esnSettingsElem)
        {
            //Validation
            //A very ugly validation
            XmlValidator validator = new XmlValidator();
            Assembly neuralAssembly = Assembly.Load("Neural");
            validator.AddSchema(neuralAssembly.GetManifestResourceStream("RCNet.Neural.Networks.EchoState.EsnSettings.xsd"));
            validator.AddSchema(neuralAssembly.GetManifestResourceStream("RCNet.Neural.NeuralSettingsTypes.xsd"));
            validator.LoadXDocFromString(esnSettingsElem.ToString());
            //Parsing
            //Randomizer seek
            RandomizerSeek = int.Parse(esnSettingsElem.Attribute("RandomizerSeek").Value);
            //Input fields
            XElement inputFieldsElem = esnSettingsElem.Descendants("InputFields").First();
            RouteInputToReadout = bool.Parse(inputFieldsElem.Attribute("RouteToReadout").Value);
            InputFieldsNames = new List<string>();
            foreach(XElement inputFieldElem in inputFieldsElem.Descendants("Field"))
            {
                InputFieldsNames.Add(inputFieldElem.Attribute("Name").Value);
            }
            //Available reservoir settings
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
            //Readout hidden layers
            ReadOutHiddenLayers = new List<HiddenLayerSettings>();
            foreach (XElement hiddenLayerElem in readoutElem.Descendants("Layer"))
            {
                ReadOutHiddenLayers.Add(new HiddenLayerSettings(hiddenLayerElem));
            }
            //Output fields
            XElement outputFieldsElem = esnSettingsElem.Descendants("OutputFields").First();
            OutputFieldsNames = new List<string>();
            foreach (XElement outputFieldElem in outputFieldsElem.Descendants("Field"))
            {
                OutputFieldsNames.Add(outputFieldElem.Attribute("Name").Value);
            }
            //Mapping of input fields to reservoir settings (future reservoir instance)
            Input2ResSettingsMapCollection = new List<Input2ResSettingsMap>();
            XElement reservoirInstancesContainerElem = esnSettingsElem.Descendants("ReservoirInstancesContainer").First();
            foreach (XElement reservoirInstanceElem in reservoirInstancesContainerElem.Descendants("ReservoirInstance"))
            {
                Input2ResSettingsMap newMap = new Input2ResSettingsMap();
                newMap.InstanceName = reservoirInstanceElem.Attribute("InstanceName").Value;
                newMap.AugmentedStates = bool.Parse(reservoirInstanceElem.Attribute("AugmentedStates").Value);
                //Select reservoir settings
                newMap.ReservoirSettings = (from settings in availableResSettings
                                            where settings.SettingsName == reservoirInstanceElem.Attribute("SettingsName").Value
                                            select settings).First();
                //Associated input fields
                foreach (XElement inputFieldElem in reservoirInstanceElem.Descendants("InputField"))
                {
                    string inputFieldName = inputFieldElem.Attribute("Name").Value;
                    //Index in InputFieldsNames
                    int inputFieldIdx = InputFieldsNames.IndexOf(inputFieldName);
                    //Found?
                    if (inputFieldIdx < 0)
                    {
                        //Not found
                        throw new Exception($"Reservoir instance {newMap.InstanceName}: input field {inputFieldName} is not defined among Esn input fields.");
                    }
                    newMap.InputFieldsIdxs.Add(inputFieldIdx);
                    newMap.InputFieldsNames.Add(inputFieldName);
                }
                //Associated feedback fields
                foreach (string feedbackFieldName in newMap.ReservoirSettings.FeedbackFieldsNames)
                {
                    //Index in OutputFieldsNames
                    int feedbackFieldIdx = OutputFieldsNames.IndexOf(feedbackFieldName);
                    //Found?
                    if (feedbackFieldIdx < 0)
                    {
                        //Not found
                        throw new Exception($"Reservoir instance {newMap.InstanceName}: feedback field {feedbackFieldName} is not defined among Esn output fields.");
                    }
                    newMap.FeedbackFieldsIdxs.Add(feedbackFieldIdx);
                    newMap.FeedbackFieldsNames.Add(feedbackFieldName);
                }
                Input2ResSettingsMapCollection.Add(newMap);
            }

            return;
        }

        //Properties
        /// <summary>Number of input fields.</summary>
        public int InputFieldsCount { get { return InputFieldsNames.Count; } }
        /// <summary>Number of output fields.</summary>
        public int OutputFieldsCount { get { return OutputFieldsNames.Count; } }


        //Methods
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            EsnSettings cmpSettings = obj as EsnSettings;
            if (RandomizerSeek != cmpSettings.RandomizerSeek ||
               !InputFieldsNames.ToArray().EqualValues(cmpSettings.InputFieldsNames.ToArray()) ||
               Input2ResSettingsMapCollection.Count != cmpSettings.Input2ResSettingsMapCollection.Count ||
               RouteInputToReadout != cmpSettings.RouteInputToReadout ||
               OutputNeuronActivation != cmpSettings.OutputNeuronActivation ||
               RegressionMethod != cmpSettings.RegressionMethod ||
               RegressionAttempts != cmpSettings.RegressionAttempts ||
               RegressionAttemptEpochs != cmpSettings.RegressionAttemptEpochs ||
               RegressionAttemptStopMSE != cmpSettings.RegressionAttemptStopMSE ||
               ReadOutHiddenLayers.Count != cmpSettings.ReadOutHiddenLayers.Count
               )
            {
                return false;
            }
            for (int i = 0; i < Input2ResSettingsMapCollection.Count; i++)
            {
                if (!Input2ResSettingsMapCollection[i].Equals(cmpSettings.Input2ResSettingsMapCollection[i]))
                {
                    return false;
                }
            }
            for (int i = 0; i < ReadOutHiddenLayers.Count; i++)
            {
                if (!ReadOutHiddenLayers[i].Equals(cmpSettings.ReadOutHiddenLayers[i]))
                {
                    return false;
                }
            }
            return true;
        }

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
        /// Mapping of Esn's input fields to specific reservoir settings
        /// </summary>
        [Serializable]
        public sealed class Input2ResSettingsMap
        {
            //Attribute properties
            public List<int> InputFieldsIdxs {get; set;}
            public List<string> InputFieldsNames { get; set; }
            public List<int> FeedbackFieldsIdxs { get; set; }
            public List<string> FeedbackFieldsNames { get; set; }
            public string InstanceName { get; set; }
            public AnalogReservoirSettings ReservoirSettings { get; set; }
            public bool AugmentedStates { get; set; }

            //Constructor
            public Input2ResSettingsMap()
            {
                InputFieldsIdxs = new List<int>();
                InputFieldsNames = new List<string>();
                FeedbackFieldsIdxs = new List<int>();
                FeedbackFieldsNames = new List<string>();
                InstanceName = string.Empty;
                ReservoirSettings = null;
                AugmentedStates = false;
                return;
            }
            public Input2ResSettingsMap(Input2ResSettingsMap source)
            {
                InputFieldsIdxs = new List<int>(source.InputFieldsIdxs);
                InputFieldsNames = new List<string>(source.InputFieldsNames);
                FeedbackFieldsIdxs = new List<int>(source.FeedbackFieldsIdxs);
                FeedbackFieldsNames = new List<string>(source.FeedbackFieldsNames);
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
            public Input2ResSettingsMap DeepClone()
            {
                return new Input2ResSettingsMap(this);
            }

            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                Input2ResSettingsMap cmpSettings = obj as Input2ResSettingsMap;
                if (InputFieldsIdxs.Count != cmpSettings.InputFieldsIdxs.Count ||
                    FeedbackFieldsIdxs.Count != cmpSettings.FeedbackFieldsIdxs.Count ||
                    InstanceName != cmpSettings.InstanceName ||
                    !ReservoirSettings.Equals(cmpSettings.ReservoirSettings) ||
                    AugmentedStates != cmpSettings.AugmentedStates
                    )
                {
                    return false;
                }
                for (int i = 0; i < InputFieldsIdxs.Count; i++)
                {
                    if (InputFieldsIdxs[i] != cmpSettings.InputFieldsIdxs[i] ||
                        InputFieldsNames[i] != cmpSettings.InputFieldsNames[i]
                        )
                    {
                        return false;
                    }
                }
                for (int i = 0; i < FeedbackFieldsIdxs.Count; i++)
                {
                    if (FeedbackFieldsIdxs[i] != cmpSettings.FeedbackFieldsIdxs[i] ||
                        FeedbackFieldsNames[i] != cmpSettings.FeedbackFieldsNames[i]
                        )
                    {
                        return false;
                    }
                }
                return true;
            }

            public override int GetHashCode()
            {
                return InstanceName.GetHashCode();
            }

        }//Input2ResSettingsMap

    }//ESNSettings


}//Namespace
