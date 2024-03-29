﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
    /// <summary>
    /// Configuration of the reservoir instances.
    /// </summary>
    [Serializable]
    public class ReservoirInstancesSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "NPResInstancesType";

        //Attribute properties
        /// <summary>
        /// The collection of the reservoir instance configurations.
        /// </summary>
        public List<ReservoirInstanceSettings> ReservoirInstanceCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        private ReservoirInstancesSettings()
        {
            ReservoirInstanceCfgCollection = new List<ReservoirInstanceSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="reservoirInstanceCfgCollection">The collection of the reservoir instance configurations.</param>
        public ReservoirInstancesSettings(IEnumerable<ReservoirInstanceSettings> reservoirInstanceCfgCollection)
            : this()
        {
            AddReservoirInstances(reservoirInstanceCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="reservoirInstanceCfgCollection">The reservoir instance configurations.</param>
        public ReservoirInstancesSettings(params ReservoirInstanceSettings[] reservoirInstanceCfgCollection)
            : this()
        {
            AddReservoirInstances(reservoirInstanceCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public ReservoirInstancesSettings(ReservoirInstancesSettings source)
            : this()
        {
            AddReservoirInstances(source.ReservoirInstanceCfgCollection);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public ReservoirInstancesSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            ReservoirInstanceCfgCollection = new List<ReservoirInstanceSettings>();
            foreach (XElement reservoirInstanceElem in settingsElem.Elements("reservoirInstance"))
            {
                ReservoirInstanceCfgCollection.Add(new ReservoirInstanceSettings(reservoirInstanceElem));
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
            if (ReservoirInstanceCfgCollection.Count == 0)
            {
                throw new ArgumentException($"At least one reservoir instance configuration must be specified.", "ReservoirInstanceCfgCollection");
            }
            //Uniqueness
            string[] names = new string[ReservoirInstanceCfgCollection.Count];
            names[0] = ReservoirInstanceCfgCollection[0].Name;
            for (int i = 1; i < ReservoirInstanceCfgCollection.Count; i++)
            {
                if (names.Contains(ReservoirInstanceCfgCollection[i].Name))
                {
                    throw new ArgumentException($"Reservoir instance name {ReservoirInstanceCfgCollection[i].Name} is not unique.", "ReservoirInstanceCfgCollection");
                }
                names[i] = ReservoirInstanceCfgCollection[i].Name;
            }
            return;
        }

        /// <summary>
        /// Adds the reservoir instance configurations from the specified collection into the internal collection.
        /// </summary>
        /// <param name="reservoirInstanceCfgCollection">The collection of the reservoir instance configurations.</param>
        private void AddReservoirInstances(IEnumerable<ReservoirInstanceSettings> reservoirInstanceCfgCollection)
        {
            foreach (ReservoirInstanceSettings reservoirInstanceCfg in reservoirInstanceCfgCollection)
            {
                ReservoirInstanceCfgCollection.Add((ReservoirInstanceSettings)reservoirInstanceCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Gets an identifier (index) of the specified reservoir instance.
        /// </summary>
        /// <param name="reservoirInstanceName">The name of the reservoir instance.</param>
        public int GetReservoirInstanceID(string reservoirInstanceName)
        {
            for (int i = 0; i < ReservoirInstanceCfgCollection.Count; i++)
            {
                if (ReservoirInstanceCfgCollection[i].Name == reservoirInstanceName)
                {
                    return i;
                }
            }
            throw new InvalidOperationException($"Reservoir instance name {reservoirInstanceName} not found.");
        }

        /// <summary>
        /// Gets the configuration of the specified reservoir instance.
        /// </summary>
        /// <param name="reservoirInstanceName">The name of the reservoir instance.</param>
        public ReservoirInstanceSettings GetReservoirInstanceCfg(string reservoirInstanceName)
        {
            return ReservoirInstanceCfgCollection[GetReservoirInstanceID(reservoirInstanceName)];
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new ReservoirInstancesSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach (ReservoirInstanceSettings reservoirInstanceCfg in ReservoirInstanceCfgCollection)
            {
                rootElem.Add(reservoirInstanceCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("reservoirInstances", suppressDefaults);
        }

    }//ReservoirInstancesSettings

}//Namespace
