using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Implements constant efficacy computer
    /// </summary>
    public class ConstantEfficacy : IEfficacy
    {
        //Attributes
        private readonly ConstantDynamicsSettings _dynamicsCfg;
        
        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="dynamicsCfg">Dynamics configuration</param>
        public ConstantEfficacy(ConstantDynamicsSettings dynamicsCfg)
        {
            _dynamicsCfg = dynamicsCfg;
            return;
        }

        //Methods
        /// <summary>
        /// Resets efficacy computer to its initial state
        /// </summary>
        public void Reset()
        {
            return;
        }

        /// <summary>
        /// Computes synapse efficacy
        /// </summary>
        public double Compute()
        {
            return _dynamicsCfg == null ? 1d : _dynamicsCfg.Efficacy;
        }

    }//ConstantEfficacy

}//Namespace
