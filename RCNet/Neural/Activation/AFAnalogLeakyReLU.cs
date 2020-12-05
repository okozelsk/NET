using RCNet.Extensions;
using RCNet.MathTools;
using System;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements the Leaky Rectified Linear Unit activation function
    /// </summary>
    [Serializable]
    public class AFAnalogLeakyReLU : AFAnalogBase
    {
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
        public AFAnalogLeakyReLU(double negSlope)
            : base(null)
        {
            NegSlope = negSlope.Bound();
            if (NegSlope < 0)
            {
                throw new ArgumentOutOfRangeException("negSlope", "negSlope must be GE 0");
            }
            if (NegSlope > 0)
            {
                _outputRange = new Interval(double.NegativeInfinity.Bound(), double.PositiveInfinity.Bound(), false, true);
            }
            else
            {
                _outputRange = new Interval(0, double.PositiveInfinity.Bound(), false, true);
            }
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double x)
        {
            x = x.Bound();
            return ((x < 0) ? (NegSlope * x) : x).Bound();
        }

        /// <inheritdoc/>
        public override double ComputeDerivative(double c, double x)
        {
            x = x.Bound();
            return (x < 0) ? NegSlope : 1d;
        }

    }//AFAnalogLeakyReLU

}//Namespace

