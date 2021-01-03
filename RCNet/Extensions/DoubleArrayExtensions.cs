using RCNet.MathTools;
using System;
using System.Runtime.CompilerServices;

namespace RCNet.Extensions
{
    /// <summary>
    /// Implements useful extensions of an array of doubles.
    /// </summary>
    public static class DoubleArrayExtensions
    {

        //Methods
        /// <summary>
        /// Multiplicates the array values by the specified coefficient.
        /// </summary>
        /// <param name="coeff">The coefficient.</param>
        /// <param name="array"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Scale(this double[] array, double coeff)
        {
            for (int idx = 0; idx < array.Length; idx++)
            {
                array[idx] *= coeff;
            }
            return;
        }

        /// <summary>
        /// Rescales the array values to the new range.
        /// </summary>
        /// <param name="newRange">The new range (min max interval).</param>
        /// <param name="array"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Rescale(this double[] array, Interval newRange = null)
        {
            if (newRange == null)
            {
                newRange = new Interval(0d, 1d);
            }
            Interval orgMinMax = new Interval(array);
            if (orgMinMax.Min != orgMinMax.Max && newRange.Min != newRange.Max)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = newRange.Rescale(array[i], orgMinMax);
                }
            }
            return;
        }

        /// <summary>
        /// Returns the max value within the array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Max(this double[] array)
        {
            double max = double.MinValue;
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
        /// Returns an index of the max value within the array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MaxIdx(this double[] array)
        {
            double max = double.MinValue;
            int maxIdx = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (max < array[i])
                {
                    max = array[i];
                    maxIdx = i;
                }
            }
            return maxIdx;
        }

        /// <summary>
        /// Returns the min value within the array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Min(this double[] array)
        {
            double min = double.MaxValue;
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
        /// Returns an index of the min value within the array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MinIdx(this double[] array)
        {
            double min = double.MaxValue;
            int minIdx = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (min > array[i])
                {
                    min = array[i];
                    minIdx = i;
                }
            }
            return minIdx;
        }

        /// <summary>
        /// Returns the sum of the values within the array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Sum(this double[] array)
        {
            double sum = 0d;
            for (int i = 0; i < array.Length; i++)
            {
                sum += array[i];
            }
            return sum;
        }


        /// <summary>
        /// Scales the values within the array in the way their sum equals to the specified value.
        /// </summary>
        /// <param name="newSum">The new sum value.</param>
        /// <param name="array"></param>
        public static void ScaleToNewSum(this double[] array, double newSum = 1d)
        {
            double sum = Sum(array);
            if (sum != newSum)
            {
                if (sum != 0d)
                {
                    array.Scale(newSum / sum);
                }
                else
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        array[i] += newSum / array.Length;
                    }
                }
            }
            return;
        }

        /// <summary>
        /// Changes the values within an array proportionally according to the rule that the new min is original max and the new max is the original min.
        /// </summary>
        public static void RevertMinMax(this double[] array)
        {
            double max = array.Max();
            double min = array.Min();
            if (min != max)
            {
                array.Rescale();
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = min + (1d - array[i]) * (max - min);
                }
            }
            return;
        }

        /// <summary>
        /// Applies the softmax.
        /// </summary>
        public static void Softmax(this double[] array)
        {
            double min = array.Min();
            double max = array.Max();
            if (min != max)
            {
                //Transformation is possible
                array.ScaleToNewSum(1d);
                min = array.Min();
                max = array.Max();
                double[] expW = new double[array.Length];
                double expWSum = 0d;
                for (int i = 0; i < array.Length; i++)
                {
                    expW[i] = Math.Exp((array[i] - min) / (max - min));
                    expWSum += expW[i];
                }
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = expW[i] / expWSum;
                }
            }
            else
            {
                //Softmax transformation is not possible, ensure the sum of weights is 1
                array.ScaleToNewSum(1d);
            }
            return;
        }



    }//DoubleArrayExtensions

}//Namespace

