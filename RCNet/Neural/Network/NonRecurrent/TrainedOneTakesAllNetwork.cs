using RCNet.MathTools;
using RCNet.Neural.Network.NonRecurrent.FF;
using System;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Contains trained "One Takes All" network and related important error statistics from training/testing.
    /// Network is Feed Forward Network having multiple probability outputs and SoftMax output activation.
    /// </summary>
    [Serializable]
    public class TrainedOneTakesAllNetwork
    {
        //Attribute properties
        /// <summary>
        /// Name of the trained network
        /// </summary>
        public string NetworkName { get; set; }
        /// <summary>
        /// Trained network
        /// </summary>
        public FeedForwardNetwork Network { get; set; }
        /// <summary>
        /// Training error statistics
        /// </summary>
        public BasicStat TrainingErrorStat { get; set; }
        /// <summary>
        /// Training binary error statistics.
        /// </summary>
        public BinErrStat TrainingBinErrorStat { get; set; }
        /// <summary>
        /// Testing error statistics
        /// </summary>
        public BasicStat TestingErrorStat { get; set; }
        /// <summary>
        /// Testing binary error statistics.
        /// </summary>
        public BinErrStat TestingBinErrorStat { get; set; }
        /// <summary>
        /// Statistics of the network weights
        /// </summary>
        public BasicStat OutputWeightsStat { get; set; }
        /// <summary>
        /// Achieved training/testing combined precision error
        /// </summary>
        public double CombinedPrecisionError { get; set; }
        /// <summary>
        /// Achieved training/testing combined binary error.
        /// </summary>
        public double CombinedBinaryError { get; set; }

        //Constructors
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        public TrainedOneTakesAllNetwork()
        {
            NetworkName = string.Empty;
            Network = null;
            TrainingErrorStat = null;
            TrainingBinErrorStat = null;
            TestingErrorStat = null;
            TestingBinErrorStat = null;
            OutputWeightsStat = null;
            CombinedPrecisionError = -1d;
            CombinedBinaryError = -1d;
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public TrainedOneTakesAllNetwork(TrainedOneTakesAllNetwork source)
        {
            NetworkName = source.NetworkName;
            Network = (FeedForwardNetwork)source.Network?.DeepClone();
            TrainingErrorStat = source.TrainingErrorStat?.DeepClone();
            TrainingBinErrorStat = source.TrainingBinErrorStat?.DeepClone();
            TestingErrorStat = source.TestingErrorStat?.DeepClone();
            TestingBinErrorStat = source.TestingBinErrorStat?.DeepClone();
            OutputWeightsStat = source.OutputWeightsStat?.DeepClone();
            CombinedPrecisionError = source.CombinedPrecisionError;
            CombinedBinaryError = source.CombinedBinaryError;
            return;
        }

        //Properties
        /// <summary>
        /// Indicates that the network ideal output is binary
        /// </summary>
        public bool BinaryOutput { get { return true; } }

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public TrainedOneTakesAllNetwork DeepClone()
        {
            return new TrainedOneTakesAllNetwork(this);
        }

    }//TrainedOneTakesAllNetwork

}//Namespace
