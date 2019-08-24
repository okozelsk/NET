using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;

namespace RCNet.Neural.Network.SM.Neuron
{
    /// <summary>
    /// Class encapsulates key statistics of the neuron
    /// </summary>
    [Serializable]
    public class NeuronStatistics
    {
        //Constants
        //Static attribute properties
        /// <summary>
        /// Normalization range of the neuron internal state
        /// </summary>
        public static Interval NormalizedStateRange { get; } = new Interval(0, 1);

        //Attribute properties
        /// <summary>
        /// Statistics of neuron's input stimulation passed to activation function
        /// </summary>
        public BasicStat InputStimuliStat { get; }

        /// <summary>
        /// Statistics of neuron's stimulation incoming from the reservoir
        /// </summary>
        public BasicStat ReservoirStimuliStat { get; }

        /// <summary>
        /// Statistics of neuron's total stimulation (all components together: Bias + Input + Reservoir)
        /// </summary>
        public BasicStat TotalStimuliStat { get; }

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
            InputStimuliStat = new BasicStat();
            ReservoirStimuliStat = new BasicStat();
            TotalStimuliStat = new BasicStat();
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
            InputStimuliStat.Reset();
            ReservoirStimuliStat.Reset();
            TotalStimuliStat.Reset();
            NormalizedActivationStateStat.Reset();
            OutputSignalStat.Reset();
            return;
        }

        /// <summary>
        /// Updates statistics
        /// </summary>
        /// <param name="iStimuli">Incoming stimulation related to part coming from connected Input neurons</param>
        /// <param name="rStimuli">Incoming stimulation related to part coming from connected reservoir's neurons</param>
        /// <param name="tStimuli">Incoming stimulation (all components together)</param>
        /// <param name="activationState">Neuron's activation function state</param>
        /// <param name="outputSignal">Neuron's output signal</param>
        public void Update(double iStimuli, double rStimuli, double tStimuli, double activationState, double outputSignal)
        {
            InputStimuliStat.AddSampleValue(iStimuli);
            ReservoirStimuliStat.AddSampleValue(rStimuli);
            TotalStimuliStat.AddSampleValue(tStimuli);
            NormalizedActivationStateStat.AddSampleValue(NormalizedStateRange.Rescale(activationState, ActivationStateRange));
            OutputSignalStat.AddSampleValue(outputSignal);
            return;
        }

    }//NeuronStatistics
}//Namespace
