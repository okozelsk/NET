using RCNet.Neural.Activation;
using RCNet.Neural.Network.NonRecurrent.FF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;


namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Configuration of the collection of the network configurations of the Probabilistic cluster.
    /// </summary>
    [Serializable]
    public class TNRNetClusterProbabilisticNetworksSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ProbabilisticTNetClusterNetworksType";

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
        public TNRNetClusterProbabilisticNetworksSettings(IEnumerable<INonRecurrentNetworkSettings> networkCfgs)
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
        public TNRNetClusterProbabilisticNetworksSettings(params INonRecurrentNetworkSettings[] networkCfgs)
            : this(networkCfgs.AsEnumerable())
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">The source instance.</param>
        public TNRNetClusterProbabilisticNetworksSettings(TNRNetClusterProbabilisticNetworksSettings source)
            : this(source.NetworkCfgCollection)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public TNRNetClusterProbabilisticNetworksSettings(XElement elem)
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
                if (((FeedForwardNetworkSettings)ffns).OutputActivationCfg.GetType() != typeof(AFAnalogSoftMaxSettings))
                {
                    throw new ArgumentException($"At least one of cluster networks does not have the SoftMax output activation.", "NetworkCfgCollection");
                }
                if (((FeedForwardNetworkSettings)ffns).TrainerCfg.GetType() != typeof(RPropTrainerSettings))
                {
                    throw new ArgumentException($"At least one of cluster networks does not have the RProp trainer.", "NetworkCfgCollection");
                }
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new TNRNetClusterProbabilisticNetworksSettings(this);
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

    }//TNRNetClusterProbabilisticNetworksSettings

}//Namespace
