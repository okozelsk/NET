using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OKOSW.MathTools;

namespace OKOSW.Neural.Activation
{
    /// <summary>
    /// Linear activation function
    /// </summary>
    [Serializable]
    public class IdentityAF : IActivationFunction
    {
        //Properties
        public Interval Range { get { return new Interval(double.NegativeInfinity, double.PositiveInfinity); } }

        //Methods
        public double Compute(double x)
        {
            //The same value
            return x;
        }

        public double ComputeDerivative(double c, double x = double.NaN)
        {
            //Allways 1
            return 1d;
        }

    }//IdentityAF
}//Namespace
