using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using RCNet.MathTools;
using RCNet.MathTools.MatrixMath;
using RCNet.Extensions;
using RCNet.Neural.Activation;
using System.Collections.Concurrent;

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

            //-----------------------------------------------------------------------------
            //Initialization of neurons
            //-----------------------------------------------------------------------------
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

            //-----------------------------------------------------------------------------
            //Reservoir neurons
            //-----------------------------------------------------------------------------
            int neuronGlobalFlatIdx = 0;
            List<INeuron> allNeurons = new List<INeuron>();
            _poolNeuronsCollection = new List<INeuron[]>(_settings.PoolSettingsCollection.Count);
            for (int poolID = 0; poolID < _settings.PoolSettingsCollection.Count; poolID++)
            {
                PoolSettings poolSettings = _settings.PoolSettingsCollection[poolID];
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
                            Activation = ActivationFactory.Create(ngs.ActivationSettings, _rand),
                            Role = ngs.Role,
                            Bias = _rand.NextDouble(ngs.BiasSettings),
                            GroupID = groupID,
                            RetainmentRate = 0
                        };
                        if(neuronParams.Activation.OutputSignalType == ActivationFactory.FunctionOutputSignalType.Analog)
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
                    _rand.Shuffle(analogActivationIdxs);
                    for (int i = 0; i < numOfRetNeurons; i++)
                    {
                        neuronParamsCollection[analogActivationIdxs[i]].RetainmentRate = _rand.NextDouble(poolSettings.RetainmentRate);
                    }
                }
                //Randomize order before sequential instantiation
                _rand.Shuffle(neuronParamsCollection);
                //Instantiate neurons
                INeuron[] poolNeurons = new INeuron[poolSettings.Dim.Size];
                int neuronPoolFlatIdx = 0;
                for (int x = 0; x < poolSettings.Dim.X; x++)
                {
                    for (int y = 0; y < poolSettings.Dim.Y; y++)
                    {
                        for (int z = 0; z < poolSettings.Dim.Z; z++)
                        {
                            NeuronPlacement placement = new NeuronPlacement(neuronGlobalFlatIdx, poolID, poolSettings.Dim, neuronPoolFlatIdx, neuronParamsCollection[neuronPoolFlatIdx].GroupID, x, y, z);
                            bool useSecondaryPredictor = false;
                            if (poolSettings.RouteToReadout)
                            {
                                useSecondaryPredictor = (instanceDefinition.AugmentedStates && poolSettings.NeuronGroups[neuronParamsCollection[neuronPoolFlatIdx].GroupID].AugmentedStates);
                                _numOfPredictors += useSecondaryPredictor ? 2 : 1;
                            }
                            //Neuron instance
                            if (neuronParamsCollection[neuronPoolFlatIdx].Activation.OutputSignalType == ActivationFactory.FunctionOutputSignalType.Spike)
                            {
                                //Spiking neuron
                                poolNeurons[neuronPoolFlatIdx] = new ReservoirSpikingNeuron(placement,
                                                                                            neuronParamsCollection[neuronPoolFlatIdx].Role,
                                                                                            neuronParamsCollection[neuronPoolFlatIdx].Activation,
                                                                                            neuronParamsCollection[neuronPoolFlatIdx].Bias,
                                                                                            useSecondaryPredictor
                                                                                            );
                            }
                            else
                            {
                                //Analog neuron
                                poolNeurons[neuronPoolFlatIdx] = new ReservoirAnalogNeuron(placement,
                                                                                           neuronParamsCollection[neuronPoolFlatIdx].Role,
                                                                                           neuronParamsCollection[neuronPoolFlatIdx].Activation,
                                                                                           neuronParamsCollection[neuronPoolFlatIdx].Bias,
                                                                                           neuronParamsCollection[neuronPoolFlatIdx].RetainmentRate,
                                                                                           useSecondaryPredictor
                                                                                           );
                            }
                            allNeurons.Add(poolNeurons[neuronPoolFlatIdx]);
                            ++neuronPoolFlatIdx;
                            ++neuronGlobalFlatIdx;
                        }//z
                    }//y
                }//x
                _poolNeuronsCollection.Add(poolNeurons);
            }//PoolID
            //All neurons flat structure
            _neurons = allNeurons.ToArray();

            //-----------------------------------------------------------------------------
            //Interconnections
            //-----------------------------------------------------------------------------
            //Connection banks allocations
            _neuronInputConnectionsCollection = new List<ISynapse>[_neurons.Length];
            _neuronNeuronConnectionsCollection = new List<ISynapse>[_neurons.Length];
            for (int n = 0; n < _neurons.Length; n++)
            {
                _neuronInputConnectionsCollection[n] = new List<ISynapse>();
                _neuronNeuronConnectionsCollection[n] = new List<ISynapse>();
            }

            //-----------------------------------------------------------------------------
            //Pools connections
            for (int poolID = 0; poolID < _poolNeuronsCollection.Count; poolID++)
            {
                //Input connection
                SetPoolInputConnections(poolID, instanceDefinition.InputFieldAssignmentCollection);
                /*
                 * Old approach
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
                */
                SetPoolInterconnections(poolID, _settings.PoolSettingsCollection[poolID]);
            }

            //-----------------------------------------------------------------------------
            //Add Pool to pool connections
            foreach (ReservoirSettings.PoolsInterconnection poolsInterConn in _settings.PoolsInterconnectionCollection)
            {
                SetPool2PoolInterconnections(poolsInterConn);
            }
            //-----------------------------------------------------------------------------
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
        public int Size { get { return _neurons.Length; } }

        /// <summary>
        /// Number of reservoir's output predictors
        /// </summary>
        public int NumOfOutputPredictors { get { return _numOfPredictors; } }

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
                    int col = _neuronNeuronConnectionsCollection[row][connIdx].SourceNeuron.Placement.ReservoirFlatIdx;
                    double weight = _neuronNeuronConnectionsCollection[row][connIdx].Weight;
                    wMatrix.Data[row][col] = weight;
                }
            });

            double maxEV = 0;
            //Slow approach - full eigen value decomposition
            //EVD eigenvaluesDecomposition = new EVD(wMatrix);
            //double maxEVFull = eigenvaluesDecomposition.MaxAbsRealEigenvalue;

            maxEV = wMatrix.EstimateLargestEigenValue(out double[] eigenVector);
            return Math.Abs(maxEV);
        }

        /// <summary>
        /// This general function checks the existency of the interconnection between the source and a target neuron
        /// </summary>
        /// <param name="connectionsCollection">Bank of synapses</param>
        /// <param name="sourceNeuron">Source neuron</param>
        /// <param name="targetNeuronIdx">An index of the target neuron</param>
        private bool ExistsInterconnection(List<ISynapse>[] connectionsCollection, INeuron sourceNeuron, int targetNeuronIdx)
        {
            //Try to select the same synapse
            ISynapse equalConn = (from connection in connectionsCollection[targetNeuronIdx]
                                  where connection.SourceNeuron.Placement.ReservoirFlatIdx == sourceNeuron.Placement.ReservoirFlatIdx
                                  select connection
                                  ).FirstOrDefault();
            return (equalConn != null);
        }

        /// <summary>
        /// This general function adds new synapse into the connections bank.
        /// </summary>
        /// <param name="connectionsCollection">Bank of connections</param>
        /// <param name="synapse">A synapse to be added into the bank</param>
        /// <param name="duplicityCheck">Indicates whether to check the interconnection existency before its creation</param>
        /// <returns>Success/Unsuccess</returns>
        private bool AddInterconnection(List<ISynapse>[] connectionsCollection, ISynapse synapse, bool duplicityCheck)
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
            connectionsCollection[synapse.TargetNeuron.Placement.ReservoirFlatIdx].Add(synapse);
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
                            /*
                            StaticSynapse synapse = new StaticSynapse(_inputNeurons[assignment.FieldIdx],
                                                                      _poolNeuronsCollection[poolID][targetNeuronIdx],
                                                                      _rand.NextDouble(assignment.SynapseWeight),
                                                                      5
                                                                      );
                            */
                            ///*
                            DynDecaySynapse synapse = new DynDecaySynapse(_inputNeurons[assignment.FieldIdx],
                                                                        _poolNeuronsCollection[poolID][targetNeuronIdx],
                                                                        _rand.NextDouble(assignment.SynapseWeight),
                                                                        3,
                                                                        10
                                                                        );
                            //*/
                            AddInterconnection(_neuronInputConnectionsCollection, synapse, false);
                        }
                    }
                }
            }
            return;
        }

        /*
         * NEW WIRING APPROACH
         * 
         */

        private List<NeuronConnCount> ExtractNeurons(int poolID, CommonEnums.NeuronRole neuronRole)
        {
            return (from neuron in _poolNeuronsCollection[poolID]
                    where neuron.Role == neuronRole
                    select new NeuronConnCount { Neuron = neuron, ConnCount = 0 }
                    ).ToList();
        }

        private List<RelatedNeuron> ExtractRelatedPoolNeurons(INeuron relatedNeuron, CommonEnums.NeuronRole neuronRole, bool allowSelf)
        {
            return (from neuron in _poolNeuronsCollection[relatedNeuron.Placement.PoolID]
                    where (neuron.Role == neuronRole && (allowSelf || (neuron != relatedNeuron)))
                    select new RelatedNeuron { Neuron = neuron,
                                                 Distance = relatedNeuron.Placement.ComputeEuclideanDistance(neuron.Placement)
                                                }).ToList();
        }

        private void ConnectPoolNeurons(int poolID,
                                        PoolSettings poolSettings,
                                        CommonEnums.NeuronRole sourceNeuronRole,
                                        CommonEnums.NeuronRole targetNeuronRole,
                                        int totalNumOfConnections,
                                        bool allowSelfConnection,
                                        double averageDistance
                                        )
        {
            if (totalNumOfConnections <= 0) return;
            //////////////////////////////////////////////////////////////////////////////////////
            //Collect all possible source neurons
            List<NeuronConnCount> sourceNeuronCollection = ExtractNeurons(poolID, sourceNeuronRole);
            //Randomize source neurons order
            _rand.Shuffle(sourceNeuronCollection);
            //Plan number of connections per each source neuron
            int connectionsCountDown = totalNumOfConnections;
            int averageConnectionsPerNeuron = (int)Math.Round((double)totalNumOfConnections / (double)sourceNeuronCollection.Count);
            if (averageConnectionsPerNeuron < 1) averageConnectionsPerNeuron = 1;
            int maxPhysicalConnCountPerNeuron = _poolNeuronsCollection[poolID].Length - ((allowSelfConnection) ? 1 : 0);
            int minConnCount = int.MaxValue;
            //Basic connection counts plan
            foreach (NeuronConnCount ncc in sourceNeuronCollection)
            {
                //Number of connections for current source neuron
                int connCount = poolSettings.ConstantNumOfConnections ? averageConnectionsPerNeuron : (int)Math.Round(_rand.NextBoundedGaussianDouble(0, 2 * averageConnectionsPerNeuron));
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
            while(connectionsCountDown > 0)
            {
                int newMinConnCount = int.MaxValue;
                foreach (NeuronConnCount ncc in sourceNeuronCollection)
                {
                    if(connectionsCountDown > 0 && ncc.ConnCount == minConnCount)
                    {
                        ++ncc.ConnCount;
                        --connectionsCountDown;
                    }
                    newMinConnCount = Math.Min(newMinConnCount, ncc.ConnCount);
                }
                minConnCount = newMinConnCount;
            }


            //////////////////////////////////////////////////////////////////////////////////////
            //Create connections
            foreach(NeuronConnCount nccSource in (from item in sourceNeuronCollection where item.ConnCount > 0 select item))
            {
                //Collect all possible target neurons
                List<RelatedNeuron> relTargetNeuronCollection = ExtractRelatedPoolNeurons(nccSource.Neuron, targetNeuronRole, allowSelfConnection);
                //Shuffle source neurons order
                _rand.Shuffle(relTargetNeuronCollection);
                for(int connNum = 0; connNum < nccSource.ConnCount; connNum++)
                {
                    List<RelatedNeuron> tmpRelTargetNeuronCollection = new List<RelatedNeuron>(relTargetNeuronCollection);
                    int targetNeuronIndex = -1;
                    //Select target neuron to be connected
                    if(averageDistance <= 0)
                    {
                        //Pure random selection
                        targetNeuronIndex = _rand.Next(tmpRelTargetNeuronCollection.Count);
                    }
                    else
                    {
                        //Selection based on average distance
                        double gaussianDistance = _rand.NextGaussianDouble(averageDistance);
                        //Find neuron having closest distance to gaussian distance
                        double minDiff = double.MaxValue;
                        for(int i = 0; i < tmpRelTargetNeuronCollection.Count; i++)
                        {
                            double err = Math.Abs(tmpRelTargetNeuronCollection[i].Distance - gaussianDistance);
                            if(err < minDiff)
                            {
                                targetNeuronIndex = i;
                                minDiff = err;
                            }
                        }
                    }
                    INeuron targetNeuron = tmpRelTargetNeuronCollection[targetNeuronIndex].Neuron;
                    //Remove targetNeuron from tmp collection
                    tmpRelTargetNeuronCollection.RemoveAt(targetNeuronIndex);
                    //Establish connection
                    /*
                    StaticSynapse synapse = new StaticSynapse(nccSource.Neuron,
                                                              targetNeuron,
                                                              _rand.NextDouble(poolSettings.InterconnectionSynapseWeight)
                                                              );
                    */
                    /*
                    DynDecaySynapse synapse = new DynDecaySynapse(nccSource.Neuron,
                                                                  targetNeuron,
                                                                  _rand.NextDouble(poolSettings.InterconnectionSynapseWeight),
                                                                  5,
                                                                  10
                                                                  );
                    */
                    ///*
                    DynSTPSynapse synapse = new DynSTPSynapse(nccSource.Neuron,
                                                              targetNeuron,
                                                              _rand.NextDouble(poolSettings.InterconnectionSynapseWeight),
                                                              6,
                                                              10,
                                                              10,
                                                              0.5
                                                              );
                    //*/
                    AddInterconnection(_neuronNeuronConnectionsCollection, synapse, false);
                }//connNum
            }//nccSource
            return;
        }

        private void SetPoolInterconnections(int poolID, PoolSettings poolSettings)
        {
            //Determine counts
            int totalNumOfSynapses = (int)(Math.Round(((double)poolSettings.Dim.Size)).Power(2) * poolSettings.InterconnectionDensity);
            /*
            int countE2E = (int)Math.Round(0.6d * totalNumOfSynapses);
            int countE2I = (int)Math.Round(0.3d * totalNumOfSynapses);
            int countI2E = (int)Math.Round(0.05d * totalNumOfSynapses);
            int countI2I = (int)Math.Round(0.05d * totalNumOfSynapses);
            */
            ///*
            int countE2E = (int)Math.Round(0.3d * totalNumOfSynapses);
            int countE2I = (int)Math.Round(0.2d * totalNumOfSynapses);
            int countI2E = (int)Math.Round(0.4d * totalNumOfSynapses);
            int countI2I = (int)Math.Round(0.1d * totalNumOfSynapses);
            //*/
            totalNumOfSynapses = countE2E + countE2I + countI2E + countI2I;
            //Connections E2E
            ConnectPoolNeurons(poolID,
                               poolSettings,
                               CommonEnums.NeuronRole.Excitatory,
                               CommonEnums.NeuronRole.Excitatory,
                               countE2E,
                               poolSettings.InterconnectionAllowSelfConn,
                               poolSettings.InterconnectionAvgDistance
                               );
            //Connections E2I
            ConnectPoolNeurons(poolID,
                               poolSettings,
                               CommonEnums.NeuronRole.Excitatory,
                               CommonEnums.NeuronRole.Inhibitory,
                               countE2I,
                               poolSettings.InterconnectionAllowSelfConn,
                               poolSettings.InterconnectionAvgDistance
                               );
            //Connections I2E
            ConnectPoolNeurons(poolID,
                               poolSettings,
                               CommonEnums.NeuronRole.Inhibitory,
                               CommonEnums.NeuronRole.Excitatory,
                               countI2E,
                               poolSettings.InterconnectionAllowSelfConn,
                               poolSettings.InterconnectionAvgDistance
                               );
            //Connections I2I
            ConnectPoolNeurons(poolID,
                               poolSettings,
                               CommonEnums.NeuronRole.Inhibitory,
                               CommonEnums.NeuronRole.Inhibitory,
                               countI2I,
                               poolSettings.InterconnectionAllowSelfConn,
                               poolSettings.InterconnectionAvgDistance
                               );

            return;
        }


        /*
         * OLD APPROACH
         * 
         */

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
                    int connCount = poolSettings.ConstantNumOfConnections ? connectionsPerNeuron : (int)Math.Round(_rand.NextBoundedGaussianDouble(1, 2 * connectionsPerNeuron));
                    for (int i = 0; i < indices.Length && addedSynapses < connCount; i++)
                    {
                        int srcNeuronIdx = indices[i];
                        if (poolSettings.InterconnectionAllowSelfConn || srcNeuronIdx != targetNeuronIdx)
                        {
                            StaticSynapse synapse = new StaticSynapse(_poolNeuronsCollection[poolID][srcNeuronIdx],
                                                                      _poolNeuronsCollection[poolID][targetNeuronIdx],
                                                                      _rand.NextDouble(poolSettings.InterconnectionSynapseWeight),
                                                                      0
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
                    int connCount = poolSettings.ConstantNumOfConnections ? connectionsPerNeuron : (int)Math.Round(_rand.NextBoundedGaussianDouble(1, 2 * connectionsPerNeuron));
                    List<INeuron> srcNeurons = SelectNeuronsByDistance(_poolNeuronsCollection[poolID][targetNeuronIdx], _poolNeuronsCollection[poolID], poolSettings.InterconnectionAvgDistance, connCount, poolSettings.InterconnectionAllowSelfConn);
                    int addedSynapses = 0;
                    for (int i = 0; i < srcNeurons.Count; i++)
                    {
                        StaticSynapse synapse = new StaticSynapse(srcNeurons[i],
                                                                    _poolNeuronsCollection[poolID][targetNeuronIdx],
                                                                    _rand.NextDouble(poolSettings.InterconnectionSynapseWeight),
                                                                    0
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
            if (numOfSrcNeurons > 0)
            {
                for (int i = 0; i < numOfTargetNeurons; i++)
                {
                    INeuron targetneuron = _poolNeuronsCollection[cfg.TargetPoolID][targetIndices[i]];
                    _rand.Shuffle(srcIndices);
                    int connCount = cfg.ConstantNumOfConnections ? numOfSrcNeurons : (int)Math.Round(_rand.NextBoundedGaussianDouble(1, 2 * numOfSrcNeurons));
                    for (int j = 0; j < connCount && j < srcIndices.Length; j++)
                    {
                        INeuron srcNeuron = _poolNeuronsCollection[cfg.SourcePoolID][srcIndices[j]];
                        StaticSynapse synapse = new StaticSynapse(srcNeuron,
                                                                  targetneuron,
                                                                  _rand.NextDouble(cfg.SynapseWeight),
                                                                  0
                                                                  );
                        AddInterconnection(_neuronNeuronConnectionsCollection, synapse, false);
                    }
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
                ReservoirStat.PoolStat poolStat = new ReservoirStat.PoolStat(poolSettings);
                //Neurons statistics
                foreach (INeuron neuron in _poolNeuronsCollection[poolID])
                {
                    poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].MaxActivationStatesStat.AddSampleValue(neuron.Statistics.NormalizedActivationStateStat.Max);
                    poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].AvgActivationStatesStat.AddSampleValue(neuron.Statistics.NormalizedActivationStateStat.ArithAvg);
                    poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].ActivationStateSpansStat.AddSampleValue(neuron.Statistics.NormalizedActivationStateStat.Span);
                    poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].AvgStimuliStat.AddSampleValue(neuron.Statistics.StimuliStat.ArithAvg);
                    poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].MaxStimuliStat.AddSampleValue(neuron.Statistics.StimuliStat.Max);
                    poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].MinStimuliStat.AddSampleValue(neuron.Statistics.StimuliStat.Min);
                    poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].StimuliSpansStat.AddSampleValue(neuron.Statistics.StimuliStat.Span);
                    poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].AvgOutputSignalStat.AddSampleValue(neuron.Statistics.OutputSignalStat.ArithAvg);
                    poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].MaxOutputSignalStat.AddSampleValue(neuron.Statistics.OutputSignalStat.Max);
                    poolStat.NeuronGroupStatCollection[neuron.Placement.GroupID].MinOutputSignalStat.AddSampleValue(neuron.Statistics.OutputSignalStat.Min);
                }
                //Weights statistics
                //Input weights
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
                //Internal weights
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
            //Reservoir neurons and all linked synapses
            Parallel.For(0, _neurons.Length, n =>
            {
                _neurons[n].Reset(resetStatistics);
                //Linked input synapses
                foreach (ISynapse synapse in _neuronInputConnectionsCollection[n])
                {
                    synapse.Reset();
                }
                //Linked internal synapses
                foreach (ISynapse synapse in _neuronInputConnectionsCollection[n])
                {
                    synapse.Reset();
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
            OrderablePartitioner<Tuple<int, int>> rangePartitioner = Partitioner.Create(0, _neurons.Length);
            //Set input to input neurons
            for (int i = 0; i < input.Length; i++)
            {
                _inputNeurons[i].NewStimuli(input[i], 0);
            }
            //Perform computation cycles
            for (int cycle = 0; cycle < _settings.InputDuration; cycle++)
            {
                //Prepare input fetching
                for (int i = 0; i < input.Length; i++)
                {
                    _inputNeurons[i].NewState(updateStatistics);
                }
                //Collect new stimulation for each reservoir neuron
                Parallel.ForEach(rangePartitioner, range =>
                {
                    for (int neuronIdx = range.Item1; neuronIdx < range.Item2; neuronIdx++)
                    {
                        //Signal from input neurons
                        double externalStimuli = 0;
                        foreach (ISynapse synapse in _neuronInputConnectionsCollection[neuronIdx])
                        {
                            externalStimuli += synapse.GetSignal();
                        }
                        //Signal from connected reservoir neurons
                        double internalStimuli = 0;
                        foreach (ISynapse synapse in _neuronNeuronConnectionsCollection[neuronIdx])
                        {
                            internalStimuli += synapse.GetSignal();
                        }
                        //Store new neuron's stimulation
                        _neurons[neuronIdx].NewStimuli(externalStimuli, internalStimuli);
                    }
                });
                //Recompute all reservoir neurons
                Parallel.ForEach(rangePartitioner, range =>
                {
                    for (int neuronIdx = range.Item1; neuronIdx < range.Item2; neuronIdx++)
                    {
                        _neurons[neuronIdx].NewState(updateStatistics);
                    }
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
                        buffer[bufferIdx] = _poolNeuronsCollection[poolID][n].PrimaryPredictor;
                        ++bufferIdx;
                        if (_poolNeuronsCollection[poolID][n].UseSecondaryPredictor)
                        {
                            buffer[bufferIdx] = _poolNeuronsCollection[poolID][n].SecondaryPredictor;
                            ++bufferIdx;
                        }
                    }
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
                /// Statistics of the neurons' average stimuli
                /// </summary>
                public BasicStat AvgStimuliStat { get; }
                /// <summary>
                /// Statistics of the neurons' max stimuli
                /// </summary>
                public BasicStat MaxStimuliStat { get; }
                /// <summary>
                /// Statistics of the neurons' min stimuli
                /// </summary>
                public BasicStat MinStimuliStat { get; }
                /// <summary>
                /// Statistics of the neurons' stimuli span
                /// </summary>
                public BasicStat StimuliSpansStat { get; }
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
                    AvgStimuliStat = new BasicStat();
                    MaxStimuliStat = new BasicStat();
                    MinStimuliStat = new BasicStat();
                    StimuliSpansStat = new BasicStat();
                    AvgOutputSignalStat = new BasicStat();
                    MaxOutputSignalStat = new BasicStat();
                    MinOutputSignalStat = new BasicStat();
                    return;
                }

            }//NeuronGroupStat

        }//PoolStat

    }//ReservoirStat

}//Namespace
