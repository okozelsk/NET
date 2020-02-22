using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using RCNet.Extensions;
using RCNet.MathTools.Probability;
using RCNet.XmlTools;
using RCNet.RandomValue;
using RCNet.Neural.Network.NonRecurrent;
using RCNet.Neural.Network.NonRecurrent.FF;
using RCNet.Neural.Network.NonRecurrent.PP;
using RCNet.Neural.Data.Filter;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Readout unit - classification task settings
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
        private static IFeatureFilterSettings _sharedBinFeatureFilterCfg = new BinFeatureFilterSettings();

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
            :this(source.OneWinnerGroupName, source.NetworksCfg)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public ClassificationTaskSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            OneWinnerGroupName = settingsElem.Attribute("oneWinnerGroupName").Value;
            //Networks
            XElement classificationNetworksSettingsElem = settingsElem.Descendants("networks").FirstOrDefault();
            NetworksCfg = classificationNetworksSettingsElem == null ? new ClassificationNetworksSettings() : new ClassificationNetworksSettings(classificationNetworksSettingsElem);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Identifies forecast task
        /// </summary>
        public ReadoutUnit.TaskType Type { get { return ReadoutUnit.TaskType.Classification; } }

        /// <summary>
        /// Output feature filter settings (always BinFeatureFilterSettings)
        /// </summary>
        public IFeatureFilterSettings FeatureFilterCfg { get { return _sharedBinFeatureFilterCfg; } }

        /// <summary>
        /// Associated networks settings
        /// </summary>
        public List<INonRecurrentNetworkSettings> NetworkCfgCollection { get { return NetworksCfg.NetworkCfgCollection; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultOneWinnerGroupName { get { return (OneWinnerGroupName == DefaultOneWinnerGroupName); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultOneWinnerGroupName && NetworksCfg.ContainsOnlyDefaults;
            }
        }

        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if(OneWinnerGroupName.Length == 0)
            {
                throw new Exception($"Name of the one winner group can not be empty.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new ClassificationTaskSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
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

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("classification", suppressDefaults);
        }

    }//ClassificationTaskSettings

}//Namespace
