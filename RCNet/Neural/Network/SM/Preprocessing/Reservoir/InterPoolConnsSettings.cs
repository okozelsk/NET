using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
    /// <summary>
    /// The collection of the pools configurations
    /// </summary>
    [Serializable]
    public class InterPoolConnsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ResStructInterPoolConnectionsType";

        //Attribute properties
        /// <summary>
        /// Collection of inter-pool connections settings
        /// </summary>
        public List<InterPoolConnSettings> InterPoolConnectionCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        private InterPoolConnsSettings()
        {
            InterPoolConnectionCfgCollection = new List<InterPoolConnSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="interPoolConnectionCfgCollection">Collection of inter-pool connection settings</param>
        public InterPoolConnsSettings(IEnumerable<InterPoolConnSettings> interPoolConnectionCfgCollection)
            : this()
        {
            AddPools(interPoolConnectionCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="interPoolConnectionCfgCollection">Collection of inter-pool connections settings</param>
        public InterPoolConnsSettings(params InterPoolConnSettings[] interPoolConnectionCfgCollection)
            : this()
        {
            AddPools(interPoolConnectionCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public InterPoolConnsSettings(InterPoolConnsSettings source)
            : this()
        {
            AddPools(source.InterPoolConnectionCfgCollection);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings.</param>
        public InterPoolConnsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            InterPoolConnectionCfgCollection = new List<InterPoolConnSettings>();
            foreach (XElement poolElem in settingsElem.Elements("interPoolConnection"))
            {
                InterPoolConnectionCfgCollection.Add(new InterPoolConnSettings(poolElem));
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
            if (InterPoolConnectionCfgCollection.Count == 0)
            {
                throw new ArgumentException($"At least one inter-pool connection configuration must be specified.", "InterPoolConnectionCfgCollection");
            }
            return;
        }

        /// <summary>
        /// Adds cloned schemas from given collection into the internal collection
        /// </summary>
        /// <param name="interPoolConnectionCfgCollection">Collection of inter-pool connection settings</param>
        private void AddPools(IEnumerable<InterPoolConnSettings> interPoolConnectionCfgCollection)
        {
            foreach (InterPoolConnSettings interPoolConnectionCfg in interPoolConnectionCfgCollection)
            {
                InterPoolConnectionCfgCollection.Add((InterPoolConnSettings)interPoolConnectionCfg.DeepClone());
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new InterPoolConnsSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach (InterPoolConnSettings interPoolConnectionCfg in InterPoolConnectionCfgCollection)
            {
                rootElem.Add(interPoolConnectionCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("interPoolConnections", suppressDefaults);
        }

    }//InterPoolConnsSettings

}//Namespace
