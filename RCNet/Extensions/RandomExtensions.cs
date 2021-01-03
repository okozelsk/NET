using RCNet.RandomValue;
using System;
using System.Collections.Generic;

namespace RCNet.Extensions
{
    /// <summary>
    /// Implements useful extensions of the Random class.
    /// </summary>
    public static class RandomExtensions
    {
        //Constants
        private const double PI2 = 2d * Math.PI;
        private static readonly double Log4 = Math.Log(4d);
        private static readonly double GammaAlgConst = 1d + Math.Log(4.5d);

        /// <summary>
        /// Randomly shuffles the elements within an array.
        /// </summary>
        /// <remarks>
        /// Follows the Uniform distribution.
        /// </remarks>
        /// <param name="array">The array to be shuffled.</param>
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
        /// Randomly shuffles the elements within a list.
        /// </summary>
        /// <remarks>
        /// Follows the Uniform distribution.
        /// </remarks>
        /// <param name="list">A list to be shuffled.</param>
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
        /// Returns the random sign (values 1 or -1).
        /// </summary>
        /// <remarks>
        /// Follows the Uniform distribution.
        /// </remarks>
        /// <param name="rand"></param>
        public static double NextSign(this Random rand)
        {
            return rand.NextDouble() >= 0.5 ? 1d : -1d;
        }

        /// <inheritdoc cref="Random.NextDouble"/>
        /// <remarks>
        /// Follows the Uniform distribution.
        /// </remarks>
        public static double NextUniformDouble(this Random rand)
        {
            return rand.NextDouble();
        }

