using RCNet.Extensions;
using RCNet.MathTools;
using System;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements the Hyperbolic Tangent activation function.
    /// </summary>
    [Serializable]
    public class AFAnalogTanH : AFAnalogBase
    {
        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public AFAnalogTanH()
            : base(Interval.IntN1P1)
        {
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double x)
        {
            x = x.Bound();
            return 2d / (1d + Math.Exp(-2d * x)) - 1d;
        }

        /// <inheritdoc/>
        public override double ComputeDerivative(double c, double x = double.NaN)
        {
            c = c.Bound();
            return 1d - c.Power(2);
        }

    }//AFAnalogTanH

}//Namespace
