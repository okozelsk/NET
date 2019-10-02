using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;

namespace RCNet.MathTools
{
    /// <summary>
    /// Implements computation of the Euclidean distance
    /// </summary>
    public static class EuclideanDistance
    {
        /// <summary>
        /// Computes the Euclidean distance
        /// </summary>
        /// <param name="sCoordinates">Source coordinates.</param>
        /// <param name="tCoordinates">Target coordinates.</param>
        public static double Compute(int[] sCoordinates, int[] tCoordinates)
        {
            double sum = 0;
            for (int i = 0; i < sCoordinates.Length; i++)
            {
                sum += ((double)(sCoordinates[i] - tCoordinates[i])).Power(2);
            }
            return Math.Sqrt(sum);
        }

        /// <summary>
        /// Computes the Euclidean distance
        /// </summary>
        /// <param name="sCoordinates">Source coordinates.</param>
        /// <param name="tCoordinates">Target coordinates.</param>
        public static double Compute(double[] sCoordinates, double[] tCoordinates)
        {
            double sum = 0;
            for (int i = 0; i < sCoordinates.Length; i++)
            {
                sum += (sCoordinates[i] - tCoordinates[i]).Power(2);
            }
            return Math.Sqrt(sum);
        }
    }//EuclideanDistance
}
