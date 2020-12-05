using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration of the synapse constant dynamics (excitatory spiking to hidden spiking neuron)
    /// </summary>
    [Serializable]
    public class ConstantDynamicsSTExcitatorySettings : ConstantDynamicsSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "SynapseConstantDynamicsSTExcitatoryType";

        //Default values
        /// <summary>
        /// Default synapse's constant efficacy
        /// </summary>
        public const double DefaultEfficacy = 1d;


        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="efficacy">Default synapse's constant efficacy</param>
        public ConstantDynamicsSTExcitatorySettings(double efficacy = DefaultEfficacy)
            : base(efficacy)
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ConstantDynamicsSTExcitatorySettings(ConstantDynamicsSTExcitatorySettings source)
            : base(source)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public ConstantDynamicsSTExcitatorySettings(XElement elem)
            : base(elem, XsdTypeName)
        {
            return;
        }

        //Properties
        /// <inheritdoc />
        public override PlasticityCommon.DynApplication Application { get { return PlasticityCommon.DynApplication.STExcitatory; } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultEfficacy { get { return (Efficacy == DefaultEfficacy); } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultEfficacy;
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
            return new ConstantDynamicsSTExcitatorySettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultEfficacy)
            {
                rootElem.Add(new XAttribute("efficacy", Efficacy.ToString(CultureInfo.InvariantCulture)));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("constantDynamics", suppressDefaults);
        }

    }//ConstantDynamicsSTExcitatorySettings

}//Namespace

