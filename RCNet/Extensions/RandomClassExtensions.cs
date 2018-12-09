using System;
using System.Collections.Generic;
using RCNet.RandomValue;

namespace RCNet.Extensions
{
    /// <summary>
    /// Useful extensions of Random class.
    /// </summary>
    public static class RandomClassExtensions
    {
        //Constants
        /// <summary>
        /// Supported distribution types
        /// </summary>
        public enum DistributionType
        {
            /// <summary>
            /// Uniform distribution
            /// </summary>
            Uniform,
            /// <summary>
            /// Gaussian distribution
            /// </summary>
            Gaussian
        }

        /// <summary>
        /// Parses code to DistributionType 
        /// </summary>
        /// <param name="code">code</param>
        public static DistributionType ParseDistributionType(string code)
        {
            switch(code.ToUpper())
            {
                case "UNIFORM":return DistributionType.Uniform;
                case "GAUSSIAN": return DistributionType.Gaussian;
                default:
                    throw new ArgumentException($"Unsupported distribution type code {code}", "code");
            }
        }

        /// <summary>
        /// Randomly shuffles an array of objects
        /// </summary>
        /// <param name="array">Array of objects to be randomly shuffled</param>
        /// <param name="rand"></param>
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
        /// <param name="rand"></param>
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
        /// Returns 1 or -1
        /// </summary>
        /// <param name="rand"></param>
        public static double NextSign(this Random rand)
        {
            return rand.NextDouble() >= 0.5 ? 1d : -1d;
        }

        /// <summary>
        /// Returns random double following Gaussian distribution.
        /// </summary>
        /// <param name="mean">Required target mean</param>
        /// <param name="stdDev">Required target standard deviation</param>
        /// <param name="rand"></param>
        public static double NextGaussianDouble(this Random rand, double mean = 0, double stdDev = 1)
        {
            //Uniform(0,1] random doubles
            double u1 = 1.0 - rand.NextDouble();
            double u2 = 1.0 - rand.NextDouble();
            //Gaussian value construction
            return mean + stdDev * Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        }

        /// <summary>
        /// Returns random double following Gaussian distribution.
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="settings">Gaussian distribution parameters</param>
        public static double NextGaussianDouble(this Random rand, RandomValueSettings.GaussianDistrSettings settings)
        {
            return NextGaussianDouble(rand, settings.Mean, settings.StdDev);
        }
        
        /// <summary>
        /// Returns random double following Gaussian distribution within min and max.
        /// Warning: this function can lead to a bad performance. Performance depends on parameters.
        /// </summary>
        public static double NextBoundedGaussianDouble(this Random rand, double min = -1, double max = 1, double mean = double.NaN, double stdDev = double.NaN)
        {
            if (min == max)
            {
                return min;
            }
            else
            {
                if(double.IsNaN(mean))
                {
                    mean = min + (max - min) / 2d;
                }
                if(double.IsNaN(stdDev))
                {
                    stdDev = ((max - min)) / 6d;
                }
                double result = 0;
                do
                {
                    result = rand.NextGaussianDouble(mean, stdDev);
                } while (result < min || result > max || result == mean);
                return result;
            }
        }

        /// <summary>
        /// Returns random double within specified boundaries following uniform distribution.
        /// </summary>
        /// <param name="min">Min random double value</param>
        /// <param name="max">Max random double value</param>
        /// <param name="rand"></param>
        public static double NextBoundedUniformDouble(this Random rand, double min = -1, double max = 1)
        {
            if (min == max)
            {
                return min;
            }
            else
            {
                return rand.NextDouble() * (max - min) + min;
            }
        }

        /// <summary>
        /// Returns random double following specified distribution.
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="min">Min value</param>
        /// <param name="max">Max value</param>
        /// <param name="randomSign">Specifies if to randomize sign</param>
        /// <param name="distrType">Specifies which distribution to use</param>
        /// <returns></returns>
        public static double NextDouble(this Random rand, double min, double max, bool randomSign, DistributionType distrType)
        {
            double value = 0;
            switch(distrType)
            {
                case DistributionType.Uniform:
                    value = rand.NextBoundedUniformDouble(min, max);
                    break;
                case DistributionType.Gaussian:
                    value = rand.NextBoundedGaussianDouble(min, max);
                    break;
            }
            if(randomSign)
            {
                value *= rand.NextSign();
            }
            return value;
        }

        /// <summary>
        /// Returns random double following specified settings.
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="settings">Encapsulated settings</param>
        public static double NextDouble(this Random rand, RandomValueSettings settings)
        {

            double value = 0;
            switch (settings.DistrType)
            {
                case DistributionType.Uniform:
                    value = rand.NextBoundedUniformDouble(settings.Min, settings.Max);
                    break;
                case DistributionType.Gaussian:
                    if(settings.GaussianDistrCfg != null)
                    {
                        value = rand.NextBoundedGaussianDouble(settings.Min, settings.Max, settings.GaussianDistrCfg.Mean, settings.GaussianDistrCfg.StdDev);
                    }
                    else
                    {
                        value = rand.NextBoundedGaussianDouble(settings.Min, settings.Max);
                    }
                    break;
            }
            if (settings.RandomSign)
            {
                value *= rand.NextSign();
            }
            return value;
        }

        /// <summary>
        /// Fills array with random values.
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="array">The array to be filled</param>
        /// <param name="min">Min value</param>
        /// <param name="max">Max value</param>
        /// <param name="randomSign">Specifies if to randomize sign</param>
        /// <param name="distrType">Specifies which distribution to use</param>
        /// <param name="count">Specifies how many elements of the array to be filled</param>
        public static void Fill(this Random rand, double[] array, double min, double max, bool randomSign, DistributionType distrType, int count = -1)
        {
            if (count < 0) count = array.Length;
            for (int i = 0; i < count; i++)
            {
                array[i] = rand.NextDouble(min, max, randomSign, distrType);
            }
            return;
        }

        /// <summary>
        /// Fills array with random values.
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="array">The array to be filled</param>
        /// <param name="settings">Encapsulated settings</param>
        /// <param name="count">Specifies how many elements of the array to be filled</param>
        public static void Fill(this Random rand, double[] array, RandomValueSettings settings, int count = -1)
        {
            if (count < 0) count = array.Length;
            for (int i = 0; i < count; i++)
            {
                array[i] = rand.NextDouble(settings);
            }
            return;
        }

    }//RandomClassExtensions

}//Namespace

