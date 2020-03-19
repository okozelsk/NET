using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.IO;
using RCNet.Extensions;
using RCNet.MathTools.Probability;
using RCNet.XmlTools;
using RCNet.RandomValue;
using System.Xml.XPath;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration parameters of a synapse providing input spiking signal to hidden analog neuron
    /// </summary>
    [Serializable]
    public class SpikingSourceATInputSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "SynapseSpikingSourceATInputType";
        
        //Default values
        /// <summary>
        /// Default minimum weight
        /// </summary>
        public const double DefaultMinWeight = 0d;
        /// <summary>
        /// Default maximum weight
        /// </summary>
        public const double DefaultMaxWeight = 1d;

        //Attribute properties
        /// <summary>
        /// Synapse's weight settings
        /// </summary>
        public URandomValueSettings WeightCfg { get; }

        /// <summary>
        /// Synapse's plasticity configuration
        /// </summary>
        public PlasticityATInputSettings PlasticityCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="weightCfg">Synapse's weight settings</param>
        /// <param name="plasticityCfg">Synapse's plasticity configuration</param>
        public SpikingSourceATInputSettings(URandomValueSettings weightCfg = null,
                                            PlasticityATInputSettings plasticityCfg = null
                                            )
        {
            WeightCfg = weightCfg == null ? new URandomValueSettings(DefaultMinWeight, DefaultMaxWeight) : (URandomValueSettings)weightCfg.DeepClone();
            PlasticityCfg = plasticityCfg == null ? new PlasticityATInputSettings() : (PlasticityATInputSettings)plasticityCfg.DeepClone();
            Check();
            return;
        }


        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public SpikingSourceATInputSettings(SpikingSourceATInputSettings source)
            :this(source.WeightCfg, source.PlasticityCfg)
        {

            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public SpikingSourceATInputSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            XElement weightSettingsElem = settingsElem.Descendants("weight").FirstOrDefault();
            WeightCfg = weightSettingsElem == null ? new URandomValueSettings(DefaultMinWeight, DefaultMaxWeight) : new URandomValueSettings(weightSettingsElem);
            XElement plasticitySettingsElem = settingsElem.Descendants("plasticity").FirstOrDefault();
            PlasticityCfg = plasticitySettingsElem == null ? new PlasticityATInputSettings() : new PlasticityATInputSettings(plasticitySettingsElem);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultWeightCfg { get { return (WeightCfg.Min == DefaultMinWeight && WeightCfg.Max == DefaultMaxWeight && WeightCfg.IsDefaultDistrType); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultPlasticityCfg { get { return (PlasticityCfg.ContainsOnlyDefaults); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return IsDefaultWeightCfg && IsDefaultPlasticityCfg; } }


        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new SpikingSourceATInputSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultWeightCfg)
            {
                rootElem.Add(WeightCfg.GetXml("weight", suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultPlasticityCfg)
            {
                rootElem.Add(PlasticityCfg.GetXml("plasticity", suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("spikingSource", suppressDefaults);
        }

    }//SpikingSourceATInputSettings

}//Namespace

