using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.MathTools
{
    /// <summary>
    /// Helper bitwise operations.
    /// Bit positions are zero based.
    /// Zero bit position has value 1, position 1 has value 2, position 2 has value 4, etc..
    /// </summary>
    public static class Bitwise
    {
        //Constants
        private const uint MaxBitPos = 63;
        //Attributes
        private static readonly ulong[] _bitValuesCache;
        
        //Constructor
        static Bitwise()
        {
            //Preparation of the precomputed bit values cache
            _bitValuesCache = new ulong[MaxBitPos + 1];
            _bitValuesCache[0] = 1;
            for (uint bitPos = 1; bitPos <= MaxBitPos; bitPos++)
            {
                _bitValuesCache[bitPos] = _bitValuesCache[bitPos - 1] * 2;
            }
            return;
        }

        //Methods
        public static ulong SetBit(ulong number, uint bitPos)
        {
            return number | _bitValuesCache[bitPos];
        }

        public static bool IsBitSet(ulong number, uint bitPos)
        {
            return ((number & _bitValuesCache[bitPos]) > 0);
        }

        public static ulong GetBitValue(ulong number, uint bitPos)
        {
            return IsBitSet(number, bitPos) ? (ulong)1 : (ulong)0;
        }

        public static int GetBitPos(ulong bitValue)
        {
            for(int i = 0; i < _bitValuesCache.Length; i++)
            {
                if(_bitValuesCache[i] == bitValue)
                {
                    return i;
                }
                else if(_bitValuesCache[i] > bitValue)
                {
                    break;
                }
            }
            return -1;
        }

        public static int GetGEBitPos(ulong value)
        {
            for (int i = 0; i < _bitValuesCache.Length; i++)
            {
                if (_bitValuesCache[i] >= value)
                {
                    return i;
                }
            }
            return -1;
        }

        public static int GetHighestBitPos(ulong value)
        {
            for (int i = _bitValuesCache.Length; i >= 0; i--)
            {
                if (IsBitSet(value, (uint)i))
                {
                    return i;
                }
            }
            return -1;
        }

        public static ulong GetHighestBitValue(ulong value)
        {
            int i = GetHighestBitPos(value);
            return (i > 0) ? _bitValuesCache[i] : (ulong)0;
        }

        public static int GetLowestBitPos(ulong value)
        {
            for (int i = 0; i < _bitValuesCache.Length; i++)
            {
                if (IsBitSet(value, (uint)i))
                {
                    return i;
                }
            }
            return -1;
        }

        public static ulong GetLowestBitValue(ulong value)
        {
            int i = GetLowestBitPos(value);
            return (i > 0) ? _bitValuesCache[i] : (ulong)0;
        }

    }//Bitwise
}//Namespace
