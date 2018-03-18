using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OKOSW.Neural.Activation
{
    /// <summary>
    /// Neuron activation
    /// </summary>
    public interface IActivationFunction
    {
        /// <summary>
        /// Process input
        /// </summary>
        /// <param name="x">Neuron input</param>
        /// <returns></returns>
        double Compute(double x);

        /// <summary>
        /// Partial derivative
        /// </summary>
        /// <param name="c">Result of Compute method</param>
        /// <returns></returns>
        double ComputeDerivative(double c);

    }//IActivationFunction
}//Namespace
