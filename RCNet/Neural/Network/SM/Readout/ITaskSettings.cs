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
        /// Output feature filter settings
        /// </summary>
        IFeatureFilterSettings FeatureFilterCfg { get; }

        /// <summary>
        /// Collection of task's associated networks
        /// </summary>
        List<INonRecurrentNetworkSettings> NetworkCfgCollection { get; }

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


    }//ITaskSettings

}//Namespace
