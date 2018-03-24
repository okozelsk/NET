using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.MathTools
{
    public static class Combinatorics
    {
        /// <summary>
        /// Computes number of combinations of length k from n
        /// </summary>
        /// <param name="n"></param>
        /// <param name="k"></param>
        public static ulong GetCombinationsCount(uint n, uint k)
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
