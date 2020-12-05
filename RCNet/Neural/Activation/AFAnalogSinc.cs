using RCNet.Extensions;
using RCNet.MathTools;
using System;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements the Sinc activation function
    /// </summary>
    [Serializable]
    public class AFAnalogSinc : AFAnalogBase
    {
        //Static members
        private static readonly Interval _sincOutputRange = new Interval(-0.217234, 1, false, true);

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public AFAnalogSinc()
            : base(_sincOutputRange)
        {
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double x)
        {
            x = x.Bound();
            return (x == 0 ? 1d : Math.Sin(x) / x).Bound(_outputRange.Min, _outputRange.Max);
        }

        /// <inheritdoc/>
        public override double ComputeDerivative(double c, double x)
        {
            x = x.Bound();
            if (x == 0)
            {
                return 0;
            }
            else
            {
                return (Math.Cos(x) / x) / (Math.Sin(x) / x.Power(2));
            }
        }

    }//AFAnalogSinc

}//Namespace
