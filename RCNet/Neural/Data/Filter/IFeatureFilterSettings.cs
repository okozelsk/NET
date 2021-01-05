using System.Xml.Linq;

namespace RCNet.Neural.Data.Filter
{
    /// <summary>
    /// Common interface of the feature filter configurations.
    /// </summary>
    public interface IFeatureFilterSettings
    {
        /// <inheritdoc cref="FeatureFilterBase.FeatureType"/>
        FeatureFilterBase.FeatureType Type { get; }

        /// <inheritdoc cref="RCNetBaseSettings.DeepClone"/>
        RCNetBaseSettings DeepClone();

        /// <inheritdoc cref="RCNetBaseSettings.GetXml(bool)"/>
        XElement GetXml(bool suppressDefaults);

    }//IFeatureFilterSettings

}//Namespace
