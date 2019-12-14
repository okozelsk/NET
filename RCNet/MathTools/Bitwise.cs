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
        /// Zero based maximum bit index
        /// </summary>
        private const uint MaxBitPos = 63;

        //Attributes
        /// <summary>
        /// Precomputed cache of bit values
        /// </summary>
        private static readonly ulong[] _cacheOfBitValues;
        
        //Constructor
        /// <summary>
        /// Prepares internal static cache of the bit values
        /// </summary>
        static Bitwise()
        {
            //Preparation of the precomputed bit values cache
            _cacheOfBitValues = new ulong[MaxBitPos + 1];
            _cacheOfBitValues[0] = 1;
            for (uint bitPos = 1; bitPos <= MaxBitPos; bitPos++)
            {
                _cacheOfBitValues[bitPos] = _cacheOfBitValues[bitPos - 1] * 2;
            }
            return;
        }

        //Methods
        /// <summary>
        /// Returns value of the bit on specified position
        /// </summary>
        /// <param name="bitIndex">Zero based position of the bit</param>
        public static ulong BitVal(int bitIndex)
        {
            return _cacheOfBitValues[bitIndex];
        }

        /// <summary>
        /// Returns 0 or 1 depending on whether the bit is set at the specified position in the given number.
        /// </summary>
        /// <param name="number">Source number</param>
        /// <param name="bitIndex">Zero based position of the bit</param>
        public static int GetBit(ulong number, int bitIndex)
        {
            return ((number & _cacheOfBitValues[bitIndex]) > 0) ? 1 : 0;
        }

        /// <summary>
        /// Sets the bit in a given source number at the specified position and returns the result
        /// </summary>
        /// <param name="number">Source number</param>
        /// <param name="bitIndex">Zero based position of the bit</param>
        public static ulong SetBit(ulong number, int bitIndex)
        {
            return number | _cacheOfBitValues[bitIndex];
        }

        /// <summary>
        /// Checks if a bit at the specified position is set within the given number
        /// </summary>
        /// <param name="number">Source number</param>
        /// <param name="bitIndex">Zero based position of the bit</param>
        public static bool IsBitSet(ulong number, int bitIndex)
        {
            return ((number & _cacheOfBitValues[bitIndex]) > 0);
        }

    }//Bitwise

}//Namespace

