using RCNet.Neural.Network.SM.Preprocessing.Neuron;
using System;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Implements nonlinear efficacy computer
    /// </summary>
    public class NonlinearEfficacy : IEfficacy
    {
        //Attributes
        private readonly INeuron _sourceNeuron;
        private readonly NonlinearDynamicsSettings _dynamicsCfg;
        private double _facilitation;
        private double _depression;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="sourceNeuron">Source neuron</param>
        /// <param name="dynamicsCfg">Dynamics configuration</param>
        public NonlinearEfficacy(INeuron sourceNeuron, NonlinearDynamicsSettings dynamicsCfg)
        {
            _sourceNeuron = sourceNeuron;
            _dynamicsCfg = (NonlinearDynamicsSettings)dynamicsCfg.DeepClone();
            Reset();
            return;
        }

        //Methods
        /// <summary>
        /// Resets efficacy computer to its initial state
        /// </summary>
        public void Reset()
        {
            _facilitation = _dynamicsCfg.RestingEfficacy;
            _depression = 1d;
            return;
        }

        /// <summary>
        /// Computes synapse efficacy (call only when spike)
        /// </summary>
        public double Compute()
        {
            if (_sourceNeuron.AfterFirstSpike)
            {
                double sourceSpikeLeak = _sourceNeuron.SpikeLeak;
                //Facilitation model
                double tmp = _facilitation * Math.Exp(-(sourceSpikeLeak / _dynamicsCfg.TauFacilitation));
                _facilitation = tmp + _dynamicsCfg.RestingEfficacy * (1d - tmp);
                //Depression model
                tmp = Math.Exp(-(sourceSpikeLeak / _dynamicsCfg.TauDepression));
                _depression = _depression * (1d - _facilitation) * tmp + 1d - tmp;
            }
            return _facilitation * _depression;
        }

    }//NonlinearEfficacy

}//Namespace
