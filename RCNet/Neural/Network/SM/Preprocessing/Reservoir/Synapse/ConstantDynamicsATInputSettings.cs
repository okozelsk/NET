﻿using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration of the efficacy's constant dynamics of an input synapse connecting presynaptic input spiking neuron and postsynaptic hidden analog neuron.
    /// </summary>
    [Serializable]
    public class ConstantDynamicsATInputSettings : ConstantDynamicsSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "SynapseConstantDynamicsATInputType";

        //Default values
        /// <summary>
        /// The default value of the synapse's constant efficacy.
        /// </summary>
        public const double DefaultEfficacy = 1d;


        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="efficacy">The default value of the synapse's constant efficacy.</param>
        public ConstantDynamicsATInputSettings(double efficacy = DefaultEfficacy)
            : base(efficacy)
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">The source instance.</param>
        public ConstantDynamicsATInputSettings(ConstantDynamicsATInputSettings source)
            : base(source)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public ConstantDynamicsATInputSettings(XElement elem)
            : base(elem, XsdTypeName)
        {
            return;
        }

        //Properties
        /// <inheritdoc />
        public override PlasticityCommon.DynApplication Application { get { return PlasticityCommon.DynApplication.ATInput; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultEfficacy { get { return (Efficacy == DefaultEfficacy); } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultEfficacy;
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
            return new ConstantDynamicsATInputSettings(this);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("constantDynamics", suppressDefaults);
        }

    }//ConstantDynamicsATInputSettings

}//Namespace

