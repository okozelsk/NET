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
using RCNet.Neural.Activation;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// The class contains neural pool configuration parameters and also contains
    /// internal logic so it is not just a container of parameters. To create the proper instance by hand is not
    /// a trivial task.
    /// The easiest and safest way to create a proper instance is to use the xml constructor.
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
        /// Pool dimensions. Pool is 3D.
        /// </summary>
        public PoolDimensions Dim { get; set; }
        /// <summary>
        /// Determines whether to use neurons as the readout predictors
        /// </summary>
        public bool RouteToReadout { get; set; }
        /// <summary>
        /// Settings of the neuron groups in the pool.
        /// </summary>
        public List<NeuronGroupSettings> NeuronGroups { get; set; }
        /// <summary>
        /// Density of interconnected neurons.
        /// Each pool neuron will be connected as a source neuron for Dim.Size * InterconnectionDensity neurons.
        /// </summary>
        public double InterconnectionDensity { get; set; }
        /// <summary>
        /// Average distance of interconnected neurons.
        /// 0 means random distance.
        /// </summary>
        public double InterconnectionAvgDistance { get; set; }
        /// <summary>
        /// Specifies whether to allow neurons to be self connected
        /// </summary>
        public bool InterconnectionAllowSelfConn { get; set; }
        /// <summary>
        /// Neurons in the pool are interconnected. The weight of the connection synapse will be selected randomly.
        /// </summary>
        public RandomValueSettings InterconnectionSynapseWeight { get; set; }
        /// <summary>
        /// Indicates whether the retainment (leaky integrators) neurons feature is used.
        /// Relevant for neurons having time independent activation (analog)
        /// </summary>
        public bool RetainmentNeuronsFeature { get; set; }
        /// <summary>
        /// The parameter says how much of the pool neurons will have the Retainment property set.
        /// Specific analog neurons will be selected randomly.
        /// Count = NumberOfAnalogNeurons * Density
        /// </summary>
        public double RetainmentNeuronsDensity { get; set; }
        /// <summary>
        /// If the pool neuron is selected to have the Retainment property then its retainment rate will be randomly selected
        /// following specified settings
        /// </summary>
        public RandomValueSettings RetainmentRate { get; set; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance
        /// </summary>
        public PoolSettings()
        {
            Name = string.Empty;
            Dim = null;
            NeuronGroups = null;
            InterconnectionDensity = 0;
            InterconnectionAvgDistance = 0;
            InterconnectionSynapseWeight = null;
            RetainmentNeuronsFeature = false;
            RetainmentNeuronsDensity = 0;
            RetainmentRate = null;
            return;
        }

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
                Dim = new PoolDimensions(source.Dim.X, source.Dim.Y, source.Dim.Z);
            }
            RouteToReadout = source.RouteToReadout;
            NeuronGroups = new List<NeuronGroupSettings>(source.NeuronGroups.Count);
            foreach(NeuronGroupSettings item in source.NeuronGroups)
            {
                NeuronGroups.Add(item.DeepClone());
            }
            InterconnectionDensity = source.InterconnectionDensity;
            InterconnectionAvgDistance = source.InterconnectionAvgDistance;
            InterconnectionSynapseWeight = null;
            if(source.InterconnectionSynapseWeight != null)
            {
                InterconnectionSynapseWeight = source.InterconnectionSynapseWeight.DeepClone();
            }
            RetainmentNeuronsFeature = source.RetainmentNeuronsFeature;
            RetainmentNeuronsDensity = source.RetainmentNeuronsDensity;
            if (RetainmentNeuronsFeature)
            {
                RetainmentRate = source.RetainmentRate.DeepClone();
            }
            else
            {
                RetainmentRate = null;
            }
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// This is the preferred way to instantiate pool settings.
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
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.SM.PoolSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement poolSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            //Name
            Name = poolSettingsElem.Attribute("name").Value;
            //Dimensions
            Dim = new PoolDimensions(int.Parse(poolSettingsElem.Attribute("dimX").Value, CultureInfo.InvariantCulture),
                                     int.Parse(poolSettingsElem.Attribute("dimY").Value, CultureInfo.InvariantCulture),
                                     int.Parse(poolSettingsElem.Attribute("dimZ").Value, CultureInfo.InvariantCulture)
                                     );
            RouteToReadout = bool.Parse(poolSettingsElem.Attribute("routeToReadout").Value);
            //NeuronGroups
            XElement neuronGroupsElem = poolSettingsElem.Descendants("neuronGroups").First();
            double totalRelShare = 0;
            NeuronGroups = new List<NeuronGroupSettings>();
            foreach(XElement neuronGroupElem in neuronGroupsElem.Descendants("neuronGroup"))
            {
                NeuronGroupSettings ngs = new NeuronGroupSettings(neuronGroupElem);
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
                    throw new Exception("Can't set proper neuron absolute counts for the neuron groups.");
                }
            }
            
            //Interconnection
            XElement interconnectionElem = poolSettingsElem.Descendants("interconnection").First();
            InterconnectionDensity = double.Parse(interconnectionElem.Attribute("density").Value, CultureInfo.InvariantCulture);
            InterconnectionAvgDistance = interconnectionElem.Attribute("avgDistance").Value == "NA" ? 0d : double.Parse(interconnectionElem.Attribute("avgDistance").Value, CultureInfo.InvariantCulture);
            InterconnectionAllowSelfConn = bool.Parse(interconnectionElem.Attribute("allowSelfConnection").Value);
            InterconnectionSynapseWeight = new RandomValueSettings(interconnectionElem.Descendants("weight").First());
            //Retainment neurons
            XElement retainmentElem = poolSettingsElem.Descendants("retainmentNeurons").FirstOrDefault();
            RetainmentNeuronsFeature = (retainmentElem != null);
            if (RetainmentNeuronsFeature)
            {
                RetainmentNeuronsDensity = double.Parse(retainmentElem.Attribute("density").Value, CultureInfo.InvariantCulture);
                RetainmentRate = new RandomValueSettings(retainmentElem.Descendants("rate").First());
                RetainmentNeuronsFeature = (RetainmentNeuronsDensity > 0 &&
                                            RetainmentRate.Max > 0
                                            );
            }
            else
            {
                RetainmentNeuronsDensity = 0;
            }
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
                !Equals(Dim, cmpSettings.Dim) ||
                RouteToReadout != cmpSettings.RouteToReadout ||
                !Equals(NeuronGroups.Count, cmpSettings.NeuronGroups.Count) ||
                InterconnectionDensity != cmpSettings.InterconnectionDensity ||
                InterconnectionAvgDistance != cmpSettings.InterconnectionAvgDistance ||
                !Equals(InterconnectionSynapseWeight, cmpSettings.InterconnectionSynapseWeight) ||
                RetainmentNeuronsFeature != cmpSettings.RetainmentNeuronsFeature ||
                RetainmentNeuronsDensity != cmpSettings.RetainmentNeuronsDensity ||
                !Equals(RetainmentRate, cmpSettings.RetainmentRate)
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
            /// Specifies, whether neurons within the group generate secondary predictors
            /// </summary>
            public bool AugmentedStates { get; set; }
            /// <summary>
            /// Computed attribute. What count of pool's neurons is formed by this group of the neurons
            /// </summary>
            public int Count { get; set; }
            /// <summary>
            /// Activation settings of the groupped neurons
            /// </summary>
            public Object ActivationSettings { get; set; }
            /// <summary>
            /// Each pool's neuron has its own constant input bias. Bias is always added to input signal of the neuron.
            /// A constant bias value of the neuron will be selected randomly.
            /// </summary>
            public RandomValueSettings BiasSettings { get; set; }

            //Constructors
            /// <summary>
            /// Creates an uninitialized instance
            /// </summary>
            public NeuronGroupSettings()
            {
                Name = string.Empty;
                Role = CommonEnums.NeuronRole.Excitatory;
                RelativeShare = 0;
                AugmentedStates = false;
                Count = 0;
                ActivationSettings = null;
                BiasSettings = null;
                return;
            }

            /// <summary>
            /// The deep copy constructor
            /// </summary>
            /// <param name="source">Source instance</param>
            public NeuronGroupSettings(NeuronGroupSettings source)
            {
                Name = source.Name;
                Role = source.Role;
                RelativeShare = source.RelativeShare;
                AugmentedStates = source.AugmentedStates;
                Count = source.Count;
                ActivationSettings = ActivationFactory.DeepCloneActivationSettings(source.ActivationSettings);
                BiasSettings = source.BiasSettings.DeepClone();
                return;
            }

            /// <summary>
            /// Creates the instance and initialize it from given xml element.
            /// </summary>
            /// <param name="elem">
            /// Xml data containing settings.
            /// Content of xml element is always validated against the xml schema.
            /// </param>
            public NeuronGroupSettings(XElement elem)
            {
                //Validation
                ElemValidator validator = new ElemValidator();
                Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
                validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.SM.PoolNeuronGroupSettings.xsd");
                validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
                XElement settingsElem = validator.Validate(elem, "rootElem");
                //Parsing
                //Name
                Name = settingsElem.Attribute("name").Value;
                //Role
                Role = CommonEnums.ParseNeuronRole(settingsElem.Attribute("role").Value);
                //Relative share
                RelativeShare = double.Parse(settingsElem.Attribute("relShare").Value, CultureInfo.InvariantCulture);
                //Augmented states
                AugmentedStates = bool.Parse(settingsElem.Attribute("augmentedStates").Value);
                //Activation settings
                ActivationSettings = ActivationFactory.LoadSettings(settingsElem.Descendants().First());
                //Bias
                BiasSettings = new RandomValueSettings(settingsElem.Descendants("bias").First());
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
                    AugmentedStates != cmpSettings.AugmentedStates ||
                    Count != cmpSettings.Count ||
                    !Equals(ActivationSettings, cmpSettings.ActivationSettings) ||
                    !Equals(BiasSettings, cmpSettings.BiasSettings)
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


        }//NeuronGroup

    }//PoolSettings

}//Namespace

