using RCNet.Neural.Activation;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron
{
    /// <summary>
    /// Common interface of the input and hidden neurons.
    /// </summary>
    public interface INeuron
    {
        //Properties
        /// <inheritdoc cref="NeuronLocation"/>
        NeuronLocation Location { get; }

        /// <inheritdoc cref="NeuronStatistics"/>
        NeuronStatistics Statistics { get; }

        /// <inheritdoc cref="NeuronType"/>
        NeuronType Type { get; }

        /// <inheritdoc cref="ActivationType"/>
        ActivationType TypeOfActivation { get; }

        /// <inheritdoc cref="NeuronOutputData"/>
        NeuronOutputData OutputData { get; }

        //Methods
        /// <summary>
        /// Resets the neuron to its initial state.
        /// </summary>
        /// <param name="statistics">Specifies whether to reset also the neuron's statistics.</param>
        void Reset(bool statistics);

        /// <summary>
        /// Sets the new incoming stimulation.
        /// </summary>
        /// <param name="iStimuli">The stimulation incoming from the input neurons.</param>
        /// <param name="rStimuli">The stimulation incoming from the hidden neurons.</param>
        void NewStimulation(double iStimuli, double rStimuli);

        /// <summary>
        /// Computes the neuron's outputs and optionally updates also its statistics.
        /// </summary>
        /// <remarks>
        /// It must be called only once per the new stimulation.
        /// </remarks>
        /// <param name="collectStatistics">Specifies whether to update the neuron's statistics.</param>
        void Recompute(bool collectStatistics);

    }//INeuron

}//Namespace
