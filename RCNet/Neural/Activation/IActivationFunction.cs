using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Each neuron activation function has to implement this interface
    /// </summary>
    public interface IActivationFunction
    {
        //Properties
        /// <summary>
        /// Type of the output signal
        /// </summary>
        CommonEnums.NeuronSignalType OutputSignalType { get; }

        /// <summary>
        /// Output range
        /// </summary>
        Interval OutputRange { get; }

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
        /// Internal state
        /// </summary>
        double InternalState { get; }

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
        /// Computes output of the activation function (changes internal state)
        /// </summary>
        /// <param name="x">Activation input</param>
        double Compute(double x);

        /// <summary>
        /// Computes derivative of the activation input (does not change internal state)
        /// </summary>
        /// <param name="c">The result of the activation (Compute method)</param>
        /// <param name="x">Activation input (x argument of the Compute method)</param>
        double ComputeDerivative(double c, double x);

    }//IActivationFunction

}//Namespace
