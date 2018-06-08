using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Spiking input neuron is the special type of neuron. Its purpose is to preprocess input analog value
    /// to be deliverable as the spike train signal into the reservoir neurons.
    /// </summary>
    [Serializable]
    public class InputSpikingNeuron : INeuron
    {
        /// <summary>
        /// Common output range 0/1 - no spike/spike
        /// </summary>
        private static Interval _transmissionSignalRange = new Interval(0, 1);

        /// <summary>
        /// Input data range
        /// </summary>
        private Interval _inputRange;

        /// <summary>
        /// Analog input to spike train converter
        /// </summary>
        private SignalConverter _signalConverter;

        /// <summary>
        /// Transmission signal of the neuron
        /// </summary>
        private double _signal;

        //Attribute properties
        /// <summary>
        /// Input neuron placement is a special case. Input neuron does not belong to a physical pool.
        /// PoolID is allways -1.
        /// </summary>
        public NeuronPlacement Placement { get; }

        /// <summary>
        /// Statistics of incoming stimulations (input values)
        /// </summary>
        public BasicStat StimuliStat { get; }

        /// <summary>
        /// Statistics of neuron output signals
        /// </summary>
        public BasicStat TransmissionSignalStat { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="inputFieldIdx">Index of corresponding reservoir input field.</param>
        /// <param name="inputRange">
        /// Range of input value.
        /// It is very recommended to have input values normalized and standardized before
        /// they are passed to input neuron.
        /// </param>
        /// <param name="inputCodingFractions">Number of coding fractions (see SpikeTrainConverter to understand)</param>
        public InputSpikingNeuron(int inputFieldIdx, Interval inputRange, int inputCodingFractions)
        {
            Placement = new NeuronPlacement(inputFieldIdx , - 1, inputFieldIdx, inputFieldIdx, 0, 0);
            _inputRange = new Interval(inputRange.Min.Bound(), inputRange.Max.Bound());
            _signalConverter = new SignalConverter(_inputRange, inputCodingFractions);
            StimuliStat = new BasicStat();
            TransmissionSignalStat = new BasicStat();
            Reset(false);
            return;
        }

        //Properties
        /// <summary>
        /// Output signal range.
        /// In case of input spiking neuron there is no activation function thus the range is in all cases the same: 0(no spike)/1(spike).
        /// </summary>
        public Interval TransmissionSignalRange { get { return _transmissionSignalRange; } }

        /// <summary>
        /// Constant bias of the input neuron is allways 0
        /// </summary>
        public double Bias { get { return 0; } }

        /// <summary>
        /// Input spiking neuron is allways excitatory
        /// </summary>
        public CommonEnums.NeuronSignalType TransmissionSignalType { get { return CommonEnums.NeuronSignalType.Excitatory; } }

        /// <summary>
        /// Statistics of neuron state values is a nonsense in case of input neuron
        /// </summary>
        public BasicStat StatesStat { get { return null; } }

        /// <summary>
        /// Neuron's transmission signal
        /// </summary>
        public double TransmissionSignal { get { return _signal; } }

        /// <summary>
        /// Statistics of neuron's transmission signal frequency
        /// </summary>
        public BasicStat TransmissionFreqStat { get { return TransmissionSignalStat; } }

        /// <summary>
        /// Value to be passed to readout layer as a predictor value is a nonsense in case of input neuron
        /// </summary>
        public double ReadoutValue { get { return double.NaN; } }

        /// <summary>
        /// Value to be passed to readout layer as an augmented predictor value is a nonsense in case of input neuron
        /// </summary>
        public double ReadoutAugmentedValue { get { return double.NaN; } }

        //Methods
        /// <summary>
        /// Resets the neuron to its initial state
        /// </summary>
        /// <param name="resetStatistics">Specifies whether to reset internal statistics</param>
        public void Reset(bool resetStatistics)
        {
            _signal = 0;
            if (resetStatistics)
            {
                StimuliStat.Reset();
                TransmissionSignalStat.Reset();
            }
            return;
        }

        /// <summary>
        /// Prepares and stores transmission signal
        /// </summary>
        public void PrepareTransmissionSignal()
        {
            _signal = _signalConverter.FetchSpike();
            TransmissionSignalStat.AddSampleValue(_signal);
            return;
        }

        /// <summary>
        /// Computes the neuron
        /// </summary>
        /// <param name="stimuli">Input stimulation</param>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        public void Compute(double stimuli, bool collectStatistics)
        {
            stimuli = stimuli.Bound();
            if (collectStatistics)
            {
                StimuliStat.AddSampleValue(stimuli);
            }
            _signalConverter.EncodeAnalogValue(stimuli);
            return;
        }

    }//InputSpikingNeuron

}//Namespace
