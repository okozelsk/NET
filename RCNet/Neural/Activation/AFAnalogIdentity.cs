using System;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements the AFAnalogIdentity activation function (aka Linear)
    /// </summary>
    [Serializable]
    public class AFAnalogIdentity : AFAnalogBase
    {
        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public AFAnalogIdentity()
            : base(Interval.IntNIPI)
        {
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double x)
        {
            //The same value
            return x.Bound();
        }

        /// <inheritdoc/>
        public override double ComputeDerivative(double c, double x = double.NaN)
        {
            //Allways 1
            return 1d;
        }

    }//AFAnalogIdentity

}//Namespace

