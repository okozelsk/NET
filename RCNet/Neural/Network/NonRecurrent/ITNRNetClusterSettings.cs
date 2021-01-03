using System.Collections.Generic;
using System.Xml.Linq;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// The common interface of the cluster configurations.
    /// </summary>
    public interface ITNRNetClusterSettings
    {
        //Properties
        /// <inheritdoc cref="TNRNet.OutputType"/>
        TNRNet.OutputType Output { get; }

        /// <summary>
        /// Gets the list of the cluster's inner networks configuration interfaces.
        /// </summary>
        List<INonRecurrentNetworkSettings> ClusterNetConfigurations { get; }

        /// <summary>
        /// The macro-weight of the group of metrics related to training.
        /// </summary>
        double TrainingGroupWeight { get; }

        /// <summary>
        /// The macro-weight of the group of metrics related to testing.
        /// </summary>
        double TestingGroupWeight { get; }

        /// <summary>
        /// The weight of the number of samples metric.
        /// </summary>
        double SamplesWeight { get; }

        /// <summary>
        /// The weight of the numerical precision metric.
        /// </summary>
        double NumericalPrecisionWeight { get; }

        /// <summary>
        /// The weight of the "misrecognized falses" metric.
        /// </summary>
        /// <remarks>
        /// The metric "misrecognized falses" is relevant only in case of the Probabilistic or SingleBool output types.
        /// </remarks>
        double MisrecognizedFalseWeight { get; }

        /// <summary>
        /// The weight of the "unrecognized trues" metric.
        /// </summary>
        /// <remarks>
        /// The metric "unrecognized trues" is relevant only in case of the Probabilistic or SingleBool output types.
        /// </remarks>
        double UnrecognizedTrueWeight { get; }

        //Methods
        /// <inheritdoc cref="RCNetBaseSettings.DeepClone"/>
        RCNetBaseSettings DeepClone();

        /// <inheritdoc cref="RCNetBaseSettings.GetXml(bool)"/>
        XElement GetXml(bool suppressDefaults);

    }//ITNRNetClusterSettings

}//Namespace
