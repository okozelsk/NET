using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Neuron is the basic unit of a neural network.
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
        /// Statistics of neuron stimulation signal
        /// </summary>
        BasicStat StimuliStat { get; }

        /// <summary>
        /// Statistics of neuron internal states
        /// </summary>
        BasicStat StatesStat { get; }

        /*
        /// <summary>
        /// Determines whether neuron's signal is excitatory, inhibitory or neutral
        /// </summary>
        CommonEnums.NeuronSignalType TransmissionSignalType { get; }
        */

        /// <summary>
        /// Neuron's transmission signal
        /// </summary>
        double TransmissinSignal { get; }

        /// <summary>
        /// Statistics of neuron's transmission signals
        /// </summary>
        BasicStat TransmissinSignalStat { get; }

        /// <summary>
        /// Value to be passed to readout layer as a predictor value
        /// </summary>
        double ReadoutPredictorValue { get; }

        //Methods
        /// <summary>
        /// Resets the neuron to its initial state
        /// </summary>
        /// <param name="resetStatistics">Specifies whether to reset internal statistics</param>
        void Reset(bool resetStatistics);

        /// <summary>
        /// Prepares and stores transmission signal
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        void PrepareTransmissionSignal(bool collectStatistics);
        
        /// <summary>
        /// Computes neuron's new state.
        /// </summary>
        /// <param name="stimuli">Input stimulation</param>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        void Compute(double stimuli, bool collectStatistics);

    }//INeuron

}//Namespace
