using System;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements SoftPlus activation function
    /// </summary>
    [Serializable]
    public class SoftPlus : AnalogActivationFunction
    {
        //Attributes
        private static readonly Interval _outputRange = new Interval(0, double.PositiveInfinity.Bound());

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public SoftPlus()
            : base()
        {
            return;
        }

        //Properties
        /// <summary>
        /// Output srange
        /// </summary>
        public override Interval OutputRange { get { return _outputRange; } }

        //Methods
        /// <summary>
        /// Computes output of the activation function (changes internal state)
        /// </summary>
        /// <param name="x">Activation input</param>
        public override double Compute(double x)
        {
            x = x.Bound();
            return (Math.Log(1d + Math.Exp(x))).Bound();
        }

        /// <summary>
        /// Computes derivative of the activation input (does not change internal state)
        /// </summary>
        /// <param name="c">The result of the activation (Compute method)</param>
        /// <param name="x">Activation input (x argument of the Compute method)</param>
        public override double ComputeDerivative(double c, double x)
        {
            x.Bound();
            return 1d / (1d + Math.Exp(-x));
        }

    }//SoftPlus

}//Namespace
