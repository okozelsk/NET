using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron
{
    /// <summary>
    /// Enums and parsings of enum values for a Neuron type
    /// </summary>
    public static class NeuronCommon
    {
        //Enums
        /// <summary>
        /// Role of the neuron
        /// </summary>
        public enum NeuronRole
        {
            /// <summary>
            /// Outgoing synapse signal will have sign driven by input data value.
            /// </summary>
            Input,
            /// <summary>
            /// Excitatory. Outgoing synapse signal will allways have (+) sign.
            /// </summary>
            Excitatory,
            /// <summary>
            /// Inhibitory. Outgoing synapse signal will allways have (-) sign.
            /// </summary>
            Inhibitory
        }

        /// <summary>
        /// Output signaling restriction of the neuron
        /// </summary>
        public enum NeuronSignalingRestrictionType
        {
            /// <summary>
            /// Neuron can emitt both analog signal and spikes.
            /// </summary>
            NoRestriction,
            /// <summary>
            /// Neuron can emitt only analog signal.
            /// </summary>
            AnalogOnly,
            /// <summary>
            /// Neuron can emitt only spiking signal.
            /// </summary>
            SpikingOnly
        }

        /// <summary>
        /// Type of weights used by moving weighted average neuron's predictors
        /// </summary>
        public enum NeuronPredictorMWAvgWeightsType
        {
            /// <summary>
            /// Exponential weights.
            /// </summary>
            Exponential,
            /// <summary>
            /// Linear weigths.
            /// </summary>
            Linear,
            /// <summary>
            /// Constant weights.
            /// </summary>
            Constant
        }


    }//NeuronCommon

} //Namespace
