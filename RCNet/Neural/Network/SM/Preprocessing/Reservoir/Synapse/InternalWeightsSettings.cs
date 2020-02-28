using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using RCNet.Extensions;
using RCNet.XmlTools;
using RCNet.RandomValue;
using RCNet.Neural.Activation;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Synaptic internal weights configuration
    /// </summary>
    [Serializable]
    public class InternalWeightsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "SynapseInternalWeightsType";
        /// <summary>
        /// Numeric value indicating no application of the spectral radius
        /// </summary>
        public const double NASpectralRadiusNum = -1d;
        /// <summary>
        /// Code indicating no application of the spectral radius
        /// </summary>
        public const string NASpectralRadiusCode = "NA";
        //Default values
        /// <summary>
        /// Default spectral radius for analog neurons part of the reservoir (num)
        /// </summary>
        public const double DefaultAnalogScopeSpectralRadiusNum = 0.9999d;
        /// <summary>
        /// Default spectral radius for analog neurons part of the reservoir
        /// </summary>
        public const double DefaultSpikingScopeSpectralRadiusNum = NASpectralRadiusNum;
        /// <summary>
        /// Default weight min value
        /// </summary>
        public const double DefaultMinWeight = 0d;
        /// <summary>
        /// Default weight max value
        /// </summary>
        public const double DefaultMaxWeight = 1d;


        //Attribute properties

        /// <summary>
        /// Spectral radius for analog neurons part of the reservoir
        /// </summary>
        public double AnalogScopeSpectralRadius { get; }

        /// <summary>
        /// Spectral radius for spiking neurons part of the reservoir
        /// </summary>
        public double SpikingScopeSpectralRadius { get; }

        /// <summary>
        /// Synapse's weight configuration when analog-analog neurons are connected
        /// </summary>
        public InternalWeightsAASettings InternalWeightsAACfg { get; }

        /// <summary>
        /// Synapse's weight configuration when analog-spiking neurons are connected
        /// </summary>
        public InternalWeightsASSettings InternalWeightsASCfg { get; }

        /// <summary>
        /// Synapse's weight configuration when spiking-analog neurons are connected
        /// </summary>
        public InternalWeightsSASettings InternalWeightsSACfg { get; }

        /// <summary>
        /// Synapse's weight configuration when spiking-spiking neurons are connected
        /// </summary>
        public InternalWeightsSSSettings InternalWeightsSSCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="analogScopeSpectralRadius">Spectral radius for analog neurons part of the reservoir</param>
        /// <param name="spikingScopeSpectralRadius">Spectral radius for spiking neurons part of the reservoir</param>
        /// <param name="weightsSettings">Specific weights settings</param>
        public InternalWeightsSettings(double analogScopeSpectralRadius = DefaultAnalogScopeSpectralRadiusNum,
                                       double spikingScopeSpectralRadius = DefaultSpikingScopeSpectralRadiusNum,
                                       IEnumerable<URandomValueSettings> weightsSettings = null
                                       )
        {
            AnalogScopeSpectralRadius = analogScopeSpectralRadius;
            SpikingScopeSpectralRadius = spikingScopeSpectralRadius;
            InternalWeightsAACfg = null;
            InternalWeightsASCfg = null;
            InternalWeightsSACfg = null;
            InternalWeightsSSCfg = null;
            if (weightsSettings != null)
            {
                foreach (URandomValueSettings ws in weightsSettings)
                {
                    if (ws.GetType() == typeof(InternalWeightsAASettings))
                    {
                        InternalWeightsAACfg = (InternalWeightsAASettings)ws.DeepClone();
                    }
                    else if (ws.GetType() == typeof(InternalWeightsASSettings))
                    {
                        InternalWeightsASCfg = (InternalWeightsASSettings)ws.DeepClone();
                    }
                    else if (ws.GetType() == typeof(InternalWeightsSASettings))
                    {
                        InternalWeightsSACfg = (InternalWeightsSASettings)ws.DeepClone();
                    }
                    else if (ws.GetType() == typeof(InternalWeightsSSSettings))
                    {
                        InternalWeightsSSCfg = (InternalWeightsSSSettings)ws.DeepClone();
                    }
                }
            }
            //Defaults weights settings when not specific
            InternalWeightsAACfg = InternalWeightsAACfg ?? new InternalWeightsAASettings(DefaultMinWeight, DefaultMaxWeight);
            InternalWeightsASCfg = InternalWeightsASCfg ?? new InternalWeightsASSettings(DefaultMinWeight, DefaultMaxWeight);
            InternalWeightsSACfg = InternalWeightsSACfg ?? new InternalWeightsSASettings(DefaultMinWeight, DefaultMaxWeight);
            InternalWeightsSSCfg = InternalWeightsSSCfg ?? new InternalWeightsSSSettings(DefaultMinWeight, DefaultMaxWeight);
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="analogScopeSpectralRadius">Spectral radius for analog neurons part of the reservoir</param>
        /// <param name="spikingScopeSpectralRadius">Spectral radius for spiking neurons part of the reservoir</param>
        /// <param name="weightsSettings">Specific weights settings</param>
        public InternalWeightsSettings(double analogScopeSpectralRadius = DefaultAnalogScopeSpectralRadiusNum,
                                       double spikingScopeSpectralRadius = DefaultSpikingScopeSpectralRadiusNum,
                                       params URandomValueSettings[] weightsSettings
                                       )
            :this(analogScopeSpectralRadius, spikingScopeSpectralRadius, (IEnumerable<URandomValueSettings>) weightsSettings)
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public InternalWeightsSettings(InternalWeightsSettings source)
            :this(source.AnalogScopeSpectralRadius, source.SpikingScopeSpectralRadius,
                  source.InternalWeightsAACfg, source.InternalWeightsASCfg, source.InternalWeightsSACfg, source.InternalWeightsSSCfg)
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
        public InternalWeightsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            //Spectral radius
            string attrValue = settingsElem.Attribute("analogScopeSpectralRadius").Value;
            AnalogScopeSpectralRadius = attrValue == NASpectralRadiusCode ? NASpectralRadiusNum : double.Parse(attrValue, CultureInfo.InvariantCulture);
            attrValue = settingsElem.Attribute("spikingScopeSpectralRadius").Value;
            SpikingScopeSpectralRadius = attrValue == NASpectralRadiusCode ? NASpectralRadiusNum : double.Parse(attrValue, CultureInfo.InvariantCulture);
            //Weights
            XElement weightsCfgElem;
            //WeightsAACfg
            weightsCfgElem = settingsElem.XPathSelectElement("./weightsAA");
            InternalWeightsAACfg = weightsCfgElem == null ? new InternalWeightsAASettings(DefaultMinWeight, DefaultMaxWeight) : new InternalWeightsAASettings(weightsCfgElem);
            //WeightsASCfg
            weightsCfgElem = settingsElem.XPathSelectElement("./weightsAS");
            InternalWeightsASCfg = weightsCfgElem == null ? new InternalWeightsASSettings(DefaultMinWeight, DefaultMaxWeight) : new InternalWeightsASSettings(weightsCfgElem);
            //WeightsSACfg
            weightsCfgElem = settingsElem.XPathSelectElement("./weightsSA");
            InternalWeightsSACfg = weightsCfgElem == null ? new InternalWeightsSASettings(DefaultMinWeight, DefaultMaxWeight) : new InternalWeightsSASettings(weightsCfgElem);
            //WeightsSSCfg
            weightsCfgElem = settingsElem.XPathSelectElement("./weightsSS");
            InternalWeightsSSCfg = weightsCfgElem == null ? new InternalWeightsSSSettings(DefaultMinWeight, DefaultMaxWeight) : new InternalWeightsSSSettings(weightsCfgElem);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultAnalogScopeSpectralRadius { get { return (AnalogScopeSpectralRadius == DefaultAnalogScopeSpectralRadiusNum); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultSpikingScopeSpectralRadius { get { return (SpikingScopeSpectralRadius == DefaultSpikingScopeSpectralRadiusNum); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultWeightsAA { get { return InternalWeightsAACfg.Min == DefaultMinWeight && InternalWeightsAACfg.Max == DefaultMaxWeight && InternalWeightsAACfg.IsDefaultDistrType; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultWeightsAS { get { return InternalWeightsASCfg.Min == DefaultMinWeight && InternalWeightsASCfg.Max == DefaultMaxWeight && InternalWeightsASCfg.IsDefaultDistrType; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultWeightsSA { get { return InternalWeightsSACfg.Min == DefaultMinWeight && InternalWeightsSACfg.Max == DefaultMaxWeight && InternalWeightsSACfg.IsDefaultDistrType; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultWeightsSS { get { return InternalWeightsSSCfg.Min == DefaultMinWeight && InternalWeightsSSCfg.Max == DefaultMaxWeight && InternalWeightsSSCfg.IsDefaultDistrType; } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultAnalogScopeSpectralRadius &&
                       IsDefaultSpikingScopeSpectralRadius &&
                       IsDefaultWeightsAA &&
                       IsDefaultWeightsAS &&
                       IsDefaultWeightsSA &&
                       IsDefaultWeightsSS;
            }
        }

        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (AnalogScopeSpectralRadius != NASpectralRadiusNum && AnalogScopeSpectralRadius <= 0)
            {
                throw new Exception($"Invalid analog scope spectral radius {AnalogScopeSpectralRadius}. AnalogScopeSpectralRadius must be GT0 or equal to -1 (no spectral radius application).");
            }
            if (SpikingScopeSpectralRadius != NASpectralRadiusNum && SpikingScopeSpectralRadius <= 0)
            {
                throw new Exception($"Invalid spiking scope spectral radius {SpikingScopeSpectralRadius}. SpikingScopeSpectralRadius must be GT0 or equal to -1 (no spectral radius application).");
            }
            return;
        }
        /// <summary>
        /// Returns appropriate weights settings
        /// </summary>
        /// <param name="sourceActivationType">Activation type of the source neuron</param>
        /// <param name="targetActivationType">Activation type of the target neuron</param>
        public URandomValueSettings GetWeightsSettings(ActivationType sourceActivationType,
                                                       ActivationType targetActivationType
                                                       )
        {
            //Choose appropriate weioghts
            if (sourceActivationType == ActivationType.Analog &&
                targetActivationType == ActivationType.Analog
                )
            {
                return InternalWeightsAACfg;
            }
            else if (sourceActivationType == ActivationType.Analog &&
                     targetActivationType == ActivationType.Spiking
                     )
            {
                return InternalWeightsASCfg;
            }
            else if (sourceActivationType == ActivationType.Spiking &&
                     targetActivationType == ActivationType.Analog
                     )
            {
                return InternalWeightsSACfg;
            }
            else if (sourceActivationType == ActivationType.Spiking &&
                     targetActivationType == ActivationType.Spiking
                     )
            {
                return InternalWeightsSSCfg;
            }
            return null;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new InternalWeightsSettings(this);
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
            if (!suppressDefaults || !IsDefaultAnalogScopeSpectralRadius)
            {
                rootElem.Add(new XAttribute("analogScopeSpectralRadius", AnalogScopeSpectralRadius == NASpectralRadiusNum ? NASpectralRadiusCode : AnalogScopeSpectralRadius.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultSpikingScopeSpectralRadius)
            {
                rootElem.Add(new XAttribute("spikingScopeSpectralRadius", SpikingScopeSpectralRadius == NASpectralRadiusNum ? NASpectralRadiusCode : SpikingScopeSpectralRadius.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultWeightsAA)
            {
                rootElem.Add(InternalWeightsAACfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultWeightsAS)
            {
                rootElem.Add(InternalWeightsASCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultWeightsSA)
            {
                rootElem.Add(InternalWeightsSACfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultWeightsSS)
            {
                rootElem.Add(InternalWeightsSSCfg.GetXml(suppressDefaults));
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
            return GetXml("internalWeights", suppressDefaults);
        }

    }//InternalWeightsSettings

}//Namespace

