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
    /// Input neuron is the special type of neuron. Its purpose is to preprocess input value to be deliverable as the
    /// signal into the reservoir neurons by the standard way through a synapse.
    /// </summary>
    [Serializable]
    public class InputAnalogNeuron : INeuron
    {
        //Attributes
        /// <summary>
        /// Signal value range (allways between 0 and 1)
        /// </summary>
        private static readonly Interval _signalRange = new Interval(0, 1);

        /// <summary>
        /// Input data range
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
        /// Statistics of neuron output signals
        /// </summary>
        public BasicStat TransmissinSignalStat { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="inputFieldIdx">Index of correspondent reservoir input field.</param>
        /// <param name="inputRange">
        /// Range of input value.
        /// It is very recommended to have input values normalized and standardized before
        /// they are passed to input neuron.
        /// </param>
        public InputAnalogNeuron(int inputFieldIdx, Interval inputRange)
        {
            Placement = new NeuronPlacement(inputFieldIdx , - 1, inputFieldIdx, inputFieldIdx, 0, 0);
            _inputRange = new Interval(inputRange.Min.Bound(), inputRange.Max.Bound());
            StimuliStat = new BasicStat();
            TransmissinSignalStat = new BasicStat();
            Reset(false);
            return;
        }

        //Properties
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
        public double TransmissinSignal { get { return _signal; } }

        /// <summary>
        /// Value to be passed to readout layer as a predictor value is a nonsense in case of input neuron
        /// </summary>
        public double ReadoutPredictorValue { get { return double.NaN; } }

        /// <summary>
        /// Value to be passed to readout layer as an augmented predictor value is a nonsense in case of input neuron
        /// </summary>
        public double ReadoutAugmentedPredictorValue { get { return double.NaN; } }

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
                TransmissinSignalStat.Reset();
            }
            return;
        }

        /// <summary>
        /// Prepares and stores transmission signal
        /// </summary>
        public void PrepareTransmissionSignal()
        {
            //Signal is already prepared by compute function
            //Statistics
            TransmissinSignalStat.AddSampleValue(_signal);
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
            //Range transformation
            _signal = _signalRange.Rescale(stimuli, _inputRange);
            return;
        }

    }//InputAnalogNeuron

}//Namespace
