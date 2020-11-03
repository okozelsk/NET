using System.Collections.Generic;
using RCNet.Extensions;
using RCNet.MathTools;
using System;

namespace RCNet.Neural.Data.Coders.AnalogToSpiking
{
    /// <summary>
    /// Codes an analog value as a set of spikes where number of consequent enabled spikes expresses the analog value strength
    /// </summary>
    [Serializable]
    public class A2SCoderPotentiometer : A2SCoderBase
    {
        //Attributes
        private readonly double[] _thresholdCollection;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="coderCfg">Coder configuration</param>
        public A2SCoderPotentiometer(A2SCoderPotentiometerSettings coderCfg)
            :base(coderCfg.AbsValCodeLength, coderCfg.Halved)
        {
            double exponent = AbsValCodeLength > 1 ? Math.Pow(1d / coderCfg.LowestThreshold, 1d / (AbsValCodeLength - 1d)) : 0d;
            _thresholdCollection = new double[AbsValCodeLength];
            double threshold = 1d;
            for (int i = 0; i < _thresholdCollection.Length - 1; i++)
            {
                threshold /= exponent;
                _thresholdCollection[i] = threshold;
            }
            _thresholdCollection[_thresholdCollection.Length - 1] = 0;
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
            if (absValue != 0)
            {
                int startIdx = Halved ? (normalizedValue < 0 ? 0 : _thresholdCollection.Length) : 0;
                for (int i = 0; i < _thresholdCollection.Length; i++)
                {
                    if (absValue > _thresholdCollection[i])
                    {
                        buffer[startIdx + i] = 1;
                    }
                }
            }
            return buffer;
        }


    }//A2SCoderPotentiometer

}//Namespace
