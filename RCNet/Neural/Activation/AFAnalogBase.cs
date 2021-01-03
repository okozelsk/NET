using RCNet.MathTools;
using System;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements the base class of all analog activation functions.
    /// </summary>
    [Serializable]
    public abstract class AFAnalogBase : IActivation
    {
        //Attributes
        /// <summary>
        /// Output range of the activation function.
        /// </summary>
        protected Interval _outputRange;

        //Constructor
        /// <summary>
        /// The base constructor.
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
        /// Indicates whether the activation function supports the derivative calculation.
        /// </summary>
        public virtual bool SupportsDerivative { get { return true; } }

        /// <summary>
        /// Indicates whether the activation function requires to works in tandem with its surroundings.
        /// </summary>
        /// <remarks>
        /// The typical case of the surroundings dependent activation function is the SoftMax activation function.
        /// </remarks>
        public virtual bool DependsOnSorround { get { return false; } }

        //Methods
        /// <inheritdoc/>
        public abstract double Compute(double x);

        /// <summary>
        /// Computes the vector of activations.
        /// </summary>
        /// <param name="xVector">The vector of inputs.</param>
        /// <param name="cVector">The vector of corresponding computed activations.</param>
        public virtual void Compute(double[] xVector, double[] cVector)
        {
            //The standard base implementation
            for (int i = 0; i < xVector.Length; i++)
            {
                cVector[i] = Compute(xVector[i]);
            }
            return;
        }

        /// <summary>
        /// Computes the derivative.
        /// </summary>
        /// <param name="c">The activation result (the result of the Compute method).</param>
        /// <param name="x">The activation input (the x argument of the Compute method resulting in the c).</param>
        public abstract double ComputeDerivative(double c, double x);

        /// <summary>
        /// Computes the vector of derivatives.
        /// </summary>
        /// <param name="cVector">The vector of the activation results (already computed activations).</param>
        /// <param name="xVector">The vector of the activation inputs.</param>
        /// <param name="dVector">The computed vector of derivatives.</param>
        public virtual void ComputeDerivative(double[] cVector, double[] xVector, double[] dVector)
        {
            //Standard base implementation
            if (!SupportsDerivative)
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
