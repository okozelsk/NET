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
        /// <param name="n">Number of elements</param>
        /// <param name="k">Length of the combination</param>
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
        /// Computes the occurence probability of specific element in a random combination of length k from n elements
        /// </summary>
        /// <param name="n">Number of elements</param>
        /// <param name="k">Length of the combination</param>
        public static double ComputeProbabilityOfElemInCombination(uint n, uint k)
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

