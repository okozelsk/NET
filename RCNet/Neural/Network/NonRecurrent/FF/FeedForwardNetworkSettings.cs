using RCNet.MathTools;
using RCNet.Neural.Activation;
using System;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.NonRecurrent.FF
{
    /// <summary>
    /// Configuration of the FeedForwardNetwork and associated trainer
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
        public IActivationSettings OutputActivationCfg { get; }
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
        /// <param name="trainerCfg">Configuration of the associated trainer</param>
        public FeedForwardNetworkSettings(IActivationSettings outputActivationCfg,
                                          HiddenLayersSettings hiddenLayersCfg,
                                          RCNetBaseSettings trainerCfg
                                          )
        {
            OutputActivationCfg = (IActivationSettings)outputActivationCfg.DeepClone();
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
            :this(source.OutputActivationCfg, source.HiddenLayersCfg, source.TrainerCfg)
        {
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
        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if(!FeedForwardNetwork.IsAllowedOutputAF(OutputActivationCfg))
            {
                throw new ArgumentException($"Specified output activation function can't be used in FF network's output activation.", "OutputActivationCfg");
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
            if ((HiddenLayersCfg.HiddenLayerCfgCollection.Count > 0 || OutputActivationCfg.GetType() != typeof(AFAnalogIdentitySettings)) &&
               trainerType != typeof(RPropTrainerSettings)
               )
            {
                throw new ArgumentException($"Improper type of trainer {trainerType.Name}. For FF having other than Identity output activation or containing hidden layers can be used only Resilient back propagation trainer.", "TrainerCfg");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new FeedForwardNetworkSettings(this);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("ff", suppressDefaults);
        }

    }//FeedForwardNetworkSettings

}//Namespace

