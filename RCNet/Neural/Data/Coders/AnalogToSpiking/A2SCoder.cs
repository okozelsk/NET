using RCNet.Extensions;
using RCNet.MathTools;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Data.Coders.AnalogToSpiking
{
    /// <summary>
    /// Codes an analog value as a set of spikes
    /// </summary>
    [Serializable]
    public class A2SCoder
    {
        /// <summary>
        /// Ways to convert an analog value to spikes
        /// </summary>
        public enum CodingMethod
        {
            /// <summary>
            /// An analog value is represented by neuronal population and it's spiking activity (in time 1:1)
            /// </summary>
            Horizontal,
            /// <summary>
            /// An analog value is represented as a fixed length spike-train (in time 1:spike-train length)
            /// </summary>
            Vertical,
            /// <summary>
            /// No coding
            /// </summary>
            None
        }

        //Attributes
        private readonly A2SCoderSettings _coderCfg;
        private readonly double[] _thresholdCollection;
        private readonly double _precisionPiece;
        private readonly ulong _maxPrecisionBitMask;

        //Attribute properties
        /// <summary>
        /// Spike-code representing encoded analog value
        /// </summary>
        public byte[] SpikeCode { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="coderCfg">Coder configuration</param>
        public A2SCoder(A2SCoderSettings coderCfg)
        {
            //Reset all members
            _thresholdCollection = null;
            _precisionPiece = 0d;
            _maxPrecisionBitMask = 0ul;
            //Store configuration
            _coderCfg = (A2SCoderSettings)coderCfg.DeepClone();
            //Initialize method
            switch(_coderCfg.CodingMethodCfg.Method)
            {
                case CodingMethod.Horizontal:
                    {
                        A2SHorizontalMethodSettings methodCfg = (A2SHorizontalMethodSettings)_coderCfg.CodingMethodCfg;
                        double exponent = methodCfg.HalfCodeLength > 1 ? Math.Pow(1d / methodCfg.LowestThreshold, 1d / (methodCfg.HalfCodeLength - 1d)) : 0d;
                        _thresholdCollection = new double[methodCfg.HalfCodeLength];
                        double threshold = 1d;
                        for (int i = 0; i < _thresholdCollection.Length - 1; i++)
                        {
                            threshold /= exponent;
                            _thresholdCollection[i] = threshold;
                        }
                        _thresholdCollection[_thresholdCollection.Length - 1] = 0;
                        //Resulting spike-code buffer allocation
                        SpikeCode = new byte[2 * methodCfg.HalfCodeLength];
                    }
                    break;
                case CodingMethod.Vertical:
                    {
                        A2SVerticalMethodSettings methodCfg = (A2SVerticalMethodSettings)_coderCfg.CodingMethodCfg;
                        _precisionPiece = 1d / Math.Pow(2d, methodCfg.SpikeTrainLength);
                        _maxPrecisionBitMask = (uint)Math.Round(Math.Pow(2d, methodCfg.SpikeTrainLength) - 1d);
                        //Resulting spike-code buffer allocation
                        SpikeCode = new byte[methodCfg.SpikeTrainLength];
                    }
                    break;
                default:
                    {
                        //Resulting spike-code buffer allocation
                        //Empty
                        SpikeCode = new byte[0];
                    }
                    break;
            }
            return;
        }

        //Properties
        /// <summary>
        /// Used coding method
        /// </summary>
        public CodingMethod Method { get { return _coderCfg.CodingMethodCfg.Method; } }

        //Methods
        /// <summary>
        /// Resets coder's output
        /// </summary>
        public void Reset()
        {
            SpikeCode.Populate((byte)0);
            return;
        }
        private void EncodeBinaryCode(double value, byte[] buffer)
        {
            //Set all output to 0
            buffer.Populate((byte)0);
            //Scale value between 0 and 1
            double absValue = (value + 1d) / 2d;
            uint pieces = (uint)Math.Min(Math.Floor(absValue / _precisionPiece), (double)_maxPrecisionBitMask);
            for (int bitIdx = buffer.Length - 1, i = 0; bitIdx >= 0; bitIdx--, i++)
            {
                //buffer[i] = (byte)Bitwise.GetBit(pieces, bitIdx);
                buffer[i] = (byte)Bitwise.GetBit(pieces, i);
            }
            return;
        }

        private void EncodeThresholdCode(double value, byte[] buffer)
        {
            //Set all output to 0
            buffer.Populate((byte)0);
            //Will be here any 1?
            if (value != 0)
            {
                int startIdx = value < 0 ? 0 : _thresholdCollection.Length;
                double absValue = Math.Abs(value);
                for (int i = 0; i < _thresholdCollection.Length; i++)
                {
                    if (absValue > _thresholdCollection[i])
                    {
                        buffer[startIdx + i] = 1;
                    }
                }
            }
            return;
        }

        /// <summary>
        /// Encodes given analog value
        /// </summary>
        /// <param name="value">Analog value to be encoded</param>
        public void Encode(double value)
        {
            //Bound analog value
            value = value.Bound(-1d, 1d);
            //Use appropriate method
            switch (_coderCfg.CodingMethodCfg.Method)
            {
                case CodingMethod.Horizontal:
                    //Encode
                    EncodeThresholdCode(value, SpikeCode);
                    break;
                case CodingMethod.Vertical:
                    //Encode
                    EncodeBinaryCode(value, SpikeCode);
                    break;
                default:
                    break;
            }
            return;
        }

    }//A2SCoder

}//Namespace
