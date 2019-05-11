using System;

namespace RCNet.MathTools
{
    /// <summary>
    /// Helper combinatorics functions.
    /// </summary>
    public static class Combinatorics
    {
        /// <summary>
        /// Computes number of combinations of length k from n
        /// </summary>
        /// <param name="n">The n</param>
        /// <param name="k">The k</param>
        public static ulong ComputeNumOfCombinations(uint n, uint k)
        {
            //Basic checks
            if (n < k)
            {
                return 0;
            }
            if (k == 0 || k == n)
            {
                return 1;
            }
            double result = 1d;
            for(uint i = k, j = 0; i >= 1; i--, j++)
            {
                result *= (double)(n - j) / (double)(i);

            }
            //Result
            return (ulong)Math.Round(result, 0);
        }

    }//Combinatorics

}//Namespace

