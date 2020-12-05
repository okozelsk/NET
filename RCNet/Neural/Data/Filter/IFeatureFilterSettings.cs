using System.Xml.Linq;

namespace RCNet.Neural.Data.Filter
{
    /// <summary>
    /// Common interface of setting classes of feature filters
    /// </summary>
    public interface IFeatureFilterSettings
    {
        /// <inheritdoc cref="FeatureFilterBase.FeatureType"/>
        FeatureFilterBase.FeatureType Type { get; }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        RCNetBaseSettings DeepClone();

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        XElement GetXml(bool suppressDefaults);

    }//IFeatureFilterSettings

}//Namespace
