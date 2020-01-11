using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;
using RCNet.Neural.Network.NonRecurrent;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Contains cluster of trained networks and related important error statistics associated with readout layer's output field.
    /// </summary>
    [Serializable]
    public class ReadoutUnit
    {
        //Enums
        /// <summary>
        /// Supported task types.
        /// </summary>
        public enum TaskType
        {
            /// <summary>
            /// Forecast
            /// </summary>
            Forecast,
            /// <summary>
            /// Classification
            /// </summary>
            Classification
        }

        //Attribute properties
        /// <summary>
        /// Index of this readout unit within the readout layer
        /// </summary>
        public int Index { get; }
        /// <summary>
        /// Cluster of trained networks and associated error statistics
        /// </summary>
        public TrainedNetworkCluster NetworkCluster { get; }

        //Constructors
        /// <summary>
        /// Creates an itialized instance
        /// </summary>
        /// <param name="index">Index of this readout unit within the readout layer</param>
        /// <param name="networkCluster">Cluster of trained networks and associated error statistics</param>
        public ReadoutUnit(int index, TrainedNetworkCluster networkCluster)
        {
            Index = index;
            NetworkCluster = networkCluster;
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public ReadoutUnit(ReadoutUnit source)
        {
            Index = source.Index;
            NetworkCluster = source.NetworkCluster.DeepClone();
            return;
        }

        //Static methods
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

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public ReadoutUnit DeepClone()
        {
            return new ReadoutUnit(this);
        }

    }//ReadoutUnit
}//Namespace
