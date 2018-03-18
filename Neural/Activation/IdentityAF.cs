using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OKOSW.Neural.Activation
{
    /// <summary>
    /// Linear activation function (returns unchanged input value)
    /// </summary>
    [Serializable]
    public class IdentityAF : IActivationFunction
    {
        public double Compute(double x)
        {
            //The same value
            return x;
        }

        public double ComputeDerivative(double c)
        {
            //Allways 1
            return 1d;
        }

    }
}
