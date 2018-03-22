using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using OKOSW.MathTools;
using OKOSW.Extensions;
using OKOSW.CsvTools;
using OKOSW.Neural.Activation;

namespace OKOSW.Neural.Reservoir.Analog
{
    /// <summary>
    /// Implements analog reservoir supporting several internal topologies and advanced features
    /// </summary>
    [Serializable]
    public class AnalogReservoir : IAnalogReservoir
    {
        //Attributes
        private string _seqNum;
        private string _configName;
        private Random _rand;
        private ReservoirInputBlock _inputBlock;
        private AnalogNeuron[] _neurons;
        private List<int>[] _partyNeuronsIdxs;
        private List<double>[] _partyNeuronsWeights;
        private bool _contextNeuronFeature;
        private AnalogNeuron _contextNeuron;
        private double[] _neurons2ContextWeights;
        private double[] _context2NeuronsWeights;
        private bool _feedbackFeature;
        private double[] _feedback;
        private double[] _feedbackWeights;
        private bool _augmentedStatesFeature;

        /// <summary>
        /// Constructs analog computing reservoir.
        /// </summary>
        /// <param name="seqNum">Reservoir sequence identifier (together with reservoir configuration name should be unique within the parent)</param>
        /// <param name="inputValuesCount">Number of reservoir input values</param>
        /// <param name="feedbackValuesCount">Number of values to be fed back</param>
        /// <param name="settings">Reservoir initialization parameters</param>
        /// <param name="randomizerSeek">
        /// Calling constructor with the same randomizerSeek greater or equal to 0 ensures the same reservoir initialization (good for tuning).
        /// Specify randomizerSeek less than 0 for different initialization aech time the constructor will be called.
        /// </param>
        public AnalogReservoir(int seqNum, int inputValuesCount, int feedbackValuesCount, AnalogReservoirSettings settings, int randomizerSeek = -1)
        {
            //--------------------------------------------------------
            //Configuration name
            _configName = settings.CfgName;
            //Reservoir ID
            _seqNum = _configName + "(" + seqNum.ToString() + ")";
            //--------------------------------------------------------
            //Random object initialization
            if (randomizerSeek < 0) _rand = new Random();
            else _rand = new Random(randomizerSeek);
            //--------------------------------------------------------
            //Input memory and connections
            int neuronsPerInput = Math.Max(1, (int)Math.Round((double)settings.Size * settings.InputConnectionDensity, 0));
            _inputBlock = new ReservoirInputBlock(inputValuesCount, settings.Size, settings.BiasScale, settings.InputWeightScale, neuronsPerInput, _rand);
            //--------------------------------------------------------
            //Reservoir neurons
            _neurons = new AnalogNeuron[settings.Size];
            //Neurons retainment rates
            double[] retainmentRates = new double[_neurons.Length];
            retainmentRates.Populate(0);
            int retainmentNeuronsCount = (int)Math.Round((double)_neurons.Length * settings.RetainmentNeuronsDensity, 0);
            if (retainmentNeuronsCount > 0 && settings.RetainmentMaxRate > 0)
            {
                _rand.FillUniform(retainmentRates, settings.RetainmentMinRate, settings.RetainmentMaxRate, 1, retainmentNeuronsCount);
                _rand.Shuffle(retainmentRates);
            }
            //Neurons creation
            for (int n = 0; n < _neurons.Length; n++)
            {
                _neurons[n] = new AnalogNeuron(ActivationFactory.CreateAF(settings.ReservoirNeuronActivation), retainmentRates[n]);
            }
            //Helper array for neurons order randomization purposes
            int[] neuronsShuffledIndices = new int[settings.Size];
            //--------------------------------------------------------
            //Context neuron feature
            int contextNeuronFeedbacksCount = (int)Math.Round((double)_neurons.Length * settings.ContextNeuronFeedbackDensity, 0);
            _contextNeuron = null;
            _neurons2ContextWeights = null;
            _context2NeuronsWeights = null;
            _contextNeuronFeature = (contextNeuronFeedbacksCount > 0);
            if (_contextNeuronFeature)
            {
                _contextNeuron = new AnalogNeuron(ActivationFactory.CreateAF(settings.ContextNeuronActivation), 0);
                //Weights from each res neuron to context neuron
                _neurons2ContextWeights = new double[_neurons.Length];
                _rand.FillUniform(_neurons2ContextWeights, -1, 1, settings.ContextNeuronInWeightScale);
                //Weights from context neuron to res neurons
                _context2NeuronsWeights = new double[_neurons.Length];
                _context2NeuronsWeights.Populate(0);
                neuronsShuffledIndices.ShuffledIndices(_rand);
                for (int i = 0; i < contextNeuronFeedbacksCount && i < _neurons.Length; i++)
                {
                    _context2NeuronsWeights[neuronsShuffledIndices[i]] = RandomWeight(_rand, settings.ContextNeuronOutWeightScale);
                }
            }
            //--------------------------------------------------------
            //Feedback feature and weights
            int neuronsPerOutput = (int)Math.Round(settings.FeedbackConnectionDensity * (double)_neurons.Length, 0);
            _feedback = new double[feedbackValuesCount];
            _feedback.Populate(0);
            _feedbackWeights = null;
            _feedbackFeature = (neuronsPerOutput > 0);
            if (_feedbackFeature)
            {
                //Feedback weights
                _feedbackWeights = new double[feedbackValuesCount * _neurons.Length];
                _feedbackWeights.Populate(0);
                for (int outNo = 0; outNo < feedbackValuesCount; outNo++)
                {
                    neuronsShuffledIndices.ShuffledIndices(_rand);
                    for (int i = 0; i < neuronsPerOutput; i++)
                    {
                        _feedbackWeights[outNo * _neurons.Length + neuronsShuffledIndices[i]] = RandomWeight(_rand, settings.FeedbackWeightScale);
                    }
                }
            }
            //--------------------------------------------------------
            //Reservoir topology -> schema of internal connections
            _partyNeuronsIdxs = new List<int>[_neurons.Length];
            _partyNeuronsWeights = new List<double>[_neurons.Length];
            for (int i = 0; i < _neurons.Length; i++)
            {
                _partyNeuronsIdxs[i] = new List<int>();
                _partyNeuronsWeights[i] = new List<double>();
            }
            switch (settings.Topology)
            {
                case AnalogReservoirSettings.ReservoirTopologyType.Random:
                    SetupRandomTopology(settings.RandomTopologyCfg, settings.InternalWeightScale);
                    break;
                case AnalogReservoirSettings.ReservoirTopologyType.Ring:
                    SetupRingTopology(settings.RingTopologyCfg, settings.InternalWeightScale);
                    break;
                case AnalogReservoirSettings.ReservoirTopologyType.DTT:
                    SetupDTTTopology(settings.DTTTopologyCfg, settings.InternalWeightScale);
                    break;
            }
            //--------------------------------------------------------
            //Augmented states
            _augmentedStatesFeature = settings.AugmentedStatesFeature;
            return;
        }

