using System.Xml.Linq;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Common interface of non-recurrent network settings
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
