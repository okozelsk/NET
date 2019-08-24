using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.IO;
using RCNet.Extensions;
using RCNet.XmlTools;
using RCNet.RandomValue;
using System.Xml.XPath;
using RCNet.Neural.Network.SM.Neuron;

namespace RCNet.Neural.Network.SM.Synapse
{
    /// <summary>
    /// Setup parameters of internal synapse
    /// </summary>
    [Serializable]
    public class InternalSynapseSettings
    {
        //Attribute properties
        /// <summary>
        /// Synapse's dynamics settings for S2S E2E neurons
        /// </summary>
        public DynamicsSettings S2SSynapseE2EDynamicsCfg { get; set; }
        /// <summary>
        /// Synapse's dynamics settings for S2S E2I neurons
        /// </summary>
        public DynamicsSettings S2SSynapseE2IDynamicsCfg { get; set; }
        /// <summary>
        /// Synapse's dynamics settings for S2S I2E neurons
        /// </summary>
        public DynamicsSettings S2SSynapseI2EDynamicsCfg { get; set; }
        /// <summary>
        /// Synapse's dynamics settings for S2S I2I neurons
        /// </summary>
        public DynamicsSettings S2SSynapseI2IDynamicsCfg { get; set; }
        /// <summary>
        /// Synapse's dynamics settings for A2S E2E neurons
        /// </summary>
        public DynamicsSettings A2SSynapseE2EDynamicsCfg { get; set; }
        /// <summary>
        /// Synapse's dynamics settings for A2S E2I neurons
        /// </summary>
        public DynamicsSettings A2SSynapseE2IDynamicsCfg { get; set; }
        /// <summary>
        /// Synapse's dynamics settings for A2S I2E neurons
        /// </summary>
        public DynamicsSettings A2SSynapseI2EDynamicsCfg { get; set; }
        /// <summary>
        /// Synapse's dynamics settings for A2S I2I neurons
        /// </summary>
        public DynamicsSettings A2SSynapseI2IDynamicsCfg { get; set; }
        /// <summary>
        /// Synapse's dynamics settings for S2A E2E neurons
        /// </summary>
        public DynamicsSettings S2ASynapseE2EDynamicsCfg { get; set; }
        /// <summary>
        /// Synapse's dynamics settings for S2A I2E neurons
        /// </summary>
        public DynamicsSettings S2ASynapseI2EDynamicsCfg { get; set; }
        /// <summary>
        /// Synapse's dynamics settings for S2A E2I neurons
        /// </summary>
        public DynamicsSettings S2ASynapseE2IDynamicsCfg { get; set; }
        /// <summary>
        /// Synapse's dynamics settings for S2A I2I neurons
        /// </summary>
        public DynamicsSettings S2ASynapseI2IDynamicsCfg { get; set; }
        /// <summary>
        /// Synapse's dynamics settings for A2A E2E neurons
        /// </summary>
        public DynamicsSettings A2ASynapseE2EDynamicsCfg { get; set; }
        /// <summary>
        /// Synapse's dynamics settings for A2A I2E neurons
        /// </summary>
        public DynamicsSettings A2ASynapseI2EDynamicsCfg { get; set; }
        /// <summary>
        /// Synapse's dynamics settings for A2A E2I neurons
        /// </summary>
        public DynamicsSettings A2ASynapseE2IDynamicsCfg { get; set; }
        /// <summary>
        /// Synapse's dynamics settings for A2A I2I neurons
        /// </summary>
        public DynamicsSettings A2ASynapseI2IDynamicsCfg { get; set; }



