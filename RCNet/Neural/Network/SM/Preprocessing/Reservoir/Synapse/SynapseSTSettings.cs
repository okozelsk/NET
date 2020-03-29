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
    /// Configuration parameters of a synapse providing signal to hidden spiking neuron
    /// </summary>
    [Serializable]
    public class SynapseSTSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "SynapseSTType";

        //Attribute properties
        /// <summary>
        /// Input synapse settings
        /// </summary>
        public SynapseSTInputSettings InputSynCfg { get; }

        /// <summary>
        /// Excitatory synapse settings
        /// </summary>
        public SynapseSTExcitatorySettings ExcitatorySynCfg { get; }

        /// <summary>
        /// Inhibitory synapse settings
        /// </summary>
        public SynapseSTInhibitorySettings InhibitorySynCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="inputSynCfg">Input synapse settings</param>
        /// <param name="excitatorySynCfg">Excitatory synapse settings</param>
        /// <param name="inhibitorySynCfg">Inhibitory synapse settings</param>
        public SynapseSTSettings(SynapseSTInputSettings inputSynCfg = null,
                                 SynapseSTExcitatorySettings excitatorySynCfg = null,
                                 SynapseSTInhibitorySettings inhibitorySynCfg = null
                                 )
        {
            InputSynCfg = inputSynCfg == null ? new SynapseSTInputSettings() : (SynapseSTInputSettings)inputSynCfg.DeepClone();
            ExcitatorySynCfg = excitatorySynCfg == null ? new SynapseSTExcitatorySettings() : (SynapseSTExcitatorySettings)excitatorySynCfg.DeepClone();
            InhibitorySynCfg = inhibitorySynCfg == null ? new SynapseSTInhibitorySettings() : (SynapseSTInhibitorySettings)inhibitorySynCfg.DeepClone();
            Check();
            return;
        }


        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public SynapseSTSettings(SynapseSTSettings source)
            :this(source.InputSynCfg, source.ExcitatorySynCfg, source.InhibitorySynCfg)
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
        public SynapseSTSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            XElement inputSynElem = settingsElem.Elements("input").FirstOrDefault();
            InputSynCfg = inputSynElem == null ? new SynapseSTInputSettings() : new SynapseSTInputSettings(inputSynElem);
            XElement excitatorySynElem = settingsElem.Elements("excitatory").FirstOrDefault();
            ExcitatorySynCfg = excitatorySynElem == null ? new SynapseSTExcitatorySettings() : new SynapseSTExcitatorySettings(excitatorySynElem);
            XElement inhibitorySynElem = settingsElem.Elements("inhibitory").FirstOrDefault();
            InhibitorySynCfg = inhibitorySynElem == null ? new SynapseSTInhibitorySettings() : new SynapseSTInhibitorySettings(inhibitorySynElem);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultInputSynCfg { get { return InputSynCfg.ContainsOnlyDefaults; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultExcitatorySynCfg { get { return ExcitatorySynCfg.ContainsOnlyDefaults; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultInhibitorySynCfg { get { return InhibitorySynCfg.ContainsOnlyDefaults; } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultInputSynCfg &&
                       IsDefaultExcitatorySynCfg &&
                       IsDefaultInhibitorySynCfg;
            }
        }


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
            return new SynapseSTSettings(this);
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
            if (!suppressDefaults || !IsDefaultInputSynCfg)
            {
                rootElem.Add(InputSynCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultExcitatorySynCfg)
            {
                rootElem.Add(ExcitatorySynCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultInhibitorySynCfg)
            {
                rootElem.Add(InhibitorySynCfg.GetXml(suppressDefaults));
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
            return GetXml("spikingTarget", suppressDefaults);
        }

    }//SynapseSTSettings

}//Namespace

