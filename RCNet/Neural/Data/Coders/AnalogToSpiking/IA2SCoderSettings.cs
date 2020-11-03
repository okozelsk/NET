using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Coders.AnalogToSpiking
{
    /// <summary>
    /// Common interface of A2S coders configurations
    /// </summary>
    public interface IA2SCoderSettings
    {
        /// <summary>
        /// Length of the spike-code of the analog absolute value
        /// </summary>
        int AbsValCodeLength { get; }

        /// <summary>
        /// Specifies if to generate halved spike-code where one half is dedicated for bellow average values (-)
        /// and second half for above average values (+)
        /// </summary>
        bool Halved { get; }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        RCNetBaseSettings DeepClone();

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        XElement GetXml(bool suppressDefaults);

    }//IA2SCoderSettings
}
