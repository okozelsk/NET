using RCNet.Extensions;
using RCNet.MathTools;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Codes analog values to a sequence of spikes (binary values)
    /// </summary>
    [Serializable]
    public class SpikeCode
    {
        //Attributes
        private readonly SpikeCodeSettings _spikeCodeCfg;
        private readonly double[] _thresholdCollection;
        private readonly double _precisionPiece;
        private readonly ulong _maxPrecisionBitMask;
        private readonly byte[][] _subCodes;
        private double _prevValue;

        //Attribute properties
        /// <summary>
        /// Binary spike-code representing encoded analog value
        /// </summary>
        public byte[] Code { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="spikeCodeCfg">Spike code configuration</param>
        public SpikeCode(SpikeCodeSettings spikeCodeCfg)
        {
            _spikeCodeCfg = (SpikeCodeSettings)spikeCodeCfg.DeepClone();
            //Binary precision
            _precisionPiece = 1d / Math.Pow(2d, _spikeCodeCfg.ComponentHalfCodeLength);
            _maxPrecisionBitMask = (uint)Math.Round(Math.Pow(2d, _spikeCodeCfg.ComponentHalfCodeLength) - 1d);
            //Sensitivity thresholds
            double exponent = _spikeCodeCfg.ComponentHalfCodeLength > 1 ? Math.Pow(1d / _spikeCodeCfg.LowestThreshold, 1d / (_spikeCodeCfg.ComponentHalfCodeLength - 1d)) : 0d;
            _thresholdCollection = new double[_spikeCodeCfg.ComponentHalfCodeLength];
            double threshold = 1d;
            for (int i = 0; i < _thresholdCollection.Length - 1; i++)
            {
                threshold /= exponent;
                _thresholdCollection[i] = threshold;
            }
            _thresholdCollection[_thresholdCollection.Length - 1] = 0;
            //Sub-code buffers allocation
            _subCodes = new byte[_spikeCodeCfg.NumOfComponents * 2][];
            for(int i = 0; i < _subCodes.Length; i++)
            {
                _subCodes[i] = new byte[_spikeCodeCfg.ComponentHalfCodeLength];
            }
            //Resulting spike-code buffer allocation
            Code = new byte[_subCodes.Length * _spikeCodeCfg.ComponentHalfCodeLength];
            Reset();
            return;
        }

        //Methods
        /// <summary>
        /// Resets component to its initial state
        /// </summary>
        public void Reset()
        {
            _prevValue = 0;
            for (int i = 0; i < _subCodes.Length; i++)
            {
                _subCodes[i].Populate((byte)0);
            }
            Code.Populate((byte)0);
            return;
        }

        /// <summary>
        /// Encodes given analog value
        /// </summary>
        /// <param name="value">Analog value to be encoded</param>
        public void Encode(double value)
        {
            value.Bound(-1d, 1d);
            int subCodesIdx = 0;
            if (_spikeCodeCfg.SignalComponent)
            {
                EncodeThresholdSubCode(Math.Abs(value), _subCodes[subCodesIdx + (value < 0 ? 0 : 1)]);
                subCodesIdx += 2;
            }
            if (_spikeCodeCfg.DeltaComponent)
            {
                double diffValue = ((value - _prevValue) / 2d);
                EncodeThresholdSubCode(Math.Abs(diffValue), _subCodes[subCodesIdx + (diffValue < 0 ? 0 : 1)]);
                subCodesIdx += 2;
            }
            if (_spikeCodeCfg.BinaryComponent)
            {
                EncodeBinarySubCode(Math.Abs(value), _subCodes[subCodesIdx + (value < 0 ? 0 : 1)]);
                subCodesIdx += 2;
            }
            //Construction of resulting Code
            int codeBuffIdx = 0;
            for (int i = 0; i < _spikeCodeCfg.ComponentHalfCodeLength; i++)
            {
                for (int j = 0; j < _subCodes.Length; j++, codeBuffIdx++)
                {
                    Code[codeBuffIdx] = _subCodes[j][i];
                }
            }
            _prevValue = value;
            return;
        }

        private void EncodeBinarySubCode(double absValue, byte[] buffer)
        {
            uint pieces = (uint)Math.Min(Math.Floor(absValue / _precisionPiece), (double)_maxPrecisionBitMask);
            for(int bitIdx = _spikeCodeCfg.ComponentHalfCodeLength - 1, i = 0; bitIdx >= 0; bitIdx--, i++)
            {
                buffer[i] = (byte)Bitwise.GetBit(pieces, bitIdx);
            }
            return;
        }

        private void EncodeThresholdSubCode(double absValue, byte[] buffer)
        {
            //Set all output to 0
            buffer.Populate((byte)0);
            //Will be here any 1?
            if (absValue != 0)
            {
                for (int i = 0; i < _thresholdCollection.Length; i++)
                {
                    if (absValue > _thresholdCollection[i])
                    {
                        buffer[i] = (byte)1;
                        if(!_spikeCodeCfg.ThresholdFullSpikeSet)
                        {
                            break;
                        }
                    }
                }
            }
            return;
        }

        /// <summary>
        /// Prepares set of meaningful combinations of joined coding spikes.
        /// </summary>
        /// <param name="numOfCombinations">Desired number of combination instances</param>
        public List<int[]> GetCombinations(int numOfCombinations)
        {
            List<int[]> result = new List<int[]>(numOfCombinations);
            //Alone coding spikes
            for (int i = 0; i < Code.Length; i++)
            {
                int[] cmbIdxs = new int[1];
                cmbIdxs[0] = i;
                result.Add(cmbIdxs);
                if (result.Count == numOfCombinations)
                {
                    //Desired number of combinations is reached
                    return result;
                }
            }
            //Combined threshold based coding spikes
            if(_spikeCodeCfg.SignalComponent && _spikeCodeCfg.DeltaComponent && result.Count < numOfCombinations)
            {
                for(int signalIdx = 0; signalIdx < _thresholdCollection.Length; signalIdx++)
                {
                    for(int deltaIdx = 0; deltaIdx < _thresholdCollection.Length; deltaIdx++)
                    {
                        for (int signalHalfIdx = 0; signalHalfIdx < 2; signalHalfIdx++)
                        {
                            for (int deltaHalfIdx = 0; deltaHalfIdx < 2; deltaHalfIdx++)
                            {
                                int[] cmbIdxs = new int[2];
                                cmbIdxs[0] = signalIdx * _subCodes.Length + signalHalfIdx;
                                cmbIdxs[1] = deltaIdx * _subCodes.Length + deltaHalfIdx + 2;
                                result.Add(cmbIdxs);
                                if (result.Count == numOfCombinations)
                                {
                                    //Desired number of combinations is reached
                                    return result;
                                }
                            }
                        }
                    }
                }
            }
            //Maximum number of combinations is reached
            return result;
        }


    }//SpikeCode

}//Namespace
