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
    /// Collection of input unit connection settings
    /// </summary>
    [Serializable]
    public class InputUnitConnsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPResInstanceInputUnitConnectionsType";

        //Attribute properties
        /// <summary>
        /// Collection of connection settings
        /// </summary>
        public List<InputUnitConnSettings> ConnCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        private InputUnitConnsSettings()
        {
            ConnCfgCollection = new List<InputUnitConnSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="connCfgCollection">Connection settings collection</param>
        public InputUnitConnsSettings(IEnumerable<InputUnitConnSettings> connCfgCollection)
            : this()
        {
            AddConnections(connCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="connCfgCollection">Connection settings collection</param>
        public InputUnitConnsSettings(params InputUnitConnSettings[] connCfgCollection)
            : this()
        {
            AddConnections(connCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public InputUnitConnsSettings(InputUnitConnsSettings source)
            : this()
        {
            AddConnections(source.ConnCfgCollection);
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing settings.</param>
        public InputUnitConnsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            ConnCfgCollection = new List<InputUnitConnSettings>();
            foreach (XElement connElem in settingsElem.Descendants("connection"))
            {
                ConnCfgCollection.Add(new InputUnitConnSettings(connElem));
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
            if (ConnCfgCollection.Count == 0)
            {
                throw new Exception($"At least one connection configuration must be specified.");
            }
            return;
        }

        /// <summary>
        /// Adds cloned connection settings from given collection into the internal collection
        /// </summary>
        /// <param name="connCfgCollection">Connection settings collection</param>
        private void AddConnections(IEnumerable<InputUnitConnSettings> connCfgCollection)
        {
            foreach (InputUnitConnSettings connCfg in connCfgCollection)
            {
                ConnCfgCollection.Add((InputUnitConnSettings)connCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new InputUnitConnsSettings(this);
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
            foreach (InputUnitConnSettings connCfg in ConnCfgCollection)
            {
                rootElem.Add(connCfg.GetXml(suppressDefaults));
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
            return GetXml("connections", suppressDefaults);
        }

    }//InputUnitConnsSettings

}//Namespace
