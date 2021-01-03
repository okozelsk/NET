using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool.NeuronGroup;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Space3D;
using System;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool
{
    /// <summary>
    /// Configuration of the pool of neurons.
    /// </summary>
    [Serializable]
    public class PoolSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "PoolType";

        //Attribute properties
        /// <summary>
        /// The name of the pool.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The configuration of the pool proportions.
        /// </summary>
        public ProportionsSettings ProportionsCfg { get; }
        /// <summary>
        /// The configuration of the pool coordinates.
        /// </summary>
        public CoordinatesSettings CoordinatesCfg { get; }
        /// <summary>
        /// The configuration of neuron groups within the pool.
        /// </summary>
        public NeuronGroupsSettings NeuronGroupsCfg { get; }
        /// <summary>
        /// The configuration of the pool's neurons interconnection.
        /// </summary>
        public InterconnSettings InterconnectionCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="name">The name of the pool.</param>
        /// <param name="proportionsCfg">The configuration of the pool proportions.</param>
        /// <param name="neuronGroupsCfg">The configuration of neuron groups within the pool.</param>
        /// <param name="interconnectionCfg">The configuration of the pool's neurons interconnection.</param>
        /// <param name="coordinatesCfg">The configuration of the pool coordinates.</param>
        public PoolSettings(string name,
                            ProportionsSettings proportionsCfg,
                            NeuronGroupsSettings neuronGroupsCfg,
                            InterconnSettings interconnectionCfg,
                            CoordinatesSettings coordinatesCfg = null
                            )
        {
            Name = name;
            ProportionsCfg = (ProportionsSettings)proportionsCfg.DeepClone();
            NeuronGroupsCfg = (NeuronGroupsSettings)neuronGroupsCfg.DeepClone();
            InterconnectionCfg = (InterconnSettings)interconnectionCfg.DeepClone();
            CoordinatesCfg = coordinatesCfg == null ? new CoordinatesSettings() : (CoordinatesSettings)coordinatesCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">The source instance.</param>
        public PoolSettings(PoolSettings source)
            : this(source.Name, source.ProportionsCfg, source.NeuronGroupsCfg, source.InterconnectionCfg, source.CoordinatesCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public PoolSettings(XElement elem)
        {
            //Validation
            XElement poolSettingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Name = poolSettingsElem.Attribute("name").Value;
            ProportionsCfg = new ProportionsSettings(poolSettingsElem.Elements("proportions").First());
            NeuronGroupsCfg = new NeuronGroupsSettings(poolSettingsElem.Elements("neuronGroups").First());
            InterconnectionCfg = new InterconnSettings(poolSettingsElem.Elements("interconnection").First());
            //Coordinates
            XElement coordinatesElem = poolSettingsElem.Elements("coordinates").FirstOrDefault();
            CoordinatesCfg = coordinatesElem == null ? new CoordinatesSettings() : new CoordinatesSettings(coordinatesElem);
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return false;
            }
        }


        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (Name.Length == 0)
            {
                throw new ArgumentException($"Name can not be empty.", "Name");
            }
            NeuronGroupsCfg.SetGrpNeuronsSubCounts(ProportionsCfg.Size);
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new PoolSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName, new XAttribute("name", Name),
                                                           ProportionsCfg.GetXml(suppressDefaults));
            if (!suppressDefaults || !CoordinatesCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(CoordinatesCfg.GetXml(suppressDefaults));
            }
            rootElem.Add(NeuronGroupsCfg.GetXml(suppressDefaults));
            rootElem.Add(InterconnectionCfg.GetXml(suppressDefaults));
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("pool", suppressDefaults);
        }

    }//PoolSettings

}//Namespace

