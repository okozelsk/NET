using RCNet.Neural.Activation;
using RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor;
using RCNet.RandomValue;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool.NeuronGroup
{
    /// <summary>
    /// Common interface of neuron group configurations.
    /// </summary>
    public interface INeuronGroupSettings
    {
        /// <summary>
        /// The name of the group of neurons.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The configuration of the neurons' activation function.
        /// </summary>
        IActivationSettings ActivationCfg { get; }

        /// <summary>
        /// Specifies how big relative portion of pool's neurons is formed by this group of the neurons.
        /// </summary>
        double RelShare { get; }

        /// <summary>
        /// The configuration of the constant input bias.
        /// </summary>
        RandomValueSettings BiasCfg { get; }

        /// <summary>
        /// The common configuration of the predictors provider.
        /// </summary>
        PredictorsProviderSettings PredictorsCfg { get; }

        /// <summary>
        /// The computed field specifying the exact number of neurons from this group within the pool.
        /// </summary>
        int Count { get; set; }

        /// <inheritdoc cref="RCNetBaseSettings.DeepClone"/>
        RCNetBaseSettings DeepClone();

        /// <inheritdoc cref="RCNetBaseSettings.GetXml(bool)"/>
        XElement GetXml(bool suppressDefaults);

    }//INeuronGroupSettings

}//Namespace
