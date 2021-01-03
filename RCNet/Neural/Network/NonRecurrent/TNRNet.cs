using RCNet.MathTools;
using System;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Implements the holder of trained non-recurrent network and its error statistics.
    /// </summary>
    [Serializable]
    public class TNRNet
    {
        //Enums
        /// <summary>
        /// The type of output.
        /// </summary>
        public enum OutputType
        {
            /// <summary>
            /// The single boolean value (the single probability).
            /// </summary>
            SingleBool,
            /// <summary>
            /// The probability distributed over the several outputs.
            /// </summary>
            Probabilistic,
            /// <summary>
            /// One or more independent real numbers.
            /// </summary>
            Real,
        }

        //Attribute properties
        /// <summary>
        /// The name of the network.
        /// </summary>
        public string Name { get; }

        /// <inheritdoc cref="OutputType"/>
        public OutputType Output { get; }

        /// <summary>
        /// The trained network instance.
        /// </summary>
        public INonRecurrentNetwork Network { get; set; }

        /// <summary>
        /// The informative message from the training.
        /// </summary>
        public string TrainerInfoMessage { get; set; }

        /// <summary>
        /// The precision error statistics of the network on training data.
        /// </summary>
        public BasicStat TrainingErrorStat { get; set; }

        /// <summary>
        /// The binary error statistics of the network on training data.
        /// </summary>
        public BinErrStat TrainingBinErrorStat { get; set; }

        /// <summary>
        /// The precision error statistics of the network on testing data.
        /// </summary>
        public BasicStat TestingErrorStat { get; set; }

        /// <summary>
        /// The binary error statistics of the network on testing data.
        /// </summary>
        public BinErrStat TestingBinErrorStat { get; set; }

        /// <summary>
        /// The statistics of the network's inner weights.
        /// </summary>
        public BasicStat NetworkWeightsStat { get; set; }

        /// <summary>
        /// The combined precision error.
        /// </summary>
        /// <remarks>
        /// The bigger precision error from the training/testing data.
        /// </remarks>
        public double CombinedPrecisionError { get; set; }

        /// <summary>
        /// The combined binary error.
        /// </summary>
        /// <remarks>
        /// The bigger binary error from the training/testing data.
        /// </remarks>
        public double CombinedBinaryError { get; set; }

        //Constructors
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        /// <param name="name">The network name.</param>
        /// <param name="output">The type of output.</param>
        public TNRNet(string name, OutputType output)
        {
            Name = name;
            Output = output;
            Network = null;
            TrainingErrorStat = null;
            TrainingBinErrorStat = null;
            TestingErrorStat = null;
            TestingBinErrorStat = null;
            NetworkWeightsStat = null;
            CombinedPrecisionError = -1d;
            CombinedBinaryError = -1d;
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public TNRNet(TNRNet source)
        {
            Name = source.Name;
            Output = source.Output;
            Network = source.Network?.DeepClone();
            TrainerInfoMessage = source.TrainerInfoMessage;
            TrainingErrorStat = source.TrainingErrorStat?.DeepClone();
            TrainingBinErrorStat = source.TrainingBinErrorStat?.DeepClone();
            TestingErrorStat = source.TestingErrorStat?.DeepClone();
            TestingBinErrorStat = source.TestingBinErrorStat?.DeepClone();
            NetworkWeightsStat = source.NetworkWeightsStat?.DeepClone();
            CombinedPrecisionError = source.CombinedPrecisionError;
            CombinedBinaryError = source.CombinedBinaryError;
            return;
        }

        //Properties
        /// <summary>
        /// Tells whether the binary error statistics are relevant for this network.
        /// </summary>
        public bool HasBinErrorStats { get { return IsBinErrorStatsOutputType(Output); } }

        //Static methods
        /// <summary>
        /// Tests whether the specified type of output is associated with the binary error statistics.
        /// </summary>
        /// <param name="outputType">The type of output.</param>
        public static bool IsBinErrorStatsOutputType(OutputType outputType)
        {
            return (outputType != OutputType.Real);
        }

        /// <summary>
        /// Gets the output data range of the network having specified type of output.
        /// </summary>
        /// <param name="outputType">The type of output.</param>
        public static Interval GetOutputDataRange(OutputType outputType)
        {
            if (outputType == TNRNet.OutputType.Probabilistic)
            {
                return Interval.IntZP1;
            }
            else
            {
                return Interval.IntN1P1;
            }
        }

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance.
        /// </summary>
        public TNRNet DeepClone()
        {
            return new TNRNet(this);
        }

    }//TNRNet

}//Namespace
