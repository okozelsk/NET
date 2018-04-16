using System;
using System.Collections.Generic;
using RCNet.Extensions;

namespace RCNet.Neural.Data
{
    /// <summary>
    /// Bundle of pattern and desired output vector
    /// </summary>
    [Serializable]
    public class PatternBundle
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
        /// Instantiates data bundle.
        /// Creates shallow copy of given lists
        /// </summary>
        /// <param name="inputPatternCollection">Collection of input patterns</param>
        /// <param name="outputVectorCollection">Collection of output vectors</param>
        public PatternBundle(List<List<double[]>> inputPatternCollection, List<double[]> outputVectorCollection)
        {
            InputPatternCollection = new List<List<double[]>>(inputPatternCollection);
            OutputVectorCollection = new List<double[]>(outputVectorCollection);
            return;
        }

        /// <summary>
        /// Instantiates data bundle
        /// </summary>
        public PatternBundle()
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

        /// <summary>
        /// Shuffles stored pairs
        /// </summary>
        /// <param name="rand">Random object</param>
        public void Shuffle(System.Random rand)
        {
            List<List<double[]>> l1 = new List<List<double[]>>(InputPatternCollection);
            List<double[]> l2 = new List<double[]>(OutputVectorCollection);
            InputPatternCollection.Clear();
            OutputVectorCollection.Clear();
            int[] shuffledIndices = new int[l2.Count];
            shuffledIndices.ShuffledIndices(rand);
            for (int i = 0; i < shuffledIndices.Length; i++)
            {
                InputPatternCollection.Add(l1[shuffledIndices[i]]);
                OutputVectorCollection.Add(l2[shuffledIndices[i]]);
            }
            return;
        }

    }//PatternBundle

}//Namespace
