using System;
using System.Runtime.CompilerServices;

namespace RCNet.Extensions
{
    /// <summary>
    /// Useful extensions of Double class.
    /// </summary>
    public static class DoubleExtensions
    {
        //Constants
        /// <summary>
        /// Reasonable min non-negative value
        /// </summary>
        public const double ReasonableAbsMin = 1e-20;

        /// <summary>
        /// Reasonable max non-negative value
        /// </summary>
        public const double ReasonableAbsMax = 1e20;

        /// <summary>
        /// Checks if this is computable double value.
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
        /// Bounds given value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Bound(this double x, double min = -ReasonableAbsMax, double max = ReasonableAbsMax)
        {
            if(x < min)
            {
                return min;
            }
            if(x > max)
            {
                return max;
            }
            return x;
        }

        /// <summary>
        /// Computes the power (faster than Math.Pow)
        /// </summary>
        /// <param name="exponent">uint exponent</param>
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
