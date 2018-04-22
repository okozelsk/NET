using System;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Analog.Activation
{
    /// <summary>
    /// Sigmoid activation function
    /// </summary>
    [Serializable]
    public class Sigmoid : IAnalogActivationFunction
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
            return 1d / (1d + Math.Exp(-x));
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

    }//Sigmoid

}//Namespace
