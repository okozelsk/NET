using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Common interface of input feeding settings
    /// </summary>
    public interface IFeedingSettings
    {
        /// <summary>
        /// Specifies whether to route input fields to readout layer together with other predictors
        /// </summary>
        bool RouteToReadout { get; }

        /// <summary>
        /// Type of input feeding
        /// </summary>
        InputEncoder.InputFeedingType FeedingType { get; }

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

    }//IFeedingSettings


}//Namespace
