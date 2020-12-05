using RCNet.Neural.Data.Filter;
using RCNet.Neural.Network.NonRecurrent;
using System.Collections.Generic;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Common interface for classification and forecast tasks settings
    /// </summary>
    public interface ITaskSettings
    {

        /// <summary>
        /// Type of the task
        /// </summary>
        ReadoutUnit.TaskType Type { get; }

        /// <summary>
        /// Output feature filter configuration
        /// </summary>
        IFeatureFilterSettings FeatureFilterCfg { get; }

        /// <summary>
        /// Collection of the configurations of the task's associated networks
        /// </summary>
        List<INonRecurrentNetworkSettings> NetworkCfgCollection { get; }

        /// <inheritdoc cref="RCNetBaseSettings.DeepClone" />
        RCNetBaseSettings DeepClone();

        /// <inheritdoc cref="RCNetBaseSettings.GetXml(bool)" />
        XElement GetXml(bool suppressDefaults);


    }//ITaskSettings

}//Namespace
