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
    /// Input neuron is the special type of very simple neuron. Its purpose is only to mediate
    /// external stimulation for a synapse.
    /// </summary>
    [Serializable]
    public class InputAnalogNeuron : INeuron
    {
        //Attributes
        /// <summary>
        /// Range of the input stimuli
        /// </summary>
        private Interval _inputRange;
        
        /// <summary>
        /// Current signal of the neuron according to external input
        /// </summary>
        private double _signal;

        //Attribute properties
        /// <summary>
        /// Input neuron placement is a special case. Input neuron does not belong to a physical pool.
        /// PoolID is allways -1.
        /// </summary>
        public NeuronPlacement Placement { get; }

        /// <summary>
        /// Statistics of incoming stimulations (external input values)
        /// </summary>
        public BasicStat StimuliStat { get; }

        /// <summary>
        /// Statistics of neuron's transmission signal frequency
        /// </summary>
        public BasicStat TransmissionFreqStat { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="inputFieldIdx">Index of correspondent reservoir input field.</param>
        /// <param name="inputRange">
        /// Range of input value.
        /// It is very recommended to have input values normalized and standardized before
        /// they are passed as an input.
        /// </param>
        public InputAnalogNeuron(int inputFieldIdx, Interval inputRange)
        {
            Placement = new NeuronPlacement(inputFieldIdx , - 1, inputFieldIdx, inputFieldIdx, 0, 0);
            _inputRange = inputRange.DeepClone();
            StimuliStat = new BasicStat();
            TransmissionFreqStat = new BasicStat();
            Reset(false);
            return;
        }

        //Properties
        /// <summary>
        /// Output signal range.
        /// In case of input neuron there is no activation function thus the range is the same as input range.
        /// </summary>
        public Interval TransmissionSignalRange { get { return _inputRange; } }

        /// <summary>
        /// Constant bias of the input neuron is allways 0
        /// </summary>
        public double Bias { get { return 0; } }

        /// <summary>
        /// Statistics of neuron state values is a nonsense in case of input neuron
        /// </summary>
        public BasicStat StatesStat { get { return null; } }

        /// <summary>
        /// Determines whether neuron's signal is excitatory or inhibitory.
        /// Input neuron is allways excitatory
        /// </summary>
        public CommonEnums.NeuronSignalType TransmissionSignalType { get { return CommonEnums.NeuronSignalType.Excitatory; } }

        /// <summary>
        /// Transmission signal
        /// </summary>
        public double TransmissionSignal { get { return _signal; } }

        /// <summary>
        /// Statistics of neuron output signals is the same as stimuli stat.
        /// </summary>
        public BasicStat TransmissionSignalStat { get { return StimuliStat; } }

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
            }
            return;
        }

        /// <summary>
        /// Prepares and stores transmission signal
        /// </summary>
        public void PrepareTransmissionSignal()
        {
            //Does nothing. Signal is already prepared by compute function
            return;
        }

        /// <summary>
        /// Prepares and stores readout value
        /// </summary>
        public void PrepareReadoutValue()
        {
            //Does nothing
            return;
        }

        /// <summary>
        /// Computes the neuron.
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
            _signal = stimuli;
            TransmissionFreqStat.AddSampleValue((_signal == 0) ? 0 : 1);
            return;
        }

    }//InputAnalogNeuron

}//Namespace
