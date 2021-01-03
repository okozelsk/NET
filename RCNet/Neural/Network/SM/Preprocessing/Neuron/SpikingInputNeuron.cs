using RCNet.Neural.Activation;
using System;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron
{
    /// <summary>
    /// Implements the input spiking neuron.
    /// </summary>
    /// <remarks>
    /// The input spiking neuron is a special case of the neuron without an activation function.
    /// Its purpose is to provide a spiking input for the reservoir's synapses.
    /// </remarks>
    [Serializable]
    public class SpikingInputNeuron : INeuron
    {
        //Attribute properties
        /// <inheritdoc/>
        public NeuronLocation Location { get; }

        /// <inheritdoc/>
        public NeuronStatistics Statistics { get; }

        /// <inheritdoc/>
        public NeuronOutputData OutputData { get; }

        //Attributes
        private double _inputSpike;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="location">Neuron's location</param>
        public SpikingInputNeuron(NeuronLocation location)
        {
            Location = location;
            Statistics = new NeuronStatistics();
            OutputData = new NeuronOutputData();
            Reset(false);
            return;
        }

        //Properties
        /// <inheritdoc/>
        public NeuronType Type { get { return NeuronType.Input; } }

        /// <inheritdoc/>
        public ActivationType TypeOfActivation { get { return ActivationType.Spiking; } }

        //Methods
        /// <inheritdoc/>
        public void Reset(bool statistics)
        {
            _inputSpike = 0;
            OutputData.Reset();
            if (statistics)
            {
                Statistics.Reset();
            }
            return;
        }

        /// <inheritdoc/>
        public void NewStimulation(double iStimuli, double rStimuli)
        {
            _inputSpike = iStimuli > 0 ? 1d : 0d;
            return;
        }

        /// <inheritdoc/>
        public void Recompute(bool collectStatistics)
        {
            if (OutputData._spikingSignal > 0)
            {
                //Spike during previous cycle, so reset the counter
                OutputData._afterFirstSpike = true;
                OutputData._spikeLeak = 0;
            }
            ++OutputData._spikeLeak;
            OutputData._spikingSignal = _inputSpike;
            //Statistics
            if (collectStatistics)
            {
                Statistics.Update(_inputSpike, 0d, _inputSpike, _inputSpike, 0, _inputSpike);
            }
            return;
        }

    }//SpikingInputNeuron

}//Namespace
