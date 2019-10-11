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
    /// Input synapse.
    /// Supports signal delay.
    /// </summary>
    [Serializable]
    public class InputSynapse : BaseSynapse, ISynapse
    {
        //Attribute properties
        /// <summary>
        /// Signal delaying (in computation cycles)
        /// </summary>
        public int Delay { get; protected set; }

        /// <summary>
        /// Method to decide signal delaying
        /// </summary>
        public SynapticDelayMethod DelayMethod { get; protected set; }

        //Attributes
        private readonly int _maxDelay;
        private SimpleQueue<Signal> _signalQueue;

        //Constructor
        /// <summary>
        /// Creates initialized instance
        /// </summary>
        /// <param name="sourceNeuron">Source neuron</param>
        /// <param name="targetNeuron">Target neuron</param>
        /// <param name="weight">Synapse weight (unsigned)</param>
        /// <param name="delayMethod">Synaptic delay method to be used</param>
        /// <param name="maxDelay">Maximum synaptic delay</param>
        public InputSynapse(INeuron sourceNeuron,
                            INeuron targetNeuron,
                            double weight,
                            SynapticDelayMethod delayMethod,
                            int maxDelay
                            )
            :base(sourceNeuron, targetNeuron, weight)
        {
            //Signal delaying can be set later by SetDelay method
            _maxDelay = maxDelay;
            Delay = 0;
            DelayMethod = delayMethod;
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
            if (statistics)
            {
                EfficacyStat.Reset();
                EfficacyStat.AddSampleValue(1d);
            }
            return;
        }

        /// <summary>
        /// Setups signal delaying
        /// </summary>
        /// <param name="distanceStat">Statistics of the distances</param>
        /// <param name="rand">Random object to be used when RandomMethod</param>
        public void SetDelay(BasicStat distanceStat, Random rand)
        {
            if (_maxDelay > 0)
            {
                //Set synapse signal delay
                if (DelayMethod == SynapticDelayMethod.Distance)
                {
                    double relDistance = (Distance - distanceStat.Min) / distanceStat.Span;
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
            //Weighted source neuron signal
            double weightedSignal = SourceNeuron.GetSignal(TargetNeuron.TypeOfActivation) * Weight; ;
            if (_signalQueue == null)
            {
                return weightedSignal;
            }
            else
            {
                //Signal to be delayed so use queue
                //Enqueue
                Signal sigObj = _signalQueue.GetElementOnEnqueuePosition();
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

        }//Signal

    }//InputSynapse

}//Namespace
