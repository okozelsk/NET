using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;

namespace RCNet.Neural.Analog.Activation
{
    /// <summary>
    /// Interface of the activation function
    /// </summary>
    public interface IAnalogActivationFunction
    {
        //Properties
        /// <summary>
        /// The working range
        /// </summary>
        Interval Range { get; }

        //Methods
        /// <summary>
        /// Computes the result of the activation function
        /// </summary>
        /// <param name="x">Argument</param>
        double Compute(double x);

        /// <summary>
        /// Computes the derivation
        /// </summary>
        /// <param name="c">The result of the Compute method</param>
        /// <param name="x">The argument of the Compute method</param>
        double Derive(double c, double x);

    }//IAnalogActivationFunction

}//Namespace
