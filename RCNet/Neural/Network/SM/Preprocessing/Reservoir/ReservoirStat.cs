using RCNet.MathTools;
using RCNet.Neural.Network.SM.Preprocessing.Neuron;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
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
        public string InstanceName { get; }
        /// <summary>
        /// Name of the reservoir structure configuration
        /// </summary>
        public string StructCfgName { get; }
        /// <summary>
        /// Collection of resrvoir pools stats
        /// </summary>
        public List<PoolStat> Pools { get; }
        /// <summary>
        /// Total number of neurons within the reservoir
        /// </summary>
        public int TotalNumOfNeurons { get; }
        /// <summary>
        /// Total number of predictors
        /// </summary>
        public int TotalNumOfPredictors { get; }
        /// <summary>
        /// Statistics of synapses
        /// </summary>
        public SynapsesByRoleStat Synapses { get; }
        /// <summary>
        /// Neurons anomalies
        /// </summary>
        public NeuronsAnomaliesStat NeuronsAnomalies { get; }


        //Constructor
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        /// <param name="instanceName">Name of the reservoir instance</param>
        /// <param name="structCfg">Reservoir structure configuration</param>
        /// <param name="numOfNeurons">Total number of neurons</param>
        /// <param name="numOfPredictors">Total number of predictors</param>
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
        /// Updates statistics
        /// </summary>
        /// <param name="neuron">Hidden neuron</param>
        /// <param name="inputSynapses">Input synaapses</param>
        /// <param name="internalSynapses">Internal synapses</param>
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
            public int NumOfNeurons { get; private set; }
            /// <summary>
            /// Collection of the neuron group statistics
            /// </summary>
            public NeuronGroupStat[] NeuronGroups { get; }
            /// <summary>
            /// Statistics of synapses
            /// </summary>
            public SynapsesByRoleStat Synapses { get; }
            /// <summary>
            /// Neurons anomalies
            /// </summary>
            public NeuronsAnomaliesStat NeuronsAnomalies { get; }

            //Constructor
            /// <summary>
            /// Creates an initialized instance
            /// </summary>
            /// <param name="poolCfg">Configuration of the pool of neurons</param>
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
            /// Updates statistics
            /// </summary>
            /// <param name="neuron">Hidden neuron</param>
            /// <param name="inputSynapses">Neuron's input synapses</param>
            /// <param name="internalSynapses">Neuron's reservoir synapses</param>
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
            /// Key statistics of the group of neurons
            /// </summary>
            [Serializable]
            public class NeuronGroupStat
            {
                /// <summary>
                /// Name of the group
                /// </summary>
                public string GroupName { get; }
                /// <summary>
                /// Number of neurons in the group
                /// </summary>
                public int NumOfNeurons { get; set; }
                /// <summary>
                /// Stimulation statistics
                /// </summary>
                public StimuliStat Stimuli { get; }
                /// <summary>
                /// Activation statistics
                /// </summary>
                public StandardStatSet Activation { get; }
                /// <summary>
                /// Statistics of synapses
                /// </summary>
                public SynapsesByRoleStat Synapses { get; }
                /// <summary>
                /// Statistics of the output signals
                /// </summary>
                public SignalStat Signal { get; }
                /// <summary>
                /// Neuron anomalies
                /// </summary>
                public NeuronsAnomaliesStat NeuronsAnomalies { get; }

                //Constructor
                /// <summary>
                /// Creates an unitialized instance
                /// </summary>
                /// <param name="groupName">Name of the neuron group</param>
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
                /// Updates statistics
                /// </summary>
                /// <param name="neuron">Hidden neuron</param>
                /// <param name="inputSynapses">Neuron's input synapses</param>
                /// <param name="internalSynapses">Neuron's reservoir synapses</param>
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
        /// Standard set of statistics
        /// </summary>
        [Serializable]
        public class StandardStatSet
        {
            //Attribute properties
            /// <summary>
            /// Minimum values statistics
            /// </summary>
            public BasicStat MinStat { get; }
            /// <summary>
            /// Maximum values statistics
            /// </summary>
            public BasicStat MaxStat { get; }
            /// <summary>
            /// Average values statistics
            /// </summary>
            public BasicStat AvgStat { get; }
            /// <summary>
            /// Min Max span statistics
            /// </summary>
            public BasicStat SpanStat { get; }

            //Constructor
            /// <summary>
            /// Creates an initialized instance
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
            /// Updates statistics
            /// </summary>
            /// <param name="stat">Basic stat</param>
            public void Update(BasicStat stat)
            {
                if (stat.NumOfSamples > 0)
                {
                    MinStat.AddSampleValue(stat.Min);
                    MaxStat.AddSampleValue(stat.Max);
                    AvgStat.AddSampleValue(stat.ArithAvg);
                    SpanStat.AddSampleValue(stat.Span);
                }
                return;
            }

        }//StandardStatSet

        /// <summary>
        /// Synapse statistics
        /// </summary>
        [Serializable]
        public class SynapseStat
        {
            //Attribute properties
            /// <summary>
            /// Role of the synapses
            /// </summary>
            public Synapse.SynRole Role { get; }
            /// <summary>
            /// Number of synapses
            /// </summary>
            public int Count { get; private set; }
            /// <summary>
            /// Distance statistics
            /// </summary>
            public BasicStat Distance { get; }
            /// <summary>
            /// Delay statistics
            /// </summary>
            public BasicStat Delay { get; }
            /// <summary>
            /// Weight statistics
            /// </summary>
            public BasicStat Weight { get; }
            /// <summary>
            /// Efficacy statistics
            /// </summary>
            public StandardStatSet Efficacy { get; }

            //Constructor
            /// <summary>
            /// Creates an initialized instance
            /// </summary>
            /// <param name="role">Role of the synapses</param>
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
            /// Updates statistics
            /// </summary>
            /// <param name="synapse">Synapse</param>
            public void Update(Synapse synapse)
            {
                if (synapse.Role == Role)
                {
                    ++Count;
                    Distance.AddSampleValue(synapse.Distance);
                    Delay.AddSampleValue(synapse.Delay);
                    Weight.AddSampleValue(synapse.Weight);
                    Efficacy.Update(synapse.EfficacyStat);
                }
                return;
            }
        }//SynapsesStat

        /// <summary>
        /// Synapse statistics by synapse role
        /// </summary>
        [Serializable]
        public class SynapsesByRoleStat
        {
            /// <summary>
            /// Total number of synapses
            /// </summary>
            public int Count { get; private set; }
            /// <summary>
            /// Statistics of synapses
            /// </summary>
            public SynapseStat[] SynapseRole { get; }

            /// <summary>
            /// Creates an initialized instance
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
            /// Updates statistics
            /// </summary>
            /// <param name="synapses">Collection of synapses</param>
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
        /// Stimulation statistics
        /// </summary>
        [Serializable]
        public class StimuliStat
        {
            //Attribute properties
            /// <summary>
            /// Statistics of stimuli from input synapses
            /// </summary>
            public StandardStatSet Input { get; }
            /// <summary>
            /// Statistics of stimuli from reservoir synapses
            /// </summary>
            public StandardStatSet Reservoir { get; }
            /// <summary>
            /// Statistics of total stimuli
            /// </summary>
            public StandardStatSet Total { get; }

            //Constructor
            /// <summary>
            /// Creates an initialized instance
            /// </summary>
            public StimuliStat()
            {
                Input = new StandardStatSet();
                Reservoir = new StandardStatSet();
                Total = new StandardStatSet();
                return;
            }

            /// <summary>
            /// Updates statistics
            /// </summary>
            /// <param name="neuron">Hidden neuron</param>
            public void Update(HiddenNeuron neuron)
            {
                Input.Update(neuron.Statistics.InputStimuliStat);
                Reservoir.Update(neuron.Statistics.ReservoirStimuliStat);
                Total.Update(neuron.Statistics.TotalStimuliStat);
                return;
            }

        }//StimuliStat

        /// <summary>
        /// Output signal statistics
        /// </summary>
        [Serializable]
        public class SignalStat
        {
            //Attribute properties
            /// <summary>
            /// Statistics of analog output signal
            /// </summary>
            public StandardStatSet Analog { get; }
            /// <summary>
            /// Statistics of firing signal
            /// </summary>
            public StandardStatSet Firing { get; }

            //Constructor
            /// <summary>
            /// Creates an initialized instance
            /// </summary>
            public SignalStat()
            {
                Analog = new StandardStatSet();
                Firing = new StandardStatSet();
                return;
            }

            /// <summary>
            /// Updates statistics
            /// </summary>
            /// <param name="neuron">Hidden neuron</param>
            public void Update(HiddenNeuron neuron)
            {
                Analog.Update(neuron.Statistics.AnalogSignalStat);
                Firing.Update(neuron.Statistics.FiringStat);
                return;
            }

        }//SignalStat

        /// <summary>
        /// Encapsulates neurons behavioral anomalies
        /// </summary>
        [Serializable]
        public class NeuronsAnomaliesStat
        {
            /// <summary>
            /// Number of neurons having no synapses from other reservoir neurons
            /// </summary>
            public int NoResSynapses { get; set; }
            /// <summary>
            /// Number of neurons getting no stimulation from connected reservoir's neurons
            /// </summary>
            public int NoResStimuli { get; set; }
            /// <summary>
            /// Number of neurons emitting no output signal
            /// </summary>
            public int NoAnalogOutput { get; set; }
            /// <summary>
            /// Number of neurons emitting constant output signal
            /// </summary>
            public int ConstAnalogOutput { get; set; }
            /// <summary>
            /// Number of neurons emitting no output spikes
            /// </summary>
            public int NotFiring { get; set; }
            /// <summary>
            /// Number of neurons emitting constant output spikes
            /// </summary>
            public int ConstFiring { get; set; }

            //Constructor
            /// <summary>
            /// Creates an initialized instance
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
            /// Updates anomalies
            /// </summary>
            /// <param name="neuron">Hidden neuron</param>
            /// <param name="numOfResSynapses">Number of reservoir synapses</param>
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
