using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Activation;
using RCNet.Neural.Network.SM.Preprocessing.Neuron;
using RCNet.Queue;
using System;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Class covers the behavior of synapse.
    /// Supports signal delay and short-term plasticity.
    /// </summary>
    [Serializable]
    public class Synapse
    {
        /// <summary>
        /// Method to decide synapse delay
        /// </summary>
        public enum SynapticDelayMethod
        {
            /// <summary>
            /// Synapse delay is decided randomly
            /// </summary>
            Random,
            /// <summary>
            /// Synapse delay depends on Euclidean distance
            /// </summary>
            Distance
        }

        /// <summary>
        /// Synapse role
        /// </summary>
        public enum SynRole
        {
            /// <summary>
            /// Input synapse
            /// </summary>
            Input,
            /// <summary>
            /// Excitatory synapse
            /// </summary>
            Excitatory,
            /// <summary>
            /// Inhibitory synapse
            /// </summary>
            Inhibitory,
            /// <summary>
            /// Indifferent synapse
            /// </summary>
            Indifferent
        }

        //Static attributes
        /// <summary>
        /// Number of defined synapse roles
        /// </summary>
        public static readonly int NumOfRoles = Enum.GetValues(typeof(SynRole)).Length;

        //Attribute properties
        /// <summary>
        /// Source neuron
        /// </summary>
        public INeuron SourceNeuron { get; }

        /// <summary>
        /// Target neuron
        /// </summary>
        public INeuron TargetNeuron { get; }

        /// <summary>
        /// Euclidean distance between SourceNeuron and TargetNeuron
        /// </summary>
        public double Distance { get; }

        /// <summary>
        /// Synapse role
        /// </summary>
        public SynRole Role { get; }

        /// <summary>
        /// Weight of the synapse (the maximum achievable weight)
        /// </summary>
        public double Weight { get; private set; }

        /// <summary>
        /// Signal traveling delay (in computation cycles)
        /// </summary>
        public int Delay { get; private set; }

        /// <summary>
        /// Method to decide signal delaying
        /// </summary>
        public SynapticDelayMethod DelayMethod { get; private set; }

        /// <summary>
        /// Synapse's efficacy statistics
        /// </summary>
        public BasicStat EfficacyStat { get; }


        //Attributes
        private readonly NeuronOutputData _sourceNeuronOutputData;
        private readonly bool _analogSourceSignal;
        private readonly IEfficacy _efficacyComputer;
        private readonly int _maxDelay;
        private SimpleQueue<Signal> _signalQueue;

        //Constructor
        /// <summary>
        /// Creates initialized instance
        /// </summary>
        /// <param name="sourceNeuron">Source neuron</param>
        /// <param name="targetNeuron">Target neuron</param>
        /// <param name="role">Synapse role</param>
        /// <param name="synapseCfg">Synapse general configuration</param>
        /// <param name="rand">Random object</param>
        public Synapse(INeuron sourceNeuron,
                       INeuron targetNeuron,
                       SynRole role,
                       SynapseSettings synapseCfg,
                       Random rand
                       )
        {
            //Neurons to be connected
            SourceNeuron = sourceNeuron;
            TargetNeuron = targetNeuron;
            //Synapse role
            Role = role;
            //Euclidean distance
            Distance = EuclideanDistance.Compute(SourceNeuron.Location.ReservoirCoordinates, TargetNeuron.Location.ReservoirCoordinates);
            //The rest
            _efficacyComputer = null;
            if (TargetNeuron.TypeOfActivation == ActivationType.Spiking)
            {
                //Spiking target
                if (Role == SynRole.Input)
                {
                    DelayMethod = synapseCfg.SpikingTargetCfg.InputSynCfg.DelayMethod;
                    _maxDelay = synapseCfg.SpikingTargetCfg.InputSynCfg.MaxDelay;
                    if (SourceNeuron.TypeOfActivation == ActivationType.Analog)
                    {
                        //Analog source
                        Weight = rand.NextDouble(synapseCfg.SpikingTargetCfg.InputSynCfg.AnalogSourceCfg.WeightCfg);
                    }
                    else
                    {
                        //Spiking source
                        Weight = rand.NextDouble(synapseCfg.SpikingTargetCfg.InputSynCfg.SpikingSourceCfg.WeightCfg);
                        _efficacyComputer = PlasticityCommon.GetEfficacyComputer(SourceNeuron,
                                                                                 synapseCfg.SpikingTargetCfg.InputSynCfg.SpikingSourceCfg.PlasticityCfg.DynamicsCfg
                                                                                 );
                    }
                }
                else if (Role == SynRole.Excitatory)
                {
                    DelayMethod = synapseCfg.SpikingTargetCfg.ExcitatorySynCfg.DelayMethod;
                    _maxDelay = synapseCfg.SpikingTargetCfg.ExcitatorySynCfg.MaxDelay;
                    if (SourceNeuron.TypeOfActivation == ActivationType.Analog)
                    {
                        //Analog source
                        Weight = rand.NextDouble(synapseCfg.SpikingTargetCfg.ExcitatorySynCfg.AnalogSourceCfg.WeightCfg);
                    }
                    else
                    {
                        //Spiking source
                        Weight = rand.NextDouble(synapseCfg.SpikingTargetCfg.ExcitatorySynCfg.SpikingSourceCfg.WeightCfg);
                    }
                    _efficacyComputer = PlasticityCommon.GetEfficacyComputer(SourceNeuron,
                                                                             synapseCfg.SpikingTargetCfg.ExcitatorySynCfg.SpikingSourceCfg.PlasticityCfg.DynamicsCfg
                                                                             );
                }
                else if (Role == SynRole.Inhibitory)
                {
                    DelayMethod = synapseCfg.SpikingTargetCfg.InhibitorySynCfg.DelayMethod;
                    _maxDelay = synapseCfg.SpikingTargetCfg.InhibitorySynCfg.MaxDelay;
                    if (SourceNeuron.TypeOfActivation == ActivationType.Analog)
                    {
                        //Analog source
                        Weight = -rand.NextDouble(synapseCfg.SpikingTargetCfg.InhibitorySynCfg.AnalogSourceCfg.WeightCfg);
                    }
                    else
                    {
                        //Spiking source
                        Weight = -rand.NextDouble(synapseCfg.SpikingTargetCfg.InhibitorySynCfg.SpikingSourceCfg.WeightCfg);
                    }
                    _efficacyComputer = PlasticityCommon.GetEfficacyComputer(SourceNeuron,
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
                    if (SourceNeuron.TypeOfActivation == ActivationType.Analog)
                    {
                        //Analog source
                        Weight = rand.NextDouble(synapseCfg.AnalogTargetCfg.InputSynCfg.AnalogSourceCfg.WeightCfg);
                    }
                    else
                    {
                        //Spiking source
                        Weight = rand.NextSign() * rand.NextDouble(synapseCfg.AnalogTargetCfg.InputSynCfg.SpikingSourceCfg.WeightCfg);
                        _efficacyComputer = PlasticityCommon.GetEfficacyComputer(SourceNeuron,
                                                                                 synapseCfg.AnalogTargetCfg.InputSynCfg.SpikingSourceCfg.PlasticityCfg.DynamicsCfg
                                                                                 );
                    }
                }
                else if (Role == SynRole.Indifferent)
                {
                    DelayMethod = synapseCfg.AnalogTargetCfg.IndifferentSynCfg.DelayMethod;
                    _maxDelay = synapseCfg.AnalogTargetCfg.IndifferentSynCfg.MaxDelay;
                    if (SourceNeuron.TypeOfActivation == ActivationType.Analog)
                    {
                        //Analog source
                        Weight = rand.NextSign() * rand.NextDouble(synapseCfg.AnalogTargetCfg.IndifferentSynCfg.AnalogSourceCfg.WeightCfg);
                    }
                    else
                    {
                        //Spiking source
                        Weight = rand.NextSign() * rand.NextDouble(synapseCfg.AnalogTargetCfg.IndifferentSynCfg.SpikingSourceCfg.WeightCfg);
                        _efficacyComputer = PlasticityCommon.GetEfficacyComputer(SourceNeuron,
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
            _sourceNeuronOutputData = SourceNeuron.OutputData;
            _analogSourceSignal = TargetNeuron.TypeOfActivation == ActivationType.Analog;
            //Efficacy statistics
            EfficacyStat = new BasicStat(false);
            Reset(true);
            return;
        }

        //Methods
        /// <summary>
        /// Rescales the synapse weight.
        /// </summary>
        /// <param name="factor">Scale factor</param>
        public void Rescale(double factor)
        {
            Weight *= factor;
            return;
        }

        /// <summary>
        /// Resets the synapse
        /// </summary>
        /// <param name="statistics">Specifies whether to reset also internal statistics</param>
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
                    EfficacyStat.AddSampleValue(1d);
                }
            }
            return;
        }

        /// <summary>
        /// Setups signal delaying
        /// </summary>
        /// <param name="distancesStat">Distance statistics to be used when synaptic delay method is Distance</param>
        /// <param name="rand">Random object to be used when synaptic delay method is Random</param>
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
        /// Returns signal to be delivered to target neuron.
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        public double GetSignal(bool collectStatistics)
        {
            //Source neuron signal
            double sourceNeuronSignal = _analogSourceSignal? _sourceNeuronOutputData._analogSignal: _sourceNeuronOutputData._spikingSignal;
            //Short-term plasticity
            double efficacy = 1d;
            if (_efficacyComputer != null && sourceNeuronSignal > 0)
            {
                //Compute synapse efficacy
                efficacy = _efficacyComputer.Compute();
                //Update statistics if necessary
                if (collectStatistics)
                {
                    EfficacyStat.AddSampleValue(efficacy);
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
        /// <summary>
        /// Signal data to be queued
        /// </summary>
        [Serializable]
        protected class Signal
        {
            /// <summary>
            /// Weighted signal
            /// </summary>
            public double _weightedSignal;

        }//Signal

    }//Synapse

}//Namespace
