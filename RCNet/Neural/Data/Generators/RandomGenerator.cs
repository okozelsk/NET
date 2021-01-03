using RCNet.Extensions;
using RCNet.RandomValue;
using System;

namespace RCNet.Neural.Data.Generators
{
    /// <summary>
    /// Implements the random signal generator.
    /// </summary>
    [Serializable]
    public class RandomGenerator : IGenerator
    {
        //Attributes
        private Random _rand;
        private readonly int _seek;
        private readonly RandomValueSettings _cfg;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="cfg">The RandomValue configuration.</param>
        /// <param name="seek">The initial seek of the random generator. Specify the seek less than 0 to obtain different initialization each time the Reset is invoked.</param>
        public RandomGenerator(RandomValueSettings cfg, int seek = 0)
        {
            _cfg = (RandomValueSettings)cfg.DeepClone();
            _seek = seek;
            Reset();
            return;
        }

        //Methods
        /// <inheritdoc />
        public void Reset()
        {
            _rand = (_seek < 0) ? new Random() : new Random(_seek);
            return;
        }

        /// <inheritdoc />
        public double Next()
        {
            return _rand.NextDouble(_cfg);
        }

    }//RandomGenerator

}//Namespace
