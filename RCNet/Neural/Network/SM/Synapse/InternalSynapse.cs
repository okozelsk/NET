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
    /// Supports the short-term-plasticity and post-synaptic dynamically decayed current.
    /// </summary>
    [Serializable]
    public class InternalSynapse : BaseSynapse, ISynapse
    {
        //Constants
        private const double MinPostSynapticCurrent = 1e-15;
        //Attributes
        private readonly double _tauFacilitation;
        private readonly double _tauDepression;
        private readonly double _restingEfficacy;
        private double _facilitation;
        private double _depression;
        private readonly bool _applyShortTermPlasticity;
        private readonly double _tauPostSynapticCurrentDecay;
        private int _t;
        private bool _stopPostSynapticCurrent;
        private readonly bool _applyPostSynapticCurrent;

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
        /// <param name="tauPostSynapticCurrentDecay">Synapse's post-synaptic current decay time constant (ms)</param>
        /// <param name="applyShortTermPlasticity">Specifies whether to apply short-term plasticity (if possible).</param>
        /// <param name="applyPostSynapticCurrent">Specifies whether to apply post-synaptic current injections (if possible).</param>
        public InternalSynapse(INeuron sourceNeuron,
                               INeuron targetNeuron,
                               double weight,
                               double tauFacilitation,
                               double tauDepression,
                               double restingEfficacy,
                               double tauPostSynapticCurrentDecay,
                               bool applyShortTermPlasticity = true,
                               bool applyPostSynapticCurrent = true
                               )
            : base(sourceNeuron, targetNeuron, weight)
        {
            //Short-term-plasticity
            _tauFacilitation = tauFacilitation;
            _tauDepression = tauDepression;
            _restingEfficacy = restingEfficacy;
            _facilitation = _restingEfficacy;
            _depression = 1;
            _applyShortTermPlasticity = applyShortTermPlasticity && (SourceNeuron.OutputType == CommonEnums.NeuronSignalType.Spike);
            //Post-synaptic current
            _tauPostSynapticCurrentDecay = tauPostSynapticCurrentDecay;
            _t = 0;
            _stopPostSynapticCurrent = false;
            _applyPostSynapticCurrent = applyPostSynapticCurrent;
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
            _depression = 1;
            _t = 0;
            _stopPostSynapticCurrent = false;
            if (statistics)
            {
                EfficacyStat.Reset();
            }
            return;
        }

        /// <summary>
        /// Sets the synapse signal delay
        /// </summary>
        /// <param name="delay">Signal delay (reservoir cycles)</param>
        public void SetDelay(int delay)
        {
            throw new NotImplementedException("Setting delay is not possible in case of reservoirs internal synapse.");
        }

        /// <summary>
        /// Computes synapse efficacy based on the short-term-plasticity model.
        /// </summary>
        protected double ComputeEfficacy()
        {
            if (_applyShortTermPlasticity)
            {
                if (SourceNeuron.AfterFirstOutputSignal)
                {
                    //Facilitation model
                    double tmp = _facilitation * Math.Exp(-(SourceNeuron.OutputSignalLeak / _tauFacilitation));
                    _facilitation = tmp + _restingEfficacy * (1d - tmp);
                    //Depression model
                    tmp = Math.Exp(-(SourceNeuron.OutputSignalLeak / _tauDepression));
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
        /// Computes post-synaptic current
        /// </summary>
        protected double ComputePostSynapticCurrent()
        {
            ++_t;
            if (_applyPostSynapticCurrent && !_stopPostSynapticCurrent)
            {
                double current = Math.Exp(-(_t / _tauPostSynapticCurrentDecay));
                if(current < MinPostSynapticCurrent)
                {
                    current = 0;
                    _stopPostSynapticCurrent = true;
                }
                return current;
            }
            else
            {
                return 0d;
            }
        }

        /// <summary>
        /// Returns signal to be delivered to target neuron.
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        public double GetSignal(bool collectStatistics)
        {
            //Source neuron signal
            double sourceSignal = SourceNeuron.OutputSignal;
            //No delay of the signal - do not use queue
            if (sourceSignal == 0)
            {
                //No source signal so simply return 0
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
                return sourceSignal * Weight * efficacy + ComputePostSynapticCurrent();
            }
        }

    }//InternalSynapse

}//Namespace
