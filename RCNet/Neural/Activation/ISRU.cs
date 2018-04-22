using System;
using RCNet.MathTools;
using RCNet.Extensions;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// ISRU (Inverse Square Root Unit) activation function
    /// </summary>
    [Serializable]
    public class ISRU : IActivationFunction
    {
        //Attributes
        //Static working ranges
        private static readonly Interval _inputRange = new Interval(double.NegativeInfinity.Bound(), double.PositiveInfinity.Bound());
        private Interval _outputRange;
        //Internal state
        private double _state;

        /// <summary>
        /// The Alpha
        /// </summary>
        public double Alpha { get; }

        //Constructor
        /// <summary>
        /// Instantiates an Inverse Square Root Unit activation function
        /// </summary>
        /// <param name="alpha">The Alpha</param>
        public ISRU(double alpha = 1)
        {
            if(alpha <= 0)
            {
                throw new ArgumentOutOfRangeException("alpha", "Alpha must be GT 0");
            }
            Alpha = alpha.Bound(); ;
            _outputRange = new Interval(-1 / Math.Sqrt(Alpha), 1 / Math.Sqrt(Alpha));
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
            _state = (x / (1d + Alpha * x.Power(2))).Bound();
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
            return (1d / Math.Sqrt(1d + Alpha * x.Power(2))).Power(3);
        }

    }//ISRU

}//Namespace

