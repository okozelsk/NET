using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.Neural.Data.Generators
{
    /// <summary>
    /// Generates constant pulses
    /// </summary>
    [Serializable]
    public class PulseGenerator : IGenerator
    {
        //Attributes
        private readonly double _signal;
        private readonly int _leak;
        private int _t;

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="signal">Pulse signal value</param>
        /// <param name="leak">Constant pulse leak</param>
        public PulseGenerator(double signal, int leak)
        {
            _signal = signal;
            _leak = Math.Abs(leak);
            Reset();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="settings">Configuration</param>
        public PulseGenerator(PulseGeneratorSettings settings)
        {
            _signal = settings.Signal;
            _leak = settings.Leak;
            Reset();
            return;
        }

        //Methods
        /// <summary>
        /// Resets generator to its initial state
        /// </summary>
        public void Reset()
        {
            _t = 0;
            return;
        }

        /// <summary>
        /// Returns next signal value
        /// </summary>
        public double Next()
        {
            if (_t == _leak)
            {
                _t = 0;
                return _signal;
            }
            else
            {
                ++_t;
                return 0;
            }
        }

    }//PulseGenerator
}//Namespace
