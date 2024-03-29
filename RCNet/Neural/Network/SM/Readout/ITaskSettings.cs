﻿using RCNet.Neural.Data.Filter;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Common interface of the task configurations.
    /// </summary>
    public interface ITaskSettings
    {

        /// <inheritdoc cref="ReadoutUnit.TaskType" />
        ReadoutUnit.TaskType Type { get; }

        /// <summary>
        /// The output feature filter configuration.
        /// </summary>
        IFeatureFilterSettings FeatureFilterCfg { get; }

        /// <inheritdoc cref="RCNetBaseSettings.DeepClone" />
        RCNetBaseSettings DeepClone();

        /// <inheritdoc cref="RCNetBaseSettings.GetXml(bool)" />
        XElement GetXml(bool suppressDefaults);


    }//ITaskSettings

}//Namespace
