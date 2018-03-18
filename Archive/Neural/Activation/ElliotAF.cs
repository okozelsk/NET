using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OKOSW.Extensions;

namespace OKOSW.Neural.Activation
{
    /// <summary>
    /// Elliot activation function
    /// </summary>
    [Serializable]
    public class ElliotAF : IActivationFunction
    {
        public double Slope { get; set; }

        public ElliotAF(double slope = 1)
        {
            Slope = slope;
            return;
        }

        public double Compute(double x)
        {
            return (x * Slope) / (1d + Math.Abs(x * Slope));
        }

        public double ComputeDerivative(double c)
        {
            return (Slope * 1d) / ((1d + Math.Abs(c * Slope)).Power(2));
        }

    }
}
