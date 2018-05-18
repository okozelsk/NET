using System;
using RCNet.MathTools;
using RCNet.Extensions;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Elliot activation function (aka Softsign).
    /// </summary>
    [Serializable]
    public class Elliot : AnalogActivationFunction
    {
        //Attributes
        private static readonly Interval _outputRange = new Interval(-1, 1);

        //Attribute properties
        /// <summary>
        /// The curve slope
        /// </summary>
        public double Slope { get; }

        //Constructor
        /// <summary>
        /// Instantiates Elliot activation function
        /// </summary>
        /// <param name="slope">The curve slope</param>
        public Elliot(double slope = 1)
            :base()
        {
            if (slope <= 0)
            {
                throw new ArgumentOutOfRangeException("slope", "Slope must be GT 0");
            }
            Slope = slope.Bound();
            return;
        }

        //Properties
        /// <summary>
        /// Output signal range
        /// </summary>
        public override Interval OutputSignalRange { get { return _outputRange; } }

        /// <summary>
        /// Normal range of the internal state
        /// </summary>
        public override Interval InternalStateRange { get { return _outputRange; } }

        //Methods
        /// <summary>
        /// Computes output of the activation function (changes internal state)
        /// </summary>
        /// <param name="x">Activation input</param>
        public override double Compute(double x)
        {
            x = x.Bound();
            _state = ((x * Slope) / (1d + Math.Abs(x * Slope))).Bound();
            return _state;
        }

        /// <summary>
        /// Computes derivative of the activation input (does not change internal state)
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

