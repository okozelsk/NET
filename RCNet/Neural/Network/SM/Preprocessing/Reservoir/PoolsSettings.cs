using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using RCNet.Extensions;
using RCNet.MathTools.Probability;
using RCNet.XmlTools;
using RCNet.RandomValue;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
    /// <summary>
    /// Collection of pool settings
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
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing settings.</param>
        public PoolsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            PoolCfgCollection = new List<PoolSettings>();
            foreach (XElement poolElem in settingsElem.Descendants("pool"))
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

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (PoolCfgCollection.Count == 0)
            {
                throw new Exception($"At least one pool configuration must be specified.");
            }
            //Uniqueness of pool names
            string[] names = new string[PoolCfgCollection.Count];
            names[0] = PoolCfgCollection[0].Name;
            for(int i = 1; i < PoolCfgCollection.Count; i++)
            {
                if(names.Contains(PoolCfgCollection[i].Name))
                {
                    throw new Exception($"Pool name {PoolCfgCollection[i].Name} is not unique.");
                }
                names[i] = PoolCfgCollection[i].Name;
            }
            return;
        }

        /// <summary>
        /// Adds cloned schemas from given collection into the internal collection
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
            for(int i = 0; i < PoolCfgCollection.Count; i++)
            {
                if(PoolCfgCollection[i].Name == poolName)
                {
                    return i;
                }
            }
            throw new Exception($"Pool name {poolName} not found.");
        }

        /// <summary>
        /// Returns configuration of the given pool
        /// </summary>
        /// <param name="poolName">Pool name</param>
        public PoolSettings GetPoolCfg(string poolName)
        {
            return PoolCfgCollection[GetPoolID(poolName)];
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new PoolsSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
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

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("pools", suppressDefaults);
        }

    }//ReservoirStructurePoolsSettings

}//Namespace
