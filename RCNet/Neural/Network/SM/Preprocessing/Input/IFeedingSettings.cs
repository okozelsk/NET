using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// The common interface of the input feeding configurations.
    /// </summary>
    public interface IFeedingSettings
    {
        /// <inheritdoc cref="InputEncoder.InputFeedingType"/>
        InputEncoder.InputFeedingType FeedingType { get; }

        /// <inheritdoc cref="RCNetBaseSettings.DeepClone"/>
        RCNetBaseSettings DeepClone();

        /// <inheritdoc cref="RCNetBaseSettings.GetXml(bool)"/>
        XElement GetXml(bool suppressDefaults);

    }//IFeedingSettings


}//Namespace
