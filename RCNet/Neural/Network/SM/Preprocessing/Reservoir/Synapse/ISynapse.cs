using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron;
using RCNet.MathTools;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Synapse
{
    /// <summary>
    /// Common interface of implemented synapses.
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
        /// Euclidean distance between SourceNeuron and TargetNeuron
        /// </summary>
        double Distance { get; }

        /// <summary>
        /// Weight of the synapse
        /// </summary>
        double Weight { get; }

        /// <summary>
        /// Efficacy statistics of the synapse
        /// </summary>
        BasicStat EfficacyStat { get; }

        //Methods
        /// <summary>
        /// Resets synapse.
        /// </summary>
        /// <param name="statistics">Specifies whether to reset also internal statistics</param>
        void Reset(bool statistics);

        /// <summary>
        /// Rescales the synapse weight.
        /// </summary>
        /// <param name="scale">Scale factor</param>
        void Rescale(double scale);

        /// <summary>
        /// Returns signal to be delivered to target neuron.
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        double GetSignal(bool collectStatistics);

    }//ISynapse

}//Namespace
