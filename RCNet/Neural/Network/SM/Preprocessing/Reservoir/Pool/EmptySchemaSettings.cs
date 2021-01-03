using System;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool
{
    /// <summary>
    /// Configuration of the Empty interconnection schema of the pool's neurons.
    /// </summary>
    [Serializable]
    public class EmptySchemaSettings : RCNetBaseSettings, IInterconnSchemaSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "PoolInterconnectionEmptySchemaType";

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public EmptySchemaSettings()
        {
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public EmptySchemaSettings(EmptySchemaSettings source)
            : this()
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public EmptySchemaSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public bool ReplaceExistingConnections { get { return false; } }

        /// <inheritdoc/>
        public int Repetitions { get { return 1; } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return true;
            }
        }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new EmptySchemaSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("emptySchema", suppressDefaults);
        }

    }//EmptySchemaSettings

}//Namespace
