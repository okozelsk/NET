using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.IO;
using RCNet.XmlTools;
using RCNet.Neural.Activation;

namespace RCNet.Neural.Network.FF
{
    /// <summary>
    /// The class contains feed forward network configuration parameters
    /// The easiest and safest way to create an instance is to use the xml constructor.
    /// </summary>
    [Serializable]
    public class FeedForwardNetworkSettings
    {
        //Constants
        /// <summary>
        /// Supported training methods
        /// </summary>
        public enum TrainingMethodType
        {
            /// <summary>
            /// Linear regression
            /// </summary>
            Linear,
            /// <summary>
            /// Resilient backpropagation
            /// </summary>
            Resilient
        }//TrainingMethodType


        //Attribute properties
        /// <summary>
        /// Activation function of the output layer.
        /// </summary>
        public ActivationFactory.ActivationType OutputActivation { get; set; }
        /// <summary>
        /// The parameter specifies what method will be used for training
        /// </summary>
        public TrainingMethodType RegressionMethod { get; set; }
        /// <summary>
        /// Collection of hidden layer definitions. Hidden layers are optional.
        /// </summary>
        public List<HiddenLayerSettings> HiddenLayerCollection { get; set; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance
        /// </summary>
        public FeedForwardNetworkSettings()
        {
            OutputActivation = ActivationFactory.ActivationType.Identity;
            RegressionMethod = TrainingMethodType.Resilient;
            HiddenLayerCollection = new List<HiddenLayerSettings>();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public FeedForwardNetworkSettings(FeedForwardNetworkSettings source)
        {
            OutputActivation = source.OutputActivation;
            RegressionMethod = source.RegressionMethod;
            HiddenLayerCollection = new List<HiddenLayerSettings>(source.HiddenLayerCollection.Count);
            foreach(HiddenLayerSettings shls in source.HiddenLayerCollection)
            {
                HiddenLayerCollection.Add(shls.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// This is the preferred way to instantiate reservoir settings.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing feed forward network settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public FeedForwardNetworkSettings(XElement elem)
        {
            //Validation
            //A very ugly validation. Xml schema does not support validation of the xml fragment against specific type.
            XmlValidator validator = new XmlValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            using (Stream schemaStream = assemblyRCNet.GetManifestResourceStream("RCNet.Neural.Network.FF.FeedForwardNetworkSettings.xsd"))
            {
                validator.AddSchema(schemaStream);
            }
            using (Stream schemaStream = assemblyRCNet.GetManifestResourceStream("RCNet.NeuralSettingsTypes.xsd"))
            {
                validator.AddSchema(schemaStream);
            }
            XElement feedForwardNetworkSettingsElem = validator.LoadXDocFromString(elem.ToString()).Root;
            //Parsing
            OutputActivation = ActivationFactory.ParseActivation(feedForwardNetworkSettingsElem.Attribute("activation").Value);
            RegressionMethod = ParseTrainingMethodType(feedForwardNetworkSettingsElem.Attribute("regressionMethod").Value);
            //Hidden layers
            HiddenLayerCollection = new List<HiddenLayerSettings>();
            foreach (XElement hiddenLayerElem in feedForwardNetworkSettingsElem.Descendants("hiddenLayer"))
            {
                HiddenLayerCollection.Add(new HiddenLayerSettings(hiddenLayerElem));
            }
            return;
        }

        //Methods
        //Static methods
        /// <summary>
        /// Parses training method type from string code
        /// </summary>
        /// <param name="code">Code of the training method type</param>
        public static TrainingMethodType ParseTrainingMethodType(string code)
        {
            switch (code.ToUpper())
            {
                case "LINEAR": return TrainingMethodType.Linear;
                case "RESILIENT": return TrainingMethodType.Resilient;
                default:
                    throw new ArgumentException($"Unknown training method code {code}");
            }
        }

        //Instance methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            FeedForwardNetworkSettings cmpSettings = obj as FeedForwardNetworkSettings;
            if (OutputActivation != cmpSettings.OutputActivation ||
                RegressionMethod != cmpSettings.RegressionMethod ||
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
        public FeedForwardNetworkSettings DeepClone()
        {
            FeedForwardNetworkSettings clone = new FeedForwardNetworkSettings(this);
            return clone;
        }


        //Inner classes
        /// <summary>
        /// Feed forward network hidden layer settings
        /// </summary>
        [Serializable]
        public class HiddenLayerSettings
        {
            //Attributes
            /// <summary>
            /// Number of hidden layer neurons
            /// </summary>
            public int NumOfNeurons { get; set; }
            /// <summary>
            /// Type of activation function of the hidden layer neurons
            /// </summary>
            public ActivationFactory.ActivationType ActivationType { get; set; }

            //Constructors
            /// <summary>
            /// Creates an initialized instance
            /// </summary>
            /// <param name="numOfNeurons">Number of hidden layer neurons</param>
            /// <param name="activationType">Type of activation function of the hidden layer neurons</param>
            public HiddenLayerSettings(int numOfNeurons, ActivationFactory.ActivationType activationType)
            {
                NumOfNeurons = numOfNeurons;
                ActivationType = activationType;
                return;
            }

            /// <summary>
            /// Copy constructor
            /// </summary>
            /// <param name="source">Source instance</param>
            public HiddenLayerSettings(HiddenLayerSettings source)
            {
                NumOfNeurons = source.NumOfNeurons;
                ActivationType = source.ActivationType;
                return;
            }

            /// <summary>
            /// Creates the instance and initializes it from given xml element.
            /// </summary>
            /// <param name="hiddenLayerElem">
            /// Xml data containing the settings.
            /// Content of xml element is not validated against the xml schema.
            /// </param>
            public HiddenLayerSettings(XElement hiddenLayerElem)
            {
                NumOfNeurons = int.Parse(hiddenLayerElem.Attribute("neurons").Value);
                ActivationType = ActivationFactory.ParseActivation(hiddenLayerElem.Attribute("activation").Value);
                return;
            }

            //Methods
            /// <summary>
            /// See the base.
            /// </summary>
            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                HiddenLayerSettings cmpSettings = obj as HiddenLayerSettings;
                if (NumOfNeurons != cmpSettings.NumOfNeurons || ActivationType != cmpSettings.ActivationType)
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
                return NumOfNeurons.GetHashCode();
            }

            /// <summary>
            /// Creates the deep copy instance of this instance.
            /// </summary>
            public HiddenLayerSettings DeepClone()
            {
                return new HiddenLayerSettings(this);
            }

        }//HiddenLayerSettings


    }//FeedForwardNetworkSettings

}//Namespace

