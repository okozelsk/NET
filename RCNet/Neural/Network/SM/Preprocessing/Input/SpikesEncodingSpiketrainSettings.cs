using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using RCNet.Neural.Data.Coders.AnalogToSpiking;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Settings of spikes encoding of spike-train type
    /// </summary>
    [Serializable]
    public class SpikesEncodingSpiketrainSettings : RCNetBaseSettings, ISpikesEncodingSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPSpikesEncodingSpiketrainType";

        //Attribute properties
        /// <summary>
        /// Configuration of spikes coder
        /// </summary>
        public IA2SCoderSettings CoderCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="coderCfg">Configuration of spikes coder</param>
        public SpikesEncodingSpiketrainSettings(IA2SCoderSettings coderCfg)
        {
            CoderCfg = (IA2SCoderSettings)coderCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public SpikesEncodingSpiketrainSettings(SpikesEncodingSpiketrainSettings source)
            : this(source.CoderCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance from the given xml element.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public SpikesEncodingSpiketrainSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            CoderCfg = A2SCoderFactory.LoadSettings(settingsElem.Elements().First());
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Type of spikes encoding
        /// </summary>
        public InputEncoder.SpikesEncodingType EncodingType { get { return InputEncoder.SpikesEncodingType.Spiketrain; } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return false; } }

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
            return new SpikesEncodingSpiketrainSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName, CoderCfg.GetXml(suppressDefaults));
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("spike-train", suppressDefaults);
        }

    }//SpikesEncodingSpiketrainSettings

}//Namespace

