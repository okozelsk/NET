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
using RCNet.Neural.Network.SM.Neuron;
using RCNet.Neural.Network.SM.Synapse;

namespace RCNet.Neural.Network.SM.Preprocessing
{
    /// <summary>
    /// Configuration of neural pool.
    /// </summary>
    [Serializable]
    public class PoolSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PoolCfgType";

        //Attribute properties
        /// <summary>
        /// Name of this pool
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Pool coordinates and dimensions. Pool is 3D.
        /// </summary>
        public PoolDimensions Dim { get; set; }
        /// <summary>
        /// Settings of the neuron groups in the pool.
        /// </summary>
        public List<NeuronGroupSettings> NeuronGroups { get; set; }
        /// <summary>
        /// Configuration of the predictors
        /// </summary>
        public HiddenNeuronPredictorsSettings PredictorsCfg { get; set; }
        /// <summary>
        /// Configuration of the pool's neurons interconnection
        /// </summary>
        public InterconnectionSettings InterconnectionCfg { get; set; }

        //Constructors
        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public PoolSettings(PoolSettings source)
        {
            Name = source.Name;
            Dim = null;
            if(source.Dim != null)
            {
                Dim = new PoolDimensions(source.Dim.X, source.Dim.Y, source.Dim.Z, source.Dim.DimX, source.Dim.DimY, source.Dim.DimZ);
            }
            NeuronGroups = new List<NeuronGroupSettings>(source.NeuronGroups.Count);
            foreach(NeuronGroupSettings item in source.NeuronGroups)
            {
                NeuronGroups.Add(item.DeepClone());
            }
            PredictorsCfg = source.PredictorsCfg?.DeepClone();
            InterconnectionCfg = source.InterconnectionCfg.DeepClone();
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing pool settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public PoolSettings(XElement elem)
        {
            //Validation
            XElement poolSettingsElem = Validate(elem, XsdTypeName);
            //Parsing
            //Name
            Name = poolSettingsElem.Attribute("name").Value;
            //Dimensions
            Dim = new PoolDimensions(int.Parse(poolSettingsElem.Attribute("x").Value, CultureInfo.InvariantCulture),
                                     int.Parse(poolSettingsElem.Attribute("y").Value, CultureInfo.InvariantCulture),
                                     int.Parse(poolSettingsElem.Attribute("z").Value, CultureInfo.InvariantCulture),
                                     int.Parse(poolSettingsElem.Attribute("dimX").Value, CultureInfo.InvariantCulture),
                                     int.Parse(poolSettingsElem.Attribute("dimY").Value, CultureInfo.InvariantCulture),
                                     int.Parse(poolSettingsElem.Attribute("dimZ").Value, CultureInfo.InvariantCulture)
                                     );
            //NeuronGroups
            XElement neuronGroupsElem = poolSettingsElem.Descendants("neuronGroups").First();
            double totalRelShare = 0;
            NeuronGroups = new List<NeuronGroupSettings>();
            //Analog neuron groups
            foreach(XElement neuronGroupElem in neuronGroupsElem.Descendants("analogGroup"))
            {
                NeuronGroupSettings ngs = new NeuronGroupSettings(neuronGroupElem, ActivationType.Analog);
                if (ngs.RelativeShare > 0)
                {
                    NeuronGroups.Add(ngs);
                    totalRelShare += ngs.RelativeShare;
                }
            }
            //Spiking neuron groups
            foreach (XElement neuronGroupElem in neuronGroupsElem.Descendants("spikingGroup"))
            {
                NeuronGroupSettings ngs = new NeuronGroupSettings(neuronGroupElem, ActivationType.Spiking);
                if (ngs.RelativeShare > 0)
                {
                    NeuronGroups.Add(ngs);
                    totalRelShare += ngs.RelativeShare;
                }
            }
            //Neuron groups counts
            int totalCount = 0;
            foreach(NeuronGroupSettings ngs in NeuronGroups)
            {
                double ratio = ngs.RelativeShare / totalRelShare;
                ngs.Count = (int)Math.Round(((double)Dim.Size) * ratio, 0);
                totalCount += ngs.Count;
            }
            while(totalCount != Dim.Size)
            {
                //Correction of neuron counts
                int sign = Math.Sign(Dim.Size - totalCount);
                if (sign < 0)
                {
                    NeuronGroups.Sort(NeuronGroupSettings.Comparer_desc);
                }
                else
                {
                    NeuronGroups.Sort(NeuronGroupSettings.Comparer_asc);
                }
                NeuronGroups[0].Count += sign;
                totalCount += sign;
                if (NeuronGroups[0].Count < 0)
                {
                    throw new Exception("Can't set proper neuron counts for the neuron groups.");
                }
            }

            //Predictors
            XElement predictorsElem = poolSettingsElem.Descendants("predictors").FirstOrDefault();
            if (predictorsElem != null)
            {
                PredictorsCfg = new HiddenNeuronPredictorsSettings(predictorsElem);
            }

            //Interconnection
            XElement interconnectionElem = poolSettingsElem.Descendants("interconnection").First();
            InterconnectionCfg = new InterconnectionSettings(interconnectionElem);
            return;
        }

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public PoolSettings DeepClone()
        {
            return new PoolSettings(this);
        }

        //Inner classes
        /// <summary>
        /// Class encapsulates specification of the groupped (excitatory or inhibitory) neurons
        /// </summary>
        [Serializable]
        public class NeuronGroupSettings
        {
            //Constants
            /// <summary>
            /// Default value for analog firing threshold
            /// </summary>
            public const double DefaultAnalogFiringThreshold = 0.00125d;
            //Attribute properties
            /// <summary>
            /// Name of the neuron group
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// Excitatory or Inhibitory role of the neurons
            /// </summary>
            public NeuronCommon.NeuronRole Role { get; set; }
            /// <summary>
            /// Used to compute Density attribute (how big relative portion of pool's neurons is formed by this group of the neurons)
            /// </summary>
            public double RelativeShare { get; set; }
            /// <summary>
            /// Determines what ratio of the neurons to use as the readout predictors
            /// </summary>
            public double ReadoutNeuronsDensity { get; set; }
            /// <summary>
            /// Computed attribute. What count of pool's neurons is formed by this group of the neurons
            /// </summary>
            public int Count { get; set; }
            /// <summary>
            /// Type of the activation function
            /// </summary>
            public ActivationType ActivationType;
            /// <summary>
            /// Common activation settings of the groupped neurons
            /// </summary>
            public Object ActivationCfg { get; set; }
            /// <summary>
            /// Restriction of neuron's output signaling
            /// </summary>
            public NeuronCommon.NeuronSignalingRestrictionType SignalingRestriction;
            /// <summary>
            /// Each pool's neuron can have its own constant input bias. Bias is always added to input signal of the neuron.
            /// A constant bias value of the neuron will be selected randomly according to the settings.
            /// </summary>
            public RandomValueSettings BiasCfg { get; set; }
            /// <summary>
            /// Firing threshold configuration for neurons having stateless analog activation function.
            /// </summary>
            public double AnalogFiringThreshold { get; set; }
            /// <summary>
            /// The parameter says how much of the neurons will have the Retainment property (leaky integrator neuron).
            /// Count = NumberOfAnalogNeurons * Density
            /// </summary>
            public double RetainmentNeuronsDensity { get; set; }
            /// <summary>
            /// If the neuron is selected to have the Retainment property then its retainment strength will be randomly selected
            /// following this settings
            /// </summary>
            public RandomValueSettings RetainmentStrengthCfg { get; set; }
            /// <summary>
            /// Configuration of the predictors
            /// </summary>
            public HiddenNeuronPredictorsSettings PredictorsCfg { get; set; }

            //Constructors
            /// <summary>
            /// The deep copy constructor
            /// </summary>
            /// <param name="source">Source instance</param>
            public NeuronGroupSettings(NeuronGroupSettings source)
            {
                Name = source.Name;
                Role = source.Role;
                RelativeShare = source.RelativeShare;
                ReadoutNeuronsDensity = source.ReadoutNeuronsDensity;
                Count = source.Count;
                ActivationType = source.ActivationType;
                ActivationCfg = ActivationFactory.DeepCloneActivationSettings(source.ActivationCfg);
                SignalingRestriction = source.SignalingRestriction;
                BiasCfg = source.BiasCfg?.DeepClone();
                AnalogFiringThreshold = source.AnalogFiringThreshold;
                RetainmentNeuronsDensity = source.RetainmentNeuronsDensity;
                RetainmentStrengthCfg = source.RetainmentStrengthCfg?.DeepClone();
                PredictorsCfg = source.PredictorsCfg?.DeepClone();
                return;
            }

            /// <summary>
            /// Creates the instance and initialize it from given xml element.
            /// </summary>
            /// <param name="settingsElem">Xml data containing settings.</param>
            /// <param name="activationType">Specifies sub-type of the neuron group</param>
            public NeuronGroupSettings(XElement settingsElem, ActivationType activationType)
            {
                ActivationType = activationType;
                //Parsing
                //Name
                Name = settingsElem.Attribute("name").Value;
                //Role
                Role = NeuronCommon.ParseNeuronRole(settingsElem.Attribute("role").Value);
                //Relative share
                RelativeShare = double.Parse(settingsElem.Attribute("relShare").Value, CultureInfo.InvariantCulture);
                //Readout neurons density
                ReadoutNeuronsDensity = double.Parse(settingsElem.Attribute("readoutDensity").Value, CultureInfo.InvariantCulture);
                //Activation settings
                ActivationCfg = ActivationFactory.LoadSettings(settingsElem.Descendants().First());
                //Bias
                XElement cfgElem = settingsElem.Descendants("bias").FirstOrDefault();
                BiasCfg = cfgElem == null ? null : new RandomValueSettings(cfgElem);
                //Spiking sub-type
                if (activationType == ActivationType.Spiking)
                {
                    //Output signaling restriction
                    SignalingRestriction = NeuronCommon.NeuronSignalingRestrictionType.SpikingOnly;
                    //Irrelevant settings
                    AnalogFiringThreshold = 0;
                    RetainmentNeuronsDensity = 0;
                    RetainmentStrengthCfg = null;
                }
                else
                {
                    //Analog sub-type
                    //Output signaling restriction
                    SignalingRestriction = NeuronCommon.ParseNeuronSignalingRestriction(settingsElem.Attribute("signalingRestriction").Value);
                    //Analog firing threshold
                    cfgElem = settingsElem.Descendants("firingThreshold").FirstOrDefault();
                    AnalogFiringThreshold = cfgElem == null ? DefaultAnalogFiringThreshold : double.Parse(cfgElem.Attribute("value").Value, CultureInfo.InvariantCulture);
                    //Retainment
                    cfgElem = settingsElem.Descendants("retainment").FirstOrDefault();
                    RetainmentNeuronsDensity = 0;
                    RetainmentStrengthCfg = null;
                    if (cfgElem != null)
                    {
                        RetainmentNeuronsDensity = double.Parse(cfgElem.Attribute("density").Value, CultureInfo.InvariantCulture);
                        RetainmentStrengthCfg = new RandomValueSettings(cfgElem.Descendants("strength").First());
                        if(RetainmentNeuronsDensity == 0 || (RetainmentStrengthCfg.Min == 0 && RetainmentStrengthCfg.Max == 0))
                        {
                            RetainmentNeuronsDensity = 0;
                            RetainmentStrengthCfg = null;
                        }
                    }
                }
                //Predictors
                XElement predictorsElem = settingsElem.Descendants("predictors").FirstOrDefault();
                if (predictorsElem != null)
                {
                    PredictorsCfg = new HiddenNeuronPredictorsSettings(predictorsElem);
                }
                return;
            }

            //Methods
            /// <summary>
            /// Ascending comparer for the sort operation
            /// </summary>
            /// <param name="item1">Instance 1</param>
            /// <param name="item2">Instance 2</param>
            public static int Comparer_asc(NeuronGroupSettings item1, NeuronGroupSettings item2)
            {
                return Math.Sign(item1.Count - item2.Count);
            }

            /// <summary>
            /// Descending comparer for the sort operation
            /// </summary>
            /// <param name="item1">Instance 1</param>
            /// <param name="item2">Instance 2</param>
            public static int Comparer_desc(NeuronGroupSettings item1, NeuronGroupSettings item2)
            {
                return Math.Sign(item2.Count - item1.Count);
            }

            /// <summary>
            /// Creates the deep copy instance of this instance
            /// </summary>
            public NeuronGroupSettings DeepClone()
            {
                return new NeuronGroupSettings(this);
            }


        }//NeuronGroupSettings

        /// <summary>
        /// Class encapsulates specification of pool's neurons interconnection
        /// </summary>
        [Serializable]
        public class InterconnectionSettings
        {
            //Constants
            //Attribute properties
            /// <summary>
            /// Collection of interconnection schemas to be applied
            /// </summary>
            public List<object> Schemas { get; }
            
            //Constructors
            /// <summary>
            /// Creates an uninitialized instance
            /// </summary>
            public InterconnectionSettings()
            {
                Schemas = new List<object>();
                return;
            }

            /// <summary>
            /// The deep copy constructor
            /// </summary>
            /// <param name="source">Source instance</param>
            public InterconnectionSettings(InterconnectionSettings source)
            {
                Schemas = new List<object>();
                if(Schemas != null)
                {
                    foreach(Object sourceSchema in source.Schemas)
                    {
                        if(sourceSchema.GetType() == typeof(RandomSchemaSettings))
                        {
                            Schemas.Add(((RandomSchemaSettings)sourceSchema).DeepClone());
                        }
                        else if(sourceSchema.GetType() == typeof(ChainSchemaSettings))
                        {
                            Schemas.Add(((ChainSchemaSettings)sourceSchema).DeepClone());
                        }
                        else
                        {
                            throw new Exception("Unknown interconnection schema");
                        }
                    }
                }
                return;
            }

            /// <summary>
            /// Creates the instance and initialize it from given xml element.
            /// </summary>
            /// <param name="settingsElem">Xml data containing settings.</param>
            public InterconnectionSettings(XElement settingsElem)
            {
                //Parsing
                Schemas = new List<object>();
                foreach (XElement schemaElem in settingsElem.Descendants())
                {
                    if(schemaElem.Name.LocalName == "randomSchema")
                    {
                        Schemas.Add(new RandomSchemaSettings(schemaElem));
                    }
                    else if(schemaElem.Name.LocalName == "chainSchema")
                    {
                        Schemas.Add(new ChainSchemaSettings(schemaElem));
                    }
                    else
                    {
                        //Ignore
                        ;
                    }
                }

                return;
            }

            //Methods
            /// <summary>
            /// Creates the deep copy instance of this instance
            /// </summary>
            public InterconnectionSettings DeepClone()
            {
                return new InterconnectionSettings(this);
            }

            //Inner classes
            /// <summary>
            /// Class contains configuration of the Random schema of pool's neurons interconnection
            /// </summary>
            [Serializable]
            public class RandomSchemaSettings
            {
                //Constants
                //Attribute properties
                /// <summary>
                /// Density of interconnected neurons.
                /// Each pool neuron will be connected as a source neuron for Pool.Size * Density neurons.
                /// </summary>
                public double Density { get; set; }
                /// <summary>
                /// EE synapses ratio
                /// </summary>
                public double RatioEE { get; set; }
                /// <summary>
                /// EI synapses ratio
                /// </summary>
                public double RatioEI { get; set; }
                /// <summary>
                /// IE synapses ratio
                /// </summary>
                public double RatioIE { get; set; }
                /// <summary>
                /// II synapses ratio
                /// </summary>
                public double RatioII { get; set; }
                /// <summary>
                /// Average distance of interconnected neurons.
                /// 0 means random distance.
                /// </summary>
                public double AvgDistance { get; set; }
                /// <summary>
                /// Specifies whether to allow neurons to be self connected
                /// </summary>
                public bool AllowSelfConnection { get; set; }
                /// <summary>
                /// Specifies whether to keep for each neuron constant number of incoming interconnections
                /// </summary>
                public bool ConstantNumOfConnections { get; set; }
                /// <summary>
                /// Specifies whether connections of this schema will replace existing connections
                /// </summary>
                public bool ReplaceExistingConnections { get; set; }
                /// <summary>
                /// Number of applications of this schema
                /// </summary>
                public int Repetitions { get; set; }
                /// <summary>
                /// Neurons in the pool are interconnected through synapses.
                /// </summary>
                public InternalSynapseSettings SynapseCfg { get; set; }

                //Constructors
                /// <summary>
                /// Creates an uninitialized instance
                /// </summary>
                public RandomSchemaSettings()
                {
                    Density = 0;
                    RatioEE = 0.3;
                    RatioEI = 0.2;
                    RatioIE = 0.4;
                    RatioII = 0.1;
                    AvgDistance = 0;
                    AllowSelfConnection = true;
                    ConstantNumOfConnections = false;
                    ReplaceExistingConnections = true;
                    Repetitions = 1;
                    SynapseCfg = null;
                    return;
                }

                /// <summary>
                /// The deep copy constructor
                /// </summary>
                /// <param name="source">Source instance</param>
                public RandomSchemaSettings(RandomSchemaSettings source)
                {
                    Density = source.Density;
                    RatioEE = source.RatioEE;
                    RatioEI = source.RatioEI;
                    RatioIE = source.RatioIE;
                    RatioII = source.RatioII;
                    AvgDistance = source.AvgDistance;
                    AllowSelfConnection = source.AllowSelfConnection;
                    ConstantNumOfConnections = source.ConstantNumOfConnections;
                    ReplaceExistingConnections = source.ReplaceExistingConnections;
                    Repetitions = source.Repetitions;
                    SynapseCfg = source.SynapseCfg.DeepClone();
                    return;
                }

                /// <summary>
                /// Creates the instance and initialize it from given xml element.
                /// </summary>
                /// <param name="settingsElem">
                /// Xml data containing settings.
                /// Content of xml element is always validated against the xml schema.
                /// </param>
                public RandomSchemaSettings(XElement settingsElem)
                {
                    //Parsing
                    //Density
                    Density = double.Parse(settingsElem.Attribute("density").Value, CultureInfo.InvariantCulture);
                    //Ratios
                    double relShareEE = double.Parse(settingsElem.Attribute("relShareEE").Value, CultureInfo.InvariantCulture);
                    double relShareEI = double.Parse(settingsElem.Attribute("relShareEI").Value, CultureInfo.InvariantCulture);
                    double relShareIE = double.Parse(settingsElem.Attribute("relShareIE").Value, CultureInfo.InvariantCulture);
                    double relShareII = double.Parse(settingsElem.Attribute("relShareII").Value, CultureInfo.InvariantCulture);
                    double sum = relShareEE + relShareEI + relShareIE + relShareII;
                    RatioEE = relShareEE / sum;
                    RatioEI = relShareEI / sum;
                    RatioIE = relShareIE / sum;
                    RatioII = relShareII / sum;
                    //Average distance
                    AvgDistance = settingsElem.Attribute("avgDistance").Value == "NA" ? 0d : double.Parse(settingsElem.Attribute("avgDistance").Value, CultureInfo.InvariantCulture);
                    //Allow self connections?
                    AllowSelfConnection = bool.Parse(settingsElem.Attribute("allowSelfConnection").Value);
                    //Will each neuron have the same number of connections?
                    ConstantNumOfConnections = bool.Parse(settingsElem.Attribute("constantNumOfConnections").Value);
                    //Will be replaced existing connections?
                    ReplaceExistingConnections = bool.Parse(settingsElem.Attribute("replaceExistingConnections").Value);
                    //Number of schema repetitions
                    Repetitions = int.Parse(settingsElem.Attribute("repetitions").Value, CultureInfo.InvariantCulture);
                    //Synapse
                    SynapseCfg = new InternalSynapseSettings(settingsElem.Descendants("synapse").First());
                    return;
                }

                //Methods
                /// <summary>
                /// Creates the deep copy instance of this instance
                /// </summary>
                public RandomSchemaSettings DeepClone()
                {
                    return new RandomSchemaSettings(this);
                }

            }//RandomSchemaSettings

            /// <summary>
            /// Class contains configuration of the Chain schema of pool's neurons interconnection
            /// </summary>
            [Serializable]
            public class ChainSchemaSettings
            {
                //Constants
                //Attribute properties
                /// <summary>
                /// Ratio of involved neurons.
                /// </summary>
                public double Ratio { get; set; }
                /// <summary>
                /// Specifies whether the chain will be closed to circle
                /// </summary>
                public bool Circle { get; set; }
                /// <summary>
                /// Specifies whether connections of this schema will replace existing connections
                /// </summary>
                public bool ReplaceExistingConnections { get; set; }
                /// <summary>
                /// Number of applications of this schema
                /// </summary>
                public int Repetitions { get; set; }
                /// <summary>
                /// Neurons in the pool are interconnected through synapses.
                /// </summary>
                public InternalSynapseSettings SynapseCfg { get; set; }

                //Constructors
                /// <summary>
                /// Creates an uninitialized instance
                /// </summary>
                public ChainSchemaSettings()
                {
                    Ratio = 0;
                    Circle = false;
                    ReplaceExistingConnections = true;
                    Repetitions = 1;
                    SynapseCfg = null;
                    return;
                }

                /// <summary>
                /// The deep copy constructor
                /// </summary>
                /// <param name="source">Source instance</param>
                public ChainSchemaSettings(ChainSchemaSettings source)
                {
                    Ratio = source.Ratio;
                    Circle = source.Circle;
                    ReplaceExistingConnections = source.ReplaceExistingConnections;
                    Repetitions = source.Repetitions;
                    SynapseCfg = source.SynapseCfg.DeepClone();
                    return;
                }

                /// <summary>
                /// Creates the instance and initialize it from given xml element.
                /// </summary>
                /// <param name="settingsElem">
                /// Xml data containing settings.
                /// Content of xml element is always validated against the xml schema.
                /// </param>
                public ChainSchemaSettings(XElement settingsElem)
                {
                    //Parsing
                    //Density
                    Ratio = double.Parse(settingsElem.Attribute("ratio").Value, CultureInfo.InvariantCulture);
                    //Will be chain closed to circle?
                    Circle = bool.Parse(settingsElem.Attribute("circle").Value);
                    //Will be replaced existing connections?
                    ReplaceExistingConnections = bool.Parse(settingsElem.Attribute("replaceExistingConnections").Value);
                    //Number of schema repetitions
                    Repetitions = int.Parse(settingsElem.Attribute("repetitions").Value, CultureInfo.InvariantCulture);
                    //Synapse
                    SynapseCfg = new InternalSynapseSettings(settingsElem.Descendants("synapse").First());
                    return;
                }

                //Methods
                /// <summary>
                /// Creates the deep copy instance of this instance
                /// </summary>
                public ChainSchemaSettings DeepClone()
                {
                    ChainSchemaSettings clone = new ChainSchemaSettings(this);
                    return clone;
                }

            }//ChainSchemaSettings

        }//InterconnectionSettings

    }//PoolSettings

}//Namespace

