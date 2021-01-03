using System;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron
{
    /// <summary>
    /// Implements the holder of the neuron's outputs.
    /// </summary>
    [Serializable]
    public class NeuronOutputData
    {
        //Constants

        //Attributes
        /// <summary>
        /// The neuron's output analog signal.
        /// </summary>
        public double _analogSignal;

        /// <summary>
        /// The neuron's output spiking signal.
        /// </summary>
        public double _spikingSignal;

        /// <summary>
        /// The number of computation cycles gone from the last emitted spike or from the start if no spike yet.
        /// </summary>
        public int _spikeLeak;

        /// <summary>
        /// Specifies whether the neuron has already emitted a spike before the current computation cycle.
        /// </summary>
        public bool _afterFirstSpike;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public NeuronOutputData()
        {
            Reset();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public NeuronOutputData(NeuronOutputData source)
        {
            _analogSignal = source._analogSignal;
            _spikingSignal = source._spikingSignal;
            _spikeLeak = source._spikeLeak;
            _afterFirstSpike = source._afterFirstSpike;
            return;
        }

        //Methods
        /// <summary>
        /// Resets the neuron's output data.
        /// </summary>
        public void Reset()
        {
            _analogSignal = 0.5d;
            _spikingSignal = 0d;
            _spikeLeak = 0;
            _afterFirstSpike = false;
        }

        /// <summary>
        /// Creates the deep copy of this instance.
        /// </summary>
        public NeuronOutputData DeepClone()
        {
            return new NeuronOutputData(this);
        }

    }//NeuronOutputData

}//Namespace
