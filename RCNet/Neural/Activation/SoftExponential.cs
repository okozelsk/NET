using System;
using RCNet.MathTools;
using RCNet.Extensions;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// SoftExponential activation function
    /// </summary>
    [Serializable]
    public class SoftExponential : AnalogActivationFunction
    {
        //Attributes
        //Static working ranges
        private static readonly Interval _outputRange = new Interval(double.NegativeInfinity.Bound(), double.PositiveInfinity.Bound());

        /// <summary>
        /// Alpha
        /// </summary>
        public double Alpha { get; }

        //Constructor
        /// <summary>
        /// Instantiates SoftExponential activation function
        /// </summary>
        /// <param name="alpha">Alpha</param>
        public SoftExponential(double alpha)
            :base()
        {
            Alpha = alpha.Bound();
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
            if(Alpha < 0)
            {
                _state = (-(Math.Log(1 - Alpha * (x + Alpha)) / Alpha)).Bound();
            }
            else if(Alpha == 0)
            {
                _state = x;
            }
            else
            {
                _state = ((Math.Exp(Alpha * x) - 1) / Alpha + Alpha).Bound();
            }
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
            if(Alpha < 0)
            {
                return 1d  / (1d - Alpha * (Alpha + x));
            }
            else
            {
                return Math.Exp(Alpha * x);
            }
        }

    }//SoftExponential

}//Namespace

