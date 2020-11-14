using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Coders.AnalogToSpiking
{
    /// <summary>
    /// Provides proper load of A2S coders settings and related instantiation of A2S coders
    /// </summary>
    public static class A2SCoderFactory
    {
        /// <summary>
        /// Based on element name loads proper type of A2S coder settings
        /// </summary>
        /// <param name="elem">Element containing settings</param>
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
        /// Based on element name checks if it is proper type of A2S coder settings
        /// </summary>
        /// <param name="elem">Element containing settings</param>
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
        /// Instantiates A2S coder of proper type according to given settings
        /// </summary>
        /// <param name="settings">Settings of A2S coder</param>
        public static A2SCoderBase Create(RCNetBaseSettings settings)
        {
            Type type = settings.GetType();
            if (type == typeof(A2SCoderGaussianReceptorsSettings))
            {
                return new A2SCoderGaussianReceptors((A2SCoderGaussianReceptorsSettings)settings);
            }
            else if (type == typeof(A2SCoderSignalStrengthSettings))
            {
                return new A2SCoderSignalStrength((A2SCoderSignalStrengthSettings)settings);
            }
            else if (type == typeof(A2SCoderUpDirArrowsSettings))
            {
                return new A2SCoderUpDirArrows((A2SCoderUpDirArrowsSettings)settings);
            }
            else if (type == typeof(A2SCoderDownDirArrowsSettings))
            {
                return new A2SCoderDownDirArrows((A2SCoderDownDirArrowsSettings)settings);
            }
            else
            {
                throw new ArgumentException($"Unexpected A2S coder type {type.Name}", "settings");
            }
        }

        /// <summary>
        /// Checks whether the given settings class is the proper A2S coder settings type
        /// </summary>
        /// <param name="settings">Settings of A2S coder</param>
        public static bool CheckSettings(RCNetBaseSettings settings)
        {
            Type type = settings.GetType();
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

