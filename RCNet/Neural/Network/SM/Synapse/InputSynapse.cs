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
        //Attributes
        private SimpleQueue<Signal> _signalQueue;

        //Constructor
        /// <summary>
        /// Creates initialized instance
        /// </summary>
        /// <param name="sourceNeuron">Source neuron</param>
        /// <param name="targetNeuron">Target neuron</param>
        /// <param name="weight">Synapse weight (unsigned)</param>
        public InputSynapse(INeuron sourceNeuron,
                            INeuron targetNeuron,
                            double weight
                            )
            :base(sourceNeuron, targetNeuron, weight)
        {
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
        /// Returns signal to be delivered to target neuron.
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
                    //Update statistics if necessary
                    if (collectStatistics)
                    {
                        EfficacyStat.AddSampleValue(1d);
                    }
                    //Return resulting signal
                    return sourceSignal * Weight;
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
                    }
                    else
                    {
                        sigObj = new Signal { _weightedSignal = 0d };
                    }
                    _signalQueue.Enqueue(sigObj);
                }
                else
                {
                    //Signal to be delayed
                    Signal sigObj = _signalQueue.GetElementOnEnqueuePosition();
                    //Compute constantly weighted signal and put it into the queue simulating the signal traveling delay
                    if (sigObj != null)
                    {
                        sigObj._weightedSignal = ((SourceNeuron.OutputSignal + _add) / _div) * Weight;
                    }
                    else
                    {
                        sigObj = new Signal { _weightedSignal = ((SourceNeuron.OutputSignal + _add) / _div) * Weight };
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
                        //Update statistics if required
                        if (collectStatistics)
                        {
                            EfficacyStat.AddSampleValue(1d);
                        }
                        //Deliver the resulting signal
                        return sigObj._weightedSignal;
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

        }//Signal

    }//InputSynapse

}//Namespace
