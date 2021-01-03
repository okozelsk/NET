using RCNet.Extensions;
using System;

namespace RCNet.Neural.Data.Coders.AnalogToSpiking
{
    /// <summary>
    /// Implements the Gussian Receptive Fields coder.
    /// </summary>
    [Serializable]
    public class A2SCoderGaussianReceptors : A2SCoderBase
    {
        //Constants
        private const double GaussianBellWidth = 0.5d;

        //Attributes
        private readonly double _gaussianMaxFx;
        private readonly double[] _peaks;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="coderCfg">The coder configuration.</param>
        public A2SCoderGaussianReceptors(A2SCoderGaussianReceptorsSettings coderCfg)
            : base(coderCfg.NumOfTimePoints, coderCfg.NumOfReceptors)
        {
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
        /// <inheritdoc/>
        public override void Reset()
        {
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
