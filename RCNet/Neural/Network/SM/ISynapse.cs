using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Synapse is a transporter of the weighted signal from source neuron to target neuron.
    /// </summary>
    public interface ISynapse
    {
        //Properties
        /// <summary>
        /// Source neuron - signal emitor
        /// </summary>
        INeuron SourceNeuron { get; }

        /// <summary>
        /// Target neuron - signal receiver
        /// </summary>
        INeuron TargetNeuron { get; }

        /// <summary>
        /// Weight of the synapse
        /// </summary>
        double Weight { get; set; }

        //Methods
        /// <summary>
        /// Adjusts synapse behavior.
        /// </summary>
        void Adjust();

        /// <summary>
        /// Computes weighted signal from source neuron to be delivered to target neuron.
        /// </summary>
        double GetWeightedSignal();

    }//ISynapse

}//Namespace
