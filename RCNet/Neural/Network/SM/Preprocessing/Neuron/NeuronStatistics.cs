using RCNet.MathTools;
using System;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron
{
    /// <summary>
    /// Implements the holder of the neuron's key statistics.
    /// </summary>
    [Serializable]
    public class NeuronStatistics
    {
        //Constants
        //Attribute properties
        /// <summary>
        /// The statistics of stimulation incoming from the input neurons.
        /// </summary>
        public BasicStat InputStimuliStat { get; }

        /// <summary>
        /// The statistics of stimulation incoming from the reservoir's hidden neurons.
        /// </summary>
        public BasicStat ReservoirStimuliStat { get; }

        /// <summary>
        /// The statistics of the total stimulation (Bias + Input + Hidden).
        /// </summary>
        public BasicStat TotalStimuliStat { get; }

        /// <summary>
        /// The statistics of activations.
        /// </summary>
        public BasicStat ActivationStat { get; }

        /// <summary>
        /// The statistics of the neuron's normalized analog output signal.
        /// </summary>
        public BasicStat AnalogSignalStat { get; }

        /// <summary>
        /// The statistics of the neuron's spiking output signal.
        /// </summary>
        public BasicStat FiringStat { get; }


        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public NeuronStatistics()
        {
            InputStimuliStat = new BasicStat();
            ReservoirStimuliStat = new BasicStat();
            TotalStimuliStat = new BasicStat();
            ActivationStat = new BasicStat();
            AnalogSignalStat = new BasicStat();
            FiringStat = new BasicStat();
            return;
        }

        //Methods
        /// <summary>
        /// Resets the statistics.
        /// </summary>
        public void Reset()
        {
            InputStimuliStat.Reset();
            ReservoirStimuliStat.Reset();
            TotalStimuliStat.Reset();
            ActivationStat.Reset();
            AnalogSignalStat.Reset();
            FiringStat.Reset();
            return;
        }

        /// <summary>
        /// Updates the statistics.
        /// </summary>
        /// <param name="iStimuli">The stimulation incoming from the input neurons.</param>
        /// <param name="rStimuli">The stimulation incoming from the reservoir's hidden neurons.</param>
        /// <param name="tStimuli">The total stimulation (Bias + Input + Hidden).</param>
        /// <param name="activation">The activation.</param>
        /// <param name="analogSignal">The analog output signal.</param>
        /// <param name="spike">The spiking output signal.</param>
        public void Update(double iStimuli, double rStimuli, double tStimuli, double activation, double analogSignal, double spike)
        {
            InputStimuliStat.AddSample(iStimuli);
            ReservoirStimuliStat.AddSample(rStimuli);
            TotalStimuliStat.AddSample(tStimuli);
            ActivationStat.AddSample(activation);
            AnalogSignalStat.AddSample(analogSignal);
            FiringStat.AddSample(spike);
            return;
        }

    }//NeuronStatistics

}//Namespace
