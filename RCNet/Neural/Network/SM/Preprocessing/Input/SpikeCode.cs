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
        public byte[] Code { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="spikeCodeCfg">Spike code configuration</param>
        public SpikeCode(SpikeCodeSettings spikeCodeCfg)
        {
            _spikeCodeCfg = (SpikeCodeSettings)spikeCodeCfg.DeepClone();
            _componentComputer = new ComponentCodeComputer(_spikeCodeCfg.ComponentHalfCodeLength, _spikeCodeCfg.BoundariesSlicer);
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
        public void Reset()
        {
            _prevValue = double.NaN;
            Code.Populate((byte)0);
            return;
        }
        /// <summary>
        /// Encodes given value
        /// </summary>
        /// <param name="value">Value to be encoded</param>
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
            private readonly double[] _boundaries;

            //Constructor
            /// <summary>
            /// Creates an initialized instance
            /// </summary>
            /// <param name="halfCodeLength"></param>
            /// <param name="boundariesSlicer"></param>
            public ComponentCodeComputer(int halfCodeLength, double boundariesSlicer)
            {
                _boundaries = new double[halfCodeLength];
                double hiBoundary = 1d;
                for(int i = 0; i < _boundaries.Length - 1; i++)
                {
                    hiBoundary /= boundariesSlicer;
                    _boundaries[i] = hiBoundary;
                }
                _boundaries[_boundaries.Length - 1] = 0;
                return;
            }

            //Properties
            /// <summary>
            /// Total length of the bin code
            /// </summary>
            public int CodeLength { get { return 2 * _boundaries.Length; } }

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
                    for(int i = 0; i < _boundaries.Length; i++)
                    {
                        if(value > _boundaries[i])
                        {
                            buffer[fromIdx + halfIdx * _boundaries.Length + i] = (byte)1;
                            break;
                        }
                    }
                }
                return;
            }

        }//ComponentCodeComputer




    }//SpikeCode

}//Namespace
