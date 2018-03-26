using System;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Sigmoid activation function (aka Logistic, Softstep)
    /// See the IActivationFunction.
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

        public double Derive(double c, double x = double.NaN)
        {
            return c * (1d - c);
        }

    }//SigmoidAF
}//Namespace

