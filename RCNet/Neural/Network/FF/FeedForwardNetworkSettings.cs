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
        /// Activation function settings of the output layer.
        /// </summary>
        public ActivationSettings OutputLayerActivation { get; set; }
        /// <summary>
        /// The parameter specifies what method will be used for training
        /// </summary>
        public TrainingMethodType RegressionMethod { get; set; }
        /// <summary>
        /// Collection of hidden layer definitions. Hidden layers are optional.
        /// </summary>
        public List<HiddenLayerSettings> HiddenLayerCollection { get; set; }
        /// <summary>
        /// Startup parameters for the linear regression trainer
        /// </summary>
        public LinRegrTrainerSettings LinRegrTrainerCfg { get; set; }
        /// <summary>
        /// Setup parameters for the resilient propagation trainer
        /// </summary>
        public RPropTrainerSettings RPropTrainerCfg { get; set; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance
        /// </summary>
        public FeedForwardNetworkSettings()
        {
            OutputLayerActivation = null;
            RegressionMethod = TrainingMethodType.Resilient;
            HiddenLayerCollection = new List<HiddenLayerSettings>();
            LinRegrTrainerCfg = null;
            RPropTrainerCfg = new RPropTrainerSettings();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public FeedForwardNetworkSettings(FeedForwardNetworkSettings source)
        {
            OutputLayerActivation = null;
            if (source.OutputLayerActivation != null)
            {
                OutputLayerActivation = source.OutputLayerActivation.DeepClone();
            }
            RegressionMethod = source.RegressionMethod;
            HiddenLayerCollection = new List<HiddenLayerSettings>(source.HiddenLayerCollection.Count);
            foreach(HiddenLayerSettings shls in source.HiddenLayerCollection)
            {
                HiddenLayerCollection.Add(shls.DeepClone());
            }
            LinRegrTrainerCfg = null;
            RPropTrainerCfg = null;
            if (source.LinRegrTrainerCfg != null)
            {
                LinRegrTrainerCfg = source.LinRegrTrainerCfg.DeepClone();
            }
            if (source.RPropTrainerCfg != null)
            {
                RPropTrainerCfg = source.RPropTrainerCfg.DeepClone();
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
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.FF.FeedForwardNetworkSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement feedForwardNetworkSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            OutputLayerActivation = new ActivationSettings(feedForwardNetworkSettingsElem.Descendants("outputActivation").First());
            if(!IsAllowedActivation(OutputLayerActivation))
            {
                throw new ApplicationException($"Activation {OutputLayerActivation.FunctionType} can't be used in FF network. Activation function has to be stateless and has to support derivative calculation.");
            }
            RegressionMethod = ParseTrainingMethodType(feedForwardNetworkSettingsElem.Attribute("regressionMethod").Value);
            //Hidden layers
            HiddenLayerCollection = new List<HiddenLayerSettings>();
            XElement hiddenLayersElem = feedForwardNetworkSettingsElem.Descendants("hiddenLayers").FirstOrDefault();
            if (hiddenLayersElem != null)
            {
                foreach (XElement layerElem in hiddenLayersElem.Descendants("layer"))
                {
                    HiddenLayerCollection.Add(new HiddenLayerSettings(layerElem));
                }
            }
            //Trainers
            LinRegrTrainerCfg = null;
            RPropTrainerCfg = null;
            switch (RegressionMethod)
            {
                case TrainingMethodType.Linear:
                    XElement linRegrTrainerElem = feedForwardNetworkSettingsElem.Descendants("linRegrTrainer").FirstOrDefault();
                    if(linRegrTrainerElem != null)
                    {
                        LinRegrTrainerCfg = new LinRegrTrainerSettings(linRegrTrainerElem);
                    }
                    else
                    {
                        LinRegrTrainerCfg = new LinRegrTrainerSettings();
                    }
                    break;
                case TrainingMethodType.Resilient:
                    XElement resPropTrainerElem = feedForwardNetworkSettingsElem.Descendants("resPropTrainer").FirstOrDefault();
                    if (resPropTrainerElem != null)
                    {
                        RPropTrainerCfg = new RPropTrainerSettings(resPropTrainerElem);
                    }
                    else
                    {
                        RPropTrainerCfg = new RPropTrainerSettings();
                    }
                    break;
            }
            return;
        }

        //Methods
        //Static methods
        /// <summary>
        /// Fuction checks if specified activation can be used in FF network 
        /// </summary>
        /// <param name="activationSettings">Activation settings</param>
        /// <returns></returns>
        public static bool IsAllowedActivation(ActivationSettings activationSettings)
        {
            IActivationFunction af = ActivationFactory.Create(activationSettings);
            if(!af.Stateless || !af.SupportsComputeDerivativeMethod)
            {
                return false;
            }
            return true;
        }
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
            if (!Equals(OutputLayerActivation, cmpSettings.OutputLayerActivation) ||
                RegressionMethod != cmpSettings.RegressionMethod ||
                HiddenLayerCollection.Count != cmpSettings.HiddenLayerCollection.Count ||
                !Equals(LinRegrTrainerCfg, cmpSettings.LinRegrTrainerCfg) ||
                !Equals(RPropTrainerCfg, cmpSettings.RPropTrainerCfg)
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
            /// Settings of activation function of the hidden layer neurons
            /// </summary>
            public ActivationSettings Activation { get; set; }

            //Constructors
            /// <summary>
            /// Creates an uninitialized instance
            /// </summary>
            public HiddenLayerSettings()
            {
                NumOfNeurons = 0;
                Activation = null;
                return;
            }

            /// <summary>
            /// Copy constructor
            /// </summary>
            /// <param name="source">Source instance</param>
            public HiddenLayerSettings(HiddenLayerSettings source)
            {
                NumOfNeurons = source.NumOfNeurons;
                Activation = null;
                if (source.Activation != null)
                {
                    Activation = source.Activation.DeepClone();
                }
                return;
            }

            /// <summary>
            /// Creates the instance and initializes it from given xml element.
            /// </summary>
            /// <param name="elem">
            /// Xml data containing the settings.
            /// </param>
            public HiddenLayerSettings(XElement elem)
            {
                NumOfNeurons = int.Parse(elem.Attribute("neurons").Value);
                Activation = new ActivationSettings(elem.Descendants("activation").First());
                if (!IsAllowedActivation(Activation))
                {
                    throw new ApplicationException($"Activation {Activation.FunctionType} can't be used in FF network. Activation has to be time independent and has to support derivative.");
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
                HiddenLayerSettings cmpSettings = obj as HiddenLayerSettings;
                if (NumOfNeurons != cmpSettings.NumOfNeurons ||
                    !Equals(Activation, cmpSettings.Activation)
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

