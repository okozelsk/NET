using System;

namespace RCNet.MathTools
{
    /// <summary>
    /// Implements some helper functions from the discrete math.
    /// </summary>
    public static class Discrete
    {
        //Constants
        /// <summary>
        /// The highest value of argument that can be used for factorial computation
        /// </summary>
        public const uint FactorialMaxArgument = 20;

        //Static attributes
        private static readonly ulong[] _factorialResultCache;

        //Static constructor
        /// <summary>
        /// Static constructor. Prepares the factorial cache.
        /// </summary>
        static Discrete()
        {
            //Factorial result cache
            _factorialResultCache = new ulong[FactorialMaxArgument + 1];
            _factorialResultCache[0] = 1;
            for (uint n = 1; n <= FactorialMaxArgument; n++)
            {
                _factorialResultCache[n] = _factorialResultCache[n - 1] * n;
            }
            return;
        }

        /// <summary>
        /// Computes the factorial.
        /// </summary>
        /// <param name="n">An argument (it has to be LE to FactorialMaxArgument).</param>
        public static ulong Factorial(uint n)
        {
            if (n > FactorialMaxArgument)
            {
                throw (new ArgumentException($"Argument is bigger than the FactorialMaxArgument {FactorialMaxArgument}"));
            }
            return _factorialResultCache[n];
        }

        /// <summary>
        /// Computes the product of integers LE to n and GE to ((n - steps) + 1).
        /// </summary>
        /// <remarks>
        /// For n = 50 and steps = 3, it computes 50 * 49 * 48 = 117600.
        /// </remarks>
        /// <param name="n">The starting integer.</param>
        /// <param name="steps">The number of backward multiplication steps.</param>
        public static ulong PartialReversalProduct(uint n, uint steps)
        {
            if (steps < 1) return 0;
            if (n == 0) return 1;
            if (steps > n) steps = n;
            ulong result = n;
            double ctrlResult = n;
            --n;
            for (steps -= 1; steps > 0; steps--, n--)
            {
                result *= n;
                ctrlResult *= n;
                if (Math.Floor(ctrlResult) > result)
                {
                    throw (new InvalidOperationException("Result is too big."));
                }
                ctrlResult = result;
            }
            return result;
        }

        //Methods
        /// <summary>
        /// Computes the greatest common divisor.
        /// </summary>
        /// <param name="n1">An integer n1.</param>
        /// <param name="n2">An integer n2.</param>
        /// <returns>The greatest common divisor.</returns>
        public static int GCD(int n1, int n2)
        {
            //Checks
            if (n1 < 1) throw new ArgumentException($"n1={n1} is less than 1", "n1");
            if (n2 < 1) throw new ArgumentException($"n2={n2} is less than 1", "n2");
            while (n2 != 0)
            {
                int tmp = n1;
                n1 = n2;
                n2 = tmp % n2;
            }
            return n1;
        }

        /// <summary>
        /// Computes the lowest common multiplier.
        /// </summary>
        /// <param name="n1">An integer n1.</param>
        /// <param name="n2">An integer n2.</param>
        /// <param name="gcd">The computed greatest common divisor.</param>
        /// <returns>The lowest common multiplier.</returns>
        public static int LCM(int n1, int n2, out int gcd)
        {
            gcd = GCD(n1, n2);
            return (n1 * n2) / gcd;
        }

        /// <summary>
        /// Tests whether the integer is the prime number.
        /// </summary>
        /// <remarks>
        /// Implements only a basic test that is inefficient for large numbers.
        /// </remarks>
        /// <param name="n">An integer to be tested.</param>
        public static bool IsPrime(int n)
        {
            if (n <= 1) return false;
            if (n == 2) return true;
            if (n % 2 == 0) return false;
            int maxTest = (int)Math.Sqrt(n);
            for (int i = 3; i <= maxTest; i += 2)
            {
                if (n % i == 0) return false;
            }
            return true;
        }


    }//Discrete
}//Namespace
