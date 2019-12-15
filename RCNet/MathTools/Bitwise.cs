using System;

namespace RCNet.MathTools
{
    /// <summary>
    /// Helper bitwise operations.
    /// </summary>
    public static class Bitwise
    {
        //Constants
        /// <summary>
        /// Zero based index of the highest allowed bit
        /// </summary>
        private const int BitMaxIndex = sizeof(ulong) * 8 - 1;

        //Attributes
        /// <summary>
        /// Precomputed cache of bit values
        /// </summary>
        private static readonly ulong[] _bitValuesCache;

        //Constructor
        /// <summary>
        /// Initializes internal static cache containing precomputed bit values
        /// </summary>
        static Bitwise()
        {
            _bitValuesCache = new ulong[BitMaxIndex + 1];
            _bitValuesCache[0] = 1;
            for (int bitPos = 1; bitPos <= BitMaxIndex; bitPos++)
            {
                _bitValuesCache[bitPos] = _bitValuesCache[bitPos - 1] * 2ul;
            }
            return;
        }

        //Methods
        /// <summary>
        /// Returns value of bit at specified position
        /// </summary>
        /// <param name="bitIndex">Zero based position of the bit (lowest bit has position 0)</param>
        public static ulong BitVal(int bitIndex)
        {
            return _bitValuesCache[bitIndex];
        }

        /// <summary>
        /// Returns 0 or 1 depending on whether the bit is set at the specified position in the given number.
        /// </summary>
        /// <param name="number">Source number</param>
        /// <param name="bitIndex">Zero based position of the bit (lowest bit has position 0)</param>
        public static int GetBit(ulong number, int bitIndex)
        {
            return ((number & _bitValuesCache[bitIndex]) > 0) ? 1 : 0;
        }

        /// <summary>
        /// Sets the bit in a given source number at the specified position and returns the result
        /// </summary>
        /// <param name="number">Source number</param>
        /// <param name="bitIndex">Zero based position of the bit (lowest bit has position 0)</param>
        public static ulong SetBit(ulong number, int bitIndex)
        {
            return number | _bitValuesCache[bitIndex];
        }

        /// <summary>
        /// Checks if a bit at the specified position is set within the given number
        /// </summary>
        /// <param name="number">Source number</param>
        /// <param name="bitIndex">Zero based position of the bit (lowest bit has position 0)</param>
        public static bool IsBitSet(ulong number, int bitIndex)
        {
            return ((number & _bitValuesCache[bitIndex]) > 0);
        }

    }//Bitwise

}//Namespace

