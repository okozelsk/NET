using System;
using RCNet.MathTools;
using RCNet.Extensions;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements ISRU (Inverse Square Root Unit) activation function
    /// </summary>
    [Serializable]
    public class ISRU : AnalogActivationFunction
    {
        //Constants
        //Attributes
        private readonly Interval _outputRange;
        
        //Attribute properties
        /// <summary>
        /// The Alpha
        /// </summary>
        public double Alpha { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="alpha">The Alpha</param>
        public ISRU(double alpha)
            : base()
        {
            Alpha = alpha.Bound();
            if (Alpha <= 0)
            {
                throw new ArgumentOutOfRangeException("alpha", "Alpha must be GT 0");
            }
            _outputRange = new Interval(-1 / Math.Sqrt(Alpha), 1 / Math.Sqrt(Alpha));
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
            return (x / Math.Sqrt(1d + Alpha * x.Power(2))).Bound();
        }

        /// <summary>
        /// Computes derivative (with respect to x)
        /// </summary>
        /// <param name="c">The result of the activation (Compute method)</param>
        /// <param name="x">Activation input (x argument of the Compute method)</param>
        public override double ComputeDerivative(double c, double x)
        {
            x = x.Bound();
            return (1d / Math.Sqrt(1d + Alpha * x.Power(2))).Power(3);
        }

    }//ISRU

}//Namespace

