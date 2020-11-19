using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Queue;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Data.Coders.AnalogToSpiking
{
    /// <summary>
    /// Implements a signal direction receptor, sensitive to upward direction against a historical value at time T-1..number of receptors.
    /// </summary>
    [Serializable]
    public class A2SCoderUpDirArrows : A2SCoderBase
    {
        //Constants
        private const double InitialValue = 1d;

        //Attributes
        private readonly A2SCoderUpDirArrowsSettings _coderCfg;
        private readonly SimpleQueue<double> _histValues;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="coderCfg">Coder configuration</param>
        public A2SCoderUpDirArrows(A2SCoderUpDirArrowsSettings coderCfg)
            : base(coderCfg.NumOfTimePoints, coderCfg.NumOfReceptors)
        {
            _coderCfg = (A2SCoderUpDirArrowsSettings)coderCfg.DeepClone();
            _histValues = new SimpleQueue<double>(_coderCfg.NumOfReceptors);
            ResetHistValues();
            return;
        }

        //Methods
        /// <summary>
        /// Resets coder
        /// </summary>
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
                if(x > histValue)
                {
                    double diff = x - histValue;
                    GetStrengthCode(diff, BaseCodeLength).CopyTo(buffer[i], 0);
                }
            }
            //Enqueue the last value
            _histValues.Enqueue(x, true);
            return buffer;
        }


    }//A2SCoderUpDirArrows

}//Namespace
