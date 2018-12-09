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
    /// Inplements the Short-Term-Plasticity dynamic synapse.
    /// </summary>
    [Serializable]
    public class DynamicSynapse : Synapse
    {
        //Attributes
        private readonly double _tauFacilitation;
        private readonly double _tauRecovery;
        private readonly double _restingEfficacy;
        private readonly bool _applySTP;
        private readonly double _tauDecay;
        private readonly bool _applyDecay;
        private double _efficacyUtilization;
        private double _efficacyAvailableFraction;

        //Constructor
        /// <summary>
        /// Creates initialized instance
        /// </summary>
        /// <param name="sourceNeuron">Source neuron</param>
        /// <param name="targetNeuron">Target neuron</param>
        /// <param name="maxWeight">Synapse weight (unsigned)</param>
        /// <param name="maxDelay">Maximum delay (in cycles) of the signal delivery</param>
        /// <param name="tauFacilitation">Synapse efficacy facilitation parameter</param>
        /// <param name="tauRecovery">Synapse efficacy recovery parameter</param>
        /// <param name="restingEfficacy">Synapse resting efficacy parameter</param>
        /// <param name="tauDecay">Decay shapness (lower = sharper)</param>
        public DynamicSynapse(INeuron sourceNeuron,
                                INeuron targetNeuron,
                                double maxWeight,
                                int maxDelay,
                                double tauFacilitation,
                                double tauRecovery,
                                double restingEfficacy,
                                double tauDecay
                                )
            :base(sourceNeuron, targetNeuron, maxWeight, maxDelay)
        {
            _tauFacilitation = tauFacilitation;
            _tauRecovery = tauRecovery;
            _restingEfficacy = restingEfficacy;
            _applySTP = (SourceNeuron.OutputType == Activation.ActivationFactory.FunctionOutputSignalType.Spike);
            _efficacyUtilization = _restingEfficacy;
            _efficacyAvailableFraction = 1;
            _tauDecay = tauDecay;
            _applyDecay = (TargetNeuron.OutputType == Activation.ActivationFactory.FunctionOutputSignalType.Spike);
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
        /// Updates synapse efficacy (dynamic adaptation of the synapse)
        /// </summary>
        protected override void UpdateEfficacy()
        {
            _efficacy = 1d;
            if (SourceNeuron.OutputSignal > 0 && SourceNeuron.OutputSignalLeak > 0)
            {
                //Adaptation is relevant when there is output from source neuron
                double adaptationSTP = 1d;
                //Short term synapse plasticity (pre-synaptic dependence)
                if (_applySTP)
                {
                    double x = Math.Exp(-(SourceNeuron.OutputSignalLeak / _tauFacilitation));
                    _efficacyUtilization = x + _restingEfficacy * (1d - x);
                    double y = Math.Exp(-(SourceNeuron.OutputSignalLeak / _tauRecovery));
                    _efficacyAvailableFraction = _efficacyAvailableFraction * (1d - _efficacyUtilization) * y + 1d - y;
                    adaptationSTP = _efficacyUtilization * _efficacyAvailableFraction;
                }
                //Decay (post-synaptic dependence)
                double adaptationDecay = 1d;
                if (_applyDecay)
                {
                    adaptationDecay = Math.Exp(-(TargetNeuron.OutputSignalLeak / _tauDecay));
                }
                //Resulting efficacy
                _efficacy = adaptationSTP * adaptationDecay;
            }
            return;
        }

    }//DynamicSynapse

}//Namespace
