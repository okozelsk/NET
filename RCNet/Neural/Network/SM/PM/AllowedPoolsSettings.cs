using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.PM
{
    /// <summary>
    /// The collection of predictors mapper's allowed pool configurations
    /// </summary>
    [Serializable]
    public class AllowedPoolsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "SMMapperAllowedPoolsType";

        //Attribute properties
        /// <summary>
        /// Collection of pools settings
        /// </summary>
        public List<AllowedPoolSettings> AllowedPoolCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        private AllowedPoolsSettings()
        {
            AllowedPoolCfgCollection = new List<AllowedPoolSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="allowedPoolCfgCollection">Allowed pool settings collection</param>
        public AllowedPoolsSettings(IEnumerable<AllowedPoolSettings> allowedPoolCfgCollection)
            : this()
        {
            AddAllowedPools(allowedPoolCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="allowedPoolCfgCollection">Allowed pool settings collection</param>
        public AllowedPoolsSettings(params AllowedPoolSettings[] allowedPoolCfgCollection)
            : this()
        {
            AddAllowedPools(allowedPoolCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public AllowedPoolsSettings(AllowedPoolsSettings source)
            : this()
        {
            AddAllowedPools(source.AllowedPoolCfgCollection);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings.</param>
        public AllowedPoolsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            AllowedPoolCfgCollection = new List<AllowedPoolSettings>();
            foreach (XElement poolElem in settingsElem.Elements("pool"))
            {
                AllowedPoolCfgCollection.Add(new AllowedPoolSettings(poolElem));
            }
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (AllowedPoolCfgCollection.Count == 0)
            {
                throw new ArgumentException($"At least one allowed pool configuration must be specified.", "AllowedPoolCfgCollection");
            }
            //Uniqueness of pool references
            string[] names = new string[AllowedPoolCfgCollection.Count];
            names[0] = AllowedPoolCfgCollection[0].ReservoirInstanceName + "." + AllowedPoolCfgCollection[0].PoolName;
            for (int i = 1; i < AllowedPoolCfgCollection.Count; i++)
            {
                string refName = AllowedPoolCfgCollection[i].ReservoirInstanceName + "." + AllowedPoolCfgCollection[i].PoolName;
                if (names.Contains(refName))
                {
                    throw new ArgumentException($"Pool reference {refName} is not unique.", "AllowedPoolCfgCollection");
                }
                names[i] = refName;
            }
            return;
        }

        /// <summary>
        /// Adds cloned allowed pool configurations from given collection into the internal collection
        /// </summary>
        /// <param name="allowedPoolCfgCollection">Allowed pool settings collection</param>
        private void AddAllowedPools(IEnumerable<AllowedPoolSettings> allowedPoolCfgCollection)
        {
            foreach (AllowedPoolSettings allowedPoolCfg in allowedPoolCfgCollection)
            {
                AllowedPoolCfgCollection.Add((AllowedPoolSettings)allowedPoolCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Check if specified pool is allowed
        /// </summary>
        /// <param name="reservoirInstanceName">Name of the reservoir instance</param>
        /// <param name="poolName">Name of the pool</param>
        public bool IsAllowed(string reservoirInstanceName, string poolName)
        {
            foreach (AllowedPoolSettings poolCfg in AllowedPoolCfgCollection)
            {
                if (poolCfg.ReservoirInstanceName == reservoirInstanceName && poolCfg.PoolName == poolName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new AllowedPoolsSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach (AllowedPoolSettings allowedPoolCfg in AllowedPoolCfgCollection)
            {
                rootElem.Add(allowedPoolCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("allowedPools", suppressDefaults);
        }

    }//AllowedPoolsSettings

}//Namespace