        /// <summary>
        /// Returns random weight within range -scale, +scale
        /// </summary>
        /// <param name="rand">Random object to be used.</param>
        /// <param name="scale">Determines the range within the weight has to be.</param>
        /// <returns></returns>
        public static double RandomWeight(Random rand, double scale)
        {
            return rand.NextBoundedUniformDouble(-1, 1) * scale;
        }

        /// <summary>
        /// Establishes connection between two reservoir neurons.
        /// </summary>
        /// <param name="targetNeuronIdx">Target neuron index</param>
        /// <param name="partyNeuronIdx">Party neuron index</param>
        /// <param name="weightScale">Connection weight scale</param>
        /// <param name="check">Check if the connection already exists?</param>
        /// <returns>Success/Unsuccess (connection already exists)</returns>
        private bool AddConnection(int targetNeuronIdx, int partyNeuronIdx, double weightScale, bool check = true)
        {
            if (!check || !_partyNeuronsIdxs[targetNeuronIdx].Contains(partyNeuronIdx))
            {
                _partyNeuronsIdxs[targetNeuronIdx].Add(partyNeuronIdx);
                _partyNeuronsWeights[targetNeuronIdx].Add(RandomWeight(_rand, weightScale));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Establishes connection between two reservoir neurons.
        /// </summary>
        /// <param name="connectionID">Position of the connection within flat representation.</param>
        /// <param name="weightScale">Connection weight scale</param>
        /// <param name="check">Check if the connection already exists?</param>
        /// <returns>Success/Unsuccess (connection already exists)</returns>
        private bool AddConnection(int connectionID, double weightScale, bool check = true)
        {
            return AddConnection(connectionID / _neurons.Length, connectionID % _neurons.Length, weightScale, check);
        }

        /// <summary>
        /// Connects all reservoir neurons to a ring shape.
        /// </summary>
        /// <param name="weightScale">Connection weight scale</param>
        /// <param name="biDirection">Bi direction ring?</param>
        /// <param name="check">Check if the connection already exists?</param>
        private void SetRingConnections(double weightScale, bool biDirection, bool check = true)
        {
            for (int i = 0; i < _neurons.Length; i++)
            {
                int partyNeuronIdx = (i == 0) ? (_neurons.Length - 1) : (i - 1);
                AddConnection(i, partyNeuronIdx, weightScale, check);
                if(biDirection)
                {
                    partyNeuronIdx = (i == _neurons.Length - 1) ? (0) : (i + 1);
                    AddConnection(i, partyNeuronIdx, weightScale, check);
                }
            }
            return;
        }

        /// <summary>
        /// Sets randomly selected number of neurons (corresponding to density) to be self-connected
        /// </summary>
        /// <param name="density">How many neurons will be self-connected?</param>
        /// <param name="weightScale">Connection weight scale</param>
        /// <param name="check">Check if the connection already exists?</param>
        private void SetSelfConnections(double density, double weightScale, bool check = true)
        {
            int connectionsCount = (int)Math.Round((double)_neurons.Length * density);
            int[] indices = new int[_neurons.Length];
            indices.ShuffledIndices(_rand);
            for (int i = 0; i < connectionsCount; i++)
            {
                AddConnection(indices[i], indices[i], weightScale, check);
            }
            return;
        }

        /// <summary>
        /// Sets random neurons inter connections.
        /// </summary>
        /// <param name="density">How many connections from Size x Size options will be randomly initialized?</param>
        /// <param name="weightScale">Connection weight scale</param>
        /// <param name="check">Check if the connection already exists?</param>
        private void SetInterConnections(double density, double weightScale, bool check = true)
        {
            int connectionsCount = (int)Math.Round((double)((_neurons.Length - 1) * _neurons.Length) * density);
            int[] randomConnections = new int[(_neurons.Length - 1) * _neurons.Length];
            int indicesPos = 0;
            for(int n1Idx = 0; n1Idx < _neurons.Length; n1Idx++)
            {
                for(int n2Idx = 0; n2Idx < _neurons.Length; n2Idx++)
                {
                    if(n1Idx != n2Idx)
                    {
                        randomConnections[indicesPos] = n1Idx * _neurons.Length + n2Idx;
                        ++indicesPos;
                    }
                }
            }
            _rand.Shuffle(randomConnections);
            for (int i = 0; i < connectionsCount; i++)
            {
                AddConnection(randomConnections[i], weightScale, check);
            }
            return;
        }

        /// <summary>
        /// Initializes random topology connection schema
        /// </summary>
        /// <param name="cfg">Configuration parameters</param>
        /// <param name="weightScale">Connection weight scale</param>
        private void SetupRandomTopology(AnalogReservoirSettings.RandomTopologyConfig cfg, double weightScale)
        {
            //Fully random connections setup
            int connectionsCount = (int)Math.Round((double)_neurons.Length * (double)_neurons.Length * cfg.ConnectionsDensity);
            int[] randomConnections = new int[_neurons.Length * _neurons.Length];
            randomConnections.ShuffledIndices(_rand);
            for (int i = 0; i < connectionsCount; i++)
            {
                AddConnection(randomConnections[i], weightScale, false);
            }
            return;
        }

        /// <summary>
        /// Initializes ring shape topology connection schema
        /// </summary>
        /// <param name="cfg">Configuration parameters</param>
        /// <param name="weightScale">Connection weight scale</param>
        private void SetupRingTopology(AnalogReservoirSettings.RingTopologyConfig cfg, double weightScale)
        {
            //Ring connections part
            SetRingConnections(weightScale, cfg.BiDirection, false);
            //Self connections part
            SetSelfConnections(cfg.SelfConnectionsDensity, weightScale, false);
            //Inter connections part
            SetInterConnections(cfg.InterConnectionsDensity, weightScale, true);
            return;
        }

        /// <summary>
        /// Initializes doubly twisted thoroidal shape topology connection schema
        /// </summary>
        /// <param name="cfg">Configuration parameters</param>
        /// <param name="weightScale">Connection weight scale</param>
        private void SetupDTTTopology(AnalogReservoirSettings.DTTTopologyConfig cfg, double weightScale)
        {
            //HTwist part (single direction ring)
            SetRingConnections(weightScale, false);
            //VTwist part
            int step = (int)Math.Floor(Math.Sqrt(_neurons.Length));
            for (int partyNeuronIdx = 0; partyNeuronIdx < _neurons.Length; partyNeuronIdx++)
            {
                int targetNeuronIdx = partyNeuronIdx + step;
                if (targetNeuronIdx > _neurons.Length - 1)
                {
                    int left = partyNeuronIdx % step;
                    targetNeuronIdx = (left == 0) ? (step - 1) : (left - 1);
                }
                AddConnection(targetNeuronIdx, partyNeuronIdx, weightScale, false);
            }
            //Self connections part
            SetSelfConnections(cfg.SelfConnectionsDensity, weightScale, false);
            return;
        }


        #region IReservoir implementation

        //Properties
        /// <summary>
        /// Reservoir ID.
        /// </summary>
        public string SeqNum { get { return _seqNum; } }

        /// <summary>
        /// Reservoir configuration name (together with ID should be unique).
        /// </summary>
        public string ConfigName { get { return _configName; } }

        /// <summary>
        /// Reservoir size. (Reservoir neurons count)
        /// </summary>
        public int Size { get { return _neurons.Length; } }

        /// <summary>
        /// Number of reservoir's output predictors (Size or Size*2 when augumented states are enabled).
        /// </summary>
        public int OutputPredictorsCount { get { return _augmentedStatesFeature ? _neurons.Length * 2 : _neurons.Length; } }

        /// <summary>
        /// Reservoir neurons.
        /// </summary>
        public AnalogNeuron[] Neurons { get { return _neurons; } }

        /// <summary>
        /// Context neuron.
        /// </summary>
        public AnalogNeuron ContextNeuron { get { return _contextNeuron; } }

        //Methods
        /// <summary>
        /// Resets all reservoir neurons to their initial state (before boot state).
        /// Function does not affect weights or internal structure of the resservoir.
        /// </summary>
        public void Reset()
        {
            foreach (AnalogNeuron neuron in _neurons)
            {
                neuron.Reset();
            }
            if(_contextNeuronFeature)_contextNeuron.Reset();
            _feedback.Populate(0);
            return;
        }

        /// <summary>
        /// Computes reservoir neurons new states and returns new set of reservoir output predictors.
        /// </summary>
        /// <param name="input">Array of new input values.</param>
        /// <param name="outputPredictors">Array to be filled with output predictors values. Array has to be sized to OutputPredictorsCount reservoir property.</param>
        /// <param name="collectStatistics">Switch dictates, if to collect statistics. Typical usage is FALSE within boot phase and TRUE after boot phase.</param>
        public void Compute(double[] input, double[] outputPredictors, bool collectStatistics)
        {
            //Update input memory
            _inputBlock.Update(input);
            //Store all reservoir neurons states
            foreach(AnalogNeuron neuron in _neurons)
            {
                neuron.StoreCurrentState();
            }
            //Compute new states of all reservoir neurons and fill output array of predictors
            Parallel.For(0, _neurons.Length, (neuronIdx) =>
            {
                //----------------------------------------------------
                //Input signal
                double inputSignal = _inputBlock.GetInputSignal(neuronIdx);
                //----------------------------------------------------
                //Signal from reservoir neurons
                double reservoirSignal = 0;
                //Add reservoir neurons signal
                for (int j = 0; j < _partyNeuronsIdxs[neuronIdx].Count; j++)
                {
                    reservoirSignal += _partyNeuronsWeights[neuronIdx][j] * _neurons[_partyNeuronsIdxs[neuronIdx][j]].PreviousState;
                }
                //Add context neuron signal if allowed
                reservoirSignal += _contextNeuronFeature ? _context2NeuronsWeights[neuronIdx] * _contextNeuron.CurrentState : 0;
                //----------------------------------------------------
                //Feedback signal
                double feedbackSignal = 0;
                if (_feedbackFeature)
                {
                    for (int outpIdx = 0; outpIdx < _feedback.Length; outpIdx++)
                    {
                        feedbackSignal += _feedback[outpIdx] * _feedbackWeights[outpIdx * _neurons.Length + neuronIdx];
                    }
                }
                //----------------------------------------------------
                //Set new state of reservoir neuron
                _neurons[neuronIdx].NewState(inputSignal + reservoirSignal + feedbackSignal, collectStatistics);
                //----------------------------------------------------
                //Set neuron state to output predictors
                outputPredictors[neuronIdx] = _neurons[neuronIdx].CurrentState;
                //----------------------------------------------------
                //Set neuron augmented state to output predictors
                if (_augmentedStatesFeature)
                {
                    outputPredictors[_neurons.Length + neuronIdx] = outputPredictors[neuronIdx] * outputPredictors[neuronIdx];
                }
            });
            //----------------------------------------------------
            //New state of context neuron if allowed
            if (_contextNeuronFeature)
            {
                double res2ContextSignal = 0;
                for (int neuronIdx = 0; neuronIdx < _neurons.Length; neuronIdx++)
                {
                    res2ContextSignal += _neurons2ContextWeights[neuronIdx] * _neurons[neuronIdx].CurrentState;
                }
                _contextNeuron.NewState(res2ContextSignal, collectStatistics);
            }
            return;
        }

        /// <summary>
        /// Sets feedback values for next computation round
        /// </summary>
        /// <param name="feedback">Feedback values.</param>
        public void SetFeedback(double[] feedback)
        {
            feedback.CopyTo(_feedback, 0);
            return;
        }


        #endregion

        //Inner classes
        /// <summary>
        /// Implements input component of the analog reservoir
        /// </summary>
        [Serializable]
        private class ReservoirInputBlock
        {
            //Constants
            //Attributes
            private int _inputValuesCount;
            private double[] _inputBiases;
            private double[] _inputValues;
            private int _reservoirNeuronsCount;
            private List<InputConnection>[] _connections;

            //Constructor
            public ReservoirInputBlock(int inputValuesCount,
                                       int reservoirNeuronsCount,
                                       double biasScale,
                                       double inputWeightScale,
                                       int neuronsPerInput,
                                       Random rand
                                       )
            {
                _inputValuesCount = inputValuesCount;
                _reservoirNeuronsCount = reservoirNeuronsCount;
                //Input biases
                _inputBiases = new double[_reservoirNeuronsCount];
                rand.FillUniform(_inputBiases, -1, 1, biasScale);
                //Input values
                _inputValues = new double[_inputValuesCount];
                _inputValues.Populate(0);
                //Connections to reservoir neurons
                _connections = new List<InputConnection>[_reservoirNeuronsCount];
                for (int i = 0; i < _reservoirNeuronsCount; i++)
                {
                    _connections[i] = new List<InputConnection>(_inputValuesCount * neuronsPerInput);
                }
                int[] neuronIdxs = new int[_reservoirNeuronsCount];
                for (int fieldIdx = 0; fieldIdx < _inputValuesCount; fieldIdx++)
                {
                    neuronIdxs.ShuffledIndices(rand);
                    for (int i = 0; i < neuronsPerInput; i++)
                    {
                        InputConnection connection = new InputConnection();
                        connection.FieldIdx = fieldIdx;
                        connection.Weight = AnalogReservoir.RandomWeight(rand, inputWeightScale);
                        _connections[neuronIdxs[i]].Add(connection);
                    }
                }
                return;
            }
            //Properties
            public int InputValuesCount { get { return _inputValuesCount; } }

            //Methods
            public void Update(double[] newInputValues)
            {
                newInputValues.CopyTo(_inputValues, 0);
                return;
            }

            /// <summary>
            /// Computes input signal from input fields to be processed by specified neuron
            /// </summary>
            /// <param name="reservoirNeuronIdx">Reservoir neuron index</param>
            public double GetInputSignal(int reservoirNeuronIdx)
            {
                double signal = 0;
                if (_connections[reservoirNeuronIdx].Count > 0)
                {
                    signal += _inputBiases[reservoirNeuronIdx];
                    foreach (InputConnection connection in _connections[reservoirNeuronIdx])
                    {
                        signal += _inputValues[connection.FieldIdx] * connection.Weight;
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

    }//AnalogReservoir
}//Namespace
