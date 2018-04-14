using System;
using RCNet.MathTools;
using RCNet.Extensions;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// ISRU (Inverse Square Root Unit) activation function
    /// </summary>
    [Serializable]
    public class ISRU : IActivationFunction
    {
        //Constructor
        /// <summary>
        /// Instantiates an Inverse Square Root Unit activation function
        /// </summary>
        /// <param name="alpha">The Alpha</param>
        public ISRU(double alpha = 1)
        {
            if(alpha <= 0)
            {
                throw new ArgumentOutOfRangeException("alpha", "Alpha must be GT 0");
            }
            Alpha = alpha;
            return;
        }

        //Properties
        /// <summary>
        /// The working range
        /// </summary>
        public Interval Range { get { return new Interval(-1 / Math.Sqrt(Alpha), 1 / Math.Sqrt(Alpha)); } }

        /// <summary>
        /// The Alpha
        /// </summary>
        public double Alpha { get; }

        //Methods
        /// <summary>
        /// Computes the result of the activation function
        /// </summary>
        /// <param name="x">Argument</param>
        public double Compute(double x)
        {
            return x / (1d + Alpha * x.Power(2));
        }

        /// <summary>
        /// Computes the derivation
        /// </summary>
        /// <param name="c">The result of the Compute method</param>
        /// <param name="x">The argument of the Compute method</param>
        public double Derive(double c, double x)
        {
            return (1d / Math.Sqrt(1d + Alpha * x.Power(2))).Power(3);
        }

    }//ISRU

}//Namespace

