using System;

namespace RCNet.Neural.Data.Generators
{
    /// <summary>
    /// Implements the sinusoidal signal generator.
    /// </summary>
    [Serializable]
    public class SinusoidalGenerator : IGenerator
    {
        //Attributes
        private double _step;
        private readonly SinusoidalGeneratorSettings _cfg;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="cfg">The configuration.</param>
        public SinusoidalGenerator(SinusoidalGeneratorSettings cfg)
        {
            _cfg = (SinusoidalGeneratorSettings)cfg.DeepClone();
            Reset();
            return;
        }

        //Methods
        /// <inheritdoc />
        public void Reset()
        {
            _step = 0;
            return;
        }

        /// <inheritdoc />
        public double Next()
        {
            double signal = _cfg.Ampl * Math.Sin(Math.PI * ((_step * _cfg.Freq + _cfg.Phase) / 180d));
            ++_step;
            return signal;
        }

    }//SinusoidalGenerator

}//Namespace
