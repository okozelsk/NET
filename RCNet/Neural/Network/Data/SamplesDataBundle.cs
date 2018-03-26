using System;
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
        public List<double[]> InputVectorCollection { get; }
        public List<double[]> OutputVectorCollection { get; }

        //Constructors
        public SamplesDataBundle()
        {
            InputVectorCollection = new List<double[]>();
            OutputVectorCollection = new List<double[]>();
            return;
        }

        public SamplesDataBundle(int numOfVectors)
        {
            InputVectorCollection = new List<double[]>(numOfVectors);
            OutputVectorCollection = new List<double[]>(numOfVectors);
            return;
        }

    }//SamplesDataBundle
}//Namespace
