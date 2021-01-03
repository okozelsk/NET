using RCNet.Neural.Network.SM.Preprocessing.Input;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS;
using System;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
    /// <summary>
    /// Configuration of the reservoir instance.
    /// </summary>
    [Serializable]
    public class ReservoirInstanceSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "NPResInstanceType";

        //Attribute properties
        /// <summary>
        /// The name of the reservoir instance.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The name of the reservoir structure configuration.
        /// </summary>
        public string StructureCfgName { get; }

        /// <summary>
        /// The configuration of the input fields connections.
        /// </summary>
        public InputConnsSettings InputConnsCfg { get; }

        /// <summary>
        /// The configuration of a synapse.
        /// </summary>
        public SynapseSettings SynapseCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="name">The name of the reservoir instance.</param>
        /// <param name="structureCfgName">The name of the reservoir structure configuration.</param>
        /// <param name="inputConnsCfg">The configuration of the input fields connections.</param>
        /// <param name="synapseCfg">The configuration of a synapse.</param>
        public ReservoirInstanceSettings(string name,
                                         string structureCfgName,
                                         InputConnsSettings inputConnsCfg,
                                         SynapseSettings synapseCfg = null
                                         )
        {
            Name = name;
            StructureCfgName = structureCfgName;
            InputConnsCfg = (InputConnsSettings)inputConnsCfg.DeepClone();
            SynapseCfg = synapseCfg == null ? null : (SynapseSettings)synapseCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public ReservoirInstanceSettings(ReservoirInstanceSettings source)
            : this(source.Name, source.StructureCfgName, source.InputConnsCfg, source.SynapseCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public ReservoirInstanceSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Name = settingsElem.Attribute("name").Value;
            StructureCfgName = settingsElem.Attribute("reservoirStructure").Value;
            //Input connections
            InputConnsCfg = new InputConnsSettings(settingsElem.Elements("inputConnections").First());
            //Synapse
            XElement synapseElem = settingsElem.Elements("synapse").FirstOrDefault();
            SynapseCfg = synapseElem == null ? new SynapseSettings() : new SynapseSettings(synapseElem);
            Check();
            return;
        }

        //Properties
        /// <inheritdoc />
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return false;
            }
        }

        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            if (Name.Length == 0)
            {
                throw new ArgumentException($"Name can not be empty.", "Name");
            }
            if (StructureCfgName.Length == 0)
            {
                throw new ArgumentException($"Name of the reservoir structure configuration can not be empty.", "StructureCfgName");
            }
            return;
        }

        /// <summary>
        /// Performs an additional consistency check.
        /// </summary>
        /// <param name="inputCfg">The configuration of the input encoder.</param>
        /// <param name="reservoirStructureCfg">The configuration of the reservoir structure.</param>
        public void CheckConsistency(InputEncoderSettings inputCfg, ReservoirStructureSettings reservoirStructureCfg)
        {
            if (StructureCfgName != reservoirStructureCfg.Name)
            {
                throw new ArgumentException($"The specified name of the reservoir structure configuration {StructureCfgName} does not correspond.", "StructureCfgName");
            }
            foreach (InputConnSettings inputConnCfg in InputConnsCfg.ConnCfgCollection)
            {
                inputCfg.VaryingFieldsCfg.GetFieldID(inputConnCfg.InputFieldName, true);
                reservoirStructureCfg.PoolsCfg.GetPoolID(inputConnCfg.PoolName);
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new ReservoirInstanceSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("name", Name),
                                             new XAttribute("reservoirStructure", StructureCfgName),
                                             InputConnsCfg.GetXml(suppressDefaults));

            if (SynapseCfg != null && !SynapseCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(SynapseCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("reservoirInstance", suppressDefaults);
        }

    }//ReservoirInstanceSettings

}//Namespace

