using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Common interface of the synapse's efficacy dynamics configurations.
    /// </summary>
    public interface IDynamicsSettings
    {

        /// <inheritdoc cref="PlasticityCommon.DynType"/>
        PlasticityCommon.DynType Type { get; }

        /// <inheritdoc cref="PlasticityCommon.DynApplication"/>
        PlasticityCommon.DynApplication Application { get; }

        /// <inheritdoc cref="RCNetBaseSettings.GetXml(bool)"/>
        XElement GetXml(bool suppressDefaults);

    }//IDynamics

}//Namespace
