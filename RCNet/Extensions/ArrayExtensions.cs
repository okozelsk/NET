using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RCNet.Extensions
{
    /// <summary>
    /// Implements useful extensions of an Array.
    /// </summary>
    public static class ArrayExtensions
    {
        /// <summary>
        /// Shifts array elements to the right and sets the first element value to a newValue.
        /// </summary>
        /// <param name="newValue">A new value of the first element in the array.</param>
        /// <param name="array"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ShiftRight<T>(this T[] array, T newValue)
        {
            for (int i = array.Length - 1; i >= 1; i--)
            {
                array[i] = array[i - 1];
            }
            array[0] = newValue;
            return;
        }

        /// <summary>
        /// Shifts array elements to the left and sets the last element value to a newValue.
        /// </summary>
        /// <param name="newValue">A new value of the last element in the array.</param>
        /// <param name="array"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ShiftLeft<T>(this T[] array, T newValue)
        {
            for (int i = 0; i <= array.Length - 2; i++)
            {
                array[i] = array[i + 1];
            }
            array[array.Length - 1] = newValue;
            return;
        }

        /// <summary>
        /// Fills the 1D array with the specified value.
        /// </summary>
        /// <param name="value">The value to be filled in.</param>
        /// <param name="start">The zero-based index where to start the filling.</param>
        /// <param name="count">The number of occurrences to be filled in (specify -1 to fill the rest of the array).</param>
        /// <param name="array"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Populate<T>(this T[] array, T value, int start = 0, int count = -1)
        {
            if (start < 0) start = 0;
            if (count < 0) count = array.Length;
            int end = Math.Min((start + count) - 1, array.Length - 1);
            for (int i = start; i <= end; i++)
            {
                array[i] = value;
            }
            return;
        }

        /// <summary>
        /// Fills the whole array of arrays with the specified value.
        /// </summary>
        /// <param name="value">The value to be filled in.</param>
        /// <param name="array"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Populate<T>(this T[][] array, T value)
        {
            int vLength = array.GetUpperBound(0) + 1;
            Parallel.For(0, vLength, i =>
            {
                array[i].Populate(value);
            });
            return;
        }

        /// <summary>
        /// Clones the array of arrays.
        /// </summary>
        /// <param name="array"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[][] Clone2D<T>(this T[][] array)
        {
            int vLength = array.GetUpperBound(0) + 1;
            T[][] clone = new T[vLength][];
            Parallel.For(0, vLength, i =>
            {
                if (array[i] == null)
                {
                    clone[i] = null;
                }
                else
                {
                    clone[i] = (T[])array[i].Clone();
                }
            });
            return clone;
        }

        /// <summary>
        /// Returns the new concatenation array of this array and other array.
        /// </summary>
        /// <param name="otherArray">Array to be concatenated with this array.</param>
        /// <param name="array"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] Concat<T>(this T[] array, T[] otherArray)
        {
            int length1 = (array == null ? 0 : array.Length);
            int length2 = (otherArray == null ? 0 : otherArray.Length);
            T[] result = new T[length1 + length2];
            if (length1 > 0)
            {
                array.CopyTo(result, 0);
            }
            if (length2 > 0)
            {
                otherArray.CopyTo(result, length1);
            }
            return result;
        }

    }//ArrayExtensions

}//Namespace

