using System;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Base class of analog activation functions
    /// </summary>
    [Serializable]
    public abstract class AFAnalogBase : IActivation
    {
        //Attributes
        /// <summary>
        /// Output range of the activation function
        /// </summary>
        protected Interval _outputRange;

        //Constructor
        /// <summary>
        /// Base constructor
        /// </summary>
        protected AFAnalogBase(Interval outputRange)
        {
            _outputRange = outputRange;
            return;
        }

        //Properties
        /// <inheritdoc/>
        public ActivationType TypeOfActivation { get { return ActivationType.Analog; } }

        /// <inheritdoc/>
        public Interval OutputRange { get { return _outputRange; } }

        /// <summary>
        /// Indicates whether the activation function supports derivative calculation
        /// </summary>
        public virtual bool SupportsDerivative { get { return true; } }

        /// <summary>
        /// Indicates whether the activation function requires multiple inputs for the Compute method
        /// </summary>
        public virtual bool RequiresMultipleInput { get { return false; } }

        //Methods
        /// <inheritdoc/>
        public abstract double Compute(double x);

        /// <summary>
        /// Computes the vector of activations.
        /// </summary>
        /// <param name="xVector">Vector of activation inputs</param>
        /// <param name="cVector">Vector of activations (computed values)</param>
        public virtual void Compute(double[] xVector, double[] cVector)
        {
            //Standard base implementation
            for(int i = 0; i < xVector.Length; i++)
            {
                cVector[i] = Compute(xVector[i]);
            }
            return;
        }

        /// <summary>
        /// Computes the derivative
        /// </summary>
        /// <param name="c">The result of the activation (Compute method)</param>
        /// <param name="x">Activation input (x argument of the Compute method resulting in c)</param>
        public abstract double ComputeDerivative(double c, double x);

        /// <summary>
        /// Computes the vector of derivatives.
        /// </summary>
        /// <param name="cVector">Vector of the activations (already computed activations)</param>
        /// <param name="xVector">Vector of the activation inputs</param>
        /// <param name="dVector">Vector of derivatives (computed values)</param>
        public virtual void ComputeDerivative(double[] cVector, double[] xVector, double[] dVector)
        {
            //Standard base implementation
            if(!SupportsDerivative)
            {
                throw new InvalidOperationException($"Called ComputeDerivative method but activation {GetType().Name} does not support the derivative computation.");
            }
            for (int i = 0; i < cVector.Length; i++)
            {
                dVector[i] = ComputeDerivative(cVector[i], xVector[i]);
            }
            return;
        }

    }//AFAnalogBase

}//Namespace
