using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using OKOSW.MathTools;
using OKOSW.Extensions;
using OKOSW.CSVTools;
using OKOSW.Neural.Activation;

namespace OKOSW.Neural.Reservoir.Analog
{
    /// <summary>
    /// Implements input component of reservoir
    /// </summary>
    [Serializable]
    public class ReservoirInputBlock
    {
        //Constants
        //Attributes
        private int m_inputValuesCount;
        private double[] m_inputBiases;
        private double[] m_inputValues;
        private int m_reservoirNeuronsCount;
        private List<InputConnection>[] m_connections;

        //Constructor
        public ReservoirInputBlock(int inputValuesCount, int reservoirNeuronsCount, double biasScale, double inputWeightScale, int neuronsPerInput, Random rand)
        {
            m_inputValuesCount = inputValuesCount;
            m_reservoirNeuronsCount = reservoirNeuronsCount;
            //Input biases
            m_inputBiases = new double[m_reservoirNeuronsCount];
            rand.FillUniform(m_inputBiases, -1, 1, biasScale);
            //Input values
            m_inputValues = new double[m_inputValuesCount];
            m_inputValues.Populate(0);
            //Connections to reservoir neurons
            m_connections = new List<InputConnection>[m_reservoirNeuronsCount];
            for(int i = 0; i < m_reservoirNeuronsCount; i++)
            {
                m_connections[i] = new List<InputConnection>(m_inputValuesCount * neuronsPerInput);
            }
            int[] neuronIdxs = new int[m_reservoirNeuronsCount];
            for (int fieldIdx = 0; fieldIdx < m_inputValuesCount; fieldIdx++)
            {
                neuronIdxs.ShuffledIndices(rand);
                for(int i = 0; i < neuronsPerInput; i++)
                {
                    InputConnection connection = new InputConnection();
                    connection.FieldIdx = fieldIdx;
                    connection.Weight = rand.NextBoundedUniformDouble() * inputWeightScale;
                    m_connections[neuronIdxs[i]].Add(connection);
                }
            }
            return;
        }
        //Properties
        public int InputValuesCount { get { return m_inputValuesCount; } }

        //Methods
        public void Update(double[] newInputValues)
        {
            newInputValues.CopyTo(m_inputValues, 0);
            return;
        }

        public double GetInputSignal(int reservoirNeuronIdx)
        {
            double signal = 0;
            if(m_connections[reservoirNeuronIdx].Count > 0)
            {
                signal += m_inputBiases[reservoirNeuronIdx];
                foreach(InputConnection connection in m_connections[reservoirNeuronIdx])
                {
                    signal += m_inputValues[connection.FieldIdx] * connection.Weight;
                }
            }
            return signal;
        }

        //Inner classes
        [Serializable]
        private class InputConnection
        {
            public int FieldIdx { get; set; } = 0;
            public double Weight { get; set; } = 0;
        }

    }//ReservoirInputBlock

    /// <summary>
    /// Implements reservoir supporting various topologies and features
    /// </summary>
    [Serializable]
    public class AnalogReservoir : IAnalogReservoir
    {
        //Attributes
        private string m_ID;
        private string m_configName;
        private Random m_rand;
        private ReservoirInputBlock m_inputBlock;
        private AnalogNeuron[] m_neurons;
        private List<int>[] m_partyNeuronsIdxs;
        private List<double>[] m_partyNeuronsWeights;
        private bool m_contextNeuronFeature;
        private AnalogNeuron m_contextNeuron;
        private double[] m_neurons2ContextWeights;
        private double[] m_context2NeuronsWeights;
        private bool m_feedbackFeature;
        private double[] m_feedback;
        private double[] m_feedbackWeights;
        private bool m_augmentedStatesFeature;

