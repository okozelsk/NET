using System;
using RCNet.MathTools;
using RCNet.Extensions;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// LeakyReLU activation function
    /// </summary>
    [Serializable]
    public class LeakyReLU : IActivationFunction
    {
        //Constructor
        /// <summary>
        /// Instantiates a LeakyReLU activation function
        /// </summary>
        /// <param name="negSlope">The negative half-line slope</param>
        public LeakyReLU(double negSlope = 0.01)
        {
            if (negSlope < 0)
            {
                throw new ArgumentOutOfRangeException("negSlope", "negSlope must be GE 0");
            }
            NegSlope = negSlope;
            return;
        }

        //Properties
        /// <summary>
        /// The working range
        /// </summary>
        public Interval Range
        {
            get
            {
                if (NegSlope > 0)
                {
                    return new Interval(double.NegativeInfinity.Bound(), double.PositiveInfinity.Bound());
                }
                else
                {
                    return new Interval(0, double.PositiveInfinity.Bound());
                }
            }
        }

        /// <summary>
        /// The curve slope
        /// </summary>
        public double NegSlope { get; }

        //Methods
        /// <summary>
        /// Computes the result of the activation function
        /// </summary>
        /// <param name="x">Argument</param>
        public double Compute(double x)
        {
            if(x < 0)
            {
                return NegSlope * x;
            }
            else
            {
                return x;
            }
        }

        /// <summary>
        /// Computes the derivation
        /// </summary>
        /// <param name="c">The result of the Compute method</param>
        /// <param name="x">The argument of the Compute method</param>
        public double Derive(double c, double x)
        {
            if (x < 0)
            {
                return NegSlope;
            }
            else
            {
                return 1;
            }
        }

    }//LeakyReLU

}//Namespace

