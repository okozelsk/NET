using System;
using System.Collections.Generic;

namespace RCNet.Neural.Data
{
    /// <summary>
    /// Bundle of input, computed and desired ideal vectors
    /// </summary>
    [Serializable]
    public class ResultBundle
    {
        //Attributes
        /// <summary>
        /// Collection of input vectors
        /// </summary>
        public List<double[]> InputVectorCollection { get; }

        /// <summary>
        /// Collection of computed vectors
        /// </summary>
        public List<double[]> ComputedVectorCollection { get; }

        /// <summary>
        /// Collection of ideal vectors (desired values)
        /// </summary>
        public List<double[]> IdealVectorCollection { get; }

        //Constructors
        /// <summary>
        /// Instantiates data bundle.
        /// Creates shallow copy of given lists
        /// </summary>
        /// <param name="inputVectorCollection">Collection of input vectors</param>
        /// <param name="computedVectorCollection">Collection of computed vectors</param>
        /// <param name="idealVectorCollection">Collection of ideal vectors</param>
        public ResultBundle(List<double[]> inputVectorCollection, List<double[]> computedVectorCollection, List<double[]> idealVectorCollection)
        {
            InputVectorCollection = new List<double[]>(inputVectorCollection);
            ComputedVectorCollection = new List<double[]>(computedVectorCollection);
            IdealVectorCollection = new List<double[]>(idealVectorCollection);
            return;
        }

        /// <summary>
        /// Instantiates data bundle
        /// </summary>
        public ResultBundle()
        {
            InputVectorCollection = new List<double[]>();
            ComputedVectorCollection = new List<double[]>();
            IdealVectorCollection = new List<double[]>();
            return;
        }

        /// <summary>
        /// Instantiates data bundle
        /// </summary>
        /// <param name="expectedNumOfRows">Expected number of vector rows</param>
        public ResultBundle(int expectedNumOfRows)
        {
            InputVectorCollection = new List<double[]>(expectedNumOfRows);
            ComputedVectorCollection = new List<double[]>(expectedNumOfRows);
            IdealVectorCollection = new List<double[]>(expectedNumOfRows);
            return;
        }

        /// <summary>
        /// Adds vectors into the bundle
        /// </summary>
        /// <param name="inputVector">Input vector</param>
        /// <param name="computedVector">Computed vector</param>
        /// <param name="idealVector">Ideal vector (desired)</param>
        public void AddVectors(double[] inputVector, double[] computedVector, double[] idealVector)
        {
            InputVectorCollection.Add(inputVector);
            ComputedVectorCollection.Add(computedVector);
            IdealVectorCollection.Add(idealVector);
            return;
        }


    }//ResultBundle

}//Namespace
