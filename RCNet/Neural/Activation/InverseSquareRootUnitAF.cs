using System;
using RCNet.MathTools;
using RCNet.Extensions;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Inverse Square Root Unit (ISRU) activation function
    /// See the IActivationFunction.
    /// </summary>
    [Serializable]
    public class InverseSquareRootUnitAF : IActivationFunction
    {
        //Constructor
        public InverseSquareRootUnitAF(double alpha = 1)
        {
            if(alpha <= 0)
            {
                throw new ArgumentOutOfRangeException("alpha", "Alpha must be GT 0");
            }
            Alpha = alpha;
            return;
        }

        //Properties
        public Interval Range { get { return new Interval(-1 / Math.Sqrt(Alpha), 1 / Math.Sqrt(Alpha)); } }
        public double Alpha { get; }

        //Methods
        public double Compute(double x)
        {
            return x / (1d + Alpha * x.Power(2));
        }

        public double Derive(double c, double x)
        {
            return (1d / Math.Sqrt(1d + Alpha * x.Power(2))).Power(3);
        }

    }//InverseSquareRootUnitAF
}//Namespace
