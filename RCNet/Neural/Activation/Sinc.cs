using System;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Sinc activation function
    /// </summary>
    [Serializable]
    public class Sinc : IActivationFunction
    {
        //Attributes
        //Static working ranges
        private static readonly Interval _inputRange = new Interval(double.NegativeInfinity.Bound(), double.PositiveInfinity.Bound());
        private static readonly Interval _outputRange = new Interval(-0.217234, 1);
        //Internal state
        private double _state;

        //Constructor
        /// <summary>
        /// Instantiates Sinc activation function
        /// </summary>
        public Sinc()
        {
            Reset();
            return;
        }

        //Properties
        /// <summary>
        /// Type of the output
        /// </summary>
        public ActivationFactory.FunctionOutputType OutputType { get { return ActivationFactory.FunctionOutputType.Analog; } }

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
        public bool SupportsDerivation { get { return true; } }

        /// <summary>
        /// Specifies whether the activation function is depending on its previous states
        /// </summary>
        public bool TimeDependent { get { return false; } }

        /// <summary>
        /// Range of the internal state
        /// </summary>
        public Interval InternalStateRange { get { return _outputRange; } }

        /// <summary>
        /// Internal state
        /// </summary>
        public double InternalState { get { return _state; } }


        //Methods
        /// <summary>
        /// Resets function to initial state
        /// </summary>
        public void Reset()
        {
            _state = 0;
            return;
        }

        /// <summary>
        /// Computes the result of the activation function
        /// </summary>
        /// <param name="x">Argument</param>
        public double Compute(double x)
        {
            x = x.Bound();
            if (x == 0)
            {
                _state = 1d;
            }
            else
            {
                _state = Math.Sin(x) / x;
            }
            _state = _state.Bound(_outputRange.Min, _outputRange.Max);
            return _state;
        }

        /// <summary>
        /// Computes the derivation
        /// </summary>
        /// <param name="c">The result of the Compute method</param>
        /// <param name="x">The argument of the Compute method</param>
        public double Derive(double c, double x)
        {
            x = x.Bound();
            if (x == 0)
            {
                return 0;
            }
            else
            {
                return (Math.Cos(x) / x) / (Math.Sin(x) / x.Power(2));
            }
        }

    }//Sinc

}//Namespace
