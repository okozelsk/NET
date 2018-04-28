using System;

namespace RCNet.MathTools
{
    /// <summary>
    /// Helper bitwise operations.
    /// Bit position is zero based.
    /// Bit at zero position has value 1, at position 1 has value 2, at position 2 has value 4, etc..
    /// </summary>
    public static class Bitwise
    {
        //Constants
        private const uint MaxBitPos = 63;
        //Attributes
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
        /// <param name="bitNum">Bit position</param>
        /// <returns></returns>
        public static ulong BitVal(uint bitNum)
        {
            return _cacheOfBitValues[bitNum];
        }

        /// <summary>
        /// Sets the bit in a given number at the specified position and returns the result
        /// </summary>
        public static ulong SetBit(ulong number, uint bitPos)
        {
            return number | _cacheOfBitValues[bitPos];
        }

        /// <summary>
        /// Checks if a bit at the specified position is set within the given number
        /// </summary>
        public static bool IsBitSet(ulong number, uint bitPos)
        {
            return ((number & _cacheOfBitValues[bitPos]) > 0);
        }

        /// <summary>
        /// Returns 0 or 1 depending on whether the bit is set at the specified position in the given number.
        /// </summary>
        public static ulong GetBit(ulong number, uint bitPos)
        {
            return IsBitSet(number, bitPos) ? (ulong)1 : (ulong)0;
        }

    }//Bitwise

}//Namespace

