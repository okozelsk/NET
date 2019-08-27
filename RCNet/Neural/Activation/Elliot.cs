using System;
using RCNet.MathTools;
using RCNet.Extensions;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements Elliot (aka Softsign) activation function
    /// </summary>
    [Serializable]
    public class Elliot : AnalogActivationFunction
    {
        //Constants

        //Static members
        private static readonly Interval _outputRange = new Interval(-1, 1);

        //Attribute properties
        /// <summary>
        /// The curve slope
        /// </summary>
        public double Slope { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="slope">Slope of the curve</param>
        public Elliot(double slope)
            : base()
        {
            Slope = slope.Bound();
            if (Slope <= 0)
            {
                throw new ArgumentOutOfRangeException("slope", "Slope must be GT 0");
            }
            return;
        }

        //Properties
        /// <summary>
        /// Output range of the Compute method
        /// </summary>
        public override Interval OutputRange { get { return _outputRange; } }

        //Methods
        /// <summary>
        /// Computes output of the activation function
        /// </summary>
        /// <param name="x">Activation input</param>
        public override double Compute(double x)
        {
            x = x.Bound();
            return ((x * Slope) / (1d + Math.Abs(x * Slope))).Bound();
        }

        /// <summary>
        /// Computes derivative (with respect to x)
        /// </summary>
        /// <param name="c">The result of the activation (Compute method)</param>
        /// <param name="x">Activation input (x argument of the Compute method)</param>
        public override double ComputeDerivative(double c, double x)
        {
            c = c.Bound();
            return (Slope * 1d) / ((1d + Math.Abs(c * Slope)).Power(2));
        }

    }//Elliot

}//Namespace

