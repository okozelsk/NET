using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool
{
    /// <summary>
    /// Common interface of interconnection schema configurations.
    /// </summary>
    public interface IInterconnSchemaSettings
    {
        /// <summary>
        /// Specifies whether the connections of this schema will replace the existing connections.
        /// </summary>
        bool ReplaceExistingConnections { get; }

        /// <summary>
        /// The number of applications of this schema.
        /// </summary>
        int Repetitions { get; }

        /// <inheritdoc cref="RCNetBaseSettings.DeepClone"/>
        RCNetBaseSettings DeepClone();

        /// <inheritdoc cref="RCNetBaseSettings.GetXml(bool)"/>
        XElement GetXml(bool suppressDefaults);

    }//IPoolInterconnectionSchemaSettings

}//Namespace
