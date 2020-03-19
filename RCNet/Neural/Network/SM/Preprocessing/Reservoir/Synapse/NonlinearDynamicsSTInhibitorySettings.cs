using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration parameters of the synapse nonlinear dynamics (inhibitory spiking to hidden spiking neuron)
    /// </summary>
    [Serializable]
    public class NonlinearDynamicsSTInhibitorySettings : NonlinearDynamicsSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "SynapseNonlinearDynamicsSTInhibitoryType";

        //Default values
        /// <summary>
        /// Default resting efficacy
        /// </summary>
        public const double DefaultRestingEfficacy = 0.25d;
        /// <summary>
        /// Default tau depression
        /// </summary>
        public const double DefaultTauDepression = 700d;
        /// <summary>
        /// Default tau facilitation
        /// </summary>
        public const double DefaultTauFacilitation = 20d;


        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="restingEfficacy">Synapse's resting efficacy (average probability of neurotransmitter release)</param>
        /// <param name="tauDepression">Synapse's efficacy depression model time constant (ms)</param>
        /// <param name="tauFacilitation">Synapse's efficacy facilitation model time constant (ms)</param>
        public NonlinearDynamicsSTInhibitorySettings(double restingEfficacy = DefaultRestingEfficacy,
                                                     double tauDepression = DefaultTauDepression,
                                                     double tauFacilitation = DefaultTauFacilitation
                                                     )
            :base(restingEfficacy, tauDepression, tauFacilitation)
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public NonlinearDynamicsSTInhibitorySettings(NonlinearDynamicsSTInhibitorySettings source)
            :base(source)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing settings</param>
        public NonlinearDynamicsSTInhibitorySettings(XElement elem)
            :base(elem, XsdTypeName)
        {
            return;
        }

        //Properties
        /// <summary>
        /// Application of the synapse's dynamics
        /// </summary>
        public override PlasticityCommon.DynApplication Application { get { return PlasticityCommon.DynApplication.STInhibitory; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultRestingEfficacy { get { return (RestingEfficacy == DefaultRestingEfficacy); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultTauDepression { get { return (TauDepression == DefaultTauDepression); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultTauFacilitation { get { return (TauFacilitation == DefaultTauFacilitation); } }


        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultRestingEfficacy &&
                       IsDefaultTauDepression &&
                       IsDefaultTauFacilitation;
            }
        }


        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new NonlinearDynamicsSTInhibitorySettings(this);
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
            if (!suppressDefaults || !IsDefaultRestingEfficacy)
            {
                rootElem.Add(new XAttribute("restingEfficacy", RestingEfficacy.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultTauDepression)
            {
                rootElem.Add(new XAttribute("tauDepression", TauDepression.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultTauFacilitation)
            {
                rootElem.Add(new XAttribute("tauFacilitation", TauFacilitation.ToString(CultureInfo.InvariantCulture)));
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
            return GetXml("dynamicsIE", suppressDefaults);
        }

    }//NonlinearDynamicsSTInhibitorySettings

}//Namespace

