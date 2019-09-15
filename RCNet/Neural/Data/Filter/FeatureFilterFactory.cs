using RCNet.MathTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Filter
{
    /// <summary>
    /// Provides proper load of settings and instantiation of feature filters
    /// </summary>
    public static class FeatureFilterFactory
    {
        /// <summary>
        /// Based on element name loads proper type of feature filter settings
        /// </summary>
        /// <param name="elem">Element containing settings</param>
        /// <returns></returns>
        public static FeatureFilterSettings LoadSettings(XElement elem)
        {
            switch(elem.Name.LocalName)
            {
                case "binFeature":
                    return new BinFeatureFilterSettings(elem);
                case "enumFeature":
                    return new EnumFeatureFilterSettings(elem);
                case "realFeature":
                    return new RealFeatureFilterSettings(elem);
                default:
                    throw new ArgumentException($"Unexpected element name {elem.Name.LocalName}", "elem");
            }
        }

        /// <summary>
        /// Instantiates feature filter of proper type according to settings
        /// </summary>
        /// <param name="outputRange">Output range of feature filter</param>
        /// <param name="settings">Settings of feature filter</param>
        public static FeatureFilter Create(Interval outputRange, FeatureFilterSettings settings)
        {
            switch(settings.Type)
            {
                case FeatureFilter.FeatureType.Binary:
                    return new BinFeatureFilter(outputRange, (BinFeatureFilterSettings)settings);
                case FeatureFilter.FeatureType.Enum:
                    return new EnumFeatureFilter(outputRange, (EnumFeatureFilterSettings)settings);
                case FeatureFilter.FeatureType.Real:
                    return new RealFeatureFilter(outputRange, (RealFeatureFilterSettings)settings);
                default:
                    throw new ArgumentException($"Unexpected feature type {settings.Type}", "settings");
            }
        }

        /// <summary>
        /// Creates deep copy of given settings
        /// </summary>
        /// <param name="settings">Settings of feature filter</param>
        public static FeatureFilterSettings DeepClone(FeatureFilterSettings settings)
        {
            switch (settings.Type)
            {
                case FeatureFilter.FeatureType.Binary:
                    return new BinFeatureFilterSettings((BinFeatureFilterSettings)settings);
                case FeatureFilter.FeatureType.Enum:
                    return new EnumFeatureFilterSettings((EnumFeatureFilterSettings)settings);
                case FeatureFilter.FeatureType.Real:
                    return new RealFeatureFilterSettings((RealFeatureFilterSettings)settings);
                default:
                    throw new ArgumentException($"Unexpected feature type {settings.Type}", "settings");
            }
        }

    }//FeatureFilterFactory
}
