using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using RCNet.MathTools;
using RCNet.MathTools.MatrixMath;
using RCNet.Extensions;
using RCNet.RandomValue;
using RCNet.Neural.Activation;
using RCNet.Neural.Network.SM.Preprocessing.Input;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron.Predictor;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool.NeuronGroup;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
    /// <summary>
    /// Implements reservoir supporting analog and spiking neurons working together.
    /// </summary>
    [Serializable]
    public class ReservoirInstance
    {
        //Attribute properties
        /// <summary>
        /// Reservoir's input units.
        /// </summary>
        public InputUnit[] InputUnitCollection { get; }

        //Attributes
        /// <summary>
        /// Neurons within the pools.
        /// </summary>
        private readonly List<HiddenNeuron[]> _poolNeuronsCollection;
        /// <summary>
        /// Ratio of the excitatory neurons within the pool
        /// </summary>
        private readonly double[] _poolExcitatoryNeuronsRatioCollection;
        /// <summary>
        /// Reservoir's all internal neurons (flat structure).
        /// </summary>
        private readonly HiddenNeuron[] _reservoirNeuronCollection;
        /// <summary>
        /// Ratio of the excitatory neurons within the reservoir
        /// </summary>
        private readonly double _reservoirExcitatoryNeuronsRatio;
        /// <summary>
        /// Input connections
        /// </summary>
        private readonly SortedList<int, Synapse>[] _neuronInputConnectionsCollection;
        /// <summary>
        /// Internal connections
        /// </summary>
        private readonly SortedList<int, Synapse>[] _neuronNeuronConnectionsCollection;

        //Attribute properties
        /// <summary>
        /// Reservoir's structure configuration
        /// </summary>
        public ReservoirStructureSettings StructureCfg { get; }
        
        /// <summary>
        /// Reservoir's instance configuration
        /// </summary>
        public ReservoirInstanceSettings InstanceCfg { get; }
        
        /// <summary>
        /// Reservoir's predicting neurons.
        /// </summary>
        public List<HiddenNeuron> PredictingNeuronCollection { get; }
        
        /// <summary>
        /// Number of reservoir's predictors
        /// </summary>
        public int NumOfPredictors { get; }
        
        /// <summary>
        /// Number of internal synapses
        /// </summary>
        public int NumOfInternalSynapses { get; private set; }

        /// <summary>
        /// Input distances statistics
        /// </summary>
        BasicStat InputDistancesStat { get; }

        /// <summary>
        /// Internal distances statistics
        /// </summary>
        public BasicStat InternalDistancesStat { get; }

        //Attributes
        private readonly List<Tuple<int, int>> _parallelRanges;
        private readonly int _numOfAnalogNeurons;
        private readonly int _numOfSpikingNeurons;


        //Constructor
        /// <summary>
        /// Instantiates the reservoir
        /// </summary>
        /// <param name="instanceID">ID of the reservoir instance</param>
        /// <param name="inputCfg">Input settings</param>
        /// <param name="reservoirStructureCfg">Reservoir structure configuration</param>
        /// <param name="reservoirInstanceCfg">Reservoir instance configuration</param>
        /// <param name="inputRange">Range of input values</param>
        /// <param name="rand">Random object to be used for random part of the initialization</param>
        public ReservoirInstance(int instanceID,
                                 InputSettings inputCfg,
                                 ReservoirStructureSettings reservoirStructureCfg,
                                 ReservoirInstanceSettings reservoirInstanceCfg,
                                 Interval inputRange,
                                 Random rand
                                 )
        {
            //Copy settings
            StructureCfg = (ReservoirStructureSettings)reservoirStructureCfg.DeepClone();
            InstanceCfg = (ReservoirInstanceSettings)reservoirInstanceCfg.DeepClone();
            //-----------------------------------------------------------------------------
            //Initialization of neurons
            //-----------------------------------------------------------------------------
            //Input neurons
            InputUnitCollection = new InputUnit[InstanceCfg.InputUnitsCfg.InputUnitCfgCollection.Count];
            for (int i = 0; i < InstanceCfg.InputUnitsCfg.InputUnitCfgCollection.Count; i++)
            {
                InputUnitCollection[i] = new InputUnit(instanceID,
                                                       inputRange,
                                                       inputCfg.FieldsCfg.GetFieldID(InstanceCfg.InputUnitsCfg.InputUnitCfgCollection[i].InputFieldName),
                                                       InstanceCfg.InputUnitsCfg.InputUnitCfgCollection[i]
                                                       );
            }
            //-----------------------------------------------------------------------------
            //Reservoir neurons
            //-----------------------------------------------------------------------------
            int neuronReservoirFlatIdx = 0;
            _numOfAnalogNeurons = 0;
            _numOfSpikingNeurons = 0;
            List<HiddenNeuron> allNeurons = new List<HiddenNeuron>();
            _poolNeuronsCollection = new List<HiddenNeuron[]>(StructureCfg.PoolsCfg.PoolCfgCollection.Count);
            _poolExcitatoryNeuronsRatioCollection = new double[StructureCfg.PoolsCfg.PoolCfgCollection.Count];
            _reservoirExcitatoryNeuronsRatio = 0d;
            PredictingNeuronCollection = new List<HiddenNeuron>();
            for (int poolID = 0; poolID < StructureCfg.PoolsCfg.PoolCfgCollection.Count; poolID++)
            {
                PoolSettings poolSettings = StructureCfg.PoolsCfg.PoolCfgCollection[poolID];
                _poolExcitatoryNeuronsRatioCollection[poolID] = 0;
                //------------------------------------------------------------------------------------
                //Neuron groups within the pool
                int groupID = 0, idx = 0;
                List<NeuronCreationParams> neuronParamsCollection = new List<NeuronCreationParams>();
                foreach (INeuronGroupSettings ngs in poolSettings.NeuronGroupsCfg.GroupCfgCollection)
                {
                    NeuronCreationParams[] grpNCP = new NeuronCreationParams[ngs.Count];
                    PredictorsSettings predictorsCfg = new PredictorsSettings(ngs.PredictorsCfg, poolSettings.PredictorsCfg, InstanceCfg.PredictorsCfg);
                    //Group neuron params
                    for (int i = 0; i < ngs.Count; i++)
                    {
                        grpNCP[i] = new NeuronCreationParams
                        {
                            Role = ngs.Role,
                            SignalingRestriction = ngs.SignalingRestriction,
                            Activation = ActivationFactory.Create(ngs.ActivationCfg, rand),
                            Bias = ngs.BiasCfg == null ? 0 : rand.NextDouble(ngs.BiasCfg),
                            GroupID = groupID,
                            AnalogFiringThreshold = ngs.Type == ActivationType.Spiking ? -1 : ((AnalogNeuronGroupSettings)ngs).FiringThreshold,
                            RetainmentStrength = 0,
                            PredictorsCfg = null
                        };
                        if (ngs.Role == NeuronCommon.NeuronRole.Excitatory)
                        {
                            ++_poolExcitatoryNeuronsRatioCollection[poolID];
                            ++_reservoirExcitatoryNeuronsRatio;
                        }
                        if (ngs.Type == ActivationType.Analog)
                        {
                            ++_numOfAnalogNeurons;
                        }
                        else
                        {
                            ++_numOfSpikingNeurons;
                        }
                        neuronParamsCollection.Add(grpNCP[i]);
                        ++idx;
                    }
                    //Retainment
                    if(ngs.Type == ActivationType.Analog && ((AnalogNeuronGroupSettings)ngs).RetainmentCfg != null)
                    {
                        int numOfRetNeurons = (int)Math.Round(((AnalogNeuronGroupSettings)ngs).RetainmentCfg.Density * ngs.Count, 0);
                        rand.Shuffle(grpNCP);
                        for (int i = 0; i < numOfRetNeurons; i++)
                        {
                            grpNCP[i].RetainmentStrength = rand.NextDouble(((AnalogNeuronGroupSettings)ngs).RetainmentCfg.StrengthCfg);
                        }
                    }
                    //Readout
                    if(ngs.ReadoutDensity > 0 && predictorsCfg.NumOfEnabledPredictors > 0)
                    {
                        int numOfReadoutneurons = (int)Math.Round(ngs.ReadoutDensity * ngs.Count);
                        rand.Shuffle(grpNCP);
                        for (int i = 0; i < numOfReadoutneurons; i++)
                        {
                            grpNCP[i].PredictorsCfg = predictorsCfg;
                        }
                    }
                    ++groupID;
                }//ngs
                //Finalize ratio of the excitatory neurons within the pool
                _poolExcitatoryNeuronsRatioCollection[poolID] /= poolSettings.ProportionsCfg.Size;
                //Randomize order before sequential instantiation
                rand.Shuffle(neuronParamsCollection);
                //Instantiate neurons
                HiddenNeuron[] poolNeurons = new HiddenNeuron[poolSettings.ProportionsCfg.Size];
                int neuronPoolFlatIdx = 0;
                for (int x = 0; x < poolSettings.ProportionsCfg.DimX; x++)
                {
                    for (int y = 0; y < poolSettings.ProportionsCfg.DimY; y++)
                    {
                        for (int z = 0; z < poolSettings.ProportionsCfg.DimZ; z++)
                        {
                            NeuronLocation placement = new NeuronLocation(instanceID, neuronReservoirFlatIdx, poolID, neuronPoolFlatIdx, neuronParamsCollection[neuronPoolFlatIdx].GroupID, poolSettings.CoordinatesCfg.X + x, poolSettings.CoordinatesCfg.Y + y, poolSettings.CoordinatesCfg.Z + z);
                            //Neuron instance
                            poolNeurons[neuronPoolFlatIdx] = new HiddenNeuron(placement,
                                                                              neuronParamsCollection[neuronPoolFlatIdx].Role,
                                                                              neuronParamsCollection[neuronPoolFlatIdx].Activation,
                                                                              neuronParamsCollection[neuronPoolFlatIdx].SignalingRestriction,
                                                                              neuronParamsCollection[neuronPoolFlatIdx].PredictorsCfg,
                                                                              neuronParamsCollection[neuronPoolFlatIdx].Bias,
                                                                              neuronParamsCollection[neuronPoolFlatIdx].AnalogFiringThreshold,
                                                                              neuronParamsCollection[neuronPoolFlatIdx].RetainmentStrength
                                                                              );
                            allNeurons.Add(poolNeurons[neuronPoolFlatIdx]);
                            //Predictor
                            if (poolNeurons[neuronPoolFlatIdx].PredictorsCfg != null && poolNeurons[neuronPoolFlatIdx].PredictorsCfg.NumOfEnabledPredictors > 0)
                            {
                                PredictingNeuronCollection.Add(poolNeurons[neuronPoolFlatIdx]);
                                NumOfPredictors += poolNeurons[neuronPoolFlatIdx].PredictorsCfg.NumOfEnabledPredictors;
                            }
                            ++neuronPoolFlatIdx;
                            ++neuronReservoirFlatIdx;
                        }//z
                    }//y
                }//x
                _poolNeuronsCollection.Add(poolNeurons);
            }//PoolID
            //All neurons flat structure
            _reservoirNeuronCollection = allNeurons.ToArray();
            //Ratio of the excitatory neurons within the reservoir
            _reservoirExcitatoryNeuronsRatio /= _reservoirNeuronCollection.Length;
            //Parallel processing ranges
            var rangePartitioner = Partitioner.Create(0, _reservoirNeuronCollection.Length);
            _parallelRanges = new List<Tuple<int, int>>(rangePartitioner.GetDynamicPartitions());


            //-----------------------------------------------------------------------------
            //Interconnections
            //-----------------------------------------------------------------------------
            InputDistancesStat = new BasicStat(false);
            InternalDistancesStat = new BasicStat(true);
            NumOfInternalSynapses = 0;
            //Connection banks allocations
            _neuronInputConnectionsCollection = new SortedList<int, Synapse>[_reservoirNeuronCollection.Length];
            _neuronNeuronConnectionsCollection = new SortedList<int, Synapse>[_reservoirNeuronCollection.Length];
            for (int n = 0; n < _reservoirNeuronCollection.Length; n++)
            {
                _neuronInputConnectionsCollection[n] = new SortedList<int, Synapse>();
                _neuronNeuronConnectionsCollection[n] = new SortedList<int, Synapse>();
            }

            //-----------------------------------------------------------------------------
            //Input connections
            for(int i = 0; i < InputUnitCollection.Length; i++)
            {
                SetInputConnections(rand, InputUnitCollection[i].InputFieldIdx, InstanceCfg.InputUnitsCfg.InputUnitCfgCollection[i].ConnsCfg.ConnCfgCollection);
            }

            //-----------------------------------------------------------------------------
            //Pools connections
            for (int poolID = 0; poolID < _poolNeuronsCollection.Count; poolID++)
            {
                //Apply defined schemas
                foreach(object connSchema in StructureCfg.PoolsCfg.PoolCfgCollection[poolID].InterconnectionCfg.SchemaCfgCollection)
                {
                    if (connSchema.GetType() == typeof(RandomSchemaSettings))
                    {
                        ConnectRandomSchema(poolID, rand, (RandomSchemaSettings)connSchema);
                    }
                    else if (connSchema.GetType() == typeof(ChainSchemaSettings))
                    {
                        ConnectChainSchema(poolID, rand, (ChainSchemaSettings)connSchema);
                    }
                    else
                    {
                        throw new Exception("Unsupported interconnection schema");
                    }
                }
            }

            //-----------------------------------------------------------------------------
            //Add Pool to pool connections
            if (StructureCfg.InterPoolConnectionsCfg != null)
            {
                foreach (InterPoolConnSettings interPoolConnectionCfg in StructureCfg.InterPoolConnectionsCfg.InterPoolConnectionCfgCollection)
                {
                    SetInterPoolConnection(rand, interPoolConnectionCfg);
                }
            }

            //-----------------------------------------------------------------------------
            //Setup delay on input synapses
            foreach (SortedList<int, Synapse> synapses in _neuronInputConnectionsCollection)
            {
                foreach (Synapse synapse in synapses.Values)
                {
                    synapse.SetupDelay(InputDistancesStat, rand);
                }
            }

            //-----------------------------------------------------------------------------
            //Setup delay on internal synapses and number of internal synapses
            NumOfInternalSynapses = 0;
            foreach (SortedList<int, Synapse> synapses in _neuronNeuronConnectionsCollection)
            {
                foreach (Synapse synapse in synapses.Values)
                {
                    synapse.SetupDelay(InternalDistancesStat, rand);
                    ++NumOfInternalSynapses;
                }
            }

            //-----------------------------------------------------------------------------
            //Spectral radius
            if (InstanceCfg.SynapseCfg.InternalWeightsCfg.SpikingScopeSpectralRadius == InstanceCfg.SynapseCfg.InternalWeightsCfg.AnalogScopeSpectralRadius)
            {
                //Full scope
                if (InstanceCfg.SynapseCfg.InternalWeightsCfg.SpikingScopeSpectralRadius != InternalWeightsSettings.NASpectralRadiusNum)
                {
                    ApplySpectralRadius(InstanceCfg.SynapseCfg.InternalWeightsCfg.SpikingScopeSpectralRadius, true, true);
                }
            }
            else
            {
                //Spiking scope
                if(_numOfSpikingNeurons > 0 && InstanceCfg.SynapseCfg.InternalWeightsCfg.SpikingScopeSpectralRadius != InternalWeightsSettings.NASpectralRadiusNum)
                {
                    ApplySpectralRadius(InstanceCfg.SynapseCfg.InternalWeightsCfg.SpikingScopeSpectralRadius, false, true);
                }
                //Analog scope
                if (_numOfAnalogNeurons > 0 && InstanceCfg.SynapseCfg.InternalWeightsCfg.AnalogScopeSpectralRadius != InternalWeightsSettings.NASpectralRadiusNum)
                {
                    ApplySpectralRadius(InstanceCfg.SynapseCfg.InternalWeightsCfg.AnalogScopeSpectralRadius, true, false);
                }
            }

            //Finished
            return;
        }

        //Properties
        /// <summary>
        /// Reservoir size. (Number of neurons in the reservoir)
        /// </summary>
        public int Size { get { return _reservoirNeuronCollection.Length; } }

        //Methods

        /// <summary>
        /// Scales weights of synapses to achieve requiered spectral radius on specified subset of weights
        /// </summary>
        private void ApplySpectralRadius(double spectralRadius, bool analogScope, bool spikingScope)
        {
            if(!analogScope && !spikingScope)
            {
                //Do nothing
                return;
            }
            //Select target neurons
            List<HiddenNeuron> scopeNeurons = new List<HiddenNeuron>(from neuron in _reservoirNeuronCollection
                                                                     where (analogScope && neuron.TypeOfActivation == ActivationType.Analog) ||
                                                                           (spikingScope && neuron.TypeOfActivation == ActivationType.Spiking)
                                                                     select neuron);
            if(scopeNeurons.Count == 0)
            {
                throw new Exception("ApplySpectralRadius: Invalid scope to apply spectral radius. No weight belongs into the spacified scope.");
            }
            //Create weight matrix
            Matrix wMatrix = new Matrix(_reservoirNeuronCollection.Length, _reservoirNeuronCollection.Length);
            Parallel.ForEach(scopeNeurons, neuron =>
            {
                foreach (Synapse synapse in _neuronNeuronConnectionsCollection[neuron.Location.ReservoirFlatIdx].Values)
                {
                    wMatrix.Data[neuron.Location.ReservoirFlatIdx][synapse.SourceNeuron.Location.ReservoirFlatIdx] = synapse.Weight;
                }
            });
            double largestEigenValue = Math.Abs(wMatrix.EstimateLargestEigenValue(out double[] eigenVector));
            if (largestEigenValue == 0)
            {
                throw new Exception("ApplySpectralRadius: Invalid weights or specified subset of weights. Largest eigenvalue is 0.");
            }
            double scale = spectralRadius / largestEigenValue;
            //Scale weights of synapses targeting analog neurons
            Parallel.ForEach(scopeNeurons, neuron =>
            {
                foreach (Synapse synapse in _neuronNeuronConnectionsCollection[neuron.Location.ReservoirFlatIdx].Values)
                {
                    synapse.Rescale(scale);
                }
            });
            return;
        }

        /// <summary>
        /// This general function adds the synapse into the connections bank.
        /// </summary>
        /// <param name="connectionsCollection">Bank of connections</param>
        /// <param name="synapse">A synapse to be added into the bank</param>
        /// <param name="replace">Specifies whether to replace existing connection</param>
        /// <returns>Success/Unsuccess</returns>
        private bool SetInterconnection(SortedList<int, Synapse>[] connectionsCollection, Synapse synapse, bool replace = false)
        {
            //Add new connection
            lock (connectionsCollection[synapse.TargetNeuron.Location.ReservoirFlatIdx])
            {
                try
                {
                    connectionsCollection[synapse.TargetNeuron.Location.ReservoirFlatIdx].Add(synapse.SourceNeuron.Location.ReservoirFlatIdx, synapse);
                    return true;
                }
                catch
                {
                    //Connection already exists
                    if(replace)
                    {
                        connectionsCollection[synapse.TargetNeuron.Location.ReservoirFlatIdx][synapse.SourceNeuron.Location.ReservoirFlatIdx] = synapse;
                        return true;
                    }
                    return false;
                }
            }
        }

        private void SetInputConnections(Random rand, int inputFieldID, List<InputUnitConnSettings> inputConnectionCollection)
        {
            //Create connections
            foreach (InputUnitConnSettings inputConnection in inputConnectionCollection)
            {
                int poolID = StructureCfg.PoolsCfg.GetPoolID(inputConnection.PoolName);
                //Select available targets according to connection's allowed scope
                List<HiddenNeuron>[] targetNeuronsByActivation = new List<HiddenNeuron>[Enum.GetValues(typeof(ActivationType)).Length];
                //Spiking target scope
                targetNeuronsByActivation[(int)ActivationType.Spiking] = new List<HiddenNeuron>(from neuron in _poolNeuronsCollection[poolID]
                                                                                                where (neuron.TypeOfActivation == ActivationType.Spiking && (inputConnection.SpikingTargetCfg.Scope == Synapse.SynapticTargetScope.All || (neuron.Role == NeuronCommon.NeuronRole.Excitatory && inputConnection.SpikingTargetCfg.Scope == Synapse.SynapticTargetScope.Excitatory) || (neuron.Role == NeuronCommon.NeuronRole.Inhibitory && inputConnection.SpikingTargetCfg.Scope == Synapse.SynapticTargetScope.Inhibitory)))
                                                                                                select neuron
                                                                                                );
                //Analog target scope
                targetNeuronsByActivation[(int)ActivationType.Analog] = new List<HiddenNeuron>(from neuron in _poolNeuronsCollection[poolID]
                                                                                               where (neuron.TypeOfActivation == ActivationType.Analog && (inputConnection.AnalogTargetCfg.Scope == Synapse.SynapticTargetScope.All || (neuron.Role == NeuronCommon.NeuronRole.Excitatory && inputConnection.AnalogTargetCfg.Scope == Synapse.SynapticTargetScope.Excitatory) || (neuron.Role == NeuronCommon.NeuronRole.Inhibitory && inputConnection.AnalogTargetCfg.Scope == Synapse.SynapticTargetScope.Inhibitory)))
                                                                                               select neuron
                                                                                               );
                //Scopes processing
                for (int activationType = 0; activationType < targetNeuronsByActivation.Length; activationType++)
                {
                    List<HiddenNeuron> targetNeurons = targetNeuronsByActivation[activationType];
                    double density = activationType == (int)ActivationType.Spiking ? inputConnection.SpikingTargetCfg.Density : inputConnection.AnalogTargetCfg.Density;
                    URandomValueSettings weightCfg = activationType == (int)ActivationType.Spiking ? inputConnection.SpikingTargetCfg.WeightCfg : inputConnection.AnalogTargetCfg.WeightCfg;

                    int connectionsPerInput = (int)Math.Round(targetNeurons.Count * density, 0);
                    if (connectionsPerInput > 0)
                    {
                        int[] indices = new int[targetNeurons.Count];
                        indices.Indices();
                        rand.Shuffle(indices);
                        int spikingNeuronSubIndex = 0;
                        InputNeuron analogInputNeuron = InputUnitCollection[inputFieldID].GetAnalogInputNeuron(inputConnection.AnalogCoding, inputConnection.OppositeAmplitude);
                        InputNeuron[] spikeTrainInputNeurons = InputUnitCollection[inputFieldID].GetSpikeTrainInputNeurons(inputConnection.AnalogCoding, inputConnection.OppositeAmplitude);
                        for (int i = 0; i < connectionsPerInput; i++)
                        {
                            int targetNeuronIdx = indices[i];
                            double weight = rand.NextDouble(weightCfg);
                            INeuron inputNeuron;
                            //Input neuron
                            if (inputConnection.SignalingRestriction == NeuronCommon.NeuronSignalingRestrictionType.AnalogOnly ||
                                (inputConnection.SignalingRestriction == NeuronCommon.NeuronSignalingRestrictionType.NoRestriction && targetNeurons[targetNeuronIdx].TypeOfActivation == ActivationType.Analog)
                                )
                            {
                                inputNeuron = analogInputNeuron;
                            }
                            else
                            {
                                //Connect spike train input neuron
                                inputNeuron = spikeTrainInputNeurons[spikingNeuronSubIndex];
                                ++spikingNeuronSubIndex;
                                if (spikingNeuronSubIndex == spikeTrainInputNeurons.Length)
                                {
                                    spikingNeuronSubIndex = 0;
                                }
                            }
                            Synapse synapse = new Synapse(inputNeuron,
                                                          targetNeurons[targetNeuronIdx],
                                                          weight,
                                                          InstanceCfg.SynapseCfg.InputDelayMethod,
                                                          InstanceCfg.SynapseCfg.InputMaxDelay,
                                                          InstanceCfg.SynapseCfg.PlasticityCfg.GetDynamicsSettings(inputNeuron.Role, targetNeurons[targetNeuronIdx].Role)
                                                          );
                            InputDistancesStat.AddSampleValue(synapse.Distance);
                            SetInterconnection(_neuronInputConnectionsCollection, synapse);
                        }
                    }
                }
            }
            return;
        }

        private void InterconnectPoolNeurons(Random rand,
                                             int poolID,
                                             NeuronCommon.NeuronRole sourceNeuronRole,
                                             NeuronCommon.NeuronRole targetNeuronRole,
                                             int numOfConnections,
                                             double averageDistance,
                                             bool allowSelfConnection,
                                             bool constantNumOfNeuronConnections,
                                             bool replaceExistingConnections
                                             )
        {
            //Initial condition
            if (numOfConnections <= 0)
            {
                return;
            }
            //////////////////////////////////////////////////////////////////////////////////////
            //Collect source neurons
            List<NeuronConnCount> sourceNeuronCollection = (from neuron in _poolNeuronsCollection[poolID]
                                                            where neuron.Role == sourceNeuronRole
                                                            select new NeuronConnCount { Neuron = neuron, ConnCount = 0 }
                                                            ).ToList();
            //Randomize source neurons order
            rand.Shuffle(sourceNeuronCollection);
            //////////////////////////////////////////////////////////////////////////////////////
            //Collect target neurons
            List<HiddenNeuron> targetNeuronCollection = (from neuron in _poolNeuronsCollection[poolID]
                                                         where neuron.Role == targetNeuronRole
                                                         select neuron
                                                         ).ToList();
            //////////////////////////////////////////////////////////////////////////////////////
            //Plan number of connections per each source neuron
            bool excludeSourceNeuronFromTarget = (sourceNeuronRole == targetNeuronRole && !allowSelfConnection);
            int maxPhysicalConnCountPerNeuron = targetNeuronCollection.Count - ((excludeSourceNeuronFromTarget) ? 1 : 0);
            //Check condition
            if (maxPhysicalConnCountPerNeuron == 0)
            {
                //No connections will be created
                return;
            }
            int averageConnectionsPerNeuron = (int)Math.Round((double)numOfConnections / (double)sourceNeuronCollection.Count);
            if (averageConnectionsPerNeuron < 1) averageConnectionsPerNeuron = 1;
            if (averageConnectionsPerNeuron > maxPhysicalConnCountPerNeuron) averageConnectionsPerNeuron = maxPhysicalConnCountPerNeuron;
            int connectionsCountDown = sourceNeuronCollection.Count * averageConnectionsPerNeuron;
            if (connectionsCountDown > numOfConnections) connectionsCountDown = numOfConnections;
            int minConnCount = int.MaxValue;
            int maxConnCount = int.MinValue;
            //Build plan of the connections distribution
            foreach (NeuronConnCount ncc in sourceNeuronCollection)
            {
                //Number of connections for current source neuron
                double gausseMin = averageConnectionsPerNeuron - 1d;
                double gausseMax = averageConnectionsPerNeuron + 1d;
                double gausseMean = gausseMin + (gausseMax - gausseMin) / 2d;
                double gausseSDev = (gausseMax - gausseMin) / 6d;
                int connCount = constantNumOfNeuronConnections ? averageConnectionsPerNeuron : (int)Math.Round(rand.NextFilterredGaussianDouble(gausseMean, gausseSDev, gausseMin, gausseMax));
                if (connCount > maxPhysicalConnCountPerNeuron) connCount = maxPhysicalConnCountPerNeuron;
                if (connCount > connectionsCountDown) connCount = connectionsCountDown;
                ncc.ConnCount = connCount;
                minConnCount = Math.Min(minConnCount, connCount);
                maxConnCount = Math.Max(maxConnCount, connCount);
                connectionsCountDown -= connCount;
                if (connectionsCountDown == 0)
                {
                    break;
                }
            }
            //Allow only small deviation around averageConnectionsPerNeuron
            if (minConnCount < averageConnectionsPerNeuron - 1)
            {
                sourceNeuronCollection.Sort(NeuronConnCount.CmpSortDesc);
                while (sourceNeuronCollection[sourceNeuronCollection.Count - 1].ConnCount < averageConnectionsPerNeuron - 1)
                {
                    ++sourceNeuronCollection[sourceNeuronCollection.Count - 1].ConnCount;
                    --sourceNeuronCollection[0].ConnCount;
                    sourceNeuronCollection.Sort(NeuronConnCount.CmpSortDesc);
                }
                minConnCount = sourceNeuronCollection[sourceNeuronCollection.Count - 1].ConnCount;
                maxConnCount = sourceNeuronCollection[0].ConnCount;
                rand.Shuffle(sourceNeuronCollection);
            }


            //////////////////////////////////////////////////////////////////////////////////////
            //Create physical connections
            bool byDistance = (averageDistance > 0);
            List<NeuronConnCount> sourceNeurons = new List<NeuronConnCount>(from item in sourceNeuronCollection where item.ConnCount > 0 select item);
            Random[] randFarm = new Random[sourceNeurons.Count];
            for (int i = 0; i < sourceNeurons.Count; i++)
            {
                int seed = rand.Next();
                randFarm[i] = new Random(seed);
            }
            Parallel.For(0, sourceNeurons.Count, sourceNeuronIdx =>
            {
                NeuronConnCount nccSource = sourceNeurons[sourceNeuronIdx];
                Random threadRandObj = randFarm[sourceNeuronIdx];
                //Copy all possible target neurons and compute distances if necessary
                List<RelatedNeuron> tmpRelTargetNeuronCollection = new List<RelatedNeuron>(from neuron in targetNeuronCollection
                                                                                           where (!excludeSourceNeuronFromTarget || (excludeSourceNeuronFromTarget && neuron != nccSource.Neuron))
                                                                                           select new RelatedNeuron
                                                                                           {
                                                                                               Neuron = neuron,
                                                                                               Distance = byDistance ? EuclideanDistance.Compute(nccSource.Neuron.Location.ReservoirCoordinates, neuron.Location.ReservoirCoordinates) : 0
                                                                                           });
                //Make connections of source neurons
                for (int connNum = 0; connNum < nccSource.ConnCount; connNum++)
                {
                    int targetNeuronIndex = -1;
                    //Select target neuron to be connected
                    if (byDistance)
                    {
                        //Selection based on average distance
                        double gaussianDistance = threadRandObj.NextGaussianDouble(averageDistance);
                        //Find neuron having closest distance to gaussian distance
                        double minDiff = double.MaxValue;
                        for (int i = 0; i < tmpRelTargetNeuronCollection.Count; i++)
                        {
                            double err = Math.Abs(tmpRelTargetNeuronCollection[i].Distance - gaussianDistance);
                            if (err < minDiff)
                            {
                                targetNeuronIndex = i;
                                minDiff = err;
                            }
                        }
                    }
                    else
                    {
                        //Pure random selection
                        targetNeuronIndex = threadRandObj.Next(tmpRelTargetNeuronCollection.Count);
                    }
                    //Establish connection
                    Synapse synapse = new Synapse(nccSource.Neuron,
                                                  tmpRelTargetNeuronCollection[targetNeuronIndex].Neuron,
                                                  threadRandObj.NextDouble(InstanceCfg.SynapseCfg.InternalWeightsCfg.GetWeightsSettings(nccSource.Neuron.TypeOfActivation, tmpRelTargetNeuronCollection[targetNeuronIndex].Neuron.TypeOfActivation)),
                                                  InstanceCfg.SynapseCfg.InternalDelayMethod,
                                                  InstanceCfg.SynapseCfg.InternalMaxDelay,
                                                  InstanceCfg.SynapseCfg.PlasticityCfg.GetDynamicsSettings(nccSource.Neuron.Role, tmpRelTargetNeuronCollection[targetNeuronIndex].Neuron.Role)
                                                  );
                    
                    SetInterconnection(_neuronNeuronConnectionsCollection, synapse, replaceExistingConnections);
                    //Remove targetNeuron from tmp collection
                    tmpRelTargetNeuronCollection.RemoveAt(targetNeuronIndex);
                }//connNum
            });
            return;
        }

        private void ConnectRandomSchema(int poolID, Random rand, RandomSchemaSettings settings)
        {
            PoolSettings poolSettings = StructureCfg.PoolsCfg.PoolCfgCollection[poolID];
            for (int repetition = 1; repetition <= settings.Repetitions; repetition++)
            {
                //Determine connection counts
                int intendedNumOfSynapses = (int)(Math.Round(((double)poolSettings.ProportionsCfg.Size)).Power(2) * settings.Density);
                int countE2E = (int)Math.Round(settings.ConnDistrCfg.RatioEE * intendedNumOfSynapses);
                int countE2I = (int)Math.Round(settings.ConnDistrCfg.RatioEI * intendedNumOfSynapses);
                int countI2E = (int)Math.Round(settings.ConnDistrCfg.RatioIE * intendedNumOfSynapses);
                int countI2I = (int)Math.Round(settings.ConnDistrCfg.RatioII * intendedNumOfSynapses);
                //Connections E2E
                InterconnectPoolNeurons(rand,
                                        poolID,
                                        NeuronCommon.NeuronRole.Excitatory,
                                        NeuronCommon.NeuronRole.Excitatory,
                                        countE2E,
                                        settings.AvgDistance,
                                        settings.AllowSelfConnection,
                                        settings.ConstantNumOfConnections,
                                        settings.ReplaceExistingConnections
                                        );
                //Connections E2I
                InterconnectPoolNeurons(rand,
                                        poolID,
                                        NeuronCommon.NeuronRole.Excitatory,
                                        NeuronCommon.NeuronRole.Inhibitory,
                                        countE2I,
                                        settings.AvgDistance,
                                        settings.AllowSelfConnection,
                                        settings.ConstantNumOfConnections,
                                        settings.ReplaceExistingConnections
                                        );
                //Connections I2E
                InterconnectPoolNeurons(rand,
                                        poolID,
                                        NeuronCommon.NeuronRole.Inhibitory,
                                        NeuronCommon.NeuronRole.Excitatory,
                                        countI2E,
                                        settings.AvgDistance,
                                        settings.AllowSelfConnection,
                                        settings.ConstantNumOfConnections,
                                        settings.ReplaceExistingConnections
                                        );
                //Connections I2I
                InterconnectPoolNeurons(rand,
                                        poolID,
                                        NeuronCommon.NeuronRole.Inhibitory,
                                        NeuronCommon.NeuronRole.Inhibitory,
                                        countI2I,
                                        settings.AvgDistance,
                                        settings.AllowSelfConnection,
                                        settings.ConstantNumOfConnections,
                                        settings.ReplaceExistingConnections
                                        );
            }
            return;
        }

        private void ConnectChainSchema(int poolID, Random rand, ChainSchemaSettings settings)
        {
            for (int repetition = 1; repetition <= settings.Repetitions; repetition++)
            {
                int chainLength = (int)Math.Round(settings.Ratio * _poolNeuronsCollection[poolID].Length);
                if(chainLength < 2)
                {
                    //Nothing to do
                    return;
                }
                //////////////////////////////////////////////////////////////////////////////////////
                //Collect neurons to be chained
                List<HiddenNeuron> chainNeuronCollection = new List<HiddenNeuron>(_poolNeuronsCollection[poolID]);
                rand.Shuffle(chainNeuronCollection);
                if(chainLength < chainNeuronCollection.Count)
                {
                    //Cut the list according to chainLength
                    chainNeuronCollection.RemoveRange(chainLength, chainNeuronCollection.Count - chainLength);
                }
                //////////////////////////////////////////////////////////////////////////////////////
                //Create connection pairs
                List<Tuple<HiddenNeuron, HiddenNeuron>> connPairs = new List<Tuple<HiddenNeuron, HiddenNeuron>>(chainNeuronCollection.Count * 2);
                for(int i = 0; i < chainNeuronCollection.Count - 1; i++)
                {
                    connPairs.Add(new Tuple<HiddenNeuron, HiddenNeuron>(chainNeuronCollection[i], chainNeuronCollection[i + 1]));
                }
                if(settings.Circle)
                {
                    connPairs.Add(new Tuple<HiddenNeuron, HiddenNeuron>(chainNeuronCollection[chainNeuronCollection.Count - 1], chainNeuronCollection[0]));
                }
                //////////////////////////////////////////////////////////////////////////////////////
                //Connect connection pairs
                foreach(Tuple<HiddenNeuron, HiddenNeuron> connPair in connPairs)
                {
                    //Establish connection
                    Synapse synapse = new Synapse(connPair.Item1,
                                                  connPair.Item2,
                                                  rand.NextDouble(InstanceCfg.SynapseCfg.InternalWeightsCfg.GetWeightsSettings(connPair.Item1.TypeOfActivation, connPair.Item2.TypeOfActivation)),
                                                  InstanceCfg.SynapseCfg.InternalDelayMethod,
                                                  InstanceCfg.SynapseCfg.InternalMaxDelay,
                                                  InstanceCfg.SynapseCfg.PlasticityCfg.GetDynamicsSettings(connPair.Item1.Role, connPair.Item2.Role)
                                                  );
                    SetInterconnection(_neuronNeuronConnectionsCollection, synapse, settings.ReplaceExistingConnections);
                }

            }
            return;
        }

        private void ConnectNeurons(Random rand,
                                    int sourcePoolID,
                                    NeuronCommon.NeuronRole sourceNeuronRole,
                                    int numOfSourceNeurons,
                                    int targetPoolID,
                                    NeuronCommon.NeuronRole targetNeuronRole,
                                    int totalNumOfConnections,
                                    bool constantNumOfNeuronConnections
                                    )
        {
            //Initial condition
            if (totalNumOfConnections <= 0 || numOfSourceNeurons == 0)
            {
                return;
            }
            PoolSettings sourcePoolSettings = StructureCfg.PoolsCfg.PoolCfgCollection[sourcePoolID];
            PoolSettings targetPoolSettings = StructureCfg.PoolsCfg.PoolCfgCollection[targetPoolID];
            //////////////////////////////////////////////////////////////////////////////////////
            //Collect all possible source neurons
            List<NeuronConnCount> sourceNeuronCollection = (from neuron in _poolNeuronsCollection[sourcePoolID]
                                                            where neuron.Role == sourceNeuronRole
                                                            select new NeuronConnCount { Neuron = neuron, ConnCount = 0 }
                                                            ).ToList();
            //Randomize source neurons order
            rand.Shuffle(sourceNeuronCollection);
            if (numOfSourceNeurons < 0 || numOfSourceNeurons > sourceNeuronCollection.Count)
            {
                //Set numOfSourceNeurons according to the length of the sourceNeuronCollection
                numOfSourceNeurons = sourceNeuronCollection.Count;
            }
            else
            {
                //Cut the list according to numOfSourceNeurons
                sourceNeuronCollection.RemoveRange(numOfSourceNeurons, sourceNeuronCollection.Count - numOfSourceNeurons);
            }
            //////////////////////////////////////////////////////////////////////////////////////
            //Collect all possible target neurons
            List<HiddenNeuron> targetNeuronCollection = (from neuron in _poolNeuronsCollection[targetPoolID]
                                                         where neuron.Role == targetNeuronRole
                                                         select neuron
                                                         ).ToList();
            //////////////////////////////////////////////////////////////////////////////////////
            //Plan number of connections per each source neuron
            //Check condition
            if (targetNeuronCollection.Count == 0)
            {
                //No connections will be created
                return;
            }
            int averageConnectionsPerNeuron = (int)Math.Round((double)totalNumOfConnections / (double)sourceNeuronCollection.Count);
            if (averageConnectionsPerNeuron < 1) averageConnectionsPerNeuron = 1;
            if (averageConnectionsPerNeuron > targetNeuronCollection.Count) averageConnectionsPerNeuron = targetNeuronCollection.Count;
            int connectionsCountDown = sourceNeuronCollection.Count * averageConnectionsPerNeuron;
            if(connectionsCountDown > totalNumOfConnections) connectionsCountDown = totalNumOfConnections;
            int minConnCount = int.MaxValue;
            int maxConnCount = int.MinValue;
            //Build plan of the connections distribution
            foreach (NeuronConnCount ncc in sourceNeuronCollection)
            {
                //Number of connections for current source neuron
                double gausseMin = averageConnectionsPerNeuron - 1d;
                double gausseMax = averageConnectionsPerNeuron + 1d;
                double gausseMean = gausseMin + (gausseMax - gausseMin) / 2d;
                double gausseSDev = (gausseMax - gausseMin) / 6d;
                int connCount = constantNumOfNeuronConnections ? averageConnectionsPerNeuron : (int)Math.Round(rand.NextFilterredGaussianDouble(gausseMean, gausseSDev, gausseMin, gausseMax));
                if (connCount > targetNeuronCollection.Count) connCount = targetNeuronCollection.Count;
                if (connCount > connectionsCountDown) connCount = connectionsCountDown;
                ncc.ConnCount = connCount;
                minConnCount = Math.Min(minConnCount, connCount);
                maxConnCount = Math.Max(maxConnCount, connCount);
                connectionsCountDown -= connCount;
                if (connectionsCountDown == 0)
                {
                    break;
                }
            }
            //Allow only small deviation around averageConnectionsPerNeuron
            if (minConnCount < averageConnectionsPerNeuron - 1)
            {
                sourceNeuronCollection.Sort(NeuronConnCount.CmpSortDesc);
                while (sourceNeuronCollection[sourceNeuronCollection.Count - 1].ConnCount < averageConnectionsPerNeuron - 1)
                {
                    ++sourceNeuronCollection[sourceNeuronCollection.Count - 1].ConnCount;
                    --sourceNeuronCollection[0].ConnCount;
                    sourceNeuronCollection.Sort(NeuronConnCount.CmpSortDesc);
                }
                minConnCount = sourceNeuronCollection[sourceNeuronCollection.Count - 1].ConnCount;
                maxConnCount = sourceNeuronCollection[0].ConnCount;
                rand.Shuffle(sourceNeuronCollection);
            }


            //////////////////////////////////////////////////////////////////////////////////////
            //Create physical connections
            List<NeuronConnCount> sourceNeurons = new List<NeuronConnCount>(from item in sourceNeuronCollection where item.ConnCount > 0 select item);
            Random[] randFarm = new Random[sourceNeurons.Count];
            for(int i = 0; i < sourceNeurons.Count; i++)
            {
                int seed = rand.Next();
                randFarm[i] = new Random(seed);
            }
            Parallel.For(0, sourceNeurons.Count, sourceNeuronIdx =>
            {
                NeuronConnCount nccSource = sourceNeurons[sourceNeuronIdx];
                Random threadRandObj = randFarm[sourceNeuronIdx];
                //Copy all possible target neurons and compute distances if necessary
                List<RelatedNeuron> tmpRelTargetNeuronCollection = new List<RelatedNeuron>(from neuron in targetNeuronCollection
                                                                                           select new RelatedNeuron
                                                                                           {
                                                                                               Neuron = neuron,
                                                                                               Distance = 0
                                                                                           });
                //Make connections of source neurons
                for (int connNum = 0; connNum < nccSource.ConnCount; connNum++)
                {
                    int targetNeuronIndex = -1;
                    //Select target neuron to be connected
                    //Pure random selection
                    targetNeuronIndex = threadRandObj.Next(tmpRelTargetNeuronCollection.Count);
                    //Establish connection
                    Synapse synapse = new Synapse(nccSource.Neuron,
                                                  tmpRelTargetNeuronCollection[targetNeuronIndex].Neuron,
                                                  threadRandObj.NextDouble(InstanceCfg.SynapseCfg.InternalWeightsCfg.GetWeightsSettings(nccSource.Neuron.TypeOfActivation, tmpRelTargetNeuronCollection[targetNeuronIndex].Neuron.TypeOfActivation)),
                                                  InstanceCfg.SynapseCfg.InternalDelayMethod,
                                                  InstanceCfg.SynapseCfg.InternalMaxDelay,
                                                  InstanceCfg.SynapseCfg.PlasticityCfg.GetDynamicsSettings(nccSource.Neuron.Role, tmpRelTargetNeuronCollection[targetNeuronIndex].Neuron.Role)
                                                  );
                    SetInterconnection(_neuronNeuronConnectionsCollection, synapse);
                    //Remove targetNeuron from tmp collection
                    tmpRelTargetNeuronCollection.RemoveAt(targetNeuronIndex);
                }//connNum
            });
            return;
        }

        private void SetInterPoolConnection(Random rand, InterPoolConnSettings cfg)
        {
            int sourcePoolID = StructureCfg.PoolsCfg.GetPoolID(cfg.SourcePoolName);
            int targetPoolID = StructureCfg.PoolsCfg.GetPoolID(cfg.TargetPoolName);
            PoolSettings sourcePoolSettings = StructureCfg.PoolsCfg.PoolCfgCollection[sourcePoolID];
            PoolSettings targetPoolSettings = StructureCfg.PoolsCfg.PoolCfgCollection[targetPoolID];
            //Determine counts
            int totalNumOfSourceNeurons = (int)(Math.Round(((double)sourcePoolSettings.ProportionsCfg.Size)) * cfg.SourceConnectionDensity);
            double numOfTargetNeuronsPerSourceNeuron = ((double)targetPoolSettings.ProportionsCfg.Size) * cfg.TargetConnectionDensity;
            int totalNumOfSynapses = (int)(Math.Round(totalNumOfSourceNeurons * numOfTargetNeuronsPerSourceNeuron));
            int countE2E = (int)Math.Round(cfg.ConnDistrCfg.RatioEE * totalNumOfSynapses);
            int countE2I = (int)Math.Round(cfg.ConnDistrCfg.RatioEI * totalNumOfSynapses);
            int countI2E = (int)Math.Round(cfg.ConnDistrCfg.RatioIE * totalNumOfSynapses);
            int countI2I = (int)Math.Round(cfg.ConnDistrCfg.RatioII * totalNumOfSynapses);
            //Connections E2E
            ConnectNeurons(rand,
                           sourcePoolID,
                           NeuronCommon.NeuronRole.Excitatory,
                           (int)Math.Round(countE2E/ numOfTargetNeuronsPerSourceNeuron),
                           targetPoolID,
                           NeuronCommon.NeuronRole.Excitatory,
                           countE2E,
                           cfg.ConstantNumOfConnections
                           );
            //Connections E2I
            ConnectNeurons(rand,
                           sourcePoolID,
                           NeuronCommon.NeuronRole.Excitatory,
                           (int)Math.Round(countE2I / numOfTargetNeuronsPerSourceNeuron),
                           targetPoolID,
                           NeuronCommon.NeuronRole.Inhibitory,
                           countE2I,
                           cfg.ConstantNumOfConnections
                           );
            //Connections I2E
            ConnectNeurons(rand,
                           sourcePoolID,
                           NeuronCommon.NeuronRole.Inhibitory,
                           (int)Math.Round(countI2E / numOfTargetNeuronsPerSourceNeuron),
                           targetPoolID,
                           NeuronCommon.NeuronRole.Excitatory,
                           countI2E,
                           cfg.ConstantNumOfConnections
                           );
            //Connections I2I
            ConnectNeurons(rand,
                           sourcePoolID,
                           NeuronCommon.NeuronRole.Inhibitory,
                           (int)Math.Round(countI2I / numOfTargetNeuronsPerSourceNeuron),
                           targetPoolID,
                           NeuronCommon.NeuronRole.Inhibitory,
                           countI2I,
                           cfg.ConstantNumOfConnections
                           );
            return;
        }

        /// <summary>
        /// Collects key statistics related to reservoir neurons states
        /// </summary>
        public ReservoirStat CollectStatistics()
        {
            ReservoirStat stats = new ReservoirStat(InstanceCfg.Name,
                                                    StructureCfg.Name,
                                                    Size,
                                                    _reservoirExcitatoryNeuronsRatio,
                                                    NumOfPredictors,
                                                    NumOfInternalSynapses
                                                    );
            int poolID = 0;
            foreach (PoolSettings poolSettings in StructureCfg.PoolsCfg.PoolCfgCollection)
            {
                ReservoirStat.PoolStat poolStat = new ReservoirStat.PoolStat(poolSettings, _poolNeuronsCollection[poolID].Length, _poolExcitatoryNeuronsRatioCollection[poolID]);
                //Neurons statistics
                foreach (HiddenNeuron neuron in _poolNeuronsCollection[poolID])
                {
                    poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].AvgActivationStatesStat.AddSampleValue(neuron.Statistics.ActivationStat.ArithAvg);
                    poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].MaxActivationStatesStat.AddSampleValue(neuron.Statistics.ActivationStat.Max);
                    poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].MinActivationStatesStat.AddSampleValue(neuron.Statistics.ActivationStat.Min);
                    poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].ActivationStateSpansStat.AddSampleValue(neuron.Statistics.ActivationStat.Span);
                    if (neuron.Statistics.InputStimuliStat.NumOfNonzeroSamples > 0)
                    {
                        poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].AvgIStimuliStat.AddSampleValue(neuron.Statistics.InputStimuliStat.ArithAvg);
                        poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].MaxIStimuliStat.AddSampleValue(neuron.Statistics.InputStimuliStat.Max);
                        poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].MinIStimuliStat.AddSampleValue(neuron.Statistics.InputStimuliStat.Min);
                        poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].IStimuliSpansStat.AddSampleValue(neuron.Statistics.InputStimuliStat.Span);
                    }
                    if (neuron.Statistics.ReservoirStimuliStat.NumOfNonzeroSamples > 0)
                    {
                        poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].AvgRStimuliStat.AddSampleValue(neuron.Statistics.ReservoirStimuliStat.ArithAvg);
                        poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].MaxRStimuliStat.AddSampleValue(neuron.Statistics.ReservoirStimuliStat.Max);
                        poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].MinRStimuliStat.AddSampleValue(neuron.Statistics.ReservoirStimuliStat.Min);
                        poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].RStimuliSpansStat.AddSampleValue(neuron.Statistics.ReservoirStimuliStat.Span);
                    }
                    if (neuron.Statistics.TotalStimuliStat.NumOfNonzeroSamples > 0)
                    {
                        poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].AvgTStimuliStat.AddSampleValue(neuron.Statistics.TotalStimuliStat.ArithAvg);
                        poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].MaxTStimuliStat.AddSampleValue(neuron.Statistics.TotalStimuliStat.Max);
                        poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].MinTStimuliStat.AddSampleValue(neuron.Statistics.TotalStimuliStat.Min);
                        poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].TStimuliSpansStat.AddSampleValue(neuron.Statistics.TotalStimuliStat.Span);
                    }
                    if (neuron.Statistics.AnalogSignalStat.NumOfNonzeroSamples > 0)
                    {
                        poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].AvgAnalogSignalStat.AddSampleValue(neuron.Statistics.AnalogSignalStat.ArithAvg);
                        poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].MaxAnalogSignalStat.AddSampleValue(neuron.Statistics.AnalogSignalStat.Max);
                        poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].MinAnalogSignalStat.AddSampleValue(neuron.Statistics.AnalogSignalStat.Min);
                    }
                    if (neuron.Statistics.FiringStat.NumOfNonzeroSamples > 0)
                    {
                        poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].AvgFiringStat.AddSampleValue(neuron.Statistics.FiringStat.ArithAvg);
                        poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].MaxFiringStat.AddSampleValue(neuron.Statistics.FiringStat.Max);
                        poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].MinFiringStat.AddSampleValue(neuron.Statistics.FiringStat.Min);
                    }
                    //Synapses efficacy statistics
                    foreach (Synapse rSynapse in _neuronNeuronConnectionsCollection[neuron.Location.ReservoirFlatIdx].Values)
                    {
                        if (rSynapse.EfficacyStat.NumOfSamples > 0)
                        {
                            poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].AvgSynEfficacyStat.AddSampleValue(rSynapse.EfficacyStat.ArithAvg);
                            poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].MaxSynEfficacyStat.AddSampleValue(rSynapse.EfficacyStat.Max);
                            poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].MinSynEfficacyStat.AddSampleValue(rSynapse.EfficacyStat.Min);
                            poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].SynEfficacySpansStat.AddSampleValue(rSynapse.EfficacyStat.Span);
                        }
                    }
                    if (neuron.Statistics.ReservoirStimuliStat.NumOfNonzeroSamples == 0)
                    {
                        ++poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].NumOfNoRStimuliNeurons;
                        ++poolStat.NumOfNoRStimuliNeurons;
                        ++stats.NumOfNoRStimuliNeurons;
                    }
                    if (neuron.Statistics.AnalogSignalStat.NumOfNonzeroSamples == 0)
                    {
                        ++poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].NumOfNoAnalogOutputSignalNeurons;
                        ++poolStat.NumOfNoAnalogOutputSignalNeurons;
                        ++stats.NumOfNoAnalogOutputSignalNeurons;
                    }
                    if (neuron.Statistics.AnalogSignalStat.Span == 0 && neuron.Statistics.AnalogSignalStat.Max != 0)
                    {
                        ++poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].NumOfConstAnalogOutputSignalNeurons;
                        ++poolStat.NumOfConstAnalogOutputSignalNeurons;
                        ++stats.NumOfConstAnalogOutputSignalNeurons;
                    }

                    if (neuron.Statistics.FiringStat.NumOfNonzeroSamples == 0)
                    {
                        ++poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].NumOfNotFiringNeurons;
                        ++poolStat.NumOfNotFiringNeurons;
                        ++stats.NumOfNotFiringNeurons;
                    }
                    if (neuron.Statistics.FiringStat.Span == 0 && neuron.Statistics.FiringStat.Max != 0)
                    {
                        ++poolStat.NeuronGroupStatCollection[neuron.Location.PoolGroupID].NumOfConstFiringNeurons;
                        ++poolStat.NumOfConstFiringNeurons;
                        ++stats.NumOfConstFiringNeurons;
                    }
                }
                //Weights statistics
                //Input weights
                foreach (SortedList<int, Synapse> synapses in _neuronInputConnectionsCollection)
                {
                    foreach (Synapse synapse in synapses.Values)
                    {
                        if (synapse.TargetNeuron.Location.PoolID == poolID)
                        {
                            poolStat.InputWeightsStat.AddSampleValue(synapse.Weight);
                        }
                    }
                }
                //Internal weights
                foreach (SortedList<int, Synapse> synapses in _neuronNeuronConnectionsCollection)
                {
                    foreach (Synapse synapse in synapses.Values)
                    {
                        if (synapse.TargetNeuron.Location.PoolID == poolID)
                        {
                            if (synapse.TargetNeuron.TypeOfActivation == ActivationType.Analog)
                            {
                                poolStat.InternalAnalogWeightsStat.AddSampleValue(synapse.Weight);
                            }
                            else
                            {
                                poolStat.InternalSpikingWeightsStat.AddSampleValue(synapse.Weight);
                            }
                        }
                    }
                }
                stats.PoolStatCollection.Add(poolStat);
                ++poolID;
            }
            return stats;
        }

        /// <summary>
        /// Resets all reservoir neurons and other components to their initial state.
        /// Function does not affect weights or internal structure of the resservoir.
        /// </summary>
        /// <param name="resetStatistics">Specifies whether to reset internal statistics</param>
        public void Reset(bool resetStatistics)
        {
            //Input neurons
            foreach(InputUnit inputUnit in InputUnitCollection)
            {
                inputUnit.Reset(resetStatistics);
            }
            //Reservoir neurons and all linked synapses
            Parallel.For(0, _reservoirNeuronCollection.Length, n =>
            {
                _reservoirNeuronCollection[n].Reset(resetStatistics);
                //Linked input synapses
                foreach (Synapse synapse in _neuronInputConnectionsCollection[n].Values)
                {
                    synapse.Reset(resetStatistics);
                }
                //Linked internal synapses
                foreach (Synapse synapse in _neuronNeuronConnectionsCollection[n].Values)
                {
                    synapse.Reset(resetStatistics);
                }
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
            //Set input values to input neurons
            for (int i = 0; i < input.Length; i++)
            {
                InputUnitCollection[i].NewStimulation(input[i]);
                InputUnitCollection[i].ComputeSignal(updateStatistics);
            }
            //Perform reservoir's computation cycle
            //Collect new stimulation for each reservoir neuron
            Parallel.ForEach(_parallelRanges, range =>
            {
                for (int neuronIdx = range.Item1; neuronIdx < range.Item2; neuronIdx++)
                {
                    //Stimulation from input neurons
                    double iStimuli = 0;
                    foreach (Synapse synapse in _neuronInputConnectionsCollection[neuronIdx].Values)
                    {
                        iStimuli += synapse.GetSignal(updateStatistics);
                    }
                    //Stimulation from connected reservoir neurons
                    double rStimuli = 0;
                    foreach (Synapse synapse in _neuronNeuronConnectionsCollection[neuronIdx].Values)
                    {
                        rStimuli += synapse.GetSignal(updateStatistics);
                    }
                    //Store new neuron's stimulation
                    _reservoirNeuronCollection[neuronIdx].NewStimulation(iStimuli, rStimuli);
                }
            });
            //Recompute state of all reservoir neurons
            Parallel.ForEach(_parallelRanges, range =>
            {
                for (int neuronIdx = range.Item1; neuronIdx < range.Item2; neuronIdx++)
                {
                    _reservoirNeuronCollection[neuronIdx].Recompute(updateStatistics);
                }
            });
            return;
        }

        /// <summary>
        /// Copies all reservoir predictors to a given buffer starting from the specified possition
        /// </summary>
        public void CopyPredictorsTo(double[] buffer, int fromOffset)
        {
            foreach(HiddenNeuron neuron in PredictingNeuronCollection)
            {
                fromOffset += neuron.CopyPredictorsTo(buffer, fromOffset);
            }
            return;
        }

        //Inner classes
        private class NeuronCreationParams
        {
            public NeuronCommon.NeuronRole Role { get; set; }
            public NeuronCommon.NeuronSignalingRestrictionType SignalingRestriction { get; set; }
            public IActivationFunction Activation { get; set; }
            public double Bias { get; set; }
            public int GroupID { get; set; }
            public double AnalogFiringThreshold { get; set; }
            public double RetainmentStrength { get; set; }
            public PredictorsSettings PredictorsCfg { get; set; }
        }

        private class RelatedNeuron
        {
            //Attribute properties
            public HiddenNeuron Neuron { get; set; }
            public double Distance { get; set; }

            //Methods
            public static int CompareByDistanceAsc(RelatedNeuron item1, RelatedNeuron item2)
            {
                if(item1.Distance < item2.Distance)
                {
                    return -1;
                }
                else if(item1.Distance > item2.Distance)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }

            //Inner classes
            public class DistanceAscComparer : IComparer<RelatedNeuron>
            {
                public int Compare(RelatedNeuron item1, RelatedNeuron item2)
                {
                    return CompareByDistanceAsc(item1, item2);
                }
            }
        }

        private class NeuronConnCount
        {
            public HiddenNeuron Neuron { get; set; }
            public int ConnCount { get; set; }

            public static int CmpSortDesc(NeuronConnCount item1, NeuronConnCount item2)
            {
                if (item1.ConnCount > item2.ConnCount)
                {
                    return -1;
                }
                else if (item1.ConnCount < item2.ConnCount)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
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
        /// Total number of neurons within the reservoir
        /// </summary>
        public int TotalNumOfNeurons { get; }
        /// <summary>
        /// Ratio of the excitatory neurons within the reservoir
        /// </summary>
        public double ExcitatoryNeuronsRatio;
        /// <summary>
        /// Total number of predictors
        /// </summary>
        public int TotalNumOfPredictors { get; }
        /// <summary>
        /// Total number of internal synapses
        /// </summary>
        public int TotalNumOfInternalSynapses { get; }
        /// <summary>
        /// Collection of resrvoir pools stats
        /// </summary>
        public List<PoolStat> PoolStatCollection { get; }
        /// <summary>
        /// Number of neurons getting no stimulation from connected reservoir's neurons
        /// </summary>
        public int NumOfNoRStimuliNeurons { get; set; }
        /// <summary>
        /// Number of neurons emitting no output analog signal
        /// </summary>
        public int NumOfNoAnalogOutputSignalNeurons { get; set; }
        /// <summary>
        /// Number of neurons emitting constant output analog signal
        /// </summary>
        public int NumOfConstAnalogOutputSignalNeurons { get; set; }
        /// <summary>
        /// Number of neurons emitting no output spikes
        /// </summary>
        public int NumOfNotFiringNeurons { get; set; }
        /// <summary>
        /// Number of neurons emitting constant output spikes
        /// </summary>
        public int NumOfConstFiringNeurons { get; set; }

        //Constructor
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        /// <param name="reservoirInstanceName">Name of the reservoir instance</param>
        /// <param name="reservoirSettingsName">Name of the reservoir configuration settings</param>
        /// <param name="numOfNeurons">Total number of neurons</param>
        /// <param name="excitatoryNeuronsRatio">Total ratio of the excitatory neurons</param>
        /// <param name="numOfPredictors">Total number of predictors</param>
        /// <param name="numOfInternalSynapses">Total number of synapses</param>
        public ReservoirStat(string reservoirInstanceName,
                             string reservoirSettingsName,
                             int numOfNeurons,
                             double excitatoryNeuronsRatio,
                             int numOfPredictors,
                             int numOfInternalSynapses
                             )
        {
            ReservoirInstanceName = reservoirInstanceName;
            ReservoirSettingsName = reservoirSettingsName;
            TotalNumOfNeurons = numOfNeurons;
            ExcitatoryNeuronsRatio = excitatoryNeuronsRatio;
            TotalNumOfPredictors = numOfPredictors;
            TotalNumOfInternalSynapses = numOfInternalSynapses;
            PoolStatCollection = new List<PoolStat>();
            NumOfNoRStimuliNeurons = 0;
            NumOfNoAnalogOutputSignalNeurons = 0;
            NumOfConstAnalogOutputSignalNeurons = 0;
            NumOfNotFiringNeurons = 0;
            NumOfConstFiringNeurons = 0;
            return;
        }

        //Inner classes
        /// <summary>
        /// Key statistics of the pool of neurons
        /// </summary>
        [Serializable]
        public class PoolStat
        {
            /// <summary>
            /// Name of the pool
            /// </summary>
            public string PoolName { get; }

            /// <summary>
            /// Number of neurons within the pool
            /// </summary>
            public int NumOfNeurons { get; }

            /// <summary>
            /// Number of neurons within the pool
            /// </summary>
            public double ExcitatoryNeuronsRatio { get; }

            /// <summary>
            /// Collection of the neuron group statistics
            /// </summary>
            public NeuronGroupStat[] NeuronGroupStatCollection { get; }
            /// <summary>
            /// Number of neurons getting no stimulation from connected reservoir's neurons
            /// </summary>
            public int NumOfNoRStimuliNeurons { get; set; }
            /// <summary>
            /// Number of neurons emitting no output signal
            /// </summary>
            public int NumOfNoAnalogOutputSignalNeurons { get; set; }
            /// <summary>
            /// Number of neurons emitting constant output signal
            /// </summary>
            public int NumOfConstAnalogOutputSignalNeurons { get; set; }
            /// <summary>
            /// Number of neurons emitting no output spikes
            /// </summary>
            public int NumOfNotFiringNeurons { get; set; }
            /// <summary>
            /// Number of neurons emitting constant output spikes
            /// </summary>
            public int NumOfConstFiringNeurons { get; set; }

            /// <summary>
            /// Input weights statistics
            /// </summary>
            public BasicStat InputWeightsStat { get; }

            /// <summary>
            /// Internal analog weights statistics
            /// </summary>
            public BasicStat InternalAnalogWeightsStat { get; }

            /// <summary>
            /// Internal spiking weights statistics
            /// </summary>
            public BasicStat InternalSpikingWeightsStat { get; }

            //Constructor
            /// <summary>
            /// Creates an unitialized instance
            /// </summary>
            /// <param name="poolSettings">Settings of the neuron pool</param>
            /// <param name="numOfNeurons">Number of neurons within the pool</param>
            /// <param name="excitatoryNeuronsRatio">Ratio of excitatory neurons within the pool</param>
            public PoolStat(PoolSettings poolSettings,
                            int numOfNeurons,
                            double excitatoryNeuronsRatio
                            )
            {
                PoolName = poolSettings.Name;
                NumOfNeurons = numOfNeurons;
                ExcitatoryNeuronsRatio = excitatoryNeuronsRatio;
                NeuronGroupStatCollection = new NeuronGroupStat[poolSettings.NeuronGroupsCfg.GroupCfgCollection.Count];
                for(int i = 0; i < poolSettings.NeuronGroupsCfg.GroupCfgCollection.Count; i++)
                {
                    NeuronGroupStatCollection[i] = new NeuronGroupStat(poolSettings.NeuronGroupsCfg.GroupCfgCollection[i].Name);
                }
                NumOfNoRStimuliNeurons = 0;
                NumOfNoAnalogOutputSignalNeurons = 0;
                NumOfConstAnalogOutputSignalNeurons = 0;
                NumOfNotFiringNeurons = 0;
                NumOfConstFiringNeurons = 0;
                InputWeightsStat = new BasicStat();
                InternalAnalogWeightsStat = new BasicStat();
                InternalSpikingWeightsStat = new BasicStat();
                return;
            }

            //Inner classes
            /// <summary>
            /// Key statistics of the group of neurons
            /// </summary>
            [Serializable]
            public class NeuronGroupStat
            {
                /// <summary>
                /// Name of the pool instance
                /// </summary>
                public string GroupName { get; }
                /// <summary>
                /// Statistics of neurons' activation min states
                /// </summary>
                public BasicStat MinActivationStatesStat { get; }
                /// <summary>
                /// Statistics of neurons' activation max states
                /// </summary>
                public BasicStat MaxActivationStatesStat { get; }
                /// <summary>
                /// Statistics of neurons' activation avg states
                /// </summary>
                public BasicStat AvgActivationStatesStat { get; }
                /// <summary>
                /// Statistics of spans of the neurons' activation states
                /// </summary>
                public BasicStat ActivationStateSpansStat { get; }
                /// <summary>
                /// Statistics of the neurons' average input stimuli passed to activation function
                /// </summary>
                public BasicStat AvgIStimuliStat { get; }
                /// <summary>
                /// Statistics of the neurons' max input stimuli passed to activation function
                /// </summary>
                public BasicStat MaxIStimuliStat { get; }
                /// <summary>
                /// Statistics of the neurons' min input stimuli passed to activation function
                /// </summary>
                public BasicStat MinIStimuliStat { get; }
                /// <summary>
                /// Statistics of the neurons' span of the input stimuli passed to activation function
                /// </summary>
                public BasicStat IStimuliSpansStat { get; }
                /// <summary>
                /// Statistics of the neurons' average stimuli related to part coming from connected reservoir's neurons
                /// </summary>
                public BasicStat AvgRStimuliStat { get; }
                /// <summary>
                /// Statistics of the neurons' max stimuli related to part coming from connected reservoir's neurons
                /// </summary>
                public BasicStat MaxRStimuliStat { get; }
                /// <summary>
                /// Statistics of the neurons' min stimuli related to part coming from connected reservoir's neurons
                /// </summary>
                public BasicStat MinRStimuliStat { get; }
                /// <summary>
                /// Statistics of the neurons' stimuli span related to part coming from connected reservoir's neurons
                /// </summary>
                public BasicStat RStimuliSpansStat { get; }
                /// <summary>
                /// Statistics of the neurons' average total stimuli (all components)
                /// </summary>
                public BasicStat AvgTStimuliStat { get; }
                /// <summary>
                /// Statistics of the neurons' max total stimuli (all components)
                /// </summary>
                public BasicStat MaxTStimuliStat { get; }
                /// <summary>
                /// Statistics of the neurons' min total stimuli (all components)
                /// </summary>
                public BasicStat MinTStimuliStat { get; }
                /// <summary>
                /// Statistics of the neurons' total stimuli span (all components)
                /// </summary>
                public BasicStat TStimuliSpansStat { get; }
                /// <summary>
                /// Statistics of neurons' average analog signals
                /// </summary>
                public BasicStat AvgAnalogSignalStat { get; }
                /// <summary>
                /// Statistics of neurons' max analog signals
                /// </summary>
                public BasicStat MaxAnalogSignalStat { get; }
                /// <summary>
                /// Statistics of neurons' min analog signals
                /// </summary>
                public BasicStat MinAnalogSignalStat { get; }
                /// <summary>
                /// Statistics of neurons' average firing signals
                /// </summary>
                public BasicStat AvgFiringStat { get; }
                /// <summary>
                /// Statistics of neurons' max firing signals
                /// </summary>
                public BasicStat MaxFiringStat { get; }
                /// <summary>
                /// Statistics of neurons' min firing signals
                /// </summary>
                public BasicStat MinFiringStat { get; }
                /// <summary>
                /// Statistics of the synapses' average efficacy
                /// </summary>
                public BasicStat AvgSynEfficacyStat { get; }
                /// <summary>
                /// Statistics of the synapses' max efficacy
                /// </summary>
                public BasicStat MaxSynEfficacyStat { get; }
                /// <summary>
                /// Statistics of the synapses' min efficacy
                /// </summary>
                public BasicStat MinSynEfficacyStat { get; }
                /// <summary>
                /// Statistics of the synapses' efficacy span
                /// </summary>
                public BasicStat SynEfficacySpansStat { get; }
                /// <summary>
                /// Number of neurons getting no stimulation from connected reservoir's neurons
                /// </summary>
                public int NumOfNoRStimuliNeurons { get; set; }
                /// <summary>
                /// Number of neurons emitting no output signal
                /// </summary>
                public int NumOfNoAnalogOutputSignalNeurons { get; set; }
                /// <summary>
                /// Number of neurons emitting constant output signal
                /// </summary>
                public int NumOfConstAnalogOutputSignalNeurons { get; set; }
                /// <summary>
                /// Number of neurons emitting no output spikes
                /// </summary>
                public int NumOfNotFiringNeurons { get; set; }
                /// <summary>
                /// Number of neurons emitting constant output spikes
                /// </summary>
                public int NumOfConstFiringNeurons { get; set; }


                //Constructor
                /// <summary>
                /// Creates an unitialized instance
                /// </summary>
                /// <param name="groupName">Name of the neuron group</param>
                public NeuronGroupStat(string groupName)
                {
                    GroupName = groupName;
                    MinActivationStatesStat = new BasicStat();
                    MaxActivationStatesStat = new BasicStat();
                    AvgActivationStatesStat = new BasicStat();
                    ActivationStateSpansStat = new BasicStat();
                    AvgTStimuliStat = new BasicStat();
                    MaxTStimuliStat = new BasicStat();
                    MinTStimuliStat = new BasicStat();
                    TStimuliSpansStat = new BasicStat();
                    AvgRStimuliStat = new BasicStat();
                    MaxRStimuliStat = new BasicStat();
                    MinRStimuliStat = new BasicStat();
                    RStimuliSpansStat = new BasicStat();
                    AvgIStimuliStat = new BasicStat();
                    MaxIStimuliStat = new BasicStat();
                    MinIStimuliStat = new BasicStat();
                    IStimuliSpansStat = new BasicStat();
                    AvgAnalogSignalStat = new BasicStat();
                    MaxAnalogSignalStat = new BasicStat();
                    MinAnalogSignalStat = new BasicStat();
                    AvgFiringStat = new BasicStat();
                    MaxFiringStat = new BasicStat();
                    MinFiringStat = new BasicStat();
                    AvgSynEfficacyStat = new BasicStat();
                    MaxSynEfficacyStat = new BasicStat();
                    MinSynEfficacyStat = new BasicStat();
                    SynEfficacySpansStat = new BasicStat();
                    NumOfNoRStimuliNeurons = 0;
                    NumOfNoAnalogOutputSignalNeurons = 0;
                    NumOfConstAnalogOutputSignalNeurons = 0;
                    NumOfNotFiringNeurons = 0;
                    NumOfConstFiringNeurons = 0;
                    return;
                }

            }//NeuronGroupStat

        }//PoolStat

    }//ReservoirStat

}//Namespace
