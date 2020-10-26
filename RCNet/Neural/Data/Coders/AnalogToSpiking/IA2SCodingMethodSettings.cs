using System;
using System.Collections.Generic;
using System.Text;

namespace RCNet.Neural.Data.Coders.AnalogToSpiking
{
    /// <summary>
    /// Common interface for configuration of methods encoding analog value to spikes
    /// </summary>
    public interface IA2SCodingMethodSettings
    {
        /// <summary>
        /// Way to convert an analog value to spikes
        /// </summary>
        A2SCoder.CodingMethod Method { get; }
    }

}//IA2SCodingMethodSettings

