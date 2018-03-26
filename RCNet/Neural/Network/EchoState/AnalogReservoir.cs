using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RCNet.MathTools;
using RCNet.Extensions;
using RCNet.Neural.Activation;

namespace RCNet.Neural.Network.EchoState
{
    /// <summary>
    /// Implements analog reservoir supporting several internal topologies and advanced features.
    /// </summary>
    [Serializable]
    public class AnalogReservoir
    {
        //Attributes
        /// <summary>
        /// Name of this instance
        /// </summary>
        private string _instanceName;
        /// <summary>
        /// Reservoir's settings.
        /// </summary>
        private AnalogReservoirSettings _settings;
        /// <summary>
        /// Random generator.
        /// </summary>
        private Random _rand;
        /// <summary>
        /// The component processing input data.
        /// </summary>
        private ReservoirInputComponent _inputComponent;
        /// <summary>
        /// Reservoir's neurons.
        /// </summary>
        private AnalogNeuron[] _neurons;
        /// <summary>
        /// Indexes of the interconnected neurons.
        /// </summary>
        private List<int>[] _partyNeuronsIdxs;
        /// <summary>
        /// Weights of the interconnections.
        /// </summary>
        private List<double>[] _partyNeuronsWeights;
        /// <summary>
        /// The context neuron
        /// </summary>
        private AnalogNeuron _contextNeuron;
        /// <summary>
        /// Context neuron's in weights
        /// </summary>
        private double[] _neurons2ContextWeights;
        /// <summary>
        /// Context neuron's out weights
        /// </summary>
        private double[] _context2NeuronsWeights;
        /// <summary>
        /// Feedback values.
        /// </summary>
        private double[] _feedback;
        /// <summary>
        /// Feedback weights.
        /// </summary>
        private double[] _feedbackWeights;
        /// <summary>
        /// Produce augmented states?
        /// </summary>
        private bool _augmentedStatesFeature;

