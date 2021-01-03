using RCNet.Extensions;
using RCNet.MathTools;
using System;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements the Bent Identity activation function.
    /// </summary>
    [Serializable]
    public class AFAnalogBentIdentity : AFAnalogBase
    {
        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public AFAnalogBentIdentity()
            : base(Interval.IntZPI)
        {
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double x)
        {
            x = x.Bound();
            return ((Math.Sqrt(x.Power(2) + 1d) - 1d) / 2d + x).Bound();
        }

        /// <inheritdoc/>
        public override double ComputeDerivative(double c, double x)
        {
            x = x.Bound();
            return x / (2d * Math.Sqrt(x.Power(2) + 1d)) + 1d;
        }

    }//AFAnalogBentIdentity

}//Namespace
