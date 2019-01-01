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
            /// <summary>
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
        /// <param name="range">Normalization range</param>
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
                case "PREDICTION": return TaskType.Prediction;
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

    }//CommonTypes
}//Namespace
