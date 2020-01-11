using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using RCNet.MathTools;
using RCNet.MathTools.MatrixMath;
using RCNet.Extensions;
using RCNet.Neural.Activation;
using System.Collections.Concurrent;
using RCNet.RandomValue;
using RCNet.Neural.Network.SM.Neuron;
using RCNet.Neural.Network.SM.Synapse;

namespace RCNet.Neural.Network.SM.Preprocessing
{
    /// <summary>
    /// Implements reservoir supporting analog and spiking neurons working together.
    /// </summary>
    [Serializable]
    public class Reservoir
    {
        //Attributes
        /// <summary>
        /// Reservoir's input units.
        /// </summary>
        private readonly InputUnit[] _inputUnitCollection;
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
        private readonly SortedList<int, ISynapse>[] _neuronInputConnectionsCollection;
        /// <summary>
        /// Internal connections
        /// </summary>
        private readonly SortedList<int, ISynapse>[] _neuronNeuronConnectionsCollection;

        //Attribute properties
        /// <summary>
        /// Reservoir's instance definition
        /// </summary>
        public NeuralPreprocessorSettings.ReservoirInstanceDefinition InstanceDefinition { get; }
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

        //Attributes
        private readonly List<Tuple<int, int>> _parallelRanges;
        private readonly int _numOfAnalogNeurons;
        private readonly int _numOfSpikingNeurons;


