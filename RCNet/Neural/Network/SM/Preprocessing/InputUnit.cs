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
    /// Provides external input for processing in the reservoir. Supports analog and spike-train realtime codings.
    /// Used spike-train coding method is from the "Sparse coding" family.
    /// </summary>
    [Serializable]
    public class InputUnit
    {
        //Enums
        /// <summary>
        /// Enumerates supported analog coding methods
        /// </summary>
        public enum AnalogCodingMethod
        {
            /// <summary>
            /// Original analog data value (1:1 coding)
            /// </summary>
            Original,
            /// <summary>
            /// Difference of the current and one of the past data value
            /// </summary>
            Difference,
            /// <summary>
            /// Linear steps
            /// </summary>
            LinearSteps,
            /// <summary>
            /// Square (power of 2), Square root (power of 1/2), Cube (power of 3), Cube root (power of 1/3) or another specified power of the original data value
            /// </summary>
            Power,
            /// <summary>
            /// power(x) - power(1-x)
            /// </summary>
            FoldedPower,
            /// <summary>
            /// Moving average of specified length (up to MovingDataWindowMaxSize)
            /// </summary>
            MovingAverage
        }

        //Constants
        /// <summary>
        /// Maximum length of the spike-train (number of bits) representing input analog data
        /// </summary>
        public const int SpikeTrainMaxLength = 32;
        /// <summary>
        /// Maximum number of the past analog values available in the moving data window
        /// </summary>
        public const int MovingDataWindowMaxSize = 1024;

        //Static attributes
        /// <summary>
        /// Number of supported analog coding methods
        /// </summary>
        public static readonly int NumOfAnalogCodingMethods = typeof(AnalogCodingMethod).GetEnumValues().Length;
        /// <summary>
        /// Commonly used 0,1 interval
        /// </summary>
        private static readonly Interval ZeroOneRange = new Interval(0d, 1d);

        //Attribute properties
        /// <summary>
        /// Set of Input neurons providing analog signals (original and trnsformed)
        /// </summary>
        public InputNeuron[] AnalogInputNeuronCollection { get; }
        /// <summary>
        /// Set of Input neurons providing spiking signal.
        /// Collection of spiking input neurons expressing a "spike-train" representing analog input value.
        /// </summary>
        public InputNeuron[] SpikingInputNeuronCollection { get; }

        //Attributes
        private readonly Interval _inputRange;
        private readonly int _spikeTrainLength;
        private readonly double _precisionPiece;
        private readonly ulong _maxPrecisionBitMask;
        private readonly MovingDataWindow _movingDW;
        private readonly int _diffDistance;
        private readonly int _numOfLinearSteps;
        private readonly double _powerExponent;
        private readonly double _foldedPowerExponent;
        private readonly int _movingAverageWindowLength;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="inputEntryPoint">Input entry point coordinates within the reservoir.</param>
        /// <param name="inputFieldIdx">Index of the corresponding reservoir's input field.</param>
        /// <param name="inputRange">Input data range.</param>
        /// <param name="spikeTrainLength">Length of the spike-train (number of bits) representing input analog value.</param>
        /// <param name="diffDistance">Distance of the past value for the computation of the difference of the current and the past value.</param>
        /// <param name="numOfLinearSteps">Number of steps dividing data interval of the Linear steps transformation.</param>
        /// <param name="powerExponent">Exponent of the Power transformation.</param>
        /// <param name="foldedPowerExponent">Exponent of the FoldedPower transformation.</param>
        /// <param name="movingAverageWindowLength">Number of the last data values involved in moving average transformation.</param>
        public InputUnit(int[] inputEntryPoint,
                         int inputFieldIdx,
                         Interval inputRange,
                         int spikeTrainLength,
                         int diffDistance,
                         int numOfLinearSteps,
                         double powerExponent,
                         double foldedPowerExponent,
                         int movingAverageWindowLength
                         )
        {
            _inputRange = inputRange.DeepClone();
            _spikeTrainLength = Math.Max(1, Math.Min(spikeTrainLength, SpikeTrainMaxLength));
            _precisionPiece = _inputRange.Span / Math.Pow(2d, _spikeTrainLength);
            _maxPrecisionBitMask = (uint)Math.Round(Math.Pow(2d, _spikeTrainLength) - 1d);
            AnalogInputNeuronCollection = new InputNeuron[NumOfAnalogCodingMethods];
            for(int i = 0; i < NumOfAnalogCodingMethods; i++)
            {
                AnalogInputNeuronCollection[i] = new InputNeuron(inputEntryPoint, inputFieldIdx, inputRange, NeuronCommon.NeuronSignalingRestrictionType.AnalogOnly);
            }
            SpikingInputNeuronCollection = new InputNeuron[_spikeTrainLength];
            for(int i = 0; i < _spikeTrainLength; i++)
            {
                SpikingInputNeuronCollection[i] = new InputNeuron(inputEntryPoint, inputFieldIdx, inputRange, NeuronCommon.NeuronSignalingRestrictionType.SpikingOnly);
            }
            _movingDW = new MovingDataWindow(MovingDataWindowMaxSize);
            _diffDistance = Math.Max(Math.Min(MovingDataWindowMaxSize, diffDistance), 1);
            _numOfLinearSteps = numOfLinearSteps;
            _powerExponent = powerExponent;
            _foldedPowerExponent = foldedPowerExponent;
            _movingAverageWindowLength = Math.Max(Math.Min(MovingDataWindowMaxSize, movingAverageWindowLength), 1);
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
        /// Resets all associated input neurons to initial state
        /// </summary>
        /// <param name="statistics">Specifies whether to reset internal statistics of the associated neurons</param>
        public void Reset(bool statistics)
        {
            foreach (InputNeuron neuron in AnalogInputNeuronCollection)
            {
                neuron.Reset(statistics);
            }
            foreach(InputNeuron neuron in SpikingInputNeuronCollection)
            {
                neuron.Reset(statistics);
            }
            _movingDW.Reset();
            return;
        }

        /// <summary>
        /// Forces associated input neurons to accept new incoming stimulation.
        /// </summary>
        /// <param name="iStimuli">External input analog signal</param>
        public void NewStimulation(double iStimuli)
        {
            //////////////////////////////////////////
            //Analog codings
            //Original
            AnalogInputNeuronCollection[(int)(AnalogCodingMethod.Original)].NewStimulation(iStimuli, 0d);
            //Difference
            double diff = 0d;
            if(_movingDW.NumOfSamples >= _diffDistance)
            {
                diff = (iStimuli - _movingDW.GetAt(_diffDistance - 1, true)) / _inputRange.Span;
            }
            AnalogInputNeuronCollection[(int)(AnalogCodingMethod.Difference)].NewStimulation(diff, 0d);
            //Steps
            double zeroOneStimuli = ZeroOneRange.Rescale(iStimuli, _inputRange);
            double stepPiece = ZeroOneRange.Span / _numOfLinearSteps;
            double stimuliStepPieces = Math.Ceiling(zeroOneStimuli / stepPiece).Bound(0, _numOfLinearSteps);
            double stimuliStepsRatio = stimuliStepPieces / _numOfLinearSteps;
            double linearSteps = _inputRange.Rescale(stimuliStepsRatio, ZeroOneRange);
            AnalogInputNeuronCollection[(int)(AnalogCodingMethod.LinearSteps)].NewStimulation(linearSteps, 0d);
            //Power
            double power = Math.Sign(iStimuli) * Math.Pow(Math.Abs(iStimuli), _powerExponent);
            AnalogInputNeuronCollection[(int)(AnalogCodingMethod.Power)].NewStimulation(power, 0d);
            //Folded power
            double foldedPower = Math.Pow(zeroOneStimuli, _foldedPowerExponent) - Math.Pow(1d - zeroOneStimuli, _foldedPowerExponent);
            //foldedPower = _inputRange.Rescale(foldedPower, ZeroOneRange);
            AnalogInputNeuronCollection[(int)(AnalogCodingMethod.FoldedPower)].NewStimulation(foldedPower, 0d);
            //Moving average
            _movingDW.AddSampleValue(iStimuli);
            double wAvg = _movingDW.NumOfSamples >= _movingAverageWindowLength ? _movingDW.GetWeightedAvg(null, true, _movingAverageWindowLength).Avg : 0d;
            AnalogInputNeuronCollection[(int)(AnalogCodingMethod.MovingAverage)].NewStimulation(wAvg, 0d);

            //////////////////////////////////////////
            //Spike-train coding
            uint bits = GetSpikeTrain(iStimuli);
            for(int i = 0; i < _spikeTrainLength; i++)
            {
                SpikingInputNeuronCollection[i].NewStimulation(Bitwise.GetBit(bits, i), 0d);
            }
            return;
        }

        /// <summary>
        /// Forces associated input neurons to compute new output signal.
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics of associated input neurons</param>
        public void ComputeSignal(bool collectStatistics)
        {
            foreach (InputNeuron neuron in AnalogInputNeuronCollection)
            {
                neuron.Recompute(collectStatistics);
            }
            foreach (InputNeuron neuron in SpikingInputNeuronCollection)
            {
                neuron.Recompute(collectStatistics);
            }
            return;
        }

    }//InputUnit

}//Namespace
