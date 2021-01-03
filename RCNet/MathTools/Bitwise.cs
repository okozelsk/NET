using RCNet.Extensions;
using System;
using System.Globalization;

namespace RCNet.MathTools
{
    /// <summary>
    /// Implements the set of bitwise operations.
    /// </summary>
    public static class Bitwise
    {
        //Constants
        /// <summary>
        /// The maximum number of bits within the largest integer variable type.
        /// </summary>
        public const int MaxBits = sizeof(ulong) * 8;

        /// <summary>
        /// The zero-based index of the highest allowed bit.
        /// </summary>
        public const int BitMaxIndex = sizeof(ulong) * 8 - 1;

        //Attributes
        /// <summary>
        /// The precomputed cache of the bit values.
        /// </summary>
        private static readonly ulong[] _bitValuesCache;

        //Constructor
        /// <summary>
        /// The static constructor. Initializes the internal static cache.
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
        /// Gets a value of the bit at the specified position.
        /// </summary>
        /// <param name="bitIndex">The zero-based position of the bit (the lowest bit has position 0).</param>
        public static ulong BitVal(int bitIndex)
        {
            return _bitValuesCache[bitIndex];
        }

        /// <summary>
        /// Gets the bit (0/1) at the specified position.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="bitIndex">The zero-based position of the bit (the lowest bit has position 0).</param>
        public static int GetBit(ulong number, int bitIndex)
        {
            return ((number & _bitValuesCache[bitIndex]) > 0) ? 1 : 0;
        }

        /// <summary>
        /// Sets the bit at the specified position.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="bitIndex">The zero-based position of the bit (the lowest bit has position 0).</param>
        /// <param name="bit">The bit to be set (true=1, false=0).</param>
        /// <returns>The resulting number.</returns>
        public static ulong SetBit(ulong number, int bitIndex, bool bit)
        {
            if (bit)
            {
                return number | _bitValuesCache[bitIndex];
            }
            else
            {
                return (number | _bitValuesCache[bitIndex]) - _bitValuesCache[bitIndex];
            }
        }

        /// <summary>
        /// Checks if a bit at the specified position is set.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="bitIndex">The zero-based position of the bit (the lowest bit has position 0).</param>
        public static bool IsBitSet(ulong number, int bitIndex)
        {
            return ((number & _bitValuesCache[bitIndex]) > 0);
        }

        //Inner classes
        /// <summary>
        /// Implements an efficient buffer of bits. Works as a moving window.
        /// </summary>
        [Serializable]
        public class Window
        {
            //Attribute properties
            /// <summary>
            /// The capacity.
            /// </summary>
            public int Capacity { get; }

            /// <summary>
            /// The used capacity.
            /// </summary>
            public int UsedCapacity { get; private set; }

            /// <summary>
            /// The total number of ones within the window.
            /// </summary>
            public int NumOfOnes { get; private set; }

            //Attributes
            private readonly int _lastSegHighestBitIndex;
            private readonly ulong[] _buffSegments;
            private readonly int[] _segBitCounter;

            /// <summary>
            /// Creates an initialized instance.
            /// </summary>
            /// <param name="capacity">The maximum capacity.</param>
            public Window(int capacity)
            {
                //Check
                if (capacity <= 0)
                {
                    throw new ArgumentException($"Invalid capacity {capacity}. Capacity has to be GT 0.", "capacity");
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
            /// Indicates the fully occupied window.
            /// </summary>
            public bool Full { get { return UsedCapacity == Capacity; } }

            //Methods
            /// <summary>
            /// Resets the window.
            /// </summary>
            public void Reset()
            {
                _buffSegments.Populate(0ul);
                _segBitCounter.Populate(0);
                NumOfOnes = 0;
                UsedCapacity = 0;
                return;
            }

            /// <summary>
            /// Adds the next bit into the window.
            /// </summary>
            /// <param name="bit">Specifies the bit value (true=1, false=0).</param>
            public void AddNext(bool bit)
            {
                int lowestBitVal = bit ? 1 : 0;
                NumOfOnes += lowestBitVal;
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
                        NumOfOnes -= segHighestBitVal;
                        _buffSegments[i] = SetBit(_buffSegments[i], _lastSegHighestBitIndex, false);
                        _buffSegments[i] <<= 1;
                        _buffSegments[i] += (ulong)lowestBitVal;
                        _segBitCounter[i] += lowestBitVal;
                    }
                }
                if (UsedCapacity < Capacity)
                {
                    ++UsedCapacity;
                }
                return;
            }

            /// <summary>
            /// Gets the number of ones within the recent history.
            /// </summary>
            /// <param name="recentHistLength">The length of the history to be considered (-1 means the whole available history).</param>
            public int GetNumOfOnes(int recentHistLength = -1)
            {
                if (recentHistLength > Capacity || recentHistLength < -1)
                {
                    throw new ArgumentException($"Invalid recentHistLength {recentHistLength}.", "recentHistLength");
                }
                else if (recentHistLength == 0)
                {
                    return 0;
                }
                else if (recentHistLength == -1)
                {
                    return NumOfOnes;
                }
                else
                {
                    int numOfSegments = recentHistLength / MaxBits + ((recentHistLength % MaxBits) > 0 ? 1 : 0);
                    int lastSegHighestBitIndex = (recentHistLength - ((numOfSegments - 1) * MaxBits)) - 1;
                    int counter = 0;
                    for (int i = 0; i < numOfSegments - 1; i++)
                    {
                        counter += _segBitCounter[i];
                    }
                    for (int i = 0; i <= lastSegHighestBitIndex; i++)
                    {
                        counter += Bitwise.GetBit(_buffSegments[numOfSegments - 1], i);
                    }
                    return counter;
                }
            }

            /// <summary>
            /// Returns the bit at the specified position within the window.
            /// </summary>
            /// <param name="bitIndex">The zero based index of the bit (0 is the index of recent bit).</param>
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
            /// Computes the fading sum of bits in the recent history.
            /// </summary>
            /// <param name="fadingStrength">The fading strength between 0 and 1.</param>
            /// <param name="recentHistLength">The length of the history to be considered (-1 means the whole available history).</param>
            public double GetFadingSum(double fadingStrength, int recentHistLength = -1)
            {
                if (fadingStrength < 0d || fadingStrength >= 1d)
                {
                    throw new ArgumentException($"Invalid fadingStrength {fadingStrength.ToString(CultureInfo.InvariantCulture)}.", "fadingStrength");
                }
                if (recentHistLength > Capacity || recentHistLength < -1)
                {
                    throw new ArgumentException($"Invalid recentHistLength {recentHistLength}.", "recentHistLength");
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
                for (int index = recentHistLength - 1; index >= 0; index--)
                {
                    fadingSum *= fadingCoeff;
                    fadingSum += GetBit(index);
                }
                return fadingSum;
            }

            /// <summary>
            /// Returns the bit sequence.
            /// </summary>
            /// <param name="index">The starting zero-based index within the window (0 is the index of recent bit)</param>
            /// <param name="length">The length of the sequence (1 ... MaxBits).</param>
            /// <param name="reverseOrder">Specifies whether to reverse the order of bits within the sequence.</param>
            /// <returns>The sequence of bits.</returns>
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
                if (reverseOrder)
                {
                    for (int i = index - (length - 1); i <= index; i++)
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
            /// Returns the bit sequence.
            /// </summary>
            /// <param name="length">The length of the sequence (1 ... MaxBits).</param>
            /// <param name="reverseOrder">Specifies whether to reverse the order of bits within the sequence.</param>
            /// <returns>The sequence of bits.</returns>
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

