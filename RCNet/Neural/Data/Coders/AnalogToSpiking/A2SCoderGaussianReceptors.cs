using System.Collections.Generic;
using RCNet.Extensions;
using RCNet.MathTools;
using System;
using System.Linq;

namespace RCNet.Neural.Data.Coders.AnalogToSpiking
{
    /// <summary>
    /// Implements Gussian Receptive Fields coder.
    /// </summary>
    [Serializable]
    public class A2SCoderGaussianReceptors : A2SCoderBase
    {
        //Constants
        private const double GaussianBellWidth = 0.5d;

        //Attributes
        private readonly A2SCoderGaussianReceptorsSettings _coderCfg;
        private readonly double _gaussianMaxFx;
        private readonly double[] _peaks;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="coderCfg">Coder configuration</param>
        public A2SCoderGaussianReceptors(A2SCoderGaussianReceptorsSettings coderCfg)
            : base(coderCfg.NumOfTimePoints, coderCfg.NumOfReceptors)
        {
            _coderCfg = (A2SCoderGaussianReceptorsSettings)coderCfg.DeepClone();
            _gaussianMaxFx = 1d / (GaussianBellWidth * Math.Sqrt(2d * Math.PI));
            _peaks = new double[coderCfg.NumOfReceptors];
            double divider = coderCfg.NumOfReceptors > 1 ? coderCfg.NumOfReceptors - 1 : 1;
            for (int i = 0; i < coderCfg.NumOfReceptors; i++)
            {
                _peaks[i] = i / divider;
            }
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
            //Allocate and set all output to 0
            byte[][] buffer = new byte[NumOfComponents][];
            for (int i = 0; i < NumOfComponents; i++)
            {
                buffer[i] = new byte[BaseCodeLength];
                buffer[i].Populate((byte)0);
            }
            //Code
            double x = (normalizedValue + 1d) / 2d;
            for (int i = 0; i < NumOfComponents; i++)
            {
                double fx = _gaussianMaxFx * Math.Exp(-(Math.Pow(x - _peaks[i], 2d) / Math.Pow(GaussianBellWidth, 2d)));
                int timePointIdx = (int)Math.Round((fx / _gaussianMaxFx) * (BaseCodeLength - 1d), 0);
                buffer[i][timePointIdx] = 1;
            }
            return buffer;
        }

    }//A2SCoderGaussianReceptors

}//Namespace
