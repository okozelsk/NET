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
    /// Collection of input connection settings
    /// </summary>
    [Serializable]
    public class InputConnsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPResInstanceInputConnectionsType";

        //Attribute properties
        /// <summary>
        /// Collection of connection settings
        /// </summary>
        public List<InputConnSettings> ConnCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        private InputConnsSettings()
        {
            ConnCfgCollection = new List<InputConnSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="connCfgCollection">Connection settings collection</param>
        public InputConnsSettings(IEnumerable<InputConnSettings> connCfgCollection)
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
        public InputConnsSettings(params InputConnSettings[] connCfgCollection)
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
        public InputConnsSettings(InputConnsSettings source)
            : this()
        {
            AddConnections(source.ConnCfgCollection);
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing settings.</param>
        public InputConnsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            ConnCfgCollection = new List<InputConnSettings>();
            foreach (XElement connElem in settingsElem.Elements("connection"))
            {
                ConnCfgCollection.Add(new InputConnSettings(connElem));
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
            if (ConnCfgCollection.Count == 0)
            {
                throw new ArgumentException($"At least one connection configuration must be specified.", "ConnCfgCollection");
            }
            return;
        }

        /// <summary>
        /// Adds cloned connection settings from given collection into the internal collection
        /// </summary>
        /// <param name="connCfgCollection">Connection settings collection</param>
        private void AddConnections(IEnumerable<InputConnSettings> connCfgCollection)
        {
            foreach (InputConnSettings connCfg in connCfgCollection)
            {
                ConnCfgCollection.Add((InputConnSettings)connCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new InputConnsSettings(this);
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
            foreach (InputConnSettings connCfg in ConnCfgCollection)
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
            return GetXml("inputConnections", suppressDefaults);
        }

    }//InputConnsSettings

}//Namespace
