using System;
using System.Collections.Generic;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Implements the holder of the computed result and sub-results of the cluster of networks.
    /// </summary>
    [Serializable]
    public class CompositeResult
    {
        /// <summary>
        /// The result.
        /// </summary>
        public double[] Result { get; set; }
        /// <summary>
        /// The sub-results.
        /// </summary>
        public List<double[]> SubResults { get; set; }

        //Constructor
        /// <summary>
        /// Creates an instance.
        /// </summary>
        public CompositeResult(double[] result = null, List<double[]> subResults = null)
        {
            Result = result;
            SubResults = subResults;
            return;
        }

        //Properties
        /// <summary>
        /// Gets the total number of holded double values.
        /// </summary>
        public int FlatLength
        {
            get
            {
                int length = 0;
                if (Result != null)
                {
                    length += Result.Length;
                }
                if (SubResults != null)
                {
                    foreach (double[] subResult in SubResults)
                    {
                        length += subResult.Length;
                    }
                }
                return length;
            }
        }

        //Methods
        /// <summary>
        /// Copies all double values into a flat buffer starting from at the specified index.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="startIndex">The zero-based starting position within the buffer.</param>
        /// <param name="includeResult">Specifies whether to include the result.</param>
        /// <param name="includeSubResults">Specifies whether to include the sub-results.</param>
        /// <returns>Number of copied values</returns>
        public int CopyTo(double[] buffer,
                          int startIndex,
                          bool includeResult = true,
                          bool includeSubResults = true
                          )
        {
            int length = 0;
            if (Result != null && includeResult)
            {
                Result.CopyTo(buffer, startIndex + length);
                length += Result.Length;
            }
            if (SubResults != null && includeSubResults)
            {
                foreach (double[] subResult in SubResults)
                {
                    subResult.CopyTo(buffer, startIndex + length);
                    length += subResult.Length;
                }
            }
            return length;
        }

    }//CompositeResult

}//Namespace
