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
    /// Collection of reservoir structure settings
    /// </summary>
    [Serializable]
    public class ReservoirStructuresSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPResStructsType";

        //Attribute properties
        /// <summary>
        /// Collection of reservoir structure settings
        /// </summary>
        public List<ReservoirStructureSettings> ReservoirStructureCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance
        /// </summary>
        private ReservoirStructuresSettings()
        {
            ReservoirStructureCfgCollection = new List<ReservoirStructureSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="reservoirStructureCfgCollection">Reservoir structure settings collection</param>
        public ReservoirStructuresSettings(IEnumerable<ReservoirStructureSettings> reservoirStructureCfgCollection)
            : this()
        {
            AddReservoirStructures(reservoirStructureCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="reservoirStructureCfgCollection">Resrvoir structure settings collection</param>
        public ReservoirStructuresSettings(params ReservoirStructureSettings[] reservoirStructureCfgCollection)
            : this()
        {
            AddReservoirStructures(reservoirStructureCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ReservoirStructuresSettings(ReservoirStructuresSettings source)
            : this()
        {
            AddReservoirStructures(source.ReservoirStructureCfgCollection);
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing settings.</param>
        public ReservoirStructuresSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            ReservoirStructureCfgCollection = new List<ReservoirStructureSettings>();
            foreach (XElement reservoirStructureElem in settingsElem.Elements("reservoirStructure"))
            {
                ReservoirStructureCfgCollection.Add(new ReservoirStructureSettings(reservoirStructureElem));
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
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (ReservoirStructureCfgCollection.Count == 0)
            {
                throw new Exception($"At least one reservoir structure configuration must be specified.");
            }
            //Uniqueness
            string[] names = new string[ReservoirStructureCfgCollection.Count];
            names[0] = ReservoirStructureCfgCollection[0].Name;
            for (int i = 1; i < ReservoirStructureCfgCollection.Count; i++)
            {
                if (names.Contains(ReservoirStructureCfgCollection[i].Name))
                {
                    throw new Exception($"Reservoir structure name {ReservoirStructureCfgCollection[i].Name} is not unique.");
                }
                names[i] = ReservoirStructureCfgCollection[i].Name;
            }
            return;
        }

        /// <summary>
        /// Adds cloned reservoir structure configurations from given collection into the internal collection
        /// </summary>
        /// <param name="reservoirStructureCfgCollection">Collection of reservoir structure configurations</param>
        private void AddReservoirStructures(IEnumerable<ReservoirStructureSettings> reservoirStructureCfgCollection)
        {
            foreach (ReservoirStructureSettings reservoirStructureCfg in reservoirStructureCfgCollection)
            {
                ReservoirStructureCfgCollection.Add((ReservoirStructureSettings)reservoirStructureCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Returns ID (index) of the given reservoir structure
        /// </summary>
        /// <param name="reservoirStructureName">Reservoir structure name</param>
        public int GetReservoirStructureID(string reservoirStructureName)
        {
            for (int i = 0; i < ReservoirStructureCfgCollection.Count; i++)
            {
                if (ReservoirStructureCfgCollection[i].Name == reservoirStructureName)
                {
                    return i;
                }
            }
            throw new Exception($"Reservoir structure name {reservoirStructureName} not found.");
        }

        /// <summary>
        /// Returns configuration of the given resrvoir structure
        /// </summary>
        /// <param name="reservoirStructureName">Reservoir structure name</param>
        public ReservoirStructureSettings GetReservoirStructureCfg(string reservoirStructureName)
        {
            return ReservoirStructureCfgCollection[GetReservoirStructureID(reservoirStructureName)];
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new ReservoirStructuresSettings(this);
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
            foreach (ReservoirStructureSettings reservoirStructureCfg in ReservoirStructureCfgCollection)
            {
                rootElem.Add(reservoirStructureCfg.GetXml(suppressDefaults));
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
            return GetXml("reservoirStructures", suppressDefaults);
        }

    }//ReservoirStructuresSettings

}//Namespace
