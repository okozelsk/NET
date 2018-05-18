using System;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Sinusoid activation function
    /// </summary>
    [Serializable]
    public class Sinusoid : AnalogActivationFunction
    {
        //Attributes
        private static readonly Interval _outputRange = new Interval(-1, 1);

        //Constructor
        /// <summary>
        /// Instantiates Sinusoid activation function
        /// </summary>
        public Sinusoid()
            :base()
        {
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
            _state = Math.Sin(2d * x).Bound();
            return _state;
        }

        /// <summary>
        /// Computes derivative of the activation input (does not change internal state)
        /// </summary>
        /// <param name="c">The result of the activation (Compute method)</param>
        /// <param name="x">Activation input (x argument of the Compute method)</param>
        public override double ComputeDerivative(double c, double x = double.NaN)
        {
            c = c.Bound();
            return Math.Cos(2d * c).Bound();
        }

    }//Sinusoid

}//Namespace
