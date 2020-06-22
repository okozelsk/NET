using RCNet.Neural.Activation;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron
{
    /// <summary>
    /// Spiking input neuron is the special type of a neuron with no assosiated activation function. Its purpose is only to mediate
    /// external input spike value for a synapse.
    /// </summary>
    [Serializable]
    public class SpikingInputNeuron : INeuron
    {
        //Attribute properties
        /// <summary>
        /// Information about a neuron location within the neural preprocessor
        /// Note that Input neuron home PoolID is always -1 because Input neurons do not belong to a standard pools.
        /// </summary>
        public NeuronLocation Location { get; }

        /// <summary>
        /// Neuron's key statistics
        /// </summary>
        public NeuronStatistics Statistics { get; }

        /// <summary>
        /// Neuron's output data
        /// </summary>
        public NeuronOutputData OutputData { get; }

        //Attributes
        private double _inputSpike;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
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
        /// <summary>
        /// Neuron type
        /// </summary>
        public NeuronCommon.NeuronType Type { get { return NeuronCommon.NeuronType.Input; } }

        /// <summary>
        /// Type of the activation function
        /// </summary>
        public ActivationType TypeOfActivation { get { return ActivationType.Spiking; } }

        //Methods
        /// <summary>
        /// Resets neuron to its initial state
        /// </summary>
        /// <param name="statistics">Specifies whether to reset internal statistics</param>
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

        /// <summary>
        /// Stores new incoming stimulation.
        /// </summary>
        /// <param name="iStimuli">External input spike</param>
        /// <param name="rStimuli">Parameter is ignored. Stimulation comming from reservoir neurons is irrelevant. </param>
        public void NewStimulation(double iStimuli, double rStimuli)
        {
            _inputSpike = iStimuli > 0 ? 1d : 0d;
            return;
        }

        /// <summary>
        /// Prepares new output signal (input for hidden neurons).
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
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
