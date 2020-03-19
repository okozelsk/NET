using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RCNet.MathTools;

namespace RCNet.Extensions
{
    /// <summary>
    /// Useful extensions of Array of int
    /// </summary>
    public static class IntArrayExtensions
    {
        //Methods
        /// <summary>
        /// Fills array with the indexes 0...(Array.Length - 1)
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
        /// Fills array with randomly shuffled indexes 0...(ArrayLength - 1)
        /// </summary>
        /// <param name="rand">Random generator to be used</param>
        /// <param name="array"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ShuffledIndices(this int[] array, Random rand)
        {
            array.Indices();
            rand.Shuffle(array);
            return;
        }

        /// <summary>
        /// Returns index of min value within given int array
        /// </summary>
        /// <param name="array"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfMin(this int[] array)
        {
            int minValue = array[0];
            int minIndex = 0;
            for (int i = 1; i < array.Length; i++)
            {
                if(array[i] < minValue)
                {
                    minValue = array[i];
                    minIndex = i;
                }
            }
            return minIndex;
        }

        /// <summary>
        /// Returns index of max value within given int array
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
        /// Returns maximum value in the array
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
        /// Returns minimum value in the array
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
        /// Returns summed values in the array
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

