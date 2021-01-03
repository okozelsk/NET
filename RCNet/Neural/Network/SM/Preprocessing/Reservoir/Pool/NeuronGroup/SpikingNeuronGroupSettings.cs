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
    /// Configuration of a group of spiking neurons.
    /// </summary>
    [Serializable]
    public class SpikingNeuronGroupSettings : RCNetBaseSettings, INeuronGroupSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "PoolSpikingNeuronGroupType";

        //Attribute properties
        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public double RelShare { get; }

        /// <inheritdoc/>
        public IActivationSettings ActivationCfg { get; }

        /// <summary>
        /// The configuration of the neurons homogenous excitability.
        /// </summary>
        public HomogenousExcitabilitySettings HomogenousExcitabilityCfg { get; }

        /// <inheritdoc/>
        public RandomValueSettings BiasCfg { get; }

        /// <inheritdoc/>
        public PredictorsProviderSettings PredictorsCfg { get; }

        /// <inheritdoc/>
        public int Count { get; set; } = 0;


        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="name">The name of the neuron group.</param>
        /// <param name="relShare">Specifies how big relative portion of pool's neurons is formed by this group of the neurons.</param>
        /// <param name="activationCfg">The common configuration of the neurons' activation function.</param>
        /// <param name="predictorsCfg">The common configuration of the predictors provider.</param>
        /// <param name="homogenousExcitabilityCfg">The configuration of the neurons homogenous excitability.</param>
        /// <param name="biasCfg">The configuration of the constant input bias.</param>
        public SpikingNeuronGroupSettings(string name,
                                          double relShare,
                                          IActivationSettings activationCfg,
                                          PredictorsProviderSettings predictorsCfg,
                                          HomogenousExcitabilitySettings homogenousExcitabilityCfg = null,
                                          RandomValueSettings biasCfg = null
                                          )
        {
            Name = name;
            RelShare = relShare;
            ActivationCfg = (IActivationSettings)activationCfg.DeepClone();
            PredictorsCfg = (PredictorsProviderSettings)predictorsCfg.DeepClone();
            HomogenousExcitabilityCfg = homogenousExcitabilityCfg == null ? new HomogenousExcitabilitySettings() : (HomogenousExcitabilitySettings)homogenousExcitabilityCfg.DeepClone();
            BiasCfg = biasCfg == null ? null : (RandomValueSettings)biasCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public SpikingNeuronGroupSettings(SpikingNeuronGroupSettings source)
            : this(source.Name, source.RelShare, source.ActivationCfg, source.PredictorsCfg, source.HomogenousExcitabilityCfg,
                  source.BiasCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
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
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultHomogenousExcitabilityCfg { get { return HomogenousExcitabilityCfg.ContainsOnlyDefaults; } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return false;
            }
        }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (Name.Length == 0)
            {
                throw new ArgumentException($"Name can not be empty.", "Name");
            }
            Type activationType = ActivationCfg.GetType();
            if (activationType != typeof(AFSpikingSimpleIFSettings) &&
                activationType != typeof(AFSpikingLeakyIFSettings) &&
                activationType != typeof(AFSpikingExpIFSettings) &&
                activationType != typeof(AFSpikingAdExpIFSettings) &&
                activationType != typeof(AFSpikingIzhikevichIFSettings) &&
                activationType != typeof(AFSpikingAutoIzhikevichIFSettings)
                )
            {
                throw new ArgumentException($"Not allowed activation type {activationType.Name}.", "ActivationCfg");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new SpikingNeuronGroupSettings(this);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("spikingGroup", suppressDefaults);
        }

    }//SpikingNeuronGroupSettings

}//Namespace
