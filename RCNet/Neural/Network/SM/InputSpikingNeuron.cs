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
        /// Analog to spikes train converter
        /// </summary>
        private SpikeTrainConverter _signalConverter;

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
        public BasicStat TransmissinSignalStat { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="inputFieldIdx">Index of corresponding reservoir input field.</param>
        /// <param name="signalConverter"> Analog to spike train converter (shared instance)</param>
        public InputSpikingNeuron(int inputFieldIdx, SpikeTrainConverter signalConverter)
        {
            Placement = new NeuronPlacement(inputFieldIdx , - 1, inputFieldIdx, inputFieldIdx, 0, 0);
            _signalConverter = signalConverter;
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
        public double TransmissinSignal { get { return _signal; } }

        /// <summary>
        /// Value to be passed to readout layer as a predictor value is a nonsense in case of input neuron
        /// </summary>
        public double ReadoutPredictorValue { get { return double.NaN; } }

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
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        public void PrepareTransmissionSignal(bool collectStatistics)
        {
            _signal = _signalConverter.FetchSpike();
            if(collectStatistics)
            {
                TransmissinSignalStat.AddSampleValue(_signal);
            }
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
