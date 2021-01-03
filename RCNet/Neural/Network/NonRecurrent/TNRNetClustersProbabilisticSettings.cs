using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Configuration of the collection of the Probabilistic cluster configurations.
    /// </summary>
    [Serializable]
    public class TNRNetClustersProbabilisticSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ProbabilisticTNetClustersType";

        //Attribute properties
        /// <summary>
        /// The collection of the cluster configurations.
        /// </summary>
        public List<ITNRNetClusterSettings> ClusterCfgCollection { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="clusterCfgCollection">The collection of the cluster configurations.</param>
        public TNRNetClustersProbabilisticSettings(IEnumerable<TNRNetClusterProbabilisticSettings> clusterCfgCollection)
        {
            ClusterCfgCollection = new List<ITNRNetClusterSettings>();
            foreach (TNRNetClusterProbabilisticSettings clusterCfg in clusterCfgCollection)
            {
                ClusterCfgCollection.Add((ITNRNetClusterSettings)clusterCfg.DeepClone());
            }
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="clusterCfgCollection">The cluster configurations.</param>
        public TNRNetClustersProbabilisticSettings(params TNRNetClusterProbabilisticSettings[] clusterCfgCollection)
            : this(clusterCfgCollection.AsEnumerable())
        {
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public TNRNetClustersProbabilisticSettings(TNRNetClustersProbabilisticSettings source)
            : this(from cfg in source.ClusterCfgCollection select (TNRNetClusterProbabilisticSettings)cfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public TNRNetClustersProbabilisticSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            ClusterCfgCollection = new List<ITNRNetClusterSettings>();
            foreach (XElement clusterElem in settingsElem.Elements("cluster"))
            {
                ClusterCfgCollection.Add(new TNRNetClusterProbabilisticSettings(clusterElem));
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
            if (ClusterCfgCollection.Count == 0)
            {
                throw new ArgumentException($"Collection of the cluster configurations can not be empty.", "ClusterCfgCollection");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new TNRNetClustersProbabilisticSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach (ITNRNetClusterSettings clusterCg in ClusterCfgCollection)
            {
                rootElem.Add(clusterCg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("clusters", suppressDefaults);
        }

    }//TNRNetClustersProbabilisticSettings

}//Namespace
