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
using RCNet.MathTools.Probability;

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


        //Attributes
        /// <summary>
        /// Neurons within the pools.
        /// </summary>
        private readonly List<HiddenNeuron[]> _poolNeuronsCollection;
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
        /// <param name="inputCfg">Input settings</param>
        /// <param name="structureCfg">Reservoir structure configuration</param>
        /// <param name="instanceCfg">Reservoir instance configuration</param>
        /// <param name="inputRange">Range of input values</param>
        /// <param name="rand">Random object to be used for random part of the initialization</param>
        public ReservoirInstance(int instanceID,
                                 InputSettings inputCfg,
                                 ReservoirStructureSettings structureCfg,
                                 ReservoirInstanceSettings instanceCfg,
                                 Interval inputRange,
                                 Random rand
                                 )
        {
            //Copy settings
            StructureCfg = (ReservoirStructureSettings)structureCfg.DeepClone();
            InstanceCfg = (ReservoirInstanceSettings)instanceCfg.DeepClone();
            //-----------------------------------------------------------------------------
            //Initialization of neurons
            //-----------------------------------------------------------------------------
            //Input neurons
            int inputNeuronsStartIdx = 0;
            InputUnitCollection = new InputUnit[InstanceCfg.InputUnitsCfg.InputUnitCfgCollection.Count];
            for (int i = 0; i < InstanceCfg.InputUnitsCfg.InputUnitCfgCollection.Count; i++)
            {
                InputUnitCollection[i] = new InputUnit(instanceID,
                                                       inputRange,
                                                       inputCfg.FieldsCfg.GetFieldID(InstanceCfg.InputUnitsCfg.InputUnitCfgCollection[i].InputFieldName),
                                                       inputNeuronsStartIdx,
                                                       InstanceCfg.InputUnitsCfg.InputUnitCfgCollection[i]
                                                       );
                inputNeuronsStartIdx += InputUnitCollection[i].NumOfInputNeurons;
            }
            //-----------------------------------------------------------------------------
            //Reservoir neurons
            //-----------------------------------------------------------------------------
            int neuronReservoirFlatIdx = 0;
            _numOfAnalogNeurons = 0;
            _numOfSpikingNeurons = 0;
            List<HiddenNeuron> allNeurons = new List<HiddenNeuron>();
            _poolNeuronsCollection = new List<HiddenNeuron[]>(StructureCfg.PoolsCfg.PoolCfgCollection.Count);
            PredictingNeuronCollection = new List<HiddenNeuron>();
            for (int poolID = 0; poolID < StructureCfg.PoolsCfg.PoolCfgCollection.Count; poolID++)
            {
                PoolSettings poolSettings = StructureCfg.PoolsCfg.PoolCfgCollection[poolID];
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
            //Parallel processing ranges
            var rangePartitioner = Partitioner.Create(0, _reservoirNeuronCollection.Length);
            _parallelRanges = new List<Tuple<int, int>>(rangePartitioner.GetDynamicPartitions());

            //-----------------------------------------------------------------------------
            //Connections
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
            for(int i = 0; i < InputUnitCollection.Length; i++)
            {
                ConnectInputUnit(InstanceCfg.InputUnitsCfg.InputUnitCfgCollection[i], InputUnitCollection[i], rand);
            }

            //-----------------------------------------------------------------------------
            //Pools interconnection
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
            //Homogenous excitability application
            if(_numOfSpikingNeurons > 0)
            {
                ApplyHomogenousExcitability();
            }
            //-----------------------------------------------------------------------------
            //Spectral radius application
            if (_numOfAnalogNeurons > 0 && InstanceCfg.SynapseCfg.AnalogTargetCfg.SpectralRadius != SynapseATSettings.NASpectralRadiusNum)
            {
                ApplySpectralRadius(InstanceCfg.SynapseCfg.AnalogTargetCfg.SpectralRadius);
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
                    throw new Exception("ApplySpectralRadius: Invalid analog weights. Largest eigenvalue is 0.");
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
            //Loop neuron by neuron
            for (int nIdx = 0; nIdx < _reservoirNeuronCollection.Length; nIdx++)
            {
                //Set homogenous excitability of hidden neuron having spiking activation
                if (_reservoirNeuronCollection[nIdx].TypeOfActivation == ActivationType.Spiking)
                {
                    int poolID = _reservoirNeuronCollection[nIdx].Location.PoolID;
                    int groupID = _reservoirNeuronCollection[nIdx].Location.PoolGroupID;
                    HomogenousExcitabilitySettings homogenousExcitabilityCfg = ((SpikingNeuronGroupSettings)StructureCfg.PoolsCfg.PoolCfgCollection[poolID].NeuronGroupsCfg.GroupCfgCollection[groupID]).HomogenousExcitabilityCfg;
                    double totalExcitatoryStrength = homogenousExcitabilityCfg.InputStrength + homogenousExcitabilityCfg.ExcitatoryStrength;
                    double sumOfInputWeights = 0d, sumOfExcitatoryWeights = 0d, sumOfInhibitoryWeights = 0d;
                    //Scan input synapses
                    foreach (Synapse synapse in _neuronInputConnectionsCollection[nIdx].Values)
                    {
                        sumOfInputWeights += Math.Abs(synapse.Weight);
                    }
                    //Scan inhibitory and excitatory synapses
                    foreach (Synapse synapse in _neuronNeuronConnectionsCollection[nIdx].Values)
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
                    if ((sumOfExcitatoryWeights + sumOfInputWeights + sumOfInhibitoryWeights) == 0)
                    {
                        throw new Exception("Can't set homogenous excitability. Hidden neuron has no synapses.");
                    }
                    if (sumOfInhibitoryWeights == 0 && (sumOfExcitatoryWeights + sumOfInputWeights) > 0)
                    {
                        throw new Exception("Can't set homogenous excitability. Hidden neuron has no inhibitory synapse.");
                    }
                    if ((sumOfExcitatoryWeights + sumOfInputWeights) == 0 && sumOfInhibitoryWeights > 0)
                    {
                        throw new Exception("Can't set homogenous excitability. Hidden neuron has no excitatory or input synapse.");
                    }
                    //Scale
                    //Rescale input synapses
                    foreach (Synapse synapse in _neuronInputConnectionsCollection[nIdx].Values)
                    {
                        double factor = homogenousExcitabilityCfg.InputStrength / sumOfInputWeights;
                        synapse.Rescale(factor);
                    }
                    //Rescale excitatory and inhibitory synapses
                    foreach (Synapse synapse in _neuronNeuronConnectionsCollection[nIdx].Values)
                    {
                        if (Math.Sign(synapse.Weight) >= 0)
                        {
                            double factor = (sumOfInputWeights == 0d ? totalExcitatoryStrength : homogenousExcitabilityCfg.ExcitatoryStrength) / sumOfExcitatoryWeights;
                            synapse.Rescale(factor);
                        }
                        else
                        {
                            double factor = (homogenousExcitabilityCfg.InhibitoryRatio * totalExcitatoryStrength) / sumOfInhibitoryWeights;
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
                    if(replace)
                    {
                        connectionsCollection[synapse.TargetNeuron.Location.ReservoirFlatIdx][synapse.SourceNeuron.Location.ReservoirFlatIdx] = synapse;
                        return true;
                    }
                    return false;
                }
            }
        }

        /// <summary>
        /// Creates input synapses from input unit to hidden neurons
        /// </summary>
        /// <param name="inputUnitCfg">Input unit configuration</param>
        /// <param name="inputUnit">Input unit instance</param>
        /// <param name="rand">Random object</param>
        private void ConnectInputUnit(InputUnitSettings inputUnitCfg, InputUnit inputUnit, Random rand)
        {
            foreach (InputUnitConnSettings inputConnCfg in inputUnitCfg.ConnsCfg.ConnCfgCollection)
            {
                //Get associated analog input neuron
                InputNeuron[] analogInputNeurons = new InputNeuron[] { inputUnit.AnalogInputNeuron };
                //Get associated spike-train input neurons
                InputNeuron[] spikeTrainInputNeurons = inputUnit.SpikeTrainInputNeuronCollection;
                //Target pool index
                int targetPoolID = StructureCfg.PoolsCfg.GetPoolID(inputConnCfg.PoolName);
                //Select available target neurons according to connection's target scope configuration
                List<HiddenNeuron>[] targetNeuronsByActivation = new List<HiddenNeuron>[Enum.GetValues(typeof(ActivationType)).Length];
                //Spiking target scope
                targetNeuronsByActivation[(int)ActivationType.Spiking] = new List<HiddenNeuron>(from neuron in _poolNeuronsCollection[targetPoolID]
                                                                                                   where (neuron.TypeOfActivation == ActivationType.Spiking && inputConnCfg.SpikingTargetDensity > 0)
                                                                                                   select neuron
                                                                                                   );
                //Analog target scope
                targetNeuronsByActivation[(int)ActivationType.Analog] = new List<HiddenNeuron>(from neuron in _poolNeuronsCollection[targetPoolID]
                                                                                                  where (neuron.TypeOfActivation == ActivationType.Analog && inputConnCfg.AnalogTargetDensity > 0)
                                                                                                  select neuron
                                                                                                  );
                //Loop through target activation scope
                for (int activationType = 0; activationType < targetNeuronsByActivation.Length; activationType++)
                {
                    //Available target neurons
                    List<HiddenNeuron> targetNeurons = targetNeuronsByActivation[activationType];
                    //Density
                    double density = activationType == (int)ActivationType.Spiking ? inputConnCfg.SpikingTargetDensity : inputConnCfg.AnalogTargetDensity;
                    //Number of synapses
                    int numOfSynapses = (int)Math.Round(targetNeurons.Count * density, 0);
                    if (targetNeurons.Count > 0 && numOfSynapses > 0)
                    {
                        foreach (InputNeuron inputNeuron in (activationType == (int)ActivationType.Spiking ? spikeTrainInputNeurons : analogInputNeurons))
                        {
                            rand.Shuffle(targetNeurons);
                            for(int i = 0; i < numOfSynapses; i++)
                            {
                                //Create synapse
                                Synapse synapse = new Synapse(inputNeuron,
                                                              targetNeurons[i],
                                                              Synapse.SynRole.Input,
                                                              InstanceCfg.SynapseCfg,
                                                              rand
                                                              );
                                _inputDistancesStat.AddSampleValue(synapse.Distance);
                                SetInterconnection(_neuronInputConnectionsCollection, synapse);
                            }//for i
                        }//foreach inputNeuron
                    }//targetNeurons.Count > 0 && numOfSynapses > 0
                }//for activationType
            }//foreach inputConnCfg
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
        /// <param name="allowSelfConnection">Specifies if to allow target neuron to be connected by itself</param>
        /// <param name="replace">Specifies if to replace existing connections</param>
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
                            if(counts[(int)elemRole] == 0)
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
            List<HiddenNeuron> sourceNeurons = new List<HiddenNeuron>(_poolNeuronsCollection[poolID]);
            RelShareSelector<Synapse.SynRole> spikingRoleSelector = new RelShareSelector<Synapse.SynRole>();
            spikingRoleSelector.Add(InstanceCfg.SynapseCfg.SpikingTargetCfg.ExcitatorySynCfg.RelShare, Synapse.SynRole.Excitatory);
            spikingRoleSelector.Add(InstanceCfg.SynapseCfg.SpikingTargetCfg.InhibitorySynCfg.RelShare, Synapse.SynRole.Inhibitory);
            RelShareSelector<int> connCountFluctuationSelector = new RelShareSelector<int>();
            connCountFluctuationSelector.Add(1, -1);
            connCountFluctuationSelector.Add(2, 0);
            connCountFluctuationSelector.Add(1, 1);

            for (int repetition = 1; repetition <= schemaCfg.Repetitions; repetition++)
            {
                foreach (HiddenNeuron targetNeuron in _poolNeuronsCollection[poolID])
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

            for (int repetition = 1; repetition <= schemaCfg.Repetitions; repetition++)
            {
                int chainLength = (int)Math.Round(schemaCfg.Ratio * _poolNeuronsCollection[poolID].Length);
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
                if(schemaCfg.Circle)
                {
                    connPairs.Add(new Tuple<HiddenNeuron, HiddenNeuron>(chainNeuronCollection[chainNeuronCollection.Count - 1], chainNeuronCollection[0]));
                }
                //////////////////////////////////////////////////////////////////////////////////////
                //Connect connection pairs
                foreach(Tuple<HiddenNeuron, HiddenNeuron> connPair in connPairs)
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
            PoolSettings targetPoolSettings = StructureCfg.PoolsCfg.PoolCfgCollection[targetPoolID];
            int sourcePoolID = StructureCfg.PoolsCfg.GetPoolID(cfg.SourcePoolName);
            PoolSettings sourcePoolSettings = StructureCfg.PoolsCfg.PoolCfgCollection[sourcePoolID];
            int numOftargetNeurons = (int)Math.Round(((double)targetPoolSettings.ProportionsCfg.Size) * cfg.TargetConnectionDensity);
            List<HiddenNeuron> targetNeurons = new List<HiddenNeuron>(_poolNeuronsCollection[targetPoolID]);
            rand.Shuffle(targetNeurons);
            List<HiddenNeuron> sourceNeurons = new List<HiddenNeuron>(_poolNeuronsCollection[sourcePoolID]);

            for (int targetNeuronIdx = 0; targetNeuronIdx < numOftargetNeurons; targetNeuronIdx++)
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
