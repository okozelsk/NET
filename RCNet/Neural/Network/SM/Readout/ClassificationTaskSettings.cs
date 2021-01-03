using RCNet.Neural.Data.Filter;
using RCNet.Neural.Network.NonRecurrent;
using System;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Configuration of the readout unit's classification task.
    /// </summary>
    [Serializable]
    public class ClassificationTaskSettings : RCNetBaseSettings, ITaskSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ROutUnitClassificationTaskType";
        //Default values
        /// <summary>
        /// The default value of the "One Takes All" group (NA means no membership in "One Takes All" group).
        /// </summary>
        public const string DefaultOneTakesAllGroupName = "NA";

        //Static members
        /// <summary>
        /// The shared instance of BinFeatureFilterSettings.
        /// </summary>
        private static readonly IFeatureFilterSettings _sharedBinFeatureFilterCfg = new BinFeatureFilterSettings();

        //Attribute properties
        /// <summary>
        /// Specifies the membership in "One Takes All" group of the specified name or no membership if NA keyword is used.
        /// </summary>
        public string OneTakesAllGroupName { get; }

        /// <summary>
        /// The cluster chain configuration.
        /// </summary>
        public TNRNetClusterChainSingleBoolSettings ClusterChainCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="oneTakesAllGroupName">Specifies the membership in "One Takes All" group of the specified name or no membership if NA keyword is used.</param>
        /// <param name="clusterChainCfg">The cluster chain configuration.</param>
        public ClassificationTaskSettings(string oneTakesAllGroupName = DefaultOneTakesAllGroupName,
                                          TNRNetClusterChainSingleBoolSettings clusterChainCfg = null
                                          )
        {
            OneTakesAllGroupName = oneTakesAllGroupName;
            ClusterChainCfg = clusterChainCfg == null ? null : (TNRNetClusterChainSingleBoolSettings)clusterChainCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public ClassificationTaskSettings(ClassificationTaskSettings source)
            : this(source.OneTakesAllGroupName, source.ClusterChainCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public ClassificationTaskSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            OneTakesAllGroupName = settingsElem.Attribute("oneTakesAllGroupName").Value;
            //Result
            XElement clusterChainElem = settingsElem.Elements("clusterChain").FirstOrDefault();
            ClusterChainCfg = clusterChainElem == null ? null : new TNRNetClusterChainSingleBoolSettings(clusterChainElem);
            Check();
            return;
        }

        //Properties
        /// <inheritdoc />
        public ReadoutUnit.TaskType Type { get { return ReadoutUnit.TaskType.Classification; } }

        /// <inheritdoc />
        public IFeatureFilterSettings FeatureFilterCfg { get { return _sharedBinFeatureFilterCfg; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultOneTakesAllGroupName { get { return (OneTakesAllGroupName == DefaultOneTakesAllGroupName); } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultOneTakesAllGroupName && ClusterChainCfg == null;
            }
        }

        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            if (OneTakesAllGroupName.Length == 0)
            {
                throw new ArgumentException($"The name of the One Takes All group can not be empty.", "OneTakesAllGroupName");
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new ClassificationTaskSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultOneTakesAllGroupName)
            {
                rootElem.Add(new XAttribute("oneTakesAllGroupName", OneTakesAllGroupName));
            }
            if (ClusterChainCfg != null && !ClusterChainCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(ClusterChainCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("classification", suppressDefaults);
        }

    }//ClassificationTaskSettings

}//Namespace
