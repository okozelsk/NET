using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration of a synapse connecting a postsynaptic hidden analog neuron.
    /// </summary>
    [Serializable]
    public class SynapseATSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "SynapseATType";
        /// <summary>
        /// The numeric code indicating no application of the spectral radius.
        /// </summary>
        public const double NASpectralRadiusNum = -1d;
        /// <summary>
        /// The string code indicating no application of the spectral radius.
        /// </summary>
        public const string NASpectralRadiusCode = "NA";

        //Default values
        /// <summary>
        /// The default value of the spectral radius.
        /// </summary>
        public const double DefaultSpectralRadiusNum = 0.9999d;

        //Attribute properties
        /// <summary>
        /// The spectral radius.
        /// </summary>
        public double SpectralRadius { get; }

        /// <summary>
        /// The configuration of an input synapse.
        /// </summary>
        public SynapseATInputSettings InputSynCfg { get; }

        /// <summary>
        /// The configuration of the indifferent synapse.
        /// </summary>
        public SynapseATIndifferentSettings IndifferentSynCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="spectralRadius">The spectral radius.</param>
        /// <param name="inputSynCfg">The configuration of an input synapse.</param>
        /// <param name="indifferentSynCfg">The configuration of the indifferent synapse.</param>
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
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public SynapseATSettings(SynapseATSettings source)
            : this(source.SpectralRadius, source.InputSynCfg, source.IndifferentSynCfg)
        {

            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
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
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultSpectralRadius { get { return SpectralRadius == DefaultSpectralRadiusNum; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultInputSynCfg { get { return InputSynCfg.ContainsOnlyDefaults; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultIndifferentSynCfg { get { return IndifferentSynCfg.ContainsOnlyDefaults; } }

        /// <inheritdoc />
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
        /// <inheritdoc />
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new SynapseATSettings(this);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("analogTarget", suppressDefaults);
        }

    }//SynapseATSettings

}//Namespace

