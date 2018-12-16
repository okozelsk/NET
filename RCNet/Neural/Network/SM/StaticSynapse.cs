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
        /// <param name="delay">Synapse delay (in cycles) of the signal delivery</param>
        public StaticSynapse(INeuron sourceNeuron,
                             INeuron targetNeuron,
                             double weight,
                             int delay
                             )
            :base(sourceNeuron, targetNeuron, weight, delay)
        {
            return;
        }

        //Methods
        /// <summary>
        /// Computes synapse efficacy based on the pre-synaptic activity.
        /// In case of static synapse, the efficacy is always full (1)
        /// </summary>
        protected override double GetPreSynapticEfficacy()
        {
            return 1d;
        }

        /// <summary>
        /// Computes synapse efficacy based on the post-synaptic activity
        /// In case of static synapse, the efficacy is always full (1)
        /// </summary>
        protected override double GetPostSynapticEfficacy()
        {
            return 1d;
        }

    }//StaticSynapse

}//Namespace
