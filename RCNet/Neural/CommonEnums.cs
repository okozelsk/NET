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



    }//CommonTypes
}//Namespace
