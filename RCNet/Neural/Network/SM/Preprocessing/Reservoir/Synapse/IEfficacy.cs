using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Common interface for synapse efficacy computers
    /// </summary>
    public interface IEfficacy
    {
        /// <summary>
        /// Resets efficacy computer to its initial state
        /// </summary>
        void Reset();

        /// <summary>
        /// Computes synapse efficacy
        /// </summary>
        double Compute();

    }//IEfficacy

}//Namespace
