using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Inplements the pre-synaptic Short-Term-Plasticity and post-synaptic dynamic decay of the efficacy of the synapse.
    /// </summary>
    [Serializable]
    public class DynamicSynapse : Synapse
    {
        //Attributes
        private readonly double _tauFacilitation;
        private readonly double _tauRecovery;
        private readonly double _restingEfficacy;
        private readonly bool _applyPreSynaptic;
        private readonly double _tauDecay;
        private readonly bool _applyPostSynaptic;
        private double _efficacyUtilization;
        private double _efficacyAvailableFraction;

        //Constructor
        /// <summary>
        /// Creates initialized instance
        /// </summary>
        /// <param name="sourceNeuron">Source neuron</param>
        /// <param name="targetNeuron">Target neuron</param>
        /// <param name="weight">Synapse weight (unsigned)</param>
        /// <param name="delay">Synapse delay (in cycles) of the signal delivery</param>
        /// <param name="tauFacilitation">Synapse efficacy facilitation parameter (pre-synaptic)</param>
        /// <param name="tauRecovery">Synapse efficacy recovery parameter (pre-synaptic)</param>
        /// <param name="restingEfficacy">Synapse resting efficacy parameter (pre-synaptic)</param>
        /// <param name="tauDecay">Decay shapness (post-synaptic)</param>
        public DynamicSynapse(INeuron sourceNeuron,
                              INeuron targetNeuron,
                              double weight,
                              int delay,
                              double tauFacilitation,
                              double tauRecovery,
                              double restingEfficacy,
                              double tauDecay
                              )
            :base(sourceNeuron, targetNeuron, weight, delay)
        {
            _tauFacilitation = tauFacilitation;
            _tauRecovery = tauRecovery;
            _restingEfficacy = restingEfficacy;
            _applyPreSynaptic = (SourceNeuron.OutputType == Activation.ActivationFactory.FunctionOutputSignalType.Spike);
            _efficacyUtilization = _restingEfficacy;
            _efficacyAvailableFraction = 1;
            _tauDecay = tauDecay;
            _applyPostSynaptic = (TargetNeuron.OutputType == Activation.ActivationFactory.FunctionOutputSignalType.Spike);
            return;
        }

        //Methods
        /// <summary>
        /// Resets synapse.
        /// </summary>
        /// <param name="statistics">Specifies whether to reset also internal statistics</param>
        public override void Reset(bool statistics)
        {
            base.Reset(statistics);
            _efficacyUtilization = _restingEfficacy;
            _efficacyAvailableFraction = 1;
            return;
        }

        /// <summary>
        /// Computes synapse efficacy based on the pre-synaptic activity.
        /// Implementation of the pre-synaptic Short-Term-Plasticity
        /// </summary>
        protected override double GetPreSynapticEfficacy()
        {
            if(_applyPreSynaptic)
            {
                double x = Math.Exp(-(SourceNeuron.OutputSignalLeak / _tauFacilitation));
                _efficacyUtilization = x + _restingEfficacy * (1d - x);
                double y = Math.Exp(-(SourceNeuron.OutputSignalLeak / _tauRecovery));
                _efficacyAvailableFraction = _efficacyAvailableFraction * (1d - _efficacyUtilization) * y + 1d - y;
                return _efficacyUtilization * _efficacyAvailableFraction;
            }
            return 1d;
        }

        /// <summary>
        /// Computes synapse efficacy based on the post-synaptic activity
        /// </summary>
        protected override double GetPostSynapticEfficacy()
        {
            if(_applyPostSynaptic)
            {
                return Math.Exp(-(TargetNeuron.OutputSignalLeak / _tauDecay));
            }
            return 1d;
        }


    }//DynamicSynapse

}//Namespace
