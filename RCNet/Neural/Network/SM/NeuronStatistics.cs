using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Class encapsulates key statistics of the neuron
    /// </summary>
    [Serializable]
    public class NeuronStatistics
    {
        //Constants
        //Static attribute properties
        public static Interval NormalizedStateRange { get; } = new Interval(0, 1);

        //Attribute properties
        /// <summary>
        /// Statistics of neuron's input stimulation
        /// </summary>
        public BasicStat StimuliStat { get; }

        /// <summary>
        /// Neuron's normal states min max interval
        /// </summary>
        public Interval ActivationStateRange { get; }

        /// <summary>
        /// Statistics of neuron's states, uniformly rescalled to range NormalizedStateRange
        /// </summary>
        public BasicStat NormalizedActivationStateStat { get; }

        /// <summary>
        /// Statistics of neuron's output signal
        /// </summary>
        public BasicStat OutputSignalStat { get; }


        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public NeuronStatistics(Interval neuronStateRange)
        {
            StimuliStat = new BasicStat();
            ActivationStateRange = neuronStateRange.DeepClone();
            NormalizedActivationStateStat = new BasicStat();
            OutputSignalStat = new BasicStat();
            return;
        }

        //Methods
        /// <summary>
        /// Resets all statistics
        /// </summary>
        public void Reset()
        {
            StimuliStat.Reset();
            NormalizedActivationStateStat.Reset();
            OutputSignalStat.Reset();
            return;
        }

        /// <summary>
        /// Updates statistics
        /// </summary>
        /// <param name="stimuli">Incoming stimuli</param>
        /// <param name="activationState">Neuron's activation function state</param>
        /// <param name="outputSignal">Neuron's output signal</param>
        public void Update(double stimuli, double activationState, double outputSignal)
        {
            StimuliStat.AddSampleValue(stimuli);
            NormalizedActivationStateStat.AddSampleValue(NormalizedStateRange.Rescale(activationState, ActivationStateRange));
            OutputSignalStat.AddSampleValue(outputSignal);
            return;
        }

    }//NeuronStatistics
}//Namespace
