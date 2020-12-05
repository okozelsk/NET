using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool;
using System;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
    /// <summary>
    /// Configuration of the reservoir structure
    /// </summary>
    [Serializable]
    public class ReservoirStructureSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ResStructType";

        //Attribute properties
        /// <summary>
        /// Name of the reservoir structure settings
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Collection of neural pools settings
        /// </summary>
        public PoolsSettings PoolsCfg { get; }
        /// <summary>
        /// Collection of pools interconnection settings
        /// </summary>
        public InterPoolConnsSettings InterPoolConnectionsCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="name">Name of the reservoir structure settings</param>
        /// <param name="poolsCfg">Collection of neural pools settings</param>
        /// <param name="interPoolConnectionsCfg">Collection of pools interconnection settings</param>
        public ReservoirStructureSettings(string name,
                                          PoolsSettings poolsCfg,
                                          InterPoolConnsSettings interPoolConnectionsCfg = null
                                          )
        {
            Name = name;
            PoolsCfg = (PoolsSettings)poolsCfg.DeepClone();
            InterPoolConnectionsCfg = (InterPoolConnsSettings)interPoolConnectionsCfg?.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ReservoirStructureSettings(ReservoirStructureSettings source)
            : this(source.Name, source.PoolsCfg, source.InterPoolConnectionsCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public ReservoirStructureSettings(XElement elem)
        {
            //Validation
            XElement reservoirSettingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Name = reservoirSettingsElem.Attribute("name").Value;
            //Pool settings collection
            PoolsCfg = new PoolsSettings(reservoirSettingsElem.Elements("pools").First());
            //Inter pool connections
            InterPoolConnectionsCfg = null;
            XElement interPoolConnectionsElem = reservoirSettingsElem.Elements("interPoolConnections").FirstOrDefault();
            if (interPoolConnectionsElem != null)
            {
                InterPoolConnectionsCfg = new InterPoolConnsSettings(interPoolConnectionsElem);
            }
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Size of largest interconnected area of hidden neurons
        /// </summary>
        public int LargestInterconnectedAreaSize
        {
            get
            {
                int maxPoolSize = 0;
                int interPoolSize = 0;
                foreach (PoolSettings poolCfg in PoolsCfg.PoolCfgCollection)
                {
                    int poolSize = poolCfg.ProportionsCfg.Size;
                    maxPoolSize = Math.Max(maxPoolSize, poolSize);
                    if (InterPoolConnectionsCfg != null)
                    {
                        foreach (InterPoolConnSettings conn in InterPoolConnectionsCfg.InterPoolConnectionCfgCollection)
                        {
                            if (conn.SourcePoolName == poolCfg.Name || conn.TargetPoolName == poolCfg.Name)
                            {
                                interPoolSize += poolSize;
                                break;
                            }
                        }
                    }
                }
                return Math.Max(maxPoolSize, interPoolSize);
            }
        }

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
            //Integrity check
            if (InterPoolConnectionsCfg != null)
            {
                foreach (InterPoolConnSettings interPoolConnectionCfg in InterPoolConnectionsCfg.InterPoolConnectionCfgCollection)
                {
                    PoolsCfg.GetPoolID(interPoolConnectionCfg.SourcePoolName);
                    PoolsCfg.GetPoolID(interPoolConnectionCfg.TargetPoolName);
                }
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new ReservoirStructureSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("name", Name),
                                             PoolsCfg.GetXml(suppressDefaults));
            if (InterPoolConnectionsCfg != null)
            {
                rootElem.Add(InterPoolConnectionsCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("reservoirStructure", suppressDefaults);
        }

    }//ReservoirStructureSettings

}//Namespace

