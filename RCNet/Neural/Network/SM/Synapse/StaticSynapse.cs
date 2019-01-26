using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Queue;
using RCNet.Neural.Network.SM.Neuron;

namespace RCNet.Neural.Network.SM.Synapse
{
    /// <summary>
    /// Static synapse deliveres constantly weighted signal from source to target neuron.
    /// Signal delivery can be delayed depending on Euclidean distance between source and target neuron.
    /// </summary>
    [Serializable]
    public class StaticSynapse : BaseSynapse, ISynapse
    {
        //Attributes
        private SimpleQueue<double> _signalQueue;

        //Constructor
        /// <summary>
        /// Creates initialized instance
        /// </summary>
        /// <param name="sourceNeuron">Source neuron</param>
        /// <param name="targetNeuron">Target neuron</param>
        /// <param name="weight">Synapse weight</param>
        public StaticSynapse(INeuron sourceNeuron,
                             INeuron targetNeuron,
                             double weight
                             )
            :base(sourceNeuron, targetNeuron, weight)
        {
            _signalQueue = null;
            //Initialize efficacy stat to constant 1 (static synapse has always efficacy 1)
            EfficacyStat.AddSampleValue(1d);
            return;
        }

        /// <summary>
        /// Resets synapse.
        /// </summary>
        /// <param name="statistics">Specifies whether to reset also internal statistics</param>
        public void Reset(bool statistics)
        {
            //Efficacy stat is constant so no need to reset statistics
            //Only queue is affected if it is instantiated
            _signalQueue?.Reset();
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
            if (Delay == 0)
            {
                //No queue will be used
                _signalQueue = null;
            }
            else
            {
                //Delay queue
                _signalQueue = new SimpleQueue<double>(Delay + 1);
            }
            return;
        }

        //Methods
        /// <summary>
        /// Returns signal to be delivered to target neuron.
        /// Note that this function has to be invoked only once per reservoir cycle !!!
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        public double GetSignal(bool collectStatistics)
        {
            double signal = ((SourceNeuron.OutputSignal + _add) / _div) * Weight;
            if (_signalQueue == null)
            {
                return signal;
            }
            else
            {
                //Enqueue weighted source neuron signal
                _signalQueue.Enqueue(signal);
                //Return signal from queue if queue is full
                return _signalQueue.Full ? _signalQueue.Dequeue() : 0;
            }
        }

    }//StaticSynapse

}//Namespace
