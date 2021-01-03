using RCNet.Extensions;
using RCNet.MathTools;
using System;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements the Sinusoid activation function.
    /// </summary>
    [Serializable]
    public class AFAnalogSinusoid : AFAnalogBase
    {
        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public AFAnalogSinusoid()
            : base(Interval.IntN1P1)
        {
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double x)
        {
            x = x.Bound();
            return Math.Sin(2d * x).Bound();
        }

        /// <inheritdoc/>
        public override double ComputeDerivative(double c, double x = double.NaN)
        {
            c = c.Bound();
            return Math.Cos(2d * c).Bound();
        }

    }//AFAnalogSinusoid

}//Namespace
