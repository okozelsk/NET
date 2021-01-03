using RCNet.Extensions;
using System;

namespace RCNet.MathTools
{
    /// <summary>
    /// Implements the computation of the Euclidean distance.
    /// </summary>
    public static class EuclideanDistance
    {
        /// <summary>
        /// Computes the Euclidean distance.
        /// </summary>
        /// <param name="coordinates1">The coordinates.</param>
        /// <param name="coordinates2">The coordinates.</param>
        /// <returns>The Euclidean distance.</returns>
        public static double Compute(int[] coordinates1, int[] coordinates2)
        {
            double sum = 0;
            for (int i = 0; i < coordinates1.Length; i++)
            {
                sum += ((double)(coordinates1[i] - coordinates2[i])).Power(2);
            }
            return Math.Sqrt(sum);
        }

        /// <summary>
        /// Computes the Euclidean distance.
        /// </summary>
        /// <param name="coordinates1">The coordinates.</param>
        /// <param name="coordinates2">The coordinates.</param>
        /// <returns>The Euclidean distance.</returns>
        public static double Compute(double[] coordinates1, double[] coordinates2)
        {
            double sum = 0;
            for (int i = 0; i < coordinates1.Length; i++)
            {
                sum += (coordinates1[i] - coordinates2[i]).Power(2);
            }
            return Math.Sqrt(sum);
        }

    }//EuclideanDistance

}//Namespace
