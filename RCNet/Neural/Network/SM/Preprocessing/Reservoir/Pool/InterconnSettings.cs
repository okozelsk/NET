using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool
{
    /// <summary>
    /// The collection of pool's neurons interconnection schemas configurations
    /// </summary>
    [Serializable]
    public class InterconnSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PoolInterconnectionType";

        //Attribute properties
        /// <summary>
        /// Collection of interconnection schemas to be applied
        /// </summary>
        public List<IInterconnSchemaSettings> SchemaCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        private InterconnSettings()
        {
            SchemaCfgCollection = new List<IInterconnSchemaSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="schemaCfgCollection">Schema settings</param>
        public InterconnSettings(IEnumerable<IInterconnSchemaSettings> schemaCfgCollection)
            : this()
        {
            AddSchemas(schemaCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="schemaCfgCollection">Schema settings</param>
        public InterconnSettings(params IInterconnSchemaSettings[] schemaCfgCollection)
            : this()
        {
            AddSchemas(schemaCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public InterconnSettings(InterconnSettings source)
            : this()
        {
            AddSchemas(source.SchemaCfgCollection);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings.</param>
        public InterconnSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            SchemaCfgCollection = new List<IInterconnSchemaSettings>();
            foreach (XElement schemaElem in settingsElem.Elements())
            {
                if (schemaElem.Name.LocalName == "randomSchema")
                {
                    SchemaCfgCollection.Add(new RandomSchemaSettings(schemaElem));
                }
                else if (schemaElem.Name.LocalName == "chainSchema")
                {
                    SchemaCfgCollection.Add(new ChainSchemaSettings(schemaElem));
                }
                else if (schemaElem.Name.LocalName == "doubleTwistedToroidSchema")
                {
                    SchemaCfgCollection.Add(new DoubleTwistedToroidSchemaSettings(schemaElem));
                }
                else if (schemaElem.Name.LocalName == "emptySchema")
                {
                    SchemaCfgCollection.Add(new EmptySchemaSettings(schemaElem));
                }
                else
                {
                    //Ignore
                    ;
                }
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
            if (SchemaCfgCollection.Count == 0)
            {
                throw new InvalidOperationException($"At least one interconnection schema must be specified.");
            }
            foreach(IInterconnSchemaSettings schema in SchemaCfgCollection)
            {
                if(schema.GetType() == typeof(EmptySchemaSettings) && SchemaCfgCollection.Count > 1)
                {
                    throw new InvalidOperationException($"No other schema specification is allowed together with EmptySchema.");
                }
            }
            return;
        }

        /// <summary>
        /// Adds cloned schemas from given collection into the internal collection
        /// </summary>
        /// <param name="schemaCfgCollection"></param>
        private void AddSchemas(IEnumerable<IInterconnSchemaSettings> schemaCfgCollection)
        {
            foreach (IInterconnSchemaSettings schemaCfg in schemaCfgCollection)
            {
                SchemaCfgCollection.Add((IInterconnSchemaSettings)schemaCfg.DeepClone());
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new InterconnSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach (IInterconnSchemaSettings schemaCfg in SchemaCfgCollection)
            {
                rootElem.Add(schemaCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("interconnection", suppressDefaults);
        }

    }//InterconnSettings

}//Namespace
