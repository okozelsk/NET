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

namespace RCNet.Neural.Network.ReservoirComputing.Readout
{
    /// <summary>
    /// The class contains readout layer configuration parameters.
    /// The easiest and safest way to create an instance is to use the xml constructor.
    /// </summary>
    [Serializable]
    public class ReadoutLayerSettings
    {
        /// <summary>
        /// Parameter specifies how big part of available samples will be used for testing.
        /// </summary>
        public double RatioOfTestData { get; set; }
        /// <summary>
        /// Number of predicting readout units for each output field.
        /// It also detemines how many data sets for testing will be prepared.
        /// (x-fold cross-validation)
        /// https://en.wikipedia.org/wiki/Cross-validation_(statistics)
        /// Parameter has two options.
        /// LE 0 - means auto setup to achieve full cross-validation if it is possible (related to specified RatioOfTestData)
        /// GT 0 - means exact number of the folds
        /// </summary>
        public int NumOfFolds { get; set; }
        /// <summary>
        /// Collection of hidden layer definitions. Hidden layers are optional and can be used in the output
        /// feed forward network that process predictors from reservoirs and computes output field
        /// value (for each output field is instantiated and trained at least one feed forward network - readout unit).
        /// </summary>
        public List<HiddenLayerSettings> HiddenLayerCollection { get; set; }
        /// <summary>
        /// Activation function of the output neuron calculates the output value.
        /// This neuron is the output layer of the feed forward network processing predictors
        /// from the reservoirs (for each output field is instantiated and trained at least
        /// one feed forward network - readout unit).
        /// </summary>
        public ActivationFactory.ActivationType OutputNeuronActivation { get; set; }
        /// <summary>
        /// The parameter specifies what method will be used for training
        /// the output feed forward networks.
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
        /// The collection of output field names in order of how they will be computed.
        /// </summary>
        public List<string> OutputFieldNameCollection { get; set; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        public ReadoutLayerSettings()
        {
            //Default settings
            RatioOfTestData = 0;
            NumOfFolds = 0;
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
        public ReadoutLayerSettings(ReadoutLayerSettings source)
        {
            //Copy
            RatioOfTestData = source.RatioOfTestData;
            NumOfFolds = source.NumOfFolds;
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
        /// This is the preferred way to instantiate ReadoutLayer settings.
        /// </summary>
        /// <param name="readoutLayerSettingsElem">
        /// Xml data containing ReadoutLayer settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public ReadoutLayerSettings(XElement readoutLayerSettingsElem)
        {
            //Validation
            //A very ugly validation. Xml schema does not support validation of the xml fragment against specific type.
            XmlValidator validator = new XmlValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            using (Stream schemaStream = assemblyRCNet.GetManifestResourceStream("RCNet.Neural.Network.ReservoirComputing.Readout.ReadoutLayerSettings.xsd"))
            {
                validator.AddSchema(schemaStream);
            }
            using (Stream schemaStream = assemblyRCNet.GetManifestResourceStream("RCNet.NeuralSettingsTypes.xsd"))
            {
                validator.AddSchema(schemaStream);
            }
            validator.LoadXDocFromString(readoutLayerSettingsElem.ToString());
            //Parsing
            RatioOfTestData = double.Parse(readoutLayerSettingsElem.Attribute("RatioOfTestData").Value, CultureInfo.InvariantCulture);
            NumOfFolds = int.Parse(readoutLayerSettingsElem.Attribute("NumOfFolds").Value);
            //Hidden layers
            HiddenLayerCollection = new List<HiddenLayerSettings>();
            foreach (XElement hiddenLayerElem in readoutLayerSettingsElem.Descendants("HiddenLayer"))
            {
                HiddenLayerCollection.Add(new HiddenLayerSettings(hiddenLayerElem));
            }
            //Output fields
            XElement outputFieldsElem = readoutLayerSettingsElem.Descendants("OutputFields").First();
            OutputNeuronActivation = ActivationFactory.ParseActivation(outputFieldsElem.Attribute("OutputActivation").Value);
            RegressionMethod = FeedForwardNetwork.ParseTrainingMethodType(outputFieldsElem.Attribute("RegressionMethod").Value);
            RegressionAttempts = int.Parse(outputFieldsElem.Attribute("Attempts").Value);
            RegressionAttemptEpochs = int.Parse(outputFieldsElem.Attribute("AttemptEpochs").Value);
            RegressionAttemptStopMSE = double.Parse(outputFieldsElem.Attribute("AttemptStopMSE").Value, CultureInfo.InvariantCulture);
            OutputFieldNameCollection = new List<string>();
            foreach (XElement outputFieldElem in outputFieldsElem.Descendants("Field"))
            {
                OutputFieldNameCollection.Add(outputFieldElem.Attribute("Name").Value);
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
            ReadoutLayerSettings cmpSettings = obj as ReadoutLayerSettings;
            if (RatioOfTestData != cmpSettings.RatioOfTestData ||
                NumOfFolds != cmpSettings.NumOfFolds ||
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
        public ReadoutLayerSettings DeepClone()
        {
            ReadoutLayerSettings clone = new ReadoutLayerSettings(this);
            return clone;
        }

    }//ReadoutLayerSettings

}//Namespace
