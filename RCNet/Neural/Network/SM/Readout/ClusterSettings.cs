using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using RCNet.Neural.Network.NonRecurrent;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Configuration of the cluster associated with the readout unit
    /// </summary>
    [Serializable]
    public class ClusterSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ROutLayerClusterType";

        //Attribute properties
        /// <summary>
        /// Crossvalidation configuration
        /// </summary>
        public CrossvalidationSettings CrossvalidationCfg { get; }

        /// <summary>
        /// Task dependent networks settings to be applied when specific networks for readout unit are not specified
        /// </summary>
        public DefaultNetworksSettings DefaultNetworksCfg { get; }

        /// <summary>
        /// Configuration of the network cluster 2nd level computation
        /// </summary>
        public NetworkClusterSecondLevelCompSettings SecondLevelCompCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="crossvalidationCfg">Crossvalidation configuration</param>
        /// <param name="defaultNetworksCfg">Task dependent networks settings to be applied when specific networks for readout unit are not specified</param>
        /// <param name="secondLevelCompCfg">Configuration of the network cluster 2nd level computation</param>
        public ClusterSettings(CrossvalidationSettings crossvalidationCfg,
                               DefaultNetworksSettings defaultNetworksCfg = null,
                               NetworkClusterSecondLevelCompSettings secondLevelCompCfg  = null
                               )
        {
            CrossvalidationCfg = (CrossvalidationSettings)crossvalidationCfg.DeepClone();
            DefaultNetworksCfg = defaultNetworksCfg == null ? new DefaultNetworksSettings() : (DefaultNetworksSettings)defaultNetworksCfg.DeepClone();
            SecondLevelCompCfg = (NetworkClusterSecondLevelCompSettings)secondLevelCompCfg?.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ClusterSettings(ClusterSettings source)
            : this(source.CrossvalidationCfg, source.DefaultNetworksCfg, source.SecondLevelCompCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public ClusterSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            //Crossvalidation
            CrossvalidationCfg = new CrossvalidationSettings(settingsElem.Element("crossvalidation"));
            //Default networks settings
            XElement defaultNetworksElem = settingsElem.Elements("defaultNetworks").FirstOrDefault();
            DefaultNetworksCfg = defaultNetworksElem == null ? new DefaultNetworksSettings() : new DefaultNetworksSettings(defaultNetworksElem);
            //Second level computation
            XElement clusterSecondLevelCompElem = settingsElem.Elements("secondLevelComputation").FirstOrDefault();
            if (clusterSecondLevelCompElem != null)
            {
                SecondLevelCompCfg = new NetworkClusterSecondLevelCompSettings(clusterSecondLevelCompElem);
            }
            else
            {
                SecondLevelCompCfg = null;
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
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new ClusterSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName, CrossvalidationCfg.GetXml(suppressDefaults));
            if (!DefaultNetworksCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(DefaultNetworksCfg.GetXml(suppressDefaults));
            }
            if (SecondLevelCompCfg != null)
            {
                rootElem.Add(SecondLevelCompCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("cluster", suppressDefaults);
        }


    }//ClusterSettings

}//Namespace
