﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration parameters of the synapse constant dynamics (input spiking to hidden analog neuron)
    /// </summary>
    [Serializable]
    public class ConstantDynamicsATInputSettings : ConstantDynamicsSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "SynapseConstantDynamicsATInputType";

        //Default values
        /// <summary>
        /// Default synapse's constant efficacy
        /// </summary>
        public const double DefaultEfficacy = 1d;


        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="efficacy">Default synapse's constant efficacy</param>
        public ConstantDynamicsATInputSettings(double efficacy = DefaultEfficacy)
            :base(efficacy)
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ConstantDynamicsATInputSettings(ConstantDynamicsATInputSettings source)
            :base(source)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing settings</param>
        public ConstantDynamicsATInputSettings(XElement elem)
            :base(elem, XsdTypeName)
        {
            return;
        }

        //Properties
        /// <summary>
        /// Application of the synapse's dynamics
        /// </summary>
        public override PlasticityCommon.DynApplication Application { get { return PlasticityCommon.DynApplication.ATInput; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultEfficacy { get { return (Efficacy == DefaultEfficacy); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultEfficacy;
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
            return new ConstantDynamicsATInputSettings(this);
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
            if (!suppressDefaults || !IsDefaultEfficacy)
            {
                rootElem.Add(new XAttribute("efficacy", Efficacy.ToString(CultureInfo.InvariantCulture)));
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
            return GetXml("constantDynamics", suppressDefaults);
        }

    }//ConstantDynamicsATInputSettings

}//Namespace

