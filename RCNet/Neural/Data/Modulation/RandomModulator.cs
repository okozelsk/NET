using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.RandomValue;

namespace RCNet.Neural.Data.Modulation
{
    /// <summary>
    /// Modulates random signal
    /// </summary>
    [Serializable]
    public class RandomModulator : IModulator
    {
        //Attributes
        private Random _rand;
        private readonly int _seek;
        private readonly RandomValueSettings _settings;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="settings">Configuration</param>
        /// <param name="seek">
        /// Initial seek of the random generator.
        /// Specify seek less than 0 to obtain different initialization each time Reset is invoked.
        /// </param>
        public RandomModulator(RandomValueSettings settings, int seek = 0)
        {
            _settings = settings.DeepClone();
            _seek = seek;
            Reset();
            return;
        }

        //Methods
        /// <summary>
        /// Resets modulator to its initial state
        /// </summary>
        public void Reset()
        {
            _rand = (_seek < 0) ? new Random() : new Random(_seek);
            return;
        }

        /// <summary>
        /// Returns next signal value
        /// </summary>
        public double Next()
        {
            return _rand.NextDouble(_settings);
        }

    }//RandomModulator
}//Namespace
