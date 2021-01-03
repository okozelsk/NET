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
    /// Implements the reservoir.
    /// </summary>
    [Serializable]
    public class ReservoirInstance
    {
        //Attribute properties
        /// <summary>
        /// The reservoir instance ID.
        /// </summary>
        public int InstanceID { get; }

        /// <summary>
        /// The configuration of the reservoir structure.
        /// </summary>
        public ReservoirStructureSettings StructureCfg { get; }

        /// <summary>
        /// The configuration of the reservoir instance.
        /// </summary>
        public ReservoirInstanceSettings InstanceCfg { get; }

        /// <summary>
        /// The collection of neurons providing the predictors.
        /// </summary>
        public List<HiddenNeuron> PredictingNeuronCollection { get; }

        /// <summary>
        /// The number of reservoir predictors.
        /// </summary>
        public int NumOfPredictors { get; }


        //Attributes
        /// <summary>
        /// The neurons within the pools.
        /// </summary>
        private readonly List<HiddenNeuron[]> _poolNeuronCollection;
        /// <summary>
        /// All reservoir neurons (flat structure).
        /// </summary>
        private readonly HiddenNeuron[] _reservoirNeuronCollection;
        /// <summary>
        /// The input synapses.
        /// </summary>
        private readonly SortedList<int, Synapse>[] _neuronInputSynapsesCollection;
        /// <summary>
        /// The internal synapses.
        /// </summary>
        private readonly SortedList<int, Synapse>[] _neuronNeuronSynapsesCollection;
        /// <summary>
        /// The ranges for parallel processing.
        /// </summary>
        private readonly List<Tuple<int, int>> _parallelRanges;
        /// <summary>
        /// The total number of analog hidden neurons.
        /// </summary>
        private readonly int _numOfAnalogNeurons;
        /// <summary>
        /// The total number of spiking hidden neurons.
        /// </summary>
        private readonly int _numOfSpikingNeurons;
        /// <summary>
        /// The input distances statistics.
        /// </summary>
        private readonly BasicStat _inputDistancesStat;
        /// <summary>
        /// The internal distances statistics.
        /// </summary>
        private readonly BasicStat _internalDistancesStat;


        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="instanceID">The reservoir instance ID.</param>
        /// <param name="structureCfg">The configuration of the reservoir structure.</param>
        /// <param name="instanceCfg">The configuration of the reservoir instance.</param>
        /// <param name="inputEncoder">The input encoder.</param>
        /// <param name="rand">The random object to be used.</param>
        public ReservoirInstance(int instanceID,
                                 ReservoirStructureSettings structureCfg,
                                 ReservoirInstanceSettings instanceCfg,
                                 InputEncoder inputEncoder,
                                 Random rand
                                 )
        {
            InstanceID = instanceID;
            //Copy the configurations
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
            PredictingNeuronCollection = new List<HiddenNeuron>();
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
                    //Group neuron params
                    int analogThresholdMaxRefDeepness = 1;
                    for (int i = 0; i < ngs.Count; i++)
                    {
                        grpNCP[i] = new NeuronCreationParams
                        {
                            Activation = ActivationFactory.CreateAF(ngs.ActivationCfg, rand),
                            Bias = ngs.BiasCfg == null ? 0 : rand.NextDouble(ngs.BiasCfg),
                            GroupID = groupID,
                            AnalogFiringThreshold = ngs.ActivationCfg.TypeOfActivation == ActivationType.Spiking ? -1 : ((AnalogNeuronGroupSettings)ngs).FiringThreshold,
                            AnalogThresholdMaxRefDeepness = ngs.ActivationCfg.TypeOfActivation == ActivationType.Spiking ? -1 : analogThresholdMaxRefDeepness++,
                            RetainmentStrength = 0,
                            PredictorsCfg = null
                        };
                        if (ngs.ActivationCfg.TypeOfActivation == ActivationType.Analog)
                        {
                            if (analogThresholdMaxRefDeepness > ((AnalogNeuronGroupSettings)ngs).ThresholdMaxRefDeepness)
                            {
                                analogThresholdMaxRefDeepness = 1;
                            }
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
                    if (ngs.ActivationCfg.TypeOfActivation == ActivationType.Analog && ((AnalogNeuronGroupSettings)ngs).RetainmentCfg != null)
                    {
                        int numOfRetNeurons = (int)Math.Round(((AnalogNeuronGroupSettings)ngs).RetainmentCfg.Density * ngs.Count, 0);
                        rand.Shuffle(grpNCP);
                        for (int i = 0; i < numOfRetNeurons; i++)
                        {
                            grpNCP[i].RetainmentStrength = rand.NextDouble(((AnalogNeuronGroupSettings)ngs).RetainmentCfg.StrengthCfg);
                        }
                    }
                    //Readout
                    if (ngs.PredictorsCfg.NumOfPredictors > 0)
                    {
                        for (int i = 0; i < ngs.Count; i++)
                        {
                            grpNCP[i].PredictorsCfg = ngs.PredictorsCfg;
                        }
                    }
                    ++groupID;
                }//ngs
                //Randomize order before sequential instantiation
                rand.Shuffle(neuronParamCollection);
                //Instantiate neurons
                HiddenNeuron[] poolNeurons = new HiddenNeuron[poolSettings.ProportionsCfg.Size];
                int neuronPoolFlatIdx = 0;
                int firingRefHistDistance = 0;
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
                                                                                  (AFAnalogBase)neuronParamCollection[neuronPoolFlatIdx].Activation,
                                                                                  neuronParamCollection[neuronPoolFlatIdx].Bias,
                                                                                  neuronParamCollection[neuronPoolFlatIdx].AnalogFiringThreshold,
                                                                                  neuronParamCollection[neuronPoolFlatIdx].AnalogThresholdMaxRefDeepness,
                                                                                  neuronParamCollection[neuronPoolFlatIdx].RetainmentStrength,
                                                                                  neuronParamCollection[neuronPoolFlatIdx].PredictorsCfg
                                                                                  );
                                if (firingRefHistDistance > 40)
                                {
                                    firingRefHistDistance = 0;
                                }
                            }
                            else
                            {
                                //Use constructor for hidden neuron having spiking activation
                                poolNeurons[neuronPoolFlatIdx] = new HiddenNeuron(location,
                                                                                  (AFSpikingBase)neuronParamCollection[neuronPoolFlatIdx].Activation,
                                                                                  neuronParamCollection[neuronPoolFlatIdx].Bias,
                                                                                  neuronParamCollection[neuronPoolFlatIdx].PredictorsCfg
                                                                                  );
                            }
                            allNeurons.Add(poolNeurons[neuronPoolFlatIdx]);
                            //Predictor
                            if (poolNeurons[neuronPoolFlatIdx].NumOfProvidedPredictors > 0)
                            {
                                PredictingNeuronCollection.Add(poolNeurons[neuronPoolFlatIdx]);
                                NumOfPredictors += poolNeurons[neuronPoolFlatIdx].NumOfProvidedPredictors;
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
            _neuronInputSynapsesCollection = new SortedList<int, Synapse>[_reservoirNeuronCollection.Length];
            _neuronNeuronSynapsesCollection = new SortedList<int, Synapse>[_reservoirNeuronCollection.Length];
            for (int n = 0; n < _reservoirNeuronCollection.Length; n++)
            {
                _neuronInputSynapsesCollection[n] = new SortedList<int, Synapse>();
                _neuronNeuronSynapsesCollection[n] = new SortedList<int, Synapse>();
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
                    if (connSchema.GetType() == typeof(EmptySchemaSettings))
                    {
                        //No interconnection
                        break;
                    }
                    else if (connSchema.GetType() == typeof(RandomSchemaSettings))
                    {
                        ConnectRandomSchema(poolID, rand, (RandomSchemaSettings)connSchema);
                    }
                    else if (connSchema.GetType() == typeof(ChainSchemaSettings))
                    {
                        ConnectChainSchema(poolID, rand, (ChainSchemaSettings)connSchema);
                    }
                    else if (connSchema.GetType() == typeof(DoubleTwistedToroidSchemaSettings))
                    {
                        ConnectDoubleTwistedToroidSchema(poolID, rand, (DoubleTwistedToroidSchemaSettings)connSchema);
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
            foreach (SortedList<int, Synapse> synapses in _neuronInputSynapsesCollection)
            {
                foreach (Synapse synapse in synapses.Values)
                {
                    synapse.SetupDelay(_inputDistancesStat, rand);
                }
            }
            //Setup delay on internal synapses
            foreach (SortedList<int, Synapse> synapses in _neuronNeuronSynapsesCollection)
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
        /// Gets the reservoir size (the number of neurons within the reservoir).
        /// </summary>
        public int Size { get { return _reservoirNeuronCollection.Length; } }

        //Methods
        /// <summary>
        /// Estimates the number of computation cycles necessary to make the reservoir and predictors useable.
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
        /// Gets the collection of descriptors of all the predictors.
        /// </summary>
        public List<PredictorDescriptor> GetPredictorsDescriptors()
        {
            List<PredictorDescriptor> result = new List<PredictorDescriptor>(NumOfPredictors);
            foreach (HiddenNeuron neuron in PredictingNeuronCollection)
            {
                if (neuron.NumOfProvidedPredictors > 0)
                {
                    foreach (PredictorsProvider.PredictorID id in neuron.GetPredictorsIDs())
                    {
                        result.Add(new PredictorDescriptor(neuron.Location.ReservoirID, neuron.Location.PoolID, id));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Adjusts weights of synapses connecting the analog neurons to achieve required spectral radius.
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
                    foreach (Synapse synapse in _neuronNeuronSynapsesCollection[neuron.Location.ReservoirFlatIdx].Values)
                    {
                        wMatrix.Data[neuron.Location.ReservoirFlatIdx][synapse.PresynapticNeuron.Location.ReservoirFlatIdx] = synapse.Weight;
                    }
                });
                double largestEigenValue = Math.Abs(wMatrix.EstimateLargestEigenvalue(out double[] eigenVector));
                if (largestEigenValue == 0)
                {
                    throw new InvalidOperationException($"Can't apply SpectralRadius. Invalid analog weights, largest eigenvalue is 0.");
                }
                double scale = spectralRadius / largestEigenValue;
                //Scale weights of synapses targeting analog neurons
                Parallel.ForEach(scopeNeurons, neuron =>
                {
                    foreach (Synapse synapse in _neuronNeuronSynapsesCollection[neuron.Location.ReservoirFlatIdx].Values)
                    {
                        synapse.Rescale(scale);
                    }
                });
            }
            return;
        }

        /// <summary>
        /// Sets the homogenous excitability of the hidden spiking neurons.
        /// </summary>
        private void ApplyHomogenousExcitability()
        {
            List<HiddenNeuron> spikingNeurons = new List<HiddenNeuron>(from neuron in _reservoirNeuronCollection where neuron.TypeOfActivation == ActivationType.Spiking select neuron);
            foreach (HiddenNeuron neuron in spikingNeurons)
            {
                HomogenousExcitabilitySettings homogenousExcitabilityCfg = ((SpikingNeuronGroupSettings)StructureCfg.PoolsCfg.PoolCfgCollection[neuron.Location.PoolID].NeuronGroupsCfg.GroupCfgCollection[neuron.Location.PoolGroupID]).HomogenousExcitabilityCfg;
                double sumOfInputWeights = 0d, sumOfExcitatoryWeights = 0d, sumOfInhibitoryWeights = 0d;
                //Scan input synapses
                foreach (Synapse synapse in _neuronInputSynapsesCollection[neuron.Location.ReservoirFlatIdx].Values)
                {
                    sumOfInputWeights += Math.Abs(synapse.Weight);
                }
                //Scan Excitatory and Inhibitory synapses
                foreach (Synapse synapse in _neuronNeuronSynapsesCollection[neuron.Location.ReservoirFlatIdx].Values)
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
                    foreach (Synapse synapse in _neuronInputSynapsesCollection[neuron.Location.ReservoirFlatIdx].Values)
                    {
                        synapse.Rescale(factor);
                    }
                }
                //Rescale excitatory synapses
                double targetSumOfExcitatoryWeights = sumOfInputWeights == 0 ? homogenousExcitabilityCfg.ExcitatoryStrength : homogenousExcitabilityCfg.ExcitatoryStrength - targetSumOfInputWeights;
                if (sumOfExcitatoryWeights > 0)
                {
                    double factor = targetSumOfExcitatoryWeights / sumOfExcitatoryWeights;
                    foreach (Synapse synapse in _neuronNeuronSynapsesCollection[neuron.Location.ReservoirFlatIdx].Values)
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
                    foreach (Synapse synapse in _neuronNeuronSynapsesCollection[neuron.Location.ReservoirFlatIdx].Values)
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
        /// Adjusts the weights of input synapses connecting analog neurons to ensure their sum does not exceed the max.
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
                    if (_neuronInputSynapsesCollection[nIdx].Values.Count > 1)
                    {
                        double factor = 1d / _neuronInputSynapsesCollection[nIdx].Values.Count;
                        foreach (Synapse synapse in _neuronInputSynapsesCollection[nIdx].Values)
                        {
                            synapse.Rescale(factor);
                        }
                    }
                }
            }
            return;
        }

        /// <summary>
        /// Adds the synapse into the specified bank.
        /// </summary>
        /// <param name="synapsesCollection">The bank of the synapses.</param>
        /// <param name="synapse">A synapse to be added into the bank.</param>
        /// <param name="replace">Specifies whether to replace existing connection.</param>
        private bool SetInterconnection(SortedList<int, Synapse>[] synapsesCollection, Synapse synapse, bool replace = false)
        {
            //Add new connection
            lock (synapsesCollection[synapse.PostsynapticNeuron.Location.ReservoirFlatIdx])
            {
                try
                {
                    synapsesCollection[synapse.PostsynapticNeuron.Location.ReservoirFlatIdx].Add(synapse.PresynapticNeuron.Location.ReservoirFlatIdx, synapse);
                    return true;
                }
                catch
                {
                    //Connection already exists
                    if (replace)
                    {
                        synapsesCollection[synapse.PostsynapticNeuron.Location.ReservoirFlatIdx][synapse.PresynapticNeuron.Location.ReservoirFlatIdx] = synapse;
                        return true;
                    }
                    return false;
                }
            }
        }

        /// <summary>
        /// Creates the input synapses from input neurons to hidden neurons.
        /// </summary>
        /// <param name="inputEncoder">The input encoder.</param>
        /// <param name="inputConnCfg">The input connection configuration.</param>
        /// <param name="rand">The random object to be used.</param>
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
                List<HiddenNeuron> targetNeurons = new List<HiddenNeuron>(targetNeuronsByActivation[activationType]);
                rand.Shuffle(targetNeurons);
                //Limit target neurons 
                int targetNeuronsCount = (int)Math.Round(targetNeurons.Count * density, 0);
                if (targetNeuronsCount < targetNeurons.Count)
                {
                    targetNeurons.RemoveRange(targetNeuronsCount, targetNeurons.Count - targetNeuronsCount);
                }
                if (targetNeurons.Count > 0)
                {
                    if (activationType == (int)ActivationType.Analog)
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
                            _inputDistancesStat.AddSample(synapse.Distance);
                            SetInterconnection(_neuronInputSynapsesCollection, synapse);
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
                            if (plannedConnCmbIdxs.Count > 0)
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
                                    _inputDistancesStat.AddSample(synapse.Distance);
                                    SetInterconnection(_neuronInputSynapsesCollection, synapse);
                                }//for i
                                 //Increment combination index
                                if (++cmbIdx == plannedConnCmbIdxs.Count)
                                {
                                    //Restart from the beginning
                                    cmbIdx = 0;
                                }
                            }
                            else
                            {
                                //Connect input analog neuron to spiking hidden neuron
                                Synapse synapse = new Synapse(inputField.AnalogNeuron,
                                                              targetNeurons[nIdx],
                                                              Synapse.SynRole.Input,
                                                              InstanceCfg.SynapseCfg,
                                                              rand
                                                              );
                                _inputDistancesStat.AddSample(synapse.Distance);
                                SetInterconnection(_neuronInputSynapsesCollection, synapse);
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
                                _inputDistancesStat.AddSample(synapse.Distance);
                                SetInterconnection(_neuronInputSynapsesCollection, synapse);
                            }//!inputNeuronUsageChecker[i]
                        }//for i
                    }//Spiking synapses
                }//targetNeurons.Count > 0
            }//for activationType
            return;
        }


        /// <summary>
        /// Connects the target neuron and source neurons.
        /// </summary>
        /// <param name="targetNeuron">The target neuron.</param>
        /// <param name="sourceNeurons">The source neurons.</param>
        /// <param name="roleSelector">The realtime probabilistic selector of the synapse role.</param>
        /// <param name="avgDistance">An average distance to be achieved.</param>
        /// <param name="numOfSynapses">The number of source neurons to be connected.</param>
        /// <param name="rand">The random object to be used</param>
        /// <param name="allowSelfConnection">Specifies whether to allow the target neuron to be connected by itself.</param>
        /// <param name="replace">Specifies whether to replace the existing connections.</param>
        private void ConnectNeuron(HiddenNeuron targetNeuron,
                                   List<HiddenNeuron> sourceNeurons,
                                   ProbabilisticSelector<Synapse.SynRole> roleSelector,
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
                    if (SetInterconnection(_neuronNeuronSynapsesCollection, synapse, replace))
                    {
                        ++counts[(int)role];
                        _internalDistancesStat.AddSample(synapse.Distance);
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
            ProbabilisticSelector<Synapse.SynRole> spikingRoleSelector = new ProbabilisticSelector<Synapse.SynRole>();
            spikingRoleSelector.Add(InstanceCfg.SynapseCfg.SpikingTargetCfg.ExcitatorySynCfg.RelShare, Synapse.SynRole.Excitatory);
            spikingRoleSelector.Add(InstanceCfg.SynapseCfg.SpikingTargetCfg.InhibitorySynCfg.RelShare, Synapse.SynRole.Inhibitory);
            ProbabilisticSelector<int> connCountFluctuationSelector = new ProbabilisticSelector<int>();
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

        private List<HiddenNeuron> RandomlySelectPoolNeurons(int poolID, Random rand, int count)
        {
            //Select neurons to be chained
            List<HiddenNeuron> neurons = new List<HiddenNeuron>(_poolNeuronCollection[poolID]);
            rand.Shuffle(neurons);
            if (count < neurons.Count)
            {
                //Cut the list according to chainLength
                neurons.RemoveRange(count, neurons.Count - count);
            }
            return neurons;
        }

        private void ConnectChainOfNeurons(List<HiddenNeuron> neurons, Random rand, bool replace)
        {
            ProbabilisticSelector<Synapse.SynRole> spikingRoleSelector = new ProbabilisticSelector<Synapse.SynRole>();
            spikingRoleSelector.Add(InstanceCfg.SynapseCfg.SpikingTargetCfg.ExcitatorySynCfg.RelShare, Synapse.SynRole.Excitatory);
            spikingRoleSelector.Add(InstanceCfg.SynapseCfg.SpikingTargetCfg.InhibitorySynCfg.RelShare, Synapse.SynRole.Inhibitory);
            for (int i = 0; i < neurons.Count - 1; i++)
            {
                //Establish connection
                ConnectNeuron(neurons[i + 1],
                              new List<HiddenNeuron>() { neurons[i] },
                              spikingRoleSelector,
                              -1,
                              1,
                              rand,
                              false,
                              replace
                              );
            }
            return;
        }

        private void ConnectChainSchema(int poolID, Random rand, ChainSchemaSettings schemaCfg)
        {
            //Length of one chain
            int chainLength = (int)Math.Round(schemaCfg.Ratio * _poolNeuronCollection[poolID].Length);
            if (chainLength < 2)
            {
                //Nothing to do
                return;
            }
            for (int repetition = 1; repetition <= schemaCfg.Repetitions; repetition++)
            {
                List<HiddenNeuron> chainNeuronCollection = RandomlySelectPoolNeurons(poolID, rand, chainLength);
                if (schemaCfg.Circle)
                {
                    chainNeuronCollection.Add(chainNeuronCollection[0]);
                }
                ConnectChainOfNeurons(chainNeuronCollection, rand, schemaCfg.ReplaceExistingConnections);
            }
            return;
        }

        private void ConnectDoubleTwistedToroidSchema(int poolID, Random rand, DoubleTwistedToroidSchemaSettings schemaCfg)
        {
            //Number of neurons to be connected
            int twistSize = (int)Math.Round(schemaCfg.Ratio * _poolNeuronCollection[poolID].Length);
            if (twistSize < 4)
            {
                //Nothing to do
                return;
            }

            for (int repetition = 1; repetition <= schemaCfg.Repetitions; repetition++)
            {
                List<HiddenNeuron> twistNeuronCollection = RandomlySelectPoolNeurons(poolID, rand, twistSize);
                int width = (int)Math.Round(Math.Sqrt(twistNeuronCollection.Count));
                int height = width + (width * width < twistNeuronCollection.Count ? 1 : 0);
                List<HiddenNeuron> chain = new List<HiddenNeuron>(twistNeuronCollection.Count + 1);
                //Double twisted toroid base
                //Chain 1
                chain.Clear();
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        int idx = i * width + j;
                        if (idx < twistNeuronCollection.Count)
                        {
                            chain.Add(twistNeuronCollection[idx]);
                        }
                    }
                }
                chain.Add(twistNeuronCollection[0]);
                ConnectChainOfNeurons(chain, rand, schemaCfg.ReplaceExistingConnections);
                //Chain 2
                chain.Clear();
                for (int j = 0; j < width; j++)
                {
                    for (int i = height - 1; i >= 0; i--)
                    {
                        int idx = i * width + j;
                        if (idx < twistNeuronCollection.Count)
                        {
                            chain.Add(twistNeuronCollection[idx]);
                        }
                    }
                }
                chain.Add(twistNeuronCollection[0]);
                ConnectChainOfNeurons(chain, rand, schemaCfg.ReplaceExistingConnections);
                if (schemaCfg.LDiagonalSelf)
                {
                    //Left diagonal self connections
                    chain.Clear();
                    for (int i = 0; i < width; i++)
                    {
                        int idx = i * width + i;
                        chain.Add(twistNeuronCollection[idx]);
                        chain.Add(twistNeuronCollection[idx]);
                    }
                    ConnectChainOfNeurons(chain, rand, schemaCfg.ReplaceExistingConnections);
                }
                if (schemaCfg.RDiagonalSelf)
                {
                    //Right diagonal self connections
                    chain.Clear();
                    for (int i = width - 1, j = 0; i >= 0; i--, j++)
                    {
                        int idx = j * width + i;
                        chain.Add(twistNeuronCollection[idx]);
                        chain.Add(twistNeuronCollection[idx]);
                    }
                    ConnectChainOfNeurons(chain, rand, schemaCfg.ReplaceExistingConnections);
                }
            }
            return;
        }

        private void SetInterPoolConnection(Random rand, InterPoolConnSettings cfg)
        {
            ProbabilisticSelector<Synapse.SynRole> spikingRoleSelector = new ProbabilisticSelector<Synapse.SynRole>();
            spikingRoleSelector.Add(InstanceCfg.SynapseCfg.SpikingTargetCfg.ExcitatorySynCfg.RelShare, Synapse.SynRole.Excitatory);
            spikingRoleSelector.Add(InstanceCfg.SynapseCfg.SpikingTargetCfg.InhibitorySynCfg.RelShare, Synapse.SynRole.Inhibitory);
            ProbabilisticSelector<int> connCountFluctuationSelector = new ProbabilisticSelector<int>();
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
        /// Resets the reservoir to its initial state.
        /// </summary>
        /// <param name="resetStatistics">Specifies whether to reset also the statistics.</param>
        public void Reset(bool resetStatistics)
        {
            //Reservoir neurons and all linked synapses
            Parallel.For(0, _reservoirNeuronCollection.Length, n =>
            {
                _reservoirNeuronCollection[n].Reset(resetStatistics);
                //Linked input synapses
                foreach (Synapse synapse in _neuronInputSynapsesCollection[n].Values)
                {
                    synapse.Reset(resetStatistics);
                }
                //Linked internal synapses
                foreach (Synapse synapse in _neuronNeuronSynapsesCollection[n].Values)
                {
                    synapse.Reset(resetStatistics);
                }
            });
            return;
        }

        /// <summary>
        /// Performs the computation cycle of the reservoir.
        /// </summary>
        /// <param name="updateStatistics">Specifies whether to update reservoir statistics.</param>
        public void Compute(bool updateStatistics)
        {
            //Set new stimulation on each reservoir neuron
            Parallel.ForEach(_parallelRanges, range =>
            {
                for (int neuronIdx = range.Item1; neuronIdx < range.Item2; neuronIdx++)
                {
                    //Stimulation from input neurons
                    double iStimuli = 0;
                    foreach (Synapse synapse in _neuronInputSynapsesCollection[neuronIdx].Values)
                    {
                        iStimuli += synapse.GetSignal(updateStatistics);
                    }
                    //Stimulation from connected reservoir neurons
                    double rStimuli = 0;
                    foreach (Synapse synapse in _neuronNeuronSynapsesCollection[neuronIdx].Values)
                    {
                        rStimuli += synapse.GetSignal(updateStatistics);
                    }
                    //Store new neuron's stimulation
                    _reservoirNeuronCollection[neuronIdx].NewStimulation(iStimuli, rStimuli);
                }
            });
            //Recompute state of each reservoir neuron
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
        /// Copies the reservoir predictors into a buffer starting from the specified position.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="fromOffset">The starting zero-based position in the buffer.</param>
        public int CopyPredictorsTo(double[] buffer, int fromOffset)
        {
            int offset = fromOffset;
            foreach (HiddenNeuron neuron in PredictingNeuronCollection)
            {
                offset += neuron.CopyPredictorsTo(buffer, offset);
            }
            return offset - fromOffset;
        }

        /// <summary>
        /// Collects the reservoir statistics.
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
                               _neuronInputSynapsesCollection[neuron.Location.ReservoirFlatIdx].Values,
                               _neuronNeuronSynapsesCollection[neuron.Location.ReservoirFlatIdx].Values
                               );
            }
            return resStat;
        }

        //Inner classes
        private class NeuronCreationParams
        {
            public IActivation Activation { get; set; }
            public double Bias { get; set; }
            public int GroupID { get; set; }
            public double AnalogFiringThreshold { get; set; }
            public int AnalogThresholdMaxRefDeepness { get; set; }
            public double RetainmentStrength { get; set; }
            public PredictorsProviderSettings PredictorsCfg { get; set; }
        }//NeuronCreationParams

        private class RelatedNeuron
        {
            //Attribute properties
            public HiddenNeuron Neuron { get; set; }
            public double Distance { get; set; }
        }//RelatedNeuron


    }//Reservoir

}//Namespace
