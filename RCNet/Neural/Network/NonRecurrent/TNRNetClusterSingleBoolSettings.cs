using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Configuration of the SingleBool network cluster.
    /// </summary>
    [Serializable]
    public class TNRNetClusterSingleBoolSettings : RCNetBaseSettings, ITNRNetClusterSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "SingleBoolTNetClusterType";

        //Attribute properties
        /// <summary>
        /// The configuration of the cluster networks.
        /// </summary>
        public TNRNetClusterSingleBoolNetworksSettings NetworksCfg { get; }

        /// <summary>
        /// The configuration of the macro-weights.
        /// </summary>
        public TNRNetClusterSingleBoolWeightsSettings WeightsCfg { get; }


        //Constructors
        /// <summary>
        /// Creates an unitialized instance.
        /// </summary>
        /// <param name="networksCfg">The configuration of the cluster networks.</param>
        /// <param name="weightsCfg">The configuration of the macro-weights.</param>
        public TNRNetClusterSingleBoolSettings(TNRNetClusterSingleBoolNetworksSettings networksCfg,
                                               TNRNetClusterSingleBoolWeightsSettings weightsCfg
                                               )
        {
            NetworksCfg = (TNRNetClusterSingleBoolNetworksSettings)networksCfg.DeepClone();
            WeightsCfg = (TNRNetClusterSingleBoolWeightsSettings)weightsCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public TNRNetClusterSingleBoolSettings(TNRNetClusterSingleBoolSettings source)
            : this(source.NetworksCfg, source.WeightsCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public TNRNetClusterSingleBoolSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            NetworksCfg = new TNRNetClusterSingleBoolNetworksSettings(settingsElem.Element("networks"));
            WeightsCfg = new TNRNetClusterSingleBoolWeightsSettings(settingsElem.Element("weights"));
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public TNRNet.OutputType Output { get { return TNRNet.OutputType.SingleBool; } }

        /// <inheritdoc/>
        public List<INonRecurrentNetworkSettings> ClusterNetConfigurations { get { return NetworksCfg.NetworkCfgCollection; } }

        /// <inheritdoc/>
        public double TrainingGroupWeight { get { return WeightsCfg.TrainingGroupWeight; } }

        /// <inheritdoc/>
        public double TestingGroupWeight { get { return WeightsCfg.TestingGroupWeight; } }

        /// <inheritdoc/>
        public double SamplesWeight { get { return WeightsCfg.SamplesWeight; } }

        /// <inheritdoc/>
        public double NumericalPrecisionWeight { get { return WeightsCfg.NumericalPrecisionWeight; } }

        /// <inheritdoc/>
        public double MisrecognizedFalseWeight { get { return WeightsCfg.MisrecognizedFalseWeight; } }

        /// <inheritdoc/>
        public double UnrecognizedTrueWeight { get { return WeightsCfg.UnrecognizedTrueWeight; } }

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
            return new TNRNetClusterSingleBoolSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             NetworksCfg.GetXml(suppressDefaults),
                                             WeightsCfg.GetXml(suppressDefaults)
                                             );
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("cluster", suppressDefaults);
        }

    }//TNRNetClusterSingleBoolSettings

}//Namespace