        /// <summary>
        /// Constructs computing reservoir
        /// </summary>
        /// <param name="ID">Reservoir identifier (together with reservoir configuration name has to be unique)</param>
        /// <param name="inputValuesCount">Count of reservoir input falues</param>
        /// <param name="feedbackValuesCount">Count of ESN output values</param>
        /// <param name="settings">Reservoir initialization parameters</param>
        /// <param name="randomizerSeek">
        /// Calling constructor with the same randomizerSeek greater or equal to 0 ensures the same reservoir initialization.
        /// Specify randomizerSeek less than 0 to have different initialization after aech call.
        /// </param>
        public AnalogReservoir(int ID, int inputValuesCount, int feedbackValuesCount, ReservoirConfig settings, int randomizerSeek = -1)
        {
            //--------------------------------------------------------
            //Configuration name
            m_configName = settings.CfgName;
            //Reservoir ID
            m_ID = m_configName + "(" + ID.ToString() + ")";
            //--------------------------------------------------------
            //Random object initialization
            if (randomizerSeek < 0) m_rand = new Random();
            else m_rand = new Random(randomizerSeek);
            //--------------------------------------------------------
            //Input memory and connections
            int neuronsPerInput = Math.Max(1, (int)Math.Round((double)settings.Size * settings.InputConnectionDensity, 0));
            m_inputBlock = new ReservoirInputBlock(inputValuesCount, settings.Size, settings.BiasScale, settings.InputWeightScale, neuronsPerInput, m_rand);
            //--------------------------------------------------------
            //Reservoir neurons
            m_neurons = new AnalogNeuron[settings.Size];
            //Neurons retainment rates
            double[] retainmentRates = new double[m_neurons.Length];
            retainmentRates.Populate(0);
            int retainmentNeuronsCount = (int)Math.Round((double)m_neurons.Length * settings.RetainmentNeuronsDensity, 0);
            if (retainmentNeuronsCount > 0 && settings.RetainmentMaxRate > 0)
            {
                m_rand.FillUniform(retainmentRates, settings.RetainmentMinRate, settings.RetainmentMaxRate, 1, retainmentNeuronsCount);
                m_rand.Shuffle(retainmentRates);
            }
            //Neurons creation
            for (int n = 0; n < m_neurons.Length; n++)
            {
                m_neurons[n] = new AnalogNeuron(ActivationFactory.CreateAF(settings.ReservoirNeuronActivation), retainmentRates[n]);
            }
            //Helper array for neurons order randomizations purposes
            int[] neuronsShuffledIndices = new int[settings.Size];
            //--------------------------------------------------------
            //Context neuron feature
            int contextNeuronFeedbacksCount = (int)Math.Round((double)m_neurons.Length * settings.ContextNeuronFeedbackDensity, 0);
            m_contextNeuron = null;
            m_neurons2ContextWeights = null;
            m_context2NeuronsWeights = null;
            m_contextNeuronFeature = (contextNeuronFeedbacksCount > 0);
            if (m_contextNeuronFeature)
            {
                m_contextNeuron = new AnalogNeuron(ActivationFactory.CreateAF(settings.ReservoirNeuronActivation), 0);
                //Weights from each res neuron to context neuron
                m_neurons2ContextWeights = new double[m_neurons.Length];
                m_rand.FillUniform(m_neurons2ContextWeights);
                //Weights from context neuron to res neurons
                m_context2NeuronsWeights = new double[m_neurons.Length];
                m_context2NeuronsWeights.Populate(0);
                neuronsShuffledIndices.ShuffledIndices(m_rand);
                for (int i = 0; i < contextNeuronFeedbacksCount && i < m_neurons.Length; i++)
                {
                    m_context2NeuronsWeights[neuronsShuffledIndices[i]] = m_rand.NextBoundedUniformDouble();
                }
            }
            //--------------------------------------------------------
            //Feedback feature and weights
            int neuronsPerOutput = (int)Math.Round(settings.FeedbackConnectionDensity * (double)m_neurons.Length, 0);
            m_feedback = new double[feedbackValuesCount];
            m_feedback.Populate(0);
            m_feedbackWeights = null;
            m_feedbackFeature = (neuronsPerOutput > 0);
            if (m_feedbackFeature)
            {
                //Feedback weights
                m_feedbackWeights = new double[feedbackValuesCount * m_neurons.Length];
                m_feedbackWeights.Populate(0);
                for (int outNo = 0; outNo < feedbackValuesCount; outNo++)
                {
                    neuronsShuffledIndices.ShuffledIndices(m_rand);
                    for (int i = 0; i < neuronsPerOutput; i++)
                    {
                        double weight = m_rand.NextBoundedUniformDouble(-1, 1) * settings.FeedbackWeightScale; ;
                        m_feedbackWeights[outNo * m_neurons.Length + neuronsShuffledIndices[i]] = weight;
                    }
                }
            }
            //--------------------------------------------------------
            //Reservoir topology -> fashion of connections between reservoir neurons
            m_partyNeuronsIdxs = new List<int>[m_neurons.Length];
            m_partyNeuronsWeights = new List<double>[m_neurons.Length];
            for (int i = 0; i < m_neurons.Length; i++)
            {
                m_partyNeuronsIdxs[i] = new List<int>();
                m_partyNeuronsWeights[i] = new List<double>();
            }
            switch (settings.Topology)
            {
                case ReservoirConfig.EnumReservoirTopology.Random:
                    SetupRandomTopology(settings.RandomTopologyCfg);
                    break;
                case ReservoirConfig.EnumReservoirTopology.Ring:
                    SetupRingTopology(settings.RingTopologyCfg);
                    break;
                case ReservoirConfig.EnumReservoirTopology.DTT:
                    SetupDTTTopology(settings.DTTTopologyCfg);
                    break;
            }
            //Scale all internal weights
            for (int i = 0; i < m_neurons.Length; i++)
            {
                for (int j = 0; j < m_partyNeuronsWeights[i].Count; j++)
                {
                    m_partyNeuronsWeights[i][j] *= settings.InternalWeightScale;
                }
            }
            if (m_contextNeuronFeature)
            {
                m_neurons2ContextWeights.Scale(settings.InternalWeightScale);
                m_context2NeuronsWeights.Scale(settings.InternalWeightScale);
            }
            //--------------------------------------------------------
            //Augmented states
            m_augmentedStatesFeature = settings.AugmentedStatesFeature;
            return;
        }

