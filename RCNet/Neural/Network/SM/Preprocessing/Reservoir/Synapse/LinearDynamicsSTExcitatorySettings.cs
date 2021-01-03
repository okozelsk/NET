using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration of the efficacy's linear dynamics of an excitatory synapse connecting presynaptic hidden spiking neuron and postsynaptic hidden spiking neuron.
    /// </summary>
    [Serializable]
    public class LinearDynamicsSTExcitatorySettings : LinearDynamicsSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "SynapseLinearDynamicsSTExcitatoryType";

        //Default values
        /// <summary>
        /// The default value of the alpha argument in the linear expression: efficacy = alpha * (spike - beta).
        /// </summary>
        public const double DefaultAlpha = 0.007d;
        /// <summary>
        /// The default value of the beta argument in the linear expression: efficacy = alpha * (spike - beta).
        /// </summary>
        public const double DefaultBeta = 0.739d;
        /// <summary>
        /// The default value of the synapse's initial efficacy.
        /// </summary>
        public const double DefaultInitialEfficacy = 0.75d;


        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="alpha">The value of the alpha argument in the linear expression: efficacy = alpha * (spike - beta).</param>
        /// <param name="beta">The value of the beta argument in the linear expression: efficacy = alpha * (spike - beta).</param>
        /// <param name="initialEfficacy">The value of the synapse's initial efficacy.</param>
        public LinearDynamicsSTExcitatorySettings(double alpha = DefaultAlpha, double beta = DefaultBeta, double initialEfficacy = DefaultInitialEfficacy)
            : base(alpha, beta, initialEfficacy)
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">The source instance.</param>
        public LinearDynamicsSTExcitatorySettings(LinearDynamicsSTExcitatorySettings source)
            : base(source)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public LinearDynamicsSTExcitatorySettings(XElement elem)
            : base(elem, XsdTypeName)
        {
            return;
        }

        //Properties
        /// <inheritdoc />
        public override PlasticityCommon.DynApplication Application { get { return PlasticityCommon.DynApplication.STExcitatory; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultAlpha { get { return (Alpha == DefaultAlpha); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultBeta { get { return (Beta == DefaultBeta); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultInitialEfficacy { get { return (InitialEfficacy == DefaultInitialEfficacy); } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultAlpha && IsDefaultBeta && IsDefaultInitialEfficacy;
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
            return new LinearDynamicsSTExcitatorySettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultAlpha)
            {
                rootElem.Add(new XAttribute("alpha", Alpha.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultBeta)
            {
                rootElem.Add(new XAttribute("beta", Beta.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultInitialEfficacy)
            {
                rootElem.Add(new XAttribute("initialEfficacy", InitialEfficacy.ToString(CultureInfo.InvariantCulture)));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("linearDynamics", suppressDefaults);
        }

    }//LinearDynamicsSTExcitatorySettings

}//Namespace

