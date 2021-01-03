using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration of the efficacy's nonlinear dynamics of an indifferent synapse connecting presynaptic hidden spiking neuron and postsynaptic hidden analog neuron.
    /// </summary>
    [Serializable]
    public class NonlinearDynamicsATIndifferentSettings : NonlinearDynamicsSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "SynapseNonlinearDynamicsATIndifferentType";

        //Default values
        /// <summary>
        /// The default value of the resting efficacy.
        /// </summary>
        public const double DefaultRestingEfficacy = 0.99d;
        /// <summary>
        /// The default value of the tau depression.
        /// </summary>
        public const double DefaultTauDepression = 3d;
        /// <summary>
        /// The default value of the tau facilitation.
        /// </summary>
        public const double DefaultTauFacilitation = 1d;


        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="restingEfficacy">The value of the resting efficacy.</param>
        /// <param name="tauDepression">The value of the tau depression (ms).</param>
        /// <param name="tauFacilitation">The value of the tau facilitation (ms).</param>
        public NonlinearDynamicsATIndifferentSettings(double restingEfficacy = DefaultRestingEfficacy,
                                                      double tauDepression = DefaultTauDepression,
                                                      double tauFacilitation = DefaultTauFacilitation
                                                      )
            : base(restingEfficacy, tauDepression, tauFacilitation)
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public NonlinearDynamicsATIndifferentSettings(NonlinearDynamicsATIndifferentSettings source)
            : base(source)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public NonlinearDynamicsATIndifferentSettings(XElement elem)
            : base(elem, XsdTypeName)
        {
            return;
        }

        //Properties
        /// <inheritdoc />
        public override PlasticityCommon.DynApplication Application { get { return PlasticityCommon.DynApplication.ATIndifferent; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultRestingEfficacy { get { return (RestingEfficacy == DefaultRestingEfficacy); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultTauDepression { get { return (TauDepression == DefaultTauDepression); } }

        /// <summary>
        /// Checks the defaults.
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
            return new NonlinearDynamicsATIndifferentSettings(this);
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

    }//NonlinearDynamicsATIndifferentSettings

}//Namespace

