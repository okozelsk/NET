using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Queue;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Data.Coders.AnalogToSpiking
{
    /// <summary>
    /// Implements a signal direction receptor, sensitive to downward direction against a historical value at time T-1..number of receptors
    /// </summary>
    [Serializable]
    public class A2SCoderDownDirArrows : A2SCoderBase
    {
        //Attributes
        private readonly A2SCoderDownDirArrowsSettings _coderCfg;
        private readonly SimpleQueue<double> _histValues;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="coderCfg">Coder configuration</param>
        public A2SCoderDownDirArrows(A2SCoderDownDirArrowsSettings coderCfg)
            : base(coderCfg.NumOfTimePoints, coderCfg.NumOfReceptors)
        {
            _coderCfg = (A2SCoderDownDirArrowsSettings)coderCfg.DeepClone();
            _histValues = new SimpleQueue<double>(_coderCfg.NumOfReceptors);
            while (!_histValues.Full) _histValues.Enqueue(0d);
            return;
        }

        //Methods
        /// <summary>
        /// Resets coder
        /// </summary>
        public override void Reset()
        {
            _histValues.Reset();
            while (!_histValues.Full) _histValues.Enqueue(0d);
            return;
        }

        /// <summary>
        /// Codes an analog value to the spike-code
        /// </summary>
        /// <param name="normalizedValue">A normalized analog value between -1 and 1</param>
        /// <returns>Resulting spike-code from all components as an array of arrays of 0/1 byte values</returns>
        public override byte[][] GetCode(double normalizedValue)
        {
            //Allocate and set all output to 0
            byte[][] buffer = new byte[NumOfComponents][];
            for(int i = 0; i < NumOfComponents; i++)
            {
                buffer[i] = new byte[BaseCodeLength];
                buffer[i].Populate((byte)0);
            }
            double x = (normalizedValue + 1d) / 2d;
            //Code
            for(int i = 0; i < _coderCfg.NumOfReceptors; i++)
            {
                double histValue = _histValues.GetElementAt(i, true);
                if(x < histValue)
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
