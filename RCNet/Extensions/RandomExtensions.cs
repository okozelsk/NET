using System;
using System.Collections.Generic;
using System.Globalization;
using RCNet.RandomValue;

namespace RCNet.Extensions
{
    /// <summary>
    /// Useful extensions of Random class
    /// </summary>
    public static class RandomExtensions
    {
        //Constants
        private const double PI2 = 2d * Math.PI;
        private static readonly double Log4 = Math.Log(4d);
        private static readonly double GammaAlgConst = 1d + Math.Log(4.5d);

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
        /// Returns random double between 0 (inclusive) and 1 (exclusive) following the Uniform distribution.
        /// Equals to Random.NextDouble() method.
        /// </summary>
        /// <param name="rand"></param>
        public static double NextUniformDouble(this Random rand)
        {
            return rand.NextDouble();
        }

        /// <summary>
        /// Returns random double within specified range following the Uniform distribution.
        /// </summary>
        /// <param name="min">Min target value (inclusive)</param>
        /// <param name="max">Max target value (exclusive)</param>
        /// <param name="rand"></param>
        public static double NextRangedUniformDouble(this Random rand, double min = -1, double max = 1)
        {
            //Check for randomness suppression
            if (min == max)
            {
                return min;
            }
            //Arguments validations
            if (min > max)
            {
                throw new ArgumentException($"Min is greater than max", "min");
            }
            //Computation
            return rand.NextUniformDouble() * (max - min) + min;
        }

        /// <summary>
        /// Returns random double following the Gaussian distribution.
        /// </summary>
        /// <param name="mean">Required target mean</param>
        /// <param name="stdDev">Required target standard deviation</param>
        /// <param name="rand"></param>
        public static double NextGaussianDouble(this Random rand, double mean = 0, double stdDev = 1)
        {
            //Uniform (0,1> random doubles
            double u1 = 1.0 - rand.NextDouble();
            double u2 = 1.0 - rand.NextDouble();
            //Computation
            return mean + stdDev * Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(PI2 * u2);
        }

        /// <summary>
        /// Returns random double following the Gaussian distribution.
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="distrParams">Gaussian distribution parameters</param>
        public static double NextGaussianDouble(this Random rand, GaussianDistrSettings distrParams)
        {
            return NextGaussianDouble(rand, distrParams.Mean, distrParams.StdDev);
        }

        /// <summary>
        /// Returns random double following the Gaussian distribution and belonging to a specified range.
        /// Warning: due to applied range filterring, this function can lead to a bad performance. Performance depends on parameters.
        /// </summary>
        /// <param name="mean">Required target mean</param>
        /// <param name="stdDev">Required target standard deviation</param>
        /// <param name="min">Required target min value</param>
        /// <param name="max">Required target max value</param>
        /// <param name="rand"></param>
        /// <returns></returns>
        public static double NextFilterredGaussianDouble(this Random rand, double mean, double stdDev, double min, double max)
        {
            //Check for randomness suppression
            if (min == max)
            {
                return min;
            }
            //Arguments validations
            if (min > max)
            {
                throw new ArgumentException($"Min is greater than max", "min");
            }
            //Filterring loop
            double result;
            do
            {
                result = rand.NextGaussianDouble(mean, stdDev);
            } while (result < min || result > max);
            return result;
        }

        /// <summary>
        /// Returns random double following the Exponential distribution.
        /// </summary>
        /// <param name="mean">Required target mean</param>
        /// <param name="rand"></param>
        public static double NextExponentialDouble(this Random rand, double mean)
        {
            //Checks
            if (mean == 0)
            {
                throw new ArgumentException("Mean parameter equals to 0.", "mean");
            }
            //Lambda
            double lambda = 1d / mean;
            //Computation
            return -Math.Log(1d - rand.NextDouble()) / lambda;
        }

        /// <summary>
        /// Returns random double following the Exponential distribution.
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="distrParams">Exponential distribution parameters</param>
        public static double NextExponentialDouble(this Random rand, ExponentialDistrSettings distrParams)
        {
            return NextExponentialDouble(rand, distrParams.Mean);
        }

        /// <summary>
        /// Returns random double following the Exponential distribution and belonging to a specified range.
        /// Warning: due to applied range filterring, this function can lead to a bad performance. Performance depends on parameters.
        /// </summary>
        /// <param name="mean">Required target mean</param>
        /// <param name="min">Required target min value</param>
        /// <param name="max">Required target max value</param>
        /// <param name="rand"></param>
        /// <returns></returns>
        public static double NextFilterredExponentialDouble(this Random rand, double mean, double min, double max)
        {
            //Check for randomness suppression
            if (min == max)
            {
                return min;
            }
            //Arguments validations
            if (min > max)
            {
                throw new ArgumentException($"Min is greater than max", "min");
            }
            //Filterring loop
            double result;
            do
            {
                result = rand.NextExponentialDouble(mean);
            } while (result < min || result > max);
            return result;
        }

