using System;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration of the efficacy's dynamics of an inhibitory synapse connecting presynaptic hidden spiking neuron and postsynaptic hidden spiking neuron.
    /// </summary>
    [Serializable]
    public class PlasticitySTInhibitorySettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "SynapsePlasticitySTInhibitoryType";

        //Attribute properties
        /// <summary>
        /// The configuration of the synapse's efficacy dynamics.
        /// </summary>
        public IDynamicsSettings DynamicsCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public PlasticitySTInhibitorySettings(IDynamicsSettings dynamicsCfg = null)
        {
            if (dynamicsCfg == null)
            {
                DynamicsCfg = new NonlinearDynamicsSTInhibitorySettings();
            }
            else if (dynamicsCfg.Application != PlasticityCommon.DynApplication.STInhibitory)
            {
                throw new InvalidOperationException($"Dynamics application must be STInhibitory.");
            }
            else
            {
                DynamicsCfg = (IDynamicsSettings)(((RCNetBaseSettings)dynamicsCfg).DeepClone());
            }
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">The source instance.</param>
        public PlasticitySTInhibitorySettings(PlasticitySTInhibitorySettings source)
            : this(source.DynamicsCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public PlasticitySTInhibitorySettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            XElement dynamicsCfgElem = settingsElem.Elements().FirstOrDefault();
            if (dynamicsCfgElem == null)
            {
                DynamicsCfg = new NonlinearDynamicsSTInhibitorySettings();
            }
            else
            {
                switch (dynamicsCfgElem.Name.LocalName)
                {
                    case "constantDynamics":
                        DynamicsCfg = new ConstantDynamicsSTInhibitorySettings(dynamicsCfgElem);
                        break;
                    case "linearDynamics":
                        DynamicsCfg = new LinearDynamicsSTInhibitorySettings(dynamicsCfgElem);
                        break;
                    case "nonlinearDynamics":
                        DynamicsCfg = new NonlinearDynamicsSTInhibitorySettings(dynamicsCfgElem);
                        break;
                    default:
                        throw new InvalidOperationException($"Unexpected element name {dynamicsCfgElem.Name.LocalName}.");
                }
            }
            return;
        }

        //Properties
        /// <inheritdoc />
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return DynamicsCfg.Type == PlasticityCommon.DynType.Linear &&
                       ((RCNetBaseSettings)DynamicsCfg).ContainsOnlyDefaults;
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
            return new PlasticitySTInhibitorySettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName, DynamicsCfg.GetXml(suppressDefaults));
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("plasticity", suppressDefaults);
        }

    }//PlasticitySTInhibitorySettings

}//Namespace

