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
        /// Type of the output
        /// </summary>
        ActivationFactory.FunctionOutputType OutputType { get; }
        
        /// <summary>
        /// Accepted input signal range
        /// </summary>
        Interval InputRange { get; }
        
        /// <summary>
        /// Output signal range
        /// </summary>
        Interval OutputRange { get; }
        
        /// <summary>
        /// Specifies whether the activation function supports derivation
        /// </summary>
        bool SupportsDerivation { get; }
        
        /// <summary>
        /// Specifies whether the activation function is depending on its previous states
        /// </summary>
        bool TimeDependent { get; }

        /// <summary>
        /// Range of the internal state
        /// </summary>
        Interval InternalStateRange { get; }

        /// <summary>
        /// Internal state
        /// </summary>
        double InternalState { get; }


        //Methods
        /// <summary>
        /// Resets function to initial state
        /// </summary>
        void Reset();

        /// <summary>
        /// Computes output of the activation function (changes internal state)
        /// </summary>
        /// <param name="x">Argument</param>
        double Compute(double x);

        /// <summary>
        /// Computes the derivation (does not change internal state)
        /// </summary>
        /// <param name="c">The result of the Compute method</param>
        /// <param name="x">The argument of the Compute method</param>
        double Derive(double c, double x);

    }//IActivationFunction

}//Namespace
