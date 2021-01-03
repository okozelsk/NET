using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
    /// <summary>
    /// Configuration of the input fields connections.
    /// </summary>
    [Serializable]
    public class InputConnsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "NPResInstanceInputConnectionsType";

        //Attribute properties
        /// <summary>
        /// The collection of an input field connection configurations.
        /// </summary>
        public List<InputConnSettings> ConnCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        private InputConnsSettings()
        {
            ConnCfgCollection = new List<InputConnSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="connCfgCollection">The collection of an input field connection configurations.</param>
        public InputConnsSettings(IEnumerable<InputConnSettings> connCfgCollection)
            : this()
        {
            AddConnections(connCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="connCfgCollection">The input field connection configurations.</param>
        public InputConnsSettings(params InputConnSettings[] connCfgCollection)
            : this()
        {
            AddConnections(connCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public InputConnsSettings(InputConnsSettings source)
            : this()
        {
            AddConnections(source.ConnCfgCollection);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
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
        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            if (ConnCfgCollection.Count == 0)
            {
                throw new ArgumentException($"At least one connection configuration must be specified.", "ConnCfgCollection");
            }
            return;
        }

        /// <summary>
        /// Adds the input field connection configurations from the specified collection into the internal collection.
        /// </summary>
        /// <param name="connCfgCollection">The collection of an input field connection configurations.</param>
        private void AddConnections(IEnumerable<InputConnSettings> connCfgCollection)
        {
            foreach (InputConnSettings connCfg in connCfgCollection)
            {
                ConnCfgCollection.Add((InputConnSettings)connCfg.DeepClone());
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new InputConnsSettings(this);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("inputConnections", suppressDefaults);
        }

    }//InputConnsSettings

}//Namespace