        /// <summary>
        /// Instantiates the analog reservoir
        /// </summary>
        /// <param name="numOfInputValues">Number of reservoir's input values</param>
        /// <param name="settings">Analog reservoir settings</param>
        /// <param name="augmentedStates">Specifies if this reservoir will add augmented states to predictors</param>
        /// <param name="randomizerSeek">
        /// A value greater than or equal to 0 will always ensure the same initialization of the internal
        /// random number generator and therefore the same reservoir structure, which is good for tuning
        /// other reservoir's parameters.
        /// A value less than 0 causes a fully random initialization when creating a reservoir instance.
        /// </param>
        public AnalogReservoir(string instanceName, int numOfInputValues, AnalogReservoirSettings settings, bool augmentedStates, int randomizerSeek = -1)
        {
            _instanceName = instanceName;
            //Settings
            _settings = settings.DeepClone();
            //Random generator initialization
            if (randomizerSeek < 0) _rand = new Random();
            else _rand = new Random(randomizerSeek);
            //Input component and connections
            int neuronsPerInput = Math.Max(1, (int)Math.Round((double)_settings.Size * _settings.InputConnectionDensity, 0));
            _inputComponent = new ReservoirInputComponent(numOfInputValues, _settings.Size, _settings.BiasScale, _settings.InputWeightScale, neuronsPerInput, _rand);
            //Reservoir neurons
            _neurons = new AnalogNeuron[_settings.Size];
            //Neurons retainment rates
            double[] retainmentRates = new double[_neurons.Length];
            retainmentRates.Populate(0);
            if (_settings.RetainmentNeuronsFeature)
            {
                int retainmentNeuronsCount = (int)Math.Round((double)_neurons.Length * _settings.RetainmentNeuronsDensity, 0);
                if (retainmentNeuronsCount > 0 && _settings.RetainmentMaxRate > 0)
                {
                    _rand.FillUniform(retainmentRates, _settings.RetainmentMinRate, _settings.RetainmentMaxRate, 1, retainmentNeuronsCount);
                    _rand.Shuffle(retainmentRates);
                }
            }
            //Neurons creation
            for (int n = 0; n < _neurons.Length; n++)
            {
                _neurons[n] = new AnalogNeuron(ActivationFactory.CreateActivationFunction(_settings.ReservoirNeuronActivation), retainmentRates[n]);
            }
            //Helper array for neurons order randomization purposes
            int[] neuronsShuffledIndices = new int[_settings.Size];
            //Context neuron feature
            _contextNeuron = null;
            _neurons2ContextWeights = null;
            _context2NeuronsWeights = null;
            if (_settings.ContextNeuronFeature)
            {
                int contextNeuronFeedbacksCount = (int)Math.Round((double)_neurons.Length * _settings.ContextNeuronFeedbackDensity, 0);
                _contextNeuron = new AnalogNeuron(ActivationFactory.CreateActivationFunction(_settings.ContextNeuronActivation), 0);
                //Weights from each res neuron to context neuron
                _neurons2ContextWeights = new double[_neurons.Length];
                _rand.FillUniform(_neurons2ContextWeights, -1, 1, _settings.ContextNeuronInWeightScale);
                //Weights from context neuron to res neurons
                _context2NeuronsWeights = new double[_neurons.Length];
                _context2NeuronsWeights.Populate(0);
                neuronsShuffledIndices.ShuffledIndices(_rand);
                for (int i = 0; i < contextNeuronFeedbacksCount && i < _neurons.Length; i++)
                {
                    _context2NeuronsWeights[neuronsShuffledIndices[i]] = RandomWeight(_rand, _settings.ContextNeuronOutWeightScale);
                }
            }
            //Feedback feature and weights
            _feedback = null;
            _feedbackWeights = null;
            if (_settings.FeedbackFeature)
            {
                int neuronsPerOutput = (int)Math.Round(_settings.FeedbackConnectionDensity * (double)_neurons.Length, 0);
                //Feedback values
                _feedback = new double[_settings.FeedbackFieldNameCollection.Count];
                _feedback.Populate(0);
                //Feedback weights
                _feedbackWeights = new double[_feedback.Length * _neurons.Length];
                _feedbackWeights.Populate(0);
                for (int outNo = 0; outNo < _feedback.Length; outNo++)
                {
                    neuronsShuffledIndices.ShuffledIndices(_rand);
                    for (int i = 0; i < neuronsPerOutput; i++)
                    {
                        _feedbackWeights[outNo * _neurons.Length + neuronsShuffledIndices[i]] = RandomWeight(_rand, _settings.FeedbackWeightScale);
                    }
                }
            }
            //Reservoir topology, the schema of interconnections
            _partyNeuronsIdxs = new List<int>[_neurons.Length];
            _partyNeuronsWeights = new List<double>[_neurons.Length];
            for (int i = 0; i < _neurons.Length; i++)
            {
                _partyNeuronsIdxs[i] = new List<int>();
                _partyNeuronsWeights[i] = new List<double>();
            }
            switch (_settings.TopologyType)
            {
                case AnalogReservoirSettings.ReservoirTopologyType.Random:
                    SetupRandomTopology((AnalogReservoirSettings.RandomTopology)(_settings.TopologySettings), _settings.InternalWeightScale);
                    break;
                case AnalogReservoirSettings.ReservoirTopologyType.Ring:
                    SetupRingTopology((AnalogReservoirSettings.RingTopology)(_settings.TopologySettings), _settings.InternalWeightScale);
                    break;
                case AnalogReservoirSettings.ReservoirTopologyType.DTT:
                    SetupDTTTopology((AnalogReservoirSettings.DTTTopology)(_settings.TopologySettings), _settings.InternalWeightScale);
                    break;
            }
            //Augmented states
            _augmentedStatesFeature = augmentedStates;
            return;
        }

        //Properties
        /// <summary>
        /// Reservoir size. (Number of neurons in the reservoir)
        /// </summary>
        public int Size { get { return _neurons.Length; } }

        /// <summary>
        /// Number of reservoir's output predictors (Size or Size*2 when augumented states are enabled).
        /// </summary>
        public int NumOfOutputPredictors { get { return _augmentedStatesFeature ? _neurons.Length * 2 : _neurons.Length; } }

        //Methods
        /// <summary>
        /// Returns random weight within the interval (-scale, +scale)
        /// </summary>
        /// <param name="rand">Random object to be used.</param>
        /// <param name="scale">Weight scale.</param>
        /// <returns></returns>
        public static double RandomWeight(Random rand, double scale)
        {
            return rand.NextBoundedUniformDouble(-1, 1) * scale;
        }

        /// <summary>
        /// Establishes the interconnection of reservoir neurons.
        /// </summary>
        /// <param name="targetNeuronIdx">Target neuron index</param>
        /// <param name="partyNeuronIdx">Party neuron index</param>
        /// <param name="weightScale">Scale of the connection weight</param>
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
        /// Establishes the interconnection of reservoir neurons.
        /// </summary>
        /// <param name="connectionID">Position of the connection within flat representation (Size * Size).</param>
        /// <param name="weightScale">Scale of the connection weight</param>
        /// <param name="check">Check if the connection already exists?</param>
        /// <returns>Success/Unsuccess (connection already exists)</returns>
        private bool AddConnection(int connectionID, double weightScale, bool check = true)
        {
            return AddConnection(connectionID / _neurons.Length, connectionID % _neurons.Length, weightScale, check);
        }

