using System.Xml.Linq;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// The common interface of the non-recurrent network configurations.
    /// </summary>
    public interface INonRecurrentNetworkSettings
    {
        //Properties
        /// <inheritdoc cref="RCNetBaseSettings.DeepClone"/>
        RCNetBaseSettings DeepClone();

        /// <inheritdoc cref="RCNetBaseSettings.GetXml(bool)"/>
        XElement GetXml(bool suppressDefaults);


    }//INonRecurrentNetworkSettings

}//Namespace
