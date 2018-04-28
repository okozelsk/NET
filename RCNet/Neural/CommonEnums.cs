using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.Neural
{
    /// <summary>
    /// Definitions of commonly used neural types and related support functions
    /// </summary>
    public static class CommonEnums
    {
        /// <summary>
        /// Supported task types.
        /// </summary>
        public enum TaskType
        {
            /// <summary>
            /// Prediction task type
            /// </summary>
            Prediction,
            /// <summary>
            /// Classification (Categorization, Pattern recognition) task type
            /// </summary>
            Classification,
            /// <summary>
            /// Prediction task type with input pattern
            /// </summary>
            Hybrid
        }

        /// <summary>
        /// Parses the task type from a string code
        /// </summary>
        /// <param name="code">Task type code</param>
        public static TaskType ParseTaskType(string code)
        {
            switch (code.ToUpper())
            {
                case "PREDICTION": return TaskType.Prediction;
                case "CLASSIFICATION": return TaskType.Classification;
                case "HYBRID": return TaskType.Hybrid;
                default:
                    throw new ArgumentException($"Unsupported task type {code}", "code");
            }
        }

        /// <summary>
        /// Type of neuron's signal
        /// </summary>
        public enum NeuronSignalType
        {
            /// <summary>
            /// Excitatory. Outgoing synapses will allways have (+) sign.
            /// </summary>
            Excitatory,
            /// <summary>
            /// Inhibitory. Outgoing synapses will allways have (-) sign.
            /// </summary>
            Inhibitory
        }

        /// <summary>
        /// Type of input coding
        /// </summary>
        public enum InputCodingType
        {
            /// <summary>
            /// Analog. Input value is used as is.
            /// </summary>
            Analog,
            /// <summary>
            /// Spike train. Input value is converted to a spike train.
            /// </summary>
            SpikeTrain
        }

        /// <summary>
        /// Parses type of input coding from a string code
        /// </summary>
        /// <param name="code">Input coding type code</param>
        public static InputCodingType ParseInputCodingType(string code)
        {
            switch (code.ToUpper())
            {
                case "ANALOG": return InputCodingType.Analog;
                case "SPIKETRAIN": return InputCodingType.SpikeTrain;
                default:
                    throw new ArgumentException($"Unsupported input coding type {code}", "code");
            }
        }


    }//CommonTypes
}//Namespace
