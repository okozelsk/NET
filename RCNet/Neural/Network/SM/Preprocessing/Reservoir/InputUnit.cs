using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;
using RCNet.Extensions;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron;


namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
    /// <summary>
    /// Provides input to be processed in the reservoir. Supports analog and spike-train codings.
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

        //Static attributes
        /// <summary>
        /// Commonly used 0,1 interval
        /// </summary>
        private static readonly Interval ZeroOneRange = new Interval(0d, 1d);

        //Attribute properties
        /// <summary>
        /// Index of the associated input field
        /// </summary>
        public int InputFieldIdx { get; }

        /// <summary>
        /// InputNeuron providing analog value
        /// </summary>
        public InputNeuron AnalogInputNeuron { get; }

        /// <summary>
        /// Collection of InputNeurons representing spike train of analog value
        /// </summary>
        public InputNeuron[] SpikeTrainInputNeuronCollection { get; }

        //Attributes
        private readonly Interval _inputRange;
        private readonly InputUnitSettings _settings;
        private readonly double _precisionPiece;
        private readonly ulong _maxPrecisionBitMask;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="reservoirID">Reservoir ID</param>
        /// <param name="inputRange">Input data range</param>
        /// <param name="inputFieldIdx">Index of the associated input field</param>
        /// <param name="inputNeuronsStartIdx">Index of the first input neuron of this unit among all input neurons</param>
        /// <param name="settings">Configuration parameters.</param>
        public InputUnit(int reservoirID, Interval inputRange, int inputFieldIdx, int inputNeuronsStartIdx, InputUnitSettings settings)
        {
            _inputRange = inputRange.DeepClone();
            _settings = (InputUnitSettings)settings.DeepClone();
            _precisionPiece = _inputRange.Span / Math.Pow(2d, _settings.SpikeTrainLength);
            _maxPrecisionBitMask = (uint)Math.Round(Math.Pow(2d, _settings.SpikeTrainLength) - 1d);
            InputFieldIdx = inputFieldIdx;
            AnalogInputNeuron = new InputNeuron(reservoirID,
                                                _settings.CoordinatesCfg.GetCoordinates(),
                                                inputNeuronsStartIdx++,
                                                _inputRange,
                                                NeuronCommon.NeuronSignalingRestrictionType.AnalogOnly
                                                );
            SpikeTrainInputNeuronCollection = new InputNeuron[_settings.SpikeTrainLength];
            for (int i = 0; i < _settings.SpikeTrainLength; i++)
            {
                SpikeTrainInputNeuronCollection[i] = new InputNeuron(reservoirID,
                                                                     _settings.CoordinatesCfg.GetCoordinates(),
                                                                     inputNeuronsStartIdx++,
                                                                     ZeroOneRange,
                                                                     NeuronCommon.NeuronSignalingRestrictionType.SpikingOnly
                                                                     );
            }
            return;
        }

        //Properties
        /// <summary>
        /// Total number of input neurons
        /// </summary>
        public int NumOfInputNeurons { get { return (1 + _settings.SpikeTrainLength); } }

        //Methods
        /// <summary>
        /// Transforms analog input to a binary sequence (spike-train)
        /// </summary>
        /// <param name="analogInput">Analog input value</param>
        private uint GetSpikeTrain(double analogInput)
        {
            double movedDataValue = Math.Max(0d, analogInput - _inputRange.Min);
            uint pieces = (uint)Math.Min(Math.Floor(movedDataValue / _precisionPiece), (double)_maxPrecisionBitMask);
            return pieces;
        }

        /// <summary>
        /// Resets all associated input neurons to initial state
        /// </summary>
        /// <param name="statistics">Specifies whether to reset internal statistics of the associated neurons</param>
        public void Reset(bool statistics)
        {
            AnalogInputNeuron.Reset(statistics);
            for (int i = 0; i < SpikeTrainInputNeuronCollection.Length; i++)
            {
                SpikeTrainInputNeuronCollection[i].Reset(statistics);
            }
            return;
        }

        /// <summary>
        /// Forces input neurons to accept new incoming stimulation.
        /// </summary>
        /// <param name="iStimuli">External input analog signal</param>
        public void NewStimulation(double iStimuli)
        {
            AnalogInputNeuron.NewStimulation(iStimuli, 0d);
            uint unchSpikeTrainBits = GetSpikeTrain(iStimuli);
            for (int i = 0; i < SpikeTrainInputNeuronCollection.Length; i++)
            {
                double spikeVal = Bitwise.GetBit(unchSpikeTrainBits, i);
                SpikeTrainInputNeuronCollection[i].NewStimulation(spikeVal, 0d);
            }
            return;
        }

        /// <summary>
        /// Forces input neurons to compute new output signal.
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics of associated input neurons</param>
        public void ComputeSignal(bool collectStatistics)
        {
            AnalogInputNeuron.Recompute(collectStatistics);
            foreach (InputNeuron neuron in SpikeTrainInputNeuronCollection)
            {
                neuron.Recompute(collectStatistics);
            }
            return;
        }

    }//InputUnit

}//Namespace
