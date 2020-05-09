using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using RCNet.Extensions;
using RCNet.XmlTools;
using RCNet.RandomValue;
using RCNet.Neural.Activation;
using RCNet.Neural.Network.SM.Preprocessing.Neuron;
using RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor;

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
        /// Every time the new normalized activation value is higher than the previous
        /// normalized activation value by at least the threshold, it is evaluated as a firing event
        /// </summary>
        public const double DefaultFiringThreshold = 0.00125d;
        /// <summary>
        /// Default restriction of signaling
        /// </summary>
        public const NeuronCommon.NeuronSignalingRestrictionType DefaultSignalingRestriction = NeuronCommon.NeuronSignalingRestrictionType.NoRestriction;

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
        /// Restriction of neuron's output signaling
        /// </summary>
        public NeuronCommon.NeuronSignalingRestrictionType SignalingRestriction { get; }

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
        public PredictorsSettings PredictorsCfg { get; }
        
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
        /// <param name="firingThreshold">
        /// A number between 0 and 1 (LT1). Every time the new normalized activation value is higher than the previous
        /// normalized activation value by at least the threshold, it is evaluated as a firing event.
        /// </param>
        /// <param name="signalingRestriction">Specifies what type of signal will be supported</param>
        /// <param name="biasCfg">Each neuron within the group receives constant input bias. Value of the neuron's bias is driven by this random settings</param>
        /// <param name="retainmentCfg">Neurons' retainment property configuration</param>
        /// <param name="predictorsCfg">Configuration of the predictors</param>
        public AnalogNeuronGroupSettings(string name,
                                         double relShare,
                                         RCNetBaseSettings activationCfg,
                                         double firingThreshold = DefaultFiringThreshold,
                                         NeuronCommon.NeuronSignalingRestrictionType signalingRestriction = DefaultSignalingRestriction,
                                         RandomValueSettings biasCfg = null,
                                         RetainmentSettings retainmentCfg = null,
                                         PredictorsSettings predictorsCfg = null
                                         )
        {
            Name = name;
            RelShare = relShare;
            ActivationCfg = activationCfg.DeepClone();
            FiringThreshold = firingThreshold;
            SignalingRestriction = signalingRestriction;
            BiasCfg = biasCfg == null ? null : (RandomValueSettings)biasCfg.DeepClone();
            RetainmentCfg = retainmentCfg == null ? null : (RetainmentSettings)retainmentCfg.DeepClone();
            PredictorsCfg = predictorsCfg == null ? null : (PredictorsSettings)predictorsCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public AnalogNeuronGroupSettings(AnalogNeuronGroupSettings source)
            :this(source.Name, source.RelShare, source.ActivationCfg, source.FiringThreshold, 
                  source.SignalingRestriction, source.BiasCfg, source.RetainmentCfg, source.PredictorsCfg)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing settings.</param>
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
            //Signaling restriction
            SignalingRestriction = (NeuronCommon.NeuronSignalingRestrictionType)Enum.Parse(typeof(NeuronCommon.NeuronSignalingRestrictionType), settingsElem.Attribute("signalingRestriction").Value, true);
            //Bias
            XElement biasSettingsElem = settingsElem.Elements("bias").FirstOrDefault();
            BiasCfg = biasSettingsElem == null ? null : new RandomValueSettings(biasSettingsElem);
            //Retainment
            XElement retainmentSettingsElem = settingsElem.Elements("retainment").FirstOrDefault();
            RetainmentCfg = retainmentSettingsElem == null ? null : new RetainmentSettings(retainmentSettingsElem);
            //Predictors
            XElement predictorsSettingsElem = settingsElem.Elements("predictors").FirstOrDefault();
            if (predictorsSettingsElem != null)
            {
                PredictorsCfg = new PredictorsSettings(predictorsSettingsElem);
            }
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
        public bool IsDefaultSignalingRestriction { get { return (SignalingRestriction == DefaultSignalingRestriction); } }

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
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (Name.Length == 0)
            {
                throw new Exception($"Name can not be empty.");
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
                throw new Exception($"Not allowed Activation settings {activationType.ToString()}.");
            }
            if (FiringThreshold < 0 || FiringThreshold > 1)
            {
                throw new Exception($"Invalid FiringThreshold {FiringThreshold.ToString(CultureInfo.InvariantCulture)}. FiringThreshold must be GE to 0 and LE to 1.");
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
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
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
            if (!suppressDefaults || !IsDefaultSignalingRestriction)
            {
                rootElem.Add(new XAttribute("signalingRestriction", SignalingRestriction.ToString()));
            }
            if(BiasCfg != null)
            {
                rootElem.Add(BiasCfg.GetXml("bias", suppressDefaults));
            }
            if(RetainmentCfg != null)
            {
                rootElem.Add(RetainmentCfg.GetXml(suppressDefaults));
            }
            if (PredictorsCfg != null && !PredictorsCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(PredictorsCfg.GetXml(suppressDefaults));
            }
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
            return GetXml("analogGroup", suppressDefaults);
        }

    }//PoolAnalogNeuronGroupSettings

}//Namespace
