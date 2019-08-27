using System;
using RCNet.MathTools;
using RCNet.Extensions;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements LeakyReLU (Leaky Rectified Linear Unit) activation function
    /// </summary>
    [Serializable]
    public class LeakyReLU : AnalogActivationFunction
    {
        //Constants
        //Attributes
        private readonly Interval _outputRange;

        //Attribute properties
        /// <summary>
        /// A slope of the negative part of the curve
        /// </summary>
        public double NegSlope { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="negSlope">A slope of the negative part of the curve</param>
        public LeakyReLU(double negSlope)
            : base()
        {
            NegSlope = negSlope.Bound();
            if (NegSlope < 0)
            {
                throw new ArgumentOutOfRangeException("negSlope", "negSlope must be GE 0");
            }
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
        /// Output range of the Compute method
        /// </summary>
        public override Interval OutputRange { get { return _outputRange; } }

        //Methods
        /// <summary>
        /// Computes output of the activation function
        /// </summary>
        /// <param name="x">Activation input</param>
        public override double Compute(double x)
        {
            x = x.Bound();
            return ((x < 0) ? (NegSlope * x) : x).Bound();
        }

        /// <summary>
        /// Computes derivative (with respect to x)
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

