using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;
using RCNet.Neural.Network.SM.Neuron;
using RCNet.Queue;

namespace RCNet.Neural.Network.SM.Synapse
{
    /// <summary>
    /// Internal synapse connecting two hidden neurons within the reservoir.
    /// Supports the short-term-plasticity.
    /// </summary>
    [Serializable]
    public class InternalSynapse : BaseSynapse, ISynapse
    {
        //Attributes
        private readonly double _tauFacilitation;
        private readonly double _tauDepression;
        private readonly double _restingEfficacy;
        private double _facilitation;
        private double _depression;
        private readonly bool _applyShortTermPlasticity;

        //Constructor
        /// <summary>
        /// Creates initialized instance
        /// </summary>
        /// <param name="sourceNeuron">Source neuron</param>
        /// <param name="targetNeuron">Target neuron</param>
        /// <param name="weight">Synapse weight (unsigned)</param>
        /// <param name="tauFacilitation">Synapse's efficacy facilitation model time constant (ms)</param>
        /// <param name="tauDepression">Synapse's efficacy depression model time constant (ms)</param>
        /// <param name="restingEfficacy">Synapse's resting efficacy (average probability of neurotransmitter release)</param>
        /// <param name="applyShortTermPlasticity">Specifies whether to apply short-term plasticity (if possible).</param>
        public InternalSynapse(INeuron sourceNeuron,
                               INeuron targetNeuron,
                               double weight,
                               double tauFacilitation,
                               double tauDepression,
                               double restingEfficacy,
                               bool applyShortTermPlasticity = true
                               )
            : base(sourceNeuron, targetNeuron, weight)
        {
            //Short-term-plasticity
            _tauFacilitation = tauFacilitation;
            _tauDepression = tauDepression;
            _restingEfficacy = restingEfficacy;
            _facilitation = _restingEfficacy;
            _depression = 1;
            _applyShortTermPlasticity = applyShortTermPlasticity;
            return;
        }

        //Methods
        /// <summary>
        /// Resets synapse.
        /// </summary>
        /// <param name="statistics">Specifies whether to reset also internal statistics</param>
        public void Reset(bool statistics)
        {
            _facilitation = _restingEfficacy;
            _depression = 1d;
            if (statistics)
            {
                EfficacyStat.Reset();
            }
            return;
        }

        /// <summary>
        /// Computes synapse efficacy based on the short-term-plasticity model.
        /// </summary>
        protected double ComputeEfficacy()
        {
            if (_applyShortTermPlasticity)
            {
                if (SourceNeuron.AfterFirstSpike)
                {
                    //Facilitation model
                    double tmp = _facilitation * Math.Exp(-(SourceNeuron.SpikeLeak / _tauFacilitation));
                    _facilitation = tmp + _restingEfficacy * (1d - tmp);
                    //Depression model
                    tmp = Math.Exp(-(SourceNeuron.SpikeLeak / _tauDepression));
                    _depression = _depression * (1d - _facilitation) * tmp + 1d - tmp;
                }
                return _facilitation * _depression;
            }
            else
            {
                return 1d;
            }
        }

        /// <summary>
        /// Returns signal to be delivered to target neuron.
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        public double GetSignal(bool collectStatistics)
        {
            //Source neuron signal
            double sourceSignal = SourceNeuron.GetSignal(TargetNeuron.TypeOfActivation);
            if (sourceSignal == 0)
            {
                return 0;
            }
            else
            {
                //Compute synapse efficacy
                double efficacy = ComputeEfficacy();
                //Update statistics if necessary
                if (collectStatistics)
                {
                    EfficacyStat.AddSampleValue(efficacy);
                }
                //Return resulting signal
                return sourceSignal * Weight * efficacy;
            }
        }

    }//InternalSynapse

}//Namespace
