using System;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Activation;

namespace RCNet.Neural.Network.EchoState
{
    /// <summary>
    /// Implements the analog reservoir's neuron
    /// </summary>
    [Serializable]
    public class AnalogNeuron
    {
        //Constants
        /// <summary>
        /// Maximum retainment rate value
        /// </summary>
        public const double RetainmentMaxRate = 0.99;

        //Attributes
        /// <summary>
        /// Neuron's activation function
        /// </summary>
        private IActivationFunction _activation;
        /// <summary>
        /// Neuron's bias
        /// </summary>
        private double _bias;
        /// <summary>
        /// Neuron's retainment rate
        /// </summary>
        private double _retainmentRate;
        /// <summary>
        /// Stored previous neuron's state
        /// </summary>
        private double _previousState;
        /// <summary>
        /// Neuron's current state.
        /// </summary>
        private double _currentState;
        //Attribute properties
        /// <summary>
        /// Neuron's states statistics
        /// </summary>
        public BasicStat StatesStat { get; }

        //Constructor
        /// <summary>
        /// Instantiates the neuron to be used in the analog reservoir.
        /// If retainmentRate is greater than 0, neuron is the leaky integrator.
        /// </summary>
        /// <param name="activation">Neuron's activation function</param>
        /// <param name="bias">Neuron's bias value</param>
        /// <param name="retainmentRate">Neuron's retainment rate</param>
        public AnalogNeuron(IActivationFunction activation, double bias, double retainmentRate = 0)
        {
            _activation = activation;
            _bias = bias;
            _retainmentRate = retainmentRate.Bound(0, RetainmentMaxRate);
            _previousState = _currentState = 0;
            StatesStat = new BasicStat();
            return;
        }

        //Properties
        /// <summary>
        /// Retainment rate value
        /// </summary>
        public double RetainmentRate { get { return _retainmentRate; } }
        /// <summary>
        /// Neuron's current state
        /// </summary>
        public double CurrentState { get { return _currentState; } }
        /// <summary>
        /// Neuron's previous state
        /// </summary>
        public double PreviousState { get { return _previousState; } }

        /// <summary>
        /// Resets neuron to its initial state (0) and optionaly resets internal statistics
        /// </summary>
        /// <param name="resetStatistics">Specifies whether to reset the internal statistics</param>
        public virtual void Reset(bool resetStatistics)
        {
            _previousState = _currentState = 0;
            if(resetStatistics)
            {
                StatesStat.Reset();
            }
            return;
        }

        /// <summary>
        /// Computes neuron's current state and updates statistics.
        /// </summary>
        public void Compute(double signal, bool collectStatistics)
        {
            _currentState = (_retainmentRate * _currentState) + (1d - _retainmentRate) * _activation.Compute(_bias + signal);
            if(collectStatistics)StatesStat.AddSampleValue(_currentState);
            return;
        }

        /// <summary>
        /// Stores current state to be accesible later.
        /// </summary>
        public void StoreCurrentState()
        {
            _previousState = _currentState;
            return;
        }

    }//AnalogNeuron

}//Namespace
