using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
    /// <summary>
    /// Collection of the pools configurations
    /// </summary>
    [Serializable]
    public class PoolsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ResStructPoolsType";

        //Attribute properties
        /// <summary>
        /// Collection of pools settings
        /// </summary>
        public List<PoolSettings> PoolCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        private PoolsSettings()
        {
            PoolCfgCollection = new List<PoolSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="poolCfgCollection">Pool settings collection</param>
        public PoolsSettings(IEnumerable<PoolSettings> poolCfgCollection)
            : this()
        {
            AddPools(poolCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="poolCfgCollection">Pool settings collection</param>
        public PoolsSettings(params PoolSettings[] poolCfgCollection)
            : this()
        {
            AddPools(poolCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public PoolsSettings(PoolsSettings source)
            : this()
        {
            AddPools(source.PoolCfgCollection);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings.</param>
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
        /// Total number of hidden neurons within the pools
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
        /// Adds cloned pool configurations from given collection into the internal collection
        /// </summary>
        /// <param name="poolCfgCollection"></param>
        private void AddPools(IEnumerable<PoolSettings> poolCfgCollection)
        {
            foreach (PoolSettings poolCfg in poolCfgCollection)
            {
                PoolCfgCollection.Add((PoolSettings)poolCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Returns ID (index) of the given pool
        /// </summary>
        /// <param name="poolName">Pool name</param>
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
        /// Returns configuration of the given pool
        /// </summary>
        /// <param name="poolName">Pool name</param>
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
