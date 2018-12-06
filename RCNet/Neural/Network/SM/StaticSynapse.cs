using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Queue;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Static synapse deliveres constantly weighted signal from source to target neuron.
    /// Signal delivery can be delayed depending on Euclidean distance between source and target neuron.
    /// </summary>
    [Serializable]
    public class StaticSynapse : Synapse
    {
        //Constructor
        /// <summary>
        /// Creates initialized instance
        /// </summary>
        /// <param name="sourceNeuron">Source neuron</param>
        /// <param name="targetNeuron">Target neuron</param>
        /// <param name="weight">Synapse weight</param>
        /// <param name="maxDelay">Maximum delay (in cycles) of the signal delivery</param>
        public StaticSynapse(INeuron sourceNeuron,
                             INeuron targetNeuron,
                             double weight,
                             int maxDelay
                             )
            :base(sourceNeuron, targetNeuron, weight, maxDelay)
        {
            return;
        }

        //Methods
        /// <summary>
        /// Resets synapse.
        /// </summary>
        public override void Reset()
        {
            //Does nothing in case of the static synapse
            return;
        }

        /// <summary>
        /// Computes weighted signal and puts it into the internal queue
        /// </summary>
        protected override void EnqueueSignal()
        {
            _qSig.Enqueue(((SourceNeuron.OutputSignal + _add) / _div) * Weight);
            return;
        }

    }//StaticSynapse

}//Namespace
