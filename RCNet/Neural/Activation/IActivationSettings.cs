using System.Xml.Linq;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Common interface of activation function configurations.
    /// </summary>
    public interface IActivationSettings
    {
        //Properties
        /// <inheritdoc cref="ActivationType"/>
        ActivationType TypeOfActivation { get; }

        /// <inheritdoc cref="RCNetBaseSettings.DeepClone"/>
        RCNetBaseSettings DeepClone();

        /// <inheritdoc cref="RCNetBaseSettings.GetXml(bool)"/>
        XElement GetXml(bool suppressDefaults);

    }//IActivationSettings

}//Namespace
