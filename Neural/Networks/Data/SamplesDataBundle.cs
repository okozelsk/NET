using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OKOSW.Neural.Networks.Data
{
    /// <summary>
    /// Bundle of predictors and desired outputs
    /// </summary>
    [Serializable]
    public class SamplesDataBundle
    {
        //Attributes
        public List<double[]> Inputs { get; }
        public List<double[]> Outputs { get; }

        //Constructors
        public SamplesDataBundle()
        {
            Inputs = new List<double[]>();
            Outputs = new List<double[]>();
            return;
        }

        public SamplesDataBundle(int vectorsCount)
        {
            Inputs = new List<double[]>(vectorsCount);
            Outputs = new List<double[]>(vectorsCount);
            return;
        }

    }//SamplesDataBundle
}//Namespace
