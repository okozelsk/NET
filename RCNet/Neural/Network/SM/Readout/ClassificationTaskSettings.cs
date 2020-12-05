using RCNet.Neural.Data.Filter;
using RCNet.Neural.Network.NonRecurrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Configuration of the classification task
    /// </summary>
    [Serializable]
    public class ClassificationTaskSettings : RCNetBaseSettings, ITaskSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ROutLayerUnitClassificationTaskType";
        //Default values
        /// <summary>
        /// Means no membership to one winner group
        /// </summary>
        public const string DefaultOneWinnerGroupName = "NA";

        //Static members
        /// <summary>
        /// Shared instance of BinFeatureFilterSettings
        /// </summary>
        private static readonly IFeatureFilterSettings _sharedBinFeatureFilterCfg = new BinFeatureFilterSettings();

        //Attribute properties
        /// <summary>
        /// Specifies membership to "one winner" group of given name or no membership if default "NA" name is used
        /// </summary>
        public string OneWinnerGroupName { get; }

        /// <summary>
        /// Classification networks settings
        /// </summary>
        public ClassificationNetworksSettings NetworksCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="oneWinnerGroupName">Specifies membership to "one winner" group of given name or no membership if default "NA" name is used</param>
        /// <param name="networksCfg">Classifying networks settings</param>
        public ClassificationTaskSettings(string oneWinnerGroupName = DefaultOneWinnerGroupName,
                                          ClassificationNetworksSettings networksCfg = null
                                          )
        {
            OneWinnerGroupName = oneWinnerGroupName;
            NetworksCfg = networksCfg == null ? new ClassificationNetworksSettings() : (ClassificationNetworksSettings)networksCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ClassificationTaskSettings(ClassificationTaskSettings source)
            : this(source.OneWinnerGroupName, source.NetworksCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public ClassificationTaskSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            OneWinnerGroupName = settingsElem.Attribute("oneWinnerGroupName").Value;
            //Networks
            XElement classificationNetworksSettingsElem = settingsElem.Elements("networks").FirstOrDefault();
            NetworksCfg = classificationNetworksSettingsElem == null ? new ClassificationNetworksSettings() : new ClassificationNetworksSettings(classificationNetworksSettingsElem);
            Check();
            return;
        }

        //Properties
        /// <inheritdoc />
        public ReadoutUnit.TaskType Type { get { return ReadoutUnit.TaskType.Classification; } }

        /// <inheritdoc />
        public IFeatureFilterSettings FeatureFilterCfg { get { return _sharedBinFeatureFilterCfg; } }

        /// <inheritdoc />
        public List<INonRecurrentNetworkSettings> NetworkCfgCollection { get { return NetworksCfg.NetworkCfgCollection; } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultOneWinnerGroupName { get { return (OneWinnerGroupName == DefaultOneWinnerGroupName); } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultOneWinnerGroupName && NetworksCfg.ContainsOnlyDefaults;
            }
        }

        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            if (OneWinnerGroupName.Length == 0)
            {
                throw new ArgumentException($"Name of the one winner group can not be empty.", "OneWinnerGroupName");
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
            if (!suppressDefaults || !IsDefaultOneWinnerGroupName)
            {
                rootElem.Add(new XAttribute("oneWinnerGroupName", OneWinnerGroupName));
            }
            if (!NetworksCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(NetworksCfg.GetXml("networks", suppressDefaults));
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
