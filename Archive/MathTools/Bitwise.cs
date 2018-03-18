using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OKOSW.MathTools
{
    /// <summary>
    /// Useful bitwise operations.
    /// Bit positions are zero based.
    /// Zero bit position has value 1, position 1 has value 2, position 2 has value 4, etc..
    /// </summary>
    public static class Bitwise
    {
        //Constants
        private const uint MAX_BIT_POS = 63;
        //Attributes
        private static readonly ulong[] m_bitValuesCache;
        
        //Constructor
        static Bitwise()
        {
            //Preparation of the precomputed bit values cache
            m_bitValuesCache = new ulong[MAX_BIT_POS + 1];
            m_bitValuesCache[0] = 1;
            for (uint bitPos = 1; bitPos <= MAX_BIT_POS; bitPos++)
            {
                m_bitValuesCache[bitPos] = m_bitValuesCache[bitPos - 1] * 2;
            }
            return;
        }

        //Methods
        public static ulong SetBit(ulong number, uint bitPos)
        {
            return number | m_bitValuesCache[bitPos];
        }

        public static bool IsBitSet(ulong number, uint bitPos)
        {
            return ((number & m_bitValuesCache[bitPos]) > 0);
        }

        public static ulong GetBitValue(ulong number, uint bitPos)
        {
            return IsBitSet(number, bitPos) ? (ulong)1 : (ulong)0;
        }

        public static int GetBitPos(ulong bitValue)
        {
            for(int i = 0; i < m_bitValuesCache.Length; i++)
            {
                if(m_bitValuesCache[i] == bitValue)
                {
                    return i;
                }
                else if(m_bitValuesCache[i] > bitValue)
                {
                    break;
                }
            }
            return -1;
        }

        public static int GetGEBitPos(ulong value)
        {
            for (int i = 0; i < m_bitValuesCache.Length; i++)
            {
                if (m_bitValuesCache[i] >= value)
                {
                    return i;
                }
            }
            return -1;
        }

        public static int GetHighestBitPos(ulong value)
        {
            for (int i = m_bitValuesCache.Length; i >= 0; i--)
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
            return (i > 0) ? m_bitValuesCache[i] : (ulong)0;
        }

        public static int GetLowestBitPos(ulong value)
        {
            for (int i = 0; i < m_bitValuesCache.Length; i++)
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
            return (i > 0) ? m_bitValuesCache[i] : (ulong)0;
        }

    }//Bitwise
}//Namespace
