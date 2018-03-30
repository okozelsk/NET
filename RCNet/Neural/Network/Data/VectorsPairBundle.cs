using System;
using System.Collections.Generic;

namespace RCNet.Neural.Network.Data
{
    /// <summary>
    /// Bundle of input vector and desired output vector
    /// </summary>
    [Serializable]
    public class VectorsPairBundle
    {
        //Attributes
        /// <summary>
        /// Collection of input vectors
        /// </summary>
        public List<double[]> InputVectorCollection { get; }
        
        /// <summary>
        /// Collection of output vectors (desired values)
        /// </summary>
        public List<double[]> OutputVectorCollection { get; }

        //Constructors
        /// <summary>
        /// Instantiates data bundle
        /// </summary>
        public VectorsPairBundle()
        {
            InputVectorCollection = new List<double[]>();
            OutputVectorCollection = new List<double[]>();
            return;
        }

        /// <summary>
        /// Instantiates data bundle
        /// </summary>
        /// <param name="expectedNumOfPairs">Expected number of sample pairs</param>
        public VectorsPairBundle(int expectedNumOfPairs)
        {
            InputVectorCollection = new List<double[]>(expectedNumOfPairs);
            OutputVectorCollection = new List<double[]>(expectedNumOfPairs);
            return;
        }

        /// <summary>
        /// Adds sample data pair into the bundle
        /// </summary>
        /// <param name="inputVector">Input vector</param>
        /// <param name="outputVector">Output vector (ideal)</param>
        public void AddPair(double[] inputVector, double[] outputVector)
        {
            InputVectorCollection.Add(inputVector);
            OutputVectorCollection.Add(outputVector);
            return;
        }


    }//VectorsPairBundle

}//Namespace
