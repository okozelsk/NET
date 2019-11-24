using System;
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

    }//DoubleArrayExtensions

}//Namespace

