using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.Neural.Data.Generators
{
    /// <summary>
    /// Generates constant signal
    /// </summary>
    [Serializable]
    public class ConstGenerator : IGenerator
    {
        //Attributes
        private readonly double _signal;

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="constSignal">Constant signal value</param>
        public ConstGenerator(double constSignal)
        {
            _signal = constSignal;
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="settings">Configuration</param>
        public ConstGenerator(ConstGeneratorSettings settings)
        {
            _signal = settings.ConstSignal;
            return;
        }

        //Methods
        /// <summary>
        /// Resets generator to its initial state
        /// </summary>
        public void Reset()
        {
            //Does nothing
            return;
        }

        /// <summary>
        /// Returns next signal value
        /// </summary>
        public double Next()
        {
            return _signal;
        }

    }//ConstGenerator
}//Namespace