        /// <summary>
        /// Connects all reservoir neurons to a ring shape.
        /// </summary>
        /// <param name="weightScale">Scale of the connection weight</param>
        /// <param name="biDirection">Bidirectional ring?</param>
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
        /// <param name="weightScale">Scale of the connection weight</param>
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
        /// <param name="weightScale">Scale of the connection weight</param>
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
        /// <param name="weightScale">Scale of the connection weight</param>
        private void SetupRandomTopology(AnalogReservoirSettings.RandomTopology cfg, double weightScale)
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
        /// <param name="weightScale">Scale of the connection weight</param>
        private void SetupRingTopology(AnalogReservoirSettings.RingTopology cfg, double weightScale)
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
        /// <param name="weightScale">Scale of the connection weight</param>
        private void SetupDTTTopology(AnalogReservoirSettings.DTTTopology cfg, double weightScale)
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

        /// <summary>
        /// Collects neurons states key statistics
        /// </summary>
        public AnalogReservoirStat CollectStateStatistics()
        {
            AnalogReservoirStat stats = new AnalogReservoirStat(_instanceName);
            foreach (AnalogNeuron neuron in _neurons)
            {
                stats.NeuronsMaxAbsStatesStat.AddSampleValue(Math.Max(Math.Abs(neuron.StatesStat.Max), Math.Abs(neuron.StatesStat.Min)));
                stats.NeuronsRMSStatesStat.AddSampleValue(neuron.StatesStat.RootMeanSquare);
                stats.NeuronsStateSpansStat.AddSampleValue(neuron.StatesStat.Span);
            }
            if (_settings.ContextNeuronFeature)
            {
                stats.CtxNeuronStatesRMS = _contextNeuron.StatesStat.RootMeanSquare;
            }
            else
            {
                stats.CtxNeuronStatesRMS = -1;
            }
            return stats;
        }

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
            if (_settings.ContextNeuronFeature)
            {
                _contextNeuron.Reset();
            }
            if (_settings.FeedbackFeature)
            {
                _feedback.Populate(0);
            }
            return;
        }

        /// <summary>
        /// Computes reservoir neurons states and fills given array of reservoir output predictors.
        /// </summary>
        /// <param name="input">
        /// Array of input values.
        /// </param>
        /// <param name="outputPredictors">
        /// Array to be filled with predictors values.
        /// Array has to be sized to the value of NumOfOutputPredictors reservoir's property.
        /// </param>
        /// <param name="collectStatistics">
        /// Switch says, if to collect statistics.
        /// Specify false within booting phase and true after booting phase.
        /// </param>
        public void Compute(double[] input, double[] outputPredictors, bool collectStatistics)
        {
            //Update input memory
            _inputComponent.Update(input);
            //Store all reservoir neurons states
            foreach(AnalogNeuron neuron in _neurons)
            {
                neuron.StoreCurrentState();
            }
            //Compute new states of all reservoir neurons and fill the array of output predictors
            Parallel.For(0, _neurons.Length, (neuronIdx) =>
            {
                //Input signal
                double inputSignal = _inputComponent.GetInputSignal(neuronIdx);
                //Signal from reservoir neurons
                double reservoirSignal = 0;
                //Add reservoir neurons signal
                for (int j = 0; j < _partyNeuronsIdxs[neuronIdx].Count; j++)
                {
                    reservoirSignal += _partyNeuronsWeights[neuronIdx][j] * _neurons[_partyNeuronsIdxs[neuronIdx][j]].PreviousState;
                }
                //Add context neuron signal if allowed
                reservoirSignal += _settings.ContextNeuronFeature ? _context2NeuronsWeights[neuronIdx] * _contextNeuron.CurrentState : 0;
                //Feedback signal
                double feedbackSignal = 0;
                if (_settings.FeedbackFeature)
                {
                    for (int outpIdx = 0; outpIdx < _feedback.Length; outpIdx++)
                    {
                        feedbackSignal += _feedback[outpIdx] * _feedbackWeights[outpIdx * _neurons.Length + neuronIdx];
                    }
                }
                //Set new state of reservoir neuron
                _neurons[neuronIdx].Compute(inputSignal + reservoirSignal + feedbackSignal, collectStatistics);
                //Set the neuron's state into the array of output predictors
                outputPredictors[neuronIdx] = _neurons[neuronIdx].CurrentState;
                //Set the neuron's augmented state into the array of output predictors
                if (_augmentedStatesFeature)
                {
                    outputPredictors[_neurons.Length + neuronIdx] = outputPredictors[neuronIdx].Power(2);
                }
            });
            //Compute context neuron state (if allowed)
            if (_settings.ContextNeuronFeature)
            {
                double res2ContextSignal = 0;
                for (int neuronIdx = 0; neuronIdx < _neurons.Length; neuronIdx++)
                {
                    res2ContextSignal += _neurons2ContextWeights[neuronIdx] * _neurons[neuronIdx].CurrentState;
                }
                _contextNeuron.Compute(res2ContextSignal, collectStatistics);
            }
            return;
        }