        private bool AddConnection(int targetNeuronIdx, int partyNeuronIdx, bool check = true)
        {
            if (!check || !m_partyNeuronsIdxs[targetNeuronIdx].Contains(partyNeuronIdx))
            {
                m_partyNeuronsIdxs[targetNeuronIdx].Add(partyNeuronIdx);
                m_partyNeuronsWeights[targetNeuronIdx].Add(m_rand.NextBoundedUniformDouble());
                return true;
            }
            return false;
        }

        private bool AddConnection(int connectionID, bool check = true)
        {
            return AddConnection(connectionID / m_neurons.Length, connectionID % m_neurons.Length);
        }

        private void SetRingConnections(bool biDirection, bool check = true)
        {
            for (int i = 0; i < m_neurons.Length; i++)
            {
                int partyNeuronIdx = (i == 0) ? (m_neurons.Length - 1) : (i - 1);
                AddConnection(i, partyNeuronIdx, check);
                if(biDirection)
                {
                    partyNeuronIdx = (i == m_neurons.Length - 1) ? (0) : (i + 1);
                    AddConnection(i, partyNeuronIdx, check);
                }
            }
            return;
        }

        private void SetSelfConnections(double density, bool check = true)
        {
            int connectionsCount = (int)Math.Round((double)m_neurons.Length * density);
            int[] indices = new int[m_neurons.Length];
            indices.ShuffledIndices(m_rand);
            for (int i = 0; i < connectionsCount; i++)
            {
                AddConnection(indices[i], indices[i], check);
            }
            return;
        }

