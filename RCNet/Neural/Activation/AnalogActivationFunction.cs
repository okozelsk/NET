using RCNet.MathTools;
using System;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Base class of analog activation functions
    /// </summary>
    [Serializable]
    public abstract class AnalogActivationFunction : IActivationFunction
    {
        //Constructor
        /// <summary>
        /// Instantiates analog activation function
        /// </summary>
        protected AnalogActivationFunction()
        {
            return;
        }

        //Properties
        /// <summary>
        /// Type of the activation function.
        /// </summary>
        public ActivationType TypeOfActivation { get { return ActivationType.Analog; } }

        /// <summary>
        /// Specifies whether the activation function supports derivative calculation
        /// </summary>
        public bool SupportsDerivative { get { return true; } }

        /// <summary>
        /// Specifies whether the activation function is independent on its previous states
        /// </summary>
        public bool Stateless { get { return true; } }

        /// <summary>
        /// Normal range of the internal state
        /// </summary>
        public Interval InternalStateRange { get { throw new NotImplementedException("Analog activation function is stateless."); } }

        /// <summary>
        /// Internal state
        /// </summary>
        public double InternalState { get { throw new NotImplementedException("Analog activation function is stateless."); } }

        /// <summary>
        /// Output range of the Compute method
        /// </summary>
        public abstract Interval OutputRange { get; }


        //Methods
        /// <summary>
        /// Resets function to its initial state.
        /// Does nothing in case of analog activation function.
        /// </summary>
        public void Reset() { return; }

        /// <summary>
        /// Sets initial state of the activation function (applicable for !stateless only)
        /// </summary>
        public void SetInitialInternalState(double state) { throw new NotImplementedException("Analog activation function is stateless."); }

        /// <summary>
        /// Computes output of the activation function.
        /// </summary>
        /// <param name="x">Activation input</param>
        public abstract double Compute(double x);

        /// <summary>
        /// Computes derivative (with respect to x)
        /// </summary>
        /// <param name="c">The result of the activation (Compute method)</param>
        /// <param name="x">Activation input (x argument of the Compute method)</param>
        public abstract double ComputeDerivative(double c, double x);

    }//AnalogActivationFunction

}//Namespace
