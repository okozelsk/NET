using RCNet.Neural.Network.SM.Preprocessing.Neuron;
using System;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Implements the common enumerations and methods related to the synapse's plasticity.
    /// </summary>
    public static class PlasticityCommon
    {
        /// <summary>
        /// The type of synapse's efficacy dynamics.
        /// </summary>
        public enum DynType
        {
            /// <summary>
            /// The constant dynamics.
            /// </summary>
            Constant,
            /// <summary>
            /// The linear dynamics.
            /// </summary>
            Linear,
            /// <summary>
            /// The nonlinear dynamics.
            /// </summary>
            Nonlinear
        }//DynType

        /// <summary>
        /// The application of the synapse's efficacy dynamics.
        /// </summary>
        public enum DynApplication
        {
            /// <summary>
            /// Applicable on an input synapse connecting presynaptic input spiking neuron and postsynaptic hidden spiking neuron.
            /// </summary>
            STInput,
            /// <summary>
            /// Applicable on an excitatory synapse connecting presynaptic hidden spiking neuron and postsynaptic hidden spiking neuron.
            /// </summary>
            STExcitatory,
            /// <summary>
            /// Applicable on an inhibitory synapse connecting presynaptic hidden spiking neuron and postsynaptic hidden spiking neuron.
            /// </summary>
            STInhibitory,
            /// <summary>
            /// Applicable on an input synapse connecting presynaptic input spiking neuron and postsynaptic hidden analog neuron.
            /// </summary>
            ATInput,
            /// <summary>
            /// Applicable on an indifferent synapse connecting presynaptic hidden spiking neuron and postsynaptic hidden analog neuron.
            /// </summary>
            ATIndifferent
        }//DynApplication

        /// <summary>
        /// Creates an instance of the efficacy computer.
        /// </summary>
        /// <param name="presynapticNeuron">The presynaptic neuron.</param>
        /// <param name="dynamicsCfg">The configuration of the dynamics.</param>
        public static IEfficacy GetEfficacyComputer(INeuron presynapticNeuron, IDynamicsSettings dynamicsCfg)
        {
            switch (dynamicsCfg.Type)
            {
                case DynType.Constant: return new ConstantEfficacy((ConstantDynamicsSettings)dynamicsCfg);
                case DynType.Linear: return new LinearEfficacy(presynapticNeuron, (LinearDynamicsSettings)dynamicsCfg);
                case DynType.Nonlinear: return new NonlinearEfficacy(presynapticNeuron, (NonlinearDynamicsSettings)dynamicsCfg);
                default:
                    throw new InvalidOperationException($"Unsupported dynamics configuration {dynamicsCfg.GetType().Name}.");
            }
        }

    }//PlasticityCommon

}//Namespace
