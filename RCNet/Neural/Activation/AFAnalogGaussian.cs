using System;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements the Gaussian activation function
    /// </summary>
    [Serializable]
    public class AFAnalogGaussian : AFAnalogBase
    {
        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public AFAnalogGaussian()
            : base(Interval.IntZP1)
        {
            return;
        }

        //Methods
        /// <overidedoc/>
        public override double Compute(double x)
        {
            x = x.Bound();
            return (Math.Exp(-(x.Power(2)))).Bound();
        }

        /// <overidedoc/>
        public override double ComputeDerivative(double c, double x)
        {
            c = c.Bound();
            x = x.Bound();
            return -2 * x * c;
        }

    }//AFAnalogGaussian

}//Namespace
