using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Common interface of spikes encoding settings
    /// </summary>
    public interface ISpikesEncodingSettings
    {
        /// <summary>
        /// Type of spikes encoding
        /// </summary>
        InputEncoder.SpikesEncodingType EncodingType { get; }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        RCNetBaseSettings DeepClone();

    }//ISpikesEncodingSettings


}//Namespace
