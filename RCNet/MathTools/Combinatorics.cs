using System;

namespace RCNet.MathTools
{
    /// <summary>
    /// Implements helper combinatorics functions.
    /// </summary>
    public static class Combinatorics
    {
        /// <summary>
        /// Computes number of combinations.
        /// </summary>
        /// <param name="n">The total number of elements.</param>
        /// <param name="k">The length of the combination.</param>
        public static ulong ComputeNumOfCombinations(uint n, uint k)
        {
            //Basic checks
            if (k == 0 || k == n)
            {
                return 1;
            }
            else if (k > n)
            {
                return 0;
            }
            //Computation
            double result = 1d;
            for (uint i = k, j = 0; i >= 1; i--, j++)
            {
                result *= (double)(n - j) / (double)(i);

            }
            //Result
            return (ulong)Math.Round(result, 0);
        }

        /// <summary>
        /// Computes the occurence probability of an element in a combination.
        /// </summary>
        /// <param name="n">The total number of elements.</param>
        /// <param name="k">The length of the combination.</param>
        public static double ComputeElemInCombinationProbability(uint n, uint k)
        {
            //Basic checks
            if (k == 0 || n == 0 || k > n)
            {
                return 0d;
            }
            else if (k == n)
            {
                return 1d;
            }
            //Computation
            double result = 0d;
            for (int i = 0; i < k; i++)
            {
                result += 1d / (n - i);

            }
            //Result
            return result;
        }

    }//Combinatorics

}//Namespace

