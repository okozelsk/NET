using RCNet.Extensions;
using RCNet.MathTools;
using System;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements the ISRU (Inverse Square Root Unit) activation function.
    /// </summary>
    [Serializable]
    public class AFAnalogISRU : AFAnalogBase
    {
        //Attribute properties
        /// <summary>
        /// The Alpha.
        /// </summary>
        public double Alpha { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="alpha">The Alpha.</param>
        public AFAnalogISRU(double alpha)
            : base(null)
        {
            Alpha = alpha.Bound();
            if (Alpha <= 0)
            {
                throw new ArgumentOutOfRangeException("alpha", "Alpha must be GT 0");
            }
            _outputRange = new Interval(-1 / Math.Sqrt(Alpha), 1 / Math.Sqrt(Alpha), false, true);
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double x)
        {
            x = x.Bound();
            return (x / Math.Sqrt(1d + Alpha * x.Power(2))).Bound();
        }

        /// <inheritdoc/>
        public override double ComputeDerivative(double c, double x)
        {
            x = x.Bound();
            return (1d / Math.Sqrt(1d + Alpha * x.Power(2))).Power(3);
        }

    }//AFAnalogISRU

}//Namespace

