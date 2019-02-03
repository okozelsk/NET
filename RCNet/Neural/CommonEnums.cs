﻿using System;
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

        /// <summary>
        /// Method to find good lambda parameter of ridge regression
        /// </summary>
        public enum RidgeRegressionLambdaMethod
        {
            /// <summary>
            /// Searching for lambda will use halfway interval method
            /// </summary>
            HalfwayInterval,
            /// <summary>
            /// Searching for lambda will use constant intervals
            /// </summary>
            ConstInterval
        }

        /// <summary>
        /// Parses method to decide ridge regression lambda from a string code
        /// </summary>
        /// <param name="code">Method to decide ridge regression lambda</param>
        public static RidgeRegressionLambdaMethod ParseRidgeRegressionLambdaMethod(string code)
        {
            switch (code.ToUpper())
            {
                case "HALFWAYINTERVAL": return RidgeRegressionLambdaMethod.HalfwayInterval;
                case "CONSTINTERVAL": return RidgeRegressionLambdaMethod.ConstInterval;
                default:
                    throw new ArgumentException($"Unsupported method to decide ridge regression lambda: {code}", "code");
            }
        }

    }//CommonTypes
}//Namespace
