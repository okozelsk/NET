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
    /// Configuration of a group of analog neurons.
    /// </summary>
    [Serializable]
    public class AnalogNeuronGroupSettings : RCNetBaseSettings, INeuronGroupSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "PoolAnalogNeuronGroupType";
        //Default values
        /// <summary>
        /// The default value of the firing threshold.
        /// </summary>
        public const double DefaultFiringThreshold = 0.00125d;
        /// <summary>
        /// The default value of the maximum age of the past activation for the evaluation of the firing event.
        /// </summary>
        public const int DefaultThresholdMaxRefDeepness = 1;

        //Attribute properties
        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public double RelShare { get; }

        /// <inheritdoc/>
        public IActivationSettings ActivationCfg { get; }

        /// <summary>
        /// The firing threshold value. Every time the current normalized activation is higher than the normalized past reference activation by at least this threshold, it is evaluated as a firing event.
        /// </summary>
        public double FiringThreshold { get; }

        /// <summary>
        /// Maximum age of the past activation for the evaluation of the firing event.
        /// </summary>
        public int ThresholdMaxRefDeepness { get; }

        /// <inheritdoc/>
        public RandomValueSettings BiasCfg { get; }

        /// <summary>
        /// The configuration of the neurons' retainment property.
        /// </summary>
        public RetainmentSettings RetainmentCfg { get; }

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
        /// <param name="firingThreshold">The firing threshold value. Every time the current normalized activation is higher than the normalized past reference activation by at least this threshold, it is evaluated as a firing event.</param>
        /// <param name="thresholdMaxRefDeepness">Maximum age of the past activation for the evaluation of the firing event.</param>
        /// <param name="biasCfg">The configuration of the constant input bias.</param>
        /// <param name="retainmentCfg">The configuration of the neurons' retainment property.</param>
        public AnalogNeuronGroupSettings(string name,
                                         double relShare,
                                         IActivationSettings activationCfg,
                                         PredictorsProviderSettings predictorsCfg,
                                         double firingThreshold = DefaultFiringThreshold,
                                         int thresholdMaxRefDeepness = DefaultThresholdMaxRefDeepness,
                                         RandomValueSettings biasCfg = null,
                                         RetainmentSettings retainmentCfg = null
                                         )
        {
            Name = name;
            RelShare = relShare;
            ActivationCfg = (IActivationSettings)activationCfg.DeepClone();
            PredictorsCfg = (PredictorsProviderSettings)predictorsCfg.DeepClone();
            FiringThreshold = firingThreshold;
            ThresholdMaxRefDeepness = thresholdMaxRefDeepness;
            BiasCfg = biasCfg == null ? null : (RandomValueSettings)biasCfg.DeepClone();
            RetainmentCfg = retainmentCfg == null ? null : (RetainmentSettings)retainmentCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public AnalogNeuronGroupSettings(AnalogNeuronGroupSettings source)
            : this(source.Name, source.RelShare, source.ActivationCfg, source.PredictorsCfg, source.FiringThreshold, source.ThresholdMaxRefDeepness,
                  source.BiasCfg, source.RetainmentCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
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
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultFiringThreshold { get { return (FiringThreshold == DefaultFiringThreshold); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultThresholdMaxRefDeepness { get { return (ThresholdMaxRefDeepness == DefaultThresholdMaxRefDeepness); } }

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
            if (activationType != typeof(AFAnalogSQNLSettings) &&
                activationType != typeof(AFAnalogElliotSettings) &&
                activationType != typeof(AFAnalogGaussianSettings) &&
                activationType != typeof(AFAnalogISRUSettings) &&
                activationType != typeof(AFAnalogSigmoidSettings) &&
                activationType != typeof(AFAnalogSincSettings) &&
                activationType != typeof(AFAnalogSinusoidSettings) &&
                activationType != typeof(AFAnalogTanHSettings)
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

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new AnalogNeuronGroupSettings(this);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("analogGroup", suppressDefaults);
        }

    }//AnalogNeuronGroupSettings

}//Namespace
