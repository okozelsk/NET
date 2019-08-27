using System;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements Sinusoid activation function
    /// </summary>
    [Serializable]
    public class Sinusoid : AnalogActivationFunction
    {

        //Static members
        private static readonly Interval _outputRange = new Interval(-1, 1);

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public Sinusoid()
            : base()
        {
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
            return Math.Sin(2d * x).Bound();
        }

        /// <summary>
        /// Computes derivative (with respect to x)
        /// </summary>
        /// <param name="c">The result of the activation (Compute method)</param>
        /// <param name="x">Activation input (x argument of the Compute method)</param>
        public override double ComputeDerivative(double c, double x = double.NaN)
        {
            c = c.Bound();
            return Math.Cos(2d * c).Bound();
        }

    }//Sinusoid

}//Namespace
