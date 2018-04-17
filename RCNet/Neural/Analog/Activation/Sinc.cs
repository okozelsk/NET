using System;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Analog.Activation
{
    /// <summary>
    /// Sinc activation function
    /// </summary>
    [Serializable]
    public class Sinc : IActivationFunction
    {
        //Properties
        /// <summary>
        /// The working range
        /// </summary>
        public Interval Range { get { return new Interval(-0.217234, 1); } }

        //Methods
        /// <summary>
        /// Computes the result of the activation function
        /// </summary>
        /// <param name="x">Argument</param>
        public double Compute(double x)
        {
            if (x == 0)
            {
                return 1d;
            }
            else
            {
                return Math.Sin(x) / x;
            }
        }

        /// <summary>
        /// Computes the derivation
        /// </summary>
        /// <param name="c">The result of the Compute method</param>
        /// <param name="x">The argument of the Compute method</param>
        public double Derive(double c, double x)
        {
            if (x == 0)
            {
                return 0;
            }
            else
            {
                return (Math.Cos(x) / x) / (Math.Sin(x) / x.Power(2));
            }
        }

    }//Sinc

}//Namespace
