using System;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration of the efficacy's dynamics of an input synapse connecting presynaptic input spiking neuron and postsynaptic hidden analog neuron.
    /// </summary>
    [Serializable]
    public class PlasticityATInputSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "SynapsePlasticityATInputType";

        //Attribute properties
        /// <summary>
        /// The configuration of the synapse's efficacy dynamics.
        /// </summary>
        public IDynamicsSettings DynamicsCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="dynamicsCfg">The configuration of the synapse's efficacy dynamics.</param>
        public PlasticityATInputSettings(IDynamicsSettings dynamicsCfg = null)
        {
            if (dynamicsCfg == null)
            {
                DynamicsCfg = new ConstantDynamicsATInputSettings();
            }
            else if (dynamicsCfg.Application != PlasticityCommon.DynApplication.ATInput)
            {
                throw new InvalidOperationException($"Dynamics application must be ATInput.");
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
        public PlasticityATInputSettings(PlasticityATInputSettings source)
            : this(source.DynamicsCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public PlasticityATInputSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            XElement dynamicsCfgElem = settingsElem.Elements().FirstOrDefault();
            if (dynamicsCfgElem == null)
            {
                DynamicsCfg = new ConstantDynamicsATInputSettings();
            }
            else
            {
                switch (dynamicsCfgElem.Name.LocalName)
                {
                    case "constantDynamics":
                        DynamicsCfg = new ConstantDynamicsATInputSettings(dynamicsCfgElem);
                        break;
                    case "linearDynamics":
                        DynamicsCfg = new LinearDynamicsATInputSettings(dynamicsCfgElem);
                        break;
                    case "nonlinearDynamics":
                        DynamicsCfg = new NonlinearDynamicsATInputSettings(dynamicsCfgElem);
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
                return DynamicsCfg.Type == PlasticityCommon.DynType.Constant &&
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
            return new PlasticityATInputSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !((RCNetBaseSettings)DynamicsCfg).ContainsOnlyDefaults)
            {
                rootElem.Add(DynamicsCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("plasticity", suppressDefaults);
        }

    }//PlasticityATInputSettings

}//Namespace

