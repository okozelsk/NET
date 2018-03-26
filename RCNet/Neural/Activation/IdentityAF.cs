using System;
using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Identity activation function (aka Linear)
    /// See the IActivationFunction.
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

        public double Derive(double c, double x = double.NaN)
        {
            //Allways 1
            return 1d;
        }

    }//IdentityAF
}//Namespace
