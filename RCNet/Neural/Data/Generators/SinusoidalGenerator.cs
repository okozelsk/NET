using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.RandomValue;

namespace RCNet.Neural.Data.Generators
{
    /// <summary>
    /// Generates sinusoidal signal
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
        /// <summary>
        /// Resets generator to its initial state
        /// </summary>
        public void Reset()
        {
            _step = 0;
            return;
        }

        /// <summary>
        /// Returns next signal value
        /// </summary>
        public double Next()
        {
            double signal = _settings.Ampl * Math.Sin(Math.PI * ((_step * _settings.Freq + _settings.Phase) / 180d));
            ++_step;
            return signal;
        }

    }//SinusoidalGenerator
}//Namespace
