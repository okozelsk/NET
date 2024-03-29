﻿using RCNet.RandomValue;
using System;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration of an excitatory synapse connecting presynaptic hidden spiking neuron and postsynaptic hidden spiking neuron.
    /// </summary>
    [Serializable]
    public class SpikingSourceSTExcitatorySettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "SynapseSpikingSourceSTExcitatoryType";

        //Default values
        /// <summary>
        /// The default minimum weight.
        /// </summary>
        public const double DefaultMinWeight = 0d;
        /// <summary>
        /// The default maximum weight.
        /// </summary>
        public const double DefaultMaxWeight = 1d;

        //Attribute properties
        /// <summary>
        /// The configuration of the synapse's weight.
        /// </summary>
        public URandomValueSettings WeightCfg { get; }

        /// <summary>
        /// The configuration of the synapse's plasticity.
        /// </summary>
        public PlasticitySTExcitatorySettings PlasticityCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="weightCfg">The configuration of the synapse's weight.</param>
        /// <param name="plasticityCfg">The configuration of the synapse's plasticity.</param>
        public SpikingSourceSTExcitatorySettings(URandomValueSettings weightCfg = null,
                                                 PlasticitySTExcitatorySettings plasticityCfg = null
                                                 )
        {
            WeightCfg = weightCfg == null ? new URandomValueSettings(DefaultMinWeight, DefaultMaxWeight) : (URandomValueSettings)weightCfg.DeepClone();
            PlasticityCfg = plasticityCfg == null ? new PlasticitySTExcitatorySettings() : (PlasticitySTExcitatorySettings)plasticityCfg.DeepClone();
            Check();
            return;
        }


        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public SpikingSourceSTExcitatorySettings(SpikingSourceSTExcitatorySettings source)
            : this(source.WeightCfg, source.PlasticityCfg)
        {

            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public SpikingSourceSTExcitatorySettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            XElement weightSettingsElem = settingsElem.Elements("weight").FirstOrDefault();
            WeightCfg = weightSettingsElem == null ? new URandomValueSettings(DefaultMinWeight, DefaultMaxWeight) : new URandomValueSettings(weightSettingsElem);
            XElement plasticitySettingsElem = settingsElem.Elements("plasticity").FirstOrDefault();
            PlasticityCfg = plasticitySettingsElem == null ? new PlasticitySTExcitatorySettings() : new PlasticitySTExcitatorySettings(plasticitySettingsElem);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultWeightCfg { get { return (WeightCfg.Min == DefaultMinWeight && WeightCfg.Max == DefaultMaxWeight && WeightCfg.IsDefaultDistrType); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultPlasticityCfg { get { return (PlasticityCfg.ContainsOnlyDefaults); } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return IsDefaultWeightCfg && IsDefaultPlasticityCfg; } }


        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new SpikingSourceSTExcitatorySettings(this);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("spikingSource", suppressDefaults);
        }

    }//SpikingSourceSTExcitatorySettings

}//Namespace

