using RCNet.Extensions;
using RCNet.MathTools;
using System;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements the Soft Exponential activation function.
    /// </summary>
    [Serializable]
    public class AFAnalogSoftExponential : AFAnalogBase
    {
        /// <summary>
        /// Alpha
        /// </summary>
        public double Alpha { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="alpha">The alpha.</param>
        public AFAnalogSoftExponential(double alpha)
            : base(Interval.IntNIPI)
        {
            Alpha = alpha.Bound();
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double x)
        {
            x = x.Bound();
            if (Alpha < 0)
            {
                return (-(Math.Log(1 - Alpha * (x + Alpha)) / Alpha)).Bound();
            }
            else if (Alpha == 0)
            {
                return x;
            }
            else
            {
                return ((Math.Exp(Alpha * x) - 1) / Alpha + Alpha).Bound();
            }
        }

        /// <inheritdoc/>
        public override double ComputeDerivative(double c, double x)
        {
            x = x.Bound();
            if (Alpha < 0)
            {
                return 1d / (1d - Alpha * (Alpha + x));
            }
            else
            {
                return Math.Exp(Alpha * x);
            }
        }

    }//AFAnalogSoftExponential

}//Namespace

