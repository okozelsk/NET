﻿using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
            if(newRange == null)
            {
                newRange = new Interval(0d, 1d);
            }
            Interval orgMinMax = new Interval(array);
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = newRange.Rescale(array[i], orgMinMax);
            }
            return;
        }

        /// <summary>
        /// Returns maximum value in the array
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Max(this double[] array)
        {
            double max = double.MinValue;
            for (int i = 0; i < array.Length; i++)
            {
                if(max < array[i])
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

    }//DoubleArrayExtensions

}//Namespace

