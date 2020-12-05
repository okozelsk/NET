using RCNet.Neural.Activation;
using RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor;
using RCNet.RandomValue;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool.NeuronGroup
{
    /// <summary>
    /// Common interface of neuron groups configurations
    /// </summary>
    public interface INeuronGroupSettings
    {
        /// <summary>
        /// Name of the neuron group
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Activation function configuration of the neurons within the group
        /// </summary>
        IActivationSettings ActivationCfg { get; }

        /// <summary>
        /// Specifies how big relative portion of pool's neurons is formed by this group of the neurons
        /// </summary>
        double RelShare { get; }

        /// <summary>
        /// Each neuron within the group receives constant input bias. Value of the neuron's bias is driven
        /// by this random settings.
        /// </summary>
        RandomValueSettings BiasCfg { get; }

        /// <summary>
        /// Configuration of the predictors
        /// </summary>
        PredictorsProviderSettings PredictorsCfg { get; }

        /// <summary>
        /// Additional helper computed field.
        /// Specifies exact number of neurons from this group within the current context.
        /// </summary>
        int Count { get; set; }

        /// <inheritdoc cref="RCNetBaseSettings.DeepClone"/>
        RCNetBaseSettings DeepClone();

        /// <inheritdoc cref="RCNetBaseSettings.GetXml(bool)"/>
        XElement GetXml(bool suppressDefaults);

    }//INeuronGroupSettings

}//Namespace
