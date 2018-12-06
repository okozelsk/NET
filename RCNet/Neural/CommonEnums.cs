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
        /// Supported data normalization ranges
        /// </summary>
        public enum DataNormalizationRange
        {
            /// <summary>
            /// Range 0;1
            /// </summary>
            Inclusive_0_Pos1,
            /// Range -1;1
            /// </summary>
            Inclusive_Neg1_Pos1
        }

        /// <summary>
        /// Parses the normalization range from a string code
        /// </summary>
        /// <param name="code">Normalization range code</param>
        public static DataNormalizationRange ParseDataNormalizationRange(string code)
        {
            switch (code.ToUpper())
            {
                case "INCLUSIVE_0_POS1": return DataNormalizationRange.Inclusive_0_Pos1;
                case "INCLUSIVE_NEG1_POS1": return DataNormalizationRange.Inclusive_Neg1_Pos1;
                default:
                    throw new ArgumentException($"Unsupported normalization range {code}", "code");
            }
        }

        /// <summary>
        /// Returns interval according to the specified normalization range
        /// </summary>
        /// <param name="code">Normalization range code</param>
        public static Interval GetDataNormalizationRange(DataNormalizationRange range)
        {
            switch (range)
            {
                case DataNormalizationRange.Inclusive_0_Pos1: return new Interval(0, 1);
                case DataNormalizationRange.Inclusive_Neg1_Pos1: return new Interval(-1, 1);
                default:
                    throw new ArgumentException($"Unsupported normalization range {range}", "range");
            }
        }


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
        /// Role of the neuron
        /// </summary>
        public enum NeuronRole
        {
            /// <summary>
            /// Outgoing synapse signal will have sign driven by input data value and target neuron range.
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
