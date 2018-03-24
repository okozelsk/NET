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
    /// Sigmoid activation function
    /// </summary>
    [Serializable]
    public class SigmoidAF : IActivationFunction
    {
        //Properties
        public Interval Range { get { return new Interval(0, 1); } }

        //Methods
        public double Compute(double x)
        {
            return 1d / (1d + Math.Exp(-x)).Bound();
        }

        public double ComputeDerivative(double c, double x = double.NaN)
        {
            return c * (1d - c);
        }

    }//SigmoidAF
}//Namespace

