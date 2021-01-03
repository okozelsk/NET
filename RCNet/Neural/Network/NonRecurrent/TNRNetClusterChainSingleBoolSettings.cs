using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Configuration of the SingleBool cluster chain.
    /// </summary>
    [Serializable]
    public class TNRNetClusterChainSingleBoolSettings : RCNetBaseSettings, ITNRNetClusterChainSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "SingleBoolTNetClusterChainType";

        //Attribute properties
        /// <summary>
        /// The crossvalidation configuration.
        /// </summary>
        public CrossvalidationSettings CrossvalidationCfg { get; }

        /// <summary>
        /// The configuration of the clusters in the chain.
        /// </summary>
        public TNRNetClustersSingleBoolSettings ClustersCfg { get; }


        //Constructors
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        /// <param name="crossvalidationCfg">The crossvalidation configuration.</param>
        /// <param name="clustersCfg">The configuration of the clusters in the chain.</param>
        public TNRNetClusterChainSingleBoolSettings(CrossvalidationSettings crossvalidationCfg,
                                                    TNRNetClustersSingleBoolSettings clustersCfg
                                                    )
        {
            CrossvalidationCfg = (CrossvalidationSettings)crossvalidationCfg.DeepClone();
            ClustersCfg = (TNRNetClustersSingleBoolSettings)clustersCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public TNRNetClusterChainSingleBoolSettings(TNRNetClusterChainSingleBoolSettings source)
            : this(source.CrossvalidationCfg, source.ClustersCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public TNRNetClusterChainSingleBoolSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            CrossvalidationCfg = new CrossvalidationSettings(settingsElem.Element("crossvalidation"));
            ClustersCfg = new TNRNetClustersSingleBoolSettings(settingsElem.Element("clusters"));
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public TNRNet.OutputType Output { get { return TNRNet.OutputType.SingleBool; } }

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
            return new TNRNetClusterChainSingleBoolSettings(this);
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

    }//TNRNetClusterChainSingleBoolSettings

}//Namespace
