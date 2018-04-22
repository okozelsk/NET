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
    public class InputNeuron : INeuron
    {
        //Attributes
        /// <summary>
        /// State value range (allways between -1 and 1)
        /// </summary>
        private static readonly Interval _stateRange = new Interval(-1, 1);

        /// <summary>
        /// Signal value range (allways between -1 and 1)
        /// </summary>
        private static readonly Interval _signalRange = new Interval(-1, 1);

        /// <summary>
        /// Input data range
        /// </summary>
        private Interval _inputRange;

        /// <summary>
        /// Current state of the neuron
        /// </summary>
        private double _state;

        /// <summary>
        /// Current signal of the neuron
        /// </summary>
        private double _signal;

        /// <summary>
        /// Stored signal
        /// </summary>
        private double _storedSignal;

        //Attribute properties
        /// <summary>
        /// Input neuron placement is a special case. Input neuron does not belong to a physical pool.
        /// PoolID is allways -1.
        /// </summary>
        public NeuronPlacement Placement { get; }

        /// <summary>
        /// Constant bias of the neuron
        /// </summary>
        public double Bias { get; }

        /// <summary>
        /// Statistics of incoming stimulations (input values)
        /// </summary>
        public BasicStat StimuliStat { get; }
        
        /// <summary>
        /// Statistics of neuron state values (output signals)
        /// </summary>
        public BasicStat StatesStat { get; }

        /// <summary>
        /// Statistics of neuron output signals
        /// </summary>
        public BasicStat SignalStat { get; }

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
        public InputNeuron(int inputFieldIdx, Interval inputRange)
        {
            Placement = new NeuronPlacement(inputFieldIdx , - 1, inputFieldIdx, inputFieldIdx, 0, 0);
            Bias = 0;
            _inputRange = new Interval(inputRange.Min.Bound(), inputRange.Max.Bound());
            StimuliStat = new BasicStat();
            StatesStat = new BasicStat();
            SignalStat = new BasicStat();
            Reset(false);
            return;
        }

        //Properties
        /// <summary>
        /// Current state of the neuron
        /// </summary>
        public double State { get { return _state; } }

        /// <summary>
        /// Current signal according to current state
        /// </summary>
        public double CurrentSignal { get { return _signal; } }

        /// <summary>
        /// Stored output signal for transmission purposes
        /// </summary>
        public double StoredSignal { get { return _storedSignal; } }

        //Methods
        /// <summary>
        /// Resets the neuron to its initial state
        /// </summary>
        /// <param name="statistics">Specifies whether to reset also internal statistics</param>
        public void Reset(bool statistics)
        {
            _state = 0;
            _signal = 0;
            _storedSignal = 0;
            if (statistics)
            {
                StimuliStat.Reset();
                StatesStat.Reset();
                SignalStat.Reset();
            }
            return;
        }

        /// <summary>
        /// Stores current signal to be used for signal transmission
        /// </summary>
        public void StoreSignal()
        {
            _storedSignal = _signal;
            return;
        }

        /// <summary>
        /// Computes neuron state and output signal.
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
            _state = _stateRange.Min + (((stimuli - _inputRange.Min) / _inputRange.Span) * _stateRange.Span);
            _signal = _state;
            if (collectStatistics)
            {
                StatesStat.AddSampleValue(_state);
                SignalStat.AddSampleValue(_signal);
            }
            return;
        }

    }//InputNeuron

}//Namespace
