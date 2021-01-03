using System;
using System.Xml.Linq;

namespace RCNet.RandomValue
{
    /// <summary>
    /// Implements the enumerations and helper methods related to random values.
    /// </summary>
    public static class RandomCommon
    {
        //Enums
        /// <summary>
        /// The type of random distribution.
        /// </summary>
        public enum DistributionType
        {
            /// <summary>
            /// The uniform distribution.
            /// </summary>
            Uniform,
            /// <summary>
            /// The gaussian distribution.
            /// </summary>
            Gaussian,
            /// <summary>
            /// The exponential distribution.
            /// </summary>
            Exponential,
            /// <summary>
            /// The gamma distribution.
            /// </summary>
            Gamma
        }

        //Static methods
        /// <summary>
        /// Gets the default name of a xml element holding the configuration of specified distribution type.
        /// </summary>
        /// <param name="distrType">The distribution type.</param>
        public static string GetDistrElemName(DistributionType distrType)
        {
            switch (distrType)
            {
                case DistributionType.Uniform:
                    return "uniformDistr";
                case DistributionType.Gaussian:
                    return "gaussianDistr";
                case DistributionType.Exponential:
                    return "exponentialDistr";
                case DistributionType.Gamma:
                    return "gammaDistr";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Loads the configuration of random distribution.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public static IDistrSettings LoadDistrCfg(XElement elem)
        {
            switch (elem.Name.LocalName)
            {
                case "uniformDistr":
                    return new UniformDistrSettings(elem);
                case "gaussianDistr":
                    return new GaussianDistrSettings(elem);
                case "exponentialDistr":
                    return new ExponentialDistrSettings(elem);
                case "gammaDistr":
                    return new GammaDistrSettings(elem);
                default:
                    throw new ArgumentException($"Unexpected element name {elem.Name.LocalName}.", "elem");
            }
        }

        /// <summary>
        /// Loads the configuration of random distribution (unsigned version).
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public static IDistrSettings LoadUDistrCfg(XElement elem)
        {
            switch (elem.Name.LocalName)
            {
                case "uniformDistr":
                    return new UniformDistrSettings(elem);
                case "gaussianDistr":
                    return new UGaussianDistrSettings(elem);
                case "exponentialDistr":
                    return new UExponentialDistrSettings(elem);
                case "gammaDistr":
                    return new GammaDistrSettings(elem);
                default:
                    throw new ArgumentException($"Unexpected element name {elem.Name.LocalName}.", "elem");
            }
        }


    }//RandomCommon

}//Namespace
