using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OKOSW.Extensions;

namespace OKOSW.Neural.Activation
{
    /// <summary>
    /// Sinusoid activation function
    /// </summary>
    [Serializable]
    public class SinusoidAF : IActivationFunction
    {
        public double Compute(double x)
        {
            return Math.Sin(2d * x).Bound();
        }

        public double ComputeDerivative(double c)
        {
            return Math.Cos(2d * c).Bound();
        }
    }
}
