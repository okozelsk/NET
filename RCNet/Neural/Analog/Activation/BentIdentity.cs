using System;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Analog.Activation
{
    /// <summary>
    /// BentIdentity activation function
    /// </summary>
    [Serializable]
    public class BentIdentity : IAnalogActivationFunction
    {
        //Properties
        /// <summary>
        /// The working range
        /// </summary>
        public Interval Range { get { return new Interval(0, double.PositiveInfinity); } }

        //Methods
        /// <summary>
        /// Computes the result of the activation function
        /// </summary>
        /// <param name="x">Argument</param>
        public double Compute(double x)
        {
            return (Math.Sqrt(x.Power(2) + 1d) - 1d) / 2d + x;
        }

        /// <summary>
        /// Computes the derivation
        /// </summary>
        /// <param name="c">The result of the Compute method</param>
        /// <param name="x">The argument of the Compute method</param>
        public double Derive(double c, double x)
        {
            return x / (2d * Math.Sqrt(x.Power(2) + 1d)) + 1d;
        }

    }//BentIdentity

}//Namespace
