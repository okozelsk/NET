using System;
using System.Runtime.CompilerServices;

namespace RCNet.Extensions
{
    /// <summary>
    /// Implements useful extensions of an array of integers.
    /// </summary>
    public static class IntArrayExtensions
    {
        //Methods
        /// <summary>
        /// Fills the array with the indices.
        /// </summary>
        /// <param name="array"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Indices(this int[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = i;
            }
            return;
        }

        /// <summary>
        /// Fills the array with the indices in a random order.
        /// </summary>
        /// <param name="rand">The random generator to be used.</param>
        /// <param name="array"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ShuffledIndices(this int[] array, Random rand)
        {
            array.Indices();
            rand.Shuffle(array);
            return;
        }

        /// <summary>
        /// Returns an index of the min value within the array.
        /// </summary>
        /// <param name="array"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfMin(this int[] array)
        {
            int minValue = array[0];
            int minIndex = 0;
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i] < minValue)
                {
                    minValue = array[i];
                    minIndex = i;
                }
            }
            return minIndex;
        }

        /// <summary>
        /// Returns an index of the max value within the array.
        /// </summary>
        /// <param name="array"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfMax(this int[] array)
        {
            int maxValue = array[0];
            int maxIndex = 0;
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i] > maxValue)
                {
                    maxValue = array[i];
                    maxIndex = i;
                }
            }
            return maxIndex;
        }

        /// <summary>
        /// Returns the max value within the array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Max(this int[] array)
        {
            int max = int.MinValue;
            for (int i = 0; i < array.Length; i++)
            {
                if (max < array[i])
                {
                    max = array[i];
                }
            }
            return max;
        }

        /// <summary>
        /// Returns the min value within the array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Min(this int[] array)
        {
            int min = int.MaxValue;
            for (int i = 0; i < array.Length; i++)
            {
                if (min > array[i])
                {
                    min = array[i];
                }
            }
            return min;
        }

        /// <summary>
        /// Returns the sum of the values within the array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sum(this int[] array)
        {
            int sum = 0;
            for (int i = 0; i < array.Length; i++)
            {
                sum += array[i];
            }
            return sum;
        }

    }//IntArrayExtensions

}//Namespace

