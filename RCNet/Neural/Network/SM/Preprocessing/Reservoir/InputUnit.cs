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
            /// Actual analog data value (1:1 coding)
            /// </summary>
            Actual,
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
            MovingAverage,
            /// <summary>
            /// Morlet wavelet
            /// </summary>
            Morlet
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
        /// <summary>
        /// Commonly used -1,1 interval
        /// </summary>
        private static readonly Interval MOnePOneRange = new Interval(-1d, 1d);

        //Attribute properties
        public int InputFieldIdx { get; }

        //Attributes
        private readonly int _reservoirID;
        private readonly Interval _inputRange;
        private readonly InputUnitSettings _settings;
        private readonly int[] _inputEntryPoint;
        private readonly MovingDataWindow _movingDW;
        private readonly TransformedValueUnit[] _transUnits;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="reservoirID">Reservoir ID</param>
        /// <param name="inputRange">Input data range</param>
        /// <param name="inputFieldIdx">Index of the associated input field</param>
        /// <param name="settings">Configuration parameters.</param>
        public InputUnit(int reservoirID, Interval inputRange, int inputFieldIdx, InputUnitSettings settings)
        {
            _reservoirID = reservoirID;
            _inputRange = inputRange.DeepClone();
            _settings = (InputUnitSettings)settings.DeepClone();
            _inputEntryPoint = _settings.CoordinatesCfg.GetCoordinates();
            InputFieldIdx = inputFieldIdx;
            _movingDW = new MovingDataWindow(MovingDataWindowMaxSize);
            _transUnits = new TransformedValueUnit[NumOfAnalogCodingMethods];
            _transUnits.Populate(null);
            return;
        }

        //Static methods
        /// <summary>
        /// Parses given code of AnalogCodingMethod
        /// </summary>
        /// <param name="code">Code to be parsed</param>
        public static AnalogCodingMethod ParseAnalogCodingMethod(string code)
        {
            switch(code.ToUpper())
            {
                case "ACTUAL": return AnalogCodingMethod.Actual;
                case "DIFFERENCE": return AnalogCodingMethod.Difference;
                case "LINEARSTEP": return AnalogCodingMethod.LinearSteps;
                case "POWER": return AnalogCodingMethod.Power;
                case "FOLDEDPOWER": return AnalogCodingMethod.FoldedPower;
                case "MOVINGAVERAGE": return AnalogCodingMethod.MovingAverage;
                case "MORLET": return AnalogCodingMethod.Morlet;
                default: throw new Exception($"Unsupported AnalogCodingMethod code: {code}.");
            }
        }

        //Methods
        /// <summary>
        /// Instantiates TransformedValueUnit associated with given AnalogCodingMethod if it doesn't exist yet.
        /// </summary>
        /// <param name="acm">Analog coding method</param>
        private void InstantiateTransUnit(AnalogCodingMethod acm)
        {
            if(_transUnits[(int)acm] == null)
            {
                _transUnits[(int)acm] = new TransformedValueUnit(_reservoirID, _inputEntryPoint, InputFieldIdx, _inputRange, _settings.SpikeTrainLength);
            }
            return;
        }

        /// <summary>
        /// Instantiates TransformedValueUnit associated with given analog coding method if it doesn't exist yet and
        /// returns input neuron associated with specified analog coding method.
        /// </summary>
        /// <param name="acm">Analog coding method</param>
        /// <param name="oppositeAmplitude">Specifies if to return variant having opposite amplitude</param>
        public InputNeuron GetAnalogInputNeuron(AnalogCodingMethod acm, bool oppositeAmplitude)
        {
            InstantiateTransUnit(acm);
            return oppositeAmplitude ? _transUnits[(int)acm].OppoAmplAnalogInputNeuron : _transUnits[(int)acm].UnchAmplAnalogInputNeuron;
        }

        /// <summary>
        /// Instantiates TransformedValueUnit associated with given analog coding method if it doesn't exist yet and
        /// returns collection of input neurons representing spike train associated with specified analog coding method
        /// </summary>
        /// <param name="acm">Analog coding method</param>
        /// <param name="oppositeAmplitude">Specifies if to return variant having opposite amplitude</param>
        public InputNeuron[] GetSpikeTrainInputNeurons(AnalogCodingMethod acm, bool oppositeAmplitude)
        {
            InstantiateTransUnit(acm);
            return oppositeAmplitude ? _transUnits[(int)acm].OppoAmplSpikeTrainInputNeuronCollection : _transUnits[(int)acm].UnchAmplSpikeTrainInputNeuronCollection;
        }

        /// <summary>
        /// Resets all associated input neurons to initial state
        /// </summary>
        /// <param name="statistics">Specifies whether to reset internal statistics of the associated neurons</param>
        public void Reset(bool statistics)
        {
            foreach(TransformedValueUnit tvu in _transUnits)
            {
                tvu?.Reset(statistics);
            }
            return;
        }

        /// <summary>
        /// Forces input neurons to accept new incoming stimulation.
        /// </summary>
        /// <param name="iStimuli">External input analog signal</param>
        public void NewStimulation(double iStimuli)
        {
            //Precompute input rescaled between 0,1
            double zeroOneStimuli = ZeroOneRange.Rescale(iStimuli, _inputRange);
            //////////////////////////////////////////
            //Coding methods
            //Actual
            if (_transUnits[(int)(AnalogCodingMethod.Actual)] != null)
            {
                double actual = iStimuli;
                _transUnits[(int)(AnalogCodingMethod.Actual)].NewStimulation(actual);
            }
            //Difference
            if (_transUnits[(int)(AnalogCodingMethod.Difference)] != null)
            {
                double difference = 0d;
                if (_movingDW.NumOfSamples >= _settings.DifferenceDistance)
                {
                    difference = (iStimuli - _movingDW.GetAt(_settings.DifferenceDistance - 1, true)) / _inputRange.Span;
                }
                _transUnits[(int)(AnalogCodingMethod.Difference)].NewStimulation(difference);
            }
            //Update moving data window
            _movingDW.AddSampleValue(iStimuli);
            //Linear steps
            if (_transUnits[(int)(AnalogCodingMethod.LinearSteps)] != null)
            {
                double stepPiece = ZeroOneRange.Span / _settings.NumOfLinearSteps;
                double stimuliStepPieces = Math.Ceiling(zeroOneStimuli / stepPiece).Bound(0, _settings.NumOfLinearSteps);
                double stimuliStepsRatio = stimuliStepPieces / _settings.NumOfLinearSteps;
                double linearSteps = _inputRange.Rescale(stimuliStepsRatio, ZeroOneRange);
                _transUnits[(int)(AnalogCodingMethod.LinearSteps)].NewStimulation(linearSteps);
            }
            //Power
            if (_transUnits[(int)(AnalogCodingMethod.Power)] != null)
            {
                double power = Math.Sign(iStimuli) * Math.Pow(Math.Abs(iStimuli), _settings.PowerExponent);
                _transUnits[(int)(AnalogCodingMethod.Power)].NewStimulation(power);
            }
            //Folded power
            if (_transUnits[(int)(AnalogCodingMethod.FoldedPower)] != null)
            {
                double foldedPower = Math.Pow(zeroOneStimuli, _settings.FoldedPowerExponent) - Math.Pow(1d - zeroOneStimuli, _settings.FoldedPowerExponent);
                foldedPower = _inputRange.Rescale(foldedPower, MOnePOneRange);
                _transUnits[(int)(AnalogCodingMethod.FoldedPower)].NewStimulation(foldedPower);
            }
            //Moving average
            if (_transUnits[(int)(AnalogCodingMethod.MovingAverage)] != null)
            {
                double movingAverage = _movingDW.NumOfSamples >= _settings.MovingAverageLength ? _movingDW.GetWeightedAvg(null, true, _settings.MovingAverageLength).Avg : 0d;
                _transUnits[(int)(AnalogCodingMethod.MovingAverage)].NewStimulation(movingAverage);
            }
            //Morlet
            if (_transUnits[(int)(AnalogCodingMethod.Morlet)] != null)
            {
                const double MorletXCoeff = 2.6927937d;
                const double MorletMinY = -0.289133093d;
                double morletX = iStimuli * MorletXCoeff;
                double morletY = (Math.Cos(1.75 * morletX) * Math.Exp(-(morletX * morletX) / 2d));
                double morlet = (((morletY - MorletMinY) / (1d - MorletMinY)) * _inputRange.Span) - _inputRange.Max;
                _transUnits[(int)(AnalogCodingMethod.Morlet)].NewStimulation(morlet);
            }
            return;
        }

        /// <summary>
        /// Forces input neurons to compute new output signal.
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics of associated input neurons</param>
        public void ComputeSignal(bool collectStatistics)
        {
            foreach (TransformedValueUnit tvu in _transUnits)
            {
                tvu?.ComputeSignal(collectStatistics);
            }
            return;
        }

        //Inner classes
        /// <summary>
        /// Encapsulates InputNeurons related to transformed input 
        /// </summary>
        [Serializable]
        public class TransformedValueUnit
        {
            //Static attributes
            /// <summary>
            /// Commonly used 0,1 interval
            /// </summary>
            private static readonly Interval ZeroOneRange = new Interval(0d, 1d);
            /// <summary>
            /// Commonly used -1,1 interval
            /// </summary>
            private static readonly Interval MOnePOneRange = new Interval(-1d, 1d);

            //Attribute properties
            /// <summary>
            /// InputNeuron providing analog value having unchanged amplitude
            /// </summary>
            public InputNeuron UnchAmplAnalogInputNeuron { get; }
            /// <summary>
            /// InputNeuron providing analog value having opposite amplitude
            /// </summary>
            public InputNeuron OppoAmplAnalogInputNeuron { get; }
            /// <summary>
            /// Collection of InputNeurons representing spike train of analog value having unchanged amplitude
            /// </summary>
            public InputNeuron[] UnchAmplSpikeTrainInputNeuronCollection { get; }
            /// <summary>
            /// Collection of InputNeurons representing spike train of analog value having opposite amplitude
            /// </summary>
            public InputNeuron[] OppoAmplSpikeTrainInputNeuronCollection { get; }

            //Attributes
            private readonly Interval _inputRange;
            private readonly double _precisionPiece;
            private readonly ulong _maxPrecisionBitMask;

            //Constructor
            /// <summary>
            /// Creates an initialized instance
            /// </summary>
            /// <param name="reservoirID">Reservoir ID</param>
            /// <param name="inputEntryPoint">Input entry point coordinates within the 3D space</param>
            /// <param name="inputFieldIdx">Index of the corresponding reservoir's input field.</param>
            /// <param name="inputRange">Input data range.</param>
            /// <param name="spikeTrainLength">Length of the spike-train (number of bits) representing input analog value.</param>
            public TransformedValueUnit(int reservoirID,
                                        int[] inputEntryPoint,
                                        int inputFieldIdx,
                                        Interval inputRange,
                                        int spikeTrainLength
                                        )
            {
                _inputRange = inputRange;
                _precisionPiece = _inputRange.Span / Math.Pow(2d, spikeTrainLength);
                _maxPrecisionBitMask = (uint)Math.Round(Math.Pow(2d, spikeTrainLength) - 1d);
                UnchAmplAnalogInputNeuron = new InputNeuron(reservoirID, inputEntryPoint, inputFieldIdx, _inputRange, NeuronCommon.NeuronSignalingRestrictionType.AnalogOnly);
                OppoAmplAnalogInputNeuron = new InputNeuron(reservoirID, inputEntryPoint, inputFieldIdx, _inputRange, NeuronCommon.NeuronSignalingRestrictionType.AnalogOnly);
                UnchAmplSpikeTrainInputNeuronCollection = new InputNeuron[spikeTrainLength];
                OppoAmplSpikeTrainInputNeuronCollection = new InputNeuron[spikeTrainLength];
                for(int i = 0; i < spikeTrainLength; i++)
                {
                    UnchAmplSpikeTrainInputNeuronCollection[i] = new InputNeuron(reservoirID, inputEntryPoint, inputFieldIdx, ZeroOneRange, NeuronCommon.NeuronSignalingRestrictionType.SpikingOnly);
                    OppoAmplSpikeTrainInputNeuronCollection[i] = new InputNeuron(reservoirID, inputEntryPoint, inputFieldIdx, ZeroOneRange, NeuronCommon.NeuronSignalingRestrictionType.SpikingOnly);
                }
                return;
            }

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
            /// Computes transformed value having opposite amplitude to x
            /// </summary>
            /// <param name="x">Input value</param>
            private double OppositeAmplitude(double x)
            {
                double rescaledX = MOnePOneRange.Rescale(x, _inputRange);
                double oppaAmplRescaledX = Math.Sign(rescaledX) * (1d - Math.Abs(rescaledX));
                return _inputRange.Rescale(oppaAmplRescaledX, MOnePOneRange);
            }

            /// <summary>
            /// Resets all associated input neurons to initial state
            /// </summary>
            /// <param name="statistics">Specifies whether to reset internal statistics of the associated neurons</param>
            public void Reset(bool statistics)
            {
                UnchAmplAnalogInputNeuron.Reset(statistics);
                OppoAmplAnalogInputNeuron.Reset(statistics);
                for (int i = 0; i < UnchAmplSpikeTrainInputNeuronCollection.Length; i++)
                {
                    UnchAmplSpikeTrainInputNeuronCollection[i].Reset(statistics);
                }
                for (int i = 0; i < OppoAmplSpikeTrainInputNeuronCollection.Length; i++)
                {
                    OppoAmplSpikeTrainInputNeuronCollection[i].Reset(statistics);
                }
                return;
            }

            /// <summary>
            /// Updates inner InputNeurons by new incoming stimulation
            /// </summary>
            /// <param name="analogValue">Analog value having unchanged amplitude</param>
            public void NewStimulation(double analogValue)
            {
                double oppoAnalogValue = OppositeAmplitude(analogValue);
                UnchAmplAnalogInputNeuron.NewStimulation(analogValue, 0d);
                OppoAmplAnalogInputNeuron.NewStimulation(oppoAnalogValue, 0d);
                uint unchSpikeTrainBits = GetSpikeTrain(analogValue);
                for (int i = 0; i < UnchAmplSpikeTrainInputNeuronCollection.Length; i++)
                {
                    UnchAmplSpikeTrainInputNeuronCollection[i].NewStimulation(Bitwise.GetBit(unchSpikeTrainBits, i), 0d);
                }
                uint oppoSpikeTrainBits = GetSpikeTrain(oppoAnalogValue);
                for (int i = 0; i < OppoAmplSpikeTrainInputNeuronCollection.Length; i++)
                {
                    OppoAmplSpikeTrainInputNeuronCollection[i].NewStimulation(Bitwise.GetBit(oppoSpikeTrainBits, i), 0d);
                }
                return;
            }

            /// <summary>
            /// Forces inner InputNeurons to compute new output signal.
            /// </summary>
            /// <param name="collectStatistics">Specifies whether to update internal statistics of associated InputNeurons</param>
            public void ComputeSignal(bool collectStatistics)
            {
                UnchAmplAnalogInputNeuron.Recompute(collectStatistics);
                OppoAmplAnalogInputNeuron.Recompute(collectStatistics);
                foreach (InputNeuron neuron in UnchAmplSpikeTrainInputNeuronCollection)
                {
                    neuron.Recompute(collectStatistics);
                }
                foreach (InputNeuron neuron in OppoAmplSpikeTrainInputNeuronCollection)
                {
                    neuron.Recompute(collectStatistics);
                }
                return;
            }

        }//TransformedValueUnit

    }//InputUnit

}//Namespace
