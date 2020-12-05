using System;
using System.Runtime.CompilerServices;
using RCNet.MathTools;

namespace RCNet.Extensions
{
    /// <summary>
    /// Useful extensions of Array of double
    /// </summary>
    public static class DoubleArrayExtensions
    {

        //Methods
        /// <summary>
        /// Multiplicates all array values by the given coefficient
        /// </summary>
        /// <param name="coeff">Coefficient</param>
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
        /// Rescales all array elements to the new range
        /// </summary>
        /// <param name="newRange">New range (min max interval)</param>
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
        /// Returns the max value within an array
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
        /// Returns the min value within an array
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
        /// Returns sum of values within an array
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
        /// Scales members in the way the sum equals to new desired value.
        /// </summary>
        /// <param name="array">Array of doubles.</param>
        /// <param name="newSum">New sum to be ensured.</param>
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
                    for(int i = 0; i < array.Length; i++)
                    {
                        array[i] += newSum / array.Length;
                    }
                }
            }
            return;
        }

        /// <summary>
        /// Makes min is max and others proportionally
        /// </summary>
        public static void RevertMeaning(this double[] array)
        {
            double max = array.Max();
            double min = array.Min();
            if(min != max)
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
        /// Applies softmax
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

