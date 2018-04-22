using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using RCNet.MathTools;
using RCNet.MathTools.MatrixMath;
using RCNet.Extensions;
using RCNet.Neural.Activation;
using RCNet.RandomValue;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Implements reservoir supporting analog and spiking neurons working together.
    /// </summary>
    [Serializable]
    public class Reservoir
    {
        //Attributes
        /// <summary>
        /// Name of this instance
        /// </summary>
        private string _instanceName;
        /// <summary>
        /// Reservoir's settings.
        /// </summary>
        private ReservoirSettings _settings;
        /// <summary>
        /// Random generator.
        /// </summary>
        private Random _rand;
        /// <summary>
        /// Reservoir's input neurons.
        /// </summary>
        private InputNeuron[] _inputNeurons;
        /// <summary>
        /// Pools and neurons within the pool.
        /// </summary>
        private List<ReservoirNeuron[]> _poolNeuronsCollection;
        /// <summary>
        /// Reservoir's all internal neurons (flat structure).
        /// </summary>
        private ReservoirNeuron[] _neurons;
        /// <summary>
        /// A list of input neurons connections for each neuron
        /// </summary>
        private List<ISynapse>[] _neuronInputConnectionsCollection;
        /// <summary>
        /// A list of internal neurons connections for each neuron
        /// </summary>
        private List<ISynapse>[] _neuronNeuronConnectionsCollection;
        /// <summary>
        /// Specifies whether to produce augmented states
        /// </summary>
        private bool _augmentedStatesFeature;

        /// <summary>
        /// Instantiates the reservoir
        /// </summary>
        /// <param name="instanceName">The name of the reservoir instance</param>
        /// <param name="numOfInputNodes">Number of reservoir inputs</param>
        /// <param name="inputRange">Range of input values</param>
        /// <param name="settings">Reservoir settings</param>
        /// <param name="augmentedStates">Specifies whether this reservoir will add augmented states to output predictors</param>
        /// <param name="randomizerSeek">
        /// A value greater than or equal to 0 will always ensure the same initialization of the internal
        /// random number generator and therefore the same reservoir structure, which is good for tuning purposes.
        /// A value less than 0 causes a fully random initialization each time creating a reservoir instance.
        /// </param>
        public Reservoir(string instanceName, int numOfInputNodes, Interval inputRange, ReservoirSettings settings, bool augmentedStates, int randomizerSeek = -1)
        {
            //Set instance name
            _instanceName = instanceName;
            //Copy settings
            _settings = settings.DeepClone();
            //Random generator initialization
            if (randomizerSeek < 0) _rand = new Random();
            else _rand = new Random(randomizerSeek);
            //Prepare neuron buffers
            //Input neurons
            _inputNeurons = new InputNeuron[numOfInputNodes];
            for(int i = 0; i < numOfInputNodes; i++)
            {
                _inputNeurons[i] = new InputNeuron(i, inputRange);
            }
            //Pools
            List<ReservoirNeuron> allNeurons = new List<ReservoirNeuron>();
            int neuronGlobalFlatIdx = 0;
            int totalNumOfNeurons = 0;
            _poolNeuronsCollection = new List<ReservoirNeuron[]>(_settings.PoolSettingsCollection.Count);
            for(int poolID = 0; poolID < _settings.PoolSettingsCollection.Count; poolID++)
            {
                PoolSettings poolSettings = _settings.PoolSettingsCollection[poolID];
                totalNumOfNeurons += poolSettings.Dim.Size;
                ReservoirNeuron[] poolNeurons = new ReservoirNeuron[poolSettings.Dim.Size];
                //Retainment rates
                double[] retRates = new double[poolSettings.Dim.Size];
                retRates.Populate(0);
                if(poolSettings.RetainmentNeuronsFeature)
                {
                    int[] indices = new int[poolSettings.Dim.Size];
                    indices.ShuffledIndices(_rand);
                    int numOfRetNeurons = (int)Math.Round(poolSettings.RetainmentNeuronsDensity * poolSettings.Dim.Size, 0);
                    for(int i = 0; i < numOfRetNeurons; i++)
                    {
                        retRates[indices[i]] = _rand.NextDouble(poolSettings.RetainmentMinRate, poolSettings.RetainmentMaxRate, false, RandomClassExtensions.DistributionType.Uniform);
                    }
                }
                //Instantiate neurons
                int neuronPoolIdx = 0;
                for (int x = 0; x < poolSettings.Dim.X; x++)
                {
                    for (int y = 0; y < poolSettings.Dim.Y; y++)
                    {
                        for (int z = 0; z < poolSettings.Dim.Z; z++)
                        {
                            NeuronPlacement placement = new NeuronPlacement(neuronGlobalFlatIdx, poolID, neuronPoolIdx, x, y, z);
                            poolNeurons[neuronPoolIdx] = new ReservoirNeuron(placement,
                                                                             ActivationFactory.Create(poolSettings.Activation),
                                                                             _rand.NextDouble(poolSettings.Bias.Min, poolSettings.Bias.Max, poolSettings.Bias.RandomSign, poolSettings.Bias.DistrType),
                                                                             retRates[neuronPoolIdx]
                                                                             );
                            allNeurons.Add(poolNeurons[neuronPoolIdx]);
                            ++neuronPoolIdx;
                            ++neuronGlobalFlatIdx;
                        }
                    }
                }
                _poolNeuronsCollection.Add(poolNeurons);
            }
            //All neurons flat structure
            _neurons = allNeurons.ToArray();

            //Interconnections
            //Banks allocations
            _neuronInputConnectionsCollection = new List<ISynapse>[totalNumOfNeurons];
            _neuronNeuronConnectionsCollection = new List<ISynapse>[totalNumOfNeurons];
            for (int n = 0; n < totalNumOfNeurons; n++)
            {
                _neuronInputConnectionsCollection[n] = new List<ISynapse>();
                _neuronNeuronConnectionsCollection[n] = new List<ISynapse>();
            }
            for(int poolID = 0; poolID < _poolNeuronsCollection.Count; poolID++)
            {
                //Input connection
                SetPoolInputConnections(poolID, _settings.PoolSettingsCollection[poolID]);
                //Pool interconnection
                if (_settings.PoolSettingsCollection[poolID].InterconnectionAvgDistance > 0)
                {
                    //Required to keep average distance
                    SetPoolDistInterconnections(poolID, _settings.PoolSettingsCollection[poolID]);
                }
                else
                {
                    //Not required to keep average distance
                    SetPoolRandInterconnections(poolID, _settings.PoolSettingsCollection[poolID]);
                }
            }
            //Connections among pools
            foreach(ReservoirSettings.PoolsInterconnection poolsInterConn in _settings.PoolsInterconnectionCollection)
            {
                SetPool2PoolInterconnections(poolsInterConn);
            }

            //Spectral radius
            if (_settings.SpectralRadius > 0)
            {
                double maxEigenvalue = ComputeMaxEigenValue();
                if(maxEigenvalue == 0)
                {
                    throw new Exception("Invalid reservoir weights. Max eigenvalue is 0.");
                }
                double scale = _settings.SpectralRadius / maxEigenvalue;
                //Scale internal weights
                foreach(List<ISynapse> connCollection in _neuronNeuronConnectionsCollection)
                {
                    foreach(ISynapse conn in connCollection)
                    {
                        conn.Weight *= scale;
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
            Matrix wMatrix = new Matrix(_neurons.Length, _neurons.Length);
            //Interconnections
            Parallel.For(0, _neuronNeuronConnectionsCollection.Length, row =>
            {
                for (int connIdx = 0; connIdx < _neuronNeuronConnectionsCollection[row].Count; connIdx++)
                {
                    int col = _neuronNeuronConnectionsCollection[row][connIdx].SourceNeuron.Placement.GlobalFlatIdx;
                    double weight = _neuronNeuronConnectionsCollection[row][connIdx].Weight;
                    wMatrix.Data[row][col] = weight;
                }
            });
            EVD eigenvaluesDecomposition = new EVD(wMatrix);
            return eigenvaluesDecomposition.MaxAbsRealEigenvalue;
        }

        /// <summary>
        /// This general function checks the existency of the interconnection between the entity and a party entity
        /// </summary>
        /// <param name="entityConnectionsCollection">Bank of synapses of the entities</param>
        /// <param name="entityIdx">An index of the entity in the connections bank (target neuron)</param>
        /// <param name="partyIdx">An index of the party entity (source neuron)</param>
        private bool ExistsInterconnection(List<ISynapse>[] entityConnectionsCollection, int entityIdx, int partyIdx)
        {
            //Try to select the same synapse
            ISynapse equalConn = (from connection in entityConnectionsCollection[entityIdx]
                                    where connection.SourceNeuron.Placement.GlobalFlatIdx == partyIdx
                                    select connection
                                    ).FirstOrDefault();
            return (equalConn != null);
        }

        /// <summary>
        /// This general function establish the interconnection between the entity and a party entity.
        /// </summary>
        /// <param name="entityConnectionsCollection">Bank of connections of the entities</param>
        /// <param name="synapse">A synapse to be added into the bank</param>
        /// <param name="duplicityCheck">Indicates whether to check the interconnection existency before its creation</param>
        /// <returns>Success/Unsuccess</returns>
        private bool AddInterconnection(List<ISynapse>[] entityConnectionsCollection, ISynapse synapse, bool duplicityCheck)
        {
            int entityIdx = synapse.TargetNeuron.Placement.GlobalFlatIdx;
            int partyIdx = synapse.SourceNeuron.Placement.GlobalFlatIdx;
            if (duplicityCheck)
            {
                if(ExistsInterconnection(entityConnectionsCollection, entityIdx, partyIdx))
                {
                    //Connection already exists
                    return false;
                }
            }
            //Add new connection
            entityConnectionsCollection[entityIdx].Add(synapse);
            return true;
        }

        private void SetPoolInputConnections(int poolID, PoolSettings poolSettings)
        {
            int connectionsPerInput = (int)Math.Round(poolSettings.Dim.Size * poolSettings.InputConnectionDensity, 0);
            if(connectionsPerInput > 0)
            {
                int[] indices = new int[poolSettings.Dim.Size];
                indices.Indices();
                for(int inpIdx = 0; inpIdx < _inputNeurons.Length; inpIdx++)
                {
                    _rand.Shuffle(indices);
                    for(int i = 0; i < connectionsPerInput; i++)
                    {
                        int targetNeuronIdx = indices[i];
                        StaticSynapse synapse = new StaticSynapse(_inputNeurons[inpIdx],
                                                                  _poolNeuronsCollection[poolID][targetNeuronIdx],
                                                                  _rand.NextDouble(poolSettings.InputSynapseWeight.Min, poolSettings.InputSynapseWeight.Max, poolSettings.InputSynapseWeight.RandomSign, poolSettings.InputSynapseWeight.DistrType)
                                                                  );
                        AddInterconnection(_neuronInputConnectionsCollection, synapse, false);
                    }
                }
            }
            return;
        }

        private void SetPoolRandInterconnections(int poolID, PoolSettings poolSettings)
        {
            int connectionsPerNeuron = (int)Math.Round(poolSettings.Dim.Size * poolSettings.InterconnectionDensity, 0);
            if (connectionsPerNeuron > 0)
            {
                int[] indices = new int[poolSettings.Dim.Size];
                indices.Indices();
                for (int targetNeuronIdx = 0; targetNeuronIdx < poolSettings.Dim.Size; targetNeuronIdx++)
                {
                    _rand.Shuffle(indices);
                    for (int i = 0; i < connectionsPerNeuron; i++)
                    {
                        int srcNeuronIdx = indices[i];
                        StaticSynapse synapse = new StaticSynapse(_poolNeuronsCollection[poolID][srcNeuronIdx],
                                                                  _poolNeuronsCollection[poolID][targetNeuronIdx],
                                                                  _rand.NextDouble(poolSettings.InterconnectionSynapseWeight.Min, poolSettings.InterconnectionSynapseWeight.Max, poolSettings.InterconnectionSynapseWeight.RandomSign, poolSettings.InterconnectionSynapseWeight.DistrType)
                                                                  );
                        AddInterconnection(_neuronNeuronConnectionsCollection, synapse, false);
                    }
                }
            }
            return;
        }

        private List<INeuron> SelectNeuronsByDistance(INeuron refNeuron, INeuron[] availableNeurons, double avgDistance, int count)
        {
            List<INeuron> selectedNeurons = new List<INeuron>(count);
            List<INeuron> remainingNeurons = new List<INeuron>(availableNeurons);
            List<double> remainingDistances = new List<double>(availableNeurons.Length);
            //Fill and analyze all distances
            BasicStat allDistancesStat = new BasicStat();
            for (int i = 0; i < availableNeurons.Length; i++)
            {
                double distance = refNeuron.Placement.ComputeEuclideanDistance(availableNeurons[i].Placement);
                remainingDistances.Add(distance);
                allDistancesStat.AddSampleValue(distance);
            }
            BasicStat selectedDistancesStat = new BasicStat();
            for (int n = 0; n < count; n++)
            {
                double distance = _rand.NextGaussianDouble(avgDistance, 1).Bound(allDistancesStat.Min, allDistancesStat.Max);
                int selectedNIdx = 0;
                double err = Math.Abs(remainingDistances[selectedNIdx] - avgDistance);
                for (int i = 1; i < remainingDistances.Count; i++)
                {
                    double cmpErr = Math.Abs(remainingDistances[i] - avgDistance);
                    if(cmpErr < err)
                    {
                        selectedNIdx = i;
                        err = cmpErr;
                    }
                }
                selectedNeurons.Add(remainingNeurons[selectedNIdx]);
                selectedDistancesStat.AddSampleValue(remainingDistances[selectedNIdx]);
                remainingNeurons.RemoveAt(selectedNIdx);
                remainingDistances.RemoveAt(selectedNIdx);
            }
            return selectedNeurons;
        }

        private void SetPoolDistInterconnections(int poolID, PoolSettings poolSettings)
        {
            int connectionsPerNeuron = (int)Math.Round(poolSettings.Dim.Size * poolSettings.InterconnectionDensity, 0);
            if (connectionsPerNeuron > 0)
            {
                for (int targetNeuronIdx = 0; targetNeuronIdx < poolSettings.Dim.Size; targetNeuronIdx++)
                {
                    List<INeuron> srcNeurons = SelectNeuronsByDistance(_poolNeuronsCollection[poolID][targetNeuronIdx], _poolNeuronsCollection[poolID], poolSettings.InterconnectionAvgDistance, connectionsPerNeuron);
                    for (int i = 0; i < srcNeurons.Count; i++)
                    {
                        StaticSynapse synapse = new StaticSynapse(srcNeurons[i],
                                                                  _poolNeuronsCollection[poolID][targetNeuronIdx],
                                                                  _rand.NextDouble(poolSettings.InterconnectionSynapseWeight.Min, poolSettings.InterconnectionSynapseWeight.Max, poolSettings.InterconnectionSynapseWeight.RandomSign, poolSettings.InterconnectionSynapseWeight.DistrType)
                                                                  );
                        AddInterconnection(_neuronNeuronConnectionsCollection, synapse, false);
                    }
                }
            }
            return;
        }

        private void SetPool2PoolInterconnections(ReservoirSettings.PoolsInterconnection cfg)
        {
            PoolSettings sourcePoolSettings = _settings.PoolSettingsCollection[cfg.SourcePoolID];
            PoolSettings targetPoolSettings = _settings.PoolSettingsCollection[cfg.TargetPoolID];

            int[] srcIndices = new int[sourcePoolSettings.Dim.Size];
            srcIndices.ShuffledIndices(_rand);
            int numOfSrcNeurons = (int)Math.Round(sourcePoolSettings.Dim.Size * cfg.SourceConnectionDensity, 0);

            int[] targetIndices = new int[targetPoolSettings.Dim.Size];
            targetIndices.Indices();
            int numOfTargetNeurons = (int)Math.Round(targetPoolSettings.Dim.Size * cfg.TargetConnectionDensity, 0);

            for(int i = 0; i < numOfSrcNeurons; i++)
            {
                INeuron srcNeuron = _poolNeuronsCollection[cfg.SourcePoolID][srcIndices[i]];
                _rand.Shuffle(targetIndices);
                for(int j = 0; j < numOfTargetNeurons; j++)
                {
                    INeuron targetneuron = _poolNeuronsCollection[cfg.TargetPoolID][targetIndices[j]];
                    StaticSynapse synapse = new StaticSynapse(srcNeuron,
                                                              targetneuron,
                                                              _rand.NextDouble(cfg.SynapseWeight.Min, cfg.SynapseWeight.Max, cfg.SynapseWeight.RandomSign, cfg.SynapseWeight.DistrType)
                                                              );
                    AddInterconnection(_neuronNeuronConnectionsCollection, synapse, false);
                }
            }

            return;
        }

        /// <summary>
        /// Collects key statistics related to reservoir neurons states
        /// </summary>
        public ReservoirStat CollectStatistics()
        {
            ReservoirStat stats = new ReservoirStat(_instanceName, _settings.SettingsName);
            //Neurons states statistics
            foreach (INeuron neuron in _neurons)
            {
                stats.NeuronsMaxAbsStatesStat.AddSampleValue(Math.Max(Math.Abs(neuron.StatesStat.Max), Math.Abs(neuron.StatesStat.Min)));
                stats.NeuronsRMSStatesStat.AddSampleValue(neuron.StatesStat.RootMeanSquare);
                stats.NeuronsStateSpansStat.AddSampleValue(neuron.StatesStat.Span);
            }
            //Weights statistics
            //Input
            foreach(List<ISynapse> synapses in _neuronInputConnectionsCollection)
            {
                foreach(ISynapse synapse in synapses)
                {
                    stats.InputWeightsStat.AddSampleValue(synapse.Weight);
                }
            }
            //Internal
            foreach (List<ISynapse> synapses in _neuronNeuronConnectionsCollection)
            {
                foreach (ISynapse synapse in synapses)
                {
                    stats.InternalWeightsStat.AddSampleValue(synapse.Weight);
                }
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
            //Input neurons
            foreach(INeuron neuron in _inputNeurons)
            {
                neuron.Reset(resetStatistics);
            }
            //Reservoir neurons
            Parallel.ForEach<INeuron>(_neurons, neuron =>
            {
                neuron.Reset(resetStatistics);
            });
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
            foreach(INeuron neuron in _neurons)
            {
                neuron.StoreSignal();
            }
            //Set input to input neurons
            for(int i = 0; i < input.Length; i++)
            {
                _inputNeurons[i].Compute(input[i], updateStatistics);
            }
            //Perform computation cycles
            int compCyccles = 1 + _settings.RefractoryCycles;
            for (int cycle = 0; cycle < compCyccles; cycle++)
            {
                //Compute new states of all reservoir neurons and fill the array of output predictors
                Parallel.For(0, _neurons.Length, (neuronIdx) =>
                {
                    //Input signal
                    double inputSignal = 0;
                    if (cycle == 0)
                    {
                        //Input is affected only in the first cycle
                        foreach (ISynapse synapse in _neuronInputConnectionsCollection[neuronIdx])
                        {
                            inputSignal += synapse.ComputeSignal(updateStatistics);
                        }
                    }
                    //Signal from reservoir neurons
                    double reservoirSignal = 0;
                    foreach (ISynapse synapse in _neuronNeuronConnectionsCollection[neuronIdx])
                    {
                        reservoirSignal += synapse.ComputeSignal(updateStatistics);
                    }
                    //Compute the new state of the reservoir neuron
                    _neurons[neuronIdx].Compute(inputSignal + reservoirSignal, updateStatistics);
                });
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
                buffer[buffIdx] = _neurons[n].State;
                if (_augmentedStatesFeature)
                {
                    buffer[buffIdx + _neurons.Length] = buffer[buffIdx] * buffer[buffIdx];
                }
            });
            return;
        }


    }//Reservoir

    /// <summary>
    /// Reservoir's key statistics
    /// </summary>
    [Serializable]
    public class ReservoirStat
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
        /// Input weights statistics
        /// </summary>
        public BasicStat InputWeightsStat { get; }
        /// <summary>
        /// Internal weights statistics
        /// </summary>
        public BasicStat InternalWeightsStat { get; }

        //Constructor
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        /// <param name="reservoirInstanceName">Name of the reservoir instance</param>
        /// <param name="reservoirSettingsName">Name of the reservoir configuration settings</param>
        public ReservoirStat(string reservoirInstanceName, string reservoirSettingsName)
        {
            ReservoirInstanceName = reservoirInstanceName;
            ReservoirSettingsName = reservoirSettingsName;
            NeuronsMaxAbsStatesStat = new BasicStat();
            NeuronsRMSStatesStat = new BasicStat();
            NeuronsStateSpansStat = new BasicStat();
            InputWeightsStat = new BasicStat();
            InternalWeightsStat = new BasicStat();
            return;
        }

    }//ReservoirStat

}//Namespace
