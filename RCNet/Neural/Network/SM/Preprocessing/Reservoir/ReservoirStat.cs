using RCNet.MathTools;
using RCNet.Neural.Network.SM.Preprocessing.Neuron;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
    /// <summary>
    /// Implements the holder of the reservoir instance key statistics.
    /// </summary>
    [Serializable]
    public class ReservoirStat
    {
        //Attributes
        /// <summary>
        /// The name of the reservoir instance.
        /// </summary>
        public string InstanceName { get; }
        /// <summary>
        /// The name of the reservoir structure configuration.
        /// </summary>
        public string StructCfgName { get; }
        /// <summary>
        /// The collection of pool statistics.
        /// </summary>
        public List<PoolStat> Pools { get; }
        /// <summary>
        /// The total number of neurons within the reservoir.
        /// </summary>
        public int TotalNumOfNeurons { get; }
        /// <summary>
        /// The total number of predictors.
        /// </summary>
        public int TotalNumOfPredictors { get; }
        /// <summary>
        /// The statistics of the synapses by the synapse role.
        /// </summary>
        public SynapsesByRoleStat Synapses { get; }
        /// <summary>
        /// The statistics of the anomalies on neurons.
        /// </summary>
        public NeuronsAnomaliesStat NeuronsAnomalies { get; }


        //Constructor
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        /// <param name="instanceName">The name of the reservoir instance.</param>
        /// <param name="structCfg">The name of the reservoir structure configuration.</param>
        /// <param name="numOfNeurons">The total number of neurons within the reservoir.</param>
        /// <param name="numOfPredictors">The total number of predictors.</param>
        public ReservoirStat(string instanceName,
                             ReservoirStructureSettings structCfg,
                             int numOfNeurons,
                             int numOfPredictors
                             )
        {
            InstanceName = instanceName;
            StructCfgName = structCfg.Name;
            TotalNumOfNeurons = numOfNeurons;
            TotalNumOfPredictors = numOfPredictors;
            Pools = new List<PoolStat>();
            foreach (PoolSettings poolCfg in structCfg.PoolsCfg.PoolCfgCollection)
            {
                Pools.Add(new PoolStat(poolCfg));
            }
            Synapses = new SynapsesByRoleStat();
            NeuronsAnomalies = new NeuronsAnomaliesStat();
            return;
        }

        /// <summary>
        /// Updates the statistics.
        /// </summary>
        /// <param name="neuron">The hidden neuron.</param>
        /// <param name="inputSynapses">A collection of the input synapses.</param>
        /// <param name="internalSynapses">A collection of the internal synapses.</param>
        public void Update(HiddenNeuron neuron, IList<Synapse> inputSynapses, IList<Synapse> internalSynapses)
        {
            Synapses.Update(inputSynapses);
            Synapses.Update(internalSynapses);
            NeuronsAnomalies.Update(neuron, internalSynapses.Count);
            Pools[neuron.Location.PoolID].Update(neuron, inputSynapses, internalSynapses);
            return;
        }

        //Inner classes
        /// <summary>
        /// Implements the key statistics of the pool of neurons.
        /// </summary>
        [Serializable]
        public class PoolStat
        {
            /// <summary>
            /// The name of the pool.
            /// </summary>
            public string PoolName { get; }
            /// <summary>
            /// The number of neurons within the pool.
            /// </summary>
            public int NumOfNeurons { get; private set; }
            /// <summary>
            /// The collection of the neuron group statistics.
            /// </summary>
            public NeuronGroupStat[] NeuronGroups { get; }
            /// <summary>
            /// The statistics of the synapses by the synapse role.
            /// </summary>
            public SynapsesByRoleStat Synapses { get; }
            /// <summary>
            /// The statistics of the anomalies on neurons.
            /// </summary>
            public NeuronsAnomaliesStat NeuronsAnomalies { get; }

            //Constructor
            /// <summary>
            /// Creates an initialized instance.
            /// </summary>
            /// <param name="poolCfg">The configuration of the pool of neurons.</param>
            public PoolStat(PoolSettings poolCfg)
            {
                PoolName = poolCfg.Name;
                NumOfNeurons = 0;
                NeuronGroups = new NeuronGroupStat[poolCfg.NeuronGroupsCfg.GroupCfgCollection.Count];
                for (int i = 0; i < poolCfg.NeuronGroupsCfg.GroupCfgCollection.Count; i++)
                {
                    NeuronGroups[i] = new NeuronGroupStat(poolCfg.NeuronGroupsCfg.GroupCfgCollection[i].Name);
                }
                Synapses = new SynapsesByRoleStat();
                NeuronsAnomalies = new NeuronsAnomaliesStat();
                return;
            }

            /// <summary>
            /// Updates the statistics.
            /// </summary>
            /// <param name="neuron">The hidden neuron.</param>
            /// <param name="inputSynapses">A collection of the input synapses.</param>
            /// <param name="internalSynapses">A collection of the internal synapses.</param>
            public void Update(HiddenNeuron neuron, IList<Synapse> inputSynapses, IList<Synapse> internalSynapses)
            {
                ++NumOfNeurons;
                Synapses.Update(inputSynapses);
                Synapses.Update(internalSynapses);
                NeuronsAnomalies.Update(neuron, internalSynapses.Count);
                NeuronGroups[neuron.Location.PoolGroupID].Update(neuron, inputSynapses, internalSynapses);
                return;
            }

            //Inner classes
            /// <summary>
            /// Implements the key statistics of the group of neurons.
            /// </summary>
            [Serializable]
            public class NeuronGroupStat
            {
                /// <summary>
                /// The name of the group.
                /// </summary>
                public string GroupName { get; }
                /// <summary>
                /// The number of neurons in the group.
                /// </summary>
                public int NumOfNeurons { get; set; }
                /// <summary>
                /// The stimulation statistics.
                /// </summary>
                public StimuliStat Stimuli { get; }
                /// <summary>
                /// The activation statistics.
                /// </summary>
                public StandardStatSet Activation { get; }
                /// <summary>
                /// The statistics of the synapses by the synapse role.
                /// </summary>
                public SynapsesByRoleStat Synapses { get; }
                /// <summary>
                /// The statistics of the output signal.
                /// </summary>
                public SignalStat Signal { get; }
                /// <summary>
                /// The statistics of the anomalies on neurons.
                /// </summary>
                public NeuronsAnomaliesStat NeuronsAnomalies { get; }

                //Constructor
                /// <summary>
                /// Creates an unitialized instance.
                /// </summary>
                /// <param name="groupName">The name of the group.</param>
                public NeuronGroupStat(string groupName)
                {
                    GroupName = groupName;
                    NumOfNeurons = 0;
                    Activation = new StandardStatSet();
                    Stimuli = new StimuliStat();
                    Synapses = new SynapsesByRoleStat();
                    Signal = new SignalStat();
                    NeuronsAnomalies = new NeuronsAnomaliesStat();
                    return;
                }

                /// <summary>
                /// Updates the statistics.
                /// </summary>
                /// <param name="neuron">The hidden neuron.</param>
                /// <param name="inputSynapses">A collection of the input synapses.</param>
                /// <param name="internalSynapses">A collection of the internal synapses.</param>
                public void Update(HiddenNeuron neuron, IList<Synapse> inputSynapses, IList<Synapse> internalSynapses)
                {
                    ++NumOfNeurons;
                    Activation.Update(neuron.Statistics.ActivationStat);
                    Stimuli.Update(neuron);
                    Synapses.Update(inputSynapses);
                    Synapses.Update(internalSynapses);
                    Signal.Update(neuron);
                    NeuronsAnomalies.Update(neuron, internalSynapses.Count);
                    return;
                }

            }//NeuronGroupStat

        }//PoolStat

        /// <summary>
        /// Implements the standard set of statistics.
        /// </summary>
        [Serializable]
        public class StandardStatSet
        {
            //Attribute properties
            /// <summary>
            /// The statistics of the minimum value.
            /// </summary>
            public BasicStat MinStat { get; }
            /// <summary>
            /// The statistics of the maximum value.
            /// </summary>
            public BasicStat MaxStat { get; }
            /// <summary>
            /// The statistics of the average value.
            /// </summary>
            public BasicStat AvgStat { get; }
            /// <summary>
            /// The statistics of the span value.
            /// </summary>
            public BasicStat SpanStat { get; }

            //Constructor
            /// <summary>
            /// Creates an initialized instance.
            /// </summary>
            public StandardStatSet()
            {
                MinStat = new BasicStat();
                MaxStat = new BasicStat();
                AvgStat = new BasicStat();
                SpanStat = new BasicStat();
                return;
            }

            /// <summary>
            /// Updates the statistics.
            /// </summary>
            /// <param name="stat">The sample statistics.</param>
            public void Update(BasicStat stat)
            {
                if (stat.NumOfSamples > 0)
                {
                    MinStat.AddSample(stat.Min);
                    MaxStat.AddSample(stat.Max);
                    AvgStat.AddSample(stat.ArithAvg);
                    SpanStat.AddSample(stat.Span);
                }
                return;
            }

        }//StandardStatSet

        /// <summary>
        /// Implements the synapse statistics.
        /// </summary>
        [Serializable]
        public class SynapseStat
        {
            //Attribute properties
            /// <summary>
            /// The role of the synapses.
            /// </summary>
            public Synapse.SynRole Role { get; }
            /// <summary>
            /// The number of synapses.
            /// </summary>
            public int Count { get; private set; }
            /// <summary>
            /// The distance statistics.
            /// </summary>
            public BasicStat Distance { get; }
            /// <summary>
            /// The delay statistics.
            /// </summary>
            public BasicStat Delay { get; }
            /// <summary>
            /// The weight statistics.
            /// </summary>
            public BasicStat Weight { get; }
            /// <summary>
            /// The efficacy statistics.
            /// </summary>
            public StandardStatSet Efficacy { get; }

            //Constructor
            /// <summary>
            /// Creates an initialized instance.
            /// </summary>
            /// <param name="role">The role of the synapses.</param>
            public SynapseStat(Synapse.SynRole role)
            {
                Role = role;
                Count = 0;
                Distance = new BasicStat();
                Delay = new BasicStat();
                Weight = new BasicStat();
                Efficacy = new StandardStatSet();
                return;
            }

            /// <summary>
            /// Updates the statistics.
            /// </summary>
            /// <param name="synapse">The sample synapse.</param>
            public void Update(Synapse synapse)
            {
                if (synapse.Role == Role)
                {
                    ++Count;
                    Distance.AddSample(synapse.Distance);
                    Delay.AddSample(synapse.Delay);
                    Weight.AddSample(synapse.Weight);
                    Efficacy.Update(synapse.EfficacyStat);
                }
                return;
            }
        }//SynapsesStat

        /// <summary>
        /// Implements the synapse statistics by synapse role.
        /// </summary>
        [Serializable]
        public class SynapsesByRoleStat
        {
            /// <summary>
            /// The total number of synapses.
            /// </summary>
            public int Count { get; private set; }
            /// <summary>
            /// The collection of the synapses statistics.
            /// </summary>
            public SynapseStat[] SynapseRole { get; }

            /// <summary>
            /// Creates an initialized instance.
            /// </summary>
            public SynapsesByRoleStat()
            {
                Count = 0;
                SynapseRole = new SynapseStat[Synapse.NumOfRoles];
                for (int i = 0; i < Synapse.NumOfRoles; i++)
                {
                    SynapseRole[i] = new SynapseStat((Synapse.SynRole)i);
                }
                return;
            }

            /// <summary>
            /// Updates the statistics.
            /// </summary>
            /// <param name="synapses">The collection of sample synapses.</param>
            public void Update(IList<Synapse> synapses)
            {
                foreach (Synapse synapse in synapses)
                {
                    ++Count;
                    SynapseRole[(int)synapse.Role].Update(synapse);
                }
                return;
            }

        }//SynapsesByRoleStat

        /// <summary>
        /// Implements the stimulation statistics.
        /// </summary>
        [Serializable]
        public class StimuliStat
        {
            //Attribute properties
            /// <summary>
            /// The statistics of stimuli from input synapses.
            /// </summary>
            public StandardStatSet Input { get; }
            /// <summary>
            /// The statistics of stimuli from internal synapses.
            /// </summary>
            public StandardStatSet Reservoir { get; }
            /// <summary>
            /// The statistics of total stimuli.
            /// </summary>
            public StandardStatSet Total { get; }

            //Constructor
            /// <summary>
            /// Creates an initialized instance.
            /// </summary>
            public StimuliStat()
            {
                Input = new StandardStatSet();
                Reservoir = new StandardStatSet();
                Total = new StandardStatSet();
                return;
            }

            /// <summary>
            /// Updates the statistics.
            /// </summary>
            /// <param name="neuron">The hidden neuron.</param>
            public void Update(HiddenNeuron neuron)
            {
                Input.Update(neuron.Statistics.InputStimuliStat);
                Reservoir.Update(neuron.Statistics.ReservoirStimuliStat);
                Total.Update(neuron.Statistics.TotalStimuliStat);
                return;
            }

        }//StimuliStat

        /// <summary>
        /// Implements the output signal statistics.
        /// </summary>
        [Serializable]
        public class SignalStat
        {
            //Attribute properties
            /// <summary>
            /// The statistics of analog output signal.
            /// </summary>
            public StandardStatSet Analog { get; }
            /// <summary>
            /// The statistics of spiking signal.
            /// </summary>
            public StandardStatSet Spiking { get; }

            //Constructor
            /// <summary>
            /// Creates an initialized instance.
            /// </summary>
            public SignalStat()
            {
                Analog = new StandardStatSet();
                Spiking = new StandardStatSet();
                return;
            }

            /// <summary>
            /// Updates statistics
            /// </summary>
            /// <param name="neuron">The hidden neuron.</param>
            public void Update(HiddenNeuron neuron)
            {
                Analog.Update(neuron.Statistics.AnalogSignalStat);
                Spiking.Update(neuron.Statistics.FiringStat);
                return;
            }

        }//SignalStat

        /// <summary>
        /// Implements the holder of the neurons' behavioral anomalies.
        /// </summary>
        [Serializable]
        public class NeuronsAnomaliesStat
        {
            /// <summary>
            /// The number of neurons having no synapses from other reservoir neurons.
            /// </summary>
            public int NoResSynapses { get; set; }
            /// <summary>
            /// The number of neurons getting no stimulation from connected reservoir neurons.
            /// </summary>
            public int NoResStimuli { get; set; }
            /// <summary>
            /// The number of neurons emitting no output analog signal.
            /// </summary>
            public int NoAnalogOutput { get; set; }
            /// <summary>
            /// The number of neurons emitting the constant output analog signal.
            /// </summary>
            public int ConstAnalogOutput { get; set; }
            /// <summary>
            /// The number of neurons emitting no output spikes.
            /// </summary>
            public int NotFiring { get; set; }
            /// <summary>
            /// The number of neurons emitting the constant output spikes.
            /// </summary>
            public int ConstFiring { get; set; }

            //Constructor
            /// <summary>
            /// Creates an initialized instance.
            /// </summary>
            public NeuronsAnomaliesStat()
            {
                NoResSynapses = 0;
                NoResStimuli = 0;
                NoAnalogOutput = 0;
                ConstAnalogOutput = 0;
                NotFiring = 0;
                ConstFiring = 0;
                return;
            }

            /// <summary>
            /// Updates the anomalies.
            /// </summary>
            /// <param name="neuron">The hidden neuron.</param>
            /// <param name="numOfResSynapses">The number of reservoir synapses.</param>
            public void Update(HiddenNeuron neuron, int numOfResSynapses)
            {
                if (numOfResSynapses == 0)
                {
                    ++NoResSynapses;
                }
                if (neuron.Statistics.ReservoirStimuliStat.NumOfNonzeroSamples == 0 && numOfResSynapses > 0)
                {
                    ++NoResStimuli;
                }
                if (neuron.Statistics.AnalogSignalStat.NumOfNonzeroSamples == 0)
                {
                    ++NoAnalogOutput;
                }
                if (neuron.Statistics.AnalogSignalStat.NumOfNonzeroSamples > 0 && neuron.Statistics.AnalogSignalStat.Span == 0)
                {
                    ++ConstAnalogOutput;
                }
                if (neuron.Statistics.FiringStat.NumOfNonzeroSamples == 0)
                {
                    ++NotFiring;
                }
                if (neuron.Statistics.FiringStat.NumOfNonzeroSamples > 0 && neuron.Statistics.FiringStat.Span == 0)
                {
                    ++ConstFiring;
                }
                return;
            }

        }//NeuronsAnomaliesStat

    }//ReservoirStat

}//Namespace
