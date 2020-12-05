using System;

namespace RCNet.Neural.Data.Generators
{
    /// <summary>
    /// Implements the sinusoidal signal generator
    /// </summary>
    [Serializable]
    public class SinusoidalGenerator : IGenerator
    {
        //Attributes
        private double _step;
        private readonly SinusoidalGeneratorSettings _settings;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="settings">Configuration</param>
        public SinusoidalGenerator(SinusoidalGeneratorSettings settings)
        {
            _settings = (SinusoidalGeneratorSettings)settings.DeepClone();
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
            double signal = _settings.Ampl * Math.Sin(Math.PI * ((_step * _settings.Freq + _settings.Phase) / 180d));
            ++_step;
            return signal;
        }

    }//SinusoidalGenerator
}//Namespace
