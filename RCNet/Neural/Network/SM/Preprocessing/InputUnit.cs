using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;
using RCNet.Extensions;
using RCNet.Neural.Network.SM.Neuron;


namespace RCNet.Neural.Network.SM.Preprocessing
{
    /// <summary>
    /// Provides external input for processing in the reservoir. Supports analog and spike-train codings.
    /// Used spike-train coding method is from the "Sparse coding" family.
    /// </summary>
    [Serializable]
    public class InputUnit
    {
        //Constants
        /// <summary>
        /// Maximum length of the spike-train (number of bits) representing input analog data
        /// </summary>
        public const int SpikeTrainMaxLength = 32;

        //Static members
        private static readonly uint[] _bitValuesCache;

        //Attribute properties
        /// <summary>
        /// Input neuron providing analog signal
        /// </summary>
        public InputNeuron AnalogInputNeuron { get; }
        /// <summary>
        /// Set of Input neurons providing spiking signal.
        /// Collection of spiking input neurons is a "spike-train" representing analog input value.
        /// </summary>
        public InputNeuron[] SpikingInputNeuronCollection { get; }

        //Attributes
        private readonly Interval _inputRange;
        private readonly int _spikeTrainLength;
        private readonly double _precisionPiece;
        private readonly uint _maxPrecisionBitMask;

        //Constructors
        /// <summary>
        /// Static constructor
        /// </summary>
        static InputUnit()
        {
            _bitValuesCache = new uint[SpikeTrainMaxLength];
            for(int i = 0; i < SpikeTrainMaxLength; i++)
            {
                _bitValuesCache[i] = (uint)Math.Round(Math.Pow(2d, i));
            }
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="inputEntryPoint">Input entry point coordinates within the reservoir.</param>
        /// <param name="inputFieldIdx">Index of the corresponding reservoir's input field.</param>
        /// <param name="inputRange">Input data range</param>
        /// <param name="spikeTrainLength">Length of the spike-train (number of bits) representing input analog value</param>
        public InputUnit(int[] inputEntryPoint, int inputFieldIdx, Interval inputRange, int spikeTrainLength)
        {
            _inputRange = inputRange.DeepClone();
            _spikeTrainLength = Math.Max(1, Math.Min(spikeTrainLength, SpikeTrainMaxLength));
            _precisionPiece = _inputRange.Span / Math.Pow(2d, _spikeTrainLength);
            _maxPrecisionBitMask = (uint)Math.Round(Math.Pow(2d, _spikeTrainLength) - 1d);
            AnalogInputNeuron = new InputNeuron(inputEntryPoint, inputFieldIdx, inputRange, NeuronCommon.NeuronSignalingRestrictionType.AnalogOnly);
            SpikingInputNeuronCollection = new InputNeuron[_spikeTrainLength];
            for(int i = 0; i < _spikeTrainLength; i++)
            {
                SpikingInputNeuronCollection[i] = new InputNeuron(inputEntryPoint, inputFieldIdx, inputRange, NeuronCommon.NeuronSignalingRestrictionType.SpikingOnly);
            }
            return;
        }

        //Methods
        /// <summary>
        /// Transforms input analog data to a binary sequence (spike-train)
        /// </summary>
        /// <param name="inputAnalogData">Input analog data</param>
        private uint GetSpikeTrain(double inputAnalogData)
        {
            double movedDataValue = Math.Max(0d, inputAnalogData - _inputRange.Min);
            uint pieces = (uint)Math.Min(Math.Floor(movedDataValue / _precisionPiece), (double)_maxPrecisionBitMask);
            return pieces;
        }

        /// <summary>
        /// Checks if specified spike should be emmited or not.
        /// (In other words: function checks if bit is set at specified position in the given sequence of bits)
        /// </summary>
        /// <param name="spikeTrain">Spike-train (sequence of bits)</param>
        /// <param name="spikeIdx">Index of the spike (bit) to be checked</param>
        private double GetSpikeValue(uint spikeTrain, int spikeIdx)
        {
            return ((spikeTrain & _bitValuesCache[spikeIdx]) == 0) ? 0d : 1d;
        }

        /// <summary>
        /// Resets all associated input neurons to initial state
        /// </summary>
        /// <param name="statistics">Specifies whether to reset internal statistics of the associated neurons</param>
        public void Reset(bool statistics)
        {
            AnalogInputNeuron.Reset(statistics);
            foreach(InputNeuron neuron in SpikingInputNeuronCollection)
            {
                neuron.Reset(statistics);
            }
            return;
        }

        /// <summary>
        /// Forces associated input neurons to accept new incoming stimulation.
        /// </summary>
        /// <param name="iStimuli">External input analog signal</param>
        public void NewStimulation(double iStimuli)
        {
            AnalogInputNeuron.NewStimulation(iStimuli, 0d);
            uint bits = GetSpikeTrain(iStimuli);
            for(int i = 0; i < _spikeTrainLength; i++)
            {
                SpikingInputNeuronCollection[i].NewStimulation(GetSpikeValue(bits, i), 0d);
            }
            return;
        }

        /// <summary>
        /// Forces associated input neurons to compute new output signal.
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics of associated input neurons</param>
        public void ComputeSignal(bool collectStatistics)
        {
            AnalogInputNeuron.ComputeSignal(collectStatistics);
            foreach (InputNeuron neuron in SpikingInputNeuronCollection)
            {
                neuron.ComputeSignal(collectStatistics);
            }
            return;
        }

    }//InputUnit

}//Namespace
