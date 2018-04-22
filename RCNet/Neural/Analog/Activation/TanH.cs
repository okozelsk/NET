using System;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Analog.Activation
{
    /// <summary>
    /// TanH activation function
    /// </summary>
    [Serializable]
    public class TanH : IAnalogActivationFunction
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
            return 2d / (1d + Math.Exp(-2d * x)).Bound() - 1d; //Faster than Math.Tanh;
        }

        /// <summary>
        /// Computes the derivation
        /// </summary>
        /// <param name="c">The result of the Compute method</param>
        /// <param name="x">The argument of the Compute method</param>
        public double Derive(double c, double x = double.NaN)
        {
            return 1d - c.Power(2);
        }

    }//TanH

}//Namespace
