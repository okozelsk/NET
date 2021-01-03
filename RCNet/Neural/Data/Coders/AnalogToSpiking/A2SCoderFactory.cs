using System;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Coders.AnalogToSpiking
{
    /// <summary>
    /// Provides a proper instantiation of the A2S coders and also proper loading of their configurations.
    /// </summary>
    public static class A2SCoderFactory
    {
        /// <summary>
        /// Based on the xml element name loads the proper type of A2S coder configuration.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration.</param>
        public static RCNetBaseSettings LoadSettings(XElement elem)
        {
            switch (elem.Name.LocalName)
            {
                case "gaussianReceptorsCoder":
                    return new A2SCoderGaussianReceptorsSettings(elem);
                case "signalStrengthCoder":
                    return new A2SCoderSignalStrengthSettings(elem);
                case "upDirArrowsCoder":
                    return new A2SCoderUpDirArrowsSettings(elem);
                case "downDirArrowsCoder":
                    return new A2SCoderDownDirArrowsSettings(elem);
                default:
                    throw new ArgumentException($"Unexpected element name {elem.Name.LocalName}", "elem");
            }
        }

        /// <summary>
        /// Based on the xml element name checks whether it is an existing type of A2S coder configuration.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration.</param>
        public static bool CheckSettingsElemName(XElement elem)
        {
            switch (elem.Name.LocalName)
            {
                case "gaussianReceptorsCoder":
                    return true;
                case "signalStrengthCoder":
                    return true;
                case "upDirArrowsCoder":
                    return true;
                case "downDirArrowsCoder":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Instantiates the appropriate A2S coder.
        /// </summary>
        /// <param name="cfg">The coder configuration.</param>
        public static A2SCoderBase Create(RCNetBaseSettings cfg)
        {
            Type type = cfg.GetType();
            if (type == typeof(A2SCoderGaussianReceptorsSettings))
            {
                return new A2SCoderGaussianReceptors((A2SCoderGaussianReceptorsSettings)cfg);
            }
            else if (type == typeof(A2SCoderSignalStrengthSettings))
            {
                return new A2SCoderSignalStrength((A2SCoderSignalStrengthSettings)cfg);
            }
            else if (type == typeof(A2SCoderUpDirArrowsSettings))
            {
                return new A2SCoderUpDirArrows((A2SCoderUpDirArrowsSettings)cfg);
            }
            else if (type == typeof(A2SCoderDownDirArrowsSettings))
            {
                return new A2SCoderDownDirArrows((A2SCoderDownDirArrowsSettings)cfg);
            }
            else
            {
                throw new ArgumentException($"Unexpected A2S coder type {type.Name}", "settings");
            }
        }

        /// <summary>
        /// Checks whether the specified configuration is an existing type of A2S coder configuration.
        /// </summary>
        /// <param name="cfg">The coder configuration.</param>
        public static bool CheckSettings(RCNetBaseSettings cfg)
        {
            Type type = cfg.GetType();
            if (type == typeof(A2SCoderGaussianReceptorsSettings))
            {
                return true;
            }
            else if (type == typeof(A2SCoderSignalStrengthSettings))
            {
                return true;
            }
            else if (type == typeof(A2SCoderUpDirArrowsSettings))
            {
                return true;
            }
            else if (type == typeof(A2SCoderDownDirArrowsSettings))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }//A2SCoderFactory

}//Namespace

