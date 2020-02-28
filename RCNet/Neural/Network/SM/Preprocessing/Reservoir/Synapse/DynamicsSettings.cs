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
using System.Xml.XPath;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration of synapse short-term plasticity dynamics
    /// </summary>
    [Serializable]
    public abstract class DynamicsSettings : RCNetBaseSettings
    {
        //Attribute properties
        /// <summary>
        /// Synapse's resting efficacy (average probability of neurotransmitter release)
        /// </summary>
        public double RestingEfficacy { get; }
        /// <summary>
        /// Synapse's efficacy depression model time constant (ms)
        /// </summary>
        public double TauDepression { get; }
        /// <summary>
        /// Synapse's efficacy facilitation model time constant (ms)
        /// </summary>
        public double TauFacilitation { get; }
        /// <summary>
        /// Specifies whether to apply short-term plasticity
        /// </summary>
        public bool Apply { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="restingEfficacy">Synapse's resting efficacy (average probability of neurotransmitter release)</param>
        /// <param name="tauDepression">Synapse's efficacy depression model time constant (ms)</param>
        /// <param name="tauFacilitation">Synapse's efficacy facilitation model time constant (ms)</param>
        /// <param name="apply">Specifies whether to apply short-term plasticity</param>
        public DynamicsSettings(double restingEfficacy,
                                double tauDepression,
                                double tauFacilitation,
                                bool apply
                                )
        {
            RestingEfficacy = restingEfficacy;
            TauDepression = tauDepression;
            TauFacilitation = tauFacilitation;
            Apply = apply;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public DynamicsSettings(DynamicsSettings source)
            :this(source.RestingEfficacy, source.TauDepression, source.TauFacilitation, source.Apply)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing settings</param>
        /// <param name="xsdTypeName">Name of the associated type defined in xsd</param>
        public DynamicsSettings(XElement elem, string xsdTypeName)
        {
            //Validation
            XElement settingsElem = Validate(elem, xsdTypeName);
            //Parsing
            //Resting efficacy
            RestingEfficacy = double.Parse(settingsElem.Attribute("restingEfficacy").Value, CultureInfo.InvariantCulture);
            //Efficacy depression
            TauDepression = double.Parse(settingsElem.Attribute("tauDepression").Value, CultureInfo.InvariantCulture);
            //Efficacy facilitation
            TauFacilitation = double.Parse(settingsElem.Attribute("tauFacilitation").Value, CultureInfo.InvariantCulture);
            //Apply short-term plasticity ?
            Apply = bool.Parse(settingsElem.Attribute("apply").Value);
            Check();
            return;
        }

        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (RestingEfficacy < 0 || RestingEfficacy > 1)
            {
                throw new Exception($"Invalid RestingEfficacy {RestingEfficacy.ToString(CultureInfo.InvariantCulture)}. RestingEfficacy must be GE to 0 and LE to 1.");
            }
            if (TauDepression < 0)
            {
                throw new Exception($"Invalid TauDepression {TauDepression.ToString(CultureInfo.InvariantCulture)}. TauDepression must be GE to 0.");
            }
            if (TauFacilitation < 0)
            {
                throw new Exception($"Invalid TauFacilitation {TauFacilitation.ToString(CultureInfo.InvariantCulture)}. TauFacilitation must be GE to 0.");
            }
            return;
        }

    }//DynamicsSettings

}//Namespace

