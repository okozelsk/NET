using RCNet.MathTools;
using System;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Filter
{
    /// <summary>
    /// Provides a proper instantiation of the feature filters and also proper loading of their configurations.
    /// </summary>
    public static class FeatureFilterFactory
    {
        /// <summary>
        /// Based on the xml element name loads the proper type of feature filter configuration.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration.</param>
        public static IFeatureFilterSettings LoadSettings(XElement elem)
        {
            switch (elem.Name.LocalName)
            {
                case "binFeature":
                    return new BinFeatureFilterSettings(elem);
                case "realFeature":
                    return new RealFeatureFilterSettings(elem);
                default:
                    throw new ArgumentException($"Unexpected element name {elem.Name.LocalName}", "elem");
            }
        }

        /// <summary>
        /// Instantiates the appropriate feature filter.
        /// </summary>
        /// <param name="outputRange">The output range of the feature filter.</param>
        /// <param name="cfg">The feature filter configuration.</param>
        public static FeatureFilterBase Create(Interval outputRange, IFeatureFilterSettings cfg)
        {
            switch (cfg.Type)
            {
                case FeatureFilterBase.FeatureType.Binary:
                    return new BinFeatureFilter(outputRange, (BinFeatureFilterSettings)cfg);
                case FeatureFilterBase.FeatureType.Real:
                    return new RealFeatureFilter(outputRange, (RealFeatureFilterSettings)cfg);
                default:
                    throw new ArgumentException($"Unexpected feature type {cfg.Type}", "settings");
            }
        }

    }//FeatureFilterFactory
}
