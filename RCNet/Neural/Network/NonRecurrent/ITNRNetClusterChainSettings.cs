using System.Collections.Generic;
using System.Xml.Linq;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Common interface of the cluster chain configurations.
    /// </summary>
    public interface ITNRNetClusterChainSettings
    {
        //Properties
        /// <inheritdoc cref="CrossvalidationSettings"/>
        CrossvalidationSettings CrossvalidationCfg { get; }

        /// <inheritdoc cref="TNRNet.OutputType"/>
        TNRNet.OutputType Output { get; }

        /// <summary>
        /// Gets the list of the cluster configuration interfaces.
        /// </summary>
        List<ITNRNetClusterSettings> ClusterCfgCollection { get; }

        //Methods
        /// <inheritdoc cref="RCNetBaseSettings.DeepClone"/>
        RCNetBaseSettings DeepClone();

        /// <inheritdoc cref="RCNetBaseSettings.GetXml(bool)"/>
        XElement GetXml(bool suppressDefaults);

    }//ITNRNetClusterChainSettings

}//Namespace
