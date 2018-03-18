using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OKOSW.Extensions
{
    /// <summary>
    /// Useful extensions of Array class
    /// </summary>
    public static class ArrayClassExtensions
    {
        /// <summary>Shifts all array elements right and sets the first element newValue</summary>
        /// <param name="newValue">New value of the first array element</param>
        public static void ShiftRight<T>(this T[] array, T newValue)
        {
            for (int idx = array.Length - 1; idx >= 1; idx--)
            {
                array[idx] = array[idx - 1];
            }
            array[0] = newValue;
            return;
        }

        /// <summary>Shifts all array elements left and sets the last element newValue</summary>
        /// <param name="newValue">New value of the last array element</param>
        public static void ShiftLeft<T>(this T[] array, T newValue)
        {
            for (int idx = 0; idx <= array.Length - 1; idx++)
            {
                array[idx] = array[idx + 1];
            }
            array[array.Length - 1] = newValue;
            return;
        }

        /// <summary>Compares arrays by values</summary>
        /// <param name="cmpArray">Array which values to be compared with this array values</param>
        public static bool EqualValues<T>(this T[] array, T[] cmpArray)
        {
            if (array.Length != cmpArray.Length) return false;
            for (int idx = 0; idx < array.Length; idx++)
            {
                if(!array[idx].Equals(cmpArray[idx]))return false;
            }
            return true;
        }

        /// <summary>Fills all one dimensional array elements by specified value</summary>
        /// <param name="value">Array will be filled by this value</param>
        public static void Populate<T>(this T[] array, T value, int start = -1, int count = -1)
        {
            if (start < 0) start = 0;
            if (count < 0) count = array.Length;
            for (int idx = start; idx < (start + count); idx++)
            {
                array[idx] = value;
            }
            return;
        }

        /// <summary>Fills all two dimensional array elements by specified value</summary>
        /// <param name="value">Array will be filled by this value</param>
        public static void Populate<T>(this T[,] array, T value)
        {
            int lastIdx0 = array.GetUpperBound(0);
            int lastIdx1 = array.GetUpperBound(1);
            for (int idx0 = 0; idx0 <= lastIdx0; idx0++)
            {
                for (int idx1 = 0; idx1 <= lastIdx1; idx1++)
                {
                    array[idx0, idx1] = value;
                }
            }
            return;
        }

        /// <summary>Fills all arrays of arrays elements by specified value</summary>
        /// <param name="value">Array will be filled by this value</param>
        public static void Populate<T>(this T[][] array, T value)
        {
            int lastIdx0 = array.GetUpperBound(0);
            for (int idx0 = 0; idx0 <= lastIdx0; idx0++)
            {
                int lastIdx1 = array[idx0].GetUpperBound(0);
                for (int idx1 = 0; idx1 <= lastIdx1; idx1++)
                {
                    array[idx0][idx1] = value;
                }
            }
            return;
        }

        /// <summary>Creates full clone of the array of arrays</summary>
        public static T[][] Clone2Dim<T>(this T[][] array)
        {
            int lastIdx0 = array.GetUpperBound(0);
            T[][] clone = new T[lastIdx0 + 1][];
            for (int idx0 = 0; idx0 <= lastIdx0; idx0++)
            {
                int lastIdx1 = array[idx0].GetUpperBound(0);
                clone[idx0] = new T[lastIdx1 + 1];
                for (int idx1 = 0; idx1 <= lastIdx1; idx1++)
                {
                    clone[idx0][idx1] = array[idx0][idx1];
                }
            }
            return clone;
        }

        /// <summary>Muls all array values by value</summary>
        /// <param name="value">Array values will be multiplicated by this value</param>
        public static void Scale(this double[] array, double value)
        {
            for (int idx = 0; idx < array.Length; idx++)
            {
                array[idx] *= value;
            }
            return;
        }

        /// <summary>
        /// Fills array by randomly shuffled indexes 0...(ArrayLength - 1)
        /// </summary>
        /// <param name="array">Array to fill</param>
        /// <param name="rand">Random object to be used</param>
        public static void ShuffledIndices(this int[] array, Random rand)
        {
            for (int idx = 0; idx < array.Length; idx++)
            {
                array[idx] = idx;
            }
            rand.Shuffle(array);
            return;
        }



        public static double[] NewVector(int len, double iniValue = 0)
        {
            double[] result = new double[len];
            for (int i = 0; i < len; i++)
                result[i] = iniValue;
            return result;
        }

        public static double[][] NewMatrix(int rows, int cols, double iniValue = 0)
        {
            double[][] matrix = new double[rows][];
            for (int row = 0; row < matrix.Length; row++)
                matrix[row] = NewVector(cols, iniValue);
            return matrix;
        }

    }

}
