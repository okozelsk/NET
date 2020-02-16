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

namespace RCNet.Neural.Network.NonRecurrent.FF
{
    /// <summary>
    /// The class contains feed forward network configuration parameters
    /// The easiest and safest way to create an instance is to use the xml constructor.
    /// </summary>
    [Serializable]
    public class FeedForwardNetworkSettings : RCNetBaseSettings, INonRecurrentNetworkSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "FFNetCfgType";

        //Attribute properties
        /// <summary>
        /// Hidden layers configuration. Hidden layers are optional.
        /// </summary>
        public HiddenLayersSettings HiddenLayersCfg { get; }
        /// <summary>
        /// Output layer activation configuration
        /// </summary>
        public RCNetBaseSettings OutputActivationCfg { get; }
        /// <summary>
        /// Network output values range.
        /// </summary>
        public Interval OutputRange { get; }
        /// <summary>
        /// Configuration of associated trainer
        /// </summary>
        public RCNetBaseSettings TrainerCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="outputActivationCfg">Output layer activation configuration</param>
        /// <param name="hiddenLayersCfg">Hidden layers configuration. Hidden layers are optional.</param>
        /// <param name="trainerCfg">Configuration of associated trainer</param>
        public FeedForwardNetworkSettings(RCNetBaseSettings outputActivationCfg,
                                          HiddenLayersSettings hiddenLayersCfg,
                                          RCNetBaseSettings trainerCfg
                                          )
        {
            OutputActivationCfg = ActivationFactory.DeepCloneActivationSettings(outputActivationCfg);
            CheckAllowedActivation(OutputActivationCfg, out Interval outputRange);
            OutputRange = outputRange;
            HiddenLayersCfg = hiddenLayersCfg == null ? new HiddenLayersSettings() : (HiddenLayersSettings)hiddenLayersCfg.DeepClone();
            if(trainerCfg.GetType() != typeof(QRDRegrTrainerSettings) &&
               trainerCfg.GetType() != typeof(RidgeRegrTrainerSettings) &&
               trainerCfg.GetType() != typeof(ElasticRegrTrainerSettings) &&
               trainerCfg.GetType() != typeof(RPropTrainerSettings)
               )
            {
                throw new Exception("Unsupported trainer settings.");
            }
            TrainerCfg = trainerCfg.DeepClone();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public FeedForwardNetworkSettings(FeedForwardNetworkSettings source)
        {
            OutputActivationCfg = ActivationFactory.DeepCloneActivationSettings(source.OutputActivationCfg);
            OutputRange = source.OutputRange.DeepClone();
            HiddenLayersCfg = (HiddenLayersSettings)source.HiddenLayersCfg.DeepClone();
            TrainerCfg = source.TrainerCfg.DeepClone();
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
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            OutputActivationCfg = ActivationFactory.LoadSettings(settingsElem.Descendants().First());
            CheckAllowedActivation(OutputActivationCfg, out Interval outputRange);
            OutputRange = outputRange;
            //Hidden layers
            XElement hiddenLayersElem = settingsElem.Descendants("hiddenLayers").FirstOrDefault();
            if (hiddenLayersElem != null)
            {
                HiddenLayersCfg = new HiddenLayersSettings(hiddenLayersElem);
            }
            else
            {
                HiddenLayersCfg = new HiddenLayersSettings();
            }
            //Trainer configuration
            TrainerCfg = null;
            foreach(XElement candidate in settingsElem.Descendants())
            {
                if(candidate.Name.LocalName == "qrdRegrTrainer")
                {
                    TrainerCfg = new QRDRegrTrainerSettings(candidate);
                    break;
                }
                else if (candidate.Name.LocalName == "ridgeRegrTrainer")
                {
                    TrainerCfg = new RidgeRegrTrainerSettings(candidate);
                    break;
                }
                else if (candidate.Name.LocalName == "elasticRegrTrainer")
                {
                    TrainerCfg = new ElasticRegrTrainerSettings(candidate);
                    break;
                }
                else if (candidate.Name.LocalName == "resPropTrainer")
                {
                    TrainerCfg = new RPropTrainerSettings(candidate);
                    break;
                }
            }
            if(TrainerCfg == null)
            {
                throw new Exception("Trainer settings not found.");
            }
            return;
        }

        //Properties
        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Static methods
        /// <summary>
        /// Fuction tests if specified activation can be used in FF network 
        /// </summary>
        /// <param name="activationSettings">Activation settings</param>
        /// <param name="outputRange">Returned range of the activation function</param>
        public static bool IsAllowedActivation(RCNetBaseSettings activationSettings, out Interval outputRange)
        {
            IActivationFunction af = ActivationFactory.Create(activationSettings, new Random());
            outputRange = af.OutputRange.DeepClone();
            if (!af.Stateless || !af.SupportsDerivative)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Fuction checks if specified activation can be used in FF network 
        /// </summary>
        /// <param name="activationSettings">Activation settings</param>
        /// <param name="outputRange">Returned range of the activation function</param>
        public static void CheckAllowedActivation(RCNetBaseSettings activationSettings, out Interval outputRange)
        {
            if(!IsAllowedActivation(activationSettings, out outputRange))
            {
                throw new ApplicationException($"Activation can't be used in FF network. Activation function has to be stateless and has to support derivative calculation.");
            }
            return;
        }

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new FeedForwardNetworkSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            rootElem.Add(OutputActivationCfg.GetXml(suppressDefaults));
            if (!HiddenLayersCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(HiddenLayersCfg.GetXml(suppressDefaults));
            }
            rootElem.Add(TrainerCfg.GetXml(suppressDefaults));
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("ff", suppressDefaults);
        }

    }//FeedForwardNetworkSettings

}//Namespace

