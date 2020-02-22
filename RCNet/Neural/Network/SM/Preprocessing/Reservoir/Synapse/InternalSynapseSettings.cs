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
using RCNet.Neural.Activation;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Synapse.Dynamics;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Synapse
{
    /// <summary>
    /// Setup parameters of internal synapse
    /// </summary>
    [Serializable]
    public class InternalSynapseSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "InternalSynapseType";

        //Attribute properties
        /// <summary>
        /// Synapse's dynamics settings for S2S E2E neurons
        /// </summary>
        public S2SSynapseE2EDynamicsSettings S2SSynapseE2EDynamicsCfg { get; }
        /// <summary>
        /// Synapse's dynamics settings for S2S E2I neurons
        /// </summary>
        public S2SSynapseE2IDynamicsSettings S2SSynapseE2IDynamicsCfg { get; }
        /// <summary>
        /// Synapse's dynamics settings for S2S I2E neurons
        /// </summary>
        public S2SSynapseI2EDynamicsSettings S2SSynapseI2EDynamicsCfg { get; }
        /// <summary>
        /// Synapse's dynamics settings for S2S I2I neurons
        /// </summary>
        public S2SSynapseI2IDynamicsSettings S2SSynapseI2IDynamicsCfg { get; }
        /// <summary>
        /// Synapse's dynamics settings for A2S E2E neurons
        /// </summary>
        public A2SSynapseE2EDynamicsSettings A2SSynapseE2EDynamicsCfg { get; }
        /// <summary>
        /// Synapse's dynamics settings for A2S E2I neurons
        /// </summary>
        public A2SSynapseE2IDynamicsSettings A2SSynapseE2IDynamicsCfg { get; }
        /// <summary>
        /// Synapse's dynamics settings for A2S I2E neurons
        /// </summary>
        public A2SSynapseI2EDynamicsSettings A2SSynapseI2EDynamicsCfg { get; }
        /// <summary>
        /// Synapse's dynamics settings for A2S I2I neurons
        /// </summary>
        public A2SSynapseI2IDynamicsSettings A2SSynapseI2IDynamicsCfg { get; }
        /// <summary>
        /// Synapse's dynamics settings for S2A E2E neurons
        /// </summary>
        public S2ASynapseE2EDynamicsSettings S2ASynapseE2EDynamicsCfg { get; }
        /// <summary>
        /// Synapse's dynamics settings for S2A I2E neurons
        /// </summary>
        public S2ASynapseI2EDynamicsSettings S2ASynapseI2EDynamicsCfg { get; }
        /// <summary>
        /// Synapse's dynamics settings for S2A E2I neurons
        /// </summary>
        public S2ASynapseE2IDynamicsSettings S2ASynapseE2IDynamicsCfg { get; }
        /// <summary>
        /// Synapse's dynamics settings for S2A I2I neurons
        /// </summary>
        public S2ASynapseI2IDynamicsSettings S2ASynapseI2IDynamicsCfg { get; }
        /// <summary>
        /// Synapse's dynamics settings for A2A E2E neurons
        /// </summary>
        public A2ASynapseE2EDynamicsSettings A2ASynapseE2EDynamicsCfg { get; }
        /// <summary>
        /// Synapse's dynamics settings for A2A I2E neurons
        /// </summary>
        public A2ASynapseI2EDynamicsSettings A2ASynapseI2EDynamicsCfg { get; }
        /// <summary>
        /// Synapse's dynamics settings for A2A E2I neurons
        /// </summary>
        public A2ASynapseE2IDynamicsSettings A2ASynapseE2IDynamicsCfg { get; }
        /// <summary>
        /// Synapse's dynamics settings for A2A I2I neurons
        /// </summary>
        public A2ASynapseI2IDynamicsSettings A2ASynapseI2IDynamicsCfg { get; }



        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public InternalSynapseSettings()
        {
            S2SSynapseE2EDynamicsCfg = new S2SSynapseE2EDynamicsSettings();
            S2SSynapseE2IDynamicsCfg = new S2SSynapseE2IDynamicsSettings();
            S2SSynapseI2EDynamicsCfg = new S2SSynapseI2EDynamicsSettings();
            S2SSynapseI2IDynamicsCfg = new S2SSynapseI2IDynamicsSettings();
            A2SSynapseE2EDynamicsCfg = new A2SSynapseE2EDynamicsSettings();
            A2SSynapseE2IDynamicsCfg = new A2SSynapseE2IDynamicsSettings();
            A2SSynapseI2EDynamicsCfg = new A2SSynapseI2EDynamicsSettings();
            A2SSynapseI2IDynamicsCfg = new A2SSynapseI2IDynamicsSettings();
            S2ASynapseE2EDynamicsCfg = new S2ASynapseE2EDynamicsSettings();
            S2ASynapseI2EDynamicsCfg = new S2ASynapseI2EDynamicsSettings();
            S2ASynapseE2IDynamicsCfg = new S2ASynapseE2IDynamicsSettings();
            S2ASynapseI2IDynamicsCfg = new S2ASynapseI2IDynamicsSettings();
            A2ASynapseE2EDynamicsCfg = new A2ASynapseE2EDynamicsSettings();
            A2ASynapseI2EDynamicsCfg = new A2ASynapseI2EDynamicsSettings();
            A2ASynapseE2IDynamicsCfg = new A2ASynapseE2IDynamicsSettings();
            A2ASynapseI2IDynamicsCfg = new A2ASynapseI2IDynamicsSettings();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="dynamicSettings">Specific dynamics settings</param>
        public InternalSynapseSettings(params DynamicsSettings[] dynamicSettings)
            :this()
        {
            foreach(DynamicsSettings ds in dynamicSettings)
            {
                if(ds.GetType() == typeof(S2SSynapseE2EDynamicsSettings))
                {
                    S2SSynapseE2EDynamicsCfg = (S2SSynapseE2EDynamicsSettings)ds;
                }
                else if (ds.GetType() == typeof(S2SSynapseE2IDynamicsSettings))
                {
                    S2SSynapseE2IDynamicsCfg = (S2SSynapseE2IDynamicsSettings)ds;
                }
                else if (ds.GetType() == typeof(S2SSynapseI2EDynamicsSettings))
                {
                    S2SSynapseI2EDynamicsCfg = (S2SSynapseI2EDynamicsSettings)ds;
                }
                else if (ds.GetType() == typeof(S2SSynapseI2IDynamicsSettings))
                {
                    S2SSynapseI2IDynamicsCfg = (S2SSynapseI2IDynamicsSettings)ds;
                }
                else if (ds.GetType() == typeof(A2SSynapseE2EDynamicsSettings))
                {
                    A2SSynapseE2EDynamicsCfg = (A2SSynapseE2EDynamicsSettings)ds;
                }
                else if (ds.GetType() == typeof(A2SSynapseE2IDynamicsSettings))
                {
                    A2SSynapseE2IDynamicsCfg = (A2SSynapseE2IDynamicsSettings)ds;
                }
                else if (ds.GetType() == typeof(A2SSynapseI2EDynamicsSettings))
                {
                    A2SSynapseI2EDynamicsCfg = (A2SSynapseI2EDynamicsSettings)ds;
                }
                else if (ds.GetType() == typeof(A2SSynapseI2IDynamicsSettings))
                {
                    A2SSynapseI2IDynamicsCfg = (A2SSynapseI2IDynamicsSettings)ds;
                }
                else if (ds.GetType() == typeof(S2ASynapseE2EDynamicsSettings))
                {
                    S2ASynapseE2EDynamicsCfg = (S2ASynapseE2EDynamicsSettings)ds;
                }
                else if (ds.GetType() == typeof(S2ASynapseE2IDynamicsSettings))
                {
                    S2ASynapseE2IDynamicsCfg = (S2ASynapseE2IDynamicsSettings)ds;
                }
                else if (ds.GetType() == typeof(S2ASynapseI2EDynamicsSettings))
                {
                    S2ASynapseI2EDynamicsCfg = (S2ASynapseI2EDynamicsSettings)ds;
                }
                else if (ds.GetType() == typeof(S2ASynapseI2IDynamicsSettings))
                {
                    S2ASynapseI2IDynamicsCfg = (S2ASynapseI2IDynamicsSettings)ds;
                }
                else if (ds.GetType() == typeof(A2ASynapseE2EDynamicsSettings))
                {
                    A2ASynapseE2EDynamicsCfg = (A2ASynapseE2EDynamicsSettings)ds;
                }
                else if (ds.GetType() == typeof(A2ASynapseE2IDynamicsSettings))
                {
                    A2ASynapseE2IDynamicsCfg = (A2ASynapseE2IDynamicsSettings)ds;
                }
                else if (ds.GetType() == typeof(A2ASynapseI2EDynamicsSettings))
                {
                    A2ASynapseI2EDynamicsCfg = (A2ASynapseI2EDynamicsSettings)ds;
                }
                else if (ds.GetType() == typeof(A2ASynapseI2IDynamicsSettings))
                {
                    A2ASynapseI2IDynamicsCfg = (A2ASynapseI2IDynamicsSettings)ds;
                }
            }
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public InternalSynapseSettings(InternalSynapseSettings source)
        {
            S2SSynapseE2EDynamicsCfg = (S2SSynapseE2EDynamicsSettings)source.S2SSynapseE2EDynamicsCfg.DeepClone();
            S2SSynapseE2IDynamicsCfg = (S2SSynapseE2IDynamicsSettings)source.S2SSynapseE2IDynamicsCfg.DeepClone();
            S2SSynapseI2EDynamicsCfg = (S2SSynapseI2EDynamicsSettings)source.S2SSynapseI2EDynamicsCfg.DeepClone();
            S2SSynapseI2IDynamicsCfg = (S2SSynapseI2IDynamicsSettings)source.S2SSynapseI2IDynamicsCfg.DeepClone();
            A2SSynapseE2EDynamicsCfg = (A2SSynapseE2EDynamicsSettings)source.A2SSynapseE2EDynamicsCfg.DeepClone();
            A2SSynapseE2IDynamicsCfg = (A2SSynapseE2IDynamicsSettings)source.A2SSynapseE2IDynamicsCfg.DeepClone();
            A2SSynapseI2EDynamicsCfg = (A2SSynapseI2EDynamicsSettings)source.A2SSynapseI2EDynamicsCfg.DeepClone();
            A2SSynapseI2IDynamicsCfg = (A2SSynapseI2IDynamicsSettings)source.A2SSynapseI2IDynamicsCfg.DeepClone();
            S2ASynapseE2EDynamicsCfg = (S2ASynapseE2EDynamicsSettings)source.S2ASynapseE2EDynamicsCfg.DeepClone();
            S2ASynapseI2EDynamicsCfg = (S2ASynapseI2EDynamicsSettings)source.S2ASynapseI2EDynamicsCfg.DeepClone();
            S2ASynapseE2IDynamicsCfg = (S2ASynapseE2IDynamicsSettings)source.S2ASynapseE2IDynamicsCfg.DeepClone();
            S2ASynapseI2IDynamicsCfg = (S2ASynapseI2IDynamicsSettings)source.S2ASynapseI2IDynamicsCfg.DeepClone();
            A2ASynapseE2EDynamicsCfg = (A2ASynapseE2EDynamicsSettings)source.A2ASynapseE2EDynamicsCfg.DeepClone();
            A2ASynapseI2EDynamicsCfg = (A2ASynapseI2EDynamicsSettings)source.A2ASynapseI2EDynamicsCfg.DeepClone();
            A2ASynapseE2IDynamicsCfg = (A2ASynapseE2IDynamicsSettings)source.A2ASynapseE2IDynamicsCfg.DeepClone();
            A2ASynapseI2IDynamicsCfg = (A2ASynapseI2IDynamicsSettings)source.A2ASynapseI2IDynamicsCfg.DeepClone();
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
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            //Spiking target
            XElement dynamicsCfgElem;
            //S2SSynapseE2EDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsS2SEE");
            S2SSynapseE2EDynamicsCfg = dynamicsCfgElem == null ? new S2SSynapseE2EDynamicsSettings() : new S2SSynapseE2EDynamicsSettings(dynamicsCfgElem);
            //S2SSynapseE2IDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsS2SEI");
            S2SSynapseE2IDynamicsCfg = dynamicsCfgElem == null ? new S2SSynapseE2IDynamicsSettings() : new S2SSynapseE2IDynamicsSettings(dynamicsCfgElem);
            //S2SSynapseI2EDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsS2SIE");
            S2SSynapseI2EDynamicsCfg = dynamicsCfgElem == null ? new S2SSynapseI2EDynamicsSettings() : new S2SSynapseI2EDynamicsSettings(dynamicsCfgElem);
            //S2SSynapseI2IDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsS2SII");
            S2SSynapseI2IDynamicsCfg = dynamicsCfgElem == null ? new S2SSynapseI2IDynamicsSettings() : new S2SSynapseI2IDynamicsSettings(dynamicsCfgElem);
            //A2SSynapseE2EDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsA2SEE");
            A2SSynapseE2EDynamicsCfg = dynamicsCfgElem == null ? new A2SSynapseE2EDynamicsSettings() : new A2SSynapseE2EDynamicsSettings(dynamicsCfgElem);
            //A2SSynapseE2IDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsA2SEI");
            A2SSynapseE2IDynamicsCfg = dynamicsCfgElem == null ? new A2SSynapseE2IDynamicsSettings() : new A2SSynapseE2IDynamicsSettings(dynamicsCfgElem);
            //A2SSynapseI2EDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsA2SIE");
            A2SSynapseI2EDynamicsCfg = dynamicsCfgElem == null ? new A2SSynapseI2EDynamicsSettings() : new A2SSynapseI2EDynamicsSettings(dynamicsCfgElem);
            //A2SSynapseI2IDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsA2SII");
            A2SSynapseI2IDynamicsCfg = dynamicsCfgElem == null ? new A2SSynapseI2IDynamicsSettings() : new A2SSynapseI2IDynamicsSettings(dynamicsCfgElem);
            //Analog target
            //S2ASynapseE2EDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsS2AEE");
            S2ASynapseE2EDynamicsCfg = dynamicsCfgElem == null ? new S2ASynapseE2EDynamicsSettings() : new S2ASynapseE2EDynamicsSettings(dynamicsCfgElem);
            //S2ASynapseI2EDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsS2AIE");
            S2ASynapseI2EDynamicsCfg = dynamicsCfgElem == null ? new S2ASynapseI2EDynamicsSettings() : new S2ASynapseI2EDynamicsSettings(dynamicsCfgElem);
            //S2ASynapseE2IDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsS2AEI");
            S2ASynapseE2IDynamicsCfg = dynamicsCfgElem == null ? new S2ASynapseE2IDynamicsSettings() : new S2ASynapseE2IDynamicsSettings(dynamicsCfgElem);
            //S2ASynapseI2IDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsS2AII");
            S2ASynapseI2IDynamicsCfg = dynamicsCfgElem == null ? new S2ASynapseI2IDynamicsSettings() : new S2ASynapseI2IDynamicsSettings(dynamicsCfgElem);
            //A2ASynapseE2EDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsA2AEE");
            A2ASynapseE2EDynamicsCfg = dynamicsCfgElem == null ? new A2ASynapseE2EDynamicsSettings() : new A2ASynapseE2EDynamicsSettings(dynamicsCfgElem);
            //A2ASynapseI2EDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsA2AIE");
            A2ASynapseI2EDynamicsCfg = dynamicsCfgElem == null ? new A2ASynapseI2EDynamicsSettings() : new A2ASynapseI2EDynamicsSettings(dynamicsCfgElem);
            //A2ASynapseE2IDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsA2AEI");
            A2ASynapseE2IDynamicsCfg = dynamicsCfgElem == null ? new A2ASynapseE2IDynamicsSettings() : new A2ASynapseE2IDynamicsSettings(dynamicsCfgElem);
            //A2ASynapseI2IDynamicsCfg
            dynamicsCfgElem = settingsElem.XPathSelectElement("./dynamicsA2AII");
            A2ASynapseI2IDynamicsCfg = dynamicsCfgElem == null ? new A2ASynapseI2IDynamicsSettings() : new A2ASynapseI2IDynamicsSettings(dynamicsCfgElem);
            return;
        }

        //Properties
        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return S2SSynapseE2EDynamicsCfg.ContainsOnlyDefaults &&
                       S2SSynapseE2IDynamicsCfg.ContainsOnlyDefaults &&
                       S2SSynapseI2EDynamicsCfg.ContainsOnlyDefaults &&
                       S2SSynapseI2IDynamicsCfg.ContainsOnlyDefaults &&
                       A2SSynapseE2EDynamicsCfg.ContainsOnlyDefaults &&
                       A2SSynapseE2IDynamicsCfg.ContainsOnlyDefaults &&
                       A2SSynapseI2EDynamicsCfg.ContainsOnlyDefaults &&
                       A2SSynapseI2IDynamicsCfg.ContainsOnlyDefaults &&
                       S2ASynapseE2EDynamicsCfg.ContainsOnlyDefaults &&
                       S2ASynapseI2EDynamicsCfg.ContainsOnlyDefaults &&
                       S2ASynapseE2IDynamicsCfg.ContainsOnlyDefaults &&
                       S2ASynapseI2IDynamicsCfg.ContainsOnlyDefaults &&
                       A2ASynapseE2EDynamicsCfg.ContainsOnlyDefaults &&
                       A2ASynapseI2EDynamicsCfg.ContainsOnlyDefaults &&
                       A2ASynapseE2IDynamicsCfg.ContainsOnlyDefaults &&
                       A2ASynapseI2IDynamicsCfg.ContainsOnlyDefaults;
            }
        }

        //Methods
        /// <summary>
        /// Returns appropriate dynamics settings
        /// </summary>
        /// <param name="sourceNeuronType">Type of the source neuron</param>
        /// <param name="sourceNeuronRole">Role of the source neuron</param>
        /// <param name="targetNeuronType">Type of the target neuron</param>
        /// <param name="targetNeuronRole">Role of the target neuron</param>
        public DynamicsSettings GetDynamicsSettings(ActivationType sourceNeuronType,
                                                    NeuronCommon.NeuronRole sourceNeuronRole,
                                                    ActivationType targetNeuronType,
                                                    NeuronCommon.NeuronRole targetNeuronRole
                                                    )
        {

            if(sourceNeuronType == ActivationType.Spiking &&
               targetNeuronType == ActivationType.Spiking &&
               sourceNeuronRole == NeuronCommon.NeuronRole.Excitatory &&
               targetNeuronRole == NeuronCommon.NeuronRole.Excitatory
               )
            {
                return S2SSynapseE2EDynamicsCfg;
            }
            else if (sourceNeuronType == ActivationType.Spiking &&
               targetNeuronType == ActivationType.Spiking &&
               sourceNeuronRole == NeuronCommon.NeuronRole.Excitatory &&
               targetNeuronRole == NeuronCommon.NeuronRole.Inhibitory
               )
            {
                return S2SSynapseE2IDynamicsCfg;
            }
            else if (sourceNeuronType == ActivationType.Spiking &&
               targetNeuronType == ActivationType.Spiking &&
               sourceNeuronRole == NeuronCommon.NeuronRole.Inhibitory &&
               targetNeuronRole == NeuronCommon.NeuronRole.Excitatory
               )
            {
                return S2SSynapseI2EDynamicsCfg;
            }
            else if (sourceNeuronType == ActivationType.Spiking &&
               targetNeuronType == ActivationType.Spiking &&
               sourceNeuronRole == NeuronCommon.NeuronRole.Inhibitory &&
               targetNeuronRole == NeuronCommon.NeuronRole.Inhibitory
               )
            {
                return S2SSynapseI2IDynamicsCfg;
            }
            else if (sourceNeuronType == ActivationType.Analog &&
               targetNeuronType == ActivationType.Spiking &&
               sourceNeuronRole == NeuronCommon.NeuronRole.Excitatory &&
               targetNeuronRole == NeuronCommon.NeuronRole.Excitatory
               )
            {
                return A2SSynapseE2EDynamicsCfg;
            }
            else if (sourceNeuronType == ActivationType.Analog &&
               targetNeuronType == ActivationType.Spiking &&
               sourceNeuronRole == NeuronCommon.NeuronRole.Excitatory &&
               targetNeuronRole == NeuronCommon.NeuronRole.Inhibitory
               )
            {
                return A2SSynapseE2IDynamicsCfg;
            }
            else if (sourceNeuronType == ActivationType.Analog &&
               targetNeuronType == ActivationType.Spiking &&
               sourceNeuronRole == NeuronCommon.NeuronRole.Inhibitory &&
               targetNeuronRole == NeuronCommon.NeuronRole.Excitatory
               )
            {
                return A2SSynapseI2EDynamicsCfg;
            }
            else if (sourceNeuronType == ActivationType.Analog &&
               targetNeuronType == ActivationType.Spiking &&
               sourceNeuronRole == NeuronCommon.NeuronRole.Inhibitory &&
               targetNeuronRole == NeuronCommon.NeuronRole.Inhibitory
               )
            {
                return A2SSynapseI2IDynamicsCfg;
            }
            else if (sourceNeuronType == ActivationType.Spiking &&
               targetNeuronType == ActivationType.Analog &&
               sourceNeuronRole == NeuronCommon.NeuronRole.Excitatory &&
               targetNeuronRole == NeuronCommon.NeuronRole.Excitatory
               )
            {
                return S2ASynapseE2EDynamicsCfg;
            }
            else if (sourceNeuronType == ActivationType.Spiking &&
               targetNeuronType == ActivationType.Analog &&
               sourceNeuronRole == NeuronCommon.NeuronRole.Excitatory &&
               targetNeuronRole == NeuronCommon.NeuronRole.Inhibitory
               )
            {
                return S2ASynapseE2IDynamicsCfg;
            }
            else if (sourceNeuronType == ActivationType.Spiking &&
               targetNeuronType == ActivationType.Analog &&
               sourceNeuronRole == NeuronCommon.NeuronRole.Inhibitory &&
               targetNeuronRole == NeuronCommon.NeuronRole.Excitatory
               )
            {
                return S2ASynapseI2EDynamicsCfg;
            }
            else if (sourceNeuronType == ActivationType.Spiking &&
               targetNeuronType == ActivationType.Analog &&
               sourceNeuronRole == NeuronCommon.NeuronRole.Inhibitory &&
               targetNeuronRole == NeuronCommon.NeuronRole.Inhibitory
               )
            {
                return S2ASynapseI2IDynamicsCfg;
            }
            else if (sourceNeuronType == ActivationType.Analog &&
               targetNeuronType == ActivationType.Analog &&
               sourceNeuronRole == NeuronCommon.NeuronRole.Excitatory &&
               targetNeuronRole == NeuronCommon.NeuronRole.Excitatory
               )
            {
                return A2ASynapseE2EDynamicsCfg;
            }
            else if (sourceNeuronType == ActivationType.Analog &&
               targetNeuronType == ActivationType.Analog &&
               sourceNeuronRole == NeuronCommon.NeuronRole.Excitatory &&
               targetNeuronRole == NeuronCommon.NeuronRole.Inhibitory
               )
            {
                return A2ASynapseE2IDynamicsCfg;
            }
            else if (sourceNeuronType == ActivationType.Analog &&
               targetNeuronType == ActivationType.Analog &&
               sourceNeuronRole == NeuronCommon.NeuronRole.Inhibitory &&
               targetNeuronRole == NeuronCommon.NeuronRole.Excitatory
               )
            {
                return A2ASynapseI2EDynamicsCfg;
            }
            else if (sourceNeuronType == ActivationType.Analog &&
               targetNeuronType == ActivationType.Analog &&
               sourceNeuronRole == NeuronCommon.NeuronRole.Inhibitory &&
               targetNeuronRole == NeuronCommon.NeuronRole.Inhibitory
               )
            {
                return A2ASynapseI2IDynamicsCfg;
            }
            return null;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new InternalSynapseSettings(this);
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
            if (!suppressDefaults || !S2SSynapseE2EDynamicsCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(S2SSynapseE2EDynamicsCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !S2SSynapseE2IDynamicsCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(S2SSynapseE2IDynamicsCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !S2SSynapseI2EDynamicsCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(S2SSynapseI2EDynamicsCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !S2SSynapseI2IDynamicsCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(S2SSynapseI2IDynamicsCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !A2SSynapseE2EDynamicsCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(A2SSynapseE2EDynamicsCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !A2SSynapseE2IDynamicsCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(A2SSynapseE2IDynamicsCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !A2SSynapseI2EDynamicsCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(A2SSynapseI2EDynamicsCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !A2SSynapseI2IDynamicsCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(A2SSynapseI2IDynamicsCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !S2ASynapseE2EDynamicsCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(S2ASynapseE2EDynamicsCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !S2ASynapseE2IDynamicsCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(S2ASynapseE2IDynamicsCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !S2ASynapseI2EDynamicsCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(S2ASynapseI2EDynamicsCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !S2ASynapseI2IDynamicsCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(S2ASynapseI2IDynamicsCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !A2ASynapseE2EDynamicsCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(A2ASynapseE2EDynamicsCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !A2ASynapseE2IDynamicsCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(A2ASynapseE2IDynamicsCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !A2ASynapseI2EDynamicsCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(A2ASynapseI2EDynamicsCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !A2ASynapseI2IDynamicsCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(A2ASynapseI2IDynamicsCfg.GetXml(suppressDefaults));
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
            return GetXml("synapse", suppressDefaults);
        }


    }//InternalSynapseSettings

}//Namespace

