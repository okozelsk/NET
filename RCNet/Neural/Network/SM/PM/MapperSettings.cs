using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.PM
{
    /// <summary>
    /// Collection of predictors mapper
    /// </summary>
    [Serializable]
    public class MapperSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "SMMapperType";

        //Attribute properties
        /// <summary>
        /// Collection of readout unit map configurations
        /// </summary>
        public List<ReadoutUnitMapSettings> MapCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        private MapperSettings()
        {
            MapCfgCollection = new List<ReadoutUnitMapSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="mapCfgCollection">Collection of readout unit map configurations</param>
        public MapperSettings(IEnumerable<ReadoutUnitMapSettings> mapCfgCollection)
            : this()
        {
            AddMaps(mapCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="mapCfgCollection">Collection of readout unit map configurations</param>
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
        /// <param name="source">Source instance</param>
        public MapperSettings(MapperSettings source)
            : this()
        {
            AddMaps(source.MapCfgCollection);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings.</param>
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
        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
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
        /// Adds cloned readout unit map configurations from given collection into the internal collection
        /// </summary>
        /// <param name="mapCfgCollection">Collection of readout unit map configurations</param>
        private void AddMaps(IEnumerable<ReadoutUnitMapSettings> mapCfgCollection)
        {
            foreach (ReadoutUnitMapSettings mapCfg in mapCfgCollection)
            {
                MapCfgCollection.Add((ReadoutUnitMapSettings)mapCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Returns ID (index) of the map for given readout unit or -1 if the map not found (ex = false)
        /// </summary>
        /// <param name="readoutUnitName">Readout unit name</param>
        /// <param name="ex">Specifies whether to throw exception or return -1 if the map for given readout unit name not found</param>
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
        /// Returns configuration of the given pool
        /// </summary>
        /// <param name="readoutUnitName">Readout unit name</param>
        /// <param name="ex">Specifies whether to throw exception or return -1 if the map for given readout unit name not found</param>
        public ReadoutUnitMapSettings GetMapCfg(string readoutUnitName, bool ex = false)
        {
            int mapID = GetMapID(readoutUnitName, ex);
            return mapID == -1 ? null : MapCfgCollection[mapID];
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new MapperSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
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

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("mapper", suppressDefaults);
        }

    }//MapperSettings

}//Namespace
