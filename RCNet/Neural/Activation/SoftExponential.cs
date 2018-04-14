using System;
using RCNet.MathTools;
using RCNet.Extensions;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// SoftExponential activation function
    /// </summary>
    [Serializable]
    public class SoftExponential : IActivationFunction
    {
        //Constructor
        /// <summary>
        /// Instantiates an SoftExponential activation function
        /// </summary>
        /// <param name="alpha">Alpha</param>
        public SoftExponential(double alpha)
        {
            Alpha = alpha;
            return;
        }

        //Properties
        /// <summary>
        /// THe working range
        /// </summary>
        public Interval Range { get { return new Interval(double.NegativeInfinity, double.PositiveInfinity); } }

        /// <summary>
        /// Alpha
        /// </summary>
        public double Alpha { get; }

        //Methods
        /// <summary>
        /// Computes the result of the activation function
        /// </summary>
        /// <param name="x">Argument</param>
        public double Compute(double x)
        {
            if(Alpha < 0)
            {
                return -(Math.Log(1 - Alpha * (x + Alpha)) / Alpha);
            }
            else if(Alpha == 0)
            {
                return x;
            }
            else
            {
                return (Math.Exp(Alpha * x) - 1) / Alpha + Alpha;
            }
        }

        /// <summary>
        /// Computes the derivation
        /// </summary>
        /// <param name="c">The result of the Compute method</param>
        /// <param name="x">The argument of the Compute method</param>
        public double Derive(double c, double x)
        {
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