        /// <summary>
        /// Returns positive random double following the Gamma distribution.
        /// Both alpha and beta parameters must be GT 0.
        /// Note that Mean tends to alpha/beta and StdDev tends to Sqrt(alpha/(beta*beta)).
        /// 
        /// (implementation converted from Python)
        /// </summary>
        /// <param name="alpha">Shape parameter</param>
        /// <param name="beta">Rate parameter</param>
        /// <param name="rand"></param>
        public static double NextGammaDouble(this Random rand, double alpha, double beta)
        {
            //Checks
            if(alpha <= 0)
            {
                throw new ArgumentException("Alpha parameter must be GT 0.", "alpha");
            }
            if (beta <= 0)
            {
                throw new ArgumentException("Beta parameter must be GT 0.", "beta");
            }
            //Computation
            if(alpha > 1d)
            {
                /* 
                 * R.C.H. Cheng, "The generation of Gamma variables with non-integral shape parameters"
                 * Applied Statistics, (1977), 26, No. 1, p71-74
                 */
                double ainv = Math.Sqrt(2d * alpha - 1d);
                double bbb = alpha - Log4;
                double ccc = alpha + ainv;
                while (true)
                {
                    double u1 = rand.NextDouble();
                    if (u1 > 1e-7d && u1 < 0.9999999d)
                    {
                        double u2 = 1d - rand.NextDouble();
                        double v = Math.Log(u1 / (1d - u1)) / ainv;
                        double x = alpha * Math.Exp(v);
                        double z = u1 * u1 * u2;
                        double r = bbb + ccc * v - x;
                        if (r + GammaAlgConst - 4.5d * z >= 0d || r >= Math.Log(z))
                        {
                            return x * beta;
                        }
                    }
                 }

            }
            else if(alpha == 1d)
            {
                //Exponential distribution
                return -Math.Log(1d - rand.NextDouble()) * beta;
            }
            else
            {
                //Algorithm GS of Statistical Computing - Kennedy & Gentle
                double x, p, r;
                do
                {
                    double b = (Math.E + alpha) / Math.E;
                    p = rand.NextDouble() * b;
                    if (p <= 1d)
                    {
                        x = Math.Pow(p, (1d / alpha));
                    }
                    else
                    {
                        x = -Math.Log((b - p) / alpha);
                    }
                    r = rand.NextDouble();
                } while (!(r <= Math.Exp(-x) || (p > 1d && r <= Math.Pow(x, alpha - 1d))));
                return x * beta;
            }
        }

        /// <summary>
        /// Returns positive random double following the Gamma distribution.
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="distrParams">Gamma distribution parameters</param>
        public static double NextGammaDouble(this Random rand, GammaDistrSettings distrParams)
        {
            return NextGammaDouble(rand, distrParams.Alpha, distrParams.Beta);
        }

        /// <summary>
        /// Returns random double following the Gamma distribution and belonging to a specified range.
        /// Warning: due to applied range filterring, this function can lead to a bad performance. Performance depends on parameters.
        /// </summary>
        /// <param name="alpha">Shape parameter</param>
        /// <param name="beta">Rate parameter</param>
        /// <param name="min">Required target min value</param>
        /// <param name="max">Required target max value</param>
        /// <param name="rand"></param>
        /// <returns></returns>
        public static double NextFilterredGammaDouble(this Random rand, double alpha, double beta, double min, double max)
        {
            //Check for randomness suppression
            if (min == max)
            {
                return min;
            }
            //Arguments validations
            if (min < 0)
            {
                throw new ArgumentException($"Min is less than 0", "min");
            }
            if (min > max)
            {
                throw new ArgumentException($"Min is greater than max", "min");
            }
            //Filterring loop
            double result;
            do
            {
                result = rand.NextGammaDouble(alpha, beta);
            } while (result < min || result > max);
            return result;
        }