        /// <summary>
        /// Returns a random double within the specified range.
        /// </summary>
        /// <remarks>
        /// Follows the Uniform distribution.
        /// </remarks>
        /// <param name="min">The min value (inclusive).</param>
        /// <param name="max">The max value (exclusive).</param>
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
        /// Fills an array with random double values within the specified range.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Follows the Uniform distribution.
        /// </para>
        /// </remarks>
        /// <param name="array">An array to be filled.</param>
        /// <param name="min">The min value (inclusive).</param>
        /// <param name="max">The max value (exclusive).</param>
        /// <param name="randomSign">Specifies whether to randomize sign.</param>
        /// <param name="rand"></param>
        public static void FillUniform(this Random rand, double[] array, double min, double max, bool randomSign)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = rand.NextRangedUniformDouble(min, max) * (randomSign ? rand.NextSign() : 1d);
            }
            return;
        }

        /// <summary>
        /// Returns a random double value.
        /// </summary>
        /// <remarks>
        /// Follows the Gaussian distribution.
        /// </remarks>
        /// <param name="mean">Required mean.</param>
        /// <param name="stdDev">Required standard deviation.</param>
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
        /// Returns a random double value.
        /// </summary>
        /// <remarks>
        /// Follows the Gaussian distribution.
        /// </remarks>
        /// <param name="distrCfg">Configuration of the Gaussian distribution.</param>
        /// <param name="rand"></param>
        public static double NextGaussianDouble(this Random rand, GaussianDistrSettings distrCfg)
        {
            return NextGaussianDouble(rand, distrCfg.Mean, distrCfg.StdDev);
        }

        /// <summary>
        /// Returns a random double value.
        /// </summary>
        /// <remarks>
        /// Follows the Gaussian distribution.
        /// </remarks>
        /// <param name="distrCfg">Configuration of the unsigned Gaussian distribution.</param>
        /// <param name="rand"></param>
        public static double NextGaussianDouble(this Random rand, UGaussianDistrSettings distrCfg)
        {
            return NextGaussianDouble(rand, distrCfg.Mean, distrCfg.StdDev);
        }

        /// <summary>
        /// Returns a random double value within the specified range.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Follows the Gaussian distribution.
        /// </para>
        ///  <para>
        /// Warning: due to application of the filtering loop to get values belonging into the specified range, this function can lead to a bad performance. The performance strongly depends on specified parameters.
        /// </para>
        /// </remarks>
        /// <param name="mean">Required mean.</param>
        /// <param name="stdDev">Required standard deviation.</param>
        /// <param name="min">The min value (inclusive).</param>
        /// <param name="max">The max value (inclusive).</param>
        /// <param name="rand"></param>
        public static double NextRangedGaussianDouble(this Random rand, double mean, double stdDev, double min, double max)
        {
            //Check the randomness suppression
            if (min == max)
            {
                return min;
            }
            //Validations
            if (min > max)
            {
                throw new ArgumentException($"Min is greater than max.", "min and max");
            }
            //Filtering loop
            double result;
            do
            {
                result = rand.NextGaussianDouble(mean, stdDev);
            } while (result < min || result > max);
            return result;
        }

        /// <summary>
        /// Returns a random double value.
        /// </summary>
        /// <remarks>
        /// Follows the Exponential distribution.
        /// </remarks>
        /// <param name="mean">Required mean.</param>
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
        /// Returns a random double value.
        /// </summary>
        /// <remarks>
        /// Follows the Gaussian distribution.
        /// </remarks>
        /// <param name="distrCfg">Configuration of the Exponential distribution.</param>
        /// <param name="rand"></param>
        public static double NextExponentialDouble(this Random rand, ExponentialDistrSettings distrCfg)
        {
            return NextExponentialDouble(rand, distrCfg.Mean);
        }

        /// <summary>
        /// Returns a random double value.
        /// </summary>
        /// <remarks>
        /// Follows the Gaussian distribution.
        /// </remarks>
        /// <param name="distrCfg">Configuration of the unsigned Exponential distribution.</param>
        /// <param name="rand"></param>
        public static double NextExponentialDouble(this Random rand, UExponentialDistrSettings distrCfg)
        {
            return NextExponentialDouble(rand, distrCfg.Mean);
        }


        /// <summary>
        /// Returns a random double value within the specified range.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Follows the Exponential distribution.
        /// </para>
        ///  <para>
        /// Warning: due to application of the filtering loop to get values belonging into the specified range, this function can lead to a bad performance. The performance strongly depends on specified parameters.
        /// </para>
        /// </remarks>
        /// <param name="mean">Required mean.</param>
        /// <param name="min">The min value (inclusive).</param>
        /// <param name="max">The max value (inclusive).</param>
        /// <param name="rand"></param>
        public static double NextRangedExponentialDouble(this Random rand, double mean, double min, double max)
        {
            //Check the randomness suppression
            if (min == max)
            {
                return min;
            }
            //Validations
            if (min > max)
            {
                throw new ArgumentException($"Min is greater than max.", "min and max");
            }
            //Filtering loop
            double result;
            do
            {
                result = rand.NextExponentialDouble(mean);
            } while (result < min || result > max);
            return result;
        }

        /// <summary>
        /// Returns a random double value.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Follows the Gaussian distribution.
        /// </para>
        /// <para>
        /// Implementation is converted from Python.
        /// Note that Mean tends to alpha/beta and StdDev tends to Sqrt(alpha/(beta*beta)).
        /// Generated number is always positive.
        /// </para>
        /// </remarks>
        /// <param name="alpha">The shape parameter (must be greater than 0).</param>
        /// <param name="beta">The rate parameter (must be greater than 0).</param>
        /// <param name="rand"></param>
        public static double NextGammaDouble(this Random rand, double alpha, double beta)
        {
            //Checks
            if (alpha <= 0)
            {
                throw new ArgumentException("Alpha parameter must be GT 0.", "alpha");
            }
            if (beta <= 0)
            {
                throw new ArgumentException("Beta parameter must be GT 0.", "beta");
            }
            //Computation
            if (alpha > 1d)
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
            else if (alpha == 1d)
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
        /// Returns a random double value.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Follows the Gaussian distribution.
        /// </para>
        /// <para>
        /// Implementation is converted from Python.
        /// Note that Mean tends to alpha/beta and StdDev tends to Sqrt(alpha/(beta*beta)).
        /// Generated number is always positive.
        /// </para>
        /// </remarks>
        /// <param name="distrCfg">Configuration of the Gamma distribution.</param>
        /// <param name="rand"></param>
        public static double NextGammaDouble(this Random rand, GammaDistrSettings distrCfg)
        {
            return NextGammaDouble(rand, distrCfg.Alpha, distrCfg.Beta);
        }


        /// <summary>
        /// Returns a random double value within the specified range.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Follows the Gamma distribution.
        /// </para>
        ///  <para>
        /// Warning: due to application of the filtering loop to get values belonging into the specified range, this function can lead to a bad performance. The performance strongly depends on specified parameters.
        /// </para>
        /// <para>
        /// Implementation is converted from Python.
        /// Note that Mean tends to alpha/beta and StdDev tends to Sqrt(alpha/(beta*beta)).
        /// Generated number is always positive.
        /// </para>
        /// </remarks>
        /// <param name="alpha">The shape parameter (must be greater than 0).</param>
        /// <param name="beta">The rate parameter (must be greater than 0).</param>
        /// <param name="min">The min value (inclusive, must be greater than 0).</param>
        /// <param name="max">The max value (inclusive, must be greater than 0).</param>
        /// <param name="rand"></param>
        public static double NextRangedGammaDouble(this Random rand, double alpha, double beta, double min, double max)
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
        /// Returns a random double value according to the specified configuration.
        /// </summary>
        /// <param name="randomValueCfg">The random value configuration.</param>
        /// <param name="rand"></param>
        public static double NextDouble(this Random rand, RandomValueSettings randomValueCfg)
        {

            double value;
            switch (randomValueCfg.DistrType)
            {
                case RandomCommon.DistributionType.Uniform:
                    value = rand.NextRangedUniformDouble(randomValueCfg.Min, randomValueCfg.Max);
                    break;
                case RandomCommon.DistributionType.Gaussian:
                    if (randomValueCfg.DistrCfg != null)
                    {
                        GaussianDistrSettings gaussianCfg = randomValueCfg.DistrCfg as GaussianDistrSettings;
                        value = rand.NextRangedGaussianDouble(gaussianCfg.Mean, gaussianCfg.StdDev, randomValueCfg.Min, randomValueCfg.Max);
                    }
                    else
                    {
                        throw new ArgumentException($"A specific configuration of the Gaussian distribution is missing.", "randomValueCfg");
                    }
                    break;
                case RandomCommon.DistributionType.Exponential:
                    if (randomValueCfg.DistrCfg != null)
                    {
                        ExponentialDistrSettings exponentialCfg = randomValueCfg.DistrCfg as ExponentialDistrSettings;
                        value = rand.NextRangedExponentialDouble(exponentialCfg.Mean, randomValueCfg.Min, randomValueCfg.Max);
                    }
                    else
                    {
                        throw new ArgumentException($"A specific configuration of the Exponential distribution is missing.", "randomValueCfg");
                    }
                    break;
                case RandomCommon.DistributionType.Gamma:
                    if (randomValueCfg.DistrCfg != null)
                    {
                        GammaDistrSettings gammaCfg = randomValueCfg.DistrCfg as GammaDistrSettings;
                        value = rand.NextRangedGammaDouble(gammaCfg.Alpha, gammaCfg.Beta, randomValueCfg.Min, randomValueCfg.Max);
                    }
                    else
                    {
                        throw new ArgumentException($"A specific configuration of the Gamma distribution is missing.", "randomValueCfg");
                    }
                    break;
                default:
                    throw new ArgumentException($"Unknown distribution type: {randomValueCfg.DistrType}.", "randomValueCfg");
            }
            if (randomValueCfg.RandomSign)
            {
                value *= rand.NextSign();
            }
            return value;
        }

        /// <summary>
        /// Returns a random double value according to the specified configuration.
        /// </summary>
        /// <param name="randomValueCfg">The random unsigned value configuration.</param>
        /// <param name="rand"></param>
        public static double NextDouble(this Random rand, URandomValueSettings randomValueCfg)
        {

            double value;
            switch (randomValueCfg.DistrType)
            {
                case RandomCommon.DistributionType.Uniform:
                    value = rand.NextRangedUniformDouble(randomValueCfg.Min, randomValueCfg.Max);
                    break;
                case RandomCommon.DistributionType.Gaussian:
                    if (randomValueCfg.DistrCfg != null)
                    {
                        UGaussianDistrSettings gaussianCfg = randomValueCfg.DistrCfg as UGaussianDistrSettings;
                        value = rand.NextRangedGaussianDouble(gaussianCfg.Mean, gaussianCfg.StdDev, randomValueCfg.Min, randomValueCfg.Max);
                    }
                    else
                    {
                        throw new ArgumentException($"A specific configuration of the Gaussian distribution is missing.", "randomValueCfg");
                    }
                    break;
                case RandomCommon.DistributionType.Exponential:
                    if (randomValueCfg.DistrCfg != null)
                    {
                        UExponentialDistrSettings exponentialCfg = randomValueCfg.DistrCfg as UExponentialDistrSettings;
                        value = rand.NextRangedExponentialDouble(exponentialCfg.Mean, randomValueCfg.Min, randomValueCfg.Max);
                    }
                    else
                    {
                        throw new ArgumentException($"A specific configuration of the Exponential distribution is missing.", "randomValueCfg");
                    }
                    break;
                case RandomCommon.DistributionType.Gamma:
                    if (randomValueCfg.DistrCfg != null)
                    {
                        GammaDistrSettings gammaCfg = randomValueCfg.DistrCfg as GammaDistrSettings;
                        value = rand.NextRangedGammaDouble(gammaCfg.Alpha, gammaCfg.Beta, randomValueCfg.Min, randomValueCfg.Max);
                    }
                    else
                    {
                        throw new ArgumentException($"A specific configuration of the Gamma distribution is missing.", "randomValueCfg");
                    }
                    break;
                default:
                    throw new ArgumentException($"Unknown distribution type: {randomValueCfg.DistrType}.", "randomValueCfg");
            }
            return value;
        }

    }//RandomExtensions

}//Namespace

