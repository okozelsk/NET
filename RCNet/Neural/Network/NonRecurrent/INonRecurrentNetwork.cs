using RCNet.MathTools;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Common interface of the non-recurrent networks
    /// </summary>
    public interface INonRecurrentNetwork
    {
        //Properties
        /// <summary>
        /// Number of network's input values
        /// </summary>
        int NumOfInputValues { get; }

        /// <summary>
        /// Number of network's output values
        /// </summary>
        int NumOfOutputValues { get; }

        /// <summary>
        /// Total number of network's weights
        /// </summary>
        int NumOfWeights { get; }

        /// <summary>
        /// Output range
        /// </summary>
        Interval OutputRange { get; }


        //Methods
        /// <summary>
        /// Computes the network output values
        /// </summary>
        /// <param name="input">Input values to be passed into the network</param>
        /// <returns>Computed output values</returns>
        double[] Compute(double[] input);

        /// <summary>
        /// Function goes through collection (batch) of the network inputs and for each of them computes the output.
        /// Computed output is then compared with a corresponding ideal output.
        /// The error Abs(ideal - computed) is passed to the result error statistics.
        /// </summary>
        /// <param name="inputCollection">Collection of the network inputs (batch)</param>
        /// <param name="idealOutputCollection">Collection of the ideal outputs (batch)</param>
        /// <param name="computedOutputCollection">Collection of the computed outputs (batch)</param>
        /// <returns>Error statistics</returns>
        BasicStat ComputeBatchErrorStat(List<double[]> inputCollection, List<double[]> idealOutputCollection, out List<double[]> computedOutputCollection);

        /// <summary>
        /// Function goes through collection (batch) of the network inputs and for each of them computes the output.
        /// Computed output is then compared with a corresponding ideal output.
        /// The error Abs(ideal - computed) is passed to the result error statistics.
        /// </summary>
        /// <param name="inputCollection">Collection of the network inputs (batch)</param>
        /// <param name="idealOutputCollection">Collection of the ideal outputs (batch)</param>
        /// <returns>Error statistics</returns>
        BasicStat ComputeBatchErrorStat(List<double[]> inputCollection, List<double[]> idealOutputCollection);

        /// <summary>
        /// Function creates the statistics of the internal weights
        /// </summary>
        BasicStat ComputeWeightsStat();

        /// <summary>
        /// Randomizes internal weights
        /// </summary>
        void RandomizeWeights(Random rand);

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        INonRecurrentNetwork DeepClone();

    }//INonRecurrentNetwork

}//Namespace
