using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;
using RCNet.Neural.Network.SM.Neuron;
using RCNet.Queue;

namespace RCNet.Neural.Network.SM.Synapse
{
    /// <summary>
    /// Inplements the pre-synaptic Short-Term-Plasticity and post-synaptic dynamic decay of the efficacy of the synapse.
    /// </summary>
    [Serializable]
    public class DynamicSynapse : BaseSynapse, ISynapse
    {
        //Attributes
        private readonly double _tauFacilitation;
        private readonly double _tauRecovery;
        private readonly double _restingEfficacy;
        private readonly bool _applyPreSynaptic;
        private readonly double _tauDecay;
        private readonly bool _applyPostSynaptic;
        private double _efficacyUtilization;
        private double _efficacyAvailableFraction;
        private SimpleQueue<Signal> _signalQueue;

        //Constructor
        /// <summary>
        /// Creates initialized instance
        /// </summary>
        /// <param name="sourceNeuron">Source neuron</param>
        /// <param name="targetNeuron">Target neuron</param>
        /// <param name="weight">Synapse weight (unsigned)</param>
        /// <param name="tauFacilitation">Synapse efficacy facilitation parameter (pre-synaptic)</param>
        /// <param name="tauRecovery">Synapse efficacy recovery parameter (pre-synaptic)</param>
        /// <param name="restingEfficacy">Synapse resting efficacy parameter (pre-synaptic)</param>
        /// <param name="tauDecay">Decay shapness (post-synaptic)</param>
        public DynamicSynapse(INeuron sourceNeuron,
                              INeuron targetNeuron,
                              double weight,
                              double tauFacilitation,
                              double tauRecovery,
                              double restingEfficacy,
                              double tauDecay
                              )
            :base(sourceNeuron, targetNeuron, weight)
        {
            _tauFacilitation = tauFacilitation;
            _tauRecovery = tauRecovery;
            _restingEfficacy = restingEfficacy;
            _applyPreSynaptic = (SourceNeuron.OutputType == CommonEnums.NeuronSignalType.Spike);
            _efficacyUtilization = _restingEfficacy;
            _efficacyAvailableFraction = 1;
            _tauDecay = tauDecay;
            _applyPostSynaptic = (TargetNeuron.OutputType == CommonEnums.NeuronSignalType.Spike);
            //Signal queue
            _signalQueue = null;
            return;
        }

        //Methods
        /// <summary>
        /// Resets synapse.
        /// </summary>
        /// <param name="statistics">Specifies whether to reset also internal statistics</param>
        public void Reset(bool statistics)
        {
            //Reset queue if it is instantiated
            _signalQueue?.Reset();
            _efficacyUtilization = _restingEfficacy;
            _efficacyAvailableFraction = 1;
            if (statistics)
            {
                EfficacyStat.Reset();
            }
            return;
        }

        /// <summary>
        /// Sets the synapse signal delay
        /// </summary>
        /// <param name="delay">Signal delay (reservoir cycles)</param>
        public void SetDelay(int delay)
        {
            //Set synapse signal delay
            Delay = delay;
            if(Delay == 0)
            {
                //No queue will be used
                _signalQueue = null;
            }
            else
            {
                //Delay queue
                _signalQueue = new SimpleQueue<Signal>(Delay + 1);
            }
            return;
        }

        /// <summary>
        /// Computes synapse efficacy based on the pre-synaptic activity.
        /// Implementation of the pre-synaptic Short-Term-Plasticity
        /// </summary>
        protected double GetPreSynapticEfficacy()
        {
            if (_applyPreSynaptic)
            {
                double x = Math.Exp(-(SourceNeuron.OutputSignalLeak / _tauFacilitation));
                _efficacyUtilization = x + _restingEfficacy * (1d - x);
                double y = Math.Exp(-(SourceNeuron.OutputSignalLeak / _tauRecovery));
                _efficacyAvailableFraction = _efficacyAvailableFraction * (1d - _efficacyUtilization) * y + 1d - y;
                return _efficacyUtilization * _efficacyAvailableFraction;
            }
            else
            {
                return 1d;
            }
        }

