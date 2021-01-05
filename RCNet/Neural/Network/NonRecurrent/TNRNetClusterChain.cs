using RCNet.MathTools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Implements the chain of cooperating non-recurrent network clusters.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The chain can contain one or more clusters. When the chain contains more than one cluster
    /// then the next cluster gets except the predictors also the output from the previous cluster.
    /// The pure cross-validation approach is still kept, no one network is tested on the data used
    /// for the training of the network providing the inputs for this network (nor indirectly).
    /// This approach sometime improves the results.
    /// </para>
    /// <para>
    /// As the final result is considered the output of the last cluster in the chain.
    /// </para>
    /// </remarks>
    [Serializable]
    public class TNRNetClusterChain
    {

        //Attribute properties
        /// <summary>
        /// The name of the cluster chain.
        /// </summary>
        public string ChainName { get; }

        /// <inheritdoc cref="TNRNet.OutputType"/>
        public TNRNet.OutputType Output { get; }

        //Attributes
        private readonly List<TNRNetCluster> _clusterCollection;

        //Constructor
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="chainName">The name of the cluster chain.</param>
        /// <param name="outputType">The type of output.</param>
        public TNRNetClusterChain(string chainName, TNRNet.OutputType outputType)
        {
            ChainName = chainName;
            Output = outputType;
            _clusterCollection = new List<TNRNetCluster>();
            return;
        }

        //Properties
        /// <summary>
        /// Indicates the chain is finalized and ready to operate.
        /// </summary>
        public bool Ready { get { return _clusterCollection.Count > 0; } }

        /// <summary>
        /// Gets the number of outputs.
        /// </summary>
        public int NumOfOutputs { get { return Ready ? _clusterCollection.Last().NumOfOutputs : 0; } }

        /// <summary>
        /// Gets the last cluster in the chain.
        /// </summary>
        public TNRNetCluster MainCluster { get { return _clusterCollection.Last(); } }

        /// <summary>
        /// Gets the output data range.
        /// </summary>
        public Interval OutputDataRange { get { return TNRNet.GetOutputDataRange(Output); } }

        //Methods
        /// <summary>
        /// Adds the cluster into the chain.
        /// </summary>
        /// <param name="cluster">The cluster to be added into the chain.</param>
        public void AddCluster(TNRNetCluster cluster)
        {
            _clusterCollection.Add(cluster);
            return;
        }

        /// <summary>
        /// Computes the cluster chain.
        /// </summary>
        /// <param name="inputVector">The input vector.</param>
        /// <param name="subResults">The collection of the main cluster's sub-results.</param>
        public double[] Compute(double[] inputVector, out List<Tuple<int, double[]>> subResults)
        {
            if (!Ready)
            {
                throw new InvalidOperationException("Chain is not ready. There is no cluster in the chain.");
            }
            subResults = null;
            double[] result = null;
            foreach (TNRNetCluster cluster in _clusterCollection)
            {
                if (subResults == null)
                {
                    result = cluster.Compute(inputVector, out subResults);
                }
                else
                {
                    result = cluster.Compute(inputVector, new List<Tuple<int, double[]>>(subResults), out subResults);
                }
            }
            return result;
        }

    }//TNRNetClusterChain

}//Namespace
