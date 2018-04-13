using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using RCNet.MathTools;
using RCNet.MathTools.MatrixMath;
using RCNet.Extensions;
using RCNet.Neural.Activation;
using RCNet.Neural.Weight;

namespace RCNet.Neural.Network.ReservoirComputing.EchoState
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
        /// Reservoir's neurons.
        /// </summary>
        private AnalogNeuron[] _neurons;
        /// <summary>
        /// Number of reservoir inputs
        /// </summary>
        private int _numOfInputNodes;
        /// <summary>
        /// A list of input connections for each neuron
        /// </summary>
        private List<Connection>[] _neuronInputConnectionsCollection;
        /// <summary>
        /// A list of neuron connections for each neuron
        /// </summary>
        private List<Connection>[] _neuronNeuronConnectionsCollection;
        /// <summary>
        /// Feedback values.
        /// </summary>
        private double[] _feedback;
        /// <summary>
        /// A list of feedback connections for each neuron
        /// </summary>
        private List<Connection>[] _neuronFeedbackConnectionsCollection;
        /// <summary>
        /// The context neuron
        /// </summary>
        private AnalogNeuron _contextNeuron;
        /// <summary>
        /// Input weights into the context neuron 
        /// </summary>
        private double[] _contextNeuronInputWeights;
        /// <summary>
        /// Context neuron feedback weights
        /// </summary>
        private double[] _contextNeuronFeedbackWeights;
        /// <summary>
        /// Specifies whether to produce augmented states
        /// </summary>
        private bool _augmentedStatesFeature;

        /// <summary>
        /// Instantiates the analog reservoir
        /// </summary>
        /// <param name="instanceName">The name of the reservoir instance</param>
        /// <param name="numOfInputNodes">Number of reservoir inputs</param>
        /// <param name="settings">Reservoir settings</param>
        /// <param name="augmentedStates">Specifies whether this reservoir will add augmented states to output predictors</param>
        /// <param name="randomizerSeek">
        /// A value greater than or equal to 0 will always ensure the same initialization of the internal
        /// random number generator and therefore the same reservoir structure, which is good for tuning purposes.
        /// A value less than 0 causes a fully random initialization each time creating a reservoir instance.
        /// </param>
        public AnalogReservoir(string instanceName, int numOfInputNodes, AnalogReservoirSettings settings, bool augmentedStates, int randomizerSeek = -1)
        {
            _instanceName = instanceName;
            _numOfInputNodes = numOfInputNodes;
            _settings = settings.DeepClone();
            //Allocations for the mapping of the connections
            _neuronInputConnectionsCollection = new List<Connection>[_settings.Size];
            _neuronNeuronConnectionsCollection = new List<Connection>[_settings.Size];
            _neuronFeedbackConnectionsCollection = new List<Connection>[_settings.Size];
            for(int n = 0; n < _settings.Size; n++)
            {
                _neuronInputConnectionsCollection[n] = new List<Connection>();
                _neuronNeuronConnectionsCollection[n] = new List<Connection>();
                _neuronFeedbackConnectionsCollection[n] = new List<Connection>();
            }
            //Random generator initialization
            if (randomizerSeek < 0) _rand = new Random();
            else _rand = new Random(randomizerSeek);
            //Allocation of reservoir neurons array and creation of the neurons
            _neurons = new AnalogNeuron[_settings.Size];
            //Neurons retainment rates
            double[] retainmentRates = new double[_neurons.Length];
            retainmentRates.Populate(0);
            if (_settings.RetainmentNeuronsFeature)
            {
                int numOfRetainmentNeurons = (int)Math.Round((double)_neurons.Length * _settings.RetainmentNeuronsDensity, 0);
                if (numOfRetainmentNeurons > 0 && _settings.RetainmentMaxRate > 0)
                {
                    _rand.Fill(retainmentRates, _settings.RetainmentMinRate, _settings.RetainmentMaxRate,false, RandomClassExtensions.DistributionType.Uniform, numOfRetainmentNeurons);
                    _rand.Shuffle(retainmentRates);
                }
            }
            //Neurons biases
            double[] biases = new double[_neurons.Length];
            _rand.Fill(biases, _settings.Bias.Min, _settings.Bias.Max, _settings.Bias.RandomSign, _settings.Bias.DistrType);
            //Neurons creation
            for (int n = 0; n < _neurons.Length; n++)
            {
                _neurons[n] = new AnalogNeuron(ActivationFactory.CreateActivationFunction(_settings.ReservoirNeuronActivation), biases[n], retainmentRates[n]);
            }
            //Input
            SetGuaranteedRandomInterconnections(_neuronInputConnectionsCollection, _numOfInputNodes, _settings.InputConnectionDensity, _settings.InputWeight);
            //Feedback
            _feedback = null;
            if (_settings.FeedbackFeature)
            {
                _feedback = new double[_settings.FeedbackFieldNameCollection.Count];
                _feedback.Populate(0);
                SetGuaranteedRandomInterconnections(_neuronFeedbackConnectionsCollection, _settings.FeedbackFieldNameCollection.Count, _settings.FeedbackConnectionDensity, _settings.FeedbackWeight);
            }
            //Context neuron
            _contextNeuron = null;
            _contextNeuronInputWeights = null;
            _contextNeuronFeedbackWeights = null;
            if (_settings.ContextNeuronFeature)
            {
                _contextNeuron = new AnalogNeuron(ActivationFactory.CreateActivationFunction(_settings.ContextNeuronActivation), 0);
                _contextNeuronInputWeights = new double[_neurons.Length];
                _rand.Fill(_contextNeuronInputWeights, _settings.ContextNeuronInputWeight.Min, _settings.ContextNeuronInputWeight.Max, _settings.ContextNeuronInputWeight.RandomSign, _settings.ContextNeuronInputWeight.DistrType);
                _contextNeuronFeedbackWeights = new double[_neurons.Length];
                _contextNeuronFeedbackWeights.Populate(0);
                int numOfContextNeuronFeedbacks = (int)Math.Round((double)_neurons.Length * _settings.ContextNeuronFeedbackDensity, 0);
                int[] neuronIndices = new int[_neurons.Length];
                neuronIndices.ShuffledIndices(_rand);
                for (int i = 0; i < numOfContextNeuronFeedbacks && i < _neurons.Length; i++)
                {
                    _contextNeuronFeedbackWeights[neuronIndices[i]] = _rand.NextDouble(_settings.ContextNeuronFeedbackWeight.Min, _settings.ContextNeuronFeedbackWeight.Max, _settings.ContextNeuronFeedbackWeight.RandomSign, _settings.ContextNeuronFeedbackWeight.DistrType);
                }
            }
            //Topology
            switch (_settings.TopologyType)
            {
                case AnalogReservoirSettings.ReservoirTopologyType.Random:
                    SetupRandomTopology((AnalogReservoirSettings.RandomTopologySettings)(_settings.TopologySettings), _settings.InternalWeight);
                    break;
                case AnalogReservoirSettings.ReservoirTopologyType.Ring:
                    SetupRingTopology((AnalogReservoirSettings.RingTopologySettings)(_settings.TopologySettings), _settings.InternalWeight);
                    break;
                case AnalogReservoirSettings.ReservoirTopologyType.DTT:
                    SetupDTTTopology((AnalogReservoirSettings.DTTTopologySettings)(_settings.TopologySettings), _settings.InternalWeight);
                    break;
            }
            //Spectral radius
            if(_settings.SpectralRadius > 0)
            {
                double maxEigenvalue = ComputeMaxEigenValue();
                if(maxEigenvalue == 0)
                {
                    throw new Exception("Invalid reservoir weights. Max eigenvalue is 0.");
                }
                double scale = _settings.SpectralRadius / maxEigenvalue;
                //Scale internal weights
                foreach(List<Connection> connCollection in _neuronNeuronConnectionsCollection)
                {
                    foreach(Connection conn in connCollection)
                    {
                        conn.Weight *= scale;
                    }
                }
                if(_settings.ContextNeuronFeature)
                {
                    for(int i = 0; i < _neurons.Length; i++)
                    {
                        _contextNeuronInputWeights[i] *= scale;
                        _contextNeuronFeedbackWeights[i] *= scale;
                    }
                }
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
        /// Computes max eigenvalue
        /// </summary>
        private double ComputeMaxEigenValue()
        {
            //Create weights matrix
            Matrix wMatrix = new Matrix(_settings.Size + 1, _settings.Size + 1);
            //Interconnections
            Parallel.For(0, _neuronNeuronConnectionsCollection.Length, row =>
            {
                for (int connIdx = 0; connIdx < _neuronNeuronConnectionsCollection[row].Count; connIdx++)
                {
                    int col = _neuronNeuronConnectionsCollection[row][connIdx].Idx;
                    double weight = _neuronNeuronConnectionsCollection[row][connIdx].Weight;
                    wMatrix.Data[row][col] = weight;
                }
            });
            //Context neuron
            if (_settings.ContextNeuronFeature)
            {
                //To context neuron
                for (int col = 0; col < _contextNeuronInputWeights.Length; col++)
                {
                    if (_contextNeuronInputWeights[col] != 0)
                    {
                        wMatrix.Data[_settings.Size][col] = _contextNeuronInputWeights[col];
                    }
                }
                //From context neuron
                for (int row = 0; row < _contextNeuronFeedbackWeights.Length; row++)
                {
                    if (_contextNeuronFeedbackWeights[row] != 0)
                    {
                        wMatrix.Data[row][_settings.Size] = _contextNeuronFeedbackWeights[row];
                    }
                }
            }
            EVD eigenvaluesDecomposition = new EVD(wMatrix);
            return eigenvaluesDecomposition.MaxAbsRealEigenvalue;
        }
        
        
        /// <summary>
        /// This general function checks the existency of the interconnection between the entity and a party entity
        /// </summary>
        /// <param name="entityConnectionsCollection">Bank of connections of the entities</param>
        /// <param name="entityIdx">An index of the entity in the connections bank</param>
        /// <param name="partyIdx">An index of the party entity</param>
        private bool ExistsInterconnection(List<Connection>[] entityConnectionsCollection, int entityIdx, int partyIdx)
        {
            //Try to select the same connection
            Connection equalConn = (from connection in entityConnectionsCollection[entityIdx]
                                    where connection.Idx == partyIdx
                                    select connection
                                    ).FirstOrDefault();
            return (equalConn != null);
        }

        /// <summary>
        /// This general function establish the interconnection between the entity and a party entity
        /// and sets the connection's random weight.
        /// </summary>
        /// <param name="entityConnectionsCollection">Bank of connections of the entities</param>
        /// <param name="entityIdx">An index of the entity in the connections bank</param>
        /// <param name="partyIdx">An index of the party entity</param>
        /// <param name="weightCfg">Random weight config</param>
        /// <param name="duplicityCheck">Indicates whether to check the interconnection existency before its creation</param>
        /// <returns>Success/Unsuccess</returns>
        private bool AddInterconnection(List<Connection>[] entityConnectionsCollection, int entityIdx, int partyIdx, RandomWeightSettings weightCfg, bool duplicityCheck)
        {
            if(duplicityCheck)
            {
                if(ExistsInterconnection(entityConnectionsCollection, entityIdx, partyIdx))
                {
                    //Connection already exists
                    return false;
                }
            }
            //Add new connection
            entityConnectionsCollection[entityIdx].Add(new Connection(partyIdx, _rand.NextDouble(weightCfg.Min, weightCfg.Max, weightCfg.RandomSign, weightCfg.DistrType)));
            return true;
        }

        /// <summary>
        /// This general function sets the random interconnections and weights between the entities and party entities.
        /// Function guarantees at least one connection for the party entity.
        /// </summary>
        /// <param name="entityConnectionsCollection">Bank of connections of the entities</param>
        /// <param name="numOfParties">Number of party entities to be interconnected with entities</param>
        /// <param name="density">Interconnection density</param>
        /// <param name="weightCfg">Random weight config</param>
        private void SetGuaranteedRandomInterconnections(List<Connection>[] entityConnectionsCollection, int numOfParties, double density, RandomWeightSettings weightCfg)
        {
            density = density.Bound(0, 1);
            int idealNumOfConnections = (int)Math.Round(entityConnectionsCollection.Length * numOfParties * density, 0);
            int partyNumOfConnections = Math.Max(1, (int)Math.Round(entityConnectionsCollection.Length * density, 0));
            //Plan the party connection counts
            int[] planedPartyConnectionCounts = new int[numOfParties];
            planedPartyConnectionCounts.Populate(partyNumOfConnections);
            int numOfAddedConnections = partyNumOfConnections * numOfParties;
            if(numOfAddedConnections < idealNumOfConnections)
            {
                for(int i = numOfAddedConnections; i < idealNumOfConnections; i++)
                {
                    int partyIdx = _rand.Next(0, numOfParties);
                    for(int j = 0; j < numOfParties; j++)
                    {
                        if (planedPartyConnectionCounts[partyIdx] < entityConnectionsCollection.Length)
                        {
                            ++planedPartyConnectionCounts[partyIdx];
                            break;
                        }
                        else
                        {
                            ++partyIdx;
                            if(partyIdx == numOfParties)
                            {
                                partyIdx = 0;
                            }
                        }
                    }
                }
            }
            //Add connections
            for (int partyIdx = 0; partyIdx < numOfParties; partyIdx++)
            {
                for (int j = 0; j < planedPartyConnectionCounts[partyIdx]; j++)
                {
                    int entityIdx = _rand.Next(0, entityConnectionsCollection.Length);
                    for(int k = 0; k < entityConnectionsCollection.Length; k++)
                    {
                        if (AddInterconnection(entityConnectionsCollection, entityIdx, partyIdx, weightCfg, true))
                        {
                            break;
                        }
                        else
                        {
                            ++entityIdx;
                            if(entityIdx == entityConnectionsCollection.Length)
                            {
                                entityIdx = 0;
                            }
                        }
                    }
                }
            }
            return;
        }

        /// <summary>
        /// This general function sets the random interconnections and weights between the entities and party entities
        /// </summary>
        /// <param name="entityConnectionsCollection">Bank of connections of the entities</param>
        /// <param name="numOfParties">Number of party entities to be interconnected with entities</param>
        /// <param name="density">Interconnection density</param>
        /// <param name="weightCfg">Random weight config</param>
        /// <param name="duplicityCheck">Indicates whether to check the interconnection existency before its creation</param>
        private void SetRandomInterconnections(List<Connection>[] entityConnectionsCollection, int numOfParties, double density, RandomWeightSettings weightCfg, bool duplicityCheck)
        {
            int[] allConnections = new int[entityConnectionsCollection.Length * numOfParties];
            int numOfConnections = (int)Math.Round(allConnections.Length * density, 0);
            allConnections.ShuffledIndices(_rand);
            for(int i = 0; i < numOfConnections && i < allConnections.Length; i++)
            {
                int entityIdx = allConnections[i] / numOfParties;
                int partyIdx = allConnections[i] % numOfParties;
                if(!AddInterconnection(entityConnectionsCollection, entityIdx, partyIdx, weightCfg, duplicityCheck))
                {
                    //Try one more
                    ++numOfConnections;
                }
            }
            return;
        }

        /// <summary>
        /// Connects all reservoir neurons to a ring shape.
        /// </summary>
        /// <param name="weightCfg">Connection weight config</param>
        /// <param name="bidirectional">Specifies whether the ring interconnection will be bidirectional.</param>
        /// <param name="duplicityCheck">Indicates whether to check the interconnection existency before its creation</param>
        private void SetRingConnections(RandomWeightSettings weightCfg, bool bidirectional, bool duplicityCheck = true)
        {
            for (int i = 0; i < _neurons.Length; i++)
            {
                int partyNeuronIdx = (i == 0) ? (_neurons.Length - 1) : (i - 1);
                AddInterconnection(_neuronNeuronConnectionsCollection, i, partyNeuronIdx, weightCfg, duplicityCheck);
                if(bidirectional)
                {
                    partyNeuronIdx = (i == _neurons.Length - 1) ? (0) : (i + 1);
                    AddInterconnection(_neuronNeuronConnectionsCollection, i, partyNeuronIdx, weightCfg, duplicityCheck);
                }
            }
            return;
        }

        /// <summary>
        /// Sets number of neurons (corresponding to density) to be self-connected
        /// </summary>
        /// <param name="density">Specifies what part of neurons will be self-connected</param>
        /// <param name="weightCfg">Random weight config</param>
        /// <param name="duplicityCheck">Indicates whether to check the interconnection existency before its creation</param>
        private void SetSelfConnections(double density, RandomWeightSettings weightCfg, bool duplicityCheck = true)
        {
            int numOfConnections = (int)Math.Round((double)_neurons.Length * density);
            int[] indices = new int[_neurons.Length];
            indices.ShuffledIndices(_rand);
            for (int i = 0; i < numOfConnections; i++)
            {
                AddInterconnection(_neuronNeuronConnectionsCollection, indices[i], indices[i], weightCfg, duplicityCheck);
            }
            return;
        }

        /// <summary>
        /// Initializes the random topology connection schema
        /// </summary>
        /// <param name="cfg">Configuration parameters</param>
        /// <param name="weightCfg">Connection weight config</param>
        private void SetupRandomTopology(AnalogReservoirSettings.RandomTopologySettings cfg, RandomWeightSettings weightCfg)
        {
            //Fully random connections setup
            SetRandomInterconnections(_neuronNeuronConnectionsCollection, _neurons.Length, cfg.ConnectionsDensity, weightCfg, false);
            return;
        }

        /// <summary>
        /// Initializes the ring topology connection schema
        /// </summary>
        /// <param name="cfg">Configuration parameters</param>
        /// <param name="weightCfg">Connection weight config</param>
        private void SetupRingTopology(AnalogReservoirSettings.RingTopologySettings cfg, RandomWeightSettings weightCfg)
        {
            //Ring connections part
            SetRingConnections(weightCfg, cfg.Bidirectional, false);
            //Self connections part
            SetSelfConnections(cfg.SelfConnectionsDensity, weightCfg, false);
            //Inter connections part
            SetRandomInterconnections(_neuronNeuronConnectionsCollection, _neurons.Length, cfg.InterConnectionsDensity, weightCfg, true);
            return;
        }

        /// <summary>
        /// Initializes the doubly twisted thoroidal topology connection schema
        /// </summary>
        /// <param name="cfg">Configuration parameters</param>
        /// <param name="weightCfg">Connection weight config</param>
        private void SetupDTTTopology(AnalogReservoirSettings.DTTTopologySettings cfg, RandomWeightSettings weightCfg)
        {
            //HTwist part (single direction ring)
            SetRingConnections(weightCfg, false);
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
                AddInterconnection(_neuronNeuronConnectionsCollection, targetNeuronIdx, partyNeuronIdx, weightCfg, false);
            }
            //Self connections part
            SetSelfConnections(cfg.SelfConnectionsDensity, weightCfg, false);
            return;
        }

        /// <summary>
        /// Collects key statistics related to reservoir neurons states
        /// </summary>
        public AnalogReservoirStat CollectStatistics()
        {
            AnalogReservoirStat stats = new AnalogReservoirStat(_instanceName, _settings.SettingsName);
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
        /// Resets all reservoir neurons to their initial state.
        /// Function does not affect weights or internal structure of the resservoir.
        /// </summary>
        /// <param name="resetStatistics">Specifies whether to reset internal statistics</param>
        public void Reset(bool resetStatistics)
        {
            Parallel.ForEach<AnalogNeuron>(_neurons, neuron =>
            {
                neuron.Reset(resetStatistics);
            });
            if (_settings.ContextNeuronFeature)
            {
                _contextNeuron.Reset(resetStatistics);
            }
            if (_settings.FeedbackFeature)
            {
                _feedback.Populate(0);
            }
            return;
        }

        /// <summary>
        /// Computes reservoir neurons states.
        /// </summary>
        /// <param name="input">
        /// Array of input values.
        /// </param>
        /// <param name="updateStatistics">
        /// Specifies whether to update neurons statistics.
        /// Specify "false" during the booting phase and "true" after the booting phase.
        /// </param>
        public void Compute(double[] input, bool updateStatistics)
        {
            //Store all the reservoir neurons states
            foreach(AnalogNeuron neuron in _neurons)
            {
                neuron.StoreCurrentState();
            }
            //Compute new states of all reservoir neurons and fill the array of output predictors
            Parallel.For(0, _neurons.Length, (neuronIdx) =>
            {
                //Input signal
                double inputSignal = 0;
                foreach(Connection conn in _neuronInputConnectionsCollection[neuronIdx])
                {
                    inputSignal += input[conn.Idx] * conn.Weight;
                }
                //Signal from reservoir neurons
                double reservoirSignal = 0;
                foreach (Connection conn in _neuronNeuronConnectionsCollection[neuronIdx])
                {
                    reservoirSignal += _neurons[conn.Idx].PreviousState * conn.Weight;
                }
                //Add context neuron signal if allowed
                reservoirSignal += _settings.ContextNeuronFeature ? _contextNeuronFeedbackWeights[neuronIdx] * _contextNeuron.CurrentState : 0;
                //Feedback signal
                double feedbackSignal = 0;
                if (_settings.FeedbackFeature)
                {
                    foreach (Connection conn in _neuronFeedbackConnectionsCollection[neuronIdx])
                    {
                        feedbackSignal += _feedback[conn.Idx] * conn.Weight;
                    }
                }
                //Compute the new state of the reservoir neuron
                _neurons[neuronIdx].Compute(inputSignal + reservoirSignal + feedbackSignal, updateStatistics);
            });
            //Compute context neuron state (if allowed)
            if (_settings.ContextNeuronFeature)
            {
                double res2ContextSignal = 0;
                for (int neuronIdx = 0; neuronIdx < _neurons.Length; neuronIdx++)
                {
                    res2ContextSignal += _contextNeuronInputWeights[neuronIdx] * _neurons[neuronIdx].CurrentState;
                }
                _contextNeuron.Compute(res2ContextSignal, updateStatistics);
            }
            return;
        }

        /// <summary>
        /// Copies all reservoir predictors to a given buffer starting from the specified possition
        /// </summary>
        public void CopyPredictorsTo(double[] buffer, int fromOffset)
        {
            Parallel.For(0, _neurons.Length, n =>
            {
                int buffIdx = fromOffset + n;
                buffer[buffIdx] = _neurons[n].CurrentState;
                if (_augmentedStatesFeature)
                {
                    buffer[buffIdx + _neurons.Length] = buffer[buffIdx] * buffer[buffIdx];
                }
            });
            return;
        }

        /// <summary>
        /// Sets feedback values for the next Compute call
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
        /// Represents a connection
        /// </summary>
        [Serializable]
        private class Connection
        {
            /// <summary>
            /// Party index
            /// </summary>
            public int Idx { get; set; }
            /// <summary>
            /// Connection weight
            /// </summary>
            public double Weight { get; set; }
            
            /// <summary>
            /// Creates an initialized instance
            /// </summary>
            /// <param name="idx">Party index</param>
            /// <param name="weight">Weight</param>
            public Connection(int idx, double weight)
            {
                Idx = idx;
                Weight = weight;
            }

        }//Connection

    }//AnalogReservoir

    /// <summary>
    /// Reservoir's key statistics
    /// </summary>
    [Serializable]
    public class AnalogReservoirStat
    {
        //Attributes
        /// <summary>
        /// Name of the reservoir instance
        /// </summary>
        public string ReservoirInstanceName { get; }
        /// <summary>
        /// Name of the reservoir configuration settings
        /// </summary>
        public string ReservoirSettingsName { get; }
        /// <summary>
        /// Statistics of max absolute values of the neurons' states
        /// </summary>
        public BasicStat NeuronsMaxAbsStatesStat { get; }
        /// <summary>
        /// Statistics of RMSs of the neurons' states
        /// </summary>
        public BasicStat NeuronsRMSStatesStat { get; }
        /// <summary>
        /// Statistics of spans of the neurons' states
        /// </summary>
        public BasicStat NeuronsStateSpansStat { get; }
        /// <summary>
        /// RMS of the context neuron's states
        /// </summary>
        public double CtxNeuronStatesRMS { get; set; }

        //Constructor
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        /// <param name="reservoirInstanceName">Name of the reservoir instance</param>
        /// <param name="reservoirSettingsName">Name of the reservoir configuration settings</param>
        public AnalogReservoirStat(string reservoirInstanceName, string reservoirSettingsName)
        {
            ReservoirInstanceName = reservoirInstanceName;
            ReservoirSettingsName = reservoirSettingsName;
            NeuronsMaxAbsStatesStat = new BasicStat();
            NeuronsRMSStatesStat = new BasicStat();
            NeuronsStateSpansStat = new BasicStat();
            CtxNeuronStatesRMS = 0;
            return;
        }

    }//AnalogReservoirStat

}//Namespace
