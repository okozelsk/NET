using System;
using System.Collections.Generic;

namespace RCNet.Extensions
{
    /// <summary>
    /// Useful extensions of Random class.
    /// </summary>
    public static class RandomClassExtensions
    {
        /// <summary>
        /// Randomly shuffles an array of objects
        /// </summary>
        /// <param name="array">Array of objects to be randomly shuffled</param>
        public static void Shuffle<T>(this Random rand, T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rand.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
            return;
        }

        /// <summary>
        /// Randomly shuffles a list of objects
        /// </summary>
        /// <param name="list">List of objects to be randomly shuffled</param>
        public static void Shuffle<T>(this Random rand, List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                int k = rand.Next(n--);
                T temp = list[n];
                list[n] = list[k];
                list[k] = temp;
            }
            return;
        }

        /// <summary>
        /// Returns random double following Gaussian distribution.
        /// </summary>
        /// <param name="mean">Required target mean</param>
        /// <param name="stdDev">Required target standard deviation</param>
        public static double NextGaussianDouble(this Random rand, double mean = 0, double stdDev = 1)
        {
            //Uniform(0,1] random doubles
            double u1 = 1.0 - rand.NextDouble();
            double u2 = 1.0 - rand.NextDouble();
            //Random normal(mean = 0, stdDev = 1)
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            //Random normal(mean,stdDev^2)
            return mean + stdDev * randStdNormal;
        }

        /// <summary>
        /// Returns random double following Gaussian distribution within min and max.
        /// </summary>
        public static double NextBoundedGaussianDouble(this Random rand, double min = -1, double max = 1, double mean = 0, double stdDev = 1)
        {
            double result = 0;
            do
            {
                result = NextGaussianDouble(rand, mean, stdDev);
            } while (result < min || result > max || result == mean);
            return result;
        }

        /// <summary>
        /// Fills array with gaussian random values.
        /// </summary>
        public static void FillGaussian(this Random rand, double[] array, double mean = 0, double stdDev = 1, double min = -1, double max = 1, int count = -1)
        {
            if (count < 0) count = array.Length;
            for (int i = 0; i < count; i++)
            {
                array[i] = rand.NextBoundedGaussianDouble(min, max, mean, stdDev);
            }
            return;
        }

        /// <summary>
        /// Returns random double within specified boundaries.
        /// </summary>
        /// <param name="min">Min random double value</param>
        /// <param name="max">Max random double value</param>
        public static double NextBoundedUniformDouble(this Random rand, double min = -1, double max = 1)
        {
            return rand.NextDouble() * (max - min) + min;
        }

        /// <summary>
        /// Returns random double within specified boundaries with random sign.
        /// </summary>
        /// <param name="absMin">Min random double absolute value</param>
        /// <param name="absMax">Max random double absolute value</param>
        public static double NextBoundedUniformDoubleRS(this Random rand, double absMin = 0, double absMax = 1)
        {
            double sign = (rand.Next(2) == 0) ? - 1d : 1d;
            return sign * (rand.NextDouble() * (absMax - absMin) + absMin);
        }

        /// <summary>
        /// Fills array with random values within the range min and max.
        /// </summary>
        public static void FillUniform(this Random rand, double[] array, double min = -1, double max = 1, double scale = 1, int count = -1)
        {
            if (count < 0) count = array.Length;
            for (int i = 0; i < count; i++)
            {
                array[i] = rand.NextBoundedUniformDouble(min, max) * scale;
            }
            return;
        }

        /// <summary>
        /// Fills array with random values within the given boundaries, having random sign.
        /// </summary>
        public static void FillUniformRS(this Random rand, double[] array, double absMin = 0, double absMax = 1, int count = -1)
        {
            if (count < 0) count = array.Length;
            for (int i = 0; i < count; i++)
            {
                array[i] = rand.NextBoundedUniformDoubleRS(absMin, absMax);
            }
            return;
        }
    }//RandomClassExtensions

}//Namespace
