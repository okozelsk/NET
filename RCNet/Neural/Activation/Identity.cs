using System;
using RCNet.MathTools;
using RCNet.Extensions;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements Identity activation function (aka Linear)
    /// </summary>
    [Serializable]
    public class Identity : AnalogActivationFunction
    {
        //Attributes
        private static readonly Interval _outputRange = new Interval(double.NegativeInfinity.Bound(), double.PositiveInfinity.Bound());

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public Identity()
            : base()
        {
            return;
        }

        //Properties
        /// <summary>
        /// Optimal strength of the stimulation is not relevant
        /// </summary>
        public override double OptimalStimulationStrength { get { throw new NotImplementedException("Optimal strength of the stimulation is not relevant."); } }

        /// <summary>
        /// Range of reasonable incoming current
        /// </summary>
        public override Interval StimuliRange { get { throw new NotImplementedException("Stimulation range is not relevant."); } }

        /// <summary>
        /// Output signal range
        /// </summary>
        public override Interval OutputRange { get { return _outputRange; } }

        //Methods
        /// <summary>
        /// Computes output of the activation function (changes internal state)
        /// </summary>
        /// <param name="x">Activation input</param>
        public override double Compute(double x)
        {
            //The same value
            return x.Bound();
        }

        /// <summary>
        /// Computes derivative of the activation input (does not change internal state)
        /// </summary>
        /// <param name="c">The result of the activation (Compute method)</param>
        /// <param name="x">Activation input (x argument of the Compute method)</param>
        public override double ComputeDerivative(double c, double x = double.NaN)
        {
            //Allways 1
            return 1d;
        }

    }//Identity

}//Namespace

