using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OKOSW.Extensions;

namespace OKOSW.Neural.Activation
{
    /// <summary>
    /// Tanh activation function
    /// </summary>
    [Serializable]
    public class TanhAF : IActivationFunction
    {
        public double Compute(double x)
        {
            //Tanh
            return 2d / (1d + Math.Exp(-2d * x).Bound()) - 1d; //3x faster than Math.Tanh;
        }

        public double ComputeDerivative(double c)
        {
            return 1d - c.Power(2);
        }

    }
}
