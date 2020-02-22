using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
    /// <summary>
    /// Common interface of connection distribution settings
    /// </summary>
    public interface IConnDistrSettings
    {
        /// <summary>
        /// EE synapses ratio
        /// </summary>
        double RatioEE { get; }

        /// <summary>
        /// EI synapses ratio
        /// </summary>
        double RatioEI { get; }

        /// <summary>
        /// IE synapses ratio
        /// </summary>
        double RatioIE { get; }

        /// <summary>
        /// II synapses ratio
        /// </summary>
        double RatioII { get; }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        RCNetBaseSettings DeepClone();

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        XElement GetXml(bool suppressDefaults);

    }//IConnProbabilitiesSettings

}//Namespace
