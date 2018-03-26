using System;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Gaussian activation function
    /// See the IActivationFunction.
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

        public double Derive(double c, double x = double.NaN)
        {
            return -2*x*c;
        }

    }//GaussianAF
}//Namespace

