using System;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Base class of analog activation functions
    /// </summary>
    [Serializable]
    public abstract class AnalogActivationFunction : IActivationFunction
    {
        //Attributes

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
        /// Type of the output signal
        /// </summary>
        public ActivationFactory.FunctionOutputSignalType OutputSignalType { get { return ActivationFactory.FunctionOutputSignalType.Analog; } }

        /// <summary>
        /// Output signal range
        /// </summary>
        public abstract Interval OutputRange { get; }

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

        //Methods
        /// <summary>
        /// Resets function to its initial state.
        /// Does nothing in case of analog activation function.
        /// </summary>
        public void Reset()
        {
            return;
        }

        /// <summary>
        /// Computes output of the activation function.
        /// </summary>
        /// <param name="x">Activation input</param>
        public abstract double Compute(double x);

        /// <summary>
        /// Computes derivative of the activation input.
        /// </summary>
        /// <param name="c">The result of the activation (Compute method)</param>
        /// <param name="x">Activation input (x argument of the Compute method)</param>
        public abstract double ComputeDerivative(double c, double x);

    }//AnalogActivationFunction

}//Namespace
