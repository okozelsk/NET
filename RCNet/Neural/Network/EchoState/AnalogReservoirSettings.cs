using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using RCNet.XmlTools;
using RCNet.Neural.Activation;

namespace RCNet.Neural.Network.EchoState
{
    /// <summary>
    /// Reservoir settings
    /// </summary>
    [Serializable]
    public sealed class AnalogReservoirSettings
    {
        //Constants
        /// <summary>Supported types of reservoir topologies</summary>
        public enum ReservoirTopologyType
        {
            /// <summary>Random topology.</summary>
            Random,
            /// <summary>Ring topology</summary>
            Ring,
            /// <summary>Doubly twisted thoroidal topology</summary>
            DTT
        };

        //Attribute properties
        public string SettingsName { get; set; }
        public double BiasScale { get; set; }
        public double InputConnectionDensity { get; set; }
        public double InputWeightScale { get; set; }
        public int Size { get; set; }
        public ActivationFactory.ActivationType ReservoirNeuronActivation { get; set; }
        public double InternalWeightScale { get; set; }
        public ReservoirTopologyType TopologyType { get; set; }
        public Object TopologySettings { get; set; }
        public bool RetainmentNeuronsFeature { get; set; }
        public double RetainmentNeuronsDensity { get; set; }
        public double RetainmentMinRate { get; set; }
        public double RetainmentMaxRate { get; set; }
        public bool ContextNeuronFeature { get; set; }
        public double ContextNeuronFeedbackDensity { get; set; }
        public ActivationFactory.ActivationType ContextNeuronActivation { get; set; }
        public double ContextNeuronInWeightScale { get; set; }
        public double ContextNeuronOutWeightScale { get; set; }
        public bool FeedbackFeature { get; set; }
        public double FeedbackConnectionDensity { get; set; }
        public double FeedbackWeightScale { get; set; }
        public List<string> FeedbackFieldsNames { get; set; }

        //Constructors
        /// <summary>Creates uninitialized reservoir settings instance</summary>
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
            FeedbackFieldsNames = new List<string>();
            return;
        }

        /// <summary>
        /// Creates this instance as a deep copy of source instance
        /// </summary>
        /// <param name="source">Source settings</param>
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
            FeedbackFieldsNames = new List<string>(source.FeedbackFieldsNames);
            return;
        }

        /// <summary>
        /// Creates instance and initialize it from given xml element
        /// </summary>
        /// <param name="reservoirSettingsElem">Xml element containing reservoir settings</param>
        public AnalogReservoirSettings(XElement reservoirSettingsElem)
        {
            //Validation
            //A very ugly validation
            XmlValidator validator = new XmlValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddSchema(assemblyRCNet.GetManifestResourceStream("RCNet.Neural.Network.EchoState.AnalogReservoirSettings.xsd"));
            validator.AddSchema(assemblyRCNet.GetManifestResourceStream("RCNet.Neural.NeuralSettingsTypes.xsd"));
            validator.LoadXDocFromString(reservoirSettingsElem.ToString());
            //Parsing
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
            //Retirement neurons
            XElement retirementElem = internalElem.Descendants("RetirementNeurons").FirstOrDefault();
            RetainmentNeuronsFeature = (retirementElem != null);
            if (RetainmentNeuronsFeature)
            {
                RetainmentNeuronsDensity = double.Parse(retirementElem.Attribute("Density").Value, CultureInfo.InvariantCulture);
                RetainmentMinRate = double.Parse(retirementElem.Attribute("RetirementMinRate").Value, CultureInfo.InvariantCulture);
                RetainmentMaxRate = double.Parse(retirementElem.Attribute("RetirementMaxRate").Value, CultureInfo.InvariantCulture);
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
            FeedbackFieldsNames = new List<string>();
            if (FeedbackFeature)
            {
                FeedbackConnectionDensity = double.Parse(feedbackElem.Attribute("Density").Value, CultureInfo.InvariantCulture);
                FeedbackWeightScale = double.Parse(feedbackElem.Attribute("WeightScale").Value, CultureInfo.InvariantCulture);
                foreach (XElement feedbackFieldElem in feedbackElem.Descendants("Field"))
                {
                    FeedbackFieldsNames.Add(feedbackFieldElem.Attribute("Name").Value);
                }
                FeedbackFeature = (FeedbackFieldsNames.Count > 0);
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
        public static ReservoirTopologyType ParseReservoirTopology(string code)
        {
            switch (code.ToUpper())
            {
                case "RANDOM": return ReservoirTopologyType.Random;
                case "RING": return ReservoirTopologyType.Ring;
                case "DTT": return ReservoirTopologyType.DTT;
                default:
                    throw new ArgumentException($"Unknown reservoir topology code {code}");
            }
        }

        //Instance methods
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
        public sealed class RandomTopology
        {
            //Attributes
            public double ConnectionsDensity { get; set; }

            //Constructors
            public RandomTopology()
            {
                ConnectionsDensity = 0;
                return;
            }

            public RandomTopology(RandomTopology source)
            {
                ConnectionsDensity = source.ConnectionsDensity;
                return;
            }

            public RandomTopology(XElement randomTopologyElem)
            {
                ConnectionsDensity = double.Parse(randomTopologyElem.Attribute("ConnectionsDensity").Value, CultureInfo.InvariantCulture);
                return;
            }

            //Methods
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

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

        }//RandomTopology

        /// <summary>
        /// Additional setup parameters for Ring reservoir topology
        /// </summary>
        [Serializable]
        public sealed class RingTopology
        {
            //Attributes
            public bool BiDirection { get; set; }
            public double SelfConnectionsDensity { get; set; }
            public double InterConnectionsDensity { get; set; }

            //Constructors
            public RingTopology()
            {
                BiDirection = false;
                SelfConnectionsDensity = 0;
                InterConnectionsDensity = 0;
                return;
            }

            public RingTopology(RingTopology source)
            {
                BiDirection = source.BiDirection;
                SelfConnectionsDensity = source.SelfConnectionsDensity;
                InterConnectionsDensity = source.InterConnectionsDensity;
                return;
            }

            public RingTopology(XElement ringTopologyElem)
            {
                BiDirection = bool.Parse(ringTopologyElem.Attribute("BiDirection").Value);
                SelfConnectionsDensity = double.Parse(ringTopologyElem.Attribute("SelfConnectionsDensity").Value, CultureInfo.InvariantCulture);
                InterConnectionsDensity = double.Parse(ringTopologyElem.Attribute("InterConnectionsDensity").Value, CultureInfo.InvariantCulture);
                return;
            }

            //Methods
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

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

        }//RingTopology

        /// <summary>
        /// Additional setup parameters for DTT reservoir topology
        /// </summary>
        [Serializable]
        public sealed class DTTTopology
        {
            //Attributes
            public double SelfConnectionsDensity { get; set; }

            //Constructors
            public DTTTopology()
            {
                SelfConnectionsDensity = 0;
                return;
            }

            public DTTTopology(DTTTopology source)
            {
                SelfConnectionsDensity = source.SelfConnectionsDensity;
                return;
            }

            public DTTTopology(XElement dttTopologyElem)
            {
                SelfConnectionsDensity = double.Parse(dttTopologyElem.Attribute("SelfConnectionsDensity").Value, CultureInfo.InvariantCulture);
                return;
            }

            //Methods
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

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

        }//DTTTopology

    }//AnalogReservoirSettings

}//Namespace