        private void SetInterConnections(double density, bool check = true)
        {
            int connectionsCount = (int)Math.Round((double)((m_neurons.Length - 1) * m_neurons.Length) * density);
            int[] randomConnections = new int[(m_neurons.Length - 1) * m_neurons.Length];
            int indicesPos = 0;
            for(int n1Idx = 0; n1Idx < m_neurons.Length; n1Idx++)
            {
                for(int n2Idx = 0; n2Idx < m_neurons.Length; n2Idx++)
                {
                    if(n1Idx != n2Idx)
                    {
                        randomConnections[indicesPos] = n1Idx * m_neurons.Length + n2Idx;
                        ++indicesPos;
                    }
                }
            }
            m_rand.Shuffle(randomConnections);
            for (int i = 0; i < connectionsCount; i++)
            {
                AddConnection(randomConnections[i], check);
            }
            return;
        }

        private void SetupRandomTopology(ReservoirConfig.RandomTopologyConfig cfg)
        {
            //Fully random connections setup
            int connectionsCount = (int)Math.Round((double)m_neurons.Length * (double)m_neurons.Length * cfg.ConnectionsDensity);
            int[] randomConnections = new int[m_neurons.Length * m_neurons.Length];
            randomConnections.ShuffledIndices(m_rand);
            for (int i = 0; i < connectionsCount; i++)
            {
                AddConnection(randomConnections[i], false);
            }
            return;
        }

        private void SetupRingTopology(ReservoirConfig.RingTopologyConfig cfg)
        {
            //Ring connections part
            SetRingConnections(cfg.BiDirection, false);
            //Self connections part
            SetSelfConnections(cfg.SelfConnectionsDensity, false);
            //Inter connections part
            SetInterConnections(cfg.InterConnectionsDensity, true);
            return;
        }

        private void SetupDTTTopology(ReservoirConfig.DTTTopologyConfig cfg)
        {
            //HTwist part (single direction ring)
            SetRingConnections(false);
            //VTwist part
            int step = (int)Math.Floor(Math.Sqrt(m_neurons.Length));
            for (int partyNeuronIdx = 0; partyNeuronIdx < m_neurons.Length; partyNeuronIdx++)
            {
                int targetNeuronIdx = partyNeuronIdx + step;
                if (targetNeuronIdx > m_neurons.Length - 1)
                {
                    int left = partyNeuronIdx % step;
                    targetNeuronIdx = (left == 0) ? (step - 1) : (left - 1);
                }
                AddConnection(targetNeuronIdx, partyNeuronIdx, false);
            }
            //Self connections part
            SetSelfConnections(cfg.SelfConnectionsDensity);
            return;
        }


        #region IReservoir implementation

        //Properties
        /// <summary>
        /// Reservoir unique ID.
        /// </summary>
        public string ID { get { return m_ID; } }

        /// <summary>
        /// Reservoir name.
        /// </summary>
        public string ConfigName { get { return m_configName; } }

        /// <summary>
        /// Reservoir size. (Reservoir neurons count)
        /// </summary>
        public int Size { get { return m_neurons.Length; } }

        /// <summary>
        /// Reservoir outputs (predictors) count from internal neurons.
        /// </summary>
        public int OutputPredictorsCount { get { return m_augmentedStatesFeature ? m_neurons.Length * 2 : m_neurons.Length; } }

        /// <summary>
        /// Reservoir neurons.
        /// </summary>
        public AnalogNeuron[] Neurons { get { return m_neurons; } }

        //Methods
        /// <summary>
        /// Resets all reservoir neurons to their initial state (typicaly zero),
        /// so client then can start the computing from the beginning.
        /// Function does not affect weights or internal structure.
        /// </summary>
        public void Reset()
        {
            foreach (AnalogNeuron neuron in m_neurons)
            {
                neuron.Reset();
            }
            if(m_contextNeuronFeature)m_contextNeuron.Reset();
            m_feedback.Populate(0);
            return;
        }

