using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
    /// <summary>
    /// The collection of the reservoir structures configurations
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
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings.</param>
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
        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            if (ReservoirStructureCfgCollection.Count == 0)
            {
                throw new ArgumentException($"At least one reservoir structure configuration must be specified.", "ReservoirStructureCfgCollection");
            }
            //Uniqueness
            string[] names = new string[ReservoirStructureCfgCollection.Count];
            names[0] = ReservoirStructureCfgCollection[0].Name;
            for (int i = 1; i < ReservoirStructureCfgCollection.Count; i++)
            {
                if (names.Contains(ReservoirStructureCfgCollection[i].Name))
                {
                    throw new ArgumentException($"Reservoir structure name {ReservoirStructureCfgCollection[i].Name} is not unique.", "ReservoirStructureCfgCollection");
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
            throw new InvalidOperationException($"Reservoir structure name {reservoirStructureName} not found.");
        }

        /// <summary>
        /// Returns configuration of the given resrvoir structure
        /// </summary>
        /// <param name="reservoirStructureName">Reservoir structure name</param>
        public ReservoirStructureSettings GetReservoirStructureCfg(string reservoirStructureName)
        {
            return ReservoirStructureCfgCollection[GetReservoirStructureID(reservoirStructureName)];
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new ReservoirStructuresSettings(this);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("reservoirStructures", suppressDefaults);
        }

    }//ReservoirStructuresSettings

}//Namespace
