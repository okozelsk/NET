using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Activation;

namespace RCNet.Neural.Network.EchoState
{
    /// <summary>
    /// Implements analog reservoir's neuron
    /// </summary>
    [Serializable]
    public class AnalogNeuron
    {
        //Constants
        public const double RetirementMaxRate = 0.99;
        //Attributes
        protected IActivationFunction _activation;
        protected double _retainmentRate;
        protected double _previousState;
        protected double _currentState;
        //Service statistics
        public BasicStat StatesStat { get; }

        /// <summary>
        /// Construct simple analog neuron for analog reservoir. If retainmentRate is greater than 0, neuron is the leaky integrator.
        /// </summary>
        public AnalogNeuron(IActivationFunction activation, double retainmentRate = 0)
        {
            _activation = activation;
            _retainmentRate = retainmentRate.Bound(0, RetirementMaxRate);
            _previousState = _currentState = 0;
            StatesStat = new BasicStat();
            return;
        }

        //Properties
        public double RetainmentRate { get { return _retainmentRate; } }
        public double CurrentState { get { return _currentState; } }
        public double PreviousState { get { return _previousState; } }

        /// <summary>
        /// Resets neuron current and stored states to its initial default values (0).
        /// </summary>
        public virtual void Reset()
        {
            _previousState = _currentState = 0;
            return;
        }

        /// <summary>
        /// Sets neuron's current state
        /// </summary>
        public void NewState(double signal, bool collectStatistics)
        {
            _currentState = (_retainmentRate * _currentState) + (1d - _retainmentRate) * _activation.Compute(signal);
            if(collectStatistics)StatesStat.AddSampleValue(_currentState);
            return;
        }

        /// <summary>
        /// Stores current state
        /// </summary>
        public void StoreCurrentState()
        {
            _previousState = _currentState;
            return;
        }
    }
}
