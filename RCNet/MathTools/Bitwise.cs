using RCNet.Extensions;
using System;
using System.Globalization;

namespace RCNet.MathTools
{
    /// <summary>
    /// Helper bitwise operations.
    /// </summary>
    public static class Bitwise
    {
        //Constants
        /// <summary>
        /// Maximum number of bits within largest integer variable type
        /// </summary>
        private const int MaxBits = sizeof(ulong) * 8;

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
        /// Sets the bit in a given number at the specified position and returns the result
        /// </summary>
        /// <param name="number">Source number</param>
        /// <param name="bitIndex">Zero based position of the bit (lowest bit has position 0)</param>
        /// <param name="bit">Bit value to be set (true=1, false=0)</param>
        public static ulong SetBit(ulong number, int bitIndex, bool bit)
        {
            if(bit)
            {
                return number | _bitValuesCache[bitIndex];
            }
            else
            {
                return (number | _bitValuesCache[bitIndex]) - _bitValuesCache[bitIndex];
            }
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

        //Inner classes
        /// <summary>
        /// Implements efficient bits buffer working as a moving window
        /// </summary>
        [Serializable]
        public class Window
        {
            //Attribute properties
            /// <summary>
            /// Window capacity
            /// </summary>
            public int Capacity { get; }

            /// <summary>
            /// Used capacity
            /// </summary>
            public int BufferedHistLength { get; private set; }

            /// <summary>
            /// Total number of set bits within the window
            /// </summary>
            public int NumOfSetBits { get; private set; }

            //Attributes
            private readonly int _lastSegHighestBitIndex;
            private readonly ulong[] _buffSegments;
            private readonly int[] _segBitCounter;

            /// <summary>
            /// Creates an initialized instance
            /// </summary>
            /// <param name="capacity">Maximum number of holded bits</param>
            public Window(int capacity)
            {
                //Check
                if(capacity <= 0)
                {
                    throw new ArgumentException($"Invalid capacity {capacity.ToString()}. Capacity has to be GT 0.", "capacity");
                }
                Capacity = capacity;
                int numOfSegments = Capacity / MaxBits + ((Capacity % MaxBits) > 0 ? 1 : 0);
                _buffSegments = new ulong[numOfSegments];
                _segBitCounter = new int[numOfSegments];
                _lastSegHighestBitIndex = (Capacity - ((numOfSegments - 1) * MaxBits)) - 1;
                Reset();
            }

            //Properties
            /// <summary>
            /// Indicates fully filled window
            /// </summary>
            public bool Full { get { return BufferedHistLength == Capacity; } }

            //Methods
            /// <summary>
            /// Resets bit window to an initial state
            /// </summary>
            public void Reset()
            {
                _buffSegments.Populate(0ul);
                _segBitCounter.Populate(0);
                NumOfSetBits = 0;
                BufferedHistLength = 0;
                return;
            }

            /// <summary>
            /// Adds next bit value into the window content
            /// </summary>
            /// <param name="bit">Specifies if the bit to be added is set or not</param>
            public void AddNext(bool bit)
            {
                int lowestBitVal = bit ? 1 : 0;
                NumOfSetBits += lowestBitVal;
                for (int i = 0; i < _buffSegments.Length; i++)
                {
                    if (i < _buffSegments.Length - 1)
                    {
                        int segHighestBitVal = Bitwise.GetBit(_buffSegments[i], BitMaxIndex);
                        _segBitCounter[i] -= segHighestBitVal;
                        _buffSegments[i] <<= 1;
                        _buffSegments[i] += (ulong)lowestBitVal;
                        _segBitCounter[i] += lowestBitVal;
                        lowestBitVal = segHighestBitVal;
                    }
                    else
                    {
                        int segHighestBitVal = Bitwise.GetBit(_buffSegments[i], _lastSegHighestBitIndex);
                        _segBitCounter[i] -= segHighestBitVal;
                        NumOfSetBits -= segHighestBitVal;
                        _buffSegments[i] = SetBit(_buffSegments[i], _lastSegHighestBitIndex, false);
                        _buffSegments[i] <<= 1;
                        _buffSegments[i] += (ulong)lowestBitVal;
                        _segBitCounter[i] += lowestBitVal;
                    }
                }
                if(BufferedHistLength < Capacity)
                {
                    ++BufferedHistLength;
                }
                return;
            }

            /// <summary>
            /// Returns num of set bits within the specified recent history of the window
            /// </summary>
            /// <param name="recentHistLength">Length of the recent history (-1 means the whole available history)</param>
            public int GetNumOfSetBits(int recentHistLength = -1)
            {
                if(recentHistLength > Capacity || recentHistLength < -1)
                {
                    throw new ArgumentException($"Invalid buffPartSize {recentHistLength}.", "buffPartSize");
                }
                else if(recentHistLength == 0)
                {
                    return 0;
                }
                else if (recentHistLength == -1)
                {
                    return NumOfSetBits;
                }
                else
                {
                    int numOfSegments = recentHistLength / MaxBits + ((recentHistLength % MaxBits) > 0 ? 1 : 0);
                    int lastSegHighestBitIndex = (recentHistLength - ((numOfSegments - 1) * MaxBits)) - 1;
                    int counter = 0;
                    for(int i = 0; i < numOfSegments - 1; i++)
                    {
                        counter += _segBitCounter[i];
                    }
                    for(int i = 0; i <= lastSegHighestBitIndex; i++)
                    {
                        counter += Bitwise.GetBit(_buffSegments[numOfSegments - 1], i);
                    }
                    return counter;
                }
            }

            /// <summary>
            /// Returns bit value at the specified index within the window
            /// </summary>
            /// <param name="bitIndex">Zero based index of the bit (0 is the recent bit)</param>
            public int GetBit(int bitIndex)
            {
                if ((bitIndex + 1) > Capacity || bitIndex < 0)
                {
                    throw new ArgumentException($"Invalid bitIndex {bitIndex}.", "bitIndex");
                }
                int recentHistLength = bitIndex + 1;
                int numOfSegments = recentHistLength / MaxBits + ((recentHistLength % MaxBits) > 0 ? 1 : 0);
                int lastSegHighestBitIndex = (recentHistLength - ((numOfSegments - 1) * MaxBits)) - 1;
                return Bitwise.GetBit(_buffSegments[numOfSegments - 1], lastSegHighestBitIndex);
            }

            /// <summary>
            /// Computes fading sum of bits stored in the recent history.
            /// </summary>
            /// <param name="fadingStrength">Fading strength between 0-1</param>
            /// <param name="recentHistLength">Length of the recent history (-1 means the whole available history)</param>
            public double GetFadingSum(double fadingStrength, int recentHistLength = -1)
            {
                if(fadingStrength < 0d || fadingStrength >= 1d)
                {
                    throw new ArgumentException($"Invalid fadingStrength {fadingStrength.ToString(CultureInfo.InvariantCulture)}.", "fadingStrength");
                }
                if (recentHistLength > Capacity || recentHistLength < -1)
                {
                    throw new ArgumentException($"Invalid buffPartSize {recentHistLength}.", "buffPartSize");
                }
                else if (recentHistLength == 0)
                {
                    return 0;
                }
                else if (recentHistLength == -1)
                {
                    recentHistLength = Capacity;
                }
                double fadingCoeff = 1d - fadingStrength;
                double fadingSum = 0;
                for(int index = recentHistLength - 1; index >= 0; index--)
                {
                    fadingSum *= fadingCoeff;
                    fadingSum += GetBit(index);
                }
                return fadingSum;
            }

            /// <summary>
            /// Returns the sequence of bits.
            /// </summary>
            /// <param name="index">Zero based index within the window (0 is the recent bit)</param>
            /// <param name="length">Sequence length (1-64)</param>
            /// <param name="reverseOrder">Specifies if to reverse order of the sequence bits</param>
            public ulong GetBits(int index, int length, bool reverseOrder = true)
            {
                if ((index + 1) > Capacity || index < 0)
                {
                    throw new ArgumentException($"Invalid index {index}.", "index");
                }
                if (length > MaxBits || length < 1 || length > (index + 1))
                {
                    throw new ArgumentException($"Invalid length {length}.", "length");
                }
                ulong sequence = 0ul;
                if(reverseOrder)
                {
                    for(int i = index - (length -1); i <= index; i++)
                    {
                        sequence <<= 1;
                        sequence += (ulong)GetBit(i);
                    }
                }
                else
                {
                    for (int i = index; i >= index - (length - 1); i--)
                    {
                        sequence <<= 1;
                        sequence += (ulong)GetBit(i);
                    }
                }
                return sequence;
            }

            /// <summary>
            /// Returns the sequence of recent bits.
            /// </summary>
            /// <param name="length">Sequence length (1-64 and LE to capacity)</param>
            /// <param name="reverseOrder">Specifies if to reverse order of the sequence bits</param>
            public ulong GetBits(int length, bool reverseOrder = true)
            {
                if (length > MaxBits || length > Capacity || length < 1)
                {
                    throw new ArgumentException($"Invalid length {length}.", "length");
                }
                return GetBits(length - 1, length, reverseOrder);
            }

        }//Window

    }//Bitwise

}//Namespace

