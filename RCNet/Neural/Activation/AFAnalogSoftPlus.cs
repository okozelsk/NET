using RCNet.Extensions;
using RCNet.MathTools;
using System;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements the Soft Plus activation function.
    /// </summary>
    [Serializable]
    public class AFAnalogSoftPlus : AFAnalogBase
    {
        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public AFAnalogSoftPlus()
            : base(Interval.IntZPI)
        {
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double x)
        {
            x = x.Bound();
            return (Math.Log(1d + Math.Exp(x))).Bound();
        }

        /// <inheritdoc/>
        public override double ComputeDerivative(double c, double x)
        {
            x.Bound();
            return 1d / (1d + Math.Exp(-x));
        }

    }//AFAnalogSoftPlus

}//Namespace
