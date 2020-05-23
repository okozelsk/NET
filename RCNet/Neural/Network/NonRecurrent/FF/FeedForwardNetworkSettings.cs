using RCNet.MathTools;
using RCNet.Neural.Activation;
using System;
using System.Linq;
using System.Xml.Linq;

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
        public const string XsdTypeName = "FFNetType";

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
            OutputRange = ActivationFactory.GetInfo(OutputActivationCfg, out _, out _);
            HiddenLayersCfg = hiddenLayersCfg == null ? new HiddenLayersSettings() : (HiddenLayersSettings)hiddenLayersCfg.DeepClone();
            TrainerCfg = trainerCfg.DeepClone();
            Check();
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
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public FeedForwardNetworkSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            OutputActivationCfg = ActivationFactory.LoadSettings(settingsElem.Elements().First());
            OutputRange = ActivationFactory.GetInfo(OutputActivationCfg, out _, out _);
            //Hidden layers
            XElement hiddenLayersElem = settingsElem.Elements("hiddenLayers").FirstOrDefault();
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
            foreach (XElement candidate in settingsElem.Elements())
            {
                if (candidate.Name.LocalName == "qrdRegrTrainer")
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
            Check();
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
            outputRange = ActivationFactory.GetInfo(activationSettings, out bool stateless, out bool supportsDerivative);
            if (!stateless || !supportsDerivative)
            {
                return false;
            }
            return true;
        }

        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            if (!IsAllowedActivation(OutputActivationCfg, out _))
            {
                throw new ArgumentException($"Specified OutputActivationCfg can't be used in FF network. Activation function has to be stateless and has to support derivative calculation.", "OutputActivationCfg");
            }
            if (TrainerCfg == null)
            {
                throw new ArgumentNullException("TrainerCfg", "TrainerCfg can not be null.");
            }
            Type trainerType = TrainerCfg.GetType();
            if (trainerType != typeof(QRDRegrTrainerSettings) &&
                trainerType != typeof(RidgeRegrTrainerSettings) &&
                trainerType != typeof(ElasticRegrTrainerSettings) &&
                trainerType != typeof(RPropTrainerSettings)
                )
            {
                throw new ArgumentException($"Unsupported TrainerCfg {trainerType.Name}.", "TrainerCfg");
            }
            if ((HiddenLayersCfg.HiddenLayerCfgCollection.Count > 0 || OutputActivationCfg.GetType() != typeof(IdentitySettings)) &&
               trainerType != typeof(RPropTrainerSettings)
               )
            {
                throw new ArgumentException($"Improper type of trainer {trainerType.Name}. For FF having other than Identity output activation or containing hidden layers can be used only Resilient back propagation trainer.", "TrainerCfg");
            }
            return;
        }

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
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
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
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("ff", suppressDefaults);
        }

    }//FeedForwardNetworkSettings

}//Namespace

