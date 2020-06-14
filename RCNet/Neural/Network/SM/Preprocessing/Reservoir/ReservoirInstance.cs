using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.MathTools.MatrixMath;
using RCNet.MathTools.Probability;
using RCNet.Neural.Activation;
using RCNet.Neural.Network.SM.Preprocessing.Input;
using RCNet.Neural.Network.SM.Preprocessing.Neuron;
using RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool.NeuronGroup;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        /// Reservoir's instance ID
        /// </summary>
        public int InstanceID { get; }

        /// <summary>
        /// Reservoir's structure configuration
        /// </summary>
        public ReservoirStructureSettings StructureCfg { get; }

        /// <summary>
        /// Reservoir's instance configuration
        /// </summary>
        public ReservoirInstanceSettings InstanceCfg { get; }

        /// <summary>
        /// Neurons providing predictors
        /// </summary>
        public List<INeuron> PredictingNeuronCollection { get; }

        /// <summary>
        /// Number of reservoir's predictors
        /// </summary>
        public int NumOfPredictors { get; }


        //Attributes
        /// <summary>
        /// Neurons within the pools.
        /// </summary>
        private readonly List<HiddenNeuron[]> _poolNeuronCollection;
        /// <summary>
        /// Reservoir's all internal neurons (flat structure).
        /// </summary>
        private readonly HiddenNeuron[] _reservoirNeuronCollection;
        /// <summary>
        /// Input connections
        /// </summary>
        private readonly SortedList<int, Synapse>[] _neuronInputConnectionsCollection;
        /// <summary>
        /// Internal connections
        /// </summary>
        private readonly SortedList<int, Synapse>[] _neuronNeuronConnectionsCollection;
        /// <summary>
        /// Prepared ranges for parallel processing
        /// </summary>
        private readonly List<Tuple<int, int>> _parallelRanges;
        /// <summary>
        /// Total number of analog hidden neurons
        /// </summary>
        private readonly int _numOfAnalogNeurons;
        /// <summary>
        /// Total number of spiking hidden neurons
        /// </summary>
        private readonly int _numOfSpikingNeurons;
        /// <summary>
        /// Input distances statistics
        /// </summary>
        private readonly BasicStat _inputDistancesStat;
        /// <summary>
        /// Internal distances statistics
        /// </summary>
        private readonly BasicStat _internalDistancesStat;


        //Constructor
        /// <summary>
        /// Instantiates the reservoir
        /// </summary>
        /// <param name="instanceID">ID of the reservoir instance</param>
        /// <param name="structureCfg">Reservoir structure configuration</param>
        /// <param name="instanceCfg">Reservoir instance configuration</param>
        /// <param name="inputEncoder">Input encoder</param>
        /// <param name="rand">Random object to be used for random part of the initialization</param>
        public ReservoirInstance(int instanceID,
                                 ReservoirStructureSettings structureCfg,
                                 ReservoirInstanceSettings instanceCfg,
                                 InputEncoder inputEncoder,
                                 Random rand
                                 )
        {
            InstanceID = instanceID;
            //Copy settings
            StructureCfg = (ReservoirStructureSettings)structureCfg.DeepClone();
            InstanceCfg = (ReservoirInstanceSettings)instanceCfg.DeepClone();
            //-----------------------------------------------------------------------------
            //Reservoir neurons
            //-----------------------------------------------------------------------------
            int neuronReservoirFlatIdx = 0;
            _numOfAnalogNeurons = 0;
            _numOfSpikingNeurons = 0;
            List<HiddenNeuron> allNeurons = new List<HiddenNeuron>();
            _poolNeuronCollection = new List<HiddenNeuron[]>(StructureCfg.PoolsCfg.PoolCfgCollection.Count);
            PredictingNeuronCollection = new List<INeuron>();
            for (int poolID = 0; poolID < StructureCfg.PoolsCfg.PoolCfgCollection.Count; poolID++)
            {
                PoolSettings poolSettings = StructureCfg.PoolsCfg.PoolCfgCollection[poolID];
                //------------------------------------------------------------------------------------
                //Neuron groups within the pool
                int groupID = 0, idx = 0;
                List<NeuronCreationParams> neuronParamCollection = new List<NeuronCreationParams>();
                foreach (INeuronGroupSettings ngs in poolSettings.NeuronGroupsCfg.GroupCfgCollection)
                {
                    NeuronCreationParams[] grpNCP = new NeuronCreationParams[ngs.Count];
                    PredictorsSettings predictorsCfg = new PredictorsSettings(ngs.PredictorsCfg, poolSettings.PredictorsCfg, InstanceCfg.PredictorsCfg);
                    //Group neuron params
                    for (int i = 0; i < ngs.Count; i++)
                    {
                        grpNCP[i] = new NeuronCreationParams
                        {
                            SignalingRestriction = ngs.SignalingRestriction,
                            Activation = ActivationFactory.Create(ngs.ActivationCfg, rand),
                            Bias = ngs.BiasCfg == null ? 0 : rand.NextDouble(ngs.BiasCfg),
                            GroupID = groupID,
                            AnalogFiringThreshold = ngs.Type == ActivationType.Spiking ? -1 : ((AnalogNeuronGroupSettings)ngs).FiringThreshold,
                            RetainmentStrength = 0,
                            PredictorsCfg = null
                        };
                        if (ngs.Type == ActivationType.Analog)
                        {
                            ++_numOfAnalogNeurons;
                        }
                        else
                        {
                            ++_numOfSpikingNeurons;
                        }
                        neuronParamCollection.Add(grpNCP[i]);
                        ++idx;
                    }
                    //Retainment
                    if (ngs.Type == ActivationType.Analog && ((AnalogNeuronGroupSettings)ngs).RetainmentCfg != null)
                    {
                        int numOfRetNeurons = (int)Math.Round(((AnalogNeuronGroupSettings)ngs).RetainmentCfg.Density * ngs.Count, 0);
                        rand.Shuffle(grpNCP);
                        for (int i = 0; i < numOfRetNeurons; i++)
                        {
                            grpNCP[i].RetainmentStrength = rand.NextDouble(((AnalogNeuronGroupSettings)ngs).RetainmentCfg.StrengthCfg);
                        }
                    }
                    //Readout
                    if (predictorsCfg.NumOfEnabledPredictors > 0)
                    {
                        for (int i = 0; i < ngs.Count; i++)
                        {
                            grpNCP[i].PredictorsCfg = predictorsCfg;
                        }
                    }
                    ++groupID;
                }//ngs
                //Randomize order before sequential instantiation
                rand.Shuffle(neuronParamCollection);
                //Instantiate neurons
                HiddenNeuron[] poolNeurons = new HiddenNeuron[poolSettings.ProportionsCfg.Size];
                int neuronPoolFlatIdx = 0;
                for (int x = 0; x < poolSettings.ProportionsCfg.DimX; x++)
                {
                    for (int y = 0; y < poolSettings.ProportionsCfg.DimY; y++)
                    {
                        for (int z = 0; z < poolSettings.ProportionsCfg.DimZ; z++)
                        {
                            NeuronLocation location = new NeuronLocation(InstanceID, neuronReservoirFlatIdx, poolID, neuronPoolFlatIdx, neuronParamCollection[neuronPoolFlatIdx].GroupID, poolSettings.CoordinatesCfg.X + x, poolSettings.CoordinatesCfg.Y + y, poolSettings.CoordinatesCfg.Z + z);
                            //Neuron instance
                            if (neuronParamCollection[neuronPoolFlatIdx].Activation.TypeOfActivation == ActivationType.Analog)
                            {
                                //Use constructor for hidden neuron having analog activation
                                poolNeurons[neuronPoolFlatIdx] = new HiddenNeuron(location,
                                                                                  neuronParamCollection[neuronPoolFlatIdx].Activation,
                                                                                  neuronParamCollection[neuronPoolFlatIdx].Bias,
                                                                                  neuronParamCollection[neuronPoolFlatIdx].AnalogFiringThreshold,
                                                                                  neuronParamCollection[neuronPoolFlatIdx].RetainmentStrength,
                                                                                  neuronParamCollection[neuronPoolFlatIdx].SignalingRestriction,
                                                                                  neuronParamCollection[neuronPoolFlatIdx].PredictorsCfg
                                                                                  );
                            }
                            else
                            {
                                //Use constructor for hidden neuron having spiking activation
                                poolNeurons[neuronPoolFlatIdx] = new HiddenNeuron(location,
                                                                                  neuronParamCollection[neuronPoolFlatIdx].Activation,
                                                                                  neuronParamCollection[neuronPoolFlatIdx].Bias,
                                                                                  neuronParamCollection[neuronPoolFlatIdx].PredictorsCfg
                                                                                  );
                            }
                            allNeurons.Add(poolNeurons[neuronPoolFlatIdx]);
                            //Predictor
                            if (poolNeurons[neuronPoolFlatIdx].NumOfEnabledPredictors > 0)
                            {
                                PredictingNeuronCollection.Add(poolNeurons[neuronPoolFlatIdx]);
                                NumOfPredictors += poolNeurons[neuronPoolFlatIdx].NumOfEnabledPredictors;
                            }
                            ++neuronPoolFlatIdx;
                            ++neuronReservoirFlatIdx;
                        }//z
                    }//y
                }//x
                _poolNeuronCollection.Add(poolNeurons);
            }//PoolID
            //All neurons flat structure
            _reservoirNeuronCollection = allNeurons.ToArray();
            //Parallel processing ranges
            var rangePartitioner = Partitioner.Create(0, _reservoirNeuronCollection.Length);
            _parallelRanges = new List<Tuple<int, int>>(rangePartitioner.GetDynamicPartitions());

            //-----------------------------------------------------------------------------
            //Connection banks
            //-----------------------------------------------------------------------------
            _inputDistancesStat = new BasicStat(false);
            _internalDistancesStat = new BasicStat(true);
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
            foreach (InputConnSettings inputConnCfg in InstanceCfg.InputConnsCfg.ConnCfgCollection)
            {
                ConnectInput(inputEncoder, inputConnCfg, rand);
            }

            //-----------------------------------------------------------------------------
            //Pools interconnection
            for (int poolID = 0; poolID < _poolNeuronCollection.Count; poolID++)
            {
                //Apply defined schemas
                foreach (object connSchema in StructureCfg.PoolsCfg.PoolCfgCollection[poolID].InterconnectionCfg.SchemaCfgCollection)
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
                        throw new InvalidOperationException($"Unsupported interconnection schema {connSchema.GetType().Name}.");
                    }
                }
            }

            //-----------------------------------------------------------------------------
            //Pool to pool connections
            if (StructureCfg.InterPoolConnectionsCfg != null)
            {
                foreach (InterPoolConnSettings interPoolConnectionCfg in StructureCfg.InterPoolConnectionsCfg.InterPoolConnectionCfgCollection)
                {
                    SetInterPoolConnection(rand, interPoolConnectionCfg);
                }
            }

            //-----------------------------------------------------------------------------
            //Delay
            //Setup delay on input synapses
            foreach (SortedList<int, Synapse> synapses in _neuronInputConnectionsCollection)
            {
                foreach (Synapse synapse in synapses.Values)
                {
                    synapse.SetupDelay(_inputDistancesStat, rand);
                }
            }
            //Setup delay on internal synapses
            foreach (SortedList<int, Synapse> synapses in _neuronNeuronConnectionsCollection)
            {
                foreach (Synapse synapse in synapses.Values)
                {
                    synapse.SetupDelay(_internalDistancesStat, rand);
                }
            }


            //-----------------------------------------------------------------------------
            //Weights adjustment
            //Analog neurons
            if (_numOfAnalogNeurons > 0)
            {
                //Analog input
                AdjustAnalogInputStrength();
                //Spectral radius
                if (InstanceCfg.SynapseCfg.AnalogTargetCfg.SpectralRadius != SynapseATSettings.NASpectralRadiusNum)
                {
                    ApplySpectralRadius(InstanceCfg.SynapseCfg.AnalogTargetCfg.SpectralRadius);
                }
            }
            //Spiking neurons
            if (_numOfSpikingNeurons > 0)
            {
                //Homogenous excitability
                ApplyHomogenousExcitability();
            }

            //Finished
            return;
        }

        //Properties
        /// <summary>
        /// Reservoir size. (Number of neurons within the reservoir)
        /// </summary>
        public int Size { get { return _reservoirNeuronCollection.Length; } }

        //Methods
        /// <summary>
        /// Estimates necessary number of cycles to make reservoir's state and predictors useable
        /// </summary>
        public int GetDefaultBootCycles()
        {
            int bootCycles = StructureCfg.LargestInterconnectedAreaSize;
            foreach (HiddenNeuron neuron in _reservoirNeuronCollection)
            {
                bootCycles = Math.Max(bootCycles, neuron.RequiredHistLength);
            }
            return bootCycles;
        }

        /// <summary>
        /// Returns collection of predictor descriptor objects related to predictors from hidden neurons
        /// </summary>
        public List<PredictorDescriptor> GetPredictorsDescriptors()
        {
            List<PredictorDescriptor> result = new List<PredictorDescriptor>(NumOfPredictors);
            foreach (INeuron neuron in PredictingNeuronCollection)
            {
                if (neuron.NumOfEnabledPredictors > 0)
                {
                    foreach (PredictorsProvider.PredictorID id in neuron.GetEnabledPredictorsIDs())
                    {
                        result.Add(new PredictorDescriptor(neuron.Location.ReservoirID, neuron.Location.PoolID, id));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Scales weights of synapses targeting analog neurons to achieve requiered spectral radius
        /// </summary>
        private void ApplySpectralRadius(double spectralRadius)
        {
            if (_numOfAnalogNeurons > 0)
            {
                //Select target neurons
                List<HiddenNeuron> scopeNeurons = new List<HiddenNeuron>(from neuron in _reservoirNeuronCollection
                                                                         where neuron.TypeOfActivation == ActivationType.Analog
                                                                         select neuron);
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
                    throw new InvalidOperationException($"Can't apply SpectralRadius. Invalid analog weights, largest eigenvalue is 0.");
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
            }
            return;
        }

        /// <summary>
        /// Function sets homogenous excitability of spiking hidden neurons
        /// </summary>
        private void ApplyHomogenousExcitability()
        {
            List<HiddenNeuron> spikingNeurons = new List<HiddenNeuron>(from neuron in _reservoirNeuronCollection where neuron.TypeOfActivation == ActivationType.Spiking select neuron);
            foreach (HiddenNeuron neuron in spikingNeurons)
            {
                HomogenousExcitabilitySettings homogenousExcitabilityCfg = ((SpikingNeuronGroupSettings)StructureCfg.PoolsCfg.PoolCfgCollection[neuron.Location.PoolID].NeuronGroupsCfg.GroupCfgCollection[neuron.Location.PoolGroupID]).HomogenousExcitabilityCfg;
                double sumOfInputWeights = 0d, sumOfExcitatoryWeights = 0d, sumOfInhibitoryWeights = 0d;
                //Scan input synapses
                foreach (Synapse synapse in _neuronInputConnectionsCollection[neuron.Location.ReservoirFlatIdx].Values)
                {
                    sumOfInputWeights += Math.Abs(synapse.Weight);
                }
                //Scan Excitatory and Inhibitory synapses
                foreach (Synapse synapse in _neuronNeuronConnectionsCollection[neuron.Location.ReservoirFlatIdx].Values)
                {
                    if (synapse.Role == Synapse.SynRole.Excitatory)
                    {
                        sumOfExcitatoryWeights += synapse.Weight;
                    }
                    else
                    {
                        sumOfInhibitoryWeights += Math.Abs(synapse.Weight);
                    }
                }
                //Check consistency
                if ((sumOfInputWeights + sumOfExcitatoryWeights) == 0)
                {
                    throw new InvalidOperationException($"Can't set homogenous excitability. Hidden neuron has no excitatory synapse.");
                }
                //Rescale input synapses
                double targetSumOfInputWeights = homogenousExcitabilityCfg.ExcitatoryStrength * homogenousExcitabilityCfg.InputRatio;
                if (sumOfInputWeights > 0)
                {
                    double factor = targetSumOfInputWeights / sumOfInputWeights;
                    foreach (Synapse synapse in _neuronInputConnectionsCollection[neuron.Location.ReservoirFlatIdx].Values)
                    {
                        synapse.Rescale(factor);
                    }
                }
                //Rescale excitatory synapses
                double targetSumOfExcitatoryWeights = sumOfInputWeights == 0 ? homogenousExcitabilityCfg.ExcitatoryStrength : homogenousExcitabilityCfg.ExcitatoryStrength - targetSumOfInputWeights;
                if (sumOfExcitatoryWeights > 0)
                {
                    double factor = targetSumOfExcitatoryWeights / sumOfExcitatoryWeights;
                    foreach (Synapse synapse in _neuronNeuronConnectionsCollection[neuron.Location.ReservoirFlatIdx].Values)
                    {
                        if (synapse.Role == Synapse.SynRole.Excitatory)
                        {
                            synapse.Rescale(factor);
                        }
                    }
                }
                //Rescale inhibitory synapses
                double targetSumOfInhibitoryWeights = homogenousExcitabilityCfg.ExcitatoryStrength * homogenousExcitabilityCfg.InhibitoryRatio;
                if (sumOfInhibitoryWeights > 0)
                {
                    double factor = targetSumOfInhibitoryWeights / sumOfInhibitoryWeights;
                    foreach (Synapse synapse in _neuronNeuronConnectionsCollection[neuron.Location.ReservoirFlatIdx].Values)
                    {
                        if (synapse.Role == Synapse.SynRole.Inhibitory)
                        {
                            synapse.Rescale(factor);
                        }
                    }
                }
            }
            return;
        }

        /// <summary>
        /// Function adjusts weights of input synapses to analog neurons to ensure theirs sum does not exceed Max
        /// </summary>
        private void AdjustAnalogInputStrength()
        {
            //Loop neuron by neuron
            for (int nIdx = 0; nIdx < _reservoirNeuronCollection.Length; nIdx++)
            {
                //Adjust input total strength of hidden neuron having analog activation
                if (_reservoirNeuronCollection[nIdx].TypeOfActivation == ActivationType.Analog)
                {
                    //Adjust input synapses
                    if (_neuronInputConnectionsCollection[nIdx].Values.Count > 1)
                    {
                        double factor = 1d / _neuronInputConnectionsCollection[nIdx].Values.Count;
                        foreach (Synapse synapse in _neuronInputConnectionsCollection[nIdx].Values)
                        {
                            synapse.Rescale(factor);
                        }
                    }
                }
            }
            return;
        }

        /// <summary>
        /// This general function adds the synapse into the given connections bank.
        /// </summary>
        /// <param name="connectionsCollection">Bank of connections</param>
        /// <param name="synapse">A synapse to be added into the bank</param>
        /// <param name="replace">Specifies whether to replace existing connection</param>
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
                    if (replace)
                    {
                        connectionsCollection[synapse.TargetNeuron.Location.ReservoirFlatIdx][synapse.SourceNeuron.Location.ReservoirFlatIdx] = synapse;
                        return true;
                    }
                    return false;
                }
            }
        }

        /// <summary>
        /// Creates input synapses from input neurons to hidden neurons
        /// </summary>
        /// <param name="inputEncoder">Input encoder instance</param>
        /// <param name="inputConnCfg">Input connection configuration</param>
        /// <param name="rand">Random object to be used</param>
        private void ConnectInput(InputEncoder inputEncoder, InputConnSettings inputConnCfg, Random rand)
        {
            InputField inputField = inputEncoder.GetVaryingInputField(inputConnCfg.InputFieldName);
            //Target pool index
            int targetPoolID = StructureCfg.PoolsCfg.GetPoolID(inputConnCfg.PoolName);
            //Select available target neurons according to connection's target scope configuration
            List<HiddenNeuron>[] targetNeuronsByActivation = new List<HiddenNeuron>[Enum.GetValues(typeof(ActivationType)).Length];
            //Spiking target scope
            targetNeuronsByActivation[(int)ActivationType.Spiking] = new List<HiddenNeuron>(from neuron in _poolNeuronCollection[targetPoolID]
                                                                                            where (neuron.TypeOfActivation == ActivationType.Spiking &&
                                                                                                    inputConnCfg.SpikingTargetDensity > 0)
                                                                                            select neuron
                                                                                            );
            //Analog target scope
            targetNeuronsByActivation[(int)ActivationType.Analog] = new List<HiddenNeuron>(from neuron in _poolNeuronCollection[targetPoolID]
                                                                                           where (neuron.TypeOfActivation == ActivationType.Analog && inputConnCfg.AnalogTargetDensity > 0)
                                                                                           select neuron
                                                                                           );

            //Loop through target activation scope
            for (int activationType = 0; activationType < targetNeuronsByActivation.Length; activationType++)
            {
                //Density
                double density = activationType == (int)ActivationType.Spiking ? inputConnCfg.SpikingTargetDensity : inputConnCfg.AnalogTargetDensity;
                //Available target neurons
                List<HiddenNeuron> targetNeurons = targetNeuronsByActivation[activationType];
                rand.Shuffle(targetNeurons);
                //Limit target neurons 
                int targetNeuronsCount = (int)Math.Round(targetNeurons.Count * density, 0);
                if (targetNeuronsCount < targetNeurons.Count)
                {
                    targetNeurons.RemoveRange(targetNeuronsCount, targetNeurons.Count - targetNeuronsCount);
                }
                if (targetNeurons.Count > 0)
                {
                    if (inputConnCfg.SignalingRestriction == NeuronCommon.NeuronSignalingRestrictionType.AnalogOnly ||
                        (activationType == (int)ActivationType.Analog && inputConnCfg.SignalingRestriction == NeuronCommon.NeuronSignalingRestrictionType.NoRestriction))
                    {
                        //Analog synapses
                        for (int i = 0; i < targetNeurons.Count; i++)
                        {
                            //Create synapse
                            Synapse synapse = new Synapse(inputField.AnalogNeuron,
                                                          targetNeurons[i],
                                                          Synapse.SynRole.Input,
                                                          InstanceCfg.SynapseCfg,
                                                          rand
                                                          );
                            _inputDistancesStat.AddSampleValue(synapse.Distance);
                            SetInterconnection(_neuronInputConnectionsCollection, synapse);
                        }//for i
                    }//Analog synapses
                    else
                    {
                        //Spiking synapses
                        List<int[]> plannedConnCmbIdxs = inputField.GetSpikingInputCombinations(targetNeurons.Count);
                        bool[] inputNeuronUsageChecker = new bool[inputField.SpikingNeuronCollection.Length];
                        inputNeuronUsageChecker.Populate(false);
                        int cmbIdx = 0;
                        for (int nIdx = 0; nIdx < targetNeurons.Count; nIdx++)
                        {
                            //Connect planned connection combination to neuron
                            for (int i = 0; i < plannedConnCmbIdxs[cmbIdx].Length; i++)
                            {
                                //Create synapse
                                inputNeuronUsageChecker[plannedConnCmbIdxs[cmbIdx][i]] = true;
                                Synapse synapse = new Synapse(inputField.SpikingNeuronCollection[plannedConnCmbIdxs[cmbIdx][i]],
                                                              targetNeurons[nIdx],
                                                              Synapse.SynRole.Input,
                                                              InstanceCfg.SynapseCfg,
                                                              rand
                                                              );
                                _inputDistancesStat.AddSampleValue(synapse.Distance);
                                SetInterconnection(_neuronInputConnectionsCollection, synapse);
                            }//for i
                            //Increment combination index
                            if (++cmbIdx == plannedConnCmbIdxs.Count)
                            {
                                //Restart from the beginning
                                cmbIdx = 0;
                            }
                        }//for nIdx
                        for (int i = 0; i < inputField.SpikingNeuronCollection.Length; i++)
                        {
                            //Unused input neuron?
                            if (!inputNeuronUsageChecker[i])
                            {
                                //Create additional synapse
                                int targetNeuronIdx = rand.Next(0, targetNeurons.Count);
                                Synapse synapse = new Synapse(inputField.SpikingNeuronCollection[i],
                                                              targetNeurons[targetNeuronIdx],
                                                              Synapse.SynRole.Input,
                                                              InstanceCfg.SynapseCfg,
                                                              rand
                                                              );
                                _inputDistancesStat.AddSampleValue(synapse.Distance);
                                SetInterconnection(_neuronInputConnectionsCollection, synapse);
                            }//!inputNeuronUsageChecker[i]
                        }//for i
                    }//Spiking synapses
                }//targetNeurons.Count > 0
            }//for activationType
            return;
        }


        /// <summary>
        /// Connects target neuron and source neurons
        /// </summary>
        /// <param name="targetNeuron">Target neuron</param>
        /// <param name="sourceNeurons">Source neurons</param>
        /// <param name="roleSelector">Realtime probabilistic selector of synapse role</param>
        /// <param name="avgDistance">Average distance to be achieved</param>
        /// <param name="numOfSynapses">Number of source neurons to be connected</param>
        /// <param name="rand">Random object</param>
        /// <param name="allowSelfConnection">Specifies whether to allow target neuron to be connected by itself</param>
        /// <param name="replace">Specifies whether to replace existing connections</param>
        private void ConnectNeuron(HiddenNeuron targetNeuron,
                                   List<HiddenNeuron> sourceNeurons,
                                   RelShareSelector<Synapse.SynRole> roleSelector,
                                   double avgDistance,
                                   int numOfSynapses,
                                   Random rand,
                                   bool allowSelfConnection,
                                   bool replace
                                   )
        {
            int[] counts = new int[Synapse.NumOfRoles];
            counts.Populate(0);
            //Create collection of available related neurons
            List<RelatedNeuron> neuronBuffer = new List<RelatedNeuron>(from neuron in sourceNeurons
                                                                       where (allowSelfConnection || (!allowSelfConnection && neuron != targetNeuron))
                                                                       select new RelatedNeuron
                                                                       {
                                                                           Neuron = neuron,
                                                                           Distance = avgDistance > 0 ? EuclideanDistance.Compute(targetNeuron.Location.ReservoirCoordinates, neuron.Location.ReservoirCoordinates) : 0
                                                                       });
            //Create connections
            for (int synapseIdx = 0; synapseIdx < numOfSynapses; synapseIdx++)
            {
                Synapse.SynRole role;
                if (targetNeuron.TypeOfActivation == ActivationType.Spiking)
                {
                    role = roleSelector.SelectNext();
                    if (numOfSynapses >= 2 &&
                       synapseIdx == numOfSynapses - 1 &&
                       roleSelector.Elements.Count == 2 &&
                       counts.Sum() == counts.Max() &&
                       counts[(int)role] > 0
                       )
                    {
                        //Override role to ensure at least one synapse having unused role
                        foreach (Synapse.SynRole elemRole in roleSelector.Elements)
                        {
                            if (counts[(int)elemRole] == 0)
                            {
                                role = elemRole;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    role = Synapse.SynRole.Indifferent;
                }
                while (neuronBuffer.Count > 0)
                {
                    int sourceNeuronIdx = 0;
                    if (avgDistance > 0d)
                    {
                        //Selection based on average distance
                        double gaussianDistance = rand.NextGaussianDouble(avgDistance, avgDistance / 2d);
                        //Find neuron having closest distance to gaussian distance
                        double minDiff = double.MaxValue;
                        for (int i = 0; i < neuronBuffer.Count; i++)
                        {
                            double err = Math.Abs(neuronBuffer[i].Distance - gaussianDistance);
                            if (err < minDiff)
                            {
                                sourceNeuronIdx = i;
                                minDiff = err;
                            }
                        }
                    }
                    else
                    {
                        //Pure random selection
                        sourceNeuronIdx = rand.Next(0, neuronBuffer.Count);
                    }
                    HiddenNeuron sourceNeuron = neuronBuffer[sourceNeuronIdx].Neuron;
                    neuronBuffer.RemoveAt(sourceNeuronIdx);
                    Synapse synapse = new Synapse(sourceNeuron, targetNeuron, role, InstanceCfg.SynapseCfg, rand);
                    if (SetInterconnection(_neuronNeuronConnectionsCollection, synapse, replace))
                    {
                        ++counts[(int)role];
                        _internalDistancesStat.AddSampleValue(synapse.Distance);
                        break;
                    }
                }
            }

            return;
        }

        private void ConnectRandomSchema(int poolID, Random rand, RandomSchemaSettings schemaCfg)
        {
            PoolSettings poolSettings = StructureCfg.PoolsCfg.PoolCfgCollection[poolID];
            List<HiddenNeuron> sourceNeurons = new List<HiddenNeuron>(_poolNeuronCollection[poolID]);
            RelShareSelector<Synapse.SynRole> spikingRoleSelector = new RelShareSelector<Synapse.SynRole>();
            spikingRoleSelector.Add(InstanceCfg.SynapseCfg.SpikingTargetCfg.ExcitatorySynCfg.RelShare, Synapse.SynRole.Excitatory);
            spikingRoleSelector.Add(InstanceCfg.SynapseCfg.SpikingTargetCfg.InhibitorySynCfg.RelShare, Synapse.SynRole.Inhibitory);
            RelShareSelector<int> connCountFluctuationSelector = new RelShareSelector<int>();
            connCountFluctuationSelector.Add(1, -1);
            connCountFluctuationSelector.Add(2, 0);
            connCountFluctuationSelector.Add(1, 1);

            for (int repetition = 1; repetition <= schemaCfg.Repetitions; repetition++)
            {
                foreach (HiddenNeuron targetNeuron in _poolNeuronCollection[poolID])
                {
                    //Determine connection counts
                    int intendedNumOfSynapses = (int)(Math.Round(((double)poolSettings.ProportionsCfg.Size) * schemaCfg.Density));
                    if (!schemaCfg.ConstantNumOfConnections && intendedNumOfSynapses > 2)
                    {
                        intendedNumOfSynapses += connCountFluctuationSelector.SelectNext();
                    }
                    ConnectNeuron(targetNeuron,
                                  sourceNeurons,
                                  spikingRoleSelector,
                                  schemaCfg.AvgDistance,
                                  intendedNumOfSynapses,
                                  rand,
                                  schemaCfg.AllowSelfConnection,
                                  schemaCfg.ReplaceExistingConnections
                                  );
                }
            }
            return;
        }

        private void ConnectChainSchema(int poolID, Random rand, ChainSchemaSettings schemaCfg)
        {
            RelShareSelector<Synapse.SynRole> spikingRoleSelector = new RelShareSelector<Synapse.SynRole>();
            spikingRoleSelector.Add(InstanceCfg.SynapseCfg.SpikingTargetCfg.ExcitatorySynCfg.RelShare, Synapse.SynRole.Excitatory);
            spikingRoleSelector.Add(InstanceCfg.SynapseCfg.SpikingTargetCfg.InhibitorySynCfg.RelShare, Synapse.SynRole.Inhibitory);
            List<HiddenNeuron> chainNeurons = new List<HiddenNeuron>(_poolNeuronCollection[poolID]);

            for (int repetition = 1; repetition <= schemaCfg.Repetitions; repetition++)
            {
                int chainLength = (int)Math.Round(schemaCfg.Ratio * chainNeurons.Count);
                if (chainLength < 2)
                {
                    //Nothing to do
                    return;
                }
                //////////////////////////////////////////////////////////////////////////////////////
                //Collect neurons to be chained
                List<HiddenNeuron> chainNeuronCollection = new List<HiddenNeuron>(chainNeurons);
                rand.Shuffle(chainNeuronCollection);
                if (chainLength < chainNeuronCollection.Count)
                {
                    //Cut the list according to chainLength
                    chainNeuronCollection.RemoveRange(chainLength, chainNeuronCollection.Count - chainLength);
                }
                //////////////////////////////////////////////////////////////////////////////////////
                //Create connection pairs
                List<Tuple<HiddenNeuron, HiddenNeuron>> connPairs = new List<Tuple<HiddenNeuron, HiddenNeuron>>(chainNeuronCollection.Count * 2);
                for (int i = 0; i < chainNeuronCollection.Count - 1; i++)
                {
                    connPairs.Add(new Tuple<HiddenNeuron, HiddenNeuron>(chainNeuronCollection[i], chainNeuronCollection[i + 1]));
                }
                if (schemaCfg.Circle)
                {
                    connPairs.Add(new Tuple<HiddenNeuron, HiddenNeuron>(chainNeuronCollection[chainNeuronCollection.Count - 1], chainNeuronCollection[0]));
                }
                //////////////////////////////////////////////////////////////////////////////////////
                //Connect connection pairs
                foreach (Tuple<HiddenNeuron, HiddenNeuron> connPair in connPairs)
                {
                    //Establish connection
                    ConnectNeuron(connPair.Item2, new List<HiddenNeuron>() { connPair.Item1 }, spikingRoleSelector, -1, 1, rand, false, schemaCfg.ReplaceExistingConnections);
                }

            }
            return;
        }

        private void SetInterPoolConnection(Random rand, InterPoolConnSettings cfg)
        {
            RelShareSelector<Synapse.SynRole> spikingRoleSelector = new RelShareSelector<Synapse.SynRole>();
            spikingRoleSelector.Add(InstanceCfg.SynapseCfg.SpikingTargetCfg.ExcitatorySynCfg.RelShare, Synapse.SynRole.Excitatory);
            spikingRoleSelector.Add(InstanceCfg.SynapseCfg.SpikingTargetCfg.InhibitorySynCfg.RelShare, Synapse.SynRole.Inhibitory);
            RelShareSelector<int> connCountFluctuationSelector = new RelShareSelector<int>();
            connCountFluctuationSelector.Add(1, -1);
            connCountFluctuationSelector.Add(2, 0);
            connCountFluctuationSelector.Add(1, 1);
            int targetPoolID = StructureCfg.PoolsCfg.GetPoolID(cfg.TargetPoolName);
            int sourcePoolID = StructureCfg.PoolsCfg.GetPoolID(cfg.SourcePoolName);
            PoolSettings sourcePoolSettings = StructureCfg.PoolsCfg.PoolCfgCollection[sourcePoolID];
            List<HiddenNeuron> targetNeurons = new List<HiddenNeuron>(_poolNeuronCollection[targetPoolID]);
            int numOfTargetNeurons = (int)Math.Round(((double)targetNeurons.Count) * cfg.TargetConnectionDensity);
            rand.Shuffle(targetNeurons);
            List<HiddenNeuron> sourceNeurons = new List<HiddenNeuron>(_poolNeuronCollection[sourcePoolID]);

            for (int targetNeuronIdx = 0; targetNeuronIdx < numOfTargetNeurons; targetNeuronIdx++)
            {
                //Determine connection counts
                int intendedNumOfSynapses = (int)(Math.Round(((double)sourcePoolSettings.ProportionsCfg.Size) * cfg.SourceConnectionDensity));
                if (!cfg.ConstantNumOfConnections && intendedNumOfSynapses > 2)
                {
                    intendedNumOfSynapses += connCountFluctuationSelector.SelectNext();
                }
                ConnectNeuron(targetNeurons[targetNeuronIdx],
                              sourceNeurons,
                              spikingRoleSelector,
                              -1,
                              intendedNumOfSynapses,
                              rand,
                              false,
                              false
                              );
            }
            return;
        }


        /// <summary>
        /// Resets reservoir's state to its initial state.
        /// Function does not affect weights or internal structure of the resservoir.
        /// </summary>
        /// <param name="resetStatistics">Specifies whether to reset internal statistics</param>
        public void Reset(bool resetStatistics)
        {
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
        /// Computes new cycle of the reservoir.
        /// </summary>
        /// <param name="updateStatistics">Specifies whether to update neurons statistics. Specify "false" during the booting phase and "true" after the booting phase.</param>
        public void Compute(bool updateStatistics)
        {
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
        /// Copies all reservoir predictors to a given buffer starting from the specified position
        /// </summary>
        /// <param name="buffer">Target buffer</param>
        /// <param name="fromOffset">Starting zero based position in the target buffer</param>
        public int CopyPredictorsTo(double[] buffer, int fromOffset)
        {
            int offset = fromOffset;
            foreach (INeuron neuron in PredictingNeuronCollection)
            {
                offset += neuron.CopyPredictorsTo(buffer, offset);
            }
            return offset - fromOffset;
        }

        /// <summary>
        /// Collects key statistics related to reservoir neurons states
        /// </summary>
        public ReservoirStat CollectStatistics()
        {
            ReservoirStat resStat = new ReservoirStat(InstanceCfg.Name,
                                                      StructureCfg,
                                                      _reservoirNeuronCollection.Length,
                                                      NumOfPredictors
                                                      );

            foreach (HiddenNeuron neuron in _reservoirNeuronCollection)
            {
                resStat.Update(neuron,
                               _neuronInputConnectionsCollection[neuron.Location.ReservoirFlatIdx].Values,
                               _neuronNeuronConnectionsCollection[neuron.Location.ReservoirFlatIdx].Values
                               );
            }
            return resStat;
        }

        //Inner classes
        private class NeuronCreationParams
        {
            public NeuronCommon.NeuronSignalingRestrictionType SignalingRestriction { get; set; }
            public IActivationFunction Activation { get; set; }
            public double Bias { get; set; }
            public int GroupID { get; set; }
            public double AnalogFiringThreshold { get; set; }
            public double RetainmentStrength { get; set; }
            public PredictorsSettings PredictorsCfg { get; set; }
        }//NeuronCreationParams

        private class RelatedNeuron
        {
            //Attribute properties
            public HiddenNeuron Neuron { get; set; }
            public double Distance { get; set; }
        }//RelatedNeuron


    }//Reservoir

}//Namespace
