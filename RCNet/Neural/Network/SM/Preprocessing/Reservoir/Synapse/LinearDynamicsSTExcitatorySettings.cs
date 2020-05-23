using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration parameters of the synapse linear dynamics (excitatory spiking to hidden spiking neuron)
    /// </summary>
    [Serializable]
    public class LinearDynamicsSTExcitatorySettings : LinearDynamicsSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "SynapseLinearDynamicsSTExcitatoryType";

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
        public LinearDynamicsSTExcitatorySettings(double alpha = DefaultAlpha, double beta = DefaultBeta, double initialEfficacy = DefaultInitialEfficacy)
            : base(alpha, beta, initialEfficacy)
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public LinearDynamicsSTExcitatorySettings(LinearDynamicsSTExcitatorySettings source)
            : base(source)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public LinearDynamicsSTExcitatorySettings(XElement elem)
            : base(elem, XsdTypeName)
        {
            return;
        }

        //Properties
        /// <summary>
        /// Application of the synapse's dynamics
        /// </summary>
        public override PlasticityCommon.DynApplication Application { get { return PlasticityCommon.DynApplication.STExcitatory; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultAlpha { get { return (Alpha == DefaultAlpha); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultBeta { get { return (Beta == DefaultBeta); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultInitialEfficacy { get { return (InitialEfficacy == DefaultInitialEfficacy); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultAlpha && IsDefaultBeta && IsDefaultInitialEfficacy;
            }
        }


        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new LinearDynamicsSTExcitatorySettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
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

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("linearDynamics", suppressDefaults);
        }

    }//LinearDynamicsSTExcitatorySettings

}//Namespace

