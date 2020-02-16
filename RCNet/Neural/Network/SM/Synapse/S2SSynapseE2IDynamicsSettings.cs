using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.IO;
using RCNet.Extensions;
using RCNet.MathTools.Probability;
using RCNet.XmlTools;
using RCNet.RandomValue;
using RCNet.Neural.Network.SM.Preprocessing;
using System.Xml.XPath;

namespace RCNet.Neural.Network.SM.Synapse
{
    /// <summary>
    /// Configuration parameters of an input synapse spiking target
    /// </summary>
    [Serializable]
    public class S2SSynapseE2IDynamicsSettings : DynamicsSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "S2SSynapseE2IDynamicsCfgType";
        
        //Default values
        /// <summary>
        /// Default resting efficacy
        /// </summary>
        public const double DefaultRestingEfficacy = 0.05d;
        /// <summary>
        /// Default tau depression
        /// </summary>
        public const double DefaultTauDepression = 125d;
        /// <summary>
        /// Default tau facilitation
        /// </summary>
        public const double DefaultTauFacilitation = 1200d;
        /// <summary>
        /// Default apply short term plasticity
        /// </summary>
        public const bool DefaultApplyShortTermPlasticity = true;


        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="restingEfficacy">Synapse's resting efficacy (average probability of neurotransmitter release)</param>
        /// <param name="tauDepression">Synapse's efficacy depression model time constant (ms)</param>
        /// <param name="tauFacilitation">Synapse's efficacy facilitation model time constant (ms)</param>
        /// <param name="applyShortTermPlasticity">Specifies whether to apply short-term plasticity</param>
        /// <param name="weightCfg">Synapse's random weight settings</param>
        public S2SSynapseE2IDynamicsSettings(double restingEfficacy = DefaultRestingEfficacy,
                                             double tauDepression = DefaultTauDepression,
                                             double tauFacilitation = DefaultTauFacilitation,
                                             bool applyShortTermPlasticity = DefaultApplyShortTermPlasticity,
                                             URandomValueSettings weightCfg = null
                                             )
            :base(restingEfficacy, tauDepression, tauFacilitation, applyShortTermPlasticity, weightCfg)
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public S2SSynapseE2IDynamicsSettings(S2SSynapseE2IDynamicsSettings source)
            :base(source)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="settingsElem">
        /// Xml data containing settings.
        /// Content of xml element is not validated against the xml schema.
        /// </param>
        public S2SSynapseE2IDynamicsSettings(XElement elem)
            :base(elem, XsdTypeName)
        {
            return;
        }

        //Properties
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
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultApplyShortTermPlasticity { get { return (ApplyShortTermPlasticity == DefaultApplyShortTermPlasticity); } }


        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultRestingEfficacy &&
                       IsDefaultTauDepression &&
                       IsDefaultTauFacilitation &&
                       IsDefaultApplyShortTermPlasticity &&
                       IsDefaultWeightCfg;
            }
        }


        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new S2SSynapseE2IDynamicsSettings(this);
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
            if (!suppressDefaults || !IsDefaultApplyShortTermPlasticity)
            {
                rootElem.Add(new XAttribute("applyShortTermPlasticity", ApplyShortTermPlasticity.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
            }
            if (!suppressDefaults || !IsDefaultWeightCfg)
            {
                rootElem.Add(WeightCfg.GetXml("weight", suppressDefaults));
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
            return GetXml("dynamicsS2SEI", suppressDefaults);
        }

    }//S2SSynapseE2IDynamicsSettings

}//Namespace

