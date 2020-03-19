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
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron.Predictor;

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
        //Default values
        /// <summary>
        /// Default readout density
        /// </summary>
        public const double DefaultReadoutDensity = 1d;

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
        /// Specifies what ratio of the neurons from this group can be used as a source of the readout predictors
        /// </summary>
        public double ReadoutDensity { get; }

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
        /// <param name="homogenousExcitabilityCfg">Configuration of the neuron's homogenous excitability</param>
        /// <param name="biasCfg">Each neuron within the group receives constant input bias. Value of the neuron's bias is driven by this random settings</param>
        /// <param name="predictorsCfg">Configuration of the predictors</param>
        /// <param name="readoutDensity">Specifies what ratio of the neurons from this group can be used as a source of the readout predictors</param>
        public SpikingNeuronGroupSettings(string name,
                                          double relShare,
                                          RCNetBaseSettings activationCfg,
                                          HomogenousExcitabilitySettings homogenousExcitabilityCfg,
                                          RandomValueSettings biasCfg = null,
                                          PredictorsSettings predictorsCfg = null,
                                          double readoutDensity = DefaultReadoutDensity
                                          )
        {
            Name = name;
            RelShare = relShare;
            ActivationCfg = activationCfg.DeepClone();
            HomogenousExcitabilityCfg = (HomogenousExcitabilitySettings)homogenousExcitabilityCfg.DeepClone();
            BiasCfg = biasCfg == null ? null : (RandomValueSettings)biasCfg.DeepClone();
            PredictorsCfg = predictorsCfg == null ? null : (PredictorsSettings)predictorsCfg.DeepClone();
            ReadoutDensity = readoutDensity;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public SpikingNeuronGroupSettings(SpikingNeuronGroupSettings source)
            :this(source.Name, source.RelShare, source.ActivationCfg, source.HomogenousExcitabilityCfg, source.BiasCfg,
                  source.PredictorsCfg, source.ReadoutDensity)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing settings.</param>
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
            ActivationCfg = ActivationFactory.LoadSettings(settingsElem.Descendants().First());
            //Homogenous excitability
            HomogenousExcitabilityCfg = new HomogenousExcitabilitySettings(settingsElem.Descendants("homogenousExcitability").First());
            //Readout neurons density
            ReadoutDensity = double.Parse(settingsElem.Attribute("readoutDensity").Value, CultureInfo.InvariantCulture);
            //Bias
            XElement biasSettingsElem = settingsElem.Descendants("bias").FirstOrDefault();
            BiasCfg = biasSettingsElem == null ? null : new RandomValueSettings(biasSettingsElem);
            //Predictors
            XElement predictorsSettingsElem = settingsElem.Descendants("predictors").FirstOrDefault();
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
        public ActivationType Type { get { return ActivationType.Spiking; } }

        /// <summary>
        /// Restriction of neuron's output signaling
        /// </summary>
        public NeuronCommon.NeuronSignalingRestrictionType SignalingRestriction { get { return NeuronCommon.NeuronSignalingRestrictionType.SpikingOnly; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultReadoutDensity { get { return (ReadoutDensity == DefaultReadoutDensity); } }

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
            if (activationType != typeof(SimpleIFSettings) &&
                activationType != typeof(LeakyIFSettings) &&
                activationType != typeof(ExpIFSettings) &&
                activationType != typeof(AdExpIFSettings) &&
                activationType != typeof(IzhikevichIFSettings) &&
                activationType != typeof(AutoIzhikevichIFSettings)
                )
            {
                throw new Exception($"Not allowed Activation settings {activationType.ToString()}.");
            }
            if (ReadoutDensity < 0)
            {
                throw new Exception($"Invalid ReadoutDensity {ReadoutDensity.ToString(CultureInfo.InvariantCulture)}. ReadoutDensity must be GE to 0.");
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
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("name", Name),
                                             new XAttribute("relShare", RelShare.ToString(CultureInfo.InvariantCulture)),
                                             ActivationCfg.GetXml(suppressDefaults),
                                             HomogenousExcitabilityCfg.GetXml(suppressDefaults)
                                             );
            if(!suppressDefaults || !IsDefaultReadoutDensity)
            {
                rootElem.Add(new XAttribute("readoutDensity", ReadoutDensity.ToString(CultureInfo.InvariantCulture)));
            }
            if(BiasCfg != null)
            {
                rootElem.Add(BiasCfg.GetXml("bias", suppressDefaults));
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
            return GetXml("spikingGroup", suppressDefaults);
        }

    }//PoolSpikingNeuronGroupSettings

}//Namespace
