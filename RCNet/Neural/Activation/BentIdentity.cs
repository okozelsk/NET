using System;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// BentIdentity activation function
    /// </summary>
    [Serializable]
    public class BentIdentity : AnalogActivationFunction
    {
        //Attributes
        //Static working ranges
        private static readonly Interval _outputSignalRange = new Interval(0, double.PositiveInfinity.Bound());

        //Constructor
        /// <summary>
        /// Instantiates Bent Identity activation function
        /// </summary>
        public BentIdentity()
            :base()
        {
            return;
        }

        //Properties
        /// <summary>
        /// Output signal range
        /// </summary>
        public override Interval OutputSignalRange { get { return _outputSignalRange; } }

        /// <summary>
        /// Normal range of the internal state
        /// </summary>
        public override Interval InternalStateRange { get { return _outputSignalRange; } }

        //Methods
        /// <summary>
        /// Computes output of the activation function (changes internal state)
        /// </summary>
        /// <param name="x">Activation input</param>
        public override double Compute(double x)
        {
            x = x.Bound();
            _state = ((Math.Sqrt(x.Power(2) + 1d) - 1d) / 2d + x).Bound();
            return _state;
        }

        /// <summary>
        /// Computes derivative of the activation input (does not change internal state)
        /// </summary>
        /// <param name="c">The result of the activation (Compute method)</param>
        /// <param name="x">Activation input (x argument of the Compute method)</param>
        public override double ComputeDerivative(double c, double x)
        {
            x = x.Bound();
            return x / (2d * Math.Sqrt(x.Power(2) + 1d)) + 1d;
        }

    }//BentIdentity

}//Namespace
