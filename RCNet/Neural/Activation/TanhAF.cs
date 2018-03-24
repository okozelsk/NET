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
    /// Tanh activation function
    /// </summary>
    [Serializable]
    public class TanhAF : IActivationFunction
    {
        //Properties
        public Interval Range { get { return new Interval(-1, 1); } }

        //Methods
        public double Compute(double x)
        {
            //Tanh
            return 2d / (1d + Math.Exp(-2d * x)).Bound() - 1d; //3x faster than Math.Tanh;
        }

        public double ComputeDerivative(double c, double x = double.NaN)
        {
            return 1d - c.Power(2);
        }

    }//TanhAF
}//Namespace

