using RCNet.Extensions;
using RCNet.MathTools;
using System;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements the Soft Max activation function.
    /// </summary>
    [Serializable]
    public class AFAnalogSoftMax : AFAnalogBase
    {
        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public AFAnalogSoftMax()
            : base(Interval.IntZP1)
        {
            return;
        }

        //Properties
        /// <inheritdoc/>
        public override bool DependsOnSorround { get { return true; } }

        //Methods
        /// <inheritdoc/>
        public override double Compute(double x)
        {
            throw new InvalidOperationException("The wrong version of the Compute method was called. It was called Compute(double) method instead of the Compute(double[], double[]) method. SoftMax method requires the sorroundings.");
        }

        /// <inheritdoc/>
        public override void Compute(double[] xVector, double[] cVector)
        {
            double maxX = xVector.Max();
            double sum = 0;
            for (int i = 0; i < xVector.Length; i++)
            {
                cVector[i] = Math.Exp(xVector[i] - maxX);
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
