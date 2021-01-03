using RCNet.Extensions;
using RCNet.Neural.Activation;
using System;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron
{
    /// <summary>
    /// Implements the input analog neuron.
    /// </summary>
    /// <remarks>
    /// The input analog neuron is a special case of the neuron without an activation function.
    /// Its purpose is to provide an analog input for the reservoir's synapses.
    /// </remarks>
    [Serializable]
    public class AnalogInputNeuron : INeuron
    {
        //Attribute properties
        /// <inheritdoc/>
        public NeuronLocation Location { get; }

        /// <inheritdoc/>
        public NeuronStatistics Statistics { get; }

        /// <inheritdoc/>
        public NeuronOutputData OutputData { get; }

        //Attributes
        private readonly int _verticalCycles;
        private double _stimuli;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="location">The neuron's location.</param>
        /// <param name="verticalCycles">The number of the neuron's vertical cycles.</param>
        public AnalogInputNeuron(NeuronLocation location, int verticalCycles = 1)
        {
            Location = location;
            _verticalCycles = verticalCycles;
            Statistics = new NeuronStatistics();
            OutputData = new NeuronOutputData();
            Reset(false);
            return;
        }

        //Properties
        /// <inheritdoc/>
        public NeuronType Type { get { return NeuronType.Input; } }

        /// <inheritdoc/>
        public ActivationType TypeOfActivation { get { return ActivationType.Analog; } }

        //Methods
        /// <inheritdoc/>
        public void Reset(bool statistics)
        {
            _stimuli = 0;
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
            _stimuli = iStimuli.Bound();
            return;
        }

        /// <inheritdoc/>
        public void Recompute(bool collectStatistics)
        {
            //Analog output signal
            OutputData._analogSignal = _stimuli;
            //Transposed and scaled analog signal as direct input for spiking target neuron
            OutputData._spikingSignal = ((_stimuli + 1d) / 2d) / _verticalCycles;
            //Statistics
            if (collectStatistics)
            {
                Statistics.Update(_stimuli, 0d, _stimuli, _stimuli, _stimuli, 0d);
            }
            return;
        }

    }//AnalogInputNeuron

}//Namespace
