﻿using System;
using System.Collections.Generic;

namespace RCNet.Neural.Network.Data
{
    /// <summary>
    /// Bundle of the input vectors and desired output vectors collections
    /// </summary>
    [Serializable]
    public class SamplesDataBundle
    {
        //Attributes
        /// <summary>
        /// Collection of input vectors
        /// </summary>
        public List<double[]> InputVectorCollection { get; }
        /// <summary>
        /// Collection of output vectors (of desired values)
        /// </summary>
        public List<double[]> OutputVectorCollection { get; }

        //Constructors
        /// <summary>
        /// Instantiates data bundle
        /// </summary>
        public SamplesDataBundle()
        {
            InputVectorCollection = new List<double[]>();
            OutputVectorCollection = new List<double[]>();
            return;
        }

        /// <summary>
        /// Instantiates data bundle
        /// </summary>
        public SamplesDataBundle(int numOfVectors)
        {
            InputVectorCollection = new List<double[]>(numOfVectors);
            OutputVectorCollection = new List<double[]>(numOfVectors);
            return;
        }

    }//SamplesDataBundle
}//Namespace
