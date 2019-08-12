using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.IO;
using RCNet.XmlTools;
using RCNet.Neural.Activation;
using RCNet.MathTools;

namespace RCNet.Neural.Network.FF
{
    /// <summary>
    /// The class contains feed forward network configuration parameters
    /// The easiest and safest way to create an instance is to use the xml constructor.
    /// </summary>
    [Serializable]
    public class FeedForwardNetworkSettings
    {
        //Attribute properties
        /// <summary>
        /// Collection of hidden layer definitions. Hidden layers are optional.
        /// </summary>
        public List<HiddenLayerSettings> HiddenLayerCollection { get; set; }
        /// <summary>
        /// Activation function settings of the output layer.
        /// </summary>
        public Object OutputLayerActivation { get; set; }
        /// <summary>
        /// Network output values range.
        /// </summary>
        public Interval OutputRange { get; set; }
        /// <summary>
        /// Startup parameters for the trainer
        /// </summary>
        public INonRecurrentNetworkTrainerSettings TrainerCfg { get; set; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance
        /// </summary>
        public FeedForwardNetworkSettings()
        {
            OutputLayerActivation = null;
            OutputRange = null;
            HiddenLayerCollection = new List<HiddenLayerSettings>();
            TrainerCfg = null;
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public FeedForwardNetworkSettings(FeedForwardNetworkSettings source)
        {
            OutputLayerActivation = null;
            OutputRange = null;
            if (source.OutputLayerActivation != null)
            {
                OutputLayerActivation = ActivationFactory.DeepCloneActivationSettings(source.OutputLayerActivation);
                OutputRange = source.OutputRange.DeepClone();
            }
            HiddenLayerCollection = new List<HiddenLayerSettings>(source.HiddenLayerCollection.Count);
            foreach(HiddenLayerSettings shls in source.HiddenLayerCollection)
            {
                HiddenLayerCollection.Add(shls.DeepClone());
            }
            TrainerCfg = null;
            if (source.TrainerCfg != null)
            {
                TrainerCfg = source.TrainerCfg.DeepClone();
            }
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// This is the preferred way to instantiate settings.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public FeedForwardNetworkSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.FF.FeedForwardNetworkSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement settingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            OutputLayerActivation = ActivationFactory.LoadSettings(settingsElem.Descendants().First());
            if (!IsAllowedActivation(OutputLayerActivation, out Interval outputRange))
            {
                throw new ApplicationException($"Activation can't be used in FF network. Activation function has to be stateless and has to support derivative calculation.");
            }
            OutputRange = outputRange;
            //Hidden layers
            HiddenLayerCollection = new List<HiddenLayerSettings>();
            XElement hiddenLayersElem = settingsElem.Descendants("hiddenLayers").FirstOrDefault();
            if (hiddenLayersElem != null)
            {
                foreach (XElement layerElem in hiddenLayersElem.Descendants("layer"))
                {
                    HiddenLayerCollection.Add(new HiddenLayerSettings(layerElem));
                }
            }
            //Trainer configuration
            TrainerCfg = null;
            foreach(XElement candidate in settingsElem.Descendants())
            {
                if(candidate.Name.LocalName == "qrdRegrTrainer")
                {
                    TrainerCfg = new QRDRegrTrainerSettings(candidate);
                }
                else if (candidate.Name.LocalName == "ridgeRegrTrainer")
                {
                    TrainerCfg = new RidgeRegrTrainerSettings(candidate);
                }
                else if (candidate.Name.LocalName == "elasticLinRegrTrainer")
                {
                    TrainerCfg = new ElasticLinRegrTrainerSettings(candidate);
                }
                else if (candidate.Name.LocalName == "resPropTrainer")
                {
                    TrainerCfg = new RPropTrainerSettings(candidate);
                }
                if (TrainerCfg != null)
                {
                    break;
                }
            }
            if(TrainerCfg == null)
            {
                throw new Exception("Trainer settings not found.");
            }
            return;
        }

        //Methods
        //Static methods
        /// <summary>
        /// Fuction checks if specified activation can be used in FF network 
        /// </summary>
        /// <param name="activationSettings">Activation settings</param>
        /// <param name="outputRange">Returned range of the activation function</param>
        public static bool IsAllowedActivation(Object activationSettings, out Interval outputRange)
        {
            IActivationFunction af = ActivationFactory.Create(activationSettings, new Random());
            outputRange = af.OutputRange.DeepClone();
            if (!af.Stateless || !af.SupportsDerivative)
            {
                return false;
            }
            return true;
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
                !Equals(OutputRange, cmpSettings.OutputRange) ||
                HiddenLayerCollection.Count != cmpSettings.HiddenLayerCollection.Count ||
                !Equals(TrainerCfg, cmpSettings.TrainerCfg)
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
            return new FeedForwardNetworkSettings(this);
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
            public Object Activation { get; set; }

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
                    Activation = ActivationFactory.DeepCloneActivationSettings(source.Activation);
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
                Activation = ActivationFactory.LoadSettings(elem.Descendants().First());
                if (!IsAllowedActivation(Activation, out Interval outputRange))
                {
                    throw new ApplicationException($"Activation can't be used in FF network. Activation has to be time independent and has to support derivative.");
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

