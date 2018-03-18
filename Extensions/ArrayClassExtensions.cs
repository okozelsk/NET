using System;
using System.Runtime.CompilerServices;

namespace OKOSW.Extensions
{
    /// <summary>
    /// Useful extensions of Array class
    /// </summary>
    public static class ArrayClassExtensions
    {
        /// <summary>Shifts all array elements right and sets the first element newValue</summary>
        /// <param name="newValue">New value of the first array element</param>
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

        /// <summary>Shifts all array elements left and sets the last element newValue</summary>
        /// <param name="newValue">New value of the last array element</param>
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

        /// <summary>Compares arrays by values</summary>
        /// <param name="cmpArray">Array which values to be compared with this array values</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualValues<T>(this T[] array, T[] cmpArray)
        {
            if (array.Length != cmpArray.Length) return false;
            for (int i = 0; i < array.Length; i++)
            {
                if(!array[i].Equals(cmpArray[i]))return false;
            }
            return true;
        }

        /// <summary>Fills all one dimensional array elements by specified value</summary>
        /// <param name="value">Array will be filled by this value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Populate<T>(this T[] array, T value, int start = -1, int count = -1)
        {
            if (start < 0) start = 0;
            if (count < 0) count = array.Length;
            for (int i = start; i < (start + count); i++)
            {
                array[i] = value;
            }
            return;
        }

        /// <summary>Fills all two dimensional array elements by specified value</summary>
        /// <param name="value">Array will be filled by this value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Populate<T>(this T[,] array, T value)
        {
            int lastIdx0 = array.GetUpperBound(0);
            int lastIdx1 = array.GetUpperBound(1);
            for (int i = 0; i <= lastIdx0; i++)
            {
                for (int j = 0; j <= lastIdx1; j++)
                {
                    array[i, j] = value;
                }
            }
            return;
        }

        /// <summary>Fills all the array of arrays elements by specified value</summary>
        /// <param name="value">Array will be filled by this value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Populate<T>(this T[][] array, T value)
        {
            int vLength = array.GetUpperBound(0) + 1;
            for (int i = 0; i < vLength; i++)
            {
                int hLength = array[i].GetUpperBound(0) + 1;
                for (int j = 0; j < hLength; j++)
                {
                    array[i][j] = value;
                }
            }
            return;
        }

        /// <summary>Creates full clone of the array of arrays</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[][] Clone2D<T>(this T[][] array)
        {
            int vLength = array.GetUpperBound(0) + 1;
            T[][] clone = new T[vLength][];
            for (int i = 0; i < vLength; i++)
            {
                if (array[i] == null)
                {
                    clone[i] = null;
                }
                else
                {
                    clone[i] = (T[])array[i].Clone();
                }
            }
            return clone;
        }

        /// <summary>Multiplicates all array values by given coefficient</summary>
        /// <param name="coeff">All array values will be multiplicated by this coefficient</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScaleValues(this double[] array, double coeff)
        {
            for (int idx = 0; idx < array.Length; idx++)
            {
                array[idx] *= coeff;
            }
            return;
        }

        /// <summary>
        /// Fills array by indexes 0...(ArrayLength - 1)
        /// </summary>
        /// <param name="array">Array to be filled</param>
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
        /// Fills array by randomly shuffled indexes 0...(ArrayLength - 1)
        /// </summary>
        /// <param name="array">Array to fill</param>
        /// <param name="rand">Random object to be used</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ShuffledIndices(this int[] array, Random rand)
        {
            array.Indices();
            rand.Shuffle(array);
            return;
        }

    }

}
