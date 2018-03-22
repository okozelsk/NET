using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using OKOSW.XMLTools;
using OKOSW.Neural.Activation;

namespace OKOSW.Neural.Reservoir.Analog
{
    /// <summary>
    /// Reservoir settings
    /// </summary>
    [Serializable]
    public class AnalogReservoirSettings
    {
        //Constants
        /// <summary>Supported types of reservoir topologies</summary>
        public enum ReservoirTopologyType
        {
            /// <summary>Random topology.</summary>
            Random,
            /// <summary>Ring topology</summary>
            Ring,
            /// <summary>Doubly twisted toroidal topology</summary>
            DTT
        };

        //Attributes
        public string CfgName { get; set; }
        public bool AugmentedStatesFeature { get; set; }
        public double BiasScale { get; set; }
        public double InputConnectionDensity { get; set; }
        public double InputWeightScale { get; set; }
        public int Size { get; set; }
        public ActivationFactory.ActivationType ReservoirNeuronActivation { get; set; }
        public double InternalWeightScale { get; set; }
        public ReservoirTopologyType Topology { get; set; }
        public RandomTopologyConfig RandomTopologyCfg { get; set; }
        public RingTopologyConfig RingTopologyCfg { get; set; }
        public DTTTopologyConfig DTTTopologyCfg { get; set; }
        public double RetainmentNeuronsDensity { get; set; }
        public double RetainmentMinRate { get; set; }
        public double RetainmentMaxRate { get; set; }
        public double ContextNeuronFeedbackDensity { get; set; }
        public ActivationFactory.ActivationType ContextNeuronActivation { get; set; }
        public double ContextNeuronInWeightScale { get; set; }
        public double ContextNeuronOutWeightScale { get; set; }
        public double FeedbackConnectionDensity { get; set; }
        public double FeedbackWeightScale { get; set; }

        //Constructors
        public AnalogReservoirSettings()
        {
            CfgName = "Unnamed";
            AugmentedStatesFeature = false; //Default is no augmented states
            BiasScale = 0;
            InputConnectionDensity = 1; //Default is full input connection
            InputWeightScale = 0.2;
            Size = 200; //Normal number of reservoir neurons
            ReservoirNeuronActivation = ActivationFactory.ActivationType.Tanh; //Default is Tanh
            InternalWeightScale = 0.2;
            Topology = ReservoirTopologyType.Random; //Default is random
            RandomTopologyCfg = new RandomTopologyConfig();
            RingTopologyCfg = null;
            DTTTopologyCfg = null;
            RetainmentNeuronsDensity = 0; //Default is no leaky integrators
            RetainmentMinRate = 0; //Default is no leaky integrators
            RetainmentMaxRate = 0; //Default is no leaky integrators
            ContextNeuronFeedbackDensity = 0; //Default is no context neuron feature
            ContextNeuronActivation = ReservoirNeuronActivation;
            ContextNeuronInWeightScale = 0;
            ContextNeuronOutWeightScale = 0;
            FeedbackConnectionDensity = 0; //Default is no feedback
            FeedbackWeightScale = 0;
            return;
        }

        public AnalogReservoirSettings(AnalogReservoirSettings source)
        {
            CfgName = source.CfgName;
            AugmentedStatesFeature = source.AugmentedStatesFeature;
            BiasScale = source.BiasScale;
            InputConnectionDensity = source.InputConnectionDensity;
            InputWeightScale = source.InputWeightScale;
            Size = source.Size;
            ReservoirNeuronActivation = source.ReservoirNeuronActivation;
            InternalWeightScale = source.InternalWeightScale;
            Topology = source.Topology;
            RandomTopologyCfg = null;
            RingTopologyCfg = null;
            DTTTopologyCfg = null;
            if (source.RandomTopologyCfg != null)
            {
                RandomTopologyCfg = new RandomTopologyConfig(source.RandomTopologyCfg);
            }
            if (source.RingTopologyCfg != null)
            {
                RingTopologyCfg = new RingTopologyConfig(source.RingTopologyCfg);
            }
            if (source.DTTTopologyCfg != null)
            {
                DTTTopologyCfg = new DTTTopologyConfig(source.DTTTopologyCfg);
            }
            RetainmentNeuronsDensity = source.RetainmentNeuronsDensity;
            RetainmentMinRate = source.RetainmentMinRate;
            RetainmentMaxRate = source.RetainmentMaxRate;
            ContextNeuronFeedbackDensity = source.ContextNeuronFeedbackDensity;
            ContextNeuronActivation = source.ContextNeuronActivation;
            ContextNeuronInWeightScale = source.ContextNeuronInWeightScale;
            ContextNeuronOutWeightScale = source.ContextNeuronOutWeightScale;
            FeedbackConnectionDensity = source.FeedbackConnectionDensity;
            FeedbackWeightScale = source.FeedbackWeightScale;
            return;
        }

