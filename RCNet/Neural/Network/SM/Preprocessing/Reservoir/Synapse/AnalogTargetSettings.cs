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
    /// Configuration parameters of an input synapse analog target
    /// </summary>
    [Serializable]
    public class AnalogTargetSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPResInstanceInputUnitConnectionAnalogTargetType";
        
        //Default values
        /// <summary>
        /// Default synapse's scope when targeting analog neurons
        /// </summary>
        public const Synapse.SynapticTargetScope DefaultScope = Synapse.SynapticTargetScope.All;
        /// <summary>
        /// Default minimum weight
        /// </summary>
        public const double DefaultMinWeight = 0d;
        /// <summary>
        /// Default maximum weight
        /// </summary>
        public const double DefaultMaxWeight = 1d;
        /// <summary>
        /// Default density
        /// </summary>
        public const double DefaultDensity = 1d;

        //Attribute properties
        /// <summary>
        /// Connection scope when targeting analog neurons
        /// </summary>
        public Synapse.SynapticTargetScope Scope { get; }

        /// <summary>
        /// Connection density within the scope
        /// </summary>
        public double Density { get; }

        /// <summary>
        /// Synapse's random weight settings for Input->Analog connection
        /// </summary>
        public URandomValueSettings WeightCfg { get; set; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="scope">Connection scope when targeting analog neurons</param>
        /// <param name="density">Connection density within the scope</param>
        /// <param name="weightCfg">Synapse's random weight settings for Input->Analog connection</param>
        public AnalogTargetSettings(Synapse.SynapticTargetScope scope = DefaultScope,
                                    double density = DefaultDensity,
                                    URandomValueSettings weightCfg = null
                                    )
        {
            Scope = scope;
            Density = density;
            WeightCfg = weightCfg == null ? new URandomValueSettings(DefaultMinWeight, DefaultMaxWeight) : (URandomValueSettings)weightCfg.DeepClone();
            Check();
            return;
        }


        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public AnalogTargetSettings(AnalogTargetSettings source)
            : this(source.Scope, source.Density, source.WeightCfg)
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
        public AnalogTargetSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Scope = (Synapse.SynapticTargetScope)Enum.Parse(typeof(Synapse.SynapticTargetScope), settingsElem.Attribute("scope").Value, true);
            Density = double.Parse(settingsElem.Attribute("density").Value, CultureInfo.InvariantCulture);
            //Weights
            XElement weightSettingsElem = settingsElem.Descendants("weight").FirstOrDefault();
            WeightCfg = weightSettingsElem == null ? new URandomValueSettings(DefaultMinWeight, DefaultMaxWeight) : new URandomValueSettings(weightSettingsElem);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultScope { get { return (Scope == DefaultScope); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultDensity { get { return (Density == DefaultDensity); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultWeightCfg { get { return (WeightCfg.Min == DefaultMinWeight && WeightCfg.Max == DefaultMaxWeight && WeightCfg.IsDefaultDistrType); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return IsDefaultScope && IsDefaultDensity && IsDefaultWeightCfg; } }


        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if(Density < 0)
            {
                throw new Exception($"Invalid Density {Density.ToString(CultureInfo.InvariantCulture)}. Density must be GE to 0.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new AnalogTargetSettings(this);
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
            if (!suppressDefaults || !IsDefaultScope)
            {
                rootElem.Add(new XAttribute("scope", Scope.ToString()));
            }
            if (!suppressDefaults || !IsDefaultDensity)
            {
                rootElem.Add(new XAttribute("density", Density.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultWeightCfg)
            {
                rootElem.Add(WeightCfg.GetXml("weight", suppressDefaults));
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
            return GetXml("analogTarget", suppressDefaults);
        }

    }//SpikingTargetSettings

}//Namespace

