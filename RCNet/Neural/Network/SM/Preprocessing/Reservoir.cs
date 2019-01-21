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
        /// Reservoir's input neurons.
        /// </summary>
        private readonly INeuron[] _inputNeuronCollection;
        /// <summary>
        /// Neurons within the pools.
        /// </summary>
        private List<INeuron[]> _poolNeuronsCollection;
        /// <summary>
        /// Reservoir's all internal neurons (flat structure).
        /// </summary>
        private INeuron[] _reservoirNeuronCollection;
        /// <summary>
        /// Reservoir's predictor neurons.
        /// </summary>
        private readonly List<PredictorNeuron> _predictorNeuronCollection;
        /// <summary>
        /// Input connections
        /// </summary>
        private readonly Dictionary<int, ISynapse>[] _neuronInputConnectionsCollection;
        /// <summary>
        /// Internal connections
        /// </summary>
        private Dictionary<int, ISynapse>[] _neuronNeuronConnectionsCollection;

        //Attribute properties
        /// <summary>
        /// Reservoir's instance definition
        /// </summary>
        public NeuralPreprocessorSettings.ReservoirInstanceDefinition InstanceDefinition { get; }

        /// <summary>
        /// Number of reservoir's output predictors
        /// </summary>
        public int NumOfOutputPredictors { get; }

        //Constructor
        /// <summary>
        /// Instantiates the reservoir
        /// </summary>
        /// <param name="instanceDefinition">Reservoir instance definition</param>
        /// <param name="inputRange">Range of input values</param>
        /// <param name="rand">Random object to be used for random part initialization </param>
        public Reservoir(NeuralPreprocessorSettings.ReservoirInstanceDefinition instanceDefinition, Interval inputRange, Random rand)
        {
            int numOfInputNodes = instanceDefinition.NPInputFieldIdxCollection.Count;
            //Copy settings
            InstanceDefinition = instanceDefinition.DeepClone();
            
            //-----------------------------------------------------------------------------
            //Initialization of neurons
            //-----------------------------------------------------------------------------
            //Input neurons
            _inputNeuronCollection = new INeuron[numOfInputNodes];
            for(int i = 0; i < numOfInputNodes; i++)
            {
                _inputNeuronCollection[i] = new InputNeuron(InstanceDefinition.Settings.InputEntryPoint, i, inputRange);
            }

            //-----------------------------------------------------------------------------
            //Reservoir neurons
            //-----------------------------------------------------------------------------
            int neuronGlobalFlatIdx = 0;
            List<INeuron> allNeurons = new List<INeuron>();
            _poolNeuronsCollection = new List<INeuron[]>(InstanceDefinition.Settings.PoolSettingsCollection.Count);
            _predictorNeuronCollection = new List<PredictorNeuron>();
            for (int poolID = 0; poolID < InstanceDefinition.Settings.PoolSettingsCollection.Count; poolID++)
            {
                PoolSettings poolSettings = InstanceDefinition.Settings.PoolSettingsCollection[poolID];
                //------------------------------------------------------------------------------------
                //Neuron groups within the pool
                int groupID = 0, idx = 0;
                List<int> analogActivationIdxs = new List<int>();
                List<NeuronCreationParams> neuronParamsCollection = new List<NeuronCreationParams>();
                foreach (PoolSettings.NeuronGroupSettings ngs in poolSettings.NeuronGroups)
                {
                    //Group neuron params
                    for (int i = 0; i < ngs.Count; i++)
                    {
                        NeuronCreationParams neuronParams = new NeuronCreationParams
                        {
                            Activation = ActivationFactory.Create(ngs.ActivationCfg, rand),
                            Role = ngs.Role,
                            Bias = rand.NextDouble(ngs.BiasCfg),
                            GroupID = groupID,
                            RetainmentRate = 0,
                            UseAsPredictor = false
                        };
                        if(neuronParams.Activation.OutputSignalType == CommonEnums.NeuronSignalType.Analog)
                        {
                            analogActivationIdxs.Add(idx);
                        }
                        neuronParamsCollection.Add(neuronParams);
                        ++idx;
                    }
                    ++groupID;
                }//ngs
                //Setup of retainment rates
                if (poolSettings.RetainmentNeuronsFeature)
                {
                    int numOfRetNeurons = (int)Math.Round(poolSettings.RetainmentNeuronsDensity * analogActivationIdxs.Count, 0);
                    rand.Shuffle(analogActivationIdxs);
                    for (int i = 0; i < numOfRetNeurons; i++)
                    {
                        neuronParamsCollection[analogActivationIdxs[i]].RetainmentRate = rand.NextDouble(poolSettings.RetainmentRate);
                    }
                }
                //Setup of readout neurons
                if(poolSettings.ReadoutNeuronsDensity > 0)
                {
                    int numOfReadoutneurons = (int)Math.Round(poolSettings.Dim.Size * poolSettings.ReadoutNeuronsDensity);
                    rand.Shuffle(neuronParamsCollection);
                    for(int i = 0; i < numOfReadoutneurons; i++)
                    {
                        neuronParamsCollection[i].UseAsPredictor = true;
                    }
                }
                //Randomize order before sequential instantiation
                rand.Shuffle(neuronParamsCollection);
                //Instantiate neurons
                INeuron[] poolNeurons = new INeuron[poolSettings.Dim.Size];
                int neuronPoolFlatIdx = 0;
                for (int x = 0; x < poolSettings.Dim.DimX; x++)
                {
                    for (int y = 0; y < poolSettings.Dim.DimY; y++)
                    {
                        for (int z = 0; z < poolSettings.Dim.DimZ; z++)
                        {
                            NeuronPlacement placement = new NeuronPlacement(neuronGlobalFlatIdx, poolID, neuronPoolFlatIdx, neuronParamsCollection[neuronPoolFlatIdx].GroupID, poolSettings.Dim.X + x, poolSettings.Dim.Y + y, poolSettings.Dim.Z + z);
                            //Neuron instance
                            if (neuronParamsCollection[neuronPoolFlatIdx].Activation.OutputSignalType == CommonEnums.NeuronSignalType.Spike)
                            {
                                //Spiking neuron
                                poolNeurons[neuronPoolFlatIdx] = new SpikingNeuron(placement,
                                                                                            neuronParamsCollection[neuronPoolFlatIdx].Role,
                                                                                            neuronParamsCollection[neuronPoolFlatIdx].Activation,
                                                                                            neuronParamsCollection[neuronPoolFlatIdx].Bias
                                                                                            );
                            }
                            else
                            {
                                //Analog neuron
                                poolNeurons[neuronPoolFlatIdx] = new AnalogNeuron(placement,
                                                                                           neuronParamsCollection[neuronPoolFlatIdx].Role,
                                                                                           neuronParamsCollection[neuronPoolFlatIdx].Activation,
                                                                                           neuronParamsCollection[neuronPoolFlatIdx].Bias,
                                                                                           neuronParamsCollection[neuronPoolFlatIdx].RetainmentRate
                                                                                           );
                            }
                            allNeurons.Add(poolNeurons[neuronPoolFlatIdx]);
                            if(neuronParamsCollection[neuronPoolFlatIdx].UseAsPredictor)
                            {
                                PredictorNeuron pn = new PredictorNeuron
                                {
                                    Neuron = poolNeurons[neuronPoolFlatIdx],
                                    UseSecondaryPredictor = (instanceDefinition.AugmentedStates && poolSettings.NeuronGroups[neuronParamsCollection[neuronPoolFlatIdx].GroupID].AugmentedStates)
                                };
                                _predictorNeuronCollection.Add(pn);
                                NumOfOutputPredictors += pn.UseSecondaryPredictor ? 2 : 1;
                            }
                            ++neuronPoolFlatIdx;
                            ++neuronGlobalFlatIdx;
                        }//z
                    }//y
                }//x
                _poolNeuronsCollection.Add(poolNeurons);
            }//PoolID
            //All neurons flat structure
            _reservoirNeuronCollection = allNeurons.ToArray();

            //-----------------------------------------------------------------------------
            //Interconnections
            //-----------------------------------------------------------------------------
            //Connection banks allocations
            _neuronInputConnectionsCollection = new Dictionary<int, ISynapse>[_reservoirNeuronCollection.Length];
            _neuronNeuronConnectionsCollection = new Dictionary<int, ISynapse>[_reservoirNeuronCollection.Length];
            for (int n = 0; n < _reservoirNeuronCollection.Length; n++)
            {
                _neuronInputConnectionsCollection[n] = new Dictionary<int, ISynapse>();
                _neuronNeuronConnectionsCollection[n] = new Dictionary<int, ISynapse>();
            }

            //-----------------------------------------------------------------------------
            //Pools connections
            for (int poolID = 0; poolID < _poolNeuronsCollection.Count; poolID++)
            {
                //Input connection
                SetPoolInputConnections(rand, poolID, instanceDefinition.InputConnectionCollection);
                SetPoolInterconnections(rand, poolID);
            }

            //-----------------------------------------------------------------------------
            //Add Pool to pool connections
            foreach (ReservoirSettings.PoolsInterconnection poolsInterConn in InstanceDefinition.Settings.PoolsInterconnectionCollection)
            {
                SetPool2PoolInterconnections(rand, poolsInterConn);
            }

            //-----------------------------------------------------------------------------
            //Setup of the synaptic delays
            //Build the distances statistics
            BasicStat distanceStat = new BasicStat();
            foreach(Dictionary<int, ISynapse> synapses in _neuronInputConnectionsCollection)
            {
                distanceStat.AddSampleValues((from synapse in synapses.Values select synapse.Distance));
            }
            foreach (Dictionary<int, ISynapse> synapses in _neuronNeuronConnectionsCollection)
            {
                distanceStat.AddSampleValues((from synapse in synapses.Values select synapse.Distance));
            }
            //Delays setup
            //Input synapses
            foreach (Dictionary<int, ISynapse> synapses in _neuronInputConnectionsCollection)
            {
                foreach(ISynapse synapse in synapses.Values)
                {
                    //Compute appropriate delay and set it to synapse
                    if (instanceDefinition.Settings.SynapticDelayMethod == CommonEnums.SynapticDelayMethod.Distance)
                    {
                        double relDistance = (synapse.Distance - distanceStat.Min) / distanceStat.Span;
                        int delay = (int)Math.Round(instanceDefinition.Settings.MaxInputDelay * relDistance);
                        synapse.SetDelay(delay);
                    }
                    else
                    {
                        synapse.SetDelay(rand.Next(instanceDefinition.Settings.MaxInputDelay + 1));
                    }
                }
            }
            //Internal synapses
            foreach (Dictionary<int, ISynapse> synapses in _neuronNeuronConnectionsCollection)
            {
                foreach (ISynapse synapse in synapses.Values)
                {
                    //Compute appropriate delay and set it to synapse
                    if (instanceDefinition.Settings.SynapticDelayMethod == CommonEnums.SynapticDelayMethod.Distance)
                    {
                        double relDistance = (synapse.Distance - distanceStat.Min) / distanceStat.Span;
                        int delay = (int)Math.Round(instanceDefinition.Settings.MaxInternalDelay * relDistance);
                        synapse.SetDelay(delay);
                    }
                    else
                    {
                        synapse.SetDelay(rand.Next(instanceDefinition.Settings.MaxInternalDelay + 1));
                    }
                }
            }

            //-----------------------------------------------------------------------------
            //Spectral radius
            if (InstanceDefinition.Settings.SpectralRadius > 0)
            {
                double maxEigenValue = EstimateLargestEigenValue();
                if(maxEigenValue == 0)
                {
                    throw new Exception("Invalid reservoir weights. Max eigenvalue is 0.");
                }
                double scale = InstanceDefinition.Settings.SpectralRadius / maxEigenValue;
                //Scale internal weights
                foreach(Dictionary<int, ISynapse> connCollection in _neuronNeuronConnectionsCollection)
                {
                    foreach(ISynapse conn in connCollection.Values)
                    {
                        conn.Rescale(scale);
                    }
                }
            }

            return;
        }

        //Properties
        /// <summary>
        /// Reservoir size. (Number of neurons in the reservoir)
        /// </summary>
        public int Size { get { return _reservoirNeuronCollection.Length; } }

        //Methods
        /// <summary>
        /// Estimates the largest eigen value
        /// </summary>
        private double EstimateLargestEigenValue()
        {
            //Create weights matrix
            Matrix wMatrix = new Matrix(_reservoirNeuronCollection.Length, _reservoirNeuronCollection.Length);
            //Interconnections
            Parallel.For(0, _neuronNeuronConnectionsCollection.Length, row =>
            {
                foreach(ISynapse synapse in _neuronNeuronConnectionsCollection[row].Values)
                {
                    wMatrix.Data[row][synapse.SourceNeuron.Placement.ReservoirFlatIdx] = synapse.Weight;
                }
            });
            double maxEV = wMatrix.EstimateLargestEigenValue(out double[] eigenVector);
            return Math.Abs(maxEV);
        }

        /// <summary>
        /// This general function checks the existency of the interconnection between the source and target neurons
        /// </summary>
        /// <param name="connectionsCollection">Bank of synapses</param>
        /// <param name="sourceNeuron">Source neuron</param>
        /// <param name="targetNeuronIdx">An index of the target neuron</param>
        private bool ExistsInterconnection(Dictionary<int, ISynapse>[] connectionsCollection, INeuron sourceNeuron, int targetNeuronIdx)
        {
            return connectionsCollection[targetNeuronIdx].ContainsKey(sourceNeuron.Placement.ReservoirFlatIdx);
        }

        /// <summary>
        /// This general function adds new synapse into the connections bank.
        /// </summary>
        /// <param name="connectionsCollection">Bank of connections</param>
        /// <param name="synapse">A synapse to be added into the bank</param>
        /// <param name="duplicityCheck">Indicates whether to check the interconnection existency before its creation</param>
        /// <returns>Success/Unsuccess</returns>
        private bool AddInterconnection(Dictionary<int, ISynapse>[] connectionsCollection, ISynapse synapse, bool duplicityCheck)
        {
            if (duplicityCheck)
            {
                if(ExistsInterconnection(connectionsCollection, synapse.SourceNeuron, synapse.TargetNeuron.Placement.ReservoirFlatIdx))
                {
                    //Connection already exists
                    return false;
                }
            }
            //Add new connection
            connectionsCollection[synapse.TargetNeuron.Placement.ReservoirFlatIdx].Add(synapse.SourceNeuron.Placement.ReservoirFlatIdx, synapse);
            return true;
        }

        private void SetPoolInputConnections(Random rand, int poolID, List<NeuralPreprocessorSettings.ReservoirInstanceDefinition.InputConnection> inputFieldAssignmentCollection)
        {
            foreach(NeuralPreprocessorSettings.ReservoirInstanceDefinition.InputConnection assignment in inputFieldAssignmentCollection)
            {
                if(assignment.PoolID == poolID)
                {
                    int connectionsPerInput = (int)Math.Round(InstanceDefinition.Settings.PoolSettingsCollection[poolID].Dim.Size * assignment.Density, 0);
                    if (connectionsPerInput > 0)
                    {
                        int[] indices = new int[InstanceDefinition.Settings.PoolSettingsCollection[poolID].Dim.Size];
                        indices.Indices();
                        rand.Shuffle(indices);
                        for (int i = 0; i < connectionsPerInput; i++)
                        {
                            int targetNeuronIdx = indices[i];
                            ISynapse synapse = null;
                            if(assignment.SynapseCfg.GetType() == typeof(StaticSynapseSettings))
                            {
                                StaticSynapseSettings sss = (StaticSynapseSettings)assignment.SynapseCfg;
                                synapse = new StaticSynapse(sourceNeuron: _inputNeuronCollection[assignment.FieldIdx],
                                                            targetNeuron: _poolNeuronsCollection[poolID][targetNeuronIdx],
                                                            weight: rand.NextDouble(sss.WeightCfg)
                                                            );
                            }
                            else
                            {
                                DynamicSynapseSettings dss = (DynamicSynapseSettings)assignment.SynapseCfg;
                                synapse = new DynamicSynapse(sourceNeuron: _inputNeuronCollection[assignment.FieldIdx],
                                                             targetNeuron: _poolNeuronsCollection[poolID][targetNeuronIdx],
                                                             weight: rand.NextDouble(dss.WeightCfg),
                                                             tauFacilitation: dss.TauFacilitation,
                                                             tauRecovery: dss.TauRecovery,
                                                             restingEfficacy: dss.RestingEfficacy,
                                                             tauDecay: dss.TauDecay
                                                             );
                            }
                            AddInterconnection(_neuronInputConnectionsCollection, synapse, false);
                        }
                    }
                }
            }
            return;
        }


        private int ConnectNeurons(Random rand,
                                   int sourcePoolID,
                                   CommonEnums.NeuronRole sourceNeuronRole,
                                   int numOfSourceNeurons,
                                   int targetPoolID,
                                   CommonEnums.NeuronRole targetNeuronRole,
                                   int totalNumOfConnections,
                                   bool constantNumOfNeuronConnections,
                                   Object synapseCfg
                                   )
        {
            //Initial condition
            if (totalNumOfConnections <= 0 || numOfSourceNeurons == 0)
            {
                //No connections will be created
                return 0;
            }
            PoolSettings sourcePoolSettings = InstanceDefinition.Settings.PoolSettingsCollection[sourcePoolID];
            PoolSettings targetPoolSettings = InstanceDefinition.Settings.PoolSettingsCollection[targetPoolID];
            //////////////////////////////////////////////////////////////////////////////////////
            //Collect all possible source neurons
            List<NeuronConnCount> sourceNeuronCollection = (from neuron in _poolNeuronsCollection[sourcePoolID]
                                                            where neuron.Role == sourceNeuronRole
                                                            select new NeuronConnCount { Neuron = neuron, ConnCount = 0 }
                                                            ).ToList();
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
            //Randomize source neurons order
            rand.Shuffle(sourceNeuronCollection);
            //////////////////////////////////////////////////////////////////////////////////////
            //Collect all possible target neurons
            List<INeuron> targetNeuronCollection = (from neuron in _poolNeuronsCollection[targetPoolID]
                                                    where neuron.Role == targetNeuronRole
                                                    select neuron
                                                    ).ToList();
            //////////////////////////////////////////////////////////////////////////////////////
            //Plan number of connections per each source neuron
            bool excludeSourceNeuronFromTarget = (sourcePoolID == targetPoolID && sourceNeuronRole == targetNeuronRole && !sourcePoolSettings.InterconnectionCfg.AllowSelfConnection);
            int maxPhysicalConnCountPerNeuron = targetNeuronCollection.Count - ((excludeSourceNeuronFromTarget) ? 1 : 0);
            //Check condition
            if (maxPhysicalConnCountPerNeuron == 0)
            {
                //No connections will be created
                return 0;
            }
            int averageConnectionsPerNeuron = (int)Math.Round((double)totalNumOfConnections / (double)sourceNeuronCollection.Count);
            if (averageConnectionsPerNeuron < 1) averageConnectionsPerNeuron = 1;
            if (averageConnectionsPerNeuron > maxPhysicalConnCountPerNeuron) averageConnectionsPerNeuron = maxPhysicalConnCountPerNeuron;
            int connectionsCountDown = sourceNeuronCollection.Count * averageConnectionsPerNeuron;
            if(connectionsCountDown > totalNumOfConnections) connectionsCountDown = totalNumOfConnections;
            int minConnCount = int.MaxValue;
            //Basic connection counts plan
            foreach (NeuronConnCount ncc in sourceNeuronCollection)
            {
                //Number of connections for current source neuron
                int connCount = constantNumOfNeuronConnections ? averageConnectionsPerNeuron : (int)Math.Round(rand.NextBoundedGaussianDouble(0, 2 * averageConnectionsPerNeuron));
                if (connCount > maxPhysicalConnCountPerNeuron) connCount = maxPhysicalConnCountPerNeuron;
                if (connCount > connectionsCountDown) connCount = connectionsCountDown;
                ncc.ConnCount = connCount;
                minConnCount = Math.Min(minConnCount, connCount);
                connectionsCountDown -= connCount;
                if (connectionsCountDown == 0)
                {
                    break;
                }
            }
            //Remaining connections count distribution
            while (connectionsCountDown > 0)
            {
                int newMinConnCount = int.MaxValue;
                foreach (NeuronConnCount ncc in sourceNeuronCollection)
                {
                    if (connectionsCountDown > 0 && ncc.ConnCount == minConnCount)
                    {
                        ++ncc.ConnCount;
                        --connectionsCountDown;
                    }
                    newMinConnCount = Math.Min(newMinConnCount, ncc.ConnCount);
                }
                minConnCount = newMinConnCount;
            }


            //////////////////////////////////////////////////////////////////////////////////////
            //Create physical connections
            int numOfCreatedConnections = 0;
            foreach (NeuronConnCount nccSource in (from item in sourceNeuronCollection where item.ConnCount > 0 select item))
            {
                //Copy all possible target neurons
                List<INeuron> tmpRelTargetNeuronCollection = new List<INeuron>(targetNeuronCollection);
                if (excludeSourceNeuronFromTarget)
                {
                    //Exclude source neuron from targets
                    tmpRelTargetNeuronCollection.Remove(nccSource.Neuron);
                }
                //Make connections of source neurons
                for (int connNum = 0; connNum < nccSource.ConnCount; connNum++)
                {
                    int targetNeuronIndex = -1;
                    //Select target neuron to be connected
                    if (sourcePoolID == targetPoolID && sourcePoolSettings.InterconnectionCfg.AvgDistance > 0)
                    {
                        //Selection based on average distance
                        double gaussianDistance = rand.NextGaussianDouble(sourcePoolSettings.InterconnectionCfg.AvgDistance);
                        //Find neuron having closest distance to gaussian distance
                        double minDiff = double.MaxValue;
                        for (int i = 0; i < tmpRelTargetNeuronCollection.Count; i++)
                        {
                            double err = Math.Abs(EuclideanDistance.Compute(nccSource.Neuron.Placement.Coordinates, tmpRelTargetNeuronCollection[i].Placement.Coordinates) - gaussianDistance);
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
                        targetNeuronIndex = rand.Next(tmpRelTargetNeuronCollection.Count);
                    }
                    //Establish connection
                    ISynapse synapse = null;
                    if (synapseCfg.GetType() == typeof(StaticSynapseSettings))
                    {
                        StaticSynapseSettings sss = (StaticSynapseSettings)synapseCfg;
                        synapse = new StaticSynapse(sourceNeuron: nccSource.Neuron,
                                                    targetNeuron: tmpRelTargetNeuronCollection[targetNeuronIndex],
                                                    weight: rand.NextDouble(sss.WeightCfg)
                                                    );
                    }
                    else
                    {
                        DynamicSynapseSettings dss = (DynamicSynapseSettings)synapseCfg;
                        synapse = new DynamicSynapse(sourceNeuron: nccSource.Neuron,
                                                     targetNeuron: tmpRelTargetNeuronCollection[targetNeuronIndex],
                                                     weight: rand.NextDouble(dss.WeightCfg),
                                                     tauFacilitation: dss.TauFacilitation,
                                                     tauRecovery: dss.TauRecovery,
                                                     restingEfficacy: dss.RestingEfficacy,
                                                     tauDecay: dss.TauDecay
                                                     );
                    }
                    if(AddInterconnection(_neuronNeuronConnectionsCollection, synapse, true))
                    {
                        ++numOfCreatedConnections;
                    }
                    //Remove targetNeuron from tmp collection
                    tmpRelTargetNeuronCollection.RemoveAt(targetNeuronIndex);
                }//connNum
            }//nccSource
            return numOfCreatedConnections;
        }


        private void SetPoolInterconnections(Random rand, int poolID)
        {
            PoolSettings poolSettings = InstanceDefinition.Settings.PoolSettingsCollection[poolID];
            //Determine counts
            int totalNumOfSynapses = (int)(Math.Round(((double)poolSettings.Dim.Size)).Power(2) * poolSettings.InterconnectionCfg.Density);
            int countE2E = (int)Math.Round(poolSettings.InterconnectionCfg.RatioEE * totalNumOfSynapses);
            int countE2I = (int)Math.Round(poolSettings.InterconnectionCfg.RatioEI * totalNumOfSynapses);
            int countI2E = (int)Math.Round(poolSettings.InterconnectionCfg.RatioIE * totalNumOfSynapses);
            int countI2I = (int)Math.Round(poolSettings.InterconnectionCfg.RatioII * totalNumOfSynapses);
            totalNumOfSynapses = countE2E + countE2I + countI2E + countI2I;
            //Connections E2E
            ConnectNeurons(rand,
                           poolID,
                           CommonEnums.NeuronRole.Excitatory,
                           -1,
                           poolID,
                           CommonEnums.NeuronRole.Excitatory,
                           countE2E,
                           poolSettings.InterconnectionCfg.ConstantNumOfConnections,
                           poolSettings.InterconnectionCfg.SynapseCfg
                           );
            //Connections E2I
            ConnectNeurons(rand,
                           poolID,
                           CommonEnums.NeuronRole.Excitatory,
                           -1,
                           poolID,
                           CommonEnums.NeuronRole.Inhibitory,
                           countE2I,
                           poolSettings.InterconnectionCfg.ConstantNumOfConnections,
                           poolSettings.InterconnectionCfg.SynapseCfg
                           );
            //Connections I2E
            ConnectNeurons(rand,
                           poolID,
                           CommonEnums.NeuronRole.Inhibitory,
                           -1,
                           poolID,
                           CommonEnums.NeuronRole.Excitatory,
                           countI2E,
                           poolSettings.InterconnectionCfg.ConstantNumOfConnections,
                           poolSettings.InterconnectionCfg.SynapseCfg
                           );
            //Connections I2I
            ConnectNeurons(rand,
                           poolID,
                           CommonEnums.NeuronRole.Inhibitory,
                           -1,
                           poolID,
                           CommonEnums.NeuronRole.Inhibitory,
                           countI2I,
                           poolSettings.InterconnectionCfg.ConstantNumOfConnections,
                           poolSettings.InterconnectionCfg.SynapseCfg
                           );
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
                           CommonEnums.NeuronRole.Excitatory,
                           (int)Math.Round(countE2E/ numOfTargetNeuronsPerSourceNeuron),
                           cfg.TargetPoolID,
                           CommonEnums.NeuronRole.Excitatory,
                           countE2E,
                           cfg.ConstantNumOfConnections,
                           cfg.SynapseCfg
                           );
            //Connections E2I
            ConnectNeurons(rand,
                           cfg.SourcePoolID,
                           CommonEnums.NeuronRole.Excitatory,
                           (int)Math.Round(countE2I / numOfTargetNeuronsPerSourceNeuron),
                           cfg.TargetPoolID,
                           CommonEnums.NeuronRole.Inhibitory,
                           countE2I,
                           cfg.ConstantNumOfConnections,
                           cfg.SynapseCfg
                           );
            //Connections I2E
            ConnectNeurons(rand,
                           cfg.SourcePoolID,
                           CommonEnums.NeuronRole.Inhibitory,
                           (int)Math.Round(countI2E / numOfTargetNeuronsPerSourceNeuron),
                           cfg.TargetPoolID,
                           CommonEnums.NeuronRole.Excitatory,
                           countI2E,
                           cfg.ConstantNumOfConnections,
                           cfg.SynapseCfg
                           );
            //Connections I2I
            ConnectNeurons(rand,
                           cfg.SourcePoolID,
                           CommonEnums.NeuronRole.Inhibitory,
                           (int)Math.Round(countI2I / numOfTargetNeuronsPerSourceNeuron),
                           cfg.TargetPoolID,
                           CommonEnums.NeuronRole.Inhibitory,
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
            ReservoirStat stats = new ReservoirStat(InstanceDefinition.InstanceName, InstanceDefinition.Settings.SettingsName);
            int poolID = 0;
            foreach (PoolSettings poolSettings in InstanceDefinition.Settings.PoolSettingsCollection)
            {
                ReservoirStat.PoolStat poolStat = new ReservoirStat.PoolStat(poolSettings);
                //Neurons statistics
                foreach (INeuron neuron in _poolNeuronsCollection[poolID])
                {
                    poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].MaxActivationStatesStat.AddSampleValue(neuron.Statistics.NormalizedActivationStateStat.Max);
                    poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].AvgActivationStatesStat.AddSampleValue(neuron.Statistics.NormalizedActivationStateStat.ArithAvg);
                    poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].ActivationStateSpansStat.AddSampleValue(neuron.Statistics.NormalizedActivationStateStat.Span);
                    poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].AvgTStimuliStat.AddSampleValue(neuron.Statistics.TStimuliStat.ArithAvg);
                    poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].MaxTStimuliStat.AddSampleValue(neuron.Statistics.TStimuliStat.Max);
                    poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].MinTStimuliStat.AddSampleValue(neuron.Statistics.TStimuliStat.Min);
                    poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].TStimuliSpansStat.AddSampleValue(neuron.Statistics.TStimuliStat.Span);
                    poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].AvgRStimuliStat.AddSampleValue(neuron.Statistics.RStimuliStat.ArithAvg);
                    poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].MaxRStimuliStat.AddSampleValue(neuron.Statistics.RStimuliStat.Max);
                    poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].MinRStimuliStat.AddSampleValue(neuron.Statistics.RStimuliStat.Min);
                    poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].RStimuliSpansStat.AddSampleValue(neuron.Statistics.RStimuliStat.Span);
                    poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].AvgOutputSignalStat.AddSampleValue(neuron.Statistics.OutputSignalStat.ArithAvg);
                    poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].MaxOutputSignalStat.AddSampleValue(neuron.Statistics.OutputSignalStat.Max);
                    poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].MinOutputSignalStat.AddSampleValue(neuron.Statistics.OutputSignalStat.Min);
                    //Synapses efficacy statistics
                    foreach (ISynapse rSynapse in _neuronNeuronConnectionsCollection[neuron.Placement.ReservoirFlatIdx].Values)
                    {
                        poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].AvgSynEfficacyStat.AddSampleValue(rSynapse.EfficacyStat.ArithAvg);
                        poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].MaxSynEfficacyStat.AddSampleValue(rSynapse.EfficacyStat.Max);
                        poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].MinSynEfficacyStat.AddSampleValue(rSynapse.EfficacyStat.Min);
                        poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].SynEfficacySpansStat.AddSampleValue(rSynapse.EfficacyStat.Span);
                    }
                }
                //Weights statistics
                //Input weights
                foreach (Dictionary<int, ISynapse> synapses in _neuronInputConnectionsCollection)
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
                foreach (Dictionary<int, ISynapse> synapses in _neuronNeuronConnectionsCollection)
                {
                    foreach (ISynapse synapse in synapses.Values)
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
        /// Resets all reservoir neurons and other components to their initial state.
        /// Function does not affect weights or internal structure of the resservoir.
        /// </summary>
        /// <param name="resetStatistics">Specifies whether to reset internal statistics</param>
        public void Reset(bool resetStatistics)
        {
            //Input neurons
            foreach(INeuron neuron in _inputNeuronCollection)
            {
                neuron.Reset(resetStatistics);
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
                _inputNeuronCollection[i].NewStimuli(input[i], 0);
                _inputNeuronCollection[i].NewState(updateStatistics);
            }
            //Perform reservoir's computation cycle
            //Collect new stimulation for each reservoir neuron
            var rangePartitioner = Partitioner.Create(0, _reservoirNeuronCollection.Length);
            ;
            Parallel.ForEach(rangePartitioner, (range, loopState) =>
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
                    _reservoirNeuronCollection[neuronIdx].NewStimuli(iStimuli, rStimuli);
                }
            });
            /*
            Parallel.For(0, _reservoirNeuronCollection.Length, neuronIdx =>
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
                _reservoirNeuronCollection[neuronIdx].NewStimuli(iStimuli, rStimuli);
            });
            */
            ;
            //Recompute state of all reservoir neurons
            Parallel.ForEach(rangePartitioner, (range, loopState) =>
            {
                for (int neuronIdx = range.Item1; neuronIdx < range.Item2; neuronIdx++)
                {
                    _reservoirNeuronCollection[neuronIdx].NewState(updateStatistics);
                }
            });
            /*
            Parallel.For(0, _reservoirNeuronCollection.Length, neuronIdx =>
            {
                _reservoirNeuronCollection[neuronIdx].NewState(updateStatistics);
            });
            */
            return;
        }

        /// <summary>
        /// Copies all reservoir predictors to a given buffer starting from the specified possition
        /// </summary>
        public void CopyPredictorsTo(double[] buffer, int fromOffset)
        {
            int bufferIdx = fromOffset;
            foreach(PredictorNeuron pn in _predictorNeuronCollection)
            {
                buffer[bufferIdx] = pn.Neuron.PrimaryPredictor;
                ++bufferIdx;
                if (pn.UseSecondaryPredictor)
                {
                    buffer[bufferIdx] = pn.Neuron.SecondaryPredictor;
                    ++bufferIdx;
                }
            }
            return;
        }

        //Inner classes
        private class NeuronCreationParams
        {
            public CommonEnums.NeuronRole Role { get; set; }
            public IActivationFunction Activation { get; set; }
            public double Bias { get; set; }
            public int GroupID { get; set; }
            public double RetainmentRate { get; set; }
            public bool UseAsPredictor { get; set; }
        }

        [Serializable]
        private class PredictorNeuron
        {
            public INeuron Neuron { get; set; }
            public bool UseSecondaryPredictor { get; set; }
        }

        private class RelatedNeuron
        {
            public INeuron Neuron { get; set; }
            public double Distance { get; set; }
        }

        private class NeuronConnCount
        {
            public INeuron Neuron { get; set; }
            public int ConnCount { get; set; }
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
            /// Collection of the neuron group statistics
            /// </summary>
            public NeuronGroupStat[] NeuronGroupStatCollection { get; }
            
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
            /// <param name="poolSettings">Settings of the neuron pool</param>
            public PoolStat(PoolSettings poolSettings)
            {
                PoolName = poolSettings.Name;
                NeuronGroupStatCollection = new NeuronGroupStat[poolSettings.NeuronGroups.Count];
                for(int i = 0; i < poolSettings.NeuronGroups.Count; i++)
                {
                    NeuronGroupStatCollection[i] = new NeuronGroupStat(poolSettings.NeuronGroups[i].Name);
                }
                InputWeightsStat = new BasicStat();
                InternalWeightsStat = new BasicStat();
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
                /// Statistics of max neurons' activation states
                /// </summary>
                public BasicStat MaxActivationStatesStat { get; }
                /// <summary>
                /// Statistics of avg neurons' activation states
                /// </summary>
                public BasicStat AvgActivationStatesStat { get; }
                /// <summary>
                /// Statistics of spans of the neurons' activation states
                /// </summary>
                public BasicStat ActivationStateSpansStat { get; }
                /// <summary>
                /// Statistics of the neurons' average stimuli (all components)
                /// </summary>
                public BasicStat AvgTStimuliStat { get; }
                /// <summary>
                /// Statistics of the neurons' max stimuli (all components)
                /// </summary>
                public BasicStat MaxTStimuliStat { get; }
                /// <summary>
                /// Statistics of the neurons' min stimuli (all components)
                /// </summary>
                public BasicStat MinTStimuliStat { get; }
                /// <summary>
                /// Statistics of the neurons' stimuli span (all components)
                /// </summary>
                public BasicStat TStimuliSpansStat { get; }
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
                /// Statistics of neurons' average output signals
                /// </summary>
                public BasicStat AvgOutputSignalStat { get; }
                /// <summary>
                /// Statistics of neurons' max output signals
                /// </summary>
                public BasicStat MaxOutputSignalStat { get; }
                /// <summary>
                /// Statistics of neurons' min output signals
                /// </summary>
                public BasicStat MinOutputSignalStat { get; }
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


                //Constructor
                /// <summary>
                /// Creates an unitialized instance
                /// </summary>
                /// <param name="groupName">Name of the neuron group</param>
                public NeuronGroupStat(string groupName)
                {
                    GroupName = groupName;
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
                    AvgOutputSignalStat = new BasicStat();
                    MaxOutputSignalStat = new BasicStat();
                    MinOutputSignalStat = new BasicStat();
                    AvgSynEfficacyStat = new BasicStat();
                    MaxSynEfficacyStat = new BasicStat();
                    MinSynEfficacyStat = new BasicStat();
                    SynEfficacySpansStat = new BasicStat();
                    return;
                }

            }//NeuronGroupStat

        }//PoolStat

    }//ReservoirStat

}//Namespace
