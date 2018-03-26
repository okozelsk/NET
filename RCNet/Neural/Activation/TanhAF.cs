using System;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// TanH activation function
    /// See the IActivationFunction.
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
            return 2d / (1d + Math.Exp(-2d * x)).Bound() - 1d; //Faster than Math.Tanh;
        }

        public double Derive(double c, double x = double.NaN)
        {
            return 1d - c.Power(2);
        }

    }//TanhAF

}//Namespace

