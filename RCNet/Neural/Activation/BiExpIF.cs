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
    /// Bidirectional Exponential integrate and fire.
    /// This is an innovative approach to allow +/- spikes.
    /// Hopefully, this innovation, which does not have an analogy in nature, will allow
    /// successful mutual cooperation between interconnected analog and spiking neurons.
    /// </summary>
    public class BiExpIF : IActivationFunction
    {
        //Attributes
        //Static working ranges
        private static readonly Interval _inputRange = new Interval(double.NegativeInfinity.Bound(), double.PositiveInfinity.Bound());
        private static readonly Interval _outputRange = new Interval(-1, 1);

        //Parameter attributes
        private Interval _stateRange;
        private double _membraneTimeScale;
        private double _membraneResistance;
        private double _restV;
        private double _resetV;
        private double _rheobaseV;
        private double _firingTresholdV;
        private double _sharpnessDeltaT;
        private int _refractoryPeriods;

        //Operation attributes
        private double _membraneV;
        private bool _inRefractory;
        private int _refractoryPeriod;

        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="membraneTimeScale">(ms)</param>
        /// <param name="membraneResistance">(MOhm)</param>
        /// <param name="resetV">Absolute value (mV)</param>
        /// <param name="rheobaseV">Absolute value (mV)</param>
        /// <param name="firingTresholdV">Absolute value (mV)</param>
        /// <param name="sharpnessDeltaT">(ms)</param>
        /// <param name="refractoryPeriods">(ms)</param>
        public BiExpIF(double membraneTimeScale,
                       double membraneResistance,
                       double resetV,
                       double rheobaseV,
                       double firingTresholdV,
                       double sharpnessDeltaT,
                       double refractoryPeriods
                       )
        {
            _membraneTimeScale = membraneTimeScale;
            _membraneResistance = membraneResistance;
            _restV = 0;
            _resetV = Math.Abs(resetV);
            _rheobaseV = Math.Abs(rheobaseV);
            _firingTresholdV = Math.Abs(firingTresholdV);
            _sharpnessDeltaT = sharpnessDeltaT;
            _refractoryPeriods = (int)refractoryPeriods;
            _stateRange = new Interval(-_firingTresholdV, _firingTresholdV);
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
            double output = 0;
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
            //Compute membrane potential
            _membraneV += (-(_membraneV - _restV) + Math.Sign(_membraneV)*_sharpnessDeltaT * Math.Exp((Math.Abs(_membraneV) - _rheobaseV) / _sharpnessDeltaT) + _membraneResistance * x) / (_membraneTimeScale);
            //Output
            if (Math.Abs(_membraneV) >= _firingTresholdV)
            {
                output = Math.Sign(_membraneV);
                _membraneV = _resetV * Math.Sign(_membraneV);
                _refractoryPeriod = 0;
                _inRefractory = true;
            }
            return output;
        }

        /// <summary>
        /// Derive is unsupported functionality
        /// </summary>
        /// <param name="c">The result of the Compute method</param>
        /// <param name="x">The argument of the Compute method</param>
        public double Derive(double c = double.NaN, double x = double.NaN)
        {
            throw new Exception("BiExpLIF does not support derivation");
        }


    }//ExpIF

}//Namespace
