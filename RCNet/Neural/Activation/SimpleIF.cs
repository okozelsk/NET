using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements simple form of Leaky Integrate and Fire neuron model
    /// </summary>
    public class SimpleIF : IActivationFunction
    {
        //Constants
        private double SpikeCurrent = 1d;

        //Attributes
        //Static working ranges
        private static readonly Interval _inputRange = new Interval(double.NegativeInfinity.Bound(), double.PositiveInfinity.Bound());
        private static readonly Interval _outputRange = new Interval(0, 1);

        //Parameters
        private Interval _stateRange;
        private double _membraneResistance;
        private double _membraneDecayRate;
        private double _restV;
        private double _resetV;
        private double _firingThresholdV;
        private int _refractoryPeriods;
        private double _stimuliCoeff;

        //Operation
        private double _membraneV;
        private bool _inRefractory;
        private int _refractoryPeriod;

        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="membraneResistance">Membrane resisatance in MOhm.</param>
        /// <param name="membraneDecayRate">Membrane potential decay</param>
        /// <param name="resetV">Reset voltage in mV (positive value)</param>
        /// <param name="firingThresholdV">Firing threshold voltage in mV (positive value)</param>
        /// <param name="refractoryPeriods">Number of after spike computation cycles, while an input stimuli is ignored.</param>
        /// <param name="stimuliCoeff"></param>
        public SimpleIF(double membraneResistance,
                        double membraneDecayRate,
                        double resetV,
                        double firingThresholdV,
                        double refractoryPeriods,
                        double stimuliCoeff
                        )
        {
            _membraneResistance = membraneResistance;
            _membraneDecayRate = membraneDecayRate;
            _restV = 0;
            _resetV = Math.Abs(resetV);
            _firingThresholdV = Math.Abs(firingThresholdV);
            _refractoryPeriods = (int)Math.Round(refractoryPeriods, 0);
            _stimuliCoeff = stimuliCoeff;
            _stateRange = new Interval(_restV, _firingThresholdV);
            Reset();
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
        /// Resets function to initial state
        /// </summary>
        public void Reset()
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
        public double Compute(double x)
        {
            x = (x * _stimuliCoeff).Bound();
            double spike = 0;
            if (_membraneV >= _firingThresholdV)
            {
                //Membrane potential after spike
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
                    x = 0;
                }
            }
            //Compute membrane new potential
            _membraneV = _restV + (_membraneV - _restV) * (1d - _membraneDecayRate) + _membraneResistance * x;
            //Output
            if (_membraneV >= _firingThresholdV)
            {
                spike = SpikeCurrent;
                _membraneV = _firingThresholdV;
            }
            return spike;
        }

        /// <summary>
        /// Unsupported functionality
        /// </summary>
        /// <param name="c">The result of the Compute method</param>
        /// <param name="x">The argument of the Compute method</param>
        public double ComputeDerivative(double c = double.NaN, double x = double.NaN)
        {
            throw new Exception("SimpleIF does not support ComputeDerivative");
        }

    }//SimpleIF

}//Namespace
