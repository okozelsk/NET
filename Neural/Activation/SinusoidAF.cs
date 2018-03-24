using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Sinusoid activation function
    /// </summary>
    [Serializable]
    public class SinusoidAF : IActivationFunction
    {
        //Properties
        public Interval Range { get { return new Interval(-1, 1); } }

        //Methods
        public double Compute(double x)
        {
            return Math.Sin(2d * x).Bound();
        }

        public double ComputeDerivative(double c, double x = double.NaN)
        {
            return Math.Cos(2d * c).Bound();
        }

    }//SinusoidAF

}//Namespace

