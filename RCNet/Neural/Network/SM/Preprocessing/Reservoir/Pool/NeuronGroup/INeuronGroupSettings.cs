using RCNet.Neural.Activation;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron;
using RCNet.RandomValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron.Predictor;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool.NeuronGroup
{
    /// <summary>
    /// Common interface of analog and spking neuron groups settings
    /// </summary>
    public interface INeuronGroupSettings
    {
        /// <summary>
        /// Name of the neuron group
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Type of the activation functions within the group (analog or spiking)
        /// </summary>
        ActivationType Type { get; }

        /// <summary>
        /// Common activation function settings of the groupped neurons
        /// </summary>
        RCNetBaseSettings ActivationCfg { get; }

        /// <summary>
        /// Specifies how big relative portion of pool's neurons is formed by this group of the neurons
        /// </summary>
        double RelShare { get; }

        /// <summary>
        /// Restriction of neuron's output signaling
        /// </summary>
        NeuronCommon.NeuronSignalingRestrictionType SignalingRestriction { get; }

        /// <summary>
        /// Each neuron within the group receives constant input bias. Value of the neuron's bias is driven
        /// by this random settings.
        /// </summary>
        RandomValueSettings BiasCfg { get; }
        
        /// <summary>
        /// Configuration of the predictors
        /// </summary>
        PredictorsSettings PredictorsCfg { get; }

        /// <summary>
        /// Additional helper computed field.
        /// Specifies exact number of neurons from this group within the current context.
        /// </summary>
        int Count { get; set; }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        RCNetBaseSettings DeepClone();

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        XElement GetXml(bool suppressDefaults);

    }//IPoolNeuronGroupSettings

}//Namespace
