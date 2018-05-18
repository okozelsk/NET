using System;
using RCNet.MathTools;
using RCNet.Extensions;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// LeakyReLU activation function
    /// </summary>
    [Serializable]
    public class LeakyReLU : AnalogActivationFunction
    {
        //Attributes
        private Interval _outputRange;

        //Attribute properties
        /// <summary>
        /// The curve slope
        /// </summary>
        public double NegSlope { get; }

        //Constructor
        /// <summary>
        /// Instantiates a LeakyReLU activation function
        /// </summary>
        /// <param name="negSlope">The negative half-line slope</param>
        public LeakyReLU(double negSlope = 0.01)
            :base()
        {
            if (negSlope < 0)
            {
                throw new ArgumentOutOfRangeException("negSlope", "negSlope must be GE 0");
            }
            NegSlope = negSlope.Bound();
            if (NegSlope > 0)
            {
                _outputRange = new Interval(double.NegativeInfinity.Bound(), double.PositiveInfinity.Bound());
            }
            else
            {
                _outputRange = new Interval(0, double.PositiveInfinity.Bound());
            }
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
            _state = ((x < 0) ? (NegSlope * x) : x).Bound();
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
            return (x < 0) ? NegSlope : 1d;
        }

    }//LeakyReLU

}//Namespace

