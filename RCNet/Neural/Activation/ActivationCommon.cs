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

}//Namespace
