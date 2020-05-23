using System;

namespace RCNet.MathTools
{
    /// <summary>
    /// Discrete mathematics
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
        /// Prepares static members
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
        /// Returns n!. Product of all positive integers less than or equal to n (n has to be LE Factorial.FactorialMaxArgument).
        /// </summary>
        /// <param name="n">Factorial argument</param>
        public static ulong Factorial(uint n)
        {
            if (n > FactorialMaxArgument)
            {
                throw (new ArgumentException($"Argument is bigger than FactorialMaxArgument {FactorialMaxArgument}"));
            }
            return _factorialResultCache[n];
        }

        /// <summary>
        /// Computes product of integers LE than n and GE than ((n - steps) + 1)
        /// Example: n = 50 and steps = 3 computes 50*49*48
        /// </summary>
        /// <param name="n">Starting number</param>
        /// <param name="steps">Number of backward multiplication steps</param>
        public static ulong PartialReversalFactorial(uint n, uint steps)
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
                    throw (new Exception("Result is too big"));
                }
                ctrlResult = result;
            }
            return result;
        }

        //Methods
        /// <summary>
        /// Computes greatest common divisor
        /// </summary>
        /// <param name="n1">Number 1</param>
        /// <param name="n2">Number 2</param>
        /// <returns>Greatest common divisor</returns>
        public static int GCD(int n1, int n2)
        {
            //Checks
            if (n1 < 1) throw new ArgumentException($"n1={n1} is less than 1");
            if (n2 < 1) throw new ArgumentException($"n2={n2} is less than 1");
            while (n2 != 0)
            {
                int tmp = n1;
                n1 = n2;
                n2 = tmp % n2;
            }
            return n1;
        }

        /// <summary>
        /// Computes lowest common multiplier
        /// </summary>
        /// <param name="n1">Number 1</param>
        /// <param name="n2">Number 2</param>
        /// <param name="gcd">Computed greatest common divisor</param>
        /// <returns>Lowest common multiplier</returns>
        public static int LCM(int n1, int n2, out int gcd)
        {
            gcd = GCD(n1, n2);
            return (n1 * n2) / gcd;
        }

        /// <summary>
        /// Tests if n is prime number.
        /// Implements basic test unefficient for large numbers.
        /// </summary>
        /// <param name="n">Number to be tested</param>
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
