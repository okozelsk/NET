using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Activation;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron
{
    /// <summary>
    /// Analog input neuron is the special type of a neuron without accosiated activation function. Its purpose is only to mediate
    /// external input analog value for a synapse in appropriate form.
    /// </summary>
    [Serializable]
    public class AnalogInputNeuron : INeuron
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
        private double _stimuli;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="location">Neuron's location</param>
        public AnalogInputNeuron(NeuronLocation location)
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
        public ActivationType TypeOfActivation { get { return ActivationType.Analog; } }

        //Methods
        /// <summary>
        /// Resets neuron to its initial state
        /// </summary>
        /// <param name="statistics">Specifies whether to reset internal statistics</param>
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

        /// <summary>
        /// Stores new incoming stimulation.
        /// </summary>
        /// <param name="iStimuli">External input analog signal</param>
        /// <param name="rStimuli">Parameter is ignored. Stimulation comming from reservoir neurons is irrelevant. </param>
        public void NewStimulation(double iStimuli, double rStimuli)
        {
            _stimuli = iStimuli.Bound();
            return;
        }

        /// <summary>
        /// Prepares new output signal (input for hidden neurons).
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        public void Recompute(bool collectStatistics)
        {
            OutputData._analogSignal = _stimuli;
            //Statistics
            if (collectStatistics)
            {
                Statistics.Update(_stimuli, 0d, _stimuli, _stimuli, _stimuli, 0d);
            }
            return;
        }

    }//AnalogInputNeuron

}//Namespace
