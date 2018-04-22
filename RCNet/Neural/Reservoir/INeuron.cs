using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;

namespace RCNet.Neural.Reservoir
{
    /// <summary>
    /// Neuron is the basic unit of a neural network.
    /// Neuron has to accept input stimuli within the range (-infinity, +infinity)
    /// and has to produce output signal within the 
    /// range (-1, 1)
    /// </summary>
    public interface INeuron
    {
        /// <summary>
        /// Home pool identificator and neuron placement within the pool
        /// </summary>
        NeuronPlacement Placement { get; }

        /// <summary>
        /// Constant bias of the neuron
        /// </summary>
        double Bias { get; }

        /// <summary>
        /// Current state of the neuron
        /// </summary>
        double State { get; }

        /// <summary>
        /// Statistics of neuron states
        /// </summary>
        BasicStat StatesStat { get; }

        /// <summary>
        /// Statistics of incoming stimulations
        /// </summary>
        BasicStat StimuliStat { get; }

        /// <summary>
        /// Current signal according to current state
        /// </summary>
        double CurrentSignal { get; }

        /// <summary>
        /// Stored signal for transmission purposes
        /// </summary>
        double StoredSignal { get; }

        /// <summary>
        /// Statistics of neuron output signals
        /// </summary>
        BasicStat SignalStat { get; }

        //Methods
        /// <summary>
        /// Resets the neuron to its initial state
        /// </summary>
        /// <param name="statistics">Specifies whether to reset also internal statistics</param>
        void Reset(bool statistics);
        
        /// <summary>
        /// Stores current signal to be used for signal transmission
        /// </summary>
        void StoreSignal();
        
        /// <summary>
        /// Computes neuron state and output signal.
        /// </summary>
        /// <param name="stimuli">Input stimulation</param>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        void Compute(double stimuli, bool collectStatistics);

    }//INeuron

}//Namespace
