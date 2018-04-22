using System;
using RCNet.MathTools;
using RCNet.Extensions;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// LeakyReLU activation function
    /// </summary>
    [Serializable]
    public class LeakyReLU : IActivationFunction
    {
        //Attributes
        //Static working ranges
        private static readonly Interval _inputRange = new Interval(double.NegativeInfinity.Bound(), double.PositiveInfinity.Bound());
        private Interval _outputRange;
        //Internal state
        private double _state;

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
            _state = ((x < 0) ? (NegSlope * x) : x).Bound();
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
            return (x < 0) ? NegSlope : 1d;
        }

    }//LeakyReLU

}//Namespace

