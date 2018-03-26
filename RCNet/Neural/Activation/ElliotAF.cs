using System;
using RCNet.MathTools;
using RCNet.Extensions;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Elliot activation function (aka Softsign).
    /// See the IActivationFunction.
    /// </summary>
    [Serializable]
    public class ElliotAF : IActivationFunction
    {
        //Constructor
        public ElliotAF(double slope = 1)
        {
            if (slope <= 0)
            {
                throw new ArgumentOutOfRangeException("slope", "Slope must be GT 0");
            }
            Slope = slope;
            return;
        }

        //Properties
        public Interval Range { get { return new Interval(-1, 1); } }
        public double Slope { get; }

        //Methods
        public double Compute(double x)
        {
            return (x * Slope) / (1d + Math.Abs(x * Slope));
        }

        public double Derive(double c, double x = double.NaN)
        {
            return (Slope * 1d) / ((1d + Math.Abs(c * Slope)).Power(2));
        }

    }//ElliotAF
}//Namespace
