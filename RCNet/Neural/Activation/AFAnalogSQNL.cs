using RCNet.MathTools;
using System;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements the Square Nonlinearity activation function.
    /// </summary>
    [Serializable]
    public class AFAnalogSQNL : AFAnalogBase
    {
        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public AFAnalogSQNL()
            : base(Interval.IntN1P1)
        {
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double x)
        {
            if (x > 2d)
            {
                return 1d;
            }
            else if (x >= 0)
            {
                return x - (x * x) / 4d;
            }
            else if (x >= -2d)
            {
                return x + (x * x) / 4d;
            }
            else
            {
                //x < -2d
                return -1d;
            }
        }

        /// <inheritdoc/>
        public override double ComputeDerivative(double c, double x = double.NaN)
        {
            //Faster than 1d + Math.Abs(x/2d);
            return x < 0 ? 1d - x / 2d : 1d + x / 2d;
        }

    }//AFAnalogSQNL

}//Namespace
