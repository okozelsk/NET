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
    /// Configuration parameters of a synapse providing signal to hidden analog neuron
    /// </summary>
    [Serializable]
    public class SynapseATSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "SynapseATType";
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
        /// Default spectral radius (num)
        /// </summary>
        public const double DefaultSpectralRadiusNum = 0.9999d;

        //Attribute properties
        /// <summary>
        /// Inhibitory synapse settings
        /// </summary>
        public double SpectralRadius { get; }

        /// <summary>
        /// Input synapse settings
        /// </summary>
        public SynapseATInputSettings InputSynCfg { get; }

        /// <summary>
        /// Indifferent synapse settings
        /// </summary>
        public SynapseATIndifferentSettings IndifferentSynCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="spectralRadius">Spectral radius</param>
        /// <param name="inputSynCfg">Input synapse settings</param>
        /// <param name="indifferentSynCfg">Indifferent synapse settings</param>
        public SynapseATSettings(double spectralRadius = DefaultSpectralRadiusNum,
                                 SynapseATInputSettings inputSynCfg = null,
                                 SynapseATIndifferentSettings indifferentSynCfg = null
                                 )
        {
            SpectralRadius = spectralRadius;
            InputSynCfg = inputSynCfg == null ? new SynapseATInputSettings() : (SynapseATInputSettings)inputSynCfg.DeepClone();
            IndifferentSynCfg = indifferentSynCfg == null ? new SynapseATIndifferentSettings() : (SynapseATIndifferentSettings)indifferentSynCfg.DeepClone();
            Check();
            return;
        }


        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public SynapseATSettings(SynapseATSettings source)
            :this(source.SpectralRadius, source.InputSynCfg, source.IndifferentSynCfg)
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
        public SynapseATSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            string attrValue = settingsElem.Attribute("spectralRadius").Value;
            SpectralRadius = attrValue == NASpectralRadiusCode ? NASpectralRadiusNum : double.Parse(attrValue, CultureInfo.InvariantCulture);
            XElement inputSynElem = settingsElem.Elements("input").FirstOrDefault();
            InputSynCfg = inputSynElem == null ? new SynapseATInputSettings() : new SynapseATInputSettings(inputSynElem);
            XElement indifferentSynElem = settingsElem.Elements("indifferent").FirstOrDefault();
            IndifferentSynCfg = indifferentSynElem == null ? new SynapseATIndifferentSettings() : new SynapseATIndifferentSettings(indifferentSynElem);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultSpectralRadius { get { return SpectralRadius == DefaultSpectralRadiusNum; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultInputSynCfg { get { return InputSynCfg.ContainsOnlyDefaults; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultIndifferentSynCfg { get { return IndifferentSynCfg.ContainsOnlyDefaults; } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultSpectralRadius &&
                       IsDefaultInputSynCfg &&
                       IsDefaultIndifferentSynCfg;
            }
        }


        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new SynapseATSettings(this);
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
            if (!suppressDefaults || !IsDefaultSpectralRadius)
            {
                rootElem.Add(new XAttribute("spectralRadius", SpectralRadius == NASpectralRadiusNum ? NASpectralRadiusCode : SpectralRadius.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultInputSynCfg)
            {
                rootElem.Add(InputSynCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultIndifferentSynCfg)
            {
                rootElem.Add(IndifferentSynCfg.GetXml(suppressDefaults));
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

    }//SynapseATSettings

}//Namespace

