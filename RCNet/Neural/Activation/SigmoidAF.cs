using System;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Sigmoid activation function (aka Logistic, Softstep)
    /// </summary>
    [Serializable]
    public class SigmoidAF : IActivationFunction
    {
        //Properties
        /// <summary>
        /// The working range
        /// </summary>
        public Interval Range { get { return new Interval(0, 1); } }

        //Methods
        /// <summary>
        /// Computes the result of the activation function
        /// </summary>
        /// <param name="x">Argument</param>
        public double Compute(double x)
        {
            return 1d / (1d + Math.Exp(-x)).Bound();
        }

        /// <summary>
        /// Computes the derivation
        /// </summary>
        /// <param name="c">The result of the Compute method</param>
        /// <param name="x">The argument of the Compute method</param>
        public double Derive(double c, double x = double.NaN)
        {
            return c * (1d - c);
        }

    }//SigmoidAF

}//Namespace
