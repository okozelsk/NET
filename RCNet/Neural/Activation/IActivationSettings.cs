using System.Xml.Linq;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// The common interface of all configurations of the activation functions.
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
