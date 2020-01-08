using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Contains trained network and related important error statistics associated with the output field.
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
        /// Trained network
        /// </summary>
        public INonRecurrentNetwork Network { get; set; }
        /// <summary>
        /// Informative message from trainer
        /// </summary>
        public string TrainerInfoMessage { get; set; }
        /// <summary>
        /// Training error statistics
        /// </summary>
        public BasicStat TrainingErrorStat { get; set; }
        /// <summary>
        /// Training binary error statistics. Relevant only for Classification task type.
        /// </summary>
        public BinErrStat TrainingBinErrorStat { get; set; }
        /// <summary>
        /// Testing error statistics
        /// </summary>
        public BasicStat TestingErrorStat { get; set; }
        /// <summary>
        /// Testing binary error statistics. Relevant only for Classification task type.
        /// </summary>
        public BinErrStat TestingBinErrorStat { get; set; }
        /// <summary>
        /// Statistics of the network weights
        /// </summary>
        public BasicStat OutputWeightsStat { get; set; }
        /// <summary>
        /// Achieved combined precision error
        /// </summary>
        public double CombinedPrecisionError { get; set; }
        /// <summary>
        /// Achieved combined binary error. Relevant only for Classification task type.
        /// </summary>
        public double CombinedBinaryError { get; set; }

        //Constructors
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        public ReadoutUnit()
        {
            Network = null;
            TrainingErrorStat = null;
            TrainingBinErrorStat = null;
            TestingErrorStat = null;
            TestingBinErrorStat = null;
            OutputWeightsStat = null;
            CombinedPrecisionError = -1;
            CombinedBinaryError = -1;
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public ReadoutUnit(ReadoutUnit source)
        {
            Network = null;
            if (source.Network != null)
            {
                Network = source.Network.DeepClone();
            }
            TrainerInfoMessage = source.TrainerInfoMessage;
            TrainingErrorStat = source.TrainingErrorStat?.DeepClone();
            TrainingBinErrorStat = source.TrainingBinErrorStat?.DeepClone();
            TestingErrorStat = source.TestingErrorStat?.DeepClone();
            TestingBinErrorStat = source.TestingBinErrorStat?.DeepClone();
            OutputWeightsStat = source.OutputWeightsStat?.DeepClone();
            CombinedPrecisionError = source.CombinedPrecisionError;
            CombinedBinaryError = source.CombinedBinaryError;
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
