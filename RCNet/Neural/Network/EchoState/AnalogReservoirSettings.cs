using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.IO;
using RCNet.XmlTools;
using RCNet.Neural.Activation;

namespace RCNet.Neural.Network.EchoState
{
    /// <summary>
    /// The class contains analog reservoir configuration parameters and also contains
    /// internal logic so it is not just a container of parameters. Creating an proper instance by hand is not
    /// a trivial task.
    /// The easiest and safest way to create an instance is to use the xml constructor.
    /// </summary>
    [Serializable]
    public class AnalogReservoirSettings
    {
        //Constants
        /// <summary>
        /// Supported types of reservoir internal topology
        /// </summary>
        public enum ReservoirTopologyType
        {
            /// <summary>
            /// Random topology. Reservoir's neurons are connected randomly.
            /// </summary>
            Random,
            /// <summary>
            /// Ring topology. Reservoir's neurons are connected in a ring shape.
            /// </summary>
            Ring,
            /// <summary>
            /// Doubly twisted thoroidal topology.
            /// </summary>
            DTT
        };

        //Attribute properties
        /// <summary>
        /// Name of this configuration
        /// </summary>
        public string SettingsName { get; set; }
        /// <summary>
        /// Each reservoir's neuron has its own constant input bias. Bias is always added to input signal.
        /// A constant bias value will be for each neuron selected randomly from the range (-BiasScale;+BiasScale).
        /// To disable input biasing specify 0.
        /// </summary>
        public double BiasScale { get; set; }
        /// <summary>
        /// Each input field will be connected by the random weight to the number of
        /// reservoir neurons = (Size * Density).
        /// Typical InputConnectionDensity = 1 (it means the full connectivity).
        /// </summary>
        public double InputConnectionDensity { get; set; }
        /// <summary>
        /// A weight of each input field to reservoir's neuron connection will be randomly selected
        /// from the open interval (-InputWeightScale, +InputWeightScale).
        /// </summary>
        public double InputWeightScale { get; set; }
        /// <summary>
        /// Number of the neurons in the reservoir.
        /// </summary>
        public int Size { get; set; }
        /// <summary>
        /// Activation function of the neurons in the reservoir.
        /// </summary>
        public ActivationFactory.ActivationType ReservoirNeuronActivation { get; set; }
        /// <summary>
        /// Neurons in the reservoir are interconnected. The weight of the connection will be randomly selected
        /// from the open interval (-InternalWeightScale, +InternalWeightScale).
        /// </summary>
        public double InternalWeightScale { get; set; }
        /// <summary>
        /// One of the supported reservoir topologies of internal neural networking.
        /// See the enumeration ReservoirTopologyType.
        /// </summary>
        public ReservoirTopologyType TopologyType { get; set; }
        /// <summary>
        /// Parameters of the topology of internal neural networking.
        /// See classes RandomTopology, RingTopology and DTTTopology.
        /// </summary>
        public Object TopologySettings { get; set; }
        /// <summary>
        /// Indicates whether the retainment (leaky integrators) neurons feature is used.
        /// </summary>
        public bool RetainmentNeuronsFeature { get; set; }
        /// <summary>
        /// The parameter says how much of the reservoir's neurons will have the Retainment property set.
        /// Specific neurons will be selected randomly.
        /// Count = Size * Density
        /// </summary>
        public double RetainmentNeuronsDensity { get; set; }
        /// <summary>
        /// If the reservoir's neuron is selected to have Retainment property then its retainment rate will be randomly selected
        /// from the closed interval (RetainmentMinRate, RetainmentMaxRate).
        /// </summary>
        public double RetainmentMinRate { get; set; }
        /// <summary>
        /// If the reservoir's neuron is selected to have Retainment property then its retainment rate will be randomly selected
        /// from the closed interval (RetainmentMinRate, RetainmentMaxRate).
        /// </summary>
        public double RetainmentMaxRate { get; set; }
        /// <summary>
        /// Indicates whether the context neuron feature is used.
        /// Context neuron is a special neuron outside the reservoir, which mixes and processes the signal from all
        /// the neurons in the reservoir. The context neuron state thus represents the state of the entire reservoir
        /// and is then used as one of the inputs to the neurons in the reservoir.
        /// </summary>
        public bool ContextNeuronFeature { get; set; }
        /// <summary>
        /// The parameter says how many neurons in the reservoir will receive the signal from the context neuron.
        /// Count = Size * Density
        /// </summary>
        public double ContextNeuronFeedbackDensity { get; set; }
        /// <summary>
        /// Activation function of the context neuron.
        /// </summary>
        public ActivationFactory.ActivationType ContextNeuronActivation { get; set; }
        /// <summary>
        /// Each weight of the connection from the reservoir's neuron to the contex neuron will be randomly selected
        /// from the open interval (-ContextNeuronInWeightScale, +ContextNeuronInWeightScale)
        /// </summary>
        public double ContextNeuronInWeightScale { get; set; }
        /// <summary>
        /// Each weight of the connection from the contex neuron to the reservoir's neuron will be randomly selected
        /// from the open interval (-ContextNeuronOutWeightScale, +ContextNeuronOutWeightScale)
        /// </summary>
        public double ContextNeuronOutWeightScale { get; set; }
        /// Indicates whether the feedback feature is used.
        public bool FeedbackFeature { get; set; }
        /// <summary>
        /// Each feedback field will be connected by the random weight to the number of
        /// reservoir neurons = (Size * Density).
        /// Typical FeedbackConnectionDensity = 1 (it means the full connectivity).
        /// </summary>
        public double FeedbackConnectionDensity { get; set; }
        /// <summary>
        /// A weight of each feedback field to reservoir's neuron connection will be randomly selected
        /// from the open interval (-FeedbackWeightScale, +FeedbackWeightScale).
        /// </summary>
        public double FeedbackWeightScale { get; set; }
        /// <summary>
        /// Collection of feedback field names.
        /// </summary>
        public List<string> FeedbackFieldNameCollection { get; set; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance
        /// </summary>
        public AnalogReservoirSettings()
        {
            SettingsName = string.Empty;
            BiasScale = 0;
            InputConnectionDensity = 0;
            InputWeightScale = 0;
            Size = 0;
            ReservoirNeuronActivation = ActivationFactory.ActivationType.TanH;
            InternalWeightScale = 0;
            TopologyType = ReservoirTopologyType.Random;
            TopologySettings = null;
            RetainmentNeuronsFeature = false;
            RetainmentNeuronsDensity = 0;
            RetainmentMinRate = 0;
            RetainmentMaxRate = 0;
            ContextNeuronFeature = false;
            ContextNeuronFeedbackDensity = 0;
            ContextNeuronActivation = ReservoirNeuronActivation;
            ContextNeuronInWeightScale = 0;
            ContextNeuronOutWeightScale = 0;
            FeedbackFeature = false;
            FeedbackConnectionDensity = 0;
            FeedbackWeightScale = 0;
            FeedbackFieldNameCollection = new List<string>();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public AnalogReservoirSettings(AnalogReservoirSettings source)
        {
            SettingsName = source.SettingsName;
            BiasScale = source.BiasScale;
            InputConnectionDensity = source.InputConnectionDensity;
            InputWeightScale = source.InputWeightScale;
            Size = source.Size;
            ReservoirNeuronActivation = source.ReservoirNeuronActivation;
            InternalWeightScale = source.InternalWeightScale;
            TopologyType = source.TopologyType;
            if (source.TopologySettings != null)
            {
                if (source.TopologySettings.GetType() == typeof(RandomTopology))
                {
                    TopologySettings = new RandomTopology((RandomTopology)source.TopologySettings);
                }
                if (source.TopologySettings.GetType() == typeof(RingTopology))
                {
                    TopologySettings = new RingTopology((RingTopology)source.TopologySettings);
                }
                if (source.TopologySettings.GetType() == typeof(DTTTopology))
                {
                    TopologySettings = new DTTTopology((DTTTopology)source.TopologySettings);
                }
            }
            RetainmentNeuronsFeature = source.RetainmentNeuronsFeature;
            RetainmentNeuronsDensity = source.RetainmentNeuronsDensity;
            RetainmentMinRate = source.RetainmentMinRate;
            RetainmentMaxRate = source.RetainmentMaxRate;
            ContextNeuronFeature = source.ContextNeuronFeature;
            ContextNeuronFeedbackDensity = source.ContextNeuronFeedbackDensity;
            ContextNeuronActivation = source.ContextNeuronActivation;
            ContextNeuronInWeightScale = source.ContextNeuronInWeightScale;
            ContextNeuronOutWeightScale = source.ContextNeuronOutWeightScale;
            FeedbackFeature = source.FeedbackFeature;
            FeedbackConnectionDensity = source.FeedbackConnectionDensity;
            FeedbackWeightScale = source.FeedbackWeightScale;
            FeedbackFieldNameCollection = new List<string>(source.FeedbackFieldNameCollection);
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// This is the preferred way to instantiate reservoir settings.
        /// </summary>
        /// <param name="reservoirSettingsElem">
        /// Xml data containing reservoir settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public AnalogReservoirSettings(XElement reservoirSettingsElem)
        {
            //Validation
            //A very ugly validation. Xml schema does not support validation of the xml fragment against specific type.
            XmlValidator validator = new XmlValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            using (Stream schemaStream = assemblyRCNet.GetManifestResourceStream("RCNet.Neural.Network.EchoState.AnalogReservoirSettings.xsd"))
            {
                validator.AddSchema(schemaStream);
            }
            using (Stream schemaStream = assemblyRCNet.GetManifestResourceStream("RCNet.NeuralSettingsTypes.xsd"))
            {
                validator.AddSchema(schemaStream);
            }
            validator.LoadXDocFromString(reservoirSettingsElem.ToString());
            //Parsing
            //Settings name
            SettingsName = reservoirSettingsElem.Attribute("Name").Value;
            //Input
            XElement inputElem = reservoirSettingsElem.Descendants("Input").First();
            BiasScale = double.Parse(inputElem.Attribute("BiasScale").Value, CultureInfo.InvariantCulture);
            InputConnectionDensity = double.Parse(inputElem.Attribute("ConnectionDensity").Value, CultureInfo.InvariantCulture);
            InputWeightScale = double.Parse(inputElem.Attribute("WeightScale").Value, CultureInfo.InvariantCulture);
            //Internal
            XElement internalElem = reservoirSettingsElem.Descendants("Internal").First();
            Size = int.Parse(internalElem.Attribute("Size").Value);
            ReservoirNeuronActivation = ActivationFactory.ParseActivation(internalElem.Attribute("Activation").Value);
            InternalWeightScale = double.Parse(internalElem.Attribute("WeightScale").Value, CultureInfo.InvariantCulture);
            //Topology
            List<XElement> topologyElems = new List<XElement>();
            topologyElems.AddRange(internalElem.Descendants("RandomTopology"));
            topologyElems.AddRange(internalElem.Descendants("RingTopology"));
            topologyElems.AddRange(internalElem.Descendants("DTTTopology"));
            if(topologyElems.Count != 1)
            {
                throw new Exception("Only one reservoir topology can be specified in reservoir settings.");
            }
            if (topologyElems.Count == 0)
            {
                throw new Exception("Reservoir topology is not specified in reservoir settings.");
            }
            XElement topologyElem = topologyElems[0];
            //Random?
            if (topologyElem.Name == "RandomTopology")
            {
                TopologyType = ReservoirTopologyType.Random;
                TopologySettings = new RandomTopology(topologyElem);
            }
            //Ring?
            else if (topologyElem.Name == "RingTopology")
            {
                TopologyType = ReservoirTopologyType.Ring;
                TopologySettings = new RingTopology(topologyElem);
            }
            else
            {
                //DTT
                TopologyType = ReservoirTopologyType.DTT;
                TopologySettings = new DTTTopology(topologyElem);
            }
            //Retainment neurons
            XElement retainmentElem = internalElem.Descendants("RetainmentNeurons").FirstOrDefault();
            RetainmentNeuronsFeature = (retainmentElem != null);
            if (RetainmentNeuronsFeature)
            {
                RetainmentNeuronsDensity = double.Parse(retainmentElem.Attribute("Density").Value, CultureInfo.InvariantCulture);
                RetainmentMinRate = double.Parse(retainmentElem.Attribute("RetainmentMinRate").Value, CultureInfo.InvariantCulture);
                RetainmentMaxRate = double.Parse(retainmentElem.Attribute("RetainmentMaxRate").Value, CultureInfo.InvariantCulture);
                RetainmentNeuronsFeature = (RetainmentNeuronsDensity > 0 &&
                                            RetainmentMaxRate > 0
                                            );
            }
            else
            {
                RetainmentNeuronsDensity = 0;
                RetainmentMinRate = 0;
                RetainmentMaxRate = 0;
            }
            //Context neuron
            XElement ctxNeuronElem = internalElem.Descendants("ContextNeuron").FirstOrDefault();
            ContextNeuronFeature = (ctxNeuronElem != null);
            if (ContextNeuronFeature)
            {
                ContextNeuronFeedbackDensity = double.Parse(ctxNeuronElem.Attribute("FeedbackDensity").Value, CultureInfo.InvariantCulture);
                ContextNeuronActivation = ActivationFactory.ParseActivation(ctxNeuronElem.Attribute("Activation").Value);
                ContextNeuronInWeightScale = double.Parse(ctxNeuronElem.Attribute("InWeightScale").Value, CultureInfo.InvariantCulture);
                ContextNeuronOutWeightScale = double.Parse(ctxNeuronElem.Attribute("OutWeightScale").Value, CultureInfo.InvariantCulture);
                ContextNeuronFeature = (ContextNeuronFeedbackDensity > 0 &&
                                        ContextNeuronInWeightScale > 0 &&
                                        ContextNeuronOutWeightScale > 0
                                        );
            }
            else
            {
                ContextNeuronFeedbackDensity = 0;
                ContextNeuronActivation = ReservoirNeuronActivation;
                ContextNeuronInWeightScale = 0;
                ContextNeuronOutWeightScale = 0;
            }
            //Feedback
            XElement feedbackElem = reservoirSettingsElem.Descendants("Feedback").FirstOrDefault();
            FeedbackFeature = (feedbackElem != null);
            FeedbackFieldNameCollection = new List<string>();
            if (FeedbackFeature)
            {
                FeedbackConnectionDensity = double.Parse(feedbackElem.Attribute("Density").Value, CultureInfo.InvariantCulture);
                FeedbackWeightScale = double.Parse(feedbackElem.Attribute("WeightScale").Value, CultureInfo.InvariantCulture);
                foreach (XElement feedbackFieldElem in feedbackElem.Descendants("Field"))
                {
                    FeedbackFieldNameCollection.Add(feedbackFieldElem.Attribute("Name").Value);
                }
                FeedbackFeature = (FeedbackFieldNameCollection.Count > 0);
            }
            else
            {
                FeedbackConnectionDensity = 0;
                FeedbackWeightScale = 0;
            }
            return;
        }

        //Methods
        //Static methods
        /// <summary>
        /// Parses string code to ReservoirTopologyType.
        /// </summary>
        /// <param name="code">Topology code</param>
        /// <returns></returns>
        public static ReservoirTopologyType ParseReservoirTopology(string code)
        {
            switch (code.ToUpper())
            {
                case "RANDOM": return ReservoirTopologyType.Random;
                case "RING": return ReservoirTopologyType.Ring;
                case "DTT": return ReservoirTopologyType.DTT;
                default:
                    throw new ArgumentException($"Unknown reservoir's topology code {code}");
            }
        }

        //Instance methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            AnalogReservoirSettings cmpSettings = obj as AnalogReservoirSettings;
            if (SettingsName != cmpSettings.SettingsName ||
                BiasScale != cmpSettings.BiasScale ||
                InputConnectionDensity != cmpSettings.InputConnectionDensity ||
                InputWeightScale != cmpSettings.InputWeightScale ||
                Size != cmpSettings.Size ||
                ReservoirNeuronActivation != cmpSettings.ReservoirNeuronActivation ||
                InternalWeightScale != cmpSettings.InternalWeightScale ||
                TopologyType != cmpSettings.TopologyType ||
                RetainmentNeuronsFeature != cmpSettings.RetainmentNeuronsFeature ||
                RetainmentNeuronsDensity != cmpSettings.RetainmentNeuronsDensity ||
                RetainmentMinRate != cmpSettings.RetainmentMinRate ||
                RetainmentMaxRate != cmpSettings.RetainmentMaxRate ||
                ContextNeuronFeature != cmpSettings.ContextNeuronFeature ||
                ContextNeuronFeedbackDensity != cmpSettings.ContextNeuronFeedbackDensity ||
                ContextNeuronActivation != cmpSettings.ContextNeuronActivation ||
                ContextNeuronInWeightScale != cmpSettings.ContextNeuronInWeightScale ||
                ContextNeuronOutWeightScale != cmpSettings.ContextNeuronOutWeightScale ||
                FeedbackFeature != cmpSettings.FeedbackFeature ||
                FeedbackConnectionDensity != cmpSettings.FeedbackConnectionDensity ||
                FeedbackWeightScale != cmpSettings.FeedbackWeightScale
                )
            {
                return false;
            }
            switch (TopologyType)
            {
                case ReservoirTopologyType.Random:
                    if (!((RandomTopology)TopologySettings).Equals((RandomTopology)cmpSettings.TopologySettings)) return false;
                    break;
                case ReservoirTopologyType.Ring:
                    if (!((RingTopology)TopologySettings).Equals((RingTopology)cmpSettings.TopologySettings)) return false;
                    break;
                case ReservoirTopologyType.DTT:
                    if (!((DTTTopology)TopologySettings).Equals((DTTTopology)cmpSettings.TopologySettings)) return false;
                    break;
            }
            return true;
        }

        /// <summary>
        /// See the base.
        /// </summary>
        public override int GetHashCode()
        {
            return SettingsName.GetHashCode();
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public AnalogReservoirSettings DeepClone()
        {
            AnalogReservoirSettings clone = new AnalogReservoirSettings(this);
            return clone;
        }


        //Inner classes
        /// <summary>
        /// Additional setup parameters for Random reservoir topology
        /// </summary>
        [Serializable]
        public class RandomTopology
        {
            //Attributes
            /// <summary>
            /// The parameter says how many interconnections from all possible interconnections will be used.
            /// Count = Size * Size * Density
            /// </summary>
            public double ConnectionsDensity { get; set; }

            //Constructors
            /// <summary>
            /// Creates an unitialized instance
            /// </summary>
            public RandomTopology()
            {
                ConnectionsDensity = 0;
                return;
            }

            /// <summary>
            /// Copy constructor
            /// </summary>
            /// <param name="source">Source instance</param>
            public RandomTopology(RandomTopology source)
            {
                ConnectionsDensity = source.ConnectionsDensity;
                return;
            }

            /// <summary>
            /// Creates the instance itialized from xml
            /// </summary>
            public RandomTopology(XElement randomTopologyElem)
            {
                ConnectionsDensity = double.Parse(randomTopologyElem.Attribute("ConnectionsDensity").Value, CultureInfo.InvariantCulture);
                return;
            }

            //Methods
            /// <summary>
            /// See the base.
            /// </summary>
            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                RandomTopology cmpSettings = obj as RandomTopology;
                if (ConnectionsDensity != cmpSettings.ConnectionsDensity)
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

        }//RandomTopology

        /// <summary>
        /// Additional setup parameters for Ring reservoir topology
        /// </summary>
        [Serializable]
        public class RingTopology
        {
            //Attributes
            /// <summary>
            /// The parameter specifies whether the ring interconnection will be bidirectional.
            /// </summary>
            public bool BiDirection { get; set; }
            /// <summary>
            /// The parameter says how many neurons in the reservoir will receive the signal from itself.
            /// Count = Size * Density
            /// </summary>
            public double SelfConnectionsDensity { get; set; }
            /// <summary>
            /// The parameter says how many additional interconnections from all possible interconnections will be used.
            /// Count = Size * Size * Density
            /// </summary>
            public double InterConnectionsDensity { get; set; }

            //Constructors
            /// <summary>
            /// Creates an unitialized instance
            /// </summary>
            public RingTopology()
            {
                BiDirection = false;
                SelfConnectionsDensity = 0;
                InterConnectionsDensity = 0;
                return;
            }

            /// <summary>
            /// Copy constructor
            /// </summary>
            /// <param name="source">Source instance</param>
            public RingTopology(RingTopology source)
            {
                BiDirection = source.BiDirection;
                SelfConnectionsDensity = source.SelfConnectionsDensity;
                InterConnectionsDensity = source.InterConnectionsDensity;
                return;
            }

            /// <summary>
            /// Creates the instance itialized from xml
            /// </summary>
            public RingTopology(XElement ringTopologyElem)
            {
                BiDirection = bool.Parse(ringTopologyElem.Attribute("BiDirection").Value);
                SelfConnectionsDensity = double.Parse(ringTopologyElem.Attribute("SelfConnectionsDensity").Value, CultureInfo.InvariantCulture);
                InterConnectionsDensity = double.Parse(ringTopologyElem.Attribute("InterConnectionsDensity").Value, CultureInfo.InvariantCulture);
                return;
            }

            //Methods
            /// <summary>
            /// See the base.
            /// </summary>
            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                RingTopology cmpSettings = obj as RingTopology;
                if (BiDirection != cmpSettings.BiDirection ||
                   SelfConnectionsDensity != cmpSettings.SelfConnectionsDensity ||
                   InterConnectionsDensity != cmpSettings.InterConnectionsDensity
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

        }//RingTopology

        /// <summary>
        /// Additional setup parameters for DTT reservoir topology
        /// </summary>
        [Serializable]
        public class DTTTopology
        {
            //Attributes
            /// <summary>
            /// The parameter says how many neurons in the reservoir will receive the signal from itself.
            /// Count = Size * Density
            /// </summary>
            public double SelfConnectionsDensity { get; set; }

            //Constructors
            /// <summary>
            /// Creates an unitialized instance
            /// </summary>
            public DTTTopology()
            {
                SelfConnectionsDensity = 0;
                return;
            }

            /// <summary>
            /// Copy constructor
            /// </summary>
            /// <param name="source">Source instance</param>
            public DTTTopology(DTTTopology source)
            {
                SelfConnectionsDensity = source.SelfConnectionsDensity;
                return;
            }

            /// <summary>
            /// Creates the instance itialized from xml
            /// </summary>
            public DTTTopology(XElement dttTopologyElem)
            {
                SelfConnectionsDensity = double.Parse(dttTopologyElem.Attribute("SelfConnectionsDensity").Value, CultureInfo.InvariantCulture);
                return;
            }

            //Methods
            /// <summary>
            /// See the base.
            /// </summary>
            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                DTTTopology cmpSettings = obj as DTTTopology;
                if (SelfConnectionsDensity != cmpSettings.SelfConnectionsDensity)
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

        }//DTTTopology

    }//AnalogReservoirSettings

}//Namespace
