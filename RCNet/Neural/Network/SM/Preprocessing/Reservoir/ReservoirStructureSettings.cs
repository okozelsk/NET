using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.IO;
using System.Xml.XPath;
using RCNet.Extensions;
using RCNet.XmlTools;
using RCNet.RandomValue;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
    /// <summary>
    /// Contains reservoir structure settings
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
            :this(source.Name, source.PoolsCfg, source.InterPoolConnectionsCfg)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing reservoir settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public ReservoirStructureSettings(XElement elem)
        {
            //Validation
            XElement reservoirSettingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Name = reservoirSettingsElem.Attribute("name").Value;
            //Pool settings collection
            PoolsCfg = new PoolsSettings(reservoirSettingsElem.Descendants("pools").First());
            //Inter pool connections
            InterPoolConnectionsCfg = null;
            XElement interPoolConnectionsElem = reservoirSettingsElem.Descendants("interPoolConnections").FirstOrDefault();
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
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (Name.Length == 0)
            {
                throw new Exception($"Name can not be empty.");
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
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new ReservoirStructureSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
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

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("reservoirStructure", suppressDefaults);
        }




    }//ReservoirStructureSettings

}//Namespace

