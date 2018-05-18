using System;
using RCNet.MathTools;
using RCNet.Extensions;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// ISRU (Inverse Square Root Unit) activation function
    /// </summary>
    [Serializable]
    public class ISRU : AnalogActivationFunction
    {
        //Attributes
        private Interval _outputRange;
        
        //Attribute properties
        /// <summary>
        /// The Alpha
        /// </summary>
        public double Alpha { get; }

        //Constructor
        /// <summary>
        /// Instantiates an Inverse Square Root Unit activation function
        /// </summary>
        /// <param name="alpha">The Alpha</param>
        public ISRU(double alpha = 1)
            :base()
        {
            if(alpha <= 0)
            {
                throw new ArgumentOutOfRangeException("alpha", "Alpha must be GT 0");
            }
            Alpha = alpha.Bound(); ;
            _outputRange = new Interval(-1 / Math.Sqrt(Alpha), 1 / Math.Sqrt(Alpha));
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
            _state = (x / (1d + Alpha * x.Power(2))).Bound();
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
            return (1d / Math.Sqrt(1d + Alpha * x.Power(2))).Power(3);
        }

    }//ISRU

}//Namespace

