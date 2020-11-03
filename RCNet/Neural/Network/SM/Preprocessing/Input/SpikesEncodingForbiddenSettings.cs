using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Settings of forbidden spikes encoding
    /// </summary>
    [Serializable]
    public class SpikesEncodingForbiddenSettings : RCNetBaseSettings, ISpikesEncodingSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPSpikesEncodingForbiddenType";

        //Attribute properties

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public SpikesEncodingForbiddenSettings()
        {
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public SpikesEncodingForbiddenSettings(SpikesEncodingForbiddenSettings source)
            : this()
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance from the given xml element.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public SpikesEncodingForbiddenSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Type of spikes encoding
        /// </summary>
        public InputEncoder.SpikesEncodingType EncodingType { get { return InputEncoder.SpikesEncodingType.Forbidden; } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return true; } }

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
            return new SpikesEncodingForbiddenSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
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
            return GetXml("forbidden", suppressDefaults);
        }

    }//SpikesEncodingForbiddenSettings

}//Namespace

