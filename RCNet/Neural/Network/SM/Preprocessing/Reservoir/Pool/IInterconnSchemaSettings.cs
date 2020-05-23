using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool
{
    /// <summary>
    /// Common interface of interconnection schema settings
    /// </summary>
    public interface IInterconnSchemaSettings
    {
        /// <summary>
        /// Specifies whether the connections of this schema will replace existing connections
        /// </summary>
        bool ReplaceExistingConnections { get; }

        /// <summary>
        /// Number of applications of this schema
        /// </summary>
        int Repetitions { get; }

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

    }//IPoolInterconnectionSchemaSettings

}//Namespace
