using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OKOSW.Extensions;
using OKOSW.MathTools;

namespace OKOSW.Neural.Activation
{
    /// <summary>
    /// Gaussian activation function
    /// </summary>
    [Serializable]
    public class GaussianAF : IActivationFunction
    {
        //Properties
        public Interval Range { get { return new Interval(0, 1); } }

        //Methods
        public double Compute(double x)
        {
            return Math.Exp(-(x.Power(2).Bound()));
        }

        public double ComputeDerivative(double c, double x = double.NaN)
        {
            return -2*x*c;
        }

    }//GaussianAF
}//Namespace