        /// <summary>
        /// Returns random double according to specified settings.
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="settings">Encapsulated settings</param>
        public static double NextDouble(this Random rand, RandomValueSettings settings)
        {

            double value;
            switch (settings.DistrType)
            {
                case RandomCommon.DistributionType.Uniform:
                    value = rand.NextRangedUniformDouble(settings.Min, settings.Max);
                    break;
                case RandomCommon.DistributionType.Gaussian:
                    if(settings.DistrCfg != null)
                    {
                        GaussianDistrSettings gaussianCfg = settings.DistrCfg as GaussianDistrSettings;
                        value = rand.NextFilterredGaussianDouble(gaussianCfg.Mean, gaussianCfg.StdDev, settings.Min, settings.Max);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Configuration of Gaussian distribution is missing");
                    }
                    break;
                case RandomCommon.DistributionType.Exponential:
                    if (settings.DistrCfg != null)
                    {
                        ExponentialDistrSettings exponentialCfg = settings.DistrCfg as ExponentialDistrSettings;
                        value = rand.NextFilterredExponentialDouble(exponentialCfg.Mean, settings.Min, settings.Max);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Configuration of Exponential distribution is missing");
                    }
                    break;
                case RandomCommon.DistributionType.Gamma:
                    if (settings.DistrCfg != null)
                    {
                        GammaDistrSettings gammaCfg = settings.DistrCfg as GammaDistrSettings;
                        value = rand.NextFilterredGammaDouble(gammaCfg.Alpha, gammaCfg.Beta, settings.Min, settings.Max);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Configuration of Gamma distribution is missing");
                    }
                    break;
                default:
                    throw new InvalidOperationException($"Unknown distribution type {settings.DistrType}.");
            }
            if (settings.RandomSign)
            {
                value *= rand.NextSign();
            }
            return value;
        }

        /// <summary>
        /// Returns random unsigned double according to specified settings.
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="settings">Encapsulated settings</param>
        public static double NextDouble(this Random rand, URandomValueSettings settings)
        {

            double value;
            switch (settings.DistrType)
            {
                case RandomCommon.DistributionType.Uniform:
                    value = rand.NextRangedUniformDouble(settings.Min, settings.Max);
                    break;
                case RandomCommon.DistributionType.Gaussian:
                    if (settings.DistrCfg != null)
                    {
                        UGaussianDistrSettings gaussianCfg = settings.DistrCfg as UGaussianDistrSettings;
                        value = rand.NextFilterredGaussianDouble(gaussianCfg.Mean, gaussianCfg.StdDev, settings.Min, settings.Max);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Configuration of Gaussian distribution is missing");
                    }
                    break;
                case RandomCommon.DistributionType.Exponential:
                    if (settings.DistrCfg != null)
                    {
                        UExponentialDistrSettings exponentialCfg = settings.DistrCfg as UExponentialDistrSettings;
                        value = rand.NextFilterredExponentialDouble(exponentialCfg.Mean, settings.Min, settings.Max);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Configuration of Exponential distribution is missing");
                    }
                    break;
                case RandomCommon.DistributionType.Gamma:
                    if (settings.DistrCfg != null)
                    {
                        GammaDistrSettings gammaCfg = settings.DistrCfg as GammaDistrSettings;
                        value = rand.NextFilterredGammaDouble(gammaCfg.Alpha, gammaCfg.Beta, settings.Min, settings.Max);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Configuration of Gamma distribution is missing");
                    }
                    break;
                default:
                    throw new InvalidOperationException($"Unknown distribution type {settings.DistrType}.");
            }
            return value;
        }

        /// <summary>
        /// Fills array by random values following the Uniform distribution.
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="array">The array to be filled</param>
        /// <param name="min">Min value</param>
        /// <param name="max">Max value</param>
        /// <param name="randomSign">Specifies if to randomize sign</param>
        /// <param name="count">Specifies how many elements of the array to be filled</param>
        public static void FillUniform(this Random rand, double[] array, double min, double max, bool randomSign, int count = -1)
        {
            if (count < 0) count = array.Length;
            for (int i = 0; i < count; i++)
            {
                array[i] = rand.NextRangedUniformDouble(min, max) * (randomSign ? rand.NextSign() : 1d);
            }
            return;
        }

        /// <summary>
        /// Fills array with random doubles.
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

        /// <summary>
        /// Fills array with random unsigned doubles.
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="array">The array to be filled</param>
        /// <param name="settings">Encapsulated settings</param>
        /// <param name="count">Specifies how many elements of the array to be filled</param>
        public static void Fill(this Random rand, double[] array, URandomValueSettings settings, int count = -1)
        {
            if (count < 0) count = array.Length;
            for (int i = 0; i < count; i++)
            {
                array[i] = rand.NextDouble(settings);
            }
            return;
        }

    }//RandomExtensions

}//Namespace

