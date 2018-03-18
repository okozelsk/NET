using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Xml;
using OKOSW.Extensions;
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
        /// <summary>Type of supported reservoir topologies</summary>
        public enum EnumReservoirTopology
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
        public ActivationFactory.EnumActivationType ReservoirNeuronActivation { get; set; }
        public double InternalWeightScale { get; set; }
        public EnumReservoirTopology Topology { get; set; }
        public RandomTopologyConfig RandomTopologyCfg { get; set; }
        public RingTopologyConfig RingTopologyCfg { get; set; }
        public DTTTopologyConfig DTTTopologyCfg { get; set; }
        public double RetainmentNeuronsDensity { get; set; }
        public double RetainmentMinRate { get; set; }
        public double RetainmentMaxRate { get; set; }
        public double ContextNeuronFeedbackDensity { get; set; }
        public ActivationFactory.EnumActivationType ContextNeuronActivation { get; set; }
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
            ReservoirNeuronActivation = ActivationFactory.EnumActivationType.Tanh; //Default is Tanh
            InternalWeightScale = 0.2;
            Topology = EnumReservoirTopology.Random; //Default is random
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

        public AnalogReservoirSettings(XmlNode xmlNode)
        {
            CfgName = xmlNode.Attributes["Name"].Value;
            AugmentedStatesFeature = bool.Parse(xmlNode.Attributes["AugmentedStates"].Value);
            //Input
            XmlNode inputNode = xmlNode.SelectSingleNode("Input");
            BiasScale = double.Parse(inputNode.Attributes["BiasScale"].Value, CultureInfo.InvariantCulture);
            InputConnectionDensity = double.Parse(inputNode.Attributes["ConnectionDensity"].Value, CultureInfo.InvariantCulture);
            InputWeightScale = double.Parse(inputNode.Attributes["WeightScale"].Value, CultureInfo.InvariantCulture);
            //Internal
            XmlNode internalNode = xmlNode.SelectSingleNode("Internal");
            Size = int.Parse(internalNode.Attributes["Size"].Value);
            ReservoirNeuronActivation = ActivationFactory.ParseActivation(internalNode.Attributes["Activation"].Value);
            InternalWeightScale = double.Parse(internalNode.Attributes["WeightScale"].Value, CultureInfo.InvariantCulture);
            //Topology
            XmlNode topologyNode = null;
            List<XmlNode> topologyNodes = new List<XmlNode>();
            if ((topologyNode = internalNode.SelectSingleNode("RandomTopology")) != null) topologyNodes.Add(topologyNode);
            if ((topologyNode = internalNode.SelectSingleNode("RingTopology")) != null) topologyNodes.Add(topologyNode);
            if ((topologyNode = internalNode.SelectSingleNode("DTTTopology")) != null) topologyNodes.Add(topologyNode);
            if(topologyNodes.Count != 1)
            {
                throw new Exception("Only one topology can be specified in reservoir settings.");
            }
            topologyNode = topologyNodes[0];
            //Random?
            if (topologyNode.Name == "RandomTopology")
            {
                RandomTopologyCfg = new RandomTopologyConfig(topologyNode);
                Topology = EnumReservoirTopology.Random;
            }
            //Ring?
            else if (topologyNode.Name == "RingTopology")
            {
                RingTopologyCfg = new RingTopologyConfig(topologyNode);
                Topology = EnumReservoirTopology.Ring;
            }
            else
            {
                //DTT
                DTTTopologyCfg = new DTTTopologyConfig(topologyNode);
                Topology = EnumReservoirTopology.DTT;
            }
            //Retirement neurons
            XmlNode retirementNode = internalNode.SelectSingleNode("RetirementNeurons");
            RetainmentNeuronsDensity = double.Parse(retirementNode.Attributes["Density"].Value, CultureInfo.InvariantCulture);
            RetainmentMinRate = double.Parse(retirementNode.Attributes["RetirementMinRate"].Value, CultureInfo.InvariantCulture);
            RetainmentMaxRate = double.Parse(retirementNode.Attributes["RetirementMaxRate"].Value, CultureInfo.InvariantCulture);
            //Context neuron
            XmlNode ctxNeuronNode = internalNode.SelectSingleNode("ContextNeuron");
            ContextNeuronFeedbackDensity = double.Parse(ctxNeuronNode.Attributes["FeedbackDensity"].Value, CultureInfo.InvariantCulture);
            ContextNeuronActivation = ActivationFactory.ParseActivation(ctxNeuronNode.Attributes["Activation"].Value);
            ContextNeuronInWeightScale = double.Parse(ctxNeuronNode.Attributes["InWeightScale"].Value, CultureInfo.InvariantCulture);
            ContextNeuronOutWeightScale = double.Parse(ctxNeuronNode.Attributes["OutWeightScale"].Value, CultureInfo.InvariantCulture);
            //Feedback
            XmlNode feedbackNode = xmlNode.SelectSingleNode("Feedback");
            FeedbackConnectionDensity = double.Parse(feedbackNode.Attributes["Density"].Value, CultureInfo.InvariantCulture);
            FeedbackWeightScale = double.Parse(feedbackNode.Attributes["WeightScale"].Value, CultureInfo.InvariantCulture);
            return;
        }

        //Methods
        /// <summary>Checkes if this settings are equivalent to specified settings</summary>
        /// <param name="cmpSettings">Settings to be compared with this settings</param>
        public bool IsEquivalent(AnalogReservoirSettings cmpSettings)
        {
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
                case EnumReservoirTopology.Random:
                    if (!RandomTopologyCfg.IsEquivalent(cmpSettings.RandomTopologyCfg)) return false;
                    break;
                case EnumReservoirTopology.Ring:
                    if (!RingTopologyCfg.IsEquivalent(cmpSettings.RingTopologyCfg)) return false;
                    break;
                case EnumReservoirTopology.DTT:
                    if (!DTTTopologyCfg.IsEquivalent(cmpSettings.DTTTopologyCfg)) return false;
                    break;
            }
            return true;
        }

        public static EnumReservoirTopology ParseReservoirTopology(string code)
        {
            switch (code.ToUpper())
            {
                case "RANDOM": return EnumReservoirTopology.Random;
                case "RING": return EnumReservoirTopology.Ring;
                case "DTT": return EnumReservoirTopology.DTT;
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

            public RandomTopologyConfig(XmlNode xmlNode)
            {
                ConnectionsDensity = double.Parse(xmlNode.Attributes["ConnectionsDensity"].Value, CultureInfo.InvariantCulture);
                return;
            }
            /// <summary>Checkes if this settings are equivalent to specified settings</summary>
            /// <param name="cmpSettings">Settings to be compared with this settings</param>
            public bool IsEquivalent(RandomTopologyConfig cmpSettings)
            {
                if (ConnectionsDensity != cmpSettings.ConnectionsDensity)
                {
                    return false;
                }
                return true;
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

            public RingTopologyConfig(XmlNode xmlNode)
            {
                BiDirection = bool.Parse(xmlNode.Attributes["BiDirection"].Value);
                SelfConnectionsDensity = double.Parse(xmlNode.Attributes["SelfConnectionsDensity"].Value, CultureInfo.InvariantCulture);
                InterConnectionsDensity = double.Parse(xmlNode.Attributes["InterConnectionsDensity"].Value, CultureInfo.InvariantCulture);
                return;
            }
            /// <summary>Checkes if this settings are equivalent to specified settings</summary>
            /// <param name="cmpSettings">Settings to be compared with this settings</param>
            public bool IsEquivalent(RingTopologyConfig cmpSettings)
            {
                if (BiDirection != cmpSettings.BiDirection ||
                   SelfConnectionsDensity != cmpSettings.SelfConnectionsDensity ||
                   InterConnectionsDensity != cmpSettings.InterConnectionsDensity
                   )
                {
                    return false;
                }
                return true;
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

            public DTTTopologyConfig(XmlNode xmlNode)
            {
                SelfConnectionsDensity = double.Parse(xmlNode.Attributes["SelfConnectionsDensity"].Value, CultureInfo.InvariantCulture);
                return;
            }
            /// <summary>Checkes if this settings are equivalent to specified settings</summary>
            /// <param name="cmpSettings">Settings to be compared with this settings</param>
            public bool IsEquivalent(DTTTopologyConfig cmpSettings)
            {
                if (SelfConnectionsDensity != cmpSettings.SelfConnectionsDensity)
                {
                    return false;
                }
                return true;
            }

        }//DTTTopologyConfig

    }//ReservoirConfig

}
