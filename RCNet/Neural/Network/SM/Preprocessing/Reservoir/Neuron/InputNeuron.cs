using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Activation;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron
{
    /// <summary>
    /// Input neuron is the special type of very simple neuron. Its purpose is only to mediate
    /// external input for a synapse.
    /// </summary>
    [Serializable]
    public class InputNeuron : INeuron
    {
        //Static attributes
        private static readonly Interval _spikingOutputRange = new Interval(0, 1);

        //Attribute properties
        /// <summary>
        /// Information about a neuron location within the neural preprocessor
        /// Note that Input neuron home PoolID is always -1 because Input neurons do not belong to a standard pools.
        /// </summary>
        public NeuronLocation Location { get; }

        /// <summary>
        /// Neuron's key statistics
        /// </summary>
        public NeuronStatistics Statistics { get; }

        /// <summary>
        /// Neuron type
        /// </summary>
        public NeuronCommon.NeuronType Type { get { return NeuronCommon.NeuronType.Input; } }

        /// <summary>
        /// Type of the activation function
        /// </summary>
        public ActivationType TypeOfActivation
        {
            get
            {
                if(SignalingRestriction == NeuronCommon.NeuronSignalingRestrictionType.SpikingOnly)
                {
                    return ActivationType.Spiking;
                }
                else
                {
                    return ActivationType.Analog;
                }
            }
        }

        /// <summary>
        /// Output signaling restriction
        /// </summary>
        public NeuronCommon.NeuronSignalingRestrictionType SignalingRestriction { get; }

        /// <summary>
        /// Constant bias.
        /// </summary>
        public double Bias { get { return 0; } }

        /// <summary>
        /// Computation cycles gone from the last emitted spike or start (if no spike emitted before current computation cycle)
        /// </summary>
        public int SpikeLeak { get; private set; }

        /// <summary>
        /// Specifies, if neuron has already emitted spike before current computation cycle
        /// </summary>
        public bool AfterFirstSpike { get; private set; }

        //Attributes
        private readonly Interval _inputRange;
        private double _iStimuli;
        private double _rStimuli;
        private double _tStimuli;
        private double _analogSignal;
        private double _spikingSignal;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="reservoirID">Reservoir ID</param>
        /// <param name="inputEntryPoint">Input entry point coordinates within the 3D space</param>
        /// <param name="flatIdx">Index of this neuron among all input neurons.</param>
        /// <param name="inputRange">Range of input value</param>
        /// <param name="signalingRestriction">Distinguish between analog/spiking provided signal (NoRestriction is forbidden)</param>
        public InputNeuron(int reservoirID,
                           int[] inputEntryPoint,
                           int flatIdx,
                           Interval inputRange,
                           NeuronCommon.NeuronSignalingRestrictionType signalingRestriction
                           )
        {
            Location = new NeuronLocation(reservoirID, flatIdx, - 1, flatIdx, 0, inputEntryPoint[0], inputEntryPoint[1], inputEntryPoint[2]);
            _inputRange = inputRange.DeepClone();
            if(signalingRestriction == NeuronCommon.NeuronSignalingRestrictionType.NoRestriction)
            {
                throw new ArgumentException("Invalid signaling restriction. NeuronSignalingRestrictionType.NoRestriction is forbidden for the InputNeuron.", "signalingRestriction");
            }
            SignalingRestriction = signalingRestriction;
            Statistics = new NeuronStatistics();
            Reset(false);
            return;
        }

        //Methods
        /// <summary>
        /// Resets neuron to its initial state
        /// </summary>
        /// <param name="statistics">Specifies whether to reset internal statistics</param>
        public void Reset(bool statistics)
        {
            _iStimuli = 0;
            _rStimuli = 0;
            _tStimuli = 0;
            _analogSignal = 0;
            _spikingSignal = 0;
            SpikeLeak = 0;
            AfterFirstSpike = false;
            if (statistics)
            {
                Statistics.Reset();
            }
            return;
        }

        /// <summary>
        /// Stores new incoming stimulation.
        /// </summary>
        /// <param name="iStimuli">External input analog signal</param>
        /// <param name="rStimuli">Parameter is ignored. Stimulation comming from reservoir neurons is irrelevant. </param>
        public void NewStimulation(double iStimuli, double rStimuli)
        {
            _iStimuli = iStimuli;
            _rStimuli = 0;
            _tStimuli = (_iStimuli + _rStimuli + Bias).Bound();
            return;
        }

        /// <summary>
        /// Prepares new output signal (input for hidden neurons).
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        public void Recompute(bool collectStatistics)
        {
            //Spike leak and first spike handling
            if (SignalingRestriction == NeuronCommon.NeuronSignalingRestrictionType.SpikingOnly)
            {
                if (_spikingSignal > 0)
                {
                    //Spike during previous cycle, so reset the counter
                    AfterFirstSpike = true;
                    SpikeLeak = 0;
                }
                ++SpikeLeak;
            }
            //Analog signal is exactly the same as stimulation
            _analogSignal = _tStimuli;
            //Spiking signal must be always between 0 and 1
            _spikingSignal = _spikingOutputRange.Rescale(_tStimuli, _inputRange);
            //Statistics
            if (collectStatistics)
            {
                Statistics.Update(_iStimuli, _rStimuli, _tStimuli, _tStimuli, _analogSignal, _spikingSignal);
            }
            return;
        }

        /// <summary>
        /// Returns input for hidden neuron having activation of specified type.
        /// </summary>
        /// <param name="targetActivationType">Specifies what type of the signal is required.</param>
        public double GetSignal(ActivationType targetActivationType)
        {
            return targetActivationType == ActivationType.Spiking ? _spikingSignal : _analogSignal;
        }


    }//InputNeuron

}//Namespace
