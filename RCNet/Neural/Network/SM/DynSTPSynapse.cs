using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Inplements the Short-Term-Plasticity dynamic synapse.
    /// </summary>
    [Serializable]
    public class DynSTPSynapse : Synapse
    {
        //Attributes
        private readonly double _tauFacilitation;
        private readonly double _tauRecovery;
        private readonly double _restingEfficacy;
        private readonly bool _applyAdaptation;
        private double _efficacyUtilization;
        private double _efficacyAvailableFraction;

        //Constructor
        /// <summary>
        /// Creates initialized instance
        /// </summary>
        /// <param name="sourceNeuron">Source neuron</param>
        /// <param name="targetNeuron">Target neuron</param>
        /// <param name="maxWeight">Synapse weight (unsigned)</param>
        public DynSTPSynapse(INeuron sourceNeuron,
                                INeuron targetNeuron,
                                double maxWeight,
                                int maxDelay,
                                double tauFacilitation,
                                double tauRecovery,
                                double restingEfficacy
                                )
            :base(sourceNeuron, targetNeuron, maxWeight, maxDelay)
        {
            _tauFacilitation = tauFacilitation;
            _tauRecovery = tauRecovery;
            _restingEfficacy = restingEfficacy;
            _applyAdaptation = (SourceNeuron.OutputType == Activation.ActivationFactory.FunctionOutputSignalType.Spike && SourceNeuron.Role != CommonEnums.NeuronRole.Input);
            _efficacyUtilization = _restingEfficacy;
            _efficacyAvailableFraction = 1;
            return;
        }

        //Methods
        /// <summary>
        /// Resets synapse.
        /// </summary>
        public override void Reset()
        {
            _efficacyUtilization = _restingEfficacy;
            _efficacyAvailableFraction = 1;
            return;
        }

        /// <summary>
        /// Computes weighted signal and puts it into the internal queue
        /// </summary>
        protected override void EnqueueSignal()
        {
            double adaptation = 1;
            if (_applyAdaptation)
            {
                if (SourceNeuron.OutputSignal > 0)
                {
                    double x = Math.Exp(-(SourceNeuron.NoSignalCycles / _tauFacilitation));
                    _efficacyUtilization = x + _restingEfficacy * (1d - x);
                    double y = Math.Exp(-(SourceNeuron.NoSignalCycles / _tauRecovery));
                    _efficacyAvailableFraction = _efficacyAvailableFraction * (1d - _efficacyUtilization) * y + 1d - y;
                    adaptation = _efficacyUtilization * _efficacyAvailableFraction;
                }
            }
            _qSig.Enqueue(((SourceNeuron.OutputSignal + _add) / _div) * Weight * adaptation);
            return;
        }

    }//DynSTPSynapse

}//Namespace
