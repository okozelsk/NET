using System;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Sinusoid activation function
    /// </summary>
    [Serializable]
    public class Sinusoid : IActivationFunction
    {
        //Attributes
        //Static working ranges
        private static readonly Interval _inputRange = new Interval(double.NegativeInfinity.Bound(), double.PositiveInfinity.Bound());
        private static readonly Interval _outputRange = new Interval(-1, 1);
        //Internal state
        private double _state;

        //Constructor
        /// <summary>
        /// Instantiates Sinusoid activation function
        /// </summary>
        public Sinusoid()
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
            _state = Math.Sin(2d * x).Bound();
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
            return Math.Cos(2d * c).Bound();
        }

    }//Sinusoid

}//Namespace
