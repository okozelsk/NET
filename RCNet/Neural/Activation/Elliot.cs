using System;
using RCNet.MathTools;
using RCNet.Extensions;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Elliot activation function (aka Softsign).
    /// </summary>
    [Serializable]
    public class Elliot : IActivationFunction
    {
        //Attributes
        //Static working ranges
        private static readonly Interval _inputRange = new Interval(double.NegativeInfinity.Bound(), double.PositiveInfinity.Bound());
        private static readonly Interval _outputRange = new Interval(-1, 1);
        //Internal state
        private double _state;

        /// <summary>
        /// The curve slope
        /// </summary>
        public double Slope { get; }

        //Constructor
        /// <summary>
        /// Instantiates Elliot activation function
        /// </summary>
        /// <param name="slope">The curve slope</param>
        public Elliot(double slope = 1)
        {
            if (slope <= 0)
            {
                throw new ArgumentOutOfRangeException("slope", "Slope must be GT 0");
            }
            Slope = slope.Bound();
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
        /// Specifies whether the activation function supports derivative
        /// </summary>
        public bool SupportsDerivative { get { return true; } }

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
            _state = ((x * Slope) / (1d + Math.Abs(x * Slope))).Bound();
            return _state;
        }

        /// <summary>
        /// Computes derivative
        /// </summary>
        /// <param name="c">The result of the Compute method</param>
        /// <param name="x">The argument of the Compute method</param>
        public double ComputeDerivative(double c, double x = double.NaN)
        {
            c = c.Bound();
            return (Slope * 1d) / ((1d + Math.Abs(c * Slope)).Power(2));
        }

    }//Elliot

}//Namespace

