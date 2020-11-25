using RCNet.Neural.Activation;
using RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor;
using RCNet.RandomValue;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool.NeuronGroup
{
    /// <summary>
    /// Contains spiking neuron group settings
    /// </summary>
    [Serializable]
    public class SpikingNeuronGroupSettings : RCNetBaseSettings, INeuronGroupSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PoolSpikingNeuronGroupType";

        //Attribute properties
        /// <summary>
        /// Name of the neuron group
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Specifies how big relative portion of pool's neurons is formed by this group of the neurons
        /// </summary>
        public double RelShare { get; }

        /// <summary>
        /// Common activation function settings of the groupped neurons
        /// </summary>
        public RCNetBaseSettings ActivationCfg { get; }

        /// <summary>
        /// Configuration of the neuron's homogenous excitability
        /// </summary>
        public HomogenousExcitabilitySettings HomogenousExcitabilityCfg { get; }

        /// <summary>
        /// Each neuron within the group receives constant input bias. Value of the neuron's bias is driven by this random settings
        /// </summary>
        public RandomValueSettings BiasCfg { get; }

        /// <summary>
        /// Configuration of the predictors
        /// </summary>
        public PredictorsProviderSettings PredictorsCfg { get; }

        /// <summary>
        /// Additional helper computed field.
        /// Specifies exact number of neurons of the group within the current context.
        /// </summary>
        public int Count { get; set; } = 0;


        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="name">Name of the neuron group</param>
        /// <param name="relShare">Specifies how big relative portion of pool's neurons is formed by this group of the neurons</param>
        /// <param name="activationCfg">Common activation function settings of the groupped neurons</param>
        /// <param name="predictorsCfg">Configuration of the predictors</param>
        /// <param name="homogenousExcitabilityCfg">Configuration of the neuron's homogenous excitability</param>
        /// <param name="biasCfg">Each neuron within the group receives constant input bias. Value of the neuron's bias is driven by this random settings</param>
        public SpikingNeuronGroupSettings(string name,
                                          double relShare,
                                          RCNetBaseSettings activationCfg,
                                          PredictorsProviderSettings predictorsCfg,
                                          HomogenousExcitabilitySettings homogenousExcitabilityCfg = null,
                                          RandomValueSettings biasCfg = null
                                          )
        {
            Name = name;
            RelShare = relShare;
            ActivationCfg = activationCfg.DeepClone();
            PredictorsCfg = (PredictorsProviderSettings)predictorsCfg.DeepClone();
            HomogenousExcitabilityCfg = homogenousExcitabilityCfg == null ? new HomogenousExcitabilitySettings() : (HomogenousExcitabilitySettings)homogenousExcitabilityCfg.DeepClone();
            BiasCfg = biasCfg == null ? null : (RandomValueSettings)biasCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public SpikingNeuronGroupSettings(SpikingNeuronGroupSettings source)
            : this(source.Name, source.RelShare, source.ActivationCfg, source.PredictorsCfg, source.HomogenousExcitabilityCfg,
                  source.BiasCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings.</param>
        public SpikingNeuronGroupSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            //Name
            Name = settingsElem.Attribute("name").Value;
            //Relative share
            RelShare = double.Parse(settingsElem.Attribute("relShare").Value, CultureInfo.InvariantCulture);
            //Activation settings
            ActivationCfg = ActivationFactory.LoadSettings(settingsElem.Elements().First());
            //Homogenous excitability
            XElement homogenousExcitabilityElem = settingsElem.Elements("homogenousExcitability").FirstOrDefault();
            HomogenousExcitabilityCfg = homogenousExcitabilityElem == null ? new HomogenousExcitabilitySettings() : new HomogenousExcitabilitySettings(homogenousExcitabilityElem);
            //Bias
            XElement biasSettingsElem = settingsElem.Elements("bias").FirstOrDefault();
            BiasCfg = biasSettingsElem == null ? null : new RandomValueSettings(biasSettingsElem);
            //Predictors
            PredictorsCfg = new PredictorsProviderSettings(settingsElem.Elements("predictors").First());
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Type of the activation functions within the group (analog or spiking)
        /// </summary>
        public ActivationType Type { get { return ActivationType.Spiking; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultHomogenousExcitabilityCfg { get { return HomogenousExcitabilityCfg.ContainsOnlyDefaults; } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return false;
            }
        }

        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            if (Name.Length == 0)
            {
                throw new ArgumentException($"Name can not be empty.", "Name");
            }
            Type activationType = ActivationCfg.GetType();
            if (activationType != typeof(SimpleIFSettings) &&
                activationType != typeof(LeakyIFSettings) &&
                activationType != typeof(ExpIFSettings) &&
                activationType != typeof(AdExpIFSettings) &&
                activationType != typeof(IzhikevichIFSettings) &&
                activationType != typeof(AutoIzhikevichIFSettings)
                )
            {
                throw new ArgumentException($"Not allowed activation type {activationType.Name}.", "ActivationCfg");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new SpikingNeuronGroupSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("name", Name),
                                             new XAttribute("relShare", RelShare.ToString(CultureInfo.InvariantCulture)),
                                             ActivationCfg.GetXml(suppressDefaults)
                                             );

            if (!suppressDefaults || !IsDefaultHomogenousExcitabilityCfg)
            {
                rootElem.Add(HomogenousExcitabilityCfg.GetXml(suppressDefaults));
            }
            if (BiasCfg != null)
            {
                rootElem.Add(BiasCfg.GetXml("bias", suppressDefaults));
            }
            rootElem.Add(PredictorsCfg.GetXml(suppressDefaults));
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
            return GetXml("spikingGroup", suppressDefaults);
        }

    }//SpikingNeuronGroupSettings

}//Namespace
