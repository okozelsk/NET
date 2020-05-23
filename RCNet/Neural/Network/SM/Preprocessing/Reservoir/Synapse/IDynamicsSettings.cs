using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Common interface of synapse's dynamics settings
    /// </summary>
    public interface IDynamicsSettings
    {

        /// <summary>
        /// Type of synapse's dynamics
        /// </summary>
        PlasticityCommon.DynType Type { get; }

        /// <summary>
        /// Application (purpose) of the synapse's dynamics
        /// </summary>
        PlasticityCommon.DynApplication Application { get; }

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        XElement GetXml(bool suppressDefaults);

    }//IDynamics

}//Namespace
