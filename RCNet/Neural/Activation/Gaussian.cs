using System;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Gaussian activation function
    /// </summary>
    [Serializable]
    public class Gaussian : AnalogActivationFunction
    {
        //Attributes
        private static readonly Interval _outputRange = new Interval(0, 1);

        //Constructor
        /// <summary>
        /// Instantiates Gaussian activation function
        /// </summary>
        public Gaussian()
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
            _state = (Math.Exp(-(x.Power(2)))).Bound();
            return _state;
        }

        /// <summary>
        /// Computes derivative of the activation input (does not change internal state)
        /// </summary>
        /// <param name="c">The result of the activation (Compute method)</param>
        /// <param name="x">Activation input (x argument of the Compute method)</param>
        public override double ComputeDerivative(double c, double x)
        {
            c = c.Bound();
            x = x.Bound();
            return -2*x*c;
        }

    }//Gaussian

}//Namespace
