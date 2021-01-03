using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Activation;
using RCNet.Neural.Network.SM.Preprocessing.Neuron;
using RCNet.Queue;
using System;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Implements the synapse.
    /// </summary>
    /// <remarks>
    /// Supports the signal delaying and the short-term plasticity.
    /// </remarks>
    [Serializable]
    public class Synapse
    {
        /// <summary>
        /// The synaptic delay method.
        /// </summary>
        public enum SynapticDelayMethod
        {
            /// <summary>
            /// The synaptic delay is decided randomly.
            /// </summary>
            Random,
            /// <summary>
            /// The synaptic delay depends on an Euclidean distance.
            /// </summary>
            Distance
        }

        /// <summary>
        /// The synapse's role.
        /// </summary>
        public enum SynRole
        {
            /// <summary>
            /// An input synapse.
            /// </summary>
            Input,
            /// <summary>
            /// An excitatory synapse.
            /// </summary>
            Excitatory,
            /// <summary>
            /// An inhibitory synapse.
            /// </summary>
            Inhibitory,
            /// <summary>
            /// An indifferent synapse.
            /// </summary>
            Indifferent
        }

        //Static attributes
        /// <summary>
        /// The number of the synapse roles.
        /// </summary>
        public static readonly int NumOfRoles = Enum.GetValues(typeof(SynRole)).Length;

        //Attribute properties
        /// <summary>
        /// The presynaptic neuron.
        /// </summary>
        public INeuron PresynapticNeuron { get; }

        /// <summary>
        /// The postsynaptic neuron.
        /// </summary>
        public INeuron PostsynapticNeuron { get; }

        /// <summary>
        /// An Euclidean distance of presynaptic and postsynaptic neurons.
        /// </summary>
        public double Distance { get; }

        /// <inheritdoc cref="SynRole"/>
        public SynRole Role { get; }

        /// <summary>
        /// The weight of the synapse (the maximum achievable weight).
        /// </summary>
        public double Weight { get; private set; }

        /// <summary>
        /// The signal delay (in computation cycles).
        /// </summary>
        public int Delay { get; private set; }

        /// <inheritdoc cref="SynapticDelayMethod"/>
        public SynapticDelayMethod DelayMethod { get; private set; }

        /// <summary>
        /// The synapse's efficacy statistics.
        /// </summary>
        public BasicStat EfficacyStat { get; }


        //Attributes
        private readonly NeuronOutputData _presynapticNeuronOutputData;
        private readonly bool _analogPresynapticSignal;
        private readonly IEfficacy _efficacyComputer;
        private readonly int _maxDelay;
        private SimpleQueue<Signal> _signalQueue;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="presynapticNeuron">The presynaptic neuron.</param>
        /// <param name="postsynapticNeuron">The postsynaptic neuron.</param>
        /// <param name="role">The synapse's role.</param>
        /// <param name="synapseCfg">The configuration of the synapse.</param>
        /// <param name="rand">The random object to be used.</param>
        public Synapse(INeuron presynapticNeuron,
                       INeuron postsynapticNeuron,
                       SynRole role,
                       SynapseSettings synapseCfg,
                       Random rand
                       )
        {
            //Neurons to be connected
            PresynapticNeuron = presynapticNeuron;
            PostsynapticNeuron = postsynapticNeuron;
            //Synapse role
            Role = role;
            //Euclidean distance
            Distance = EuclideanDistance.Compute(PresynapticNeuron.Location.ReservoirCoordinates, PostsynapticNeuron.Location.ReservoirCoordinates);
            //The rest
            _efficacyComputer = null;
            if (PostsynapticNeuron.TypeOfActivation == ActivationType.Spiking)
            {
                //Spiking target
                if (Role == SynRole.Input)
                {
                    DelayMethod = synapseCfg.SpikingTargetCfg.InputSynCfg.DelayMethod;
                    _maxDelay = synapseCfg.SpikingTargetCfg.InputSynCfg.MaxDelay;
                    if (PresynapticNeuron.TypeOfActivation == ActivationType.Analog)
                    {
                        //Analog source
                        Weight = rand.NextDouble(synapseCfg.SpikingTargetCfg.InputSynCfg.AnalogSourceCfg.WeightCfg);
                    }
                    else
                    {
                        //Spiking source
                        Weight = rand.NextDouble(synapseCfg.SpikingTargetCfg.InputSynCfg.SpikingSourceCfg.WeightCfg);
                        _efficacyComputer = PlasticityCommon.GetEfficacyComputer(PresynapticNeuron,
                                                                                 synapseCfg.SpikingTargetCfg.InputSynCfg.SpikingSourceCfg.PlasticityCfg.DynamicsCfg
                                                                                 );
                    }
                }
                else if (Role == SynRole.Excitatory)
                {
                    DelayMethod = synapseCfg.SpikingTargetCfg.ExcitatorySynCfg.DelayMethod;
                    _maxDelay = synapseCfg.SpikingTargetCfg.ExcitatorySynCfg.MaxDelay;
                    if (PresynapticNeuron.TypeOfActivation == ActivationType.Analog)
                    {
                        //Analog source
                        Weight = rand.NextDouble(synapseCfg.SpikingTargetCfg.ExcitatorySynCfg.AnalogSourceCfg.WeightCfg);
                    }
                    else
                    {
                        //Spiking source
                        Weight = rand.NextDouble(synapseCfg.SpikingTargetCfg.ExcitatorySynCfg.SpikingSourceCfg.WeightCfg);
                    }
                    _efficacyComputer = PlasticityCommon.GetEfficacyComputer(PresynapticNeuron,
                                                                             synapseCfg.SpikingTargetCfg.ExcitatorySynCfg.SpikingSourceCfg.PlasticityCfg.DynamicsCfg
                                                                             );
                }
                else if (Role == SynRole.Inhibitory)
                {
                    DelayMethod = synapseCfg.SpikingTargetCfg.InhibitorySynCfg.DelayMethod;
                    _maxDelay = synapseCfg.SpikingTargetCfg.InhibitorySynCfg.MaxDelay;
                    if (PresynapticNeuron.TypeOfActivation == ActivationType.Analog)
                    {
                        //Analog source
                        Weight = -rand.NextDouble(synapseCfg.SpikingTargetCfg.InhibitorySynCfg.AnalogSourceCfg.WeightCfg);
                    }
                    else
                    {
                        //Spiking source
                        Weight = -rand.NextDouble(synapseCfg.SpikingTargetCfg.InhibitorySynCfg.SpikingSourceCfg.WeightCfg);
                    }
                    _efficacyComputer = PlasticityCommon.GetEfficacyComputer(PresynapticNeuron,
                                                                             synapseCfg.SpikingTargetCfg.InhibitorySynCfg.SpikingSourceCfg.PlasticityCfg.DynamicsCfg
                                                                             );
                }
                else
                {
                    throw new ArgumentException($"Invalid synapse role {role}.", "role");
                }
            }
            else
            {
                //Analog target
                if (Role == SynRole.Input)
                {
                    DelayMethod = synapseCfg.AnalogTargetCfg.InputSynCfg.DelayMethod;
                    _maxDelay = synapseCfg.AnalogTargetCfg.InputSynCfg.MaxDelay;
                    if (PresynapticNeuron.TypeOfActivation == ActivationType.Analog)
                    {
                        //Analog source
                        Weight = rand.NextDouble(synapseCfg.AnalogTargetCfg.InputSynCfg.AnalogSourceCfg.WeightCfg);
                    }
                    else
                    {
                        //Spiking source
                        Weight = rand.NextSign() * rand.NextDouble(synapseCfg.AnalogTargetCfg.InputSynCfg.SpikingSourceCfg.WeightCfg);
                        _efficacyComputer = PlasticityCommon.GetEfficacyComputer(PresynapticNeuron,
                                                                                 synapseCfg.AnalogTargetCfg.InputSynCfg.SpikingSourceCfg.PlasticityCfg.DynamicsCfg
                                                                                 );
                    }
                }
                else if (Role == SynRole.Indifferent)
                {
                    DelayMethod = synapseCfg.AnalogTargetCfg.IndifferentSynCfg.DelayMethod;
                    _maxDelay = synapseCfg.AnalogTargetCfg.IndifferentSynCfg.MaxDelay;
                    if (PresynapticNeuron.TypeOfActivation == ActivationType.Analog)
                    {
                        //Analog source
                        Weight = rand.NextSign() * rand.NextDouble(synapseCfg.AnalogTargetCfg.IndifferentSynCfg.AnalogSourceCfg.WeightCfg);
                    }
                    else
                    {
                        //Spiking source
                        Weight = rand.NextSign() * rand.NextDouble(synapseCfg.AnalogTargetCfg.IndifferentSynCfg.SpikingSourceCfg.WeightCfg);
                        _efficacyComputer = PlasticityCommon.GetEfficacyComputer(PresynapticNeuron,
                                                                                 synapseCfg.AnalogTargetCfg.IndifferentSynCfg.SpikingSourceCfg.PlasticityCfg.DynamicsCfg
                                                                                 );
                    }

                }
                else
                {
                    throw new ArgumentException($"Invalid synapse role {role}.", "role");
                }
            }
            //Source neuron - output data and signal index
            _presynapticNeuronOutputData = PresynapticNeuron.OutputData;
            _analogPresynapticSignal = PostsynapticNeuron.TypeOfActivation == ActivationType.Analog;
            //Efficacy statistics
            EfficacyStat = new BasicStat(false);
            Reset(true);
            return;
        }

        //Methods
        /// <summary>
        /// Rescales the synapse weight.
        /// </summary>
        /// <param name="factor">The scale factor.</param>
        public void Rescale(double factor)
        {
            Weight *= factor;
            return;
        }

        /// <summary>
        /// Resets the synapse.
        /// </summary>
        /// <param name="statistics">Specifies whether to reset also the efficacy statistics.</param>
        public void Reset(bool statistics)
        {
            //Reset queue if it is instantiated
            _signalQueue?.Reset();
            _efficacyComputer?.Reset();
            if (statistics)
            {
                EfficacyStat.Reset();
                if (_efficacyComputer == null)
                {
                    //Efficacy will be always 1
                    EfficacyStat.AddSample(1d);
                }
            }
            return;
        }

        /// <summary>
        /// Setups the signal delaying.
        /// </summary>
        /// <param name="distancesStat">The distance statistics to be used when the synaptic delaying depends on a distance.</param>
        /// <param name="rand">The random object to be used when synaptic delaying is random.</param>
        public void SetupDelay(BasicStat distancesStat, Random rand)
        {
            if (_maxDelay > 0)
            {
                //Set synapse signal delay
                if (DelayMethod == SynapticDelayMethod.Distance)
                {
                    double relDistance = (Distance - distancesStat.Min) / distancesStat.Span;
                    Delay = (int)Math.Round(_maxDelay * relDistance);
                }
                else
                {
                    Delay = rand.Next(_maxDelay + 1);
                }
                if (Delay == 0)
                {
                    //No queue will be used
                    _signalQueue = null;
                }
                else
                {
                    //Delay queue
                    _signalQueue = new SimpleQueue<Signal>(Delay + 1);
                }
            }
            return;
        }

        /// <summary>
        /// Gets the signal to be delivered to postsynaptic neuron.
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update efficacy statistics.</param>
        public double GetSignal(bool collectStatistics)
        {
            //Source neuron signal
            double sourceNeuronSignal = _analogPresynapticSignal ? _presynapticNeuronOutputData._analogSignal : _presynapticNeuronOutputData._spikingSignal;
            //Short-term plasticity
            double efficacy = 1d;
            if (_efficacyComputer != null && sourceNeuronSignal > 0)
            {
                //Compute synapse efficacy
                efficacy = _efficacyComputer.Compute();
                //Update statistics if necessary
                if (collectStatistics)
                {
                    EfficacyStat.AddSample(efficacy);
                }
            }
            //Final signal to be delivered
            double signalToTarget = sourceNeuronSignal * Weight * efficacy;
            //Delayed signal?
            if (_signalQueue == null)
            {
                //No delay
                return signalToTarget;
            }
            else
            {
                //Signal to be delayed so use queue
                //Enqueue
                Signal sigObj = _signalQueue.GetElementAtEnqueuePosition();
                if (sigObj != null)
                {
                    sigObj._weightedSignal = signalToTarget;
                }
                else
                {
                    sigObj = new Signal { _weightedSignal = signalToTarget };
                }
                _signalQueue.Enqueue(sigObj);
                //Is there delayed signal to be delivered?
                if (_signalQueue.Full)
                {
                    //Queue is full, so synapse is ready to deliver delayed signal
                    sigObj = _signalQueue.Dequeue();
                    return sigObj._weightedSignal;
                }
                else
                {
                    //No signal to be delivered, signal is still "on the road"
                    return 0d;
                }
            }
        }


        //Inner classes
        [Serializable]
        internal class Signal
        {
            /// <summary>
            /// The weighted signal.
            /// </summary>
            public double _weightedSignal;

        }//Signal

    }//Synapse

}//Namespace
