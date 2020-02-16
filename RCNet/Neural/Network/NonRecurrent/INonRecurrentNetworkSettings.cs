using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using RCNet.MathTools;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Common interface of non-recurrent network settings
    /// </summary>
    public interface INonRecurrentNetworkSettings
    {
        //Properties
        /// <summary>
        /// Output range
        /// </summary>
        Interval OutputRange { get; }

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


    }//INonRecurrentNetworkSettings

}//Namespace
