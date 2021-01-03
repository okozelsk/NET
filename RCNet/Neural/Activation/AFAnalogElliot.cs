using RCNet.Extensions;
using RCNet.MathTools;
using System;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements the Elliot (aka Softsign) activation function.
    /// </summary>
    [Serializable]
    public class AFAnalogElliot : AFAnalogBase
    {
        //Attribute properties
        /// <summary>
        /// The curve slope.
        /// </summary>
        public double Slope { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="slope">The slope of the curve.</param>
        public AFAnalogElliot(double slope)
            : base(Interval.IntN1P1)
        {
            Slope = slope.Bound();
            if (Slope <= 0)
            {
                throw new ArgumentOutOfRangeException("slope", "Slope must be GT 0");
            }
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double x)
        {
            x = x.Bound();
            return ((x * Slope) / (1d + Math.Abs(x * Slope))).Bound();
        }

        /// <inheritdoc/>
        public override double ComputeDerivative(double c, double x)
        {
            c = c.Bound();
            return (Slope * 1d) / ((1d + Math.Abs(c * Slope)).Power(2));
        }

    }//AFAnalogElliot

}//Namespace

