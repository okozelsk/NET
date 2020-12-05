using System;
using System.Xml.Linq;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Configuration of the AFAnalogSoftMax activation function
    /// </summary>
    [Serializable]
    public class AFAnalogSoftMaxSettings : RCNetBaseSettings, IActivationSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ActivationSoftMaxType";

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public AFAnalogSoftMaxSettings()
        {
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public AFAnalogSoftMaxSettings(AFAnalogSoftMaxSettings source)
        {
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public AFAnalogSoftMaxSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            return;
        }

        //Properties
        /// <inheritdoc/>
        public ActivationType TypeOfActivation { get { return ActivationType.Analog; } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return true; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new AFAnalogSoftMaxSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            return Validate(new XElement(rootElemName), XsdTypeName);
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("activationSoftMax", suppressDefaults);
        }

    }//AFAnalogSoftMaxSettings

}//Namespace