        /// <summary>
        /// Computes reservoir neurons new states and returns new set of reservoir output predictors.
        /// </summary>
        /// <param name="input">Array of new input values.</param>
        /// <param name="outputPredictors">Array of output predictors values. Array has to be sized to OutputPredictorsCount reservoir property.</param>
        /// <param name="collectStatistics">Switch if to collect statistics. Typical usage is FALSE within boot phase and TRUE after boot phase.</param>
        public void Compute(double[] input, double[] outputPredictors, bool collectStatistics)
        {
            //Update input memory
            m_inputBlock.Update(input);
            //Store all reservoir neurons states
            foreach(AnalogNeuron neuron in m_neurons)
            {
                neuron.StoreCurrentState();
            }
            //Compute new states of all reservoir neurons and fill output array of predictors
            Parallel.For(0, m_neurons.Length, (neuronIdx) =>
            {
                //----------------------------------------------------
                //Input signal
                double inputSignal = m_inputBlock.GetInputSignal(neuronIdx);
                //----------------------------------------------------
                //Signal from reservoir neurons
                double reservoirSignal = 0;
                //Add reservoir neurons signal
                for (int j = 0; j < m_partyNeuronsIdxs[neuronIdx].Count; j++)
                {
                    reservoirSignal += m_partyNeuronsWeights[neuronIdx][j] * m_neurons[m_partyNeuronsIdxs[neuronIdx][j]].PreviousState;
                }
                //Add context neuron signal if allowed
                reservoirSignal += m_contextNeuronFeature ? m_context2NeuronsWeights[neuronIdx] * m_contextNeuron.CurrentState : 0;
                //----------------------------------------------------
                //Feedback signal
                double feedbackSignal = 0;
                if (m_feedbackFeature)
                {
                    for (int outpIdx = 0; outpIdx < m_feedback.Length; outpIdx++)
                    {
                        feedbackSignal += m_feedback[outpIdx] * m_feedbackWeights[outpIdx * m_neurons.Length + neuronIdx];
                    }
                }
                //----------------------------------------------------
                //Set new state of reservoir neuron
                m_neurons[neuronIdx].NewState(inputSignal + reservoirSignal + feedbackSignal, collectStatistics);
                //----------------------------------------------------
                //Set neuron state to output predictors
                outputPredictors[neuronIdx] = m_neurons[neuronIdx].CurrentState;
                //----------------------------------------------------
                //Set neuron augmented state to output predictors
                if (m_augmentedStatesFeature)
                {
                    outputPredictors[m_neurons.Length + neuronIdx] = outputPredictors[neuronIdx] * outputPredictors[neuronIdx];
                }
            });
            //----------------------------------------------------
            //New state of context neuron if allowed
            if (m_contextNeuronFeature)
            {
                double res2ContextSignal = 0;
                for (int neuronIdx = 0; neuronIdx < m_neurons.Length; neuronIdx++)
                {
                    res2ContextSignal += m_neurons2ContextWeights[neuronIdx] * m_neurons[neuronIdx].CurrentState;
                }
                m_contextNeuron.NewState(res2ContextSignal, collectStatistics);
            }
            return;
        }

        /// <summary>
        /// Sets feedback values for next computation round
        /// </summary>
        /// <param name="feedback">Array of feedback values.</param>
        public void SetFeedback(double[] feedback)
        {
            feedback.CopyTo(m_feedback, 0);
            return;
        }


        #endregion

