using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration of the synapse's nonlinear efficacy dynamics.
    /// </summary>
    [Serializable]
    public abstract class NonlinearDynamicsSettings : RCNetBaseSettings, IDynamicsSettings
    {
        //Attribute properties
        /// <summary>
        /// The value of the resting efficacy.
        /// </summary>
        public double RestingEfficacy { get; }
        /// <summary>
        /// The value of the tau depression (ms).
        /// </summary>
        public double TauDepression { get; }
        /// <summary>
        /// The value of the tau facilitation (ms).
        /// </summary>
        public double TauFacilitation { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="restingEfficacy">The value of the resting efficacy.</param>
        /// <param name="tauDepression">The value of the tau depression (ms).</param>
        /// <param name="tauFacilitation">The value of the tau facilitation (ms).</param>
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
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public NonlinearDynamicsSettings(NonlinearDynamicsSettings source)
            : this(source.RestingEfficacy, source.TauDepression, source.TauFacilitation)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
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

