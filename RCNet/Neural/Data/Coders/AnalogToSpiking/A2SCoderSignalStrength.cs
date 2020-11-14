using System.Collections.Generic;
using RCNet.Extensions;
using RCNet.MathTools;
using System;
using System.Globalization;

namespace RCNet.Neural.Data.Coders.AnalogToSpiking
{
    /// <summary>
    /// Implements signal strength coder meeting two important spike-train conditions together:
    /// 1. Frequency - as stronger value as higher spiking frequency
    /// 2. Time to first spike - as stronger value as earlier spike
    /// </summary>
    [Serializable]
    public class A2SCoderSignalStrength : A2SCoderBase
    {
        //Attributes
        private readonly A2SCoderSignalStrengthSettings _coderCfg;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="coderCfg">Coder configuration</param>
        public A2SCoderSignalStrength(A2SCoderSignalStrengthSettings coderCfg)
            :base(coderCfg.NumOfTimePoints, 2)
        {
            _coderCfg = (A2SCoderSignalStrengthSettings)coderCfg.DeepClone();
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
        /// <returns>Resulting spike-code from all components as an array of arrays of 0/1 byte values</returns>
        public override byte[][] GetCode(double normalizedValue)
        {
            //Allocate
            byte[][] buffer = new byte[NumOfComponents][];
            for (int i = 0; i < NumOfComponents; i++)
            {
                buffer[i] = new byte[BaseCodeLength];
                buffer[i].Populate((byte)0);
            }
            //Code
            int componentIdx = normalizedValue < 0 ? 0 : 1;
            double x = (Math.Abs(normalizedValue) + 1d) / 2d;
            GetStrengthCode(x, BaseCodeLength).CopyTo(buffer[componentIdx], 0);
            return buffer;
        }

    }//A2SCoderSignalStrength

}//Namespace
