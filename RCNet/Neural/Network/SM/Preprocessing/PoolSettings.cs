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
using RCNet.Neural.Network.SM.Synapse;

namespace RCNet.Neural.Network.SM.Preprocessing
{
    /// <summary>
    /// Configuration of neural pool.
    /// </summary>
    [Serializable]
    public class PoolSettings
    {
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
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.SM.Preprocessing.PoolSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement poolSettingsElem = validator.Validate(elem, "rootElem");
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
                NeuronGroupSettings ngs = new NeuronGroupSettings(neuronGroupElem, CommonEnums.ActivationType.Analog);
                if (ngs.RelativeShare > 0)
                {
                    NeuronGroups.Add(ngs);
                    totalRelShare += ngs.RelativeShare;
                }
            }
            //Spiking neuron groups
            foreach (XElement neuronGroupElem in neuronGroupsElem.Descendants("spikingGroup"))
            {
                NeuronGroupSettings ngs = new NeuronGroupSettings(neuronGroupElem, CommonEnums.ActivationType.Spiking);
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
            
            //Interconnection
            XElement interconnectionElem = poolSettingsElem.Descendants("interconnection").First();
            InterconnectionCfg = new InterconnectionSettings(interconnectionElem);
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            PoolSettings cmpSettings = obj as PoolSettings;
            if (Name != cmpSettings.Name ||
                NeuronGroups.Count != NeuronGroups.Count ||
                !Equals(Dim, cmpSettings.Dim) ||
                !Equals(InterconnectionCfg, cmpSettings.InterconnectionCfg)
                )
            {
                return false;
            }
            for (int i = 0; i < NeuronGroups.Count; i++)
            {
                if(!Equals(NeuronGroups[i], cmpSettings.NeuronGroups[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// See the base.
        /// </summary>
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public PoolSettings DeepClone()
        {
            PoolSettings clone = new PoolSettings(this);
            return clone;
        }

        //Inner classes
        /// <summary>
        /// Class encapsulates specification of the groupped (excitatory or inhibitory) neurons
        /// </summary>
        [Serializable]
        public class NeuronGroupSettings
        {
            //Constants
            public const double DefaultAnalogFiringThreshold = 0.0025d;
            //Attribute properties
            /// <summary>
            /// Name of the neuron group
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// Excitatory or Inhibitory role of the neurons
            /// </summary>
            public CommonEnums.NeuronRole Role { get; set; }
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
            public CommonEnums.ActivationType ActivationType;
            /// <summary>
            /// Common activation settings of the groupped neurons
            /// </summary>
            public Object ActivationCfg { get; set; }
            /// <summary>
            /// Restriction of neuron's output signaling
            /// </summary>
            public CommonEnums.NeuronSignalingRestrictionType SignalingRestriction;
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
            /// Configuration of the predictors (enabling/disabling)
            /// </summary>
            public PredictorsSettings PredictorsCfg { get; set; }

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
            /// <param name="elem">
            /// Xml data containing settings.
            /// Content of xml element is always validated against the appropriate xml schema.
            /// </param>
            /// <param name="activationType">Specifies sub-type of the neuron group</param>
            public NeuronGroupSettings(XElement elem, CommonEnums.ActivationType activationType)
            {
                //Validation
                ElemValidator validator = new ElemValidator();
                Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
                if (activationType == CommonEnums.ActivationType.Analog)
                {
                    validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.SM.Preprocessing.PoolAnalogNeuronGroupSettings.xsd");
                }
                else
                {
                    validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.SM.Preprocessing.PoolSpikingNeuronGroupSettings.xsd");
                }
                validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
                XElement settingsElem = validator.Validate(elem, "rootElem");
                ActivationType = activationType;
                //Parsing
                //Name
                Name = settingsElem.Attribute("name").Value;
                //Role
                Role = CommonEnums.ParseNeuronRole(settingsElem.Attribute("role").Value);
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
                if (activationType == CommonEnums.ActivationType.Spiking)
                {
                    SignalingRestriction = CommonEnums.NeuronSignalingRestrictionType.SpikingOnly;
                    AnalogFiringThreshold = 0;
                    RetainmentNeuronsDensity = 0;
                    RetainmentStrengthCfg = null;
                }
                else
                {
                    //Analog sub-type
                    //Output signaling restriction
                    SignalingRestriction = CommonEnums.ParseNeuronSignalingRestriction(settingsElem.Attribute("signalingRestriction").Value);
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
                PredictorsCfg = new PredictorsSettings(settingsElem.Descendants("predictors").First());
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
            /// See the base.
            /// </summary>
            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                NeuronGroupSettings cmpSettings = obj as NeuronGroupSettings;
                if (Name != cmpSettings.Name ||
                    Role != cmpSettings.Role ||
                    RelativeShare != cmpSettings.RelativeShare ||
                    ReadoutNeuronsDensity != cmpSettings.ReadoutNeuronsDensity ||
                    Count != cmpSettings.Count ||
                    ActivationType != cmpSettings.ActivationType ||
                    !Equals(ActivationCfg, cmpSettings.ActivationCfg) ||
                    SignalingRestriction != cmpSettings.SignalingRestriction ||
                    !Equals(BiasCfg, cmpSettings.BiasCfg) ||
                    AnalogFiringThreshold != cmpSettings.AnalogFiringThreshold ||
                    RetainmentNeuronsDensity != cmpSettings.RetainmentNeuronsDensity ||
                    !Equals(RetainmentStrengthCfg, cmpSettings.RetainmentStrengthCfg) ||
                    !Equals(PredictorsCfg, cmpSettings.PredictorsCfg)
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
            public NeuronGroupSettings DeepClone()
            {
                NeuronGroupSettings clone = new NeuronGroupSettings(this);
                return clone;
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
            /// Density of interconnected neurons.
            /// Each pool neuron will be connected as a source neuron for Dim.Size * InterconnectionDensity neurons.
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
            /// Neurons in the pool are interconnected through synapses.
            /// </summary>
            public InternalSynapseSettings SynapseCfg { get; set; }

            //Constructors
            /// <summary>
            /// Creates an uninitialized instance
            /// </summary>
            public InterconnectionSettings()
            {
                Density = 0;
                RatioEE = 0.3;
                RatioEI = 0.2;
                RatioIE = 0.4;
                RatioII = 0.1;
                AvgDistance = 0;
                AllowSelfConnection = true;
                ConstantNumOfConnections = false;
                SynapseCfg = null;
                return;
            }

            /// <summary>
            /// The deep copy constructor
            /// </summary>
            /// <param name="source">Source instance</param>
            public InterconnectionSettings(InterconnectionSettings source)
            {
                Density = source.Density;
                RatioEE = source.RatioEE;
                RatioEI = source.RatioEI;
                RatioIE = source.RatioIE;
                RatioII = source.RatioII;
                AvgDistance = source.AvgDistance;
                AllowSelfConnection = source.AllowSelfConnection;
                ConstantNumOfConnections = source.ConstantNumOfConnections;
                SynapseCfg = source.SynapseCfg.DeepClone();
                return;
            }

            /// <summary>
            /// Creates the instance and initialize it from given xml element.
            /// </summary>
            /// <param name="elem">
            /// Xml data containing settings.
            /// Content of xml element is always validated against the xml schema.
            /// </param>
            public InterconnectionSettings(XElement elem)
            {
                //Validation
                ElemValidator validator = new ElemValidator();
                Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
                validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.SM.Preprocessing.PoolInterconnectionSettings.xsd");
                validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
                XElement settingsElem = validator.Validate(elem, "rootElem");
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
                //Will have each neuron the same number of connections?
                ConstantNumOfConnections = bool.Parse(settingsElem.Attribute("constantNumOfConnections").Value);
                //Synapse
                SynapseCfg = new InternalSynapseSettings(settingsElem.Descendants("synapse").First());
                return;
            }

            //Methods
            /// <summary>
            /// See the base.
            /// </summary>
            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                InterconnectionSettings cmpSettings = obj as InterconnectionSettings;
                if (Density != cmpSettings.Density ||
                    RatioEE != cmpSettings.RatioEE ||
                    RatioEI != cmpSettings.RatioEI ||
                    RatioIE != cmpSettings.RatioIE ||
                    RatioII != cmpSettings.RatioII ||
                    AvgDistance != cmpSettings.AvgDistance ||
                    AllowSelfConnection != cmpSettings.AllowSelfConnection ||
                    ConstantNumOfConnections != cmpSettings.ConstantNumOfConnections ||
                    !Equals(SynapseCfg, cmpSettings.SynapseCfg)
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
            public InterconnectionSettings DeepClone()
            {
                InterconnectionSettings clone = new InterconnectionSettings(this);
                return clone;
            }

        }//InterconnectionSettings


    }//PoolSettings

}//Namespace

