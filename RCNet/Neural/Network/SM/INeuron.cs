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
        /// Statistics of neuron stimulation signal
        /// </summary>
        BasicStat StimuliStat { get; }

        /// <summary>
        /// Statistics of neuron internal states
        /// </summary>
        BasicStat StatesStat { get; }

        /// <summary>
        /// Output signal range
        /// </summary>
        Interval TransmissionSignalRange { get; }

        /// <summary>
        /// Determines whether neuron's signal is excitatory or inhibitory
        /// </summary>
        CommonEnums.NeuronSignalType TransmissionSignalType { get; }

        /// <summary>
        /// Neuron's transmission signal
        /// </summary>
        double TransmissionSignal { get; }

        /// <summary>
        /// Statistics of neuron's transmission signal values
        /// </summary>
        BasicStat TransmissionSignalStat { get; }

        /// <summary>
        /// Statistics of neuron's transmission signal frequency
        /// </summary>
        BasicStat TransmissionFreqStat { get; }

        /// <summary>
        /// Value to be passed to readout layer as a predictor value.
        /// Available after the execution of PrepareTransmissionSignal function.
        /// </summary>
        double ReadoutValue { get; }

        /// <summary>
        /// Value to be passed to readout layer as an augmented predictor value
        /// Available after the execution of PrepareTransmissionSignal function.
        /// </summary>
        double ReadoutAugmentedValue { get; }

        //Methods
        /// <summary>
        /// Resets neuron to its initial state
        /// </summary>
        /// <param name="resetStatistics">Specifies whether to reset internal statistics</param>
        void Reset(bool resetStatistics);

        /// <summary>
        /// Prepares neuron's transmission signal
        /// </summary>
        void PrepareTransmissionSignal();

        /// <summary>
        /// Computes neuron's new state.
        /// </summary>
        /// <param name="stimuli">Input stimulation</param>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        void Compute(double stimuli, bool collectStatistics);

    }//INeuron

}//Namespace
