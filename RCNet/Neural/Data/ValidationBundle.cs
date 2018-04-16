using System;
using System.Collections.Generic;

namespace RCNet.Neural.Data
{
    /// <summary>
    /// Bundle of computed vector and desired ideal vector
    /// </summary>
    [Serializable]
    public class ValidationBundle
    {
        //Attributes
        /// <summary>
        /// Collection of input vectors
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
        /// <param name="computedVectorCollection">Collection of computed vectors</param>
        /// <param name="idealVectorCollection">Collection of ideal vectors</param>
        public ValidationBundle(List<double[]> computedVectorCollection, List<double[]> idealVectorCollection)
        {
            ComputedVectorCollection = new List<double[]>(computedVectorCollection);
            IdealVectorCollection = new List<double[]>(idealVectorCollection);
            return;
        }

        /// <summary>
        /// Instantiates data bundle
        /// </summary>
        public ValidationBundle()
        {
            ComputedVectorCollection = new List<double[]>();
            IdealVectorCollection = new List<double[]>();
            return;
        }

        /// <summary>
        /// Instantiates data bundle
        /// </summary>
        /// <param name="expectedNumOfPairs">Expected number of vector pairs</param>
        public ValidationBundle(int expectedNumOfPairs)
        {
            ComputedVectorCollection = new List<double[]>(expectedNumOfPairs);
            IdealVectorCollection = new List<double[]>(expectedNumOfPairs);
            return;
        }

        /// <summary>
        /// Adds vector pair into the bundle
        /// </summary>
        /// <param name="computedVector">Computed vector</param>
        /// <param name="idealVector">Ideal vector (desired)</param>
        public void AddPair(double[] computedVector, double[] idealVector)
        {
            ComputedVectorCollection.Add(computedVector);
            IdealVectorCollection.Add(idealVector);
            return;
        }


    }//ValidationBundle

}//Namespace
