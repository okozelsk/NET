using System;
using System.Collections.Generic;

namespace RCNet.Neural.Data
{
    /// <summary>
    /// Implements the bundle of input, computed and ideal (desired) data vectors.
    /// </summary>
    [Serializable]
    public class ResultBundle
    {
        //Attributes
        /// <summary>
        /// The collection of input vectors.
        /// </summary>
        public List<double[]> InputVectorCollection { get; }

        /// <summary>
        /// The collection of computed vectors.
        /// </summary>
        public List<double[]> ComputedVectorCollection { get; }

        /// <summary>
        /// The collection of ideal vectors.
        /// </summary>
        public List<double[]> IdealVectorCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="inputVectorCollection">The collection of input vectors.</param>
        /// <param name="computedVectorCollection">The collection of computed vectors.</param>
        /// <param name="idealVectorCollection">The collection of ideal vectors.</param>
        public ResultBundle(List<double[]> inputVectorCollection, List<double[]> computedVectorCollection, List<double[]> idealVectorCollection)
        {
            InputVectorCollection = new List<double[]>(inputVectorCollection);
            ComputedVectorCollection = new List<double[]>(computedVectorCollection);
            IdealVectorCollection = new List<double[]>(idealVectorCollection);
            return;
        }

        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        public ResultBundle()
        {
            InputVectorCollection = new List<double[]>();
            ComputedVectorCollection = new List<double[]>();
            IdealVectorCollection = new List<double[]>();
            return;
        }

        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="expectedNumOfRows">The expected number of vector rows.</param>
        public ResultBundle(int expectedNumOfRows)
        {
            InputVectorCollection = new List<double[]>(expectedNumOfRows);
            ComputedVectorCollection = new List<double[]>(expectedNumOfRows);
            IdealVectorCollection = new List<double[]>(expectedNumOfRows);
            return;
        }

        /// <summary>
        /// Adds vectors into the bundle.
        /// </summary>
        /// <param name="inputVector">The input vector.</param>
        /// <param name="computedVector">The computed vector.</param>
        /// <param name="idealVector">The ideal vector.</param>
        public void AddVectors(double[] inputVector, double[] computedVector, double[] idealVector)
        {
            InputVectorCollection.Add(inputVector);
            ComputedVectorCollection.Add(computedVector);
            IdealVectorCollection.Add(idealVector);
            return;
        }


    }//ResultBundle

}//Namespace
