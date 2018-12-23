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
        /// Statistics of neuron's stimulation (all components together)
        /// </summary>
        public BasicStat TStimuliStat { get; }

        /// <summary>
        /// Statistics of neuron's stimulation (stimulation component related to part coming from connected reservoir's neurons)
        /// </summary>
        public BasicStat RStimuliStat { get; }

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
            TStimuliStat = new BasicStat();
            RStimuliStat = new BasicStat();
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
            TStimuliStat.Reset();
            RStimuliStat.Reset();
            NormalizedActivationStateStat.Reset();
            OutputSignalStat.Reset();
            return;
        }

        /// <summary>
        /// Updates statistics
        /// </summary>
        /// <param name="tStimuli">Incoming stimulation (all components together)</param>
        /// <param name="rStimuli">Incoming stimulation component related to part coming from connected reservoir's neurons</param>
        /// <param name="activationState">Neuron's activation function state</param>
        /// <param name="outputSignal">Neuron's output signal</param>
        public void Update(double tStimuli, double rStimuli, double activationState, double outputSignal)
        {
            TStimuliStat.AddSampleValue(tStimuli);
            RStimuliStat.AddSampleValue(rStimuli);
            NormalizedActivationStateStat.AddSampleValue(NormalizedStateRange.Rescale(activationState, ActivationStateRange));
            OutputSignalStat.AddSampleValue(outputSignal);
            return;
        }

    }//NeuronStatistics
}//Namespace
