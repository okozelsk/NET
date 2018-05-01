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
    /// Implements Leaky Integrate and Fire neuron model
    /// </summary>
    public class LIF : IActivationFunction
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
        private double _firingTresholdV;
        private int _refractoryPeriods;

        //Operation
        private double _membraneV;
        private bool _inRefractory;
        private int _refractoryPeriod;

        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="membraneResistance">Membrane resisatance in MOhm.</param>
        /// <param name="membraneDecayRate">Membrane potential decay</param>
        /// <param name="absResetV">Reset voltage in mV (positive value)</param>
        /// <param name="absFiringTresholdV">Firing treshold voltage in mV (positive value)</param>
        /// <param name="refractoryPeriods">Number of after spike computation cycles, when is ignored input stimuli.</param>
        public LIF(double membraneResistance,
                   double membraneDecayRate,
                   double absResetV,
                   double absFiringTresholdV,
                   double refractoryPeriods
                   )
        {
            _membraneResistance = membraneResistance;
            _membraneDecayRate = membraneDecayRate;
            _restV = 0;
            _resetV = Math.Abs(absResetV);
            _firingTresholdV = Math.Abs(absFiringTresholdV);
            _refractoryPeriods = (int)Math.Round(refractoryPeriods, 0);
            _stateRange = new Interval(_restV, _firingTresholdV);
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
        /// Specifies whether the activation function supports derivation
        /// </summary>
        public bool SupportsDerivation { get { return false; } }

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
            x = x.Bound();
            double spike = 0;
            if (_membraneV >= _firingTresholdV)
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
            if (_membraneV >= _firingTresholdV)
            {
                spike = SpikeCurrent;
                _membraneV = _firingTresholdV;
            }
            return spike;
        }

        /// <summary>
        /// Derive is unsupported functionality
        /// </summary>
        /// <param name="c">The result of the Compute method</param>
        /// <param name="x">The argument of the Compute method</param>
        public double Derive(double c = double.NaN, double x = double.NaN)
        {
            throw new Exception("LIF does not support derivation");
        }

    }//LIF

}//Namespace
