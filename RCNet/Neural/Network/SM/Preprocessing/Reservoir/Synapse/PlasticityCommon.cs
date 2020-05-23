using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Neural.Network.SM.Preprocessing.Neuron;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Helper class for synapse's plasticity
    /// </summary>
    public static class PlasticityCommon
    {
        /// <summary>
        /// Type of synapse's dynamics
        /// </summary>
        public enum DynType
        {
            /// <summary>
            /// Constant dynamics
            /// </summary>
            Constant,
            /// <summary>
            /// Linear dynamics
            /// </summary>
            Linear,
            /// <summary>
            /// Nonlinear dynamics
            /// </summary>
            Nonlinear
        }//DynType

        /// <summary>
        /// Application of the synapse's dynamics
        /// </summary>
        public enum DynApplication
        {
            /// <summary>
            /// Input spiking synapse connecting spiking hidden neuron
            /// </summary>
            STInput,
            /// <summary>
            /// Excitatory spiking synapse connecting spiking hidden neuron
            /// </summary>
            STExcitatory,
            /// <summary>
            /// Inhibitory spiking synapse connecting spiking hidden neuron
            /// </summary>
            STInhibitory,
            /// <summary>
            /// Input spiking synapse connecting analog hidden neuron
            /// </summary>
            ATInput,
            /// <summary>
            /// Indifferent spiking synapse connecting analog hidden neuron
            /// </summary>
            ATIndifferent
        }//DynApplication

        /// <summary>
        /// Creates appropriate instance of the efficacy computer
        /// </summary>
        /// <param name="sourceNeuron">Source neuron</param>
        /// <param name="dynamicsCfg">Dynamics configuration</param>
        public static IEfficacy GetEfficacyComputer(INeuron sourceNeuron, IDynamicsSettings dynamicsCfg)
        {
            switch(dynamicsCfg.Type)
            {
                case DynType.Constant: return new ConstantEfficacy((ConstantDynamicsSettings)dynamicsCfg);
                case DynType.Linear: return new LinearEfficacy(sourceNeuron, (LinearDynamicsSettings)dynamicsCfg);
                case DynType.Nonlinear: return new NonlinearEfficacy(sourceNeuron, (NonlinearDynamicsSettings)dynamicsCfg);
                default:
                    throw new InvalidOperationException($"Unsupported dynamics configuration {dynamicsCfg.GetType().Name}.");
            }
        }


    }//PlasticityCommon
}//Namespace
