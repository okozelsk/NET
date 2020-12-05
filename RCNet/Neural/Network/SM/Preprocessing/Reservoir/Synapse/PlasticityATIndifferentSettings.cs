using System;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Synapse's plasticity configuration (indifferent spiking to hidden analog neuron)
    /// </summary>
    [Serializable]
    public class PlasticityATIndifferentSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "SynapsePlasticityATIndifferentType";

        //Attribute properties
        /// <summary>
        /// Synapse's dynamics configuration
        /// </summary>
        public IDynamicsSettings DynamicsCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public PlasticityATIndifferentSettings(IDynamicsSettings dynamicsCfg = null)
        {
            if (dynamicsCfg == null)
            {
                DynamicsCfg = new ConstantDynamicsATIndifferentSettings();
            }
            else if (dynamicsCfg.Application != PlasticityCommon.DynApplication.ATIndifferent)
            {
                throw new InvalidOperationException($"Dynamics application must be ATIndifferent.");
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
        /// <param name="source">Source instance</param>
        public PlasticityATIndifferentSettings(PlasticityATIndifferentSettings source)
            : this(source.DynamicsCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public PlasticityATIndifferentSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            XElement dynamicsCfgElem = settingsElem.Elements().FirstOrDefault();
            if (dynamicsCfgElem == null)
            {
                DynamicsCfg = new ConstantDynamicsATIndifferentSettings();
            }
            else
            {
                switch (dynamicsCfgElem.Name.LocalName)
                {
                    case "constantDynamics":
                        DynamicsCfg = new ConstantDynamicsATIndifferentSettings(dynamicsCfgElem);
                        break;
                    case "linearDynamics":
                        DynamicsCfg = new LinearDynamicsATIndifferentSettings(dynamicsCfgElem);
                        break;
                    case "nonlinearDynamics":
                        DynamicsCfg = new NonlinearDynamicsATIndifferentSettings(dynamicsCfgElem);
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
            return new PlasticityATIndifferentSettings(this);
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

    }//PlasticityATIndifferentSettings

}//Namespace

