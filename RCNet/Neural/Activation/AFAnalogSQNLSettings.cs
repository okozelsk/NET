﻿using System;
using System.Xml.Linq;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Configuration of the AFAnalogSQNL activation function.
    /// </summary>
    [Serializable]
    public class AFAnalogSQNLSettings : RCNetBaseSettings, IActivationSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ActivationSQNLType";

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public AFAnalogSQNLSettings()
        {
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        public AFAnalogSQNLSettings(AFAnalogSQNLSettings source)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public AFAnalogSQNLSettings(XElement elem)
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
            return new AFAnalogSQNLSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            return Validate(new XElement(rootElemName), XsdTypeName);
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("activationSQNL", suppressDefaults);
        }

    }//AFAnalogSQNLSettings

}//Namespace
