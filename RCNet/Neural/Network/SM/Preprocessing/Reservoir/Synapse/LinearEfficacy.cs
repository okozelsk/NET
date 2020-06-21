using RCNet.Extensions;
using RCNet.Neural.Network.SM.Preprocessing.Neuron;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Implements linear efficacy computer
    /// </summary>
    public class LinearEfficacy : IEfficacy
    {
        //Attributes
        private readonly NeuronOutputData _sourceNeuronOutputData;
        private readonly LinearDynamicsSettings _dynamicsCfg;
        private double _efficacy;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="sourceNeuron">Source neuron</param>
        /// <param name="dynamicsCfg">Dynamics configuration</param>
        public LinearEfficacy(INeuron sourceNeuron, LinearDynamicsSettings dynamicsCfg)
        {
            _sourceNeuronOutputData = sourceNeuron.OutputData;
            _dynamicsCfg = (LinearDynamicsSettings)dynamicsCfg.DeepClone();
            Reset();
            return;
        }

        //Methods
        /// <summary>
        /// Resets efficacy computer to its initial state
        /// </summary>
        public void Reset()
        {
            _efficacy = _dynamicsCfg.InitialEfficacy;
            return;
        }

        /// <summary>
        /// Computes synapse efficacy (call only when spike)
        /// </summary>
        public double Compute()
        {
            _efficacy -= (_sourceNeuronOutputData._spikeLeak - 1) * (_dynamicsCfg.Alpha * (0d - _dynamicsCfg.Beta));
            _efficacy = _efficacy.Bound(0d, 1d);
            _efficacy -= (_dynamicsCfg.Alpha * (1d - _dynamicsCfg.Beta));
            _efficacy = _efficacy.Bound(0d, 1d);
            return _efficacy;
        }

    }//LinearEfficacy

}//Namespace
