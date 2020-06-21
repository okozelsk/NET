using RCNet.Extensions;
using System;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron
{
    /// <summary>
    /// Neuron's output data
    /// </summary>
    [Serializable]
    public class NeuronOutputData
    {
        //Constants
        /// <summary>
        /// Index of analog output signal within the Signals array
        /// </summary>
        public const int AnalogSignalIdx = 0;
        /// <summary>
        /// Index of spiking output signal within the Signals array
        /// </summary>
        public const int SpikingSignalIdx = 1;

        //Attributes
        /// <summary>
        /// Neuron's output signals (analog, spiking)
        /// </summary>
        public double[] _signals;

        /// <summary>
        /// Computation cycles gone from the last emitted spike or start (if no spike emitted before current computation cycle)
        /// </summary>
        public int _spikeLeak;

        /// <summary>
        /// Specifies, if neuron has already emitted spike before current computation cycle
        /// </summary>
        public bool _afterFirstSpike;

        //Constructor
        /// <summary>
        /// Instantiates an initialized instance
        /// </summary>
        public NeuronOutputData()
        {
            _signals = new double[2];
            Reset();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance.</param>
        public NeuronOutputData(NeuronOutputData source)
        {
            _signals = (double[])source._signals.Clone();
            _spikeLeak = source._spikeLeak;
            _afterFirstSpike = source._afterFirstSpike;
            return;
        }

        //Properties
        /// <summary>
        /// Extracted analog signal from the Signals array
        /// </summary>
        public double AnalogSignal { get { return _signals[AnalogSignalIdx]; } }

        /// <summary>
        /// Extracted spiking signal from the Signals array
        /// </summary>
        public double SpikingSignal { get { return _signals[SpikingSignalIdx]; } }

        //Methods
        /// <summary>
        /// Resets neuron's output data to its initial state
        /// </summary>
        public void Reset()
        {
            _signals.Populate(0);
            _spikeLeak = 0;
            _afterFirstSpike = false;
        }

        /// <summary>
        /// Creates deep copy of this instance
        /// </summary>
        public NeuronOutputData DeepClone()
        {
            return new NeuronOutputData(this);
        }

    }//NeuronOutputData

}//Namespace
