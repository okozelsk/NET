using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Activation;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron;
using RCNet.Queue;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Class covers the behaviour of synapse.
    /// Supports signal delay and short-term plasticity.
    /// </summary>
    [Serializable]
    public class Synapse
    {
        //Enums
        /// <summary>
        /// Target scope of the synapse
        /// </summary>
        public enum SynapticTargetScope
        {
            /// <summary>
            /// Both Excitatory and Inhibitory neurons
            /// </summary>
            All,
            /// <summary>
            /// Excitatory neurons only
            /// </summary>
            Excitatory,
            /// <summary>
            /// Inhibitory neurons only
            /// </summary>
            Inhibitory
        }

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

        //Constants
        private const double PlasticityParamGaussianCoeff = 0.25d;
        private const double PlasticityParamMinGaussianCoeff = (1d - PlasticityParamGaussianCoeff);
        private const double PlasticityParamMaxGaussianCoeff = (1d + PlasticityParamGaussianCoeff);

        //Attribute properties
        /// <summary>
        /// Source neuron - signal emitter
        /// </summary>
        public INeuron SourceNeuron { get; }

        /// <summary>
        /// Target neuron - signal receiver
        /// </summary>
        public INeuron TargetNeuron { get; }

        /// <summary>
        /// Weight of the synapse (the maximum achievable weight)
        /// </summary>
        public double Weight { get; private set; }

        /// <summary>
        /// Euclidean distance between SourceNeuron and TargetNeuron
        /// </summary>
        public double Distance { get; }

        /// <summary>
        /// Signal delaying (in computation cycles)
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
        private readonly int _maxDelay;
        private SimpleQueue<Signal> _signalQueue;
        private readonly double _tauFacilitation;
        private readonly double _tauDepression;
        private readonly double _restingEfficacy;
        private double _facilitation;
        private double _depression;
        private readonly bool _applyPlasticity;



        //Constructor
        /// <summary>
        /// Creates initialized instance
        /// </summary>
        /// <param name="sourceNeuron">Source neuron</param>
        /// <param name="targetNeuron">Target neuron</param>
        /// <param name="weight">Synapse initial weight</param>
        /// <param name="delayMethod">Synaptic delay method to be used</param>
        /// <param name="maxDelay">Maximum synaptic delay</param>
        /// <param name="dynamicsCfg">Synapse's short-term plasticity dynamics configuration</param>
        /// <param name="rand">Random object to be used to setup plasticity parameters</param>
        public Synapse(INeuron sourceNeuron,
                       INeuron targetNeuron,
                       double weight,
                       SynapticDelayMethod delayMethod,
                       int maxDelay,
                       DynamicsSettings dynamicsCfg,
                       Random rand
                       )
        {
            //Neurons to be connected
            SourceNeuron = sourceNeuron;
            TargetNeuron = targetNeuron;
            //Euclidean distance
            Distance = EuclideanDistance.Compute(SourceNeuron.Location.ReservoirCoordinates, TargetNeuron.Location.ReservoirCoordinates);
            //Signal delaying should be set later by SetupDelay method
            _maxDelay = maxDelay;
            _signalQueue = null;
            Delay = 0;
            DelayMethod = delayMethod;
            //Plasticity
            if(dynamicsCfg != null &&
               dynamicsCfg.Apply &&
               TargetNeuron.TypeOfActivation == ActivationType.Spiking &&
               SourceNeuron.Role != NeuronCommon.NeuronRole.Input &&
               (SourceNeuron.SignalingRestriction == NeuronCommon.NeuronSignalingRestrictionType.SpikingOnly ||
                SourceNeuron.SignalingRestriction == NeuronCommon.NeuronSignalingRestrictionType.NoRestriction)
               )
            {
                _applyPlasticity = true;
                _restingEfficacy = rand.NextFilterredGaussianDouble(dynamicsCfg.RestingEfficacy, Math.Sqrt(dynamicsCfg.RestingEfficacy / 2d), dynamicsCfg.RestingEfficacy * PlasticityParamMinGaussianCoeff, dynamicsCfg.RestingEfficacy * PlasticityParamMaxGaussianCoeff);
                _tauFacilitation = rand.NextFilterredGaussianDouble(dynamicsCfg.TauFacilitation, Math.Sqrt(dynamicsCfg.TauFacilitation / 2d), dynamicsCfg.TauFacilitation * PlasticityParamMinGaussianCoeff, dynamicsCfg.TauFacilitation * PlasticityParamMaxGaussianCoeff);
                _tauDepression = rand.NextFilterredGaussianDouble(dynamicsCfg.TauDepression, Math.Sqrt(dynamicsCfg.TauDepression / 2d), dynamicsCfg.TauDepression * PlasticityParamMinGaussianCoeff, dynamicsCfg.TauDepression * PlasticityParamMaxGaussianCoeff);
            }
            else
            {
                _applyPlasticity = false;
                _restingEfficacy = 0d;
                _tauFacilitation = 0d;
                _tauDepression = 0d;
            }

            //Weight sign rules
            if (SourceNeuron.Role == NeuronCommon.NeuronRole.Input)
            {
                if (TargetNeuron.TypeOfActivation == ActivationType.Analog)
                {
                    //No change of the weight sign
                    Weight = weight;
                }
                else
                {
                    //Target is spiking neuron
                    //Weight must be always positive
                    Weight = Math.Abs(weight);
                }
            }
            else
            {
                //Weight sign depends on source neuron role
                Weight = Math.Abs(weight) * (SourceNeuron.Role == NeuronCommon.NeuronRole.Excitatory ? 1d : -1d);
            }
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
            _facilitation = _applyPlasticity ? _restingEfficacy : 1d;
            _depression = 1d;
            if (statistics)
            {
                EfficacyStat.Reset();
                if (!_applyPlasticity)
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
        /// Computes synapse efficacy (short-term plasticity model).
        /// </summary>
        private double ComputeEfficacy()
        {
            if (SourceNeuron.AfterFirstSpike)
            {
                double sourceSpikeLeak = SourceNeuron.SpikeLeak;
                //Facilitation model
                double tmp = _facilitation * Math.Exp(-(sourceSpikeLeak / _tauFacilitation));
                _facilitation = tmp + _restingEfficacy * (1d - tmp);
                //Depression model
                tmp = Math.Exp(-(sourceSpikeLeak / _tauDepression));
                _depression = _depression * (1d - _facilitation) * tmp + 1d - tmp;
            }
            return _facilitation * _depression;
        }

        /// <summary>
        /// Returns signal to be delivered to target neuron.
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        public double GetSignal(bool collectStatistics)
        {
            //Weighted source neuron signal
            double weightedSignal = SourceNeuron.GetSignal(TargetNeuron.TypeOfActivation) * Weight;
            //Short-term plasticity
            if (_applyPlasticity)
            {
                //Compute synapse efficacy
                double efficacy = ComputeEfficacy();
                //Update statistics if necessary
                if (collectStatistics)
                {
                    EfficacyStat.AddSampleValue(efficacy);
                }
                //Resulting weighted signal
                weightedSignal *= efficacy;
            }
            //Delayed signal
            if (_signalQueue == null)
            {
                return weightedSignal;
            }
            else
            {
                //Signal to be delayed so use queue
                //Enqueue
                Signal sigObj = _signalQueue.GetElementAtEnqueuePosition();
                if (sigObj != null)
                {
                    sigObj._weightedSignal = weightedSignal;
                }
                else
                {
                    sigObj = new Signal { _weightedSignal = weightedSignal };
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
