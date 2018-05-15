using System;
using RCNet.MathTools;
using RCNet.Extensions;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// SoftExponential activation function
    /// </summary>
    [Serializable]
    public class SoftExponential : IActivationFunction
    {
        //Attributes
        //Static working ranges
        private static readonly Interval _inputRange = new Interval(double.NegativeInfinity.Bound(), double.PositiveInfinity.Bound());
        private static readonly Interval _outputRange = _inputRange;
        //Internal state
        private double _state;

        /// <summary>
        /// Alpha
        /// </summary>
        public double Alpha { get; }

        //Constructor
        /// <summary>
        /// Instantiates SoftExponential activation function
        /// </summary>
        /// <param name="alpha">Alpha</param>
        public SoftExponential(double alpha)
        {
            Alpha = alpha.Bound();
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
            if(Alpha < 0)
            {
                _state = (-(Math.Log(1 - Alpha * (x + Alpha)) / Alpha)).Bound();
            }
            else if(Alpha == 0)
            {
                _state = x;
            }
            else
            {
                _state = ((Math.Exp(Alpha * x) - 1) / Alpha + Alpha).Bound();
            }
            return _state;
        }

        /// <summary>
        /// Computes derivative
        /// </summary>
        /// <param name="c">The result of the Compute method</param>
        /// <param name="x">The argument of the Compute method</param>
        public double ComputeDerivative(double c, double x)
        {
            x = x.Bound();
            if(Alpha < 0)
            {
                return 1d  / (1d - Alpha * (Alpha + x));
            }
            else
            {
                return Math.Exp(Alpha * x);
            }
        }

    }//SoftExponential

}//Namespace

