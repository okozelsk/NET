using RCNet.Extensions;
using RCNet.RandomValue;
using System;

namespace RCNet.Neural.Data.Generators
{
    /// <summary>
    /// Generates random signal
    /// </summary>
    [Serializable]
    public class RandomGenerator : IGenerator
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
        public RandomGenerator(RandomValueSettings settings, int seek = 0)
        {
            _settings = (RandomValueSettings)settings.DeepClone();
            _seek = seek;
            Reset();
            return;
        }

        //Methods
        /// <summary>
        /// Resets generator to its initial state
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

    }//RandomGenerator
}//Namespace
