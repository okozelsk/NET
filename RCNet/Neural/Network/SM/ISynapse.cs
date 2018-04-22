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
        //Static in time
        /// <summary>
        /// Source neuron - signal emitor
        /// </summary>
        INeuron SourceNeuron { get; }
        /// <summary>
        /// Target neuron - signal receiver
        /// </summary>
        INeuron TargetNeuron { get; }
        //Dynamic in time
        /// <summary>
        /// Weight of the synapse
        /// </summary>
        double Weight { get; set; }
        /// <summary>
        /// Statistics of the transported signals
        /// </summary>
        BasicStat SignalStat { get; }

        //Methods
        /// <summary>
        /// Adjusts synapse behavior.
        /// </summary>
        void Adjust();

        /// <summary>
        /// Computes stimulating signal to be passed to target neuron.
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to add signal into the internal statistics</param>
        /// <returns>Signal to target neuron</returns>
        double ComputeSignal(bool collectStatistics);

    }//ISynapse

}//Namespace
