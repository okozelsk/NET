using RCNet.Extensions;
using RCNet.Neural.Network.SM.Preprocessing.Neuron;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Implements the linear efficacy computer
    /// </summary>
    public class LinearEfficacy : IEfficacy
    {
        //Attributes
        private readonly NeuronOutputData _presynapticNeuronOutputData;
        private readonly LinearDynamicsSettings _dynamicsCfg;
        private double _efficacy;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="presynapticNeuron">Presynaptic neuron</param>
        /// <param name="dynamicsCfg">Dynamics configuration</param>
        public LinearEfficacy(INeuron presynapticNeuron, LinearDynamicsSettings dynamicsCfg)
        {
            _presynapticNeuronOutputData = presynapticNeuron.OutputData;
            _dynamicsCfg = (LinearDynamicsSettings)dynamicsCfg.DeepClone();
            Reset();
            return;
        }

        //Methods
        /// <inheritdoc />
        public void Reset()
        {
            _efficacy = _dynamicsCfg.InitialEfficacy;
            return;
        }

        /// <inheritdoc />
        public double Compute()
        {
            _efficacy -= (_presynapticNeuronOutputData._spikeLeak - 1) * (_dynamicsCfg.Alpha * (0d - _dynamicsCfg.Beta));
            _efficacy = _efficacy.Bound(0d, 1d);
            _efficacy -= (_dynamicsCfg.Alpha * (1d - _dynamicsCfg.Beta));
            _efficacy = _efficacy.Bound(0d, 1d);
            return _efficacy;
        }

    }//LinearEfficacy

}//Namespace