        public AnalogReservoirSettings(XElement resElem)
        {
            //Validation
            //A very ugly validation
            XmlValidator validator = new XmlValidator();
            Assembly neuralAssembly = Assembly.Load("Neural");
            validator.AddSchema(neuralAssembly.GetManifestResourceStream("OKOSW.Neural.Reservoir.Analog.AnalogReservoirSettings.xsd"));
            validator.AddSchema(neuralAssembly.GetManifestResourceStream("OKOSW.Neural.OKOSWNeuralSettingsTypes.xsd"));
            validator.LoadXDocFromString(resElem.ToString());
            //Parsing
            CfgName = resElem.Attribute("Name").Value;
            AugmentedStatesFeature = bool.Parse(resElem.Attribute("AugmentedStates").Value);
            //Input
            XElement inputElem = resElem.Descendants("Input").First();
            BiasScale = double.Parse(inputElem.Attribute("BiasScale").Value, CultureInfo.InvariantCulture);
            InputConnectionDensity = double.Parse(inputElem.Attribute("ConnectionDensity").Value, CultureInfo.InvariantCulture);
            InputWeightScale = double.Parse(inputElem.Attribute("WeightScale").Value, CultureInfo.InvariantCulture);
            //Internal
            XElement internalElem = resElem.Descendants("Internal").First();
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
                throw new Exception("Only one topology can be specified in reservoir settings.");
            }
            XElement topologyElem = topologyElems[0];
            //Random?
            if (topologyElem.Name == "RandomTopology")
            {
                RandomTopologyCfg = new RandomTopologyConfig(topologyElem);
                Topology = ReservoirTopologyType.Random;
            }
            //Ring?
            else if (topologyElem.Name == "RingTopology")
            {
                RingTopologyCfg = new RingTopologyConfig(topologyElem);
                Topology = ReservoirTopologyType.Ring;
            }
            else
            {
                //DTT
                DTTTopologyCfg = new DTTTopologyConfig(topologyElem);
                Topology = ReservoirTopologyType.DTT;
            }
            //Retirement neurons
            XElement retirementElem = internalElem.Descendants("RetirementNeurons").First();
            RetainmentNeuronsDensity = double.Parse(retirementElem.Attribute("Density").Value, CultureInfo.InvariantCulture);
            RetainmentMinRate = double.Parse(retirementElem.Attribute("RetirementMinRate").Value, CultureInfo.InvariantCulture);
            RetainmentMaxRate = double.Parse(retirementElem.Attribute("RetirementMaxRate").Value, CultureInfo.InvariantCulture);
            //Context neuron
            XElement ctxNeuronElem = internalElem.Descendants("ContextNeuron").First();
            ContextNeuronFeedbackDensity = double.Parse(ctxNeuronElem.Attribute("FeedbackDensity").Value, CultureInfo.InvariantCulture);
            ContextNeuronActivation = ActivationFactory.ParseActivation(ctxNeuronElem.Attribute("Activation").Value);
            ContextNeuronInWeightScale = double.Parse(ctxNeuronElem.Attribute("InWeightScale").Value, CultureInfo.InvariantCulture);
            ContextNeuronOutWeightScale = double.Parse(ctxNeuronElem.Attribute("OutWeightScale").Value, CultureInfo.InvariantCulture);
            //Feedback
            XElement feedbackElem = resElem.Descendants("Feedback").First();
            FeedbackConnectionDensity = double.Parse(feedbackElem.Attribute("Density").Value, CultureInfo.InvariantCulture);
            FeedbackWeightScale = double.Parse(feedbackElem.Attribute("WeightScale").Value, CultureInfo.InvariantCulture);
            return;
        }

