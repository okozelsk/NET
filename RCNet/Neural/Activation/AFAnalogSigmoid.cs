using RCNet.Extensions;
using RCNet.MathTools;
using System;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements the Sigmoid activation function
    /// </summary>
    [Serializable]
    public class AFAnalogSigmoid : AFAnalogBase
    {
        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public AFAnalogSigmoid()
            : base(Interval.IntZP1)
        {
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double x)
        {
            x = x.Bound();
            return 1d / (1d + Math.Exp(-x));
        }

        /// <inheritdoc/>
        public override double ComputeDerivative(double c, double x)
        {
            c = c.Bound();
            return c * (1d - c);
        }

    }//AFAnalogSigmoid

}//Namespace
