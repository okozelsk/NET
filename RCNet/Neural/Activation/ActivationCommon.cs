using RCNet.MathTools.Differential;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Types of the neuron's activation function
    /// </summary>
    public enum ActivationType
    {
        /// <summary>
        /// Fires spike only when firing condition is met
        /// </summary>
        Spiking,
        /// <summary>
        /// Produces continuous analog output signal
        /// </summary>
        Analog
    };

    /// <summary>
    /// Helper class for activations
    /// </summary>
    public static class ActivationCommon
    {
        /// <summary>
        /// Parses ActivationType from the given code
        /// </summary>
        /// <param name="code">Activation type code</param>
        public static ActivationType ParseActivationType(string code)
        {
            switch(code.ToUpper())
            {
                case "ANALOG":
                    return ActivationType.Analog;
                case "SPIKING":
                    return ActivationType.Spiking;
                default:
                    throw new Exception($"Unknown activation type code {code}.");
            }
        }

    }//ActivationCommon

}//Namespace
