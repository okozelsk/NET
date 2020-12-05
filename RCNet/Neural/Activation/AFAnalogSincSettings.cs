using System;
using System.Xml.Linq;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Configuration of the Sinc activation function
    /// </summary>
    [Serializable]
    public class AFAnalogSincSettings : RCNetBaseSettings, IActivationSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ActivationSincType";

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public AFAnalogSincSettings()
        {
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public AFAnalogSincSettings(AFAnalogSincSettings source)
        {
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public AFAnalogSincSettings(XElement elem)
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
            return new AFAnalogSincSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            return Validate(new XElement(rootElemName), XsdTypeName);
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("activationSinc", suppressDefaults);
        }

    }//AFAnalogSincSettings

}//Namespace