        /// <summary>
        /// Reservoir settings
        /// </summary>
        [Serializable]
        public class ReservoirConfig
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
            public EnumReservoirTopology Topology { get; set; }
            public int Size { get; set; }
            public double BiasScale { get; set; }
            public double InputConnectionDensity { get; set; }
            public double InputWeightScale { get; set; }
            public double FeedbackConnectionDensity { get; set; }
            public double FeedbackWeightScale { get; set; }
            public double ContextNeuronFeedbackDensity { get; set; }
            public ActivationFactory.EnumActivationType ReservoirNeuronActivation { get; set; }
            public double RetainmentNeuronsDensity { get; set; }
            public double RetainmentMinRate { get; set; }
            public double RetainmentMaxRate { get; set; }
            public double InternalWeightScale { get; set; }
            public bool AugmentedStatesFeature { get; set; }
            public RandomTopologyConfig RandomTopologyCfg { get; set; }
            public RingTopologyConfig RingTopologyCfg { get; set; }
            public DTTTopologyConfig DTTTopologyCfg { get; set; }

            //Constructors
            public ReservoirConfig()
            {
                CfgName = "Unnamed";
                Topology = EnumReservoirTopology.Random; //Default is random
                Size = 200; //Typical number of reservoir neurons
                BiasScale = 0;
                InputConnectionDensity = 1; //Default is full input connection
                InputWeightScale = 0.2; //Default scale
                ContextNeuronFeedbackDensity = 0; //Default is no context neuron feature
                FeedbackConnectionDensity = 0; //Default is no feedback
                FeedbackWeightScale = 0; //Default is no feedback
                ReservoirNeuronActivation = ActivationFactory.EnumActivationType.Tanh; //Default is Tanh
                RetainmentNeuronsDensity = 0; //Default is no leaky integrators
                RetainmentMinRate = 0; //Default is no leaky integrators
                RetainmentMaxRate = 0; //Default is no leaky integrators
                InternalWeightScale = 0.2; //Default scale
                AugmentedStatesFeature = false; //Default is no augmented states
                RandomTopologyCfg = new RandomTopologyConfig(); //Default random topology config
                RingTopologyCfg = null;
                DTTTopologyCfg = null;
                return;
            }

            public ReservoirConfig(ReservoirConfig source)
            {
                CfgName = source.CfgName;
                Topology = source.Topology;
                Size = source.Size;
                BiasScale = source.BiasScale;
                InputConnectionDensity = source.InputConnectionDensity;
                InputWeightScale = source.InputWeightScale;
                ContextNeuronFeedbackDensity = source.ContextNeuronFeedbackDensity;
                FeedbackConnectionDensity = source.FeedbackConnectionDensity;
                FeedbackWeightScale = source.FeedbackWeightScale;
                ReservoirNeuronActivation = source.ReservoirNeuronActivation;
                RetainmentNeuronsDensity = source.RetainmentNeuronsDensity;
                RetainmentMinRate = source.RetainmentMinRate;
                RetainmentMaxRate = source.RetainmentMaxRate;
                InternalWeightScale = source.InternalWeightScale;
                AugmentedStatesFeature = source.AugmentedStatesFeature;
                switch (Topology)
                {
                    case EnumReservoirTopology.Random:
                        RandomTopologyCfg = new RandomTopologyConfig(source.RandomTopologyCfg);
                        break;
                    case EnumReservoirTopology.Ring:
                        RingTopologyCfg = new RingTopologyConfig(source.RingTopologyCfg);
                        break;
                    case EnumReservoirTopology.DTT:
                        DTTTopologyCfg = new DTTTopologyConfig(source.DTTTopologyCfg);
                        break;
                }
                return;
            }

