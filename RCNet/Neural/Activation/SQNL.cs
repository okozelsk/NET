using System;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements SQNL (Square nonlinearity) activation function
    /// </summary>
    [Serializable]
    public class SQNL : AnalogActivationFunction
    {

        //Static members
        private static readonly Interval _outputRange = new Interval(-1, 1);

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public SQNL()
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
            if(x > 2d)
            {
                return 1d;
            }
            else if(x >= 0)
            {
                return x - (x * x) / 4d;
            }
            else if(x >= -2d)
            {
                return x + (x * x) / 4d;
            }
            else
            {
                //x < -2d
                return -1d;
            }
        }

        /// <summary>
        /// Computes derivative (with respect to x)
        /// </summary>
        /// <param name="c">The result of the activation (Compute method)</param>
        /// <param name="x">Activation input (x argument of the Compute method)</param>
        public override double ComputeDerivative(double c, double x = double.NaN)
        {
            //Faster than 1d + Math.Abs(x/2d);
            return x < 0? 1d - x/2d : 1d + x/2d;
        }

    }//SQNL

}//Namespace
