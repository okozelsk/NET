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

        //Attributes
        /// <summary>
        /// Neuron's output analog signal
        /// </summary>
        public double _analogSignal;

        /// <summary>
        /// Neuron's output spiking signal
        /// </summary>
        public double _spikingSignal;

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
            Reset();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance.</param>
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
        /// Resets neuron's output data to its initial state
        /// </summary>
        public void Reset()
        {
            _analogSignal = 0d;
            _spikingSignal = 0d;
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
