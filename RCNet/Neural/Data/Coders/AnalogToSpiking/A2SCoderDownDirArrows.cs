﻿using RCNet.Extensions;
using RCNet.Queue;
using System;

namespace RCNet.Neural.Data.Coders.AnalogToSpiking
{
    /// <summary>
    /// Implements the downward signal direction receptors coder.
    /// </summary>
    /// <remarks>
    /// Each receptor is sensitive to downward signal direction of the current signal against the past
    /// signal at the time T-x (where x is 1...number of receptors).
    /// The negative difference of current and past signal is then expressed as spikes through a novel coding
    /// algorithm meeting together two important spike-train conditions where stronger stimulation leads to earlier
    /// first spike and higher spiking frequency.
    /// </remarks>
    [Serializable]
    public class A2SCoderDownDirArrows : A2SCoderBase
    {
        //Constants
        private const double InitialValue = 0d;

        //Attributes
        private readonly A2SCoderDownDirArrowsSettings _coderCfg;
        private readonly SimpleQueue<double> _histValues;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="coderCfg">The coder configuration.</param>
        public A2SCoderDownDirArrows(A2SCoderDownDirArrowsSettings coderCfg)
            : base(coderCfg.NumOfTimePoints, coderCfg.NumOfReceptors)
        {
            _coderCfg = (A2SCoderDownDirArrowsSettings)coderCfg.DeepClone();
            _histValues = new SimpleQueue<double>(_coderCfg.NumOfReceptors);
            ResetHistValues();
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override void Reset()
        {
            ResetHistValues();
            return;
        }

        private void ResetHistValues()
        {
            _histValues.Reset();
            while (!_histValues.Full) _histValues.Enqueue(InitialValue);
            return;
        }

        /// <inheritdoc/>
        public override byte[][] GetCode(double normalizedValue)
        {
            //Allocate and set all output to 0
            byte[][] buffer = new byte[NumOfComponents][];
            for (int i = 0; i < NumOfComponents; i++)
            {
                buffer[i] = new byte[BaseCodeLength];
                buffer[i].Populate((byte)0);
            }
            double x = (normalizedValue + 1d) / 2d;
            //Code
            for (int i = 0; i < _coderCfg.NumOfReceptors; i++)
            {
                double histValue = _histValues.GetElementAt(i, true);
                if (x < histValue)
                {
                    double diff = histValue - x;
                    GetStrengthCode(diff, BaseCodeLength).CopyTo(buffer[i], 0);
                }
            }
            //Enqueue the last value
            _histValues.Enqueue(x, true);
            return buffer;
        }


    }//A2SCoderDownDirArrows

}//Namespace
