using RCNet.Extensions;
using System;

namespace RCNet.Neural.Data.Coders.AnalogToSpiking
{
    /// <summary>
    /// Implements the common base class of all A2S coders (transformation of an analog value to a spike-code).
    /// </summary>
    [Serializable]
    public abstract class A2SCoderBase
    {
        //Attribute properties
        /// <summary>
        /// The length of the base spike-code (the spike-code length of the single component).
        /// </summary>
        public int BaseCodeLength { get; protected set; }

        /// <summary>
        /// The number of components that make up the spike-coding.
        /// </summary>
        public int NumOfComponents { get; protected set; }

        //Constructor
        /// <summary>
        /// Protected constructor
        /// </summary>
        /// <param name="baseCodeLength">The length of the base spike-code (the spike-code length of the single component).</param>
        /// <param name="numOfComponents">The number of components that make up the spike-coding.</param>
        protected A2SCoderBase(int baseCodeLength, int numOfComponents)
        {
            BaseCodeLength = baseCodeLength;
            NumOfComponents = numOfComponents;
            return;
        }

        //Methods
        /// <summary>
        /// Implements a novel coding algorithm to have met the two important spike-train conditions together:
        /// <para>
        /// 1. The frequency - as stronger value as higher spiking frequency.
        /// </para>
        /// <para>
        /// 2. The time to a first spike - as stronger value as earlier spike.
        /// </para>
        /// </summary>
        /// <param name="normalizedAbsValue">A normalized analog value between 0 and 1.</param>
        /// <param name="codeLength">The desired output code length.</param>
        public static byte[] GetStrengthCode(double normalizedAbsValue, int codeLength)
        {
            int[] spikeSquashPos = new int[codeLength];
            for (int i = 0; i < codeLength; i++)
            {
                spikeSquashPos[i] = (i + 1) * codeLength + 1;
            }
            double scale = ((double)codeLength) / (codeLength * codeLength + 1);
            //Allocate and set all output to 0
            byte[] buffer = new byte[codeLength];
            buffer.Populate((byte)0);
            //Generate output code
            double x = Math.Abs(normalizedAbsValue);
            for (int i = 0; i < codeLength; i++)
            {
                double stretchPos = spikeSquashPos[i] - (x * (1d - scale) * spikeSquashPos[i]);
                int idx = (int)Math.Round(stretchPos, 0) - 1;
                if (idx >= 0 && idx < codeLength)
                {
                    buffer[idx] = 1;
                }
            }
            return buffer;
        }

        /// <summary>
        /// Resets the coder.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Codes an analog value.
        /// </summary>
        /// <param name="normalizedValue">A normalized analog value between -1 and 1.</param>
        /// <returns>The resulting spike-code from all coder components as an array of arrays of 0/1 byte values.</returns>
        public abstract byte[][] GetCode(double normalizedValue);


    }//A2SCoderBase
}
