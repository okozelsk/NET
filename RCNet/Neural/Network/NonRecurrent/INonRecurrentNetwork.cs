using RCNet.MathTools;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// The common interface of the non-recurrent networks.
    /// </summary>
    public interface INonRecurrentNetwork
    {
        //Properties
        /// <summary>
        /// The number of network's input values.
        /// </summary>
        int NumOfInputValues { get; }

        /// <summary>
        /// The number of network's output values.
        /// </summary>
        int NumOfOutputValues { get; }

        /// <summary>
        /// The number of network's internal weights.
        /// </summary>
        int NumOfWeights { get; }

        /// <summary>
        /// The output range of the network.
        /// </summary>
        Interval OutputRange { get; }


        //Methods
        /// <summary>
        /// Computes the network output.
        /// </summary>
        /// <param name="input">The input values to be passed into the network.</param>
        /// <returns>The computed values.</returns>
        double[] Compute(double[] input);

        /// <summary>
        /// Computes the batch error statistics.
        /// </summary>
        /// <remarks>
        /// Method goes through the batch of the inputs and for each input computes the output.
        /// Computed output is then compared with a corresponding ideal output.
        /// The resulting error statistics is updated by the error = Abs(ideal - computed).
        /// </remarks>
        /// <param name="inputCollection">The collection of the input vectors (input).</param>
        /// <param name="idealOutputCollection">The collection of the ideal output vectors (ideal).</param>
        /// <param name="computedOutputCollection">The collection of the computed output vectors (computed).</param>
        /// <returns>The resulting error statistics.</returns>
        BasicStat ComputeBatchErrorStat(List<double[]> inputCollection, List<double[]> idealOutputCollection, out List<double[]> computedOutputCollection);

        /// <summary>
        /// Computes the batch error statistics.
        /// </summary>
        /// <remarks>
        /// Method goes through the batch of the inputs and for each input computes the output.
        /// Computed output is then compared with a corresponding ideal output.
        /// The resulting error statistics is updated by the error = Abs(ideal - computed).
        /// </remarks>
        /// <param name="inputCollection">The collection of the input vectors (input).</param>
        /// <param name="idealOutputCollection">The collection of the ideal output vectors (ideal).</param>
        /// <returns>The resulting error statistics.</returns>
        BasicStat ComputeBatchErrorStat(List<double[]> inputCollection, List<double[]> idealOutputCollection);

        /// <summary>
        /// Creates the statistics of the internal weights.
        /// </summary>
        BasicStat ComputeWeightsStat();

        /// <summary>
        /// Randomizes the internal weights.
        /// </summary>
        void RandomizeWeights(Random rand);

        /// <summary>
        /// Creates the deep copy instance of this instance.
        /// </summary>
        INonRecurrentNetwork DeepClone();

    }//INonRecurrentNetwork

}//Namespace
