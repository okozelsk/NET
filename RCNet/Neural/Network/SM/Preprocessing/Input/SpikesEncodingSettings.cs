using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using RCNet.Neural.Data.Coders.AnalogToSpiking;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Settings of spikes encoding
    /// </summary>
    [Serializable]
    public class SpikesEncodingSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPSpikesEncodingType";

        //Attribute properties
        /// <summary>
        /// Configuration of spikes coder
        /// </summary>
        public ISpikesEncodingSettings EncodingCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="encodingCfg">Configuration of spikes encoding</param>
        public SpikesEncodingSettings(ISpikesEncodingSettings encodingCfg)
        {
            EncodingCfg = (ISpikesEncodingSettings)encodingCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public SpikesEncodingSettings(SpikesEncodingSettings source)
            : this(source.EncodingCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance from the given xml element.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public SpikesEncodingSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            EncodingCfg = SpikesEncodingFactory.LoadSettings(settingsElem.Elements().First());
            Check();
            return;
        }

        //Properties
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
            return new SpikesEncodingSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName, ((RCNetBaseSettings)EncodingCfg).GetXml(suppressDefaults));
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
            return GetXml("spikesEncoding", suppressDefaults);
        }

    }//SpikesEncodingSettings

}//Namespace

