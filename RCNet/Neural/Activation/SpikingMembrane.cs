using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.MathTools.Differential;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Base class for spiking neuron models.
    /// </summary>
    [Serializable]
    public abstract class SpikingMembrane : IActivationFunction
    {
        //Attributes
        //Static working ranges
        protected static readonly Interval _inputRange = new Interval(double.NegativeInfinity.Bound(), double.PositiveInfinity.Bound());
        protected static readonly Interval _outputRange = new Interval(0, 1);

        //Parameter attributes
        protected Interval _stateRange;
        protected double _restV;
        protected double _resetV;
        protected double _firingThresholdV;
        protected int _refractoryPeriods;
        protected double _stimuliCoeff;

        //Operation attributes
        protected double _membraneV;
        protected bool _inRefractory;
        protected int _refractoryPeriod;
        protected double _stimuli;

        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="restV">Membrane rest voltage (mV)</param>
        /// <param name="resetV">Membrane reset voltage (mV)</param>
        /// <param name="firingThresholdV">Firing threshold (mV)</param>
        /// <param name="refractoryPeriods">Refractory periods (ms)</param>
        /// <param name="stimuliCoeff">Input stimuli coefficient (dimensionless)</param>
        protected SpikingMembrane(double restV,
                                  double resetV,
                                  double firingThresholdV,
                                  double refractoryPeriods,
                                  double stimuliCoeff
                                  )
        {
            _restV = restV;
            _resetV = resetV;
            _firingThresholdV = firingThresholdV;
            _refractoryPeriods = (int)refractoryPeriods;
            _stimuliCoeff = stimuliCoeff;
            _stateRange = new Interval(_restV, _firingThresholdV);
            _membraneV = _restV;
            _inRefractory = false;
            _refractoryPeriod = 0;
            return;
        }

        //Properties
        /// <summary>
        /// Type of the output
        /// </summary>
        public ActivationFactory.FunctionOutputType OutputType { get { return ActivationFactory.FunctionOutputType.Spike; } }

        /// <summary>
        /// Accepted input signal range
        /// </summary>
        public Interval InputRange { get { return _inputRange; } }

        /// <summary>
        /// Output signal range
        /// </summary>
        public Interval OutputRange { get { return _outputRange; } }

        /// <summary>
        /// Specifies whether the activation function supports derivative
        /// </summary>
        public bool SupportsDerivative { get { return false; } }

        /// <summary>
        /// Specifies whether the activation function is depending on its previous states
        /// </summary>
        public bool TimeDependent { get { return true; } }

        /// <summary>
        /// Range of the internal state
        /// </summary>
        public Interval InternalStateRange { get { return _stateRange; } }

        /// <summary>
        /// Internal state
        /// </summary>
        public double InternalState { get { return _membraneV; } }

        //Methods
        /// <summary>
        /// Resets membrane to initial state
        /// </summary>
        public virtual void Reset()
        {
            _membraneV = _restV;
            _inRefractory = false;
            _refractoryPeriod = 0;
            return;
        }

        /// <summary>
        /// Computes the result of the activation function
        /// </summary>
        /// <param name="x">Argument</param>
        public virtual double Compute(double x)
        {
            _stimuli = (x * _stimuliCoeff).Bound();
            double output = 0;
            if (_membraneV >= _firingThresholdV)
            {
                _membraneV = _resetV;
                _refractoryPeriod = 0;
                _inRefractory = true;
            }
            if (_inRefractory)
            {
                ++_refractoryPeriod;
                if (_refractoryPeriod > _refractoryPeriods)
                {
                    _refractoryPeriod = 0;
                    _inRefractory = false;
                }
                else
                {
                    //Ignore input stimuli
                    _stimuli = 0;
                }
            }
            //Compute membrane potential
            _membraneV = AutonomousODE.Solve(MembraneVoltageDiffEq, _membraneV, 1, 10, AutonomousODE.Method.Euler);
            //Output
            if (_membraneV >= _firingThresholdV)
            {
                output = 1;
                _membraneV = _firingThresholdV;
            }
            return output;
        }

        protected abstract double MembraneVoltageDiffEq(double membraneV);

        /// <summary>
        /// Unsupported functionality
        /// </summary>
        /// <param name="c">The result of the Compute method</param>
        /// <param name="x">The argument of the Compute method</param>
        public double ComputeDerivative(double c = double.NaN, double x = double.NaN)
        {
            throw new Exception("ComputeDerivative is unsupported method in case of spiking activation.");
        }


    }//SpikingMembrane

}//Namespace
