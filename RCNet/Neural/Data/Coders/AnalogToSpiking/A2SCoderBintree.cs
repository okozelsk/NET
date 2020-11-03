using RCNet.Extensions;
using RCNet.MathTools;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Data.Coders.AnalogToSpiking
{
    /// <summary>
    /// Codes an analog value as a set of spikes where combination of enabled spikes expresses the analog value
    /// </summary>
    [Serializable]
    public class A2SCoderBintree : A2SCoderBase
    {
        //Attributes
        private readonly double _precisionPiece;
        private readonly ulong _maxPrecisionBitMask;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="coderCfg">Coder configuration</param>
        public A2SCoderBintree(A2SCoderBintreeSettings coderCfg)
            :base(coderCfg.AbsValCodeLength, coderCfg.Halved)
        {
            _precisionPiece = 1d / Math.Pow(2d, AbsValCodeLength);
            _maxPrecisionBitMask = (ulong)Math.Round(Math.Pow(2d, AbsValCodeLength) - 1d);
            return;
        }

        //Methods
        /// <summary>
        /// Resets coder
        /// </summary>
        public override void Reset()
        {
            return;
        }

        /// <summary>
        /// Codes an analog value to the spike-code
        /// </summary>
        /// <param name="normalizedValue">A normalized analog value between -1 and 1</param>
        /// <returns>Resulting spike-code as an array of 0/1 byte values</returns>
        public override byte[] GetCode(double normalizedValue)
        {
            byte[] buffer = new byte[CodeTotalLength];
            //Allocate and set all output to 0
            buffer.Populate((byte)0);
            //Will be here any 1?
            double absValue = Halved ? Math.Abs(normalizedValue) : ((normalizedValue + 1d) / 2d);
            ulong pieces = Math.Min((ulong)Math.Floor(absValue / _precisionPiece), _maxPrecisionBitMask);
            if (pieces > 0)
            {
                int startIdx = Halved ? (normalizedValue < 0 ? 0 : AbsValCodeLength) : 0;
                for (int i = 0; i < AbsValCodeLength; i++)
                {
                    buffer[startIdx + i] = (byte)Bitwise.GetBit(pieces, i);
                }
            }
            return buffer;
        }


    }//A2SCoderBintree

}//Namespace
