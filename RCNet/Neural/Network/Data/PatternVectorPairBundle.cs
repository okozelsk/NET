using System;
using System.Collections.Generic;

namespace RCNet.Neural.Network.Data
{
    /// <summary>
    /// Bundle of pattern and desired output vector
    /// </summary>
    [Serializable]
    public class PatternVectorPairBundle
    {
        //Attributes
        /// <summary>
        /// Collection of input patterns
        /// </summary>
        public List<List<double[]>> InputPatternCollection { get; }
        /// <summary>
        /// Collection of output vectors (desired values)
        /// </summary>
        public List<double[]> OutputVectorCollection { get; }

        //Constructors
        /// <summary>
        /// Instantiates data bundle
        /// </summary>
        public PatternVectorPairBundle()
        {
            InputPatternCollection = new List<List<double[]>>();
            OutputVectorCollection = new List<double[]>();
            return;
        }

        /// <summary>
        /// Adds pattern/vector pair into the bundle
        /// </summary>
        /// <param name="pattern">Input pattern of vectors</param>
        /// <param name="outputVector">Output vector (ideal)</param>
        public void AddPair(List<double[]> pattern, double[] outputVector)
        {
            InputPatternCollection.Add(pattern);
            OutputVectorCollection.Add(outputVector);
            return;
        }

    }//PatternVectorPairBundle

}//Namespace
