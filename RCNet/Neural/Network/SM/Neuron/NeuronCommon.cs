using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.Neural.Network.SM.Neuron
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

        //Methods
        /// <summary>
        /// Parses neuron role from a string code
        /// </summary>
        /// <param name="code">Neuron role code</param>
        public static NeuronRole ParseNeuronRole(string code)
        {
            switch (code.ToUpper())
            {
                case "INPUT": return NeuronRole.Input;
                case "EXCITATORY": return NeuronRole.Excitatory;
                case "INHIBITORY": return NeuronRole.Inhibitory;
                default:
                    throw new ArgumentException($"Unsupported neuron role {code}", "code");
            }
        }

        /// <summary>
        /// Parses neuron's output signaling restriction
        /// </summary>
        /// <param name="code">Restriction code</param>
        public static NeuronSignalingRestrictionType ParseNeuronSignalingRestriction(string code)
        {
            switch (code.ToUpper())
            {
                case "NORESTRICTION": return NeuronSignalingRestrictionType.NoRestriction;
                case "ANALOGONLY": return NeuronSignalingRestrictionType.AnalogOnly;
                case "SPIKINGONLY": return NeuronSignalingRestrictionType.SpikingOnly;
                default:
                    throw new ArgumentException($"Unsupported neuron output signaling restriction {code}", "code");
            }
        }

    }//NeuronCommon

} //Namespace
