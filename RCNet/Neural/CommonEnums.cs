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
        /// Types of the neuron's output signal
        /// </summary>
        public enum NeuronSignalType
        {
            /// <summary>
            /// Neuron fires spike when firing condition is met
            /// </summary>
            Spike,
            /// <summary>
            /// Neuron has continuous analog output signal
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
        /// Scope of the input synapse
        /// </summary>
        public enum InputSynapseScope
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
        /// Parses scope of the input synapse from a string code
        /// </summary>
        /// <param name="code">Scope code</param>
        public static InputSynapseScope ParseInputSynapseScope(string code)
        {
            switch (code.ToUpper())
            {
                case "ALL": return InputSynapseScope.All;
                case "EXCITATORY": return InputSynapseScope.Excitatory;
                case "INHIBITORY": return InputSynapseScope.Inhibitory;
                default:
                    throw new ArgumentException($"Unsupported scope of the input synapse {code}", "code");
            }
        }


    }//CommonTypes
}//Namespace
