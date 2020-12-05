using RCNet.Extensions;
using RCNet.MathTools;
using System;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements the Soft Max activation function
    /// </summary>
    [Serializable]
    public class AFAnalogSoftMax : AFAnalogBase
    {
        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public AFAnalogSoftMax()
            : base(Interval.IntZP1)
        {
            return;
        }

        //Properties
        /// <inheritdoc/>
        public override bool RequiresMultipleInput { get { return true; } }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double x)
        {
            throw new InvalidOperationException("Called the Compute(double) method instead of the Compute(double[], double[]) method. SoftMax requires multiple input.");
        }

        /// <inheritdoc/>
        public override void Compute(double[] xVector, double[] cVector)
        {
            if(xVector.Length < 2)
            {
                throw new ArgumentException("The length of the xVector must be GE to 2.", "xVector");
            }
            if (cVector.Length < xVector.Length)
            {
                throw new ArgumentException("The length of the cVector must be GE to xVector length.", "cVector");
            }
            double sum = 0;
            for (int i = 0; i < xVector.Length; i++)
            {
                cVector[i] = Math.Exp(xVector[i].Bound(-40d, 40d));
                sum += cVector[i];
            }
            for (int i = 0; i < xVector.Length; i++)
            {
                cVector[i] /= sum;
            }
            return;
        }

        /// <inheritdoc/>
        public override double ComputeDerivative(double c, double x)
        {
            return 1d;
        }

    }//AFAnalogSoftMax

}//Namespace