        //Constructor
        /// <summary>
        /// Instantiates the reservoir
        /// </summary>
        /// <param name="instanceDefinition">Reservoir instance definition</param>
        /// <param name="inputRange">Range of input values</param>
        /// <param name="rand">Random object to be used for random part initialization </param>
        public Reservoir(NeuralPreprocessorSettings.ReservoirInstanceDefinition instanceDefinition, Interval inputRange, Random rand)
        {
            //Copy settings
            InstanceDefinition = instanceDefinition.DeepClone();
            //-----------------------------------------------------------------------------
            //Initialization of neurons
            //-----------------------------------------------------------------------------
            //Input neurons
            _inputUnitCollection = new InputUnit[InstanceDefinition.InputFieldInfoCollection.Count];
            for (int i = 0; i < InstanceDefinition.InputFieldInfoCollection.Count; i++)
            {
                _inputUnitCollection[i] = new InputUnit(InstanceDefinition.Settings.InputEntryPoint,
                                                        i,
                                                        inputRange,
                                                        InstanceDefinition.InputFieldInfoCollection[i].SpikeTrainLength,
                                                        InstanceDefinition.InputFieldInfoCollection[i].TransDiffDistance,
                                                        InstanceDefinition.InputFieldInfoCollection[i].TransNumOfLinearSteps,
                                                        InstanceDefinition.InputFieldInfoCollection[i].TransPowerExponent,
                                                        InstanceDefinition.InputFieldInfoCollection[i].TransFoldedPowerExponent,
                                                        InstanceDefinition.InputFieldInfoCollection[i].TransMovingAverageWindowLength
                                                        );
            }
            BasicStat[] inputDistanceStatCollection = new BasicStat[InstanceDefinition.Settings.PoolSettingsCollection.Count];
            for(int i = 0; i < inputDistanceStatCollection.Length; i++)
            {
                inputDistanceStatCollection[i] = new BasicStat();
            }
            //-----------------------------------------------------------------------------
            //Reservoir neurons
            //-----------------------------------------------------------------------------
            int neuronReservoirFlatIdx = 0;
            _numOfAnalogNeurons = 0;
            _numOfSpikingNeurons = 0;
            List<HiddenNeuron> allNeurons = new List<HiddenNeuron>();
            _poolNeuronsCollection = new List<HiddenNeuron[]>(InstanceDefinition.Settings.PoolSettingsCollection.Count);
            _poolExcitatoryNeuronsRatioCollection = new double[InstanceDefinition.Settings.PoolSettingsCollection.Count];
            _reservoirExcitatoryNeuronsRatio = 0d;
            PredictingNeuronCollection = new List<HiddenNeuron>();
            for (int poolID = 0; poolID < InstanceDefinition.Settings.PoolSettingsCollection.Count; poolID++)
            {
                PoolSettings poolSettings = InstanceDefinition.Settings.PoolSettingsCollection[poolID];
                _poolExcitatoryNeuronsRatioCollection[poolID] = 0;
                //------------------------------------------------------------------------------------
                //Neuron groups within the pool
                int groupID = 0, idx = 0;
                List<NeuronCreationParams> neuronParamsCollection = new List<NeuronCreationParams>();
                foreach (PoolSettings.NeuronGroupSettings ngs in poolSettings.NeuronGroups)
                {
                    NeuronCreationParams[] grpNCP = new NeuronCreationParams[ngs.Count];
                    HiddenNeuronPredictorsSettings predictorsCfg = new HiddenNeuronPredictorsSettings(ngs.PredictorsCfg, poolSettings.PredictorsCfg, instanceDefinition.PredictorsCfg);
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
                            AnalogFiringThreshold = ngs.AnalogFiringThreshold,
                            RetainmentStrength = 0,
                            PredictorsCfg = null
                        };
                        if (ngs.Role == NeuronCommon.NeuronRole.Excitatory)
                        {
                            ++_poolExcitatoryNeuronsRatioCollection[poolID];
                            ++_reservoirExcitatoryNeuronsRatio;
                        }
                        if (ngs.ActivationType == ActivationType.Analog)
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
                    if(ngs.RetainmentNeuronsDensity > 0)
                    {
                        int numOfRetNeurons = (int)Math.Round(ngs.RetainmentNeuronsDensity * ngs.Count, 0);
                        rand.Shuffle(grpNCP);
                        for (int i = 0; i < numOfRetNeurons; i++)
                        {
                            grpNCP[i].RetainmentStrength = rand.NextDouble(ngs.RetainmentStrengthCfg);
                        }
                    }
                    //Readout
                    if(ngs.ReadoutNeuronsDensity > 0 && predictorsCfg.NumOfEnabledPredictors > 0)
                    {
                        int numOfReadoutneurons = (int)Math.Round(ngs.ReadoutNeuronsDensity * ngs.Count);
                        rand.Shuffle(grpNCP);
                        for (int i = 0; i < numOfReadoutneurons; i++)
                        {
                            grpNCP[i].PredictorsCfg = predictorsCfg;
                        }
                    }
                    ++groupID;
                }//ngs
                //Finalize ratio of the excitatory neurons within the pool
                _poolExcitatoryNeuronsRatioCollection[poolID] /= poolSettings.Dim.Size;
                //Randomize order before sequential instantiation
                rand.Shuffle(neuronParamsCollection);
                //Instantiate neurons
                HiddenNeuron[] poolNeurons = new HiddenNeuron[poolSettings.Dim.Size];
                int neuronPoolFlatIdx = 0;
                for (int x = 0; x < poolSettings.Dim.DimX; x++)
                {
                    for (int y = 0; y < poolSettings.Dim.DimY; y++)
                    {
                        for (int z = 0; z < poolSettings.Dim.DimZ; z++)
                        {
                            NeuronPlacement placement = new NeuronPlacement(InstanceDefinition.InstanceID, neuronReservoirFlatIdx, poolID, neuronPoolFlatIdx, neuronParamsCollection[neuronPoolFlatIdx].GroupID, poolSettings.Dim.X + x, poolSettings.Dim.Y + y, poolSettings.Dim.Z + z);
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
                            //Input distance statistics
                            inputDistanceStatCollection[placement.PoolID].AddSampleValue(EuclideanDistance.Compute(InstanceDefinition.Settings.InputEntryPoint, placement.ReservoirCoordinates));
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
            NumOfInternalSynapses = 0;
            //Connection banks allocations
            _neuronInputConnectionsCollection = new SortedList<int, ISynapse>[_reservoirNeuronCollection.Length];
            _neuronNeuronConnectionsCollection = new SortedList<int, ISynapse>[_reservoirNeuronCollection.Length];
            for (int n = 0; n < _reservoirNeuronCollection.Length; n++)
            {
                _neuronInputConnectionsCollection[n] = new SortedList<int, ISynapse>();
                _neuronNeuronConnectionsCollection[n] = new SortedList<int, ISynapse>();
            }

            //-----------------------------------------------------------------------------
            //Pools connections
            for (int poolID = 0; poolID < _poolNeuronsCollection.Count; poolID++)
            {
                //Input connections
                SetPoolInputConnections(rand, poolID, instanceDefinition.InputConnectionCollection);
                //Apply defined schemas
                foreach(object connSchema in InstanceDefinition.Settings.PoolSettingsCollection[poolID].InterconnectionCfg.Schemas)
                {
                    if (connSchema.GetType() == typeof(PoolSettings.InterconnectionSettings.RandomSchemaSettings))
                    {
                        ConnectRandomSchema(poolID, rand, (PoolSettings.InterconnectionSettings.RandomSchemaSettings)connSchema);
                    }
                    else if (connSchema.GetType() == typeof(PoolSettings.InterconnectionSettings.ChainSchemaSettings))
                    {
                        ConnectChainSchema(poolID, rand, (PoolSettings.InterconnectionSettings.ChainSchemaSettings)connSchema);
                    }
                    else
                    {
                        throw new Exception("Unsupported interconnection schema");
                    }
                }
            }

            //-----------------------------------------------------------------------------
            //Add Pool to pool connections
            foreach (ReservoirSettings.PoolsInterconnection poolsInterConn in InstanceDefinition.Settings.PoolsInterconnectionCollection)
            {
                SetPool2PoolInterconnections(rand, poolsInterConn);
            }

            //Number of internal synapses
            NumOfInternalSynapses = GetNumOfInternalSynapses();
            
            //-----------------------------------------------------------------------------
            //Setup of the input synaptic delays
            //Delay setup on input synapses
            //Parallel.ForEach(_neuronInputConnectionsCollection, synapses =>
            foreach (SortedList<int, ISynapse> synapses in _neuronInputConnectionsCollection)
            {
                foreach (InputSynapse synapse in synapses.Values)
                {
                    synapse.SetDelay(inputDistanceStatCollection[synapse.TargetNeuron.Placement.PoolID], rand);
                }
            }//);

            //-----------------------------------------------------------------------------
            //Spectral radius
            if (instanceDefinition.Settings.SpikingScopeSpectralRadius == instanceDefinition.Settings.AnalogScopeSpectralRadius)
            {
                //Full scope
                if (instanceDefinition.Settings.SpikingScopeSpectralRadius != ReservoirSettings.NASpectralRadius)
                {
                    ApplySpectralRadius(instanceDefinition.Settings.SpikingScopeSpectralRadius, true, true);
                }
            }
            else
            {
                //Spiking scope
                if(_numOfSpikingNeurons > 0 && instanceDefinition.Settings.SpikingScopeSpectralRadius != ReservoirSettings.NASpectralRadius)
                {
                    ApplySpectralRadius(instanceDefinition.Settings.SpikingScopeSpectralRadius, false, true);
                }
                //Analog scope
                if (_numOfAnalogNeurons > 0 && instanceDefinition.Settings.AnalogScopeSpectralRadius != ReservoirSettings.NASpectralRadius)
                {
                    ApplySpectralRadius(instanceDefinition.Settings.AnalogScopeSpectralRadius, true, false);
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
                foreach (ISynapse synapse in _neuronNeuronConnectionsCollection[neuron.Placement.ReservoirFlatIdx].Values)
                {
                    wMatrix.Data[neuron.Placement.ReservoirFlatIdx][synapse.SourceNeuron.Placement.ReservoirFlatIdx] = synapse.Weight;
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
                foreach (ISynapse synapse in _neuronNeuronConnectionsCollection[neuron.Placement.ReservoirFlatIdx].Values)
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
        private bool SetInterconnection(SortedList<int, ISynapse>[] connectionsCollection, ISynapse synapse, bool replace = false)
        {
            //Add new connection
            lock (connectionsCollection[synapse.TargetNeuron.Placement.ReservoirFlatIdx])
            {
                try
                {
                    connectionsCollection[synapse.TargetNeuron.Placement.ReservoirFlatIdx].Add(synapse.SourceNeuron.Placement.ReservoirFlatIdx, synapse);
                    return true;
                }
                catch
                {
                    //Connection already exists
                    if(replace)
                    {
                        connectionsCollection[synapse.TargetNeuron.Placement.ReservoirFlatIdx][synapse.SourceNeuron.Placement.ReservoirFlatIdx] = synapse;
                        return true;
                    }
                    return false;
                }
            }
        }

        private int GetNumOfInternalSynapses()
        {
            int numOfSynapses = 0;
            foreach(SortedList<int, ISynapse> conns in _neuronNeuronConnectionsCollection)
            {
                numOfSynapses += conns.Count;
            }
            return numOfSynapses;
        }

        private void SetPoolInputConnections(Random rand, int poolID, List<NeuralPreprocessorSettings.ReservoirInstanceDefinition.InputConnection> inputConnectionCollection)
        {
            //Create connections
            foreach (NeuralPreprocessorSettings.ReservoirInstanceDefinition.InputConnection inputConnection in inputConnectionCollection)
            {
                if (inputConnection.PoolID == poolID)
                {
                    //Select available targets according to synapse's allowed scope
                    List<HiddenNeuron> targetNeurons = new List<HiddenNeuron>(from neuron in _poolNeuronsCollection[poolID]
                                                                              where  (neuron.TypeOfActivation == ActivationType.Analog && (inputConnection.SynapseCfg.AnalogTargetScope == BaseSynapse.SynapticTargetScope.All || (neuron.Role == NeuronCommon.NeuronRole.Excitatory && inputConnection.SynapseCfg.AnalogTargetScope == BaseSynapse.SynapticTargetScope.Excitatory) || (neuron.Role == NeuronCommon.NeuronRole.Inhibitory && inputConnection.SynapseCfg.AnalogTargetScope == BaseSynapse.SynapticTargetScope.Inhibitory))) ||
                                                                                     (neuron.TypeOfActivation == ActivationType.Spiking && (inputConnection.SynapseCfg.SpikingTargetScope == BaseSynapse.SynapticTargetScope.All || (neuron.Role == NeuronCommon.NeuronRole.Excitatory && inputConnection.SynapseCfg.SpikingTargetScope == BaseSynapse.SynapticTargetScope.Excitatory) || (neuron.Role == NeuronCommon.NeuronRole.Inhibitory && inputConnection.SynapseCfg.SpikingTargetScope == BaseSynapse.SynapticTargetScope.Inhibitory)))
                                                                              select neuron
                                                                              );
                    int connectionsPerInput = (int)Math.Round(InstanceDefinition.Settings.PoolSettingsCollection[poolID].Dim.Size * inputConnection.Density, 0);
                    if(connectionsPerInput > targetNeurons.Count)
                    {
                        connectionsPerInput = targetNeurons.Count;
                    }
                    if (connectionsPerInput > 0)
                    {
                        int[] indices = new int[targetNeurons.Count];
                        indices.Indices();
                        rand.Shuffle(indices);
                        int spikingNeuronSubIndex = 0;
                        for (int i = 0; i < connectionsPerInput; i++)
                        {
                            int targetNeuronIdx = indices[i];
                            double weight = targetNeurons[targetNeuronIdx].TypeOfActivation == ActivationType.Spiking ? rand.NextDouble(inputConnection.SynapseCfg.SpikingTargetWeightCfg) : rand.NextDouble(inputConnection.SynapseCfg.AnalogTargetWeightCfg);
                            INeuron inputNeuron;
                            //Input neuron
                            if(inputConnection.SignalingRestriction == NeuronCommon.NeuronSignalingRestrictionType.AnalogOnly ||
                               (inputConnection.SignalingRestriction == NeuronCommon.NeuronSignalingRestrictionType.NoRestriction && targetNeurons[targetNeuronIdx].TypeOfActivation == ActivationType.Analog)
                               )
                            {
                                //Connect analog input neuron
                                if (targetNeurons[targetNeuronIdx].TypeOfActivation == ActivationType.Analog)
                                {
                                    inputNeuron = _inputUnitCollection[inputConnection.FieldIdx].AnalogInputNeuronCollection[inputConnection.SynapseCfg.AnalogTransRelShares.SelectNext()];
                                }
                                else
                                {
                                    inputNeuron = _inputUnitCollection[inputConnection.FieldIdx].AnalogInputNeuronCollection[(int)InputUnit.AnalogCodingMethod.Original];
                                }
                            }
                            else
                            {
                                //Connect train spiking input neuron
                                inputNeuron = _inputUnitCollection[inputConnection.FieldIdx].SpikingInputNeuronCollection[spikingNeuronSubIndex];
                                ++spikingNeuronSubIndex;
                                if (spikingNeuronSubIndex == _inputUnitCollection[inputConnection.FieldIdx].SpikingInputNeuronCollection.Length)
                                {
                                    spikingNeuronSubIndex = 0;
                                }
                            }
                            ISynapse synapse = new InputSynapse(inputNeuron,
                                                                targetNeurons[targetNeuronIdx],
                                                                weight,
                                                                inputConnection.SynapseCfg.DelayMethod,
                                                                inputConnection.SynapseCfg.MaxDelay
                                                                );
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
                                             bool replaceExistingConnections,
                                             InternalSynapseSettings synapseCfg
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
                                                                                               Distance = byDistance ? EuclideanDistance.Compute(nccSource.Neuron.Placement.ReservoirCoordinates, neuron.Placement.ReservoirCoordinates) : 0
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
                    InternalSynapseSettings.DynamicsSettings dynamicsSettings = synapseCfg.GetDynamicsSettings(nccSource.Neuron.TypeOfActivation,
                                                                                                               nccSource.Neuron.Role,
                                                                                                               tmpRelTargetNeuronCollection[targetNeuronIndex].Neuron.TypeOfActivation,
                                                                                                               tmpRelTargetNeuronCollection[targetNeuronIndex].Neuron.Role
                                                                                                               );
                    ISynapse synapse = new InternalSynapse(nccSource.Neuron,
                                                           tmpRelTargetNeuronCollection[targetNeuronIndex].Neuron,
                                                           threadRandObj.NextDouble(dynamicsSettings.WeightCfg),
                                                           dynamicsSettings.TauFacilitation,
                                                           dynamicsSettings.TauDepression,
                                                           dynamicsSettings.RestingEfficacy,
                                                           dynamicsSettings.ApplyShortTermPlasticity
                                                           );
                    SetInterconnection(_neuronNeuronConnectionsCollection, synapse, replaceExistingConnections);
                    //Remove targetNeuron from tmp collection
                    tmpRelTargetNeuronCollection.RemoveAt(targetNeuronIndex);
                }//connNum
            });
            return;
        }

        private void ConnectRandomSchema(int poolID, Random rand, PoolSettings.InterconnectionSettings.RandomSchemaSettings settings)
        {
            PoolSettings poolSettings = InstanceDefinition.Settings.PoolSettingsCollection[poolID];
            for (int repetition = 1; repetition <= settings.Repetitions; repetition++)
            {
                //Determine connection counts
                int intendedNumOfSynapses = (int)(Math.Round(((double)poolSettings.Dim.Size)).Power(2) * settings.Density);
                int countE2E = (int)Math.Round(settings.RatioEE * intendedNumOfSynapses);
                int countE2I = (int)Math.Round(settings.RatioEI * intendedNumOfSynapses);
                int countI2E = (int)Math.Round(settings.RatioIE * intendedNumOfSynapses);
                int countI2I = (int)Math.Round(settings.RatioII * intendedNumOfSynapses);
                //Connections E2E
                InterconnectPoolNeurons(rand,
                                        poolID,
                                        NeuronCommon.NeuronRole.Excitatory,
                                        NeuronCommon.NeuronRole.Excitatory,
                                        countE2E,
                                        settings.AvgDistance,
                                        settings.AllowSelfConnection,
                                        settings.ConstantNumOfConnections,
                                        settings.ReplaceExistingConnections,
                                        settings.SynapseCfg
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
                                        settings.ReplaceExistingConnections,
                                        settings.SynapseCfg
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
                                        settings.ReplaceExistingConnections,
                                        settings.SynapseCfg
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
                                        settings.ReplaceExistingConnections,
                                        settings.SynapseCfg
                                        );
            }
            return;
        }

        private void ConnectChainSchema(int poolID, Random rand, PoolSettings.InterconnectionSettings.ChainSchemaSettings settings)
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
                    InternalSynapseSettings.DynamicsSettings dynamicsSettings = settings.SynapseCfg.GetDynamicsSettings(connPair.Item1.TypeOfActivation,
                                                                                                                        connPair.Item1.Role,
                                                                                                                        connPair.Item2.TypeOfActivation,
                                                                                                                        connPair.Item2.Role
                                                                                                                        );
                    ISynapse synapse = new InternalSynapse(connPair.Item1,
                                                           connPair.Item2,
                                                           rand.NextDouble(dynamicsSettings.WeightCfg),
                                                           dynamicsSettings.TauFacilitation,
                                                           dynamicsSettings.TauDepression,
                                                           dynamicsSettings.RestingEfficacy,
                                                           dynamicsSettings.ApplyShortTermPlasticity
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
                                    bool constantNumOfNeuronConnections,
                                    InternalSynapseSettings synapseCfg
                                    )
        {
            //Initial condition
            if (totalNumOfConnections <= 0 || numOfSourceNeurons == 0)
            {
                return;
            }
            PoolSettings sourcePoolSettings = InstanceDefinition.Settings.PoolSettingsCollection[sourcePoolID];
            PoolSettings targetPoolSettings = InstanceDefinition.Settings.PoolSettingsCollection[targetPoolID];
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
                    InternalSynapseSettings.DynamicsSettings dynamicsSettings = synapseCfg.GetDynamicsSettings(nccSource.Neuron.TypeOfActivation,
                                                                                                               nccSource.Neuron.Role,
                                                                                                               tmpRelTargetNeuronCollection[targetNeuronIndex].Neuron.TypeOfActivation,
                                                                                                               tmpRelTargetNeuronCollection[targetNeuronIndex].Neuron.Role
                                                                                                               );
                    ISynapse synapse = new InternalSynapse(nccSource.Neuron,
                                                           tmpRelTargetNeuronCollection[targetNeuronIndex].Neuron,
                                                           threadRandObj.NextDouble(dynamicsSettings.WeightCfg),
                                                           dynamicsSettings.TauFacilitation,
                                                           dynamicsSettings.TauDepression,
                                                           dynamicsSettings.RestingEfficacy,
                                                           dynamicsSettings.ApplyShortTermPlasticity
                                                           );
                    SetInterconnection(_neuronNeuronConnectionsCollection, synapse);
                    //Remove targetNeuron from tmp collection
                    tmpRelTargetNeuronCollection.RemoveAt(targetNeuronIndex);
                }//connNum
            });
            return;
        }

        private void SetPool2PoolInterconnections(Random rand, ReservoirSettings.PoolsInterconnection cfg)
        {
            PoolSettings sourcePoolSettings = InstanceDefinition.Settings.PoolSettingsCollection[cfg.SourcePoolID];
            PoolSettings targetPoolSettings = InstanceDefinition.Settings.PoolSettingsCollection[cfg.TargetPoolID];
            //Determine counts
            int totalNumOfSourceNeurons = (int)(Math.Round(((double)sourcePoolSettings.Dim.Size)) * cfg.SourceConnectionDensity);
            double numOfTargetNeuronsPerSourceNeuron = ((double)targetPoolSettings.Dim.Size) * cfg.TargetConnectionDensity;
            int totalNumOfSynapses = (int)(Math.Round(totalNumOfSourceNeurons * numOfTargetNeuronsPerSourceNeuron));
            int countE2E = (int)Math.Round(cfg.RatioEE * totalNumOfSynapses);
            int countE2I = (int)Math.Round(cfg.RatioEI * totalNumOfSynapses);
            int countI2E = (int)Math.Round(cfg.RatioIE * totalNumOfSynapses);
            int countI2I = (int)Math.Round(cfg.RatioII * totalNumOfSynapses);
            //Connections E2E
            ConnectNeurons(rand,
                           cfg.SourcePoolID,
                           NeuronCommon.NeuronRole.Excitatory,
                           (int)Math.Round(countE2E/ numOfTargetNeuronsPerSourceNeuron),
                           cfg.TargetPoolID,
                           NeuronCommon.NeuronRole.Excitatory,
                           countE2E,
                           cfg.ConstantNumOfConnections,
                           cfg.SynapseCfg
                           );
            //Connections E2I
            ConnectNeurons(rand,
                           cfg.SourcePoolID,
                           NeuronCommon.NeuronRole.Excitatory,
                           (int)Math.Round(countE2I / numOfTargetNeuronsPerSourceNeuron),
                           cfg.TargetPoolID,
                           NeuronCommon.NeuronRole.Inhibitory,
                           countE2I,
                           cfg.ConstantNumOfConnections,
                           cfg.SynapseCfg
                           );
            //Connections I2E
            ConnectNeurons(rand,
                           cfg.SourcePoolID,
                           NeuronCommon.NeuronRole.Inhibitory,
                           (int)Math.Round(countI2E / numOfTargetNeuronsPerSourceNeuron),
                           cfg.TargetPoolID,
                           NeuronCommon.NeuronRole.Excitatory,
                           countI2E,
                           cfg.ConstantNumOfConnections,
                           cfg.SynapseCfg
                           );
            //Connections I2I
            ConnectNeurons(rand,
                           cfg.SourcePoolID,
                           NeuronCommon.NeuronRole.Inhibitory,
                           (int)Math.Round(countI2I / numOfTargetNeuronsPerSourceNeuron),
                           cfg.TargetPoolID,
                           NeuronCommon.NeuronRole.Inhibitory,
                           countI2I,
                           cfg.ConstantNumOfConnections,
                           cfg.SynapseCfg
                           );
            return;
        }

        /// <summary>
        /// Collects key statistics related to reservoir neurons states
        /// </summary>
        public ReservoirStat CollectStatistics()
        {
            ReservoirStat stats = new ReservoirStat(InstanceDefinition.InstanceName,
                                                    InstanceDefinition.Settings.SettingsName,
                                                    Size,
                                                    _reservoirExcitatoryNeuronsRatio,
                                                    NumOfPredictors,
                                                    NumOfInternalSynapses
                                                    );
            int poolID = 0;
            foreach (PoolSettings poolSettings in InstanceDefinition.Settings.PoolSettingsCollection)
            {
                ReservoirStat.PoolStat poolStat = new ReservoirStat.PoolStat(poolSettings, _poolNeuronsCollection[poolID].Length, _poolExcitatoryNeuronsRatioCollection[poolID]);
                //Neurons statistics
                foreach (HiddenNeuron neuron in _poolNeuronsCollection[poolID])
                {
                    poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].AvgActivationStatesStat.AddSampleValue(neuron.Statistics.ActivationStat.ArithAvg);
                    poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].MaxActivationStatesStat.AddSampleValue(neuron.Statistics.ActivationStat.Max);
                    poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].MinActivationStatesStat.AddSampleValue(neuron.Statistics.ActivationStat.Min);
                    poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].ActivationStateSpansStat.AddSampleValue(neuron.Statistics.ActivationStat.Span);
                    if (neuron.Statistics.InputStimuliStat.NumOfNonzeroSamples > 0)
                    {
                        poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].AvgIStimuliStat.AddSampleValue(neuron.Statistics.InputStimuliStat.ArithAvg);
                        poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].MaxIStimuliStat.AddSampleValue(neuron.Statistics.InputStimuliStat.Max);
                        poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].MinIStimuliStat.AddSampleValue(neuron.Statistics.InputStimuliStat.Min);
                        poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].IStimuliSpansStat.AddSampleValue(neuron.Statistics.InputStimuliStat.Span);
                    }
                    if (neuron.Statistics.ReservoirStimuliStat.NumOfNonzeroSamples > 0)
                    {
                        poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].AvgRStimuliStat.AddSampleValue(neuron.Statistics.ReservoirStimuliStat.ArithAvg);
                        poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].MaxRStimuliStat.AddSampleValue(neuron.Statistics.ReservoirStimuliStat.Max);
                        poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].MinRStimuliStat.AddSampleValue(neuron.Statistics.ReservoirStimuliStat.Min);
                        poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].RStimuliSpansStat.AddSampleValue(neuron.Statistics.ReservoirStimuliStat.Span);
                    }
                    if (neuron.Statistics.TotalStimuliStat.NumOfNonzeroSamples > 0)
                    {
                        poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].AvgTStimuliStat.AddSampleValue(neuron.Statistics.TotalStimuliStat.ArithAvg);
                        poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].MaxTStimuliStat.AddSampleValue(neuron.Statistics.TotalStimuliStat.Max);
                        poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].MinTStimuliStat.AddSampleValue(neuron.Statistics.TotalStimuliStat.Min);
                        poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].TStimuliSpansStat.AddSampleValue(neuron.Statistics.TotalStimuliStat.Span);
                    }
                    if (neuron.Statistics.AnalogSignalStat.NumOfNonzeroSamples > 0)
                    {
                        poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].AvgAnalogSignalStat.AddSampleValue(neuron.Statistics.AnalogSignalStat.ArithAvg);
                        poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].MaxAnalogSignalStat.AddSampleValue(neuron.Statistics.AnalogSignalStat.Max);
                        poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].MinAnalogSignalStat.AddSampleValue(neuron.Statistics.AnalogSignalStat.Min);
                    }
                    if (neuron.Statistics.FiringStat.NumOfNonzeroSamples > 0)
                    {
                        poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].AvgFiringStat.AddSampleValue(neuron.Statistics.FiringStat.ArithAvg);
                        poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].MaxFiringStat.AddSampleValue(neuron.Statistics.FiringStat.Max);
                        poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].MinFiringStat.AddSampleValue(neuron.Statistics.FiringStat.Min);
                    }
                    //Synapses efficacy statistics
                    foreach (ISynapse rSynapse in _neuronNeuronConnectionsCollection[neuron.Placement.ReservoirFlatIdx].Values)
                    {
                        if (rSynapse.EfficacyStat.NumOfSamples > 0)
                        {
                            poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].AvgSynEfficacyStat.AddSampleValue(rSynapse.EfficacyStat.ArithAvg);
                            poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].MaxSynEfficacyStat.AddSampleValue(rSynapse.EfficacyStat.Max);
                            poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].MinSynEfficacyStat.AddSampleValue(rSynapse.EfficacyStat.Min);
                            poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].SynEfficacySpansStat.AddSampleValue(rSynapse.EfficacyStat.Span);
                        }
                    }
                    if (neuron.Statistics.ReservoirStimuliStat.NumOfNonzeroSamples == 0)
                    {
                        ++poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].NumOfNoRStimuliNeurons;
                        ++poolStat.NumOfNoRStimuliNeurons;
                        ++stats.NumOfNoRStimuliNeurons;
                    }
                    if (neuron.Statistics.AnalogSignalStat.NumOfNonzeroSamples == 0)
                    {
                        ++poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].NumOfNoAnalogOutputSignalNeurons;
                        ++poolStat.NumOfNoAnalogOutputSignalNeurons;
                        ++stats.NumOfNoAnalogOutputSignalNeurons;
                    }
                    if (neuron.Statistics.AnalogSignalStat.Span == 0 && neuron.Statistics.AnalogSignalStat.Max != 0)
                    {
                        ++poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].NumOfConstAnalogOutputSignalNeurons;
                        ++poolStat.NumOfConstAnalogOutputSignalNeurons;
                        ++stats.NumOfConstAnalogOutputSignalNeurons;
                    }

                    if (neuron.Statistics.FiringStat.NumOfNonzeroSamples == 0)
                    {
                        ++poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].NumOfNotFiringNeurons;
                        ++poolStat.NumOfNotFiringNeurons;
                        ++stats.NumOfNotFiringNeurons;
                    }
                    if (neuron.Statistics.FiringStat.Span == 0 && neuron.Statistics.FiringStat.Max != 0)
                    {
                        ++poolStat.NeuronGroupStatCollection[neuron.Placement.PoolGroupID].NumOfConstFiringNeurons;
                        ++poolStat.NumOfConstFiringNeurons;
                        ++stats.NumOfConstFiringNeurons;
                    }
                }
                //Weights statistics
                //Input weights
                foreach (SortedList<int, ISynapse> synapses in _neuronInputConnectionsCollection)
                {
                    foreach (ISynapse synapse in synapses.Values)
                    {
                        if (synapse.TargetNeuron.Placement.PoolID == poolID)
                        {
                            poolStat.InputWeightsStat.AddSampleValue(synapse.Weight);
                        }
                    }
                }
                //Internal weights
                foreach (SortedList<int, ISynapse> synapses in _neuronNeuronConnectionsCollection)
                {
                    foreach (ISynapse synapse in synapses.Values)
                    {
                        if (synapse.TargetNeuron.Placement.PoolID == poolID)
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
            foreach(InputUnit inputUnit in _inputUnitCollection)
            {
                inputUnit.Reset(resetStatistics);
            }
            //Reservoir neurons and all linked synapses
            Parallel.For(0, _reservoirNeuronCollection.Length, n =>
            {
                _reservoirNeuronCollection[n].Reset(resetStatistics);
                //Linked input synapses
                foreach (ISynapse synapse in _neuronInputConnectionsCollection[n].Values)
                {
                    synapse.Reset(resetStatistics);
                }
                //Linked internal synapses
                foreach (ISynapse synapse in _neuronNeuronConnectionsCollection[n].Values)
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
                _inputUnitCollection[i].NewStimulation(input[i]);
                _inputUnitCollection[i].ComputeSignal(updateStatistics);
            }
            //Perform reservoir's computation cycle
            //Collect new stimulation for each reservoir neuron
            Parallel.ForEach(_parallelRanges, range =>
            {
                for (int neuronIdx = range.Item1; neuronIdx < range.Item2; neuronIdx++)
                {
                    //Stimulation from input neurons
                    double iStimuli = 0;
                    foreach (ISynapse synapse in _neuronInputConnectionsCollection[neuronIdx].Values)
                    {
                        iStimuli += synapse.GetSignal(updateStatistics);
                    }
                    //Stimulation from connected reservoir neurons
                    double rStimuli = 0;
                    foreach (ISynapse synapse in _neuronNeuronConnectionsCollection[neuronIdx].Values)
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
                    _reservoirNeuronCollection[neuronIdx].ComputeSignal(updateStatistics);
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
            public HiddenNeuronPredictorsSettings PredictorsCfg { get; set; }
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
                NeuronGroupStatCollection = new NeuronGroupStat[poolSettings.NeuronGroups.Count];
                for(int i = 0; i < poolSettings.NeuronGroups.Count; i++)
                {
                    NeuronGroupStatCollection[i] = new NeuronGroupStat(poolSettings.NeuronGroups[i].Name);
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
