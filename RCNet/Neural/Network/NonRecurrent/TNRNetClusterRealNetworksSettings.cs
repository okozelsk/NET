using RCNet.Neural.Network.NonRecurrent.FF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;


namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Configuration of the collection of the network configurations of the Real cluster.
    /// </summary>
    [Serializable]
    public class TNRNetClusterRealNetworksSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "RealTNetClusterNetworksType";

        //Attribute properties
        /// <summary>
        /// The collection of the network configurations.
        /// </summary>
        public List<INonRecurrentNetworkSettings> NetworkCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="networkCfgs">The collection of the network configurations.</param>
        public TNRNetClusterRealNetworksSettings(IEnumerable<INonRecurrentNetworkSettings> networkCfgs)
        {
            NetworkCfgCollection = new List<INonRecurrentNetworkSettings>();
            foreach (INonRecurrentNetworkSettings netCfg in networkCfgs)
            {
                NetworkCfgCollection.Add((INonRecurrentNetworkSettings)netCfg.DeepClone());
            }
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="networkCfgs">The network configurations.</param>
        public TNRNetClusterRealNetworksSettings(params INonRecurrentNetworkSettings[] networkCfgs)
            : this(networkCfgs.AsEnumerable())
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public TNRNetClusterRealNetworksSettings(TNRNetClusterRealNetworksSettings source)
            : this(source.NetworkCfgCollection)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public TNRNetClusterRealNetworksSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            NetworkCfgCollection = new List<INonRecurrentNetworkSettings>();
            foreach (XElement ffSettingsElem in settingsElem.Elements("ff"))
            {
                NetworkCfgCollection.Add(new FeedForwardNetworkSettings(ffSettingsElem));
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
            if (NetworkCfgCollection.Count == 0)
            {
                throw new ArgumentException($"There is no cluster network defined", "NetworkCfgCollection");
            }
            foreach (INonRecurrentNetworkSettings ffns in NetworkCfgCollection)
            {
                if (ffns.GetType() != typeof(FeedForwardNetworkSettings))
                {
                    throw new ArgumentException($"Network types can be only the Feed forward networks.", "NetworkCfgCollection");
                }
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new TNRNetClusterRealNetworksSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach (INonRecurrentNetworkSettings netCfg in NetworkCfgCollection)
            {
                rootElem.Add(netCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("networks", suppressDefaults);
        }

    }//TNRNetClusterRealNetworksSettings

}//Namespace
