using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;

namespace RCNet.Neural
{
    /// <summary>
    /// Definitions of commonly used neural types and related support functions
    /// </summary>
    public static class CommonEnums
    {
        /// <summary>
        /// Input feeding variants
        /// </summary>
        public enum InputFeedingType
        {
            /// <summary>
            /// Continuous feeding
            /// </summary>
            Continuous,
            /// <summary>
            /// Patterned feeding
            /// </summary>
            Patterned
        }

        /// <summary>
        /// Types of the neuron's activation function
        /// </summary>
        public enum ActivationType
        {
            /// <summary>
            /// Fires spike only when firing condition is met
            /// </summary>
            Spiking,
            /// <summary>
            /// Produces continuous analog output signal
            /// </summary>
            Analog
        };


        /// <summary>
        /// Supported task types.
        /// </summary>
        public enum TaskType
        {
            /// <summary>
            /// Forecasting of the next value
            /// </summary>
            Forecast,
            /// <summary>
            /// Classification (Categorization, Pattern recognition) task type
            /// </summary>
            Classification
        }

        /// <summary>
        /// Parses the task type from a string code
        /// </summary>
        /// <param name="code">Task type code</param>
        public static TaskType ParseTaskType(string code)
        {
            switch (code.ToUpper())
            {
                case "FORECAST": return TaskType.Forecast;
                case "CLASSIFICATION": return TaskType.Classification;
                default:
                    throw new ArgumentException($"Unsupported task type {code}", "code");
            }
        }

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

        /// <summary>
        /// Method to decide synapse delay
        /// </summary>
        public enum SynapticDelayMethod
        {
            /// <summary>
            /// Synapse delay is decided randomly
            /// </summary>
            Random,
            /// <summary>
            /// Synapse delay is decided according to Euclidean distance
            /// </summary>
            Distance
        }

        /// <summary>
        /// Parses method to decide synapse delay from a string code
        /// </summary>
        /// <param name="code">Method to decide synapse delay code</param>
        public static SynapticDelayMethod ParseSynapticDelayMethod(string code)
        {
            switch (code.ToUpper())
            {
                case "RANDOM": return SynapticDelayMethod.Random;
                case "DISTANCE": return SynapticDelayMethod.Distance;
                default:
                    throw new ArgumentException($"Unsupported synapse delay decision method: {code}", "code");
            }
        }

        /// <summary>
        /// Target scope of the synapse
        /// </summary>
        public enum SynapticTargetScope
        {
            /// <summary>
            /// Both Excitatory and Inhibitory neurons
            /// </summary>
            All,
            /// <summary>
            /// Excitatory neurons only
            /// </summary>
            Excitatory,
            /// <summary>
            /// Inhibitory neurons only
            /// </summary>
            Inhibitory
        }

        /// <summary>
        /// Parses target scope of the synapse from a string code
        /// </summary>
        /// <param name="code">Scope code</param>
        public static SynapticTargetScope ParseSynapticTargetScope(string code)
        {
            switch (code.ToUpper())
            {
                case "ALL": return SynapticTargetScope.All;
                case "EXCITATORY": return SynapticTargetScope.Excitatory;
                case "INHIBITORY": return SynapticTargetScope.Inhibitory;
                default:
                    throw new ArgumentException($"Unsupported synaptic target scope {code}", "code");
            }
        }


    }//CommonEnums
}//Namespace