        /// <summary>
        /// Sets feedback values for the next Compute calling
        /// </summary>
        /// <param name="feedback">Feedback values.</param>
        public void SetFeedback(double[] feedback)
        {
            if (_settings.FeedbackFeature)
            {
                feedback.CopyTo(_feedback, 0);
            }
            return;
        }

        //Inner classes
        /// <summary>
        /// Implements input component of the analog reservoir
        /// </summary>
        [Serializable]
        private class ReservoirInputComponent
        {
            //Constants
            //Attributes
            private int _numOfInputValues;
            private double[] _inputBiases;
            private double[] _inputValues;
            private int _reservoirNeuronsCount;
            private List<InputConnection>[] _inputConnectionCollection;

            //Constructor
            public ReservoirInputComponent(int numOfInputValues,
                                           int reservoirNeuronsCount,
                                           double biasScale,
                                           double inputWeightScale,
                                           int neuronsPerInput,
                                           Random rand
                                           )
            {
                _numOfInputValues = numOfInputValues;
                _reservoirNeuronsCount = reservoirNeuronsCount;
                //Input biases
                _inputBiases = new double[_reservoirNeuronsCount];
                rand.FillUniform(_inputBiases, -1, 1, biasScale);
                //Input values
                _inputValues = new double[_numOfInputValues];
                _inputValues.Populate(0);
                //Connections to reservoir neurons
                _inputConnectionCollection = new List<InputConnection>[_reservoirNeuronsCount];
                for (int i = 0; i < _reservoirNeuronsCount; i++)
                {
                    _inputConnectionCollection[i] = new List<InputConnection>(_numOfInputValues * neuronsPerInput);
                }
                int[] neuronIdxs = new int[_reservoirNeuronsCount];
                for (int fieldIdx = 0; fieldIdx < _numOfInputValues; fieldIdx++)
                {
                    neuronIdxs.ShuffledIndices(rand);
                    for (int i = 0; i < neuronsPerInput; i++)
                    {
                        InputConnection connection = new InputConnection();
                        connection.InputValueIdx = fieldIdx;
                        connection.Weight = AnalogReservoir.RandomWeight(rand, inputWeightScale);
                        _inputConnectionCollection[neuronIdxs[i]].Add(connection);
                    }
                }
                return;
            }

            //Properties
            public int NumOfInputValues { get { return _numOfInputValues; } }

            //Methods
            public void Update(double[] newInputValues)
            {
                newInputValues.CopyTo(_inputValues, 0);
                return;
            }

            /// <summary>
            /// Computes input signal from input to be processed by specified neuron
            /// </summary>
            /// <param name="reservoirNeuronIdx">Index of the reservoir's neuron</param>
            public double GetInputSignal(int reservoirNeuronIdx)
            {
                double signal = 0;
                if (_inputConnectionCollection[reservoirNeuronIdx].Count > 0)
                {
                    signal += _inputBiases[reservoirNeuronIdx];
                    foreach (InputConnection connection in _inputConnectionCollection[reservoirNeuronIdx])
                    {
                        signal += _inputValues[connection.InputValueIdx] * connection.Weight;
                    }
                }
                return signal;
            }

            //Inner classes
            [Serializable]
            private class InputConnection
            {
                public int InputValueIdx { get; set; } = 0;
                public double Weight { get; set; } = 0;
            }//InputConnection

        }//ReservoirInputComponent

    }//AnalogReservoir

    /// <summary>
    /// Reservoir's key statistics
    /// </summary>
    [Serializable]
    public class AnalogReservoirStat
    {
        //Attributes
        public string ReservoirInstanceName { get; }
        public BasicStat NeuronsMaxAbsStatesStat { get; }
        public BasicStat NeuronsRMSStatesStat { get; }
        public BasicStat NeuronsStateSpansStat { get; }
        public double CtxNeuronStatesRMS { get; set; }

        //Constructor
        public AnalogReservoirStat(string reservoirInstanceName)
        {
            ReservoirInstanceName = reservoirInstanceName;
            NeuronsMaxAbsStatesStat = new BasicStat();
            NeuronsRMSStatesStat = new BasicStat();
            NeuronsStateSpansStat = new BasicStat();
            CtxNeuronStatesRMS = 0;
            return;
        }

    }//AnalogReservoirStat



}//Namespace