        /// <summary>
        /// Computes synapse efficacy based on the post-synaptic activity
        /// </summary>
        protected double GetPostSynapticEfficacy()
        {
            if (_applyPostSynaptic)
            {
                return Math.Exp(-(TargetNeuron.OutputSignalLeak / _tauDecay));
            }
            else
            {
                return 1d;
            }
        }

        /// <summary>
        /// Returns signal to be delivered to target neuron.
        /// Note that this function has to be invoked only once per reservoir cycle !!!
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        public double GetSignal(bool collectStatistics)
        {
            //Source neuron signal
            double sourceSignal = SourceNeuron.OutputSignal;
            if (_signalQueue == null)
            {
                //No delay of the signal - do not use queue
                if (sourceSignal == 0)
                {
                    //No source signal so simply return 0
                    return 0;
                }
                else
                {
                    //Compute synapse efficacy
                    double efficacy = GetPreSynapticEfficacy() * GetPostSynapticEfficacy();
                    //Update statistics if necessary
                    if (collectStatistics)
                    {
                        EfficacyStat.AddSampleValue(efficacy);
                    }
                    //Return resulting signal
                    return sourceSignal * Weight * efficacy;
                }
            }
            else
            {
                //Signal to be delayed so use queue
                if (sourceSignal == 0)
                {
                    //No signal adjustments
                    Signal sigObj = _signalQueue.GetElementOnEnqueuePosition();
                    if (sigObj != null)
                    {
                        sigObj._weightedSignal = 0d;
                        sigObj._preSynapticEfficacy = 1d;
                    }
                    else
                    {
                        sigObj = new Signal { _weightedSignal = 0d, _preSynapticEfficacy = 1d };
                    }
                    _signalQueue.Enqueue(sigObj);
                }
                else
                {
                    //Signal to be delayed
                    Signal sigObj = _signalQueue.GetElementOnEnqueuePosition();
                    //Compute pre-synaptic efficacy
                    double preSynapticEfficacy = GetPreSynapticEfficacy();
                    //Compute constantly weighted signal and pre-synaptic part of efficacy and put them into the queue simulating the signal traveling
                    if (sigObj != null)
                    {
                        sigObj._weightedSignal = ((SourceNeuron.OutputSignal + _add) / _div) * Weight;
                        sigObj._preSynapticEfficacy = preSynapticEfficacy;
                    }
                    else
                    {
                        sigObj = new Signal { _weightedSignal = ((SourceNeuron.OutputSignal + _add) / _div) * Weight, _preSynapticEfficacy = preSynapticEfficacy };
                    }
                    _signalQueue.Enqueue(sigObj);
                }
                //Is there delayed signal to be delivered?
                if (_signalQueue.Full)
                {
                    //Queue is full, so synapse is ready to deliver delayed signal
                    //Pick up source signal and pre-synaptic part of efficacy
                    Signal sigObj = _signalQueue.Dequeue();
                    if (sigObj._weightedSignal == 0)
                    {
                        //No need of signal adjustment
                        return 0;
                    }
                    else
                    {
                        //Compute resulting efficacy
                        double efficacy = sigObj._preSynapticEfficacy * GetPostSynapticEfficacy();
                        //Update statistics if required
                        if (collectStatistics)
                        {
                            EfficacyStat.AddSampleValue(efficacy);
                        }
                        //Deliver the resulting signal
                        return sigObj._weightedSignal * efficacy;
                    }
                }
                else
                {
                    //No signal to be delivered, signal is still "on the road"
                    return 0;
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
            /// Weighted signal with no adjustments
            /// </summary>
            public double _weightedSignal;

            /// <summary>
            /// Computed synapse efficacy based on pre-synaptic activity
            /// </summary>
            public double _preSynapticEfficacy;

        }//Signal

    }//DynamicSynapse

}//Namespace
