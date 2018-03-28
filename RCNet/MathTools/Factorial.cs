using System;

namespace RCNet.MathTools
{
    /// <summary>
    /// Implements the cached factorial computation.
    /// </summary>
    public static class Factorial
    {
        //Constants
        /// <summary>
        /// The highest value that can be used as the argument for factorial computation
        /// </summary>
        public const uint FactorialMaxInputValue = 20;
        //Attributes
        private static readonly ulong[] _resultCache;

        //Constructor
        /// <summary>
        /// Prepares the internal static result cache
        /// </summary>
        static Factorial()
        {
            //Preparation of the precomputed factorial cache
            _resultCache = new ulong[FactorialMaxInputValue + 1];
            _resultCache[0] = 1;
            for(uint n = 1; n <= FactorialMaxInputValue; n++)
            {
                _resultCache[n] = _resultCache[n - 1] * n;
            }
            return;
        }

        /// <summary>
        /// Returns n!. Product of all positive integers less than or equal to n (n has to be LE to Factorial.MAX_BASE).
        /// </summary>
        /// <param name="n"></param>
        public static ulong Get(uint n)
        {
            if(n > FactorialMaxInputValue)
            {
                throw (new ArgumentException("Argument is bigger than Factorial.MAX_BASE"));
            }
            return _resultCache[n];
        }

        /// <summary>
        /// Computes product of integers LE than n and GE than ((n - count) + 1)
        /// Example: n = 50 and count = 3 computes 50*49*48
        /// </summary>
        /// <param name="n"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static ulong PartialReversal(uint n, uint count)
        {
            if (count < 1) return 0;
            if (n == 0) return 1;
            if (count > n) count = n;
            ulong result = n;
            double ctrlResult = n;
            --n;
            for (count = count - 1; count > 0; count--, n--)
            {
                result *= n;
                ctrlResult *= n;
                if(Math.Floor(ctrlResult) > result)
                {
                    throw (new Exception("Result is too big"));
                }
                ctrlResult = result;
            }
            return result;
        }

    }//Factorial

} //Namespace