            public ReservoirConfig(XmlNode xmlNode)
            {
                CfgName = xmlNode.Attributes["CfgName"].Value;
                DelimitedStringValues inpFields = new DelimitedStringValues();
                inpFields.LoadFromString(xmlNode.Attributes["ApplyToInputFields"].Value);
                Topology = ParseReservoirTopology(xmlNode.Attributes["Topology"].Value);
                Size = int.Parse(xmlNode.Attributes["Size"].Value);
                BiasScale = double.Parse(xmlNode.Attributes["BiasScale"].Value);
                InputConnectionDensity = double.Parse(xmlNode.Attributes["InputConnectionDensity"].Value);
                InputWeightScale = double.Parse(xmlNode.Attributes["InputWeightScale"].Value);
                ContextNeuronFeedbackDensity = double.Parse(xmlNode.Attributes["ContextNeuronFeedbackDensity"].Value);
                FeedbackConnectionDensity = double.Parse(xmlNode.Attributes["FeedbackConnectionDensity"].Value);
                FeedbackWeightScale = double.Parse(xmlNode.Attributes["FeedbackWeightScale"].Value);
                ReservoirNeuronActivation = ActivationFactory.ParseActivation(xmlNode.Attributes["ReservoirNeuronActivation"].Value);
                RetainmentNeuronsDensity = double.Parse(xmlNode.Attributes["RetainmentNeuronsDensity"].Value);
                RetainmentMinRate = double.Parse(xmlNode.Attributes["RetainmentMinRate"].Value);
                RetainmentMaxRate = double.Parse(xmlNode.Attributes["RetainmentMaxRate"].Value);
                InternalWeightScale = double.Parse(xmlNode.Attributes["InternalWeightScale"].Value);
                AugmentedStatesFeature = bool.Parse(xmlNode.Attributes["AugmentedStatesFeature"].Value);
                RandomTopologyCfg = null;
                RingTopologyCfg = null;
                DTTTopologyCfg = null;
                switch (Topology)
                {
                    case EnumReservoirTopology.Random:
                        RandomTopologyCfg = new RandomTopologyConfig(xmlNode.SelectSingleNode("RandomTopologyConfig"));
                        break;
                    case EnumReservoirTopology.Ring:
                        RingTopologyCfg = new RingTopologyConfig(xmlNode.SelectSingleNode("RingTopologyConfig"));
                        break;
                    case EnumReservoirTopology.DTT:
                        DTTTopologyCfg = new DTTTopologyConfig(xmlNode.SelectSingleNode("DTTTopologyConfig"));
                        break;
                }
                return;
            }

            //Methods
            /// <summary>Checkes if this settings are equivalent to specified settings</summary>
            /// <param name="cmpSettings">Settings to be compared with this settings</param>
            public bool IsEquivalent(ReservoirConfig cmpSettings)
            {
                if (CfgName != cmpSettings.CfgName ||
                    Topology != cmpSettings.Topology ||
                    Size != cmpSettings.Size ||
                    BiasScale != cmpSettings.BiasScale ||
                    InputConnectionDensity != cmpSettings.InputConnectionDensity ||
                    InputWeightScale != cmpSettings.InputWeightScale ||
                    ContextNeuronFeedbackDensity != cmpSettings.ContextNeuronFeedbackDensity ||
                    FeedbackConnectionDensity != cmpSettings.FeedbackConnectionDensity ||
                    FeedbackWeightScale != cmpSettings.FeedbackWeightScale ||
                    ReservoirNeuronActivation != cmpSettings.ReservoirNeuronActivation ||
                    RetainmentNeuronsDensity != cmpSettings.RetainmentNeuronsDensity ||
                    RetainmentMinRate != cmpSettings.RetainmentMinRate ||
                    RetainmentMaxRate != cmpSettings.RetainmentMaxRate ||
                    InternalWeightScale != cmpSettings.InternalWeightScale ||
                    AugmentedStatesFeature != cmpSettings.AugmentedStatesFeature
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
                    default: return EnumReservoirTopology.Ring;
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
                    ConnectionsDensity = double.Parse(xmlNode.Attributes["ConnectionsDensity"].Value);
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
                    SelfConnectionsDensity = double.Parse(xmlNode.Attributes["SelfConnectionsDensity"].Value);
                    InterConnectionsDensity = double.Parse(xmlNode.Attributes["InterConnectionsDensity"].Value);
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
                    SelfConnectionsDensity = double.Parse(xmlNode.Attributes["SelfConnectionsDensity"].Value);
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




    }//Reservoir
}//Namespace
