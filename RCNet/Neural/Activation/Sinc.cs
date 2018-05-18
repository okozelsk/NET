using System;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Sinc activation function
    /// </summary>
    [Serializable]
    public class Sinc : AnalogActivationFunction
    {
        //Attributes
        private static readonly Interval _outputRange = new Interval(-0.217234, 1);

        //Constructor
        /// <summary>
        /// Instantiates Sinc activation function
        /// </summary>
        public Sinc()
            :base()
        {
            Reset();
            return;
        }

        //Properties
        /// <summary>
        /// Output signal range
        /// </summary>
        public override Interval OutputSignalRange { get { return _outputRange; } }

        /// <summary>
        /// Normal range of the internal state
        /// </summary>
        public override Interval InternalStateRange { get { return _outputRange; } }

        //Methods
        /// <summary>
        /// Computes output of the activation function (changes internal state)
        /// </summary>
        /// <param name="x">Activation input</param>
        public override double Compute(double x)
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
        /// Computes derivative of the activation input (does not change internal state)
        /// </summary>
        /// <param name="c">The result of the activation (Compute method)</param>
        /// <param name="x">Activation input (x argument of the Compute method)</param>
        public override double ComputeDerivative(double c, double x)
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
