using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Configuration of the Real cluster chain.
    /// </summary>
    [Serializable]
    public class TNRNetClusterChainRealSettings : RCNetBaseSettings, ITNRNetClusterChainSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "RealTNetClusterChainType";

        //Attribute properties
        /// <summary>
        /// The crossvalidation configuration.
        /// </summary>
        public CrossvalidationSettings CrossvalidationCfg { get; }

        /// <summary>
        /// The configuration of the clusters in the chain.
        /// </summary>
        public TNRNetClustersRealSettings ClustersCfg { get; }


        //Constructors
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        /// <param name="crossvalidationCfg">The crossvalidation configuration.</param>
        /// <param name="clustersCfg">The configuration of the clusters in the chain.</param>
        public TNRNetClusterChainRealSettings(CrossvalidationSettings crossvalidationCfg,
                                              TNRNetClustersRealSettings clustersCfg
                                              )
        {
            CrossvalidationCfg = (CrossvalidationSettings)crossvalidationCfg.DeepClone();
            ClustersCfg = (TNRNetClustersRealSettings)clustersCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public TNRNetClusterChainRealSettings(TNRNetClusterChainRealSettings source)
            : this(source.CrossvalidationCfg, source.ClustersCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public TNRNetClusterChainRealSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            CrossvalidationCfg = new CrossvalidationSettings(settingsElem.Element("crossvalidation"));
            ClustersCfg = new TNRNetClustersRealSettings(settingsElem.Element("clusters"));
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public TNRNet.OutputType Output { get { return TNRNet.OutputType.Real; } }

        /// <inheritdoc/>
        public List<ITNRNetClusterSettings> ClusterCfgCollection { get { return ClustersCfg.ClusterCfgCollection; } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new TNRNetClusterChainRealSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             CrossvalidationCfg.GetXml(suppressDefaults),
                                             ClustersCfg.GetXml(suppressDefaults)
                                             );
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("clusterChain", suppressDefaults);
        }

    }//TNRNetClusterChainRealSettings

}//Namespace
