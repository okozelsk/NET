using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration of the synapse nonlinear dynamics (input spiking to hidden spiking neuron)
    /// </summary>
    [Serializable]
    public class NonlinearDynamicsSTInputSettings : NonlinearDynamicsSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "SynapseNonlinearDynamicsSTInputType";

        //Default values
        /// <summary>
        /// Default resting efficacy
        /// </summary>
        public const double DefaultRestingEfficacy = 0.99d;
        /// <summary>
        /// Default tau depression
        /// </summary>
        public const double DefaultTauDepression = 3d;
        /// <summary>
        /// Default tau facilitation
        /// </summary>
        public const double DefaultTauFacilitation = 1d;


        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="restingEfficacy">Synapse's resting efficacy (average probability of neurotransmitter release)</param>
        /// <param name="tauDepression">Synapse's efficacy depression model time constant (ms)</param>
        /// <param name="tauFacilitation">Synapse's efficacy facilitation model time constant (ms)</param>
        public NonlinearDynamicsSTInputSettings(double restingEfficacy = DefaultRestingEfficacy,
                                                double tauDepression = DefaultTauDepression,
                                                double tauFacilitation = DefaultTauFacilitation
                                                )
            : base(restingEfficacy, tauDepression, tauFacilitation)
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public NonlinearDynamicsSTInputSettings(NonlinearDynamicsSTInputSettings source)
            : base(source)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public NonlinearDynamicsSTInputSettings(XElement elem)
            : base(elem, XsdTypeName)
        {
            return;
        }

        //Properties
        /// <inheritdoc />
        public override PlasticityCommon.DynApplication Application { get { return PlasticityCommon.DynApplication.STInput; } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultRestingEfficacy { get { return (RestingEfficacy == DefaultRestingEfficacy); } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultTauDepression { get { return (TauDepression == DefaultTauDepression); } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultTauFacilitation { get { return (TauFacilitation == DefaultTauFacilitation); } }


        /// <inheritdoc />
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
        /// <inheritdoc />
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new NonlinearDynamicsSTInputSettings(this);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("nonlinearDynamics", suppressDefaults);
        }

    }//NonlinearDynamicsSTInputSettings

}//Namespace

