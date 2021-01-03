using System;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.PM
{
    /// <summary>
    /// Configuration of the predictors mapper's allowed pool.
    /// </summary>
    [Serializable]
    public class AllowedPoolSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "SMMapperAllowedPoolType";

        //Attribute properties
        /// <summary>
        /// The name of the reservoir instance.
        /// </summary>
        public string ReservoirInstanceName { get; }

        /// <summary>
        /// The name of the pool.
        /// </summary>
        public string PoolName { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="reservoirInstanceName">The name of the reservoir instance.</param>
        /// <param name="poolName">The name of the pool.</param>
        public AllowedPoolSettings(string reservoirInstanceName, string poolName)
        {
            ReservoirInstanceName = reservoirInstanceName;
            PoolName = poolName;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">The source instance.</param>
        public AllowedPoolSettings(AllowedPoolSettings source)
            : this(source.ReservoirInstanceName, source.PoolName)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public AllowedPoolSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            ReservoirInstanceName = settingsElem.Attribute("reservoirInstanceName").Value;
            PoolName = settingsElem.Attribute("poolName").Value;
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return false;
            }
        }


        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (ReservoirInstanceName.Length == 0)
            {
                throw new ArgumentException($"ReservoirInstanceName can not be empty.", "ReservoirInstanceName");
            }
            if (PoolName.Length == 0)
            {
                throw new ArgumentException($"PoolName can not be empty.", "PoolName");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new AllowedPoolSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("reservoirInstanceName", ReservoirInstanceName),
                                             new XAttribute("poolName", PoolName)
                                             );
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("pool", suppressDefaults);
        }


    }//AllowedPoolSettings

}//Namespace
