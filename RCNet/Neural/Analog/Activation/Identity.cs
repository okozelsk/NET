using System;
using RCNet.MathTools;
using RCNet.Extensions;

namespace RCNet.Neural.Analog.Activation
{
    /// <summary>
    /// Identity activation function (aka Linear)
    /// See the IActivationFunction.
    /// </summary>
    [Serializable]
    public class Identity : IAnalogActivationFunction
    {
        //Properties
        /// <summary>
        /// The working range
        /// </summary>
        public Interval Range { get { return new Interval(double.NegativeInfinity.Bound(), double.PositiveInfinity.Bound()); } }

        //Methods
        /// <summary>
        /// Computes the result of the activation function
        /// </summary>
        /// <param name="x">Argument</param>
        public double Compute(double x)
        {
            //The same value
            return x;
        }

        /// <summary>
        /// Computes the derivation
        /// </summary>
        /// <param name="c">The result of the Compute method</param>
        /// <param name="x">The argument of the Compute method</param>
        public double Derive(double c, double x = double.NaN)
        {
            //Allways 1
            return 1d;
        }

    }//Identity

}//Namespace

