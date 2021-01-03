using System;
using System.Runtime.CompilerServices;

namespace RCNet.Extensions
{
    /// <summary>
    /// Implements useful extensions of a double.
    /// </summary>
    public static class DoubleExtensions
    {
        //Constants
        /// <summary>
        /// The reasonable non-negative min value.
        /// </summary>
        public const double ReasonableAbsMin = 1e-20;

        /// <summary>
        /// The reasonable non-negative max value.
        /// </summary>
        public const double ReasonableAbsMax = 1e20;

        /// <summary>
        /// Checks whether this value is a valid value for computations.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValid(this double x)
        {
            if (Double.IsInfinity(x) || Double.IsNaN(x))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns this value bounded in specified min and max value.
        /// </summary>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        /// <param name="x"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Bound(this double x, double min = -ReasonableAbsMax, double max = ReasonableAbsMax)
        {
            if (x < min)
            {
                return min;
            }
            else if (x > max)
            {
                return max;
            }
            else
            {
                return x;
            }
        }

        /// <summary>
        /// Computes the power (faster than Math.Pow).
        /// </summary>
        /// <param name="exponent">An uint exponent.</param>
        /// <param name="x"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Power(this double x, uint exponent)
        {
            //Faster than Math.Pow
            switch (exponent)
            {
                case 2: return x * x;
                case 1: return x;
                case 0: return 1;
                default:
                    {
                        double result = x;
                        for (uint level = 2; level <= exponent; level++)
                        {
                            result *= result;
                        }
                        return result;
                    }
            }
        }

    }//DoubleExtensions

}//Namespace
