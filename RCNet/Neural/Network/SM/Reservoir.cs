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
        private readonly string _instanceName;
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
        private readonly INeuron[] _inputNeurons;
        /// <summary>
        /// Pools and neurons within the pool.
        /// </summary>
        private List<INeuron[]> _poolNeuronsCollection;
        /// <summary>
        /// Reservoir's all internal neurons (flat structure).
        /// </summary>
        private INeuron[] _neurons;
        /// <summary>
        /// A list of input neurons connections for each neuron
        /// </summary>
        private readonly List<ISynapse>[] _neuronInputConnectionsCollection;
        /// <summary>
        /// A list of internal neurons connections for each neuron
        /// </summary>
        private List<ISynapse>[] _neuronNeuronConnectionsCollection;
        /// <summary>
        /// Number of output predictors
        /// </summary>
        private readonly int _numOfPredictors;
        /// <summary>
        /// Specifies whether to produce augmented states
        /// </summary>
        private readonly bool _augmentedStatesFeature;

        /// <summary>
        /// Instantiates the reservoir
        /// </summary>
        /// <param name="instanceDefinition">Reservoir instance definition</param>
        /// <param name="inputRange">Range of input values</param>
        /// <param name="randomizerSeek">
        /// A value greater than or equal to 0 will always ensure the same initialization of the internal
        /// random number generator and therefore the same reservoir structure, which is good for tuning purposes.
        /// A value less than 0 causes a fully random initialization each time creating a reservoir instance.
        /// </param>
        public Reservoir(StateMachineSettings.ReservoirInstanceDefinition instanceDefinition, Interval inputRange, int randomizerSeek = -1)
        {
            int numOfInputNodes = instanceDefinition.InputFieldIdxCollection.Count;
            //Set instance name
            _instanceName = instanceDefinition.InstanceName;
            //Copy settings
            _settings = instanceDefinition.Settings.DeepClone();
            //Random generator initialization
            if (randomizerSeek < 0) _rand = new Random();
            else _rand = new Random(randomizerSeek);
            //Prepare neuron buffers
            //Input neurons
            _inputNeurons = new INeuron[numOfInputNodes];
            for(int i = 0; i < numOfInputNodes; i++)
            {
                if (_settings.InputCoding == CommonEnums.InputCodingType.Analog)
                {
                    //Analog input
                    _inputNeurons[i] = new InputAnalogNeuron(i, inputRange);
                }
                else
                {
                    //Spiking input
                    _inputNeurons[i] = new InputSpikingNeuron(i, inputRange, _settings.InputDuration);
                }
            }
            //Pools
            _numOfPredictors = 0;
            List<INeuron> allNeurons = new List<INeuron>();
            int neuronGlobalFlatIdx = 0;
            int totalNumOfNeurons = 0;
            _poolNeuronsCollection = new List<INeuron[]>(_settings.PoolSettingsCollection.Count);
            for(int poolID = 0; poolID < _settings.PoolSettingsCollection.Count; poolID++)
            {
                PoolSettings poolSettings = _settings.PoolSettingsCollection[poolID];
                totalNumOfNeurons += poolSettings.Dim.Size;
                _numOfPredictors += poolSettings.RouteToReadout ? poolSettings.Dim.Size : 0;
                //Neurons creation
                INeuron[] poolNeurons = new INeuron[poolSettings.Dim.Size];
                //Activations, Biases...
                int idx = 0;
                CommonEnums.NeuronRole[] neuronRoles = new CommonEnums.NeuronRole[poolSettings.Dim.Size];
                IActivationFunction[] activationFunctions = new IActivationFunction[poolSettings.Dim.Size];
                double[] biases = new double[poolSettings.Dim.Size];
                List<int> analogNeuronIdxs = new List<int>();
                foreach (PoolSettings.NeuronGroupSettings ngs in poolSettings.NeuronGroups)
                {
                    for(int i = 0; i < ngs.Count; i++)
                    {
                        neuronRoles[idx] = ngs.Role;
                        activationFunctions[idx] = ActivationFactory.Create(ngs.ActivationSettings, _rand);
                        if (activationFunctions[idx].OutputSignalType == ActivationFactory.FunctionOutputSignalType.Analog)
                        {
                            analogNeuronIdxs.Add(idx);
                        }
                        biases[idx] = _rand.NextDouble(ngs.BiasSettings);
                        ++idx;
                    }
                }
                //Retainment rates
                double[] retRates = new double[poolSettings.Dim.Size];
                retRates.Populate(0);
                if (poolSettings.RetainmentNeuronsFeature)
                {
                    int numOfRetNeurons = (int)Math.Round(poolSettings.RetainmentNeuronsDensity * analogNeuronIdxs.Count, 0);
                    _rand.Shuffle(analogNeuronIdxs);
                    for (int i = 0; i < numOfRetNeurons; i++)
                    {
                        retRates[analogNeuronIdxs[i]] = _rand.NextDouble(poolSettings.RetainmentRate);
                    }
                }
                //Instantiate neurons
                int neuronPoolIdx = 0;
                int[] neuronIndices = new int[poolSettings.Dim.Size];
                neuronIndices.ShuffledIndices(_rand);
                for (int x = 0; x < poolSettings.Dim.X; x++)
                {
                    for (int y = 0; y < poolSettings.Dim.Y; y++)
                    {
                        for (int z = 0; z < poolSettings.Dim.Z; z++)
                        {
                            NeuronPlacement placement = new NeuronPlacement(neuronGlobalFlatIdx, poolID, neuronPoolIdx, x, y, z);
                            //Neuron instance
                            if (activationFunctions[neuronIndices[neuronPoolIdx]].OutputSignalType == ActivationFactory.FunctionOutputSignalType.Spike)
                            {
                                //Spiking neuron
                                poolNeurons[neuronPoolIdx] = new ReservoirSpikingNeuron(placement,
                                                                                        neuronRoles[neuronIndices[neuronPoolIdx]],
                                                                                        activationFunctions[neuronIndices[neuronPoolIdx]],
                                                                                        biases[neuronIndices[neuronPoolIdx]]
                                                                                        );
                            }
                            else
                            {
                                //Analog neuron
                                poolNeurons[neuronPoolIdx] = new ReservoirAnalogNeuron(placement,
                                                                                        neuronRoles[neuronIndices[neuronPoolIdx]],
                                                                                        activationFunctions[neuronIndices[neuronPoolIdx]],
                                                                                        biases[neuronIndices[neuronPoolIdx]],
                                                                                        retRates[neuronIndices[neuronPoolIdx]]
                                                                                        );
                            }
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
            //Wiring setup
            //Pools connections
            for(int poolID = 0; poolID < _poolNeuronsCollection.Count; poolID++)
            {
                //Input connection
                SetPoolInputConnections(poolID, instanceDefinition.InputFieldAssignmentCollection);
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
            //Add pool to pool connections
            foreach(ReservoirSettings.PoolsInterconnection poolsInterConn in _settings.PoolsInterconnectionCollection)
            {
                SetPool2PoolInterconnections(poolsInterConn);
            }

            //Spectral radius
            if (_settings.SpectralRadius > 0)
            {
                double maxEigenValue = ComputeMaxEigenValue();
                if(maxEigenValue == 0)
                {
                    throw new Exception("Invalid reservoir weights. Max eigenvalue is 0.");
                }
                double scale = _settings.SpectralRadius / maxEigenValue;
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
            _augmentedStatesFeature = instanceDefinition.AugmentedStates;
            return;
        }

        //Properties
        /// <summary>
        /// Reservoir size. (Number of neurons in the reservoir)
        /// </summary>
        public int Size { get { return _neurons.Length; } }

        /// <summary>
        /// Number of reservoir's output predictors
        /// </summary>
        public int NumOfOutputPredictors { get { return _augmentedStatesFeature ? _numOfPredictors * 2 : _numOfPredictors; } }

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

        private void SetPoolInputConnections(int poolID, List<StateMachineSettings.ReservoirInstanceDefinition.InputFieldAssignment> inputFieldAssignmentCollection)
        {
            foreach(StateMachineSettings.ReservoirInstanceDefinition.InputFieldAssignment assignment in inputFieldAssignmentCollection)
            {
                if(assignment.PoolID == poolID)
                {
                    int connectionsPerInput = (int)Math.Round(_settings.PoolSettingsCollection[poolID].Dim.Size * assignment.Density, 0);
                    if (connectionsPerInput > 0)
                    {
                        int[] indices = new int[_settings.PoolSettingsCollection[poolID].Dim.Size];
                        indices.Indices();
                        _rand.Shuffle(indices);
                        for (int i = 0; i < connectionsPerInput; i++)
                        {
                            int targetNeuronIdx = indices[i];
                            StaticSynapse synapse = new StaticSynapse(_inputNeurons[assignment.FieldIdx],
                                                                      _poolNeuronsCollection[poolID][targetNeuronIdx],
                                                                      _rand.NextDouble(assignment.SynapseWeight)
                                                                      );
                            AddInterconnection(_neuronInputConnectionsCollection, synapse, false);
                        }
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
                    int addedSynapses = 0;
                    for (int i = 0; i < indices.Length && addedSynapses < connectionsPerNeuron; i++)
                    {
                        int srcNeuronIdx = indices[i];
                        if (poolSettings.InterconnectionAllowSelfConn || srcNeuronIdx != targetNeuronIdx)
                        {
                            StaticSynapse synapse = new StaticSynapse(_poolNeuronsCollection[poolID][srcNeuronIdx],
                                                                      _poolNeuronsCollection[poolID][targetNeuronIdx],
                                                                      _rand.NextDouble(poolSettings.InterconnectionSynapseWeight)
                                                                      );
                            AddInterconnection(_neuronNeuronConnectionsCollection, synapse, false);
                            ++addedSynapses;
                        }
                    }
                }
            }
            return;
        }

        private List<INeuron> SelectNeuronsByDistance(INeuron refNeuron, INeuron[] availableNeurons, double avgDistance, int count, bool allowSelfConnection)
        {
            List<INeuron> selectedNeurons = new List<INeuron>(count);
            List<INeuron> remainingNeurons = new List<INeuron>(availableNeurons.Length);
            List<double> remainingDistances = new List<double>(availableNeurons.Length);
            //Fill and analyze all distances
            BasicStat allDistancesStat = new BasicStat();
            for (int i = 0; i < availableNeurons.Length; i++)
            {
                if (allowSelfConnection || availableNeurons[i] != refNeuron)
                {
                    double distance = refNeuron.Placement.ComputeEuclideanDistance(availableNeurons[i].Placement);
                    remainingNeurons.Add(availableNeurons[i]);
                    remainingDistances.Add(distance);
                    allDistancesStat.AddSampleValue(distance);
                }
            }
            BasicStat selectedDistancesStat = new BasicStat();
            for (int n = 0; n < count; n++)
            {
                double distance = _rand.NextGaussianDouble(avgDistance).Bound(allDistancesStat.Min, allDistancesStat.Max);
                int selectedNIdx = 0;
                double err = Math.Abs(remainingDistances[selectedNIdx] - distance);
                for (int i = 1; i < remainingDistances.Count; i++)
                {
                    double cmpErr = Math.Abs(remainingDistances[i] - distance);
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
                    List<INeuron> srcNeurons = SelectNeuronsByDistance(_poolNeuronsCollection[poolID][targetNeuronIdx], _poolNeuronsCollection[poolID], poolSettings.InterconnectionAvgDistance, connectionsPerNeuron, poolSettings.InterconnectionAllowSelfConn);
                    int addedSynapses = 0;
                    for (int i = 0; i < srcNeurons.Count; i++)
                    {
                        StaticSynapse synapse = new StaticSynapse(srcNeurons[i],
                                                                    _poolNeuronsCollection[poolID][targetNeuronIdx],
                                                                    _rand.NextDouble(poolSettings.InterconnectionSynapseWeight)
                                                                    );
                        AddInterconnection(_neuronNeuronConnectionsCollection, synapse, false);
                        ++addedSynapses;
                    }
                }
            }
            return;
        }

        private void SetPool2PoolInterconnections(ReservoirSettings.PoolsInterconnection cfg)
        {
            PoolSettings targetPoolSettings = _settings.PoolSettingsCollection[cfg.TargetPoolID];
            PoolSettings sourcePoolSettings = _settings.PoolSettingsCollection[cfg.SourcePoolID];

            int[] targetIndices = new int[targetPoolSettings.Dim.Size];
            targetIndices.ShuffledIndices(_rand);
            int numOfTargetNeurons = (int)Math.Round(targetPoolSettings.Dim.Size * cfg.TargetConnectionDensity, 0);

            int[] srcIndices = new int[sourcePoolSettings.Dim.Size];
            srcIndices.Indices();
            int numOfSrcNeurons = (int)Math.Round(sourcePoolSettings.Dim.Size * cfg.SourceConnectionDensity, 0);

            for(int i = 0; i < numOfTargetNeurons; i++)
            {
                INeuron targetneuron = _poolNeuronsCollection[cfg.TargetPoolID][targetIndices[i]];
                _rand.Shuffle(srcIndices);
                for(int j = 0; j < numOfSrcNeurons; j++)
                {
                    INeuron srcNeuron = _poolNeuronsCollection[cfg.SourcePoolID][srcIndices[j]];
                    StaticSynapse synapse = new StaticSynapse(srcNeuron,
                                                              targetneuron,
                                                              _rand.NextDouble(cfg.SynapseWeight)
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
            int poolID = 0;
            foreach (PoolSettings poolSettings in _settings.PoolSettingsCollection)
            {
                ReservoirStat.PoolStat poolStat = new ReservoirStat.PoolStat(poolSettings.Name);

                //Neurons states statistics
                foreach (INeuron neuron in _poolNeuronsCollection[poolID])
                {
                    poolStat.NeuronsMaxStatesStat.AddSampleValue(neuron.StatesStat.Max);
                    poolStat.NeuronsAvgStatesStat.AddSampleValue(neuron.StatesStat.ArithAvg);
                    poolStat.NeuronsStateSpansStat.AddSampleValue(neuron.StatesStat.Span);
                    poolStat.NeuronsAvgStimuliStat.AddSampleValue(neuron.StimuliStat.ArithAvg);
                    poolStat.NeuronsMaxStimuliStat.AddSampleValue(neuron.StimuliStat.Max);
                    poolStat.NeuronsMinStimuliStat.AddSampleValue(neuron.StimuliStat.Min);
                    poolStat.NeuronsStimuliSpansStat.AddSampleValue(neuron.StimuliStat.Span);
                    poolStat.NeuronsAvgTransmissionSignalStat.AddSampleValue(neuron.TransmissionSignalStat.ArithAvg);
                    poolStat.NeuronsMaxTransmissionSignalStat.AddSampleValue(neuron.TransmissionSignalStat.Max);
                    poolStat.NeuronsMinTransmissionSignalStat.AddSampleValue(neuron.TransmissionSignalStat.Min);
                    poolStat.NeuronsAvgTransmissionFreqStat.AddSampleValue(neuron.TransmissionFreqStat.ArithAvg);
                }
                //Weights statistics
                //Input
                foreach (List<ISynapse> synapses in _neuronInputConnectionsCollection)
                {
                    foreach (ISynapse synapse in synapses)
                    {
                        if (synapse.TargetNeuron.Placement.PoolID == poolID)
                        {
                            poolStat.InputWeightsStat.AddSampleValue(synapse.Weight);
                        }
                    }
                }
                //Internal
                foreach (List<ISynapse> synapses in _neuronNeuronConnectionsCollection)
                {
                    foreach (ISynapse synapse in synapses)
                    {
                        if (synapse.TargetNeuron.Placement.PoolID == poolID)
                        {
                            poolStat.InternalWeightsStat.AddSampleValue(synapse.Weight);
                        }
                    }
                }
                stats.PoolStatCollection.Add(poolStat);
                ++poolID;
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
            //Set input to input neurons
            for(int i = 0; i < input.Length; i++)
            {
                _inputNeurons[i].Compute(input[i], updateStatistics);
            }
            //Perform computation cycles
            for (int cycle = 0; cycle < _settings.InputDuration; cycle++)
            {
                //Prepare input signal
                for (int i = 0; i < input.Length; i++)
                {
                    _inputNeurons[i].PrepareTransmissionSignal();
                }
                //Compute all reservoir neurons
                Parallel.For(0, _neurons.Length, (neuronIdx) =>
                {
                    //Input signal
                    double inputSignal = 0;
                    //Signal from input neurons
                    foreach (ISynapse synapse in _neuronInputConnectionsCollection[neuronIdx])
                    {
                        inputSignal += synapse.GetWeightedSignal();
                    }
                    //Signal from reservoir neurons
                    double reservoirSignal = 0;
                    foreach (ISynapse synapse in _neuronNeuronConnectionsCollection[neuronIdx])
                    {
                        reservoirSignal += synapse.GetWeightedSignal();
                    }
                    //Compute the new state of the reservoir neuron
                    _neurons[neuronIdx].Compute(inputSignal + reservoirSignal, updateStatistics);
                });
                //Prepare neurons signal for next computation
                Parallel.For(0, _neurons.Length, (neuronIdx) =>
                {
                    _neurons[neuronIdx].PrepareTransmissionSignal();
                });
            }
            return;
        }

        /// <summary>
        /// Copies all reservoir predictors to a given buffer starting from the specified possition
        /// </summary>
        public void CopyPredictorsTo(double[] buffer, int fromOffset)
        {
            int bufferIdx = fromOffset;
            for (int poolID = 0; poolID < _poolNeuronsCollection.Count; poolID++)
            {
                if (_settings.PoolSettingsCollection[poolID].RouteToReadout)
                {
                    for(int n = 0; n < _poolNeuronsCollection[poolID].Length; n++)
                    {
                        buffer[bufferIdx] = _poolNeuronsCollection[poolID][n].ReadoutValue;
                        ++bufferIdx;
                        if (_augmentedStatesFeature)
                        {
                            buffer[bufferIdx] = _poolNeuronsCollection[poolID][n].ReadoutAugmentedValue;
                            ++bufferIdx;
                        }
                    }
                }
            }
            return;
        }


    }//Reservoir

    /// <summary>
    /// Key statistics of the reservoir
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
        /// Collection of resrvoir pools stats
        /// </summary>
        public List<PoolStat> PoolStatCollection { get; }

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
            PoolStatCollection = new List<PoolStat>();
            return;
        }

        //Inner classes
        /// <summary>
        /// Key statistics of the reservoirs pool of neurons
        /// </summary>
        [Serializable]
        public class PoolStat
        {
            /// <summary>
            /// Name of the pool instance
            /// </summary>
            public string PoolInstanceName { get; }
            /// <summary>
            /// Statistics of max neurons' states
            /// </summary>
            public BasicStat NeuronsMaxStatesStat { get; }
            /// <summary>
            /// Statistics of avg neurons' states
            /// </summary>
            public BasicStat NeuronsAvgStatesStat { get; }
            /// <summary>
            /// Statistics of spans of the neurons' states
            /// </summary>
            public BasicStat NeuronsStateSpansStat { get; }
            /// <summary>
            /// Statistics of the neurons' average stimuli
            /// </summary>
            public BasicStat NeuronsAvgStimuliStat { get; }
            /// <summary>
            /// Statistics of the neurons' max stimuli
            /// </summary>
            public BasicStat NeuronsMaxStimuliStat { get; }
            /// <summary>
            /// Statistics of the neurons' min stimuli
            /// </summary>
            public BasicStat NeuronsMinStimuliStat { get; }
            /// <summary>
            /// Statistics of the neurons' stimuli span
            /// </summary>
            public BasicStat NeuronsStimuliSpansStat { get; }
            /// <summary>
            /// Statistics of average neurons' transmission signals
            /// </summary>
            public BasicStat NeuronsAvgTransmissionSignalStat { get; }
            /// <summary>
            /// Statistics of max neurons' transmission signals
            /// </summary>
            public BasicStat NeuronsMaxTransmissionSignalStat { get; }
            /// <summary>
            /// Statistics of min neurons' transmission signals
            /// </summary>
            public BasicStat NeuronsMinTransmissionSignalStat { get; }
            /// <summary>
            /// Statistics of average neurons' transmission frequencies
            /// </summary>
            public BasicStat NeuronsAvgTransmissionFreqStat { get; }
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
            /// <param name="poolInstanceName">Name of the pool</param>
            public PoolStat(string poolInstanceName)
            {
                PoolInstanceName = poolInstanceName;
                NeuronsMaxStatesStat = new BasicStat();
                NeuronsAvgStatesStat = new BasicStat();
                NeuronsStateSpansStat = new BasicStat();
                NeuronsAvgStimuliStat = new BasicStat();
                NeuronsMaxStimuliStat = new BasicStat();
                NeuronsMinStimuliStat = new BasicStat();
                NeuronsStimuliSpansStat = new BasicStat();
                NeuronsAvgTransmissionSignalStat = new BasicStat();
                NeuronsMaxTransmissionSignalStat = new BasicStat();
                NeuronsMinTransmissionSignalStat = new BasicStat();
                NeuronsAvgTransmissionFreqStat = new BasicStat();
                InputWeightsStat = new BasicStat();
                InternalWeightsStat = new BasicStat();
                return;
            }

        }//PoolStat

    }//ReservoirStat

}//Namespace
