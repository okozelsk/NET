using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.Neural.Data.Modulation
{
    /// <summary>
    /// Common interface for modulators of the internal input signal
    /// </summary>
    public interface IModulator
    {
        /// <summary>
        /// Resets modulator to its initial state
        /// </summary>
        void Reset();

        /// <summary>
        /// Returns next signal value
        /// </summary>
        double Next();

    }//IModulator

}//Namespace
