using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.Neural.Data.Modulation
{
    /// <summary>
    /// Modulates constant signal
    /// </summary>
    [Serializable]
    public class ConstModulator : IModulator
    {
        //Attributes
        private readonly double _signal;

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="constSignal">Constant signal value</param>
        public ConstModulator(double constSignal)
        {
            _signal = constSignal;
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="settings">Configuration</param>
        public ConstModulator(ConstModulatorSettings settings)
        {
            _signal = settings.ConstSignal;
            return;
        }

        //Methods
        /// <summary>
        /// Resets modulator to its initial state
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

    }//ConstModulator
}//Namespace
