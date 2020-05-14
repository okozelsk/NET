using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;

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
        private readonly ComponentCodeComputer _componentComputer;
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
            _componentComputer = new ComponentCodeComputer(_spikeCodeCfg.ComponentHalfCodeLength, _spikeCodeCfg.LowestThreshold);
            int codeLength = 0;
            if(_spikeCodeCfg.UseDeviation)
            {
                codeLength += _componentComputer.CodeLength;
            }
            if (_spikeCodeCfg.UseDifference)
            {
                codeLength += _componentComputer.CodeLength;
            }
            Code = new byte[codeLength];
            Reset();
            return;
        }

        //Methods
        /// <summary>
        /// Resets component to its initial state
        /// </summary>
        public void Reset()
        {
            _prevValue = double.NaN;
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
            int fromIdx = 0;
            if(_spikeCodeCfg.UseDeviation)
            {
                _componentComputer.Encode(value, Code, fromIdx);
                fromIdx += _componentComputer.CodeLength;
            }
            if (_spikeCodeCfg.UseDifference)
            {
                double diffValue = double.IsNaN(_prevValue) ? value : ((value - _prevValue) / 2d);
                _componentComputer.Encode(diffValue, Code, fromIdx);
            }
            _prevValue = value;
            return;
        }

        //Inner classes
        [Serializable]
        private class ComponentCodeComputer
        {

            //Attributes
            private readonly double[] _thresholdCollection;

            //Constructor
            /// <summary>
            /// Creates an initialized instance
            /// </summary>
            /// <param name="halfCodeLength">Length of the half of component code</param>
            /// <param name="lowestThreshold">Firing threshold of the most sensitive input neuron</param>
            public ComponentCodeComputer(int halfCodeLength, double lowestThreshold)
            {
                double exponent = halfCodeLength > 1 ? Math.Pow(1d / lowestThreshold, 1d / (halfCodeLength - 1d)) : 0d;
                _thresholdCollection = new double[halfCodeLength];
                double threshold = 1d;
                for(int i = 0; i < _thresholdCollection.Length - 1; i++)
                {
                    threshold /= exponent;
                    _thresholdCollection[i] = threshold;
                }
                _thresholdCollection[_thresholdCollection.Length - 1] = 0;
                return;
            }

            //Properties
            /// <summary>
            /// Total length of the bin code
            /// </summary>
            public int CodeLength { get { return 2 * _thresholdCollection.Length; } }

            //Methods
            /// <summary>
            /// Encodes value into the buffer from given position
            /// </summary>
            /// <param name="value">Value to be encoded (between -1 and 1)</param>
            /// <param name="buffer">Output buffer</param>
            /// <param name="fromIdx">Position from which to write into the buffer</param>
            public void Encode(double value, byte[] buffer, int fromIdx)
            {
                //Set all output to 0
                buffer.Populate((byte)0, fromIdx, CodeLength);
                //Will there be any 1
                if (value != 0)
                {
                    int halfIdx = value < 0 ? 0 : 1;
                    value = Math.Abs(value);
                    for(int i = 0; i < _thresholdCollection.Length; i++)
                    {
                        if(value > _thresholdCollection[i])
                        {
                            buffer[fromIdx + halfIdx * _thresholdCollection.Length + i] = (byte)1;
                        }
                    }
                }
                return;
            }

        }//ComponentCodeComputer




    }//SpikeCode

}//Namespace
