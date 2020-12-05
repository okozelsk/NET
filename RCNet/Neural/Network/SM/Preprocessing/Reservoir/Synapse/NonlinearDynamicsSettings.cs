using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration of the synapse nonlinear short-term plasticity dynamics
    /// </summary>
    [Serializable]
    public abstract class NonlinearDynamicsSettings : RCNetBaseSettings, IDynamicsSettings
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

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="restingEfficacy">Synapse's resting efficacy (average probability of neurotransmitter release)</param>
        /// <param name="tauDepression">Synapse's efficacy depression model time constant (ms)</param>
        /// <param name="tauFacilitation">Synapse's efficacy facilitation model time constant (ms)</param>
        public NonlinearDynamicsSettings(double restingEfficacy,
                                         double tauDepression,
                                         double tauFacilitation
                                         )
        {
            RestingEfficacy = restingEfficacy;
            TauDepression = tauDepression;
            TauFacilitation = tauFacilitation;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public NonlinearDynamicsSettings(NonlinearDynamicsSettings source)
            : this(source.RestingEfficacy, source.TauDepression, source.TauFacilitation)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        /// <param name="xsdTypeName">Name of the associated type defined in xsd</param>
        public NonlinearDynamicsSettings(XElement elem, string xsdTypeName)
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
            Check();
            return;
        }

        //Properties
        /// <inheritdoc />
        public PlasticityCommon.DynType Type { get { return PlasticityCommon.DynType.Nonlinear; } }

        /// <inheritdoc />
        public abstract PlasticityCommon.DynApplication Application { get; }

        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            if (RestingEfficacy < 0 || RestingEfficacy > 1)
            {
                throw new ArgumentException($"Invalid RestingEfficacy {RestingEfficacy.ToString(CultureInfo.InvariantCulture)}. RestingEfficacy must be GE to 0 and LE to 1.", "RestingEfficacy");
            }
            if (TauDepression < 0)
            {
                throw new ArgumentException($"Invalid TauDepression {TauDepression.ToString(CultureInfo.InvariantCulture)}. TauDepression must be GE to 0.", "TauDepression");
            }
            if (TauFacilitation < 0)
            {
                throw new ArgumentException($"Invalid TauFacilitation {TauFacilitation.ToString(CultureInfo.InvariantCulture)}. TauFacilitation must be GE to 0.", "TauFacilitation");
            }
            return;
        }

    }//NonlinearDynamicsSettings

}//Namespace

