using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.PM
{
    /// <summary>
    /// Configuration of the predictors mapper.
    /// </summary>
    [Serializable]
    public class MapperSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "SMMapperType";

        //Attribute properties
        /// <summary>
        /// The collection of the readout unit mapping configurations.
        /// </summary>
        public List<ReadoutUnitMapSettings> MapCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        private MapperSettings()
        {
            MapCfgCollection = new List<ReadoutUnitMapSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="mapCfgCollection">The collection of the readout unit mapping configurations.</param>
        public MapperSettings(IEnumerable<ReadoutUnitMapSettings> mapCfgCollection)
            : this()
        {
            AddMaps(mapCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="mapCfgCollection">The readout unit mapping configurations.</param>
        public MapperSettings(params ReadoutUnitMapSettings[] mapCfgCollection)
            : this()
        {
            AddMaps(mapCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">The source instance.</param>
        public MapperSettings(MapperSettings source)
            : this()
        {
            AddMaps(source.MapCfgCollection);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public MapperSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            MapCfgCollection = new List<ReadoutUnitMapSettings>();
            foreach (XElement mapElem in settingsElem.Elements("map"))
            {
                MapCfgCollection.Add(new ReadoutUnitMapSettings(mapElem));
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
            if (MapCfgCollection.Count == 0)
            {
                throw new ArgumentException($"At least one readout unit map configuration must be specified.", "MapCfgCollection");
            }
            //Uniqueness of readout unit name
            string[] names = new string[MapCfgCollection.Count];
            names[0] = MapCfgCollection[0].ReadoutUnitName;
            for (int i = 1; i < MapCfgCollection.Count; i++)
            {
                if (names.Contains(MapCfgCollection[i].ReadoutUnitName))
                {
                    throw new ArgumentException($"Readout unit name {MapCfgCollection[i].ReadoutUnitName} is not unique.", "MapCfgCollection");
                }
                names[i] = MapCfgCollection[i].ReadoutUnitName;
            }
            return;
        }

        /// <summary>
        /// Adds the readout unit mapping configurations from the specified collection into the internal collection.
        /// </summary>
        /// <param name="mapCfgCollection">The collection of the readout unit mapping configurations.</param>
        private void AddMaps(IEnumerable<ReadoutUnitMapSettings> mapCfgCollection)
        {
            foreach (ReadoutUnitMapSettings mapCfg in mapCfgCollection)
            {
                MapCfgCollection.Add((ReadoutUnitMapSettings)mapCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Gets the index of the readout unit mapping.
        /// </summary>
        /// <param name="readoutUnitName">The name of the readout unit.</param>
        /// <param name="ex">Specifies whether to throw exception or return -1 in case the map for given readout unit name not found.</param>
        public int GetMapID(string readoutUnitName, bool ex = false)
        {
            for (int i = 0; i < MapCfgCollection.Count; i++)
            {
                if (MapCfgCollection[i].ReadoutUnitName == readoutUnitName)
                {
                    return i;
                }
            }
            if (ex)
            {
                throw new InvalidOperationException($"Readout unit name {readoutUnitName} not found.");
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Gets the configuration of the readout unit mapping.
        /// </summary>
        /// <param name="readoutUnitName">The name of the readout unit.</param>
        /// <param name="ex">Specifies whether to throw exception or return -1 in case the map for given readout unit name not found.</param>
        public ReadoutUnitMapSettings GetMapCfg(string readoutUnitName, bool ex = false)
        {
            int mapID = GetMapID(readoutUnitName, ex);
            return mapID == -1 ? null : MapCfgCollection[mapID];
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new MapperSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach (ReadoutUnitMapSettings mapCfg in MapCfgCollection)
            {
                rootElem.Add(mapCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("mapper", suppressDefaults);
        }

    }//MapperSettings

}//Namespace
