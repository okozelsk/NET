using System;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Analog.Activation
{
    /// <summary>
    /// Sinusoid activation function
    /// </summary>
    [Serializable]
    public class Sinusoid : IAnalogActivationFunction
    {
        //Properties
        /// <summary>
        /// The working range
        /// </summary>
        public Interval Range { get { return new Interval(-1, 1); } }

        //Methods
        /// <summary>
        /// Computes the result of the activation function
        /// </summary>
        /// <param name="x">Argument</param>
        public double Compute(double x)
        {
            return Math.Sin(2d * x).Bound();
        }

        /// <summary>
        /// Computes the derivation
        /// </summary>
        /// <param name="c">The result of the Compute method</param>
        /// <param name="x">The argument of the Compute method</param>
        public double Derive(double c, double x = double.NaN)
        {
            return Math.Cos(2d * c).Bound();
        }

    }//Sinusoid

}//Namespace
