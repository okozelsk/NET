using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RCNet.RandomValue
{

    public static class RandomCommon
    {
        //Enums
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
            Gaussian,
            /// <summary>
            /// Exponential distribution
            /// </summary>
            Exponential,
            /// <summary>
            /// Gamma distribution
            /// </summary>
            Gamma
        }

        //Static methods
        /// <summary>
        /// Parses code to DistributionType 
        /// </summary>
        /// <param name="code">code</param>
        public static DistributionType ParseDistributionType(string code)
        {
            switch (code.ToUpper())
            {
                case "UNIFORM": return DistributionType.Uniform;
                case "GAUSSIAN": return DistributionType.Gaussian;
                case "EXPONENTIAL": return DistributionType.Exponential;
                case "GAMMA": return DistributionType.Gamma;
                default:
                    throw new ArgumentException($"Unsupported distribution type code {code}", "code");
            }
        }

        /// <summary>
        /// Returns default name of the xml element containing settings for given distribution type
        /// </summary>
        /// <param name="distrType">Distribution type</param>
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
        /// Creates appropriate instance of DistributionSettings based on given xml element
        /// </summary>
        /// <param name="elem">Xml element containing distribution settings</param>
        /// <returns>Appropriate instance of DistributionSettings</returns>
        public static IDistrSettings CreateDistrSettings(XElement elem)
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
                    throw new Exception($"Unexpected element {elem.Name.LocalName}");
            }
        }

        /// <summary>
        /// Creates appropriate instance of DistributionSettings based on given xml element (unsigned)
        /// </summary>
        /// <param name="elem">Xml element containing distribution settings</param>
        /// <returns>Appropriate instance of DistributionSettings</returns>
        public static IDistrSettings CreateUDistrSettings(XElement elem)
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
                    throw new Exception($"Unexpected element {elem.Name.LocalName}");
            }
        }


    }//RandomCommon

}//Namespace
