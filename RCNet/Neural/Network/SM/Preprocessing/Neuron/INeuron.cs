using RCNet.Neural.Activation;
using RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor;
using System.Collections.Generic;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron
{
    /// <summary>
    /// Neuron is the basic unit of a neural network.
    /// </summary>
    public interface INeuron
    {
        //Attribute properties
        /// <summary>
        /// Information about a neuron location within the neural preprocessor
        /// </summary>
        NeuronLocation Location { get; }

        /// <summary>
        /// Neuron's key statistics
        /// </summary>
        NeuronStatistics Statistics { get; }

        /// <summary>
        /// Neuron type (input or hidden)
        /// </summary>
        NeuronCommon.NeuronType Type { get; }

        /// <summary>
        /// Type of the activation function
        /// </summary>
        ActivationType TypeOfActivation { get; }

        /// <summary>
        /// Neuron's output data
        /// </summary>
        NeuronOutputData OutputData { get; }

        //Methods
        /// <summary>
        /// Resets neuron to its initial state
        /// </summary>
        /// <param name="statistics">Specifies whether to reset internal statistics</param>
        void Reset(bool statistics);

        /// <summary>
        /// Stores new incoming stimulation.
        /// </summary>
        /// <param name="iStimuli">Stimulation comming from input neurons</param>
        /// <param name="rStimuli">Stimulation comming from reservoir neurons</param>
        void NewStimulation(double iStimuli, double rStimuli);

        /// <summary>
        /// Computes neuron's new OutputData and updates Statistics.
        /// Must be called only once per stored incoming stimulation.
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        void Recompute(bool collectStatistics);

    }//INeuron

}//Namespace
