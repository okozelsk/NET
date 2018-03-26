using System;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Sinusoid activation function
    /// See the IActivationFunction.
    /// </summary>
    [Serializable]
    public class SinusoidAF : IActivationFunction
    {
        //Properties
        public Interval Range { get { return new Interval(-1, 1); } }

        //Methods
        public double Compute(double x)
        {
            return Math.Sin(2d * x).Bound();
        }

        public double Derive(double c, double x = double.NaN)
        {
            return Math.Cos(2d * c).Bound();
        }

    }//SinusoidAF

}//Namespace

