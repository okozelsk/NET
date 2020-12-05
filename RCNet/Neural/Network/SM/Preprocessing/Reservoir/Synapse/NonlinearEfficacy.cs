using RCNet.Neural.Network.SM.Preprocessing.Neuron;
using System;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Implements the nonlinear efficacy computer
    /// </summary>
    public class NonlinearEfficacy : IEfficacy
    {
        //Attributes
        private readonly NeuronOutputData _presynapticNeuronOutputData;
        private readonly NonlinearDynamicsSettings _dynamicsCfg;
        private double _facilitation;
        private double _depression;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="presynapticNeuron">Presynaptic neuron</param>
        /// <param name="dynamicsCfg">Dynamics configuration</param>
        public NonlinearEfficacy(INeuron presynapticNeuron, NonlinearDynamicsSettings dynamicsCfg)
        {
            _presynapticNeuronOutputData = presynapticNeuron.OutputData;
            _dynamicsCfg = (NonlinearDynamicsSettings)dynamicsCfg.DeepClone();
            Reset();
            return;
        }

        //Methods
        /// <inheritdoc />
        public void Reset()
        {
            _facilitation = _dynamicsCfg.RestingEfficacy;
            _depression = 1d;
            return;
        }

        /// <inheritdoc />
        public double Compute()
        {
            if (_presynapticNeuronOutputData._afterFirstSpike)
            {
                double presynapticSpikeLeak = _presynapticNeuronOutputData._spikeLeak;
                //Facilitation model
                double tmp = _facilitation * Math.Exp(-(presynapticSpikeLeak / _dynamicsCfg.TauFacilitation));
                _facilitation = tmp + _dynamicsCfg.RestingEfficacy * (1d - tmp);
                //Depression model
                tmp = Math.Exp(-(presynapticSpikeLeak / _dynamicsCfg.TauDepression));
                _depression = _depression * (1d - _facilitation) * tmp + 1d - tmp;
            }
            return _facilitation * _depression;
        }

    }//NonlinearEfficacy

}//Namespace
