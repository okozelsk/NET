using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OKOSW.Extensions;
using OKOSW.MathTools;
using OKOSW.Neural.Activation;

namespace OKOSW.Neural.Reservoir.Analog
{
    /// <summary>
    /// Implements analog reservoir's neuron
    /// </summary>
    [Serializable]
    public class AnalogNeuron
    {
        //Constants
        public const double MAX_RETAINMENT_RATE = 0.99;
        //Attributes
        protected IActivationFunction m_aFn;
        protected double m_retainmentRate;
        protected double m_previousState;
        protected double m_currentState;
        //Service statistics
        public BasicStat StatesStat { get; }

        /// <summary>
        /// Construct simple analog neuron for analog reservoir. If retainmentRate is greater than 0, neuron is the leaky integrator.
        /// </summary>
        public AnalogNeuron(IActivationFunction aFn, double retainmentRate = 0)
        {
            m_aFn = aFn;
            m_retainmentRate = retainmentRate.Bound(0, MAX_RETAINMENT_RATE);
            m_previousState = m_currentState = 0;
            StatesStat = new BasicStat();
            return;
        }

        //Properties
        public double RetainmentRate { get { return m_retainmentRate; } }
        public double CurrentState { get { return m_currentState; } }
        public double PreviousState { get { return m_previousState; } }

        /// <summary>
        /// Resets neuron current and stored states to its initial default values (0).
        /// </summary>
        public virtual void Reset()
        {
            m_previousState = m_currentState = 0;
            return;
        }

        /// <summary>
        /// Sets neuron's current state
        /// </summary>
        public void NewState(double signal, bool collectStatistics)
        {
            m_currentState = (m_retainmentRate * m_currentState) + (1d - m_retainmentRate) * m_aFn.Compute(signal);
            if(collectStatistics)StatesStat.AddSampleValue(m_currentState);
            return;
        }

        /// <summary>
        /// Stores current state
        /// </summary>
        public void StoreCurrentState()
        {
            m_previousState = m_currentState;
            return;
        }
    }
}
