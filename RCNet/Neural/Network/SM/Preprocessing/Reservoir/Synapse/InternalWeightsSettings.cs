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
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron;

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
        public InternalWeightAASettings InternalWeightAACfg { get; }

        /// <summary>
        /// Synapse's weight configuration when analog-spiking neurons are connected
        /// </summary>
        public InternalWeightASSettings InternalWeightASCfg { get; }

        /// <summary>
        /// Synapse's weight configuration when spiking-analog neurons are connected
        /// </summary>
        public InternalWeightSASettings InternalWeightSACfg { get; }

        /// <summary>
        /// Synapse's weight configuration when spiking-spiking neurons are connected
        /// </summary>
        public InternalWeightSSSettings InternalWeightSSCfg { get; }

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
            InternalWeightAACfg = null;
            InternalWeightASCfg = null;
            InternalWeightSACfg = null;
            InternalWeightSSCfg = null;
            if (weightsSettings != null)
            {
                foreach (URandomValueSettings ws in weightsSettings)
                {
                    if (ws.GetType() == typeof(InternalWeightAASettings))
                    {
                        InternalWeightAACfg = (InternalWeightAASettings)ws.DeepClone();
                    }
                    else if (ws.GetType() == typeof(InternalWeightASSettings))
                    {
                        InternalWeightASCfg = (InternalWeightASSettings)ws.DeepClone();
                    }
                    else if (ws.GetType() == typeof(InternalWeightSASettings))
                    {
                        InternalWeightSACfg = (InternalWeightSASettings)ws.DeepClone();
                    }
                    else if (ws.GetType() == typeof(InternalWeightSSSettings))
                    {
                        InternalWeightSSCfg = (InternalWeightSSSettings)ws.DeepClone();
                    }
                }
            }
            //Defaults weights settings when not specific
            InternalWeightAACfg = InternalWeightAACfg ?? new InternalWeightAASettings(DefaultMinWeight, DefaultMaxWeight);
            InternalWeightASCfg = InternalWeightASCfg ?? new InternalWeightASSettings(DefaultMinWeight, DefaultMaxWeight);
            InternalWeightSACfg = InternalWeightSACfg ?? new InternalWeightSASettings(DefaultMinWeight, DefaultMaxWeight);
            InternalWeightSSCfg = InternalWeightSSCfg ?? new InternalWeightSSSettings(DefaultMinWeight, DefaultMaxWeight);
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
                  source.InternalWeightAACfg, source.InternalWeightASCfg, source.InternalWeightSACfg, source.InternalWeightSSCfg)
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
            XElement weightCfgElem;
            //WeightAACfg
            weightCfgElem = settingsElem.XPathSelectElement("./weightAA");
            InternalWeightAACfg = weightCfgElem == null ? new InternalWeightAASettings(DefaultMinWeight, DefaultMaxWeight) : new InternalWeightAASettings(weightCfgElem);
            //WeightASCfg
            weightCfgElem = settingsElem.XPathSelectElement("./weightAS");
            InternalWeightASCfg = weightCfgElem == null ? new InternalWeightASSettings(DefaultMinWeight, DefaultMaxWeight) : new InternalWeightASSettings(weightCfgElem);
            //WeightSACfg
            weightCfgElem = settingsElem.XPathSelectElement("./weightSA");
            InternalWeightSACfg = weightCfgElem == null ? new InternalWeightSASettings(DefaultMinWeight, DefaultMaxWeight) : new InternalWeightSASettings(weightCfgElem);
            //WeightSSCfg
            weightCfgElem = settingsElem.XPathSelectElement("./weightSS");
            InternalWeightSSCfg = weightCfgElem == null ? new InternalWeightSSSettings(DefaultMinWeight, DefaultMaxWeight) : new InternalWeightSSSettings(weightCfgElem);
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
        public bool IsDefaultWeightAA { get { return InternalWeightAACfg.Min == DefaultMinWeight && InternalWeightAACfg.Max == DefaultMaxWeight && InternalWeightAACfg.IsDefaultDistrType; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultWeightAS { get { return InternalWeightASCfg.Min == DefaultMinWeight && InternalWeightASCfg.Max == DefaultMaxWeight && InternalWeightASCfg.IsDefaultDistrType; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultWeightSA { get { return InternalWeightSACfg.Min == DefaultMinWeight && InternalWeightSACfg.Max == DefaultMaxWeight && InternalWeightSACfg.IsDefaultDistrType; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultWeightSS { get { return InternalWeightSSCfg.Min == DefaultMinWeight && InternalWeightSSCfg.Max == DefaultMaxWeight && InternalWeightSSCfg.IsDefaultDistrType; } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultAnalogScopeSpectralRadius &&
                       IsDefaultSpikingScopeSpectralRadius &&
                       IsDefaultWeightAA &&
                       IsDefaultWeightAS &&
                       IsDefaultWeightSA &&
                       IsDefaultWeightSS;
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
        /// <param name="sourceRole">Role of the source neuron</param>
        /// <param name="targetActivationType">Activation type of the target neuron</param>
        /// <param name="targetRole">Role of the target neuron</param>
        /// <param name="scale">Scale factor dependent on neuron roles to be applied</param>
        public URandomValueSettings GetWeightsSettings(ActivationType sourceActivationType,
                                                       NeuronCommon.NeuronRole sourceRole,
                                                       ActivationType targetActivationType,
                                                       NeuronCommon.NeuronRole targetRole,
                                                       out double scale
                                                       )
        {
            //Choose appropriate weioghts
            if (sourceActivationType == ActivationType.Analog &&
                targetActivationType == ActivationType.Analog
                )
            {
                if(sourceRole == NeuronCommon.NeuronRole.Excitatory && targetRole == NeuronCommon.NeuronRole.Excitatory)
                {
                    scale = InternalWeightAASettings.ScaleEE;
                }
                else if(sourceRole == NeuronCommon.NeuronRole.Excitatory && targetRole == NeuronCommon.NeuronRole.Inhibitory)
                {
                    scale = InternalWeightAASettings.ScaleEI;
                }
                else if (sourceRole == NeuronCommon.NeuronRole.Inhibitory && targetRole == NeuronCommon.NeuronRole.Excitatory)
                {
                    scale = InternalWeightAASettings.ScaleIE;
                }
                else
                {
                    scale = InternalWeightAASettings.ScaleII;
                }
                return InternalWeightAACfg;
            }
            else if (sourceActivationType == ActivationType.Analog &&
                     targetActivationType == ActivationType.Spiking
                     )
            {
                if (sourceRole == NeuronCommon.NeuronRole.Excitatory && targetRole == NeuronCommon.NeuronRole.Excitatory)
                {
                    scale = InternalWeightASSettings.ScaleEE;
                }
                else if (sourceRole == NeuronCommon.NeuronRole.Excitatory && targetRole == NeuronCommon.NeuronRole.Inhibitory)
                {
                    scale = InternalWeightASSettings.ScaleEI;
                }
                else if (sourceRole == NeuronCommon.NeuronRole.Inhibitory && targetRole == NeuronCommon.NeuronRole.Excitatory)
                {
                    scale = InternalWeightASSettings.ScaleIE;
                }
                else
                {
                    scale = InternalWeightASSettings.ScaleII;
                }
                return InternalWeightASCfg;
            }
            else if (sourceActivationType == ActivationType.Spiking &&
                     targetActivationType == ActivationType.Analog
                     )
            {
                if (sourceRole == NeuronCommon.NeuronRole.Excitatory && targetRole == NeuronCommon.NeuronRole.Excitatory)
                {
                    scale = InternalWeightSASettings.ScaleEE;
                }
                else if (sourceRole == NeuronCommon.NeuronRole.Excitatory && targetRole == NeuronCommon.NeuronRole.Inhibitory)
                {
                    scale = InternalWeightSASettings.ScaleEI;
                }
                else if (sourceRole == NeuronCommon.NeuronRole.Inhibitory && targetRole == NeuronCommon.NeuronRole.Excitatory)
                {
                    scale = InternalWeightSASettings.ScaleIE;
                }
                else
                {
                    scale = InternalWeightSASettings.ScaleII;
                }
                return InternalWeightSACfg;
            }
            else if (sourceActivationType == ActivationType.Spiking &&
                     targetActivationType == ActivationType.Spiking
                     )
            {
                if (sourceRole == NeuronCommon.NeuronRole.Excitatory && targetRole == NeuronCommon.NeuronRole.Excitatory)
                {
                    scale = InternalWeightSSSettings.ScaleEE;
                }
                else if (sourceRole == NeuronCommon.NeuronRole.Excitatory && targetRole == NeuronCommon.NeuronRole.Inhibitory)
                {
                    scale = InternalWeightSSSettings.ScaleEI;
                }
                else if (sourceRole == NeuronCommon.NeuronRole.Inhibitory && targetRole == NeuronCommon.NeuronRole.Excitatory)
                {
                    scale = InternalWeightSSSettings.ScaleIE;
                }
                else
                {
                    scale = InternalWeightSSSettings.ScaleII;
                }
                return InternalWeightSSCfg;
            }
            scale = 0d;
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
            if (!suppressDefaults || !IsDefaultWeightAA)
            {
                rootElem.Add(InternalWeightAACfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultWeightAS)
            {
                rootElem.Add(InternalWeightASCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultWeightSA)
            {
                rootElem.Add(InternalWeightSACfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultWeightSS)
            {
                rootElem.Add(InternalWeightSSCfg.GetXml(suppressDefaults));
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

