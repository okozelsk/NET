using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using RCNet.Extensions;
using RCNet.MathTools.Probability;
using RCNet.XmlTools;
using RCNet.RandomValue;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration parameters of a synapse
    /// </summary>
    [Serializable]
    public class SynapseSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "SynapseType";
        //Default values
        /// <summary>
        /// Default input delay method
        /// </summary>
        const Synapse.SynapticDelayMethod DefaultInputDelayMethod = Synapse.SynapticDelayMethod.Random;
        /// <summary>
        /// Default input maximum delay
        /// </summary>
        const int DefaultInputMaxDelay = 0;
        /// <summary>
        /// Default internal delay method
        /// </summary>
        const Synapse.SynapticDelayMethod DefaultInternalDelayMethod = Synapse.SynapticDelayMethod.Random;
        /// <summary>
        /// Default internal maximum delay
        /// </summary>
        const int DefaultInternalMaxDelay = 0;

        //Attribute properties
        /// <summary>
        /// Specifies how will be decided input synaptic delay
        /// </summary>
        public Synapse.SynapticDelayMethod InputDelayMethod { get; }

        /// <summary>
        /// Maximum delay of the input signal
        /// </summary>
        public int InputMaxDelay { get; }

        /// <summary>
        /// Specifies how will be decided internal synaptic delay
        /// </summary>
        public Synapse.SynapticDelayMethod InternalDelayMethod { get; }

        /// <summary>
        /// Maximum delay of the internal signal
        /// </summary>
        public int InternalMaxDelay { get; }

        /// <summary>
        /// Synapse's plasticity settings
        /// </summary>
        public PlasticitySettings PlasticityCfg { get; }
        
        /// <summary>
        /// Synapse's internal weights settings
        /// </summary>
        public InternalWeightsSettings InternalWeightsCfg { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="inputDelayMethod">Specifies how will be decided input synaptic delay</param>
        /// <param name="inputMaxDelay">Maximum delay of the input signal</param>
        /// <param name="internalDelayMethod">Specifies how will be decided internal synaptic delay</param>
        /// <param name="internalMaxDelay">Maximum delay of the internal signal</param>
        /// <param name="plasticityCfg">Synapse's settings when targeting spiking neurons</param>
        /// <param name="internalWeightsCfg">Synapse's settings when targeting analog neurons</param>
        public SynapseSettings(Synapse.SynapticDelayMethod inputDelayMethod = DefaultInputDelayMethod,
                               int inputMaxDelay = DefaultInputMaxDelay,
                               Synapse.SynapticDelayMethod internalDelayMethod = DefaultInternalDelayMethod,
                               int internalMaxDelay = DefaultInternalMaxDelay,
                               PlasticitySettings plasticityCfg = null,
                               InternalWeightsSettings internalWeightsCfg = null
                               )
        {
            InputDelayMethod = inputDelayMethod;
            InputMaxDelay = inputMaxDelay;
            InternalDelayMethod = internalDelayMethod;
            InternalMaxDelay = internalMaxDelay;
            PlasticityCfg = plasticityCfg == null ? new PlasticitySettings() : (PlasticitySettings)plasticityCfg.DeepClone();
            InternalWeightsCfg = internalWeightsCfg == null ? new InternalWeightsSettings() : (InternalWeightsSettings)internalWeightsCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public SynapseSettings(SynapseSettings source)
            :this(source.InputDelayMethod, source.InputMaxDelay, source.InternalDelayMethod, source.InputMaxDelay,
                  source.PlasticityCfg, source.InternalWeightsCfg)
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
        public SynapseSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            //Delay
            InputDelayMethod = (Synapse.SynapticDelayMethod)Enum.Parse(typeof(Synapse.SynapticDelayMethod), settingsElem.Attribute("inputDelayMethod").Value, true);
            InputMaxDelay = int.Parse(settingsElem.Attribute("inputMaxDelay").Value, CultureInfo.InvariantCulture);
            InternalDelayMethod = (Synapse.SynapticDelayMethod)Enum.Parse(typeof(Synapse.SynapticDelayMethod), settingsElem.Attribute("internalDelayMethod").Value, true);
            InternalMaxDelay = int.Parse(settingsElem.Attribute("internalMaxDelay").Value, CultureInfo.InvariantCulture);
            XElement cfgElem;
            //Plasticity
            cfgElem = settingsElem.Descendants("plasticity").FirstOrDefault();
            PlasticityCfg = cfgElem == null ? new PlasticitySettings() : new PlasticitySettings(cfgElem);
            //Internal weights
            cfgElem = settingsElem.Descendants("internalWeights").FirstOrDefault();
            InternalWeightsCfg = cfgElem == null ? new InternalWeightsSettings() : new InternalWeightsSettings(cfgElem);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultInputDelayMethod { get { return (InputDelayMethod == DefaultInputDelayMethod); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultInputMaxDelay { get { return (InputMaxDelay == DefaultInputMaxDelay); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultInternalDelayMethod { get { return (InternalDelayMethod == DefaultInternalDelayMethod); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultInternalMaxDelay { get { return (InternalMaxDelay == DefaultInternalMaxDelay); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultInputDelayMethod &&
                       IsDefaultInputMaxDelay &&
                       IsDefaultInternalDelayMethod &&
                       IsDefaultInternalMaxDelay &&
                       PlasticityCfg.ContainsOnlyDefaults &&
                       InternalWeightsCfg.ContainsOnlyDefaults;
            }
        }


        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (InputMaxDelay < 0)
            {
                throw new Exception($"Invalid InputMaxDelay {InputMaxDelay.ToString(CultureInfo.InvariantCulture)}. InputMaxDelay must be GE to 0.");
            }
            if (InternalMaxDelay < 0)
            {
                throw new Exception($"Invalid InternalMaxDelay {InternalMaxDelay.ToString(CultureInfo.InvariantCulture)}. InternalMaxDelay must be GE to 0.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new SynapseSettings(this);
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
            if (!suppressDefaults || !IsDefaultInputDelayMethod)
            {
                rootElem.Add(new XAttribute("inputDelayMethod", InputDelayMethod.ToString()));
            }
            if (!suppressDefaults || !IsDefaultInputMaxDelay)
            {
                rootElem.Add(new XAttribute("inputMaxDelay", InputMaxDelay.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultInternalDelayMethod)
            {
                rootElem.Add(new XAttribute("internalDelayMethod", InternalDelayMethod.ToString()));
            }
            if (!suppressDefaults || !IsDefaultInternalMaxDelay)
            {
                rootElem.Add(new XAttribute("internalMaxDelay", InternalMaxDelay.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !PlasticityCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(PlasticityCfg.GetXml("plasticity", suppressDefaults));
            }
            if (!suppressDefaults || !InternalWeightsCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(InternalWeightsCfg.GetXml("internalWeights", suppressDefaults));
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
            return GetXml("synapse", suppressDefaults);
        }

    }//SynapseSettings

}//Namespace

