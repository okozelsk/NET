using System;
using RCNet.MathTools;
using RCNet.Extensions;

namespace RCNet.Neural.Analog.Activation
{
    /// <summary>
    /// Elliot activation function (aka Softsign).
    /// </summary>
    [Serializable]
    public class Elliot : IAnalogActivationFunction
    {
        //Constructor
        /// <summary>
        /// Instantiates an Elliot activation function
        /// </summary>
        /// <param name="slope">The curve slope</param>
        public Elliot(double slope = 1)
        {
            if (slope <= 0)
            {
                throw new ArgumentOutOfRangeException("slope", "Slope must be GT 0");
            }
            Slope = slope;
            return;
        }

        //Properties
        /// <summary>
        /// THe working range
        /// </summary>
        public Interval Range { get { return new Interval(-1, 1); } }

        /// <summary>
        /// The curve slope
        /// </summary>
        public double Slope { get; }

        //Methods
        /// <summary>
        /// Computes the result of the activation function
        /// </summary>
        /// <param name="x">Argument</param>
        public double Compute(double x)
        {
            return (x * Slope) / (1d + Math.Abs(x * Slope));
        }

        /// <summary>
        /// Computes the derivation
        /// </summary>
        /// <param name="c">The result of the Compute method</param>
        /// <param name="x">The argument of the Compute method</param>
        public double Derive(double c, double x = double.NaN)
        {
            return (Slope * 1d) / ((1d + Math.Abs(c * Slope)).Power(2));
        }

    }//Elliot

}//Namespace

