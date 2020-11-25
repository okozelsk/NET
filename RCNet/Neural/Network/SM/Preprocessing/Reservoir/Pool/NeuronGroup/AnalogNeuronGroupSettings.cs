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
    /// Contains analog neuron group settings
    /// </summary>
    [Serializable]
    public class AnalogNeuronGroupSettings : RCNetBaseSettings, INeuronGroupSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PoolAnalogNeuronGroupType";
        /// <summary>
        /// Default value of the firing threshold.
        /// Every time the new normalized activation value is higher than the previous
        /// normalized activation value by at least the threshold, it is evaluated as a firing event
        /// </summary>
        public const double DefaultFiringThreshold = 0.00125d;
        /// <summary>
        /// Default maximum deepness of historical normalized activation value to be compared with current normalized activation value when evaluating firing event.
        /// </summary>
        public const int DefaultThresholdMaxRefDeepness = 1;

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
        /// A number between 0 and 1 (LT1). Every time the new normalized activation value is higher than the previous
        /// normalized activation value by at least the threshold, it is evaluated as a firing event.
        /// </summary>
        public double FiringThreshold { get; }

        /// <summary>
        /// Maximum deepness of historical normalized activation value to be compared with current normalized activation value when evaluating firing event.
        /// </summary>
        public int ThresholdMaxRefDeepness { get; }

        /// <summary>
        /// Each neuron within the group receives constant input bias. Value of the neuron's bias is driven by this random settings
        /// </summary>
        public RandomValueSettings BiasCfg { get; }

        /// <summary>
        /// Neurons' retainment property configuration
        /// </summary>
        public RetainmentSettings RetainmentCfg { get; }

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
        /// <param name="firingThreshold">
        /// A number between 0 and 1 (LT1). Every time the new normalized activation value is higher than the previous
        /// normalized activation value by at least the threshold, it is evaluated as a firing event.
        /// </param>
        /// <param name="thresholdMaxRefDeepness">Maximum deepness of historical normalized activation value to be compared with current normalized activation value when evaluating firing event.</param>
        /// <param name="biasCfg">Each neuron within the group receives constant input bias. Value of the neuron's bias is driven by this random settings</param>
        /// <param name="retainmentCfg">Neurons' retainment property configuration</param>
        public AnalogNeuronGroupSettings(string name,
                                         double relShare,
                                         RCNetBaseSettings activationCfg,
                                         PredictorsProviderSettings predictorsCfg,
                                         double firingThreshold = DefaultFiringThreshold,
                                         int thresholdMaxRefDeepness = DefaultThresholdMaxRefDeepness,
                                         RandomValueSettings biasCfg = null,
                                         RetainmentSettings retainmentCfg = null
                                         )
        {
            Name = name;
            RelShare = relShare;
            ActivationCfg = activationCfg.DeepClone();
            PredictorsCfg = (PredictorsProviderSettings)predictorsCfg.DeepClone();
            FiringThreshold = firingThreshold;
            ThresholdMaxRefDeepness = thresholdMaxRefDeepness;
            BiasCfg = biasCfg == null ? null : (RandomValueSettings)biasCfg.DeepClone();
            RetainmentCfg = retainmentCfg == null ? null : (RetainmentSettings)retainmentCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public AnalogNeuronGroupSettings(AnalogNeuronGroupSettings source)
            : this(source.Name, source.RelShare, source.ActivationCfg, source.PredictorsCfg, source.FiringThreshold, source.ThresholdMaxRefDeepness,
                  source.BiasCfg, source.RetainmentCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings.</param>
        public AnalogNeuronGroupSettings(XElement elem)
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
            //Firing threshold
            FiringThreshold = double.Parse(settingsElem.Attribute("firingThreshold").Value, CultureInfo.InvariantCulture);
            ThresholdMaxRefDeepness = int.Parse(settingsElem.Attribute("thresholdMaxRefDeepness").Value, CultureInfo.InvariantCulture);
            //Bias
            XElement biasSettingsElem = settingsElem.Elements("bias").FirstOrDefault();
            BiasCfg = biasSettingsElem == null ? null : new RandomValueSettings(biasSettingsElem);
            //Retainment
            XElement retainmentSettingsElem = settingsElem.Elements("retainment").FirstOrDefault();
            RetainmentCfg = retainmentSettingsElem == null ? null : new RetainmentSettings(retainmentSettingsElem);
            //Predictors
            PredictorsCfg = new PredictorsProviderSettings(settingsElem.Elements("predictors").First());
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Type of the activation functions within the group (analog or spiking)
        /// </summary>
        public ActivationType Type { get { return ActivationType.Analog; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultFiringThreshold { get { return (FiringThreshold == DefaultFiringThreshold); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultThresholdMaxRefDeepness { get { return (ThresholdMaxRefDeepness == DefaultThresholdMaxRefDeepness); } }

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
            if (activationType != typeof(SQNLSettings) &&
                activationType != typeof(ElliotSettings) &&
                activationType != typeof(GaussianSettings) &&
                activationType != typeof(ISRUSettings) &&
                activationType != typeof(SigmoidSettings) &&
                activationType != typeof(SincSettings) &&
                activationType != typeof(SinusoidSettings) &&
                activationType != typeof(TanHSettings)
                )
            {
                throw new ArgumentException($"Not allowed Activation settings {activationType.Name}.", "ActivationCfg");
            }
            if (FiringThreshold < 0 || FiringThreshold > 1)
            {
                throw new ArgumentException($"Invalid FiringThreshold {FiringThreshold.ToString(CultureInfo.InvariantCulture)}. FiringThreshold must be GE to 0 and LE to 1.", "FiringThreshold");
            }
            if (ThresholdMaxRefDeepness < 1)
            {
                throw new ArgumentException($"Invalid ThresholdMaxRefDeepness {ThresholdMaxRefDeepness.ToString(CultureInfo.InvariantCulture)}. ThresholdMaxRefDeepness must be GT 1.", "ThresholdMaxRefDeepness");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new AnalogNeuronGroupSettings(this);
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
            if (!suppressDefaults || !IsDefaultFiringThreshold)
            {
                rootElem.Add(new XAttribute("firingThreshold", FiringThreshold.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultThresholdMaxRefDeepness)
            {
                rootElem.Add(new XAttribute("thresholdMaxRefDeepness", ThresholdMaxRefDeepness.ToString(CultureInfo.InvariantCulture)));
            }
            if (BiasCfg != null)
            {
                rootElem.Add(BiasCfg.GetXml("bias", suppressDefaults));
            }
            if (RetainmentCfg != null)
            {
                rootElem.Add(RetainmentCfg.GetXml(suppressDefaults));
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
            return GetXml("analogGroup", suppressDefaults);
        }

    }//AnalogNeuronGroupSettings

}//Namespace
