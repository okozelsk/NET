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
            if (n < k || k > Factorial.FactorialMaxInputValue)
            {
                return 0;
            }
            if(k == 0 || k == n)
            {
                return 1;
            }
            //Result
            return Factorial.PartialReversal(n, k) / Factorial.Get(k);
        }

    }//Combinatorics

}//Namespace

