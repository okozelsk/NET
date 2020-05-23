using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Common interface of all activation functions
    /// </summary>
    public interface IActivationFunction
    {
        //Properties
        /// <summary>
        /// Type of the activation function
        /// </summary>
        ActivationType TypeOfActivation { get; }

        /// <summary>
        /// Specifies whether the activation function supports derivative calculation
        /// </summary>
        bool SupportsDerivative { get; }

        /// <summary>
        /// Specifies whether the activation function is independent on its previous states
        /// </summary>
        bool Stateless { get; }

        /// <summary>
        /// Normal range of the internal state
        /// </summary>
        Interval InternalStateRange { get; }

        /// <summary>
        /// Current internal state
        /// </summary>
        double InternalState { get; }

        /// <summary>
        /// Output range of the Compute method
        /// </summary>
        Interval OutputRange { get; }

        //Methods
        /// <summary>
        /// Resets function to its initial state
        /// </summary>
        void Reset();

        /// <summary>
        /// Sets initial state of the activation function (applicable for !stateless only)
        /// </summary>
        void SetInitialInternalState(double state);

        /// <summary>
        /// Computes output of the activation function
        /// </summary>
        /// <param name="x">Activation input</param>
        double Compute(double x);

        /// <summary>
        /// Computes derivative (with respect to x)
        /// </summary>
        /// <param name="c">The result of the activation (Compute method)</param>
        /// <param name="x">Activation input (x argument of the Compute method)</param>
        double ComputeDerivative(double c, double x);

    }//IActivationFunction

}//Namespace
