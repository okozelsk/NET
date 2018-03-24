using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Neuron activation function
    /// </summary>
    public interface IActivationFunction
    {
        //Properties
        Interval Range { get; }

        //Methods
        /// <summary>
        /// Process input
        /// </summary>
        /// <param name="x">Neuron input</param>
        /// <returns></returns>
        double Compute(double x);

        /// <summary>
        /// Derivative
        /// </summary>
        /// <param name="c">Result of Compute method</param>
        /// <param name="x">Argument of Compute method</param>
        /// <returns></returns>
        double ComputeDerivative(double c, double x);

    }//IActivationFunction
}//Namespace
