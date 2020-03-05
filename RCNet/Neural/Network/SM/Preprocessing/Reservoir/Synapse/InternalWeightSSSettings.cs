using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using RCNet.RandomValue;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration parameters of the synapse's weight when connecting internal Spiking-Spiking neurons
    /// </summary>
    [Serializable]
    public class InternalWeightSSSettings : URandomValueSettings
    {
        //Constants
        /// <summary>
        /// Weight scale factor when EE synapse
        /// </summary>
        public const double ScaleEE = 0.5d;

        /// <summary>
        /// Weight scale factor when EI synapse
        /// </summary>
        public const double ScaleEI = 1d;

        /// <summary>
        /// Weight scale factor when IE synapse
        /// </summary>
        public const double ScaleIE = 1.9d / 6d;

        /// <summary>
        /// Weight scale factor when II synapse
        /// </summary>
        public const double ScaleII = 1.9d / 6d;

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="min">Min random value</param>
        /// <param name="max">Max random value</param>
        /// <param name="distrCfg">Specific parameters of the distribution</param>
        public InternalWeightSSSettings(double min,
                                         double max,
                                         IDistrSettings distrCfg = null
                                         )
            : base(min, max, distrCfg)
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public InternalWeightSSSettings(InternalWeightSSSettings source)
            :base(source)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing settings</param>
        public InternalWeightSSSettings(XElement elem)
            :base(elem)
        {
            return;
        }

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new InternalWeightSSSettings(this);
        }

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("weightSS", suppressDefaults);
        }

    }//InternalWeightSSSettings

}//Namespace

