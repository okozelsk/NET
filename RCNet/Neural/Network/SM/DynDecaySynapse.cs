using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Inplements synapse having dynamic expenential decay behavior based on target neuron spiking frequency.
    /// </summary>
    [Serializable]
    public class DynDecaySynapse : Synapse
    {
        //Attributes
        private readonly double _tauDecay;
        private readonly bool _applyAdaptation;

        //Constructor
        /// <summary>
        /// Creates initialized instance
        /// </summary>
        /// <param name="sourceNeuron">Source neuron</param>
        /// <param name="targetNeuron">Target neuron</param>
        /// <param name="maxWeight">Synapse weight</param>
        /// <param name="maxDelay">Maximum delay (in cycles) of the signal delivery</param>
        /// <param name="tauDecay">Decay shapness (lower = sharper)</param>
        public DynDecaySynapse(INeuron sourceNeuron,
                              INeuron targetNeuron,
                              double maxWeight,
                              int maxDelay,
                              double tauDecay
                              )
            :base(sourceNeuron, targetNeuron, maxWeight, maxDelay)
        {
            _tauDecay = tauDecay;
            _applyAdaptation = (TargetNeuron.OutputType == Activation.ActivationFactory.FunctionOutputSignalType.Spike);
            return;
        }

        //Methods
        /// <summary>
        /// Resets synapse.
        /// </summary>
        public override void Reset()
        {
            //Does nothing in case of the dynamic synapse
            return;
        }

        /// <summary>
        /// Computes weighted signal and puts it into the internal queue
        /// </summary>
        protected override void EnqueueSignal()
        {
            double signal = ((SourceNeuron.OutputSignal + _add) / _div) * Weight;
            if (_applyAdaptation && SourceNeuron.OutputSignal > 0)
            {
                signal *= Math.Exp(-((TargetNeuron.NoSignalCycles + 1) / _tauDecay));
                //signal *= Math.Exp(-((TargetNeuron.NoSignalCycles + 1) / ((Weight < 0) ? 1 : _tauDecay)));
            }
            _qSig.Enqueue(signal);
            return;
        }

    }//DynDecaySynapse

}//Namespace
