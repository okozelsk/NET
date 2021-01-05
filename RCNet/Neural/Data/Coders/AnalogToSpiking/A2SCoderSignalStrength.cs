using RCNet.Extensions;
using System;

namespace RCNet.Neural.Data.Coders.AnalogToSpiking
{
    /// <summary>
    /// Implements the signal strength coder.
    /// </summary>
    /// <remarks>
    /// Uses a novel coding algorithm meeting together two important spike-train conditions where stronger
    /// stimulation leads to earlier first spike and higher spiking frequency.
    /// </remarks>
    [Serializable]
    public class A2SCoderSignalStrength : A2SCoderBase
    {
        //Attributes

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="coderCfg">The coder configuration.</param>
        public A2SCoderSignalStrength(A2SCoderSignalStrengthSettings coderCfg)
            : base(coderCfg.NumOfTimePoints, 2)
        {
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override void Reset()
        {
            return;
        }

        /// <inheritdoc/>
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
