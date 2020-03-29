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

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
    /// <summary>
    /// Collection of reservoir instance settings
    /// </summary>
    [Serializable]
    public class ReservoirInstancesSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPResInstancesType";

        //Attribute properties
        /// <summary>
        /// Collection of reservoir instance settings
        /// </summary>
        public List<ReservoirInstanceSettings> ReservoirInstanceCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        private ReservoirInstancesSettings()
        {
            ReservoirInstanceCfgCollection = new List<ReservoirInstanceSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="reservoirInstanceCfgCollection">Reservoir instance settings collection</param>
        public ReservoirInstancesSettings(IEnumerable<ReservoirInstanceSettings> reservoirInstanceCfgCollection)
            : this()
        {
            AddReservoirInstances(reservoirInstanceCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="reservoirInstanceCfgCollection">Resrvoir instance settings collection</param>
        public ReservoirInstancesSettings(params ReservoirInstanceSettings[] reservoirInstanceCfgCollection)
            : this()
        {
            AddReservoirInstances(reservoirInstanceCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ReservoirInstancesSettings(ReservoirInstancesSettings source)
            : this()
        {
            AddReservoirInstances(source.ReservoirInstanceCfgCollection);
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing settings.</param>
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
            if (ReservoirInstanceCfgCollection.Count == 0)
            {
                throw new Exception($"At least one reservoir instance configuration must be specified.");
            }
            //Uniqueness
            string[] names = new string[ReservoirInstanceCfgCollection.Count];
            names[0] = ReservoirInstanceCfgCollection[0].Name;
            for (int i = 1; i < ReservoirInstanceCfgCollection.Count; i++)
            {
                if (names.Contains(ReservoirInstanceCfgCollection[i].Name))
                {
                    throw new Exception($"Reservoir instance name {ReservoirInstanceCfgCollection[i].Name} is not unique.");
                }
                names[i] = ReservoirInstanceCfgCollection[i].Name;
            }
            return;
        }

        /// <summary>
        /// Adds cloned reservoir instance configurations from given collection into the internal collection
        /// </summary>
        /// <param name="reservoirInstanceCfgCollection">Collection of reservoir instance configurations</param>
        private void AddReservoirInstances(IEnumerable<ReservoirInstanceSettings> reservoirInstanceCfgCollection)
        {
            foreach (ReservoirInstanceSettings reservoirInstanceCfg in reservoirInstanceCfgCollection)
            {
                ReservoirInstanceCfgCollection.Add((ReservoirInstanceSettings)reservoirInstanceCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Returns ID (index) of the given reservoir instance
        /// </summary>
        /// <param name="reservoirInstanceName">Reservoir instance name</param>
        public int GetReservoirInstanceID(string reservoirInstanceName)
        {
            for (int i = 0; i < ReservoirInstanceCfgCollection.Count; i++)
            {
                if (ReservoirInstanceCfgCollection[i].Name == reservoirInstanceName)
                {
                    return i;
                }
            }
            throw new Exception($"Reservoir instance name {reservoirInstanceName} not found.");
        }

        /// <summary>
        /// Returns configuration of the given reservoir instance
        /// </summary>
        /// <param name="reservoirInstanceName">Reservoir instance name</param>
        public ReservoirInstanceSettings GetReservoirInstanceCfg(string reservoirInstanceName)
        {
            return ReservoirInstanceCfgCollection[GetReservoirInstanceID(reservoirInstanceName)];
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new ReservoirInstancesSettings(this);
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
            foreach (ReservoirInstanceSettings reservoirInstanceCfg in ReservoirInstanceCfgCollection)
            {
                rootElem.Add(reservoirInstanceCfg.GetXml(suppressDefaults));
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
            return GetXml("reservoirInstances", suppressDefaults);
        }

    }//ReservoirInstancesSettings

}//Namespace
