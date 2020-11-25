using RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool.NeuronGroup;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Space3D;
using System;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool
{
    /// <summary>
    /// Configuration of a neural pool.
    /// </summary>
    [Serializable]
    public class PoolSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PoolType";

        //Attribute properties
        /// <summary>
        /// Pool name
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Pool dimensions
        /// </summary>
        public ProportionsSettings ProportionsCfg { get; }
        /// <summary>
        /// Pool coordinates within the 3D space
        /// </summary>
        public CoordinatesSettings CoordinatesCfg { get; }
        /// <summary>
        /// Settings of the neuron groups in the pool
        /// </summary>
        public NeuronGroupsSettings NeuronGroupsCfg { get; }
        /// <summary>
        /// Configuration of the pool's neurons interconnection
        /// </summary>
        public InterconnSettings InterconnectionCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="name">Pool name</param>
        /// <param name="proportionsCfg">Pool dimensions</param>
        /// <param name="neuronGroupsCfg">Settings of the neuron groups in the pool</param>
        /// <param name="interconnectionCfg">Configuration of the pool's neurons interconnection</param>
        /// <param name="coordinatesCfg">Pool coordinates within the 3D space</param>
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
        /// <param name="source">Source instance</param>
        public PoolSettings(PoolSettings source)
            : this(source.Name, source.ProportionsCfg, source.NeuronGroupsCfg, source.InterconnectionCfg, source.CoordinatesCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance from the given xml element.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
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
        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return false;
            }
        }


        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            if (Name.Length == 0)
            {
                throw new ArgumentException($"Name can not be empty.", "Name");
            }
            NeuronGroupsCfg.SetGrpNeuronsSubCounts(ProportionsCfg.Size);
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new PoolSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
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

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("pool", suppressDefaults);
        }

    }//PoolSettings

}//Namespace

