using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration of the synapse linear dynamics (indifferent spiking to hidden analog neuron)
    /// </summary>
    [Serializable]
    public class LinearDynamicsATIndifferentSettings : LinearDynamicsSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "SynapseLinearDynamicsATIndifferentType";

        //Default values
        /// <summary>
        /// Default alpha argument in the linear expression efficacy = alpha * (spike - beta)
        /// </summary>
        public const double DefaultAlpha = 0.007d;
        /// <summary>
        /// Default beta argument in the linear expression efficacy = alpha * (spike - beta)
        /// </summary>
        public const double DefaultBeta = 0.739d;
        /// <summary>
        /// Default synapse initial efficacy
        /// </summary>
        public const double DefaultInitialEfficacy = 0.75d;


        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="alpha">Alpha argument in the linear expression efficacy = alpha * (spike - beta)</param>
        /// <param name="beta">Beta argument in the linear expression efficacy = alpha * (spike - beta)</param>
        /// <param name="initialEfficacy">Synapse initial efficacy</param>
        public LinearDynamicsATIndifferentSettings(double alpha = DefaultAlpha, double beta = DefaultBeta, double initialEfficacy = DefaultInitialEfficacy)
            : base(alpha, beta, initialEfficacy)
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public LinearDynamicsATIndifferentSettings(LinearDynamicsATIndifferentSettings source)
            : base(source)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public LinearDynamicsATIndifferentSettings(XElement elem)
            : base(elem, XsdTypeName)
        {
            return;
        }

        //Properties
        /// <inheritdoc />
        public override PlasticityCommon.DynApplication Application { get { return PlasticityCommon.DynApplication.ATIndifferent; } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultAlpha { get { return (Alpha == DefaultAlpha); } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultBeta { get { return (Beta == DefaultBeta); } }

        /// <summary>
        /// Checks the defaults
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
            return new LinearDynamicsATIndifferentSettings(this);
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

    }//LinearDynamicsATIndifferentSettings

}//Namespace