        //Constructors
        /// <summary>
        /// Creates an uninitialized instance
        /// </summary>
        public InternalSynapseSettings()
        {
            S2SSynapseE2EDynamicsCfg = null;
            S2SSynapseE2IDynamicsCfg = null;
            S2SSynapseI2EDynamicsCfg = null;
            S2SSynapseI2IDynamicsCfg = null;
            A2SSynapseE2EDynamicsCfg = null;
            A2SSynapseE2IDynamicsCfg = null;
            A2SSynapseI2EDynamicsCfg = null;
            A2SSynapseI2IDynamicsCfg = null;
            S2ASynapseE2EDynamicsCfg = null;
            S2ASynapseI2EDynamicsCfg = null;
            S2ASynapseE2IDynamicsCfg = null;
            S2ASynapseI2IDynamicsCfg = null;
            A2ASynapseE2EDynamicsCfg = null;
            A2ASynapseI2EDynamicsCfg = null;
            A2ASynapseE2IDynamicsCfg = null;
            A2ASynapseI2IDynamicsCfg = null;
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public InternalSynapseSettings(InternalSynapseSettings source)
        {
            S2SSynapseE2EDynamicsCfg = source.S2SSynapseE2EDynamicsCfg?.DeepClone();
            S2SSynapseE2IDynamicsCfg = source.S2SSynapseE2IDynamicsCfg?.DeepClone();
            S2SSynapseI2EDynamicsCfg = source.S2SSynapseI2EDynamicsCfg?.DeepClone();
            S2SSynapseI2IDynamicsCfg = source.S2SSynapseI2IDynamicsCfg?.DeepClone();
            A2SSynapseE2EDynamicsCfg = source.A2SSynapseE2EDynamicsCfg?.DeepClone();
            A2SSynapseE2IDynamicsCfg = source.A2SSynapseE2IDynamicsCfg?.DeepClone();
            A2SSynapseI2EDynamicsCfg = source.A2SSynapseI2EDynamicsCfg?.DeepClone();
            A2SSynapseI2IDynamicsCfg = source.A2SSynapseI2IDynamicsCfg?.DeepClone();
            S2ASynapseE2EDynamicsCfg = source.S2ASynapseE2EDynamicsCfg?.DeepClone();
            S2ASynapseI2EDynamicsCfg = source.S2ASynapseI2EDynamicsCfg?.DeepClone();
            S2ASynapseE2IDynamicsCfg = source.S2ASynapseE2IDynamicsCfg?.DeepClone();
            S2ASynapseI2IDynamicsCfg = source.S2ASynapseI2IDynamicsCfg?.DeepClone();
            A2ASynapseE2EDynamicsCfg = source.A2ASynapseE2EDynamicsCfg?.DeepClone();
            A2ASynapseI2EDynamicsCfg = source.A2ASynapseI2EDynamicsCfg?.DeepClone();
            A2ASynapseE2IDynamicsCfg = source.A2ASynapseE2IDynamicsCfg?.DeepClone();
            A2ASynapseI2IDynamicsCfg = source.A2ASynapseI2IDynamicsCfg?.DeepClone();
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public InternalSynapseSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.SM.Synapse.InternalSynapseSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement settingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            //Spiking target
            XElement dynamicsCfgElem;
            //S2SSynapseE2EDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsS2SEE");
            S2SSynapseE2EDynamicsCfg = dynamicsCfgElem == null ? new DynamicsSettings(0.5, 1100, 50, 3, true, true) : new DynamicsSettings(dynamicsCfgElem);
            //S2SSynapseE2IDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsS2SEI");
            S2SSynapseE2IDynamicsCfg = dynamicsCfgElem == null ? new DynamicsSettings(0.05, 125, 1200, 6, true, true) : new DynamicsSettings(dynamicsCfgElem);
            //S2SSynapseI2EDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsS2SIE");
            S2SSynapseI2EDynamicsCfg = dynamicsCfgElem == null ? new DynamicsSettings(0.25, 700, 20, 3, true, true) : new DynamicsSettings(dynamicsCfgElem);
            //S2SSynapseI2IDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsS2SII");
            S2SSynapseI2IDynamicsCfg = dynamicsCfgElem == null ? new DynamicsSettings(0.32, 144, 60, 6, true, true) : new DynamicsSettings(dynamicsCfgElem);
            //A2SSynapseE2EDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsA2SEE");
            A2SSynapseE2EDynamicsCfg = dynamicsCfgElem == null ? new DynamicsSettings(0.5, 1100, 50, 3, true, false) : new DynamicsSettings(dynamicsCfgElem);
            //A2SSynapseE2IDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsA2SEI");
            A2SSynapseE2IDynamicsCfg = dynamicsCfgElem == null ? new DynamicsSettings(0.05, 125, 1200, 6, true, false) : new DynamicsSettings(dynamicsCfgElem);
            //A2SSynapseI2EDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsA2SIE");
            A2SSynapseI2EDynamicsCfg = dynamicsCfgElem == null ? new DynamicsSettings(0.25, 700, 20, 3, true, false) : new DynamicsSettings(dynamicsCfgElem);
            //A2SSynapseI2IDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsA2SII");
            A2SSynapseI2IDynamicsCfg = dynamicsCfgElem == null ? new DynamicsSettings(0.32, 144, 60, 6, true, false) : new DynamicsSettings(dynamicsCfgElem);

            //Analog target
            //S2ASynapseE2EDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsS2AEE");
            S2ASynapseE2EDynamicsCfg = dynamicsCfgElem == null ? new DynamicsSettings(0, 0, 0, 0, false, false) : new DynamicsSettings(dynamicsCfgElem);
            //S2ASynapseI2EDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsS2AIE");
            S2ASynapseI2EDynamicsCfg = dynamicsCfgElem == null ? new DynamicsSettings(0, 0, 0, 0, false, false) : new DynamicsSettings(dynamicsCfgElem);
            //S2ASynapseE2IDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsS2AEI");
            S2ASynapseE2IDynamicsCfg = dynamicsCfgElem == null ? new DynamicsSettings(0, 0, 0, 0, false, false) : new DynamicsSettings(dynamicsCfgElem);
            //S2ASynapseI2IDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsS2AII");
            S2ASynapseI2IDynamicsCfg = dynamicsCfgElem == null ? new DynamicsSettings(0, 0, 0, 0, false, false) : new DynamicsSettings(dynamicsCfgElem);
            //A2ASynapseE2EDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsA2AEE");
            A2ASynapseE2EDynamicsCfg = dynamicsCfgElem == null ? new DynamicsSettings(0, 0, 0, 0, false, false) : new DynamicsSettings(dynamicsCfgElem);
            //A2ASynapseI2EDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsA2AIE");
            A2ASynapseI2EDynamicsCfg = dynamicsCfgElem == null ? new DynamicsSettings(0, 0, 0, 0, false, false) : new DynamicsSettings(dynamicsCfgElem);
            //A2ASynapseE2IDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsA2AEI");
            A2ASynapseE2IDynamicsCfg = dynamicsCfgElem == null ? new DynamicsSettings(0, 0, 0, 0, false, false) : new DynamicsSettings(dynamicsCfgElem);
            //A2ASynapseI2IDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsA2AII");
            A2ASynapseI2IDynamicsCfg = dynamicsCfgElem == null ? new DynamicsSettings(0, 0, 0, 0, false, false) : new DynamicsSettings(dynamicsCfgElem);
            return;
        }

        //Methods
        /// <summary>
        /// Returns appropriate dynamics settings
        /// </summary>
        /// <param name="sourceNeuronType">Type of the source neuron</param>
        /// <param name="sourceNeuronRole">Role of the source neuron</param>
        /// <param name="targetNeuronType">Type of the target neuron</param>
        /// <param name="targetNeuronRole">Role of the target neuron</param>
        public DynamicsSettings GetDynamicsSettings(CommonEnums.NeuronSignalType sourceNeuronType,
                                                    CommonEnums.NeuronRole sourceNeuronRole,
                                                    CommonEnums.NeuronSignalType targetNeuronType,
                                                    CommonEnums.NeuronRole targetNeuronRole
                                                    )
        {

            if(sourceNeuronType == CommonEnums.NeuronSignalType.Spike &&
               targetNeuronType == CommonEnums.NeuronSignalType.Spike &&
               sourceNeuronRole == CommonEnums.NeuronRole.Excitatory &&
               targetNeuronRole == CommonEnums.NeuronRole.Excitatory
               )
            {
                return S2SSynapseE2EDynamicsCfg;
            }
            else if (sourceNeuronType == CommonEnums.NeuronSignalType.Spike &&
               targetNeuronType == CommonEnums.NeuronSignalType.Spike &&
               sourceNeuronRole == CommonEnums.NeuronRole.Excitatory &&
               targetNeuronRole == CommonEnums.NeuronRole.Inhibitory
               )
            {
                return S2SSynapseE2IDynamicsCfg;
            }
            else if (sourceNeuronType == CommonEnums.NeuronSignalType.Spike &&
               targetNeuronType == CommonEnums.NeuronSignalType.Spike &&
               sourceNeuronRole == CommonEnums.NeuronRole.Inhibitory &&
               targetNeuronRole == CommonEnums.NeuronRole.Excitatory
               )
            {
                return S2SSynapseI2EDynamicsCfg;
            }
            else if (sourceNeuronType == CommonEnums.NeuronSignalType.Spike &&
               targetNeuronType == CommonEnums.NeuronSignalType.Spike &&
               sourceNeuronRole == CommonEnums.NeuronRole.Inhibitory &&
               targetNeuronRole == CommonEnums.NeuronRole.Inhibitory
               )
            {
                return S2SSynapseI2IDynamicsCfg;
            }
            else if (sourceNeuronType == CommonEnums.NeuronSignalType.Analog &&
               targetNeuronType == CommonEnums.NeuronSignalType.Spike &&
               sourceNeuronRole == CommonEnums.NeuronRole.Excitatory &&
               targetNeuronRole == CommonEnums.NeuronRole.Excitatory
               )
            {
                return A2SSynapseE2EDynamicsCfg;
            }
            else if (sourceNeuronType == CommonEnums.NeuronSignalType.Analog &&
               targetNeuronType == CommonEnums.NeuronSignalType.Spike &&
               sourceNeuronRole == CommonEnums.NeuronRole.Excitatory &&
               targetNeuronRole == CommonEnums.NeuronRole.Inhibitory
               )
            {
                return A2SSynapseE2IDynamicsCfg;
            }
            else if (sourceNeuronType == CommonEnums.NeuronSignalType.Analog &&
               targetNeuronType == CommonEnums.NeuronSignalType.Spike &&
               sourceNeuronRole == CommonEnums.NeuronRole.Inhibitory &&
               targetNeuronRole == CommonEnums.NeuronRole.Excitatory
               )
            {
                return A2SSynapseI2EDynamicsCfg;
            }
            else if (sourceNeuronType == CommonEnums.NeuronSignalType.Analog &&
               targetNeuronType == CommonEnums.NeuronSignalType.Spike &&
               sourceNeuronRole == CommonEnums.NeuronRole.Inhibitory &&
               targetNeuronRole == CommonEnums.NeuronRole.Inhibitory
               )
            {
                return A2SSynapseI2IDynamicsCfg;
            }
            else if (sourceNeuronType == CommonEnums.NeuronSignalType.Spike &&
               targetNeuronType == CommonEnums.NeuronSignalType.Analog &&
               sourceNeuronRole == CommonEnums.NeuronRole.Excitatory &&
               targetNeuronRole == CommonEnums.NeuronRole.Excitatory
               )
            {
                return S2ASynapseE2EDynamicsCfg;
            }
            else if (sourceNeuronType == CommonEnums.NeuronSignalType.Spike &&
               targetNeuronType == CommonEnums.NeuronSignalType.Analog &&
               sourceNeuronRole == CommonEnums.NeuronRole.Excitatory &&
               targetNeuronRole == CommonEnums.NeuronRole.Inhibitory
               )
            {
                return S2ASynapseE2IDynamicsCfg;
            }
            else if (sourceNeuronType == CommonEnums.NeuronSignalType.Spike &&
               targetNeuronType == CommonEnums.NeuronSignalType.Analog &&
               sourceNeuronRole == CommonEnums.NeuronRole.Inhibitory &&
               targetNeuronRole == CommonEnums.NeuronRole.Excitatory
               )
            {
                return S2ASynapseI2EDynamicsCfg;
            }
            else if (sourceNeuronType == CommonEnums.NeuronSignalType.Spike &&
               targetNeuronType == CommonEnums.NeuronSignalType.Analog &&
               sourceNeuronRole == CommonEnums.NeuronRole.Inhibitory &&
               targetNeuronRole == CommonEnums.NeuronRole.Inhibitory
               )
            {
                return S2ASynapseI2IDynamicsCfg;
            }
            else if (sourceNeuronType == CommonEnums.NeuronSignalType.Analog &&
               targetNeuronType == CommonEnums.NeuronSignalType.Analog &&
               sourceNeuronRole == CommonEnums.NeuronRole.Excitatory &&
               targetNeuronRole == CommonEnums.NeuronRole.Excitatory
               )
            {
                return A2ASynapseE2EDynamicsCfg;
            }
            else if (sourceNeuronType == CommonEnums.NeuronSignalType.Analog &&
               targetNeuronType == CommonEnums.NeuronSignalType.Analog &&
               sourceNeuronRole == CommonEnums.NeuronRole.Excitatory &&
               targetNeuronRole == CommonEnums.NeuronRole.Inhibitory
               )
            {
                return A2ASynapseE2IDynamicsCfg;
            }
            else if (sourceNeuronType == CommonEnums.NeuronSignalType.Analog &&
               targetNeuronType == CommonEnums.NeuronSignalType.Analog &&
               sourceNeuronRole == CommonEnums.NeuronRole.Inhibitory &&
               targetNeuronRole == CommonEnums.NeuronRole.Excitatory
               )
            {
                return A2ASynapseI2EDynamicsCfg;
            }
            else if (sourceNeuronType == CommonEnums.NeuronSignalType.Analog &&
               targetNeuronType == CommonEnums.NeuronSignalType.Analog &&
               sourceNeuronRole == CommonEnums.NeuronRole.Inhibitory &&
               targetNeuronRole == CommonEnums.NeuronRole.Inhibitory
               )
            {
                return A2ASynapseI2IDynamicsCfg;
            }
            return null;
        }

        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            InternalSynapseSettings cmpSettings = obj as InternalSynapseSettings;
            if (!Equals(S2SSynapseE2EDynamicsCfg, cmpSettings.S2SSynapseE2EDynamicsCfg) ||
                !Equals(S2SSynapseE2IDynamicsCfg, cmpSettings.S2SSynapseE2IDynamicsCfg) ||
                !Equals(S2SSynapseI2EDynamicsCfg, cmpSettings.S2SSynapseI2EDynamicsCfg) ||
                !Equals(S2SSynapseI2IDynamicsCfg, cmpSettings.S2SSynapseI2IDynamicsCfg) ||
                !Equals(A2SSynapseE2EDynamicsCfg, cmpSettings.A2SSynapseE2EDynamicsCfg) ||
                !Equals(A2SSynapseE2IDynamicsCfg, cmpSettings.A2SSynapseE2IDynamicsCfg) ||
                !Equals(A2SSynapseI2EDynamicsCfg, cmpSettings.A2SSynapseI2EDynamicsCfg) ||
                !Equals(A2SSynapseI2IDynamicsCfg, cmpSettings.A2SSynapseI2IDynamicsCfg) ||
                !Equals(S2ASynapseE2EDynamicsCfg, cmpSettings.S2ASynapseE2EDynamicsCfg) ||
                !Equals(S2ASynapseI2EDynamicsCfg, cmpSettings.S2ASynapseI2EDynamicsCfg) ||
                !Equals(S2ASynapseE2IDynamicsCfg, cmpSettings.S2ASynapseE2IDynamicsCfg) ||
                !Equals(S2ASynapseI2IDynamicsCfg, cmpSettings.S2ASynapseI2IDynamicsCfg) ||
                !Equals(A2ASynapseE2EDynamicsCfg, cmpSettings.A2ASynapseE2EDynamicsCfg) ||
                !Equals(A2ASynapseI2EDynamicsCfg, cmpSettings.A2ASynapseI2EDynamicsCfg) ||
                !Equals(A2ASynapseE2IDynamicsCfg, cmpSettings.A2ASynapseE2IDynamicsCfg) ||
                !Equals(A2ASynapseI2IDynamicsCfg, cmpSettings.A2ASynapseI2IDynamicsCfg)
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
            return base.GetHashCode();
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public InternalSynapseSettings DeepClone()
        {
            InternalSynapseSettings clone = new InternalSynapseSettings(this);
            return clone;
        }

        //Inner classes
        public class DynamicsSettings
        {
            //Attribute properties
            /// <summary>
            /// Synapse's resting efficacy (average probability of neurotransmitter release)
            /// </summary>
            public double RestingEfficacy { get; set; }
            /// <summary>
            /// Synapse's efficacy depression model time constant (ms)
            /// </summary>
            public double TauDepression { get; set; }
            /// <summary>
            /// Synapse's efficacy facilitation model time constant (ms)
            /// </summary>
            public double TauFacilitation { get; set; }
            /// <summary>
            /// Synapse's post-synaptic current decay time constant (ms)
            /// </summary>
            public double TauPostSynapticCurrentDecay { get; set; }
            /// <summary>
            /// Specifies whether to apply short-term plasticity
            /// </summary>
            public bool ApplyShortTermPlasticity { get; set; }
            /// <summary>
            /// Specifies whether to apply post-synaptic current injections
            /// </summary>
            public bool ApplyPostSynapticCurrent { get; set; }
            /// <summary>
            /// Synapse's random weight settings
            /// </summary>
            public RandomValueSettings WeightCfg { get; set; }

            //Constructors
            /// <summary>
            /// Creates an uninitialized instance
            /// </summary>
            public DynamicsSettings()
            {
                RestingEfficacy = 0;
                TauFacilitation = 0;
                TauDepression = 0;
                TauPostSynapticCurrentDecay = 0;
                ApplyShortTermPlasticity = true;
                ApplyPostSynapticCurrent = true;
                WeightCfg = null;
                return;
            }

            /// <summary>
            /// The deep copy constructor
            /// </summary>
            /// <param name="source">Source instance</param>
            public DynamicsSettings(DynamicsSettings source)
            {
                RestingEfficacy = source.RestingEfficacy;
                TauFacilitation = source.TauFacilitation;
                TauDepression = source.TauDepression;
                TauPostSynapticCurrentDecay = source.TauPostSynapticCurrentDecay;
                ApplyShortTermPlasticity = source.ApplyShortTermPlasticity;
                ApplyPostSynapticCurrent = source.ApplyPostSynapticCurrent;
                WeightCfg = null;
                if (source.WeightCfg != null)
                {
                    WeightCfg = source.WeightCfg.DeepClone();
                }
                return;
            }

            /// <summary>
            /// Creates the instance and initialize it from given xml element.
            /// </summary>
            /// <param name="elem">
            /// Xml data containing settings.
            /// Content of xml element is not validated against the xml schema.
            /// </param>
            public DynamicsSettings(XElement settingsElem)
            {
                if (settingsElem != null)
                {
                    //Parsing
                    //Resting efficacy
                    RestingEfficacy = double.Parse(settingsElem.Attribute("restingEfficacy").Value, CultureInfo.InvariantCulture);
                    //Efficacy depression
                    TauDepression = double.Parse(settingsElem.Attribute("tauDepression").Value, CultureInfo.InvariantCulture);
                    //Efficacy facilitation
                    TauFacilitation = double.Parse(settingsElem.Attribute("tauFacilitation").Value, CultureInfo.InvariantCulture);
                    //Decay of the post-synaptic current
                    TauPostSynapticCurrentDecay = double.Parse(settingsElem.Attribute("tauPostSynapticCurrentDecay").Value, CultureInfo.InvariantCulture);
                    //Apply short-term plasticity ?
                    ApplyShortTermPlasticity = bool.Parse(settingsElem.Attribute("applyShortTermPlasticity").Value);
                    //Apply dynamic decay?
                    ApplyPostSynapticCurrent = bool.Parse(settingsElem.Attribute("applyPostSynapticCurrent").Value);
                    //Weight
                    XElement weightCfgElem = settingsElem.Descendants("weight").FirstOrDefault();
                    if (weightCfgElem != null)
                    {
                        WeightCfg = new RandomValueSettings(settingsElem.Descendants("weight").FirstOrDefault());
                    }
                    else
                    {
                        WeightCfg = new RandomValueSettings(0, 1);
                    }
                }
                return;
            }

            /// <summary>
            /// Creates initialized instance
            /// </summary>
            /// <param name="restingEfficacy">Synapse's resting efficacy (average probability of neurotransmitter release)</param>
            /// <param name="tauDepression">Synapse's efficacy depression model time constant (ms)</param>
            /// <param name="tauFacilitation">Synapse's efficacy facilitation model time constant (ms)</param>
            /// <param name="tauPostSynapticCurrentDecay">Synapse's post-synaptic current decay time constant (ms)</param>
            /// <param name="applyShortTermPlasticity">Specifies whether to apply short-term plasticity</param>
            /// <param name="applyPostSynapticCurrent">Specifies whether to apply post-synaptic current injections</param>
            /// <param name="weightCfg">Synapse's random weight settings</param>
            public DynamicsSettings(double restingEfficacy,
                                    double tauDepression,
                                    double tauFacilitation,
                                    double tauPostSynapticCurrentDecay,
                                    bool applyShortTermPlasticity,
                                    bool applyPostSynapticCurrent,
                                    RandomValueSettings weightCfg = null
                                    )
            {
                RestingEfficacy = restingEfficacy;
                TauDepression = tauDepression;
                TauFacilitation = tauFacilitation;
                TauPostSynapticCurrentDecay = tauPostSynapticCurrentDecay;
                ApplyShortTermPlasticity = applyShortTermPlasticity;
                ApplyPostSynapticCurrent = applyPostSynapticCurrent;
                WeightCfg = (weightCfg == null ? new RandomValueSettings(0, 1) : weightCfg.DeepClone());
                return;
            }

            //Methods
            /// <summary>
            /// See the base.
            /// </summary>
            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                DynamicsSettings cmpSettings = obj as DynamicsSettings;
                if (RestingEfficacy != cmpSettings.RestingEfficacy ||
                    TauFacilitation != cmpSettings.TauFacilitation ||
                    TauDepression != cmpSettings.TauDepression ||
                    TauPostSynapticCurrentDecay != cmpSettings.TauPostSynapticCurrentDecay ||
                    ApplyShortTermPlasticity != cmpSettings.ApplyShortTermPlasticity ||
                    ApplyPostSynapticCurrent != cmpSettings.ApplyPostSynapticCurrent ||
                    !Equals(WeightCfg, cmpSettings.WeightCfg)
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
                return base.GetHashCode();
            }

            /// <summary>
            /// Creates the deep copy instance of this instance
            /// </summary>
            public DynamicsSettings DeepClone()
            {
                DynamicsSettings clone = new DynamicsSettings(this);
                return clone;
            }

        }//DynamicsSettings



    }//InternalSynapseSettings

}//Namespace

