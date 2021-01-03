using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
    /// <summary>
    /// Configuration of the pool configurations.
    /// </summary>
    [Serializable]
    public class PoolsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ResStructPoolsType";

        //Attribute properties
        /// <summary>
        /// The collection of the pool configurations.
        /// </summary>
        public List<PoolSettings> PoolCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        private PoolsSettings()
        {
            PoolCfgCollection = new List<PoolSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="poolCfgCollection">The collection of the pool configurations.</param>
        public PoolsSettings(IEnumerable<PoolSettings> poolCfgCollection)
            : this()
        {
            AddPools(poolCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="poolCfgCollection">The pool configurations.</param>
        public PoolsSettings(params PoolSettings[] poolCfgCollection)
            : this()
        {
            AddPools(poolCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public PoolsSettings(PoolsSettings source)
            : this()
        {
            AddPools(source.PoolCfgCollection);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public PoolsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            PoolCfgCollection = new List<PoolSettings>();
            foreach (XElement poolElem in settingsElem.Elements("pool"))
            {
                PoolCfgCollection.Add(new PoolSettings(poolElem));
            }
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Gets the total number of hidden neurons within all pools.
        /// </summary>
        public int TotalSize
        {
            get
            {
                int sum = 0;
                foreach (PoolSettings poolCfg in PoolCfgCollection)
                {
                    sum += poolCfg.ProportionsCfg.Size;
                }
                return sum;
            }
        }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            if (PoolCfgCollection.Count == 0)
            {
                throw new ArgumentException($"At least one pool configuration must be specified.", "PoolCfgCollection");
            }
            //Uniqueness of pool names
            string[] names = new string[PoolCfgCollection.Count];
            names[0] = PoolCfgCollection[0].Name;
            for (int i = 1; i < PoolCfgCollection.Count; i++)
            {
                if (names.Contains(PoolCfgCollection[i].Name))
                {
                    throw new ArgumentException($"Pool name {PoolCfgCollection[i].Name} is not unique.", "PoolCfgCollection");
                }
                names[i] = PoolCfgCollection[i].Name;
            }
            return;
        }

        /// <summary>
        /// Adds pool configurations from the specified collection into the internal collection.
        /// </summary>
        /// <param name="poolCfgCollection">The collection of the pool configurations.</param>
        private void AddPools(IEnumerable<PoolSettings> poolCfgCollection)
        {
            foreach (PoolSettings poolCfg in poolCfgCollection)
            {
                PoolCfgCollection.Add((PoolSettings)poolCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Gets an identifier (index) of the specified pool.
        /// </summary>
        /// <param name="poolName">The name of the pool.</param>
        public int GetPoolID(string poolName)
        {
            for (int i = 0; i < PoolCfgCollection.Count; i++)
            {
                if (PoolCfgCollection[i].Name == poolName)
                {
                    return i;
                }
            }
            throw new InvalidOperationException($"Pool name {poolName} not found.");
        }

        /// <summary>
        /// Gets a configuration of the specified pool.
        /// </summary>
        /// <param name="poolName">The name of the pool.</param>
        public PoolSettings GetPoolCfg(string poolName)
        {
            return PoolCfgCollection[GetPoolID(poolName)];
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new PoolsSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach (PoolSettings poolCfg in PoolCfgCollection)
            {
                rootElem.Add(poolCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("pools", suppressDefaults);
        }

    }//PoolsSettings

}//Namespace