        //Methods
        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(AnalogReservoirSettings)) return false;
            AnalogReservoirSettings cmpSettings = (AnalogReservoirSettings)obj;
            if (CfgName != cmpSettings.CfgName ||
                AugmentedStatesFeature != cmpSettings.AugmentedStatesFeature ||
                BiasScale != cmpSettings.BiasScale ||
                InputConnectionDensity != cmpSettings.InputConnectionDensity ||
                InputWeightScale != cmpSettings.InputWeightScale ||
                Size != cmpSettings.Size ||
                ReservoirNeuronActivation != cmpSettings.ReservoirNeuronActivation ||
                InternalWeightScale != cmpSettings.InternalWeightScale ||
                Topology != cmpSettings.Topology ||
                RetainmentNeuronsDensity != cmpSettings.RetainmentNeuronsDensity ||
                RetainmentMinRate != cmpSettings.RetainmentMinRate ||
                RetainmentMaxRate != cmpSettings.RetainmentMaxRate ||
                ContextNeuronFeedbackDensity != cmpSettings.ContextNeuronFeedbackDensity ||
                ContextNeuronActivation != cmpSettings.ContextNeuronActivation ||
                ContextNeuronInWeightScale != cmpSettings.ContextNeuronInWeightScale ||
                ContextNeuronOutWeightScale != cmpSettings.ContextNeuronOutWeightScale ||
                FeedbackConnectionDensity != cmpSettings.FeedbackConnectionDensity ||
                FeedbackWeightScale != cmpSettings.FeedbackWeightScale
                )
            {
                return false;
            }
            switch (Topology)
            {
                case ReservoirTopologyType.Random:
                    if (!RandomTopologyCfg.Equals(cmpSettings.RandomTopologyCfg)) return false;
                    break;
                case ReservoirTopologyType.Ring:
                    if (!RingTopologyCfg.Equals(cmpSettings.RingTopologyCfg)) return false;
                    break;
                case ReservoirTopologyType.DTT:
                    if (!DTTTopologyCfg.Equals(cmpSettings.DTTTopologyCfg)) return false;
                    break;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static ReservoirTopologyType ParseReservoirTopology(string code)
        {
            switch (code.ToUpper())
            {
                case "RANDOM": return ReservoirTopologyType.Random;
                case "RING": return ReservoirTopologyType.Ring;
                case "DTT": return ReservoirTopologyType.DTT;
                default:
                    throw new Exception("Unknown topology code " + code);
            }
        }

        //Inner classes
        /// <summary>
        /// Additional setup parameters for Random reservoir type
        /// </summary>
        [Serializable]
        public class RandomTopologyConfig
        {
            //Attributes
            public double ConnectionsDensity { get; set; }

            //Constructors
            public RandomTopologyConfig()
            {
                ConnectionsDensity = 0.1; //Typical density
                return;
            }

            public RandomTopologyConfig(RandomTopologyConfig source)
            {
                ConnectionsDensity = source.ConnectionsDensity;
                return;
            }

            public RandomTopologyConfig(XElement randomTopologyElem)
            {
                ConnectionsDensity = double.Parse(randomTopologyElem.Attribute("ConnectionsDensity").Value, CultureInfo.InvariantCulture);
                return;
            }

            //Methods
            public override bool Equals(object obj)
            {
                if (obj.GetType() != typeof(RandomTopologyConfig)) return false;
                RandomTopologyConfig cmpSettings = (RandomTopologyConfig)obj;
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

        }//RandomTopologyConfig

        /// <summary>
        /// Additional setup parameters for Ring reservoir type
        /// </summary>
        [Serializable]
        public class RingTopologyConfig
        {
            //Attributes
            public bool BiDirection { get; set; }
            public double SelfConnectionsDensity { get; set; }
            public double InterConnectionsDensity { get; set; }

            //Constructors
            public RingTopologyConfig()
            {
                BiDirection = false; //Single direction ring
                SelfConnectionsDensity = 0; //No self connected neurons
                InterConnectionsDensity = 0; //No additional inter-connections
                return;
            }

            public RingTopologyConfig(RingTopologyConfig source)
            {
                BiDirection = source.BiDirection;
                SelfConnectionsDensity = source.SelfConnectionsDensity;
                InterConnectionsDensity = source.InterConnectionsDensity;
                return;
            }

            public RingTopologyConfig(XElement ringTopologyElem)
            {
                BiDirection = bool.Parse(ringTopologyElem.Attribute("BiDirection").Value);
                SelfConnectionsDensity = double.Parse(ringTopologyElem.Attribute("SelfConnectionsDensity").Value, CultureInfo.InvariantCulture);
                InterConnectionsDensity = double.Parse(ringTopologyElem.Attribute("InterConnectionsDensity").Value, CultureInfo.InvariantCulture);
                return;
            }

            //Methods
            public override bool Equals(object obj)
            {
                if (obj.GetType() != typeof(RingTopologyConfig)) return false;
                RingTopologyConfig cmpSettings = (RingTopologyConfig)obj;
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

        }//RingTopologyConfig

        /// <summary>
        /// Additional setup parameters for DTT reservoir type
        /// </summary>
        [Serializable]
        public class DTTTopologyConfig
        {
            //Attributes
            public double SelfConnectionsDensity { get; set; }

            //Constructors
            public DTTTopologyConfig()
            {
                SelfConnectionsDensity = 0; //No self connected neurons
                return;
            }

            public DTTTopologyConfig(DTTTopologyConfig source)
            {
                SelfConnectionsDensity = source.SelfConnectionsDensity;
                return;
            }

            public DTTTopologyConfig(XElement dttTopologyElem)
            {
                SelfConnectionsDensity = double.Parse(dttTopologyElem.Attribute("SelfConnectionsDensity").Value, CultureInfo.InvariantCulture);
                return;
            }

            //Methods
            public override bool Equals(object obj)
            {
                if (obj.GetType() != typeof(DTTTopologyConfig)) return false;
                DTTTopologyConfig cmpSettings = (DTTTopologyConfig)obj;
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

        }//DTTTopologyConfig

    }//ReservoirConfig

}
