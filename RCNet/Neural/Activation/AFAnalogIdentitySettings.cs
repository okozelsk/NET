using System;
using System.Xml.Linq;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Configuration of the AFAnalogIdentity activation function
    /// </summary>
    [Serializable]
    public class AFAnalogIdentitySettings : RCNetBaseSettings, IActivationSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ActivationIdentityType";

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public AFAnalogIdentitySettings()
        {
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public AFAnalogIdentitySettings(AFAnalogIdentitySettings source)
        {
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public AFAnalogIdentitySettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            return;
        }

        //Properties
        /// <inheritdoc/>
        public ActivationType TypeOfActivation { get { return ActivationType.Analog; } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return true; } }

        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new AFAnalogIdentitySettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            return Validate(new XElement(rootElemName), XsdTypeName);
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("activationIdentity", suppressDefaults);
        }

    }//AFAnalogIdentitySettings

}//Namespace
