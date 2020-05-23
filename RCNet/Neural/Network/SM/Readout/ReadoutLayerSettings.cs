using RCNet.Neural.Network.NonRecurrent;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// The class contains readout layer configuration parameters.
    /// </summary>
    [Serializable]
    public class ReadoutLayerSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ROutLayerType";
        /// <summary>
        /// Maximum allowed test data ratio
        /// </summary>
        public const double MaxTestDataRatio = 0.5d;
        /// <summary>
        /// Automatic number of folds (code)
        /// </summary>
        public const string AutoFoldsCode = "Auto";
        /// <summary>
        /// Automatic number of folds (num)
        /// </summary>
        public const int AutoFolds = 0;
        //Default values
        /// <summary>
        /// Default number of folds - string code
        /// </summary>
        public const string DefaultFoldsString = AutoFoldsCode;
        /// <summary>
        /// Default number of folds - numeric code
        /// </summary>
        public const int DefaultFoldsNum = AutoFolds;
        /// <summary>
        /// Default number of repetitions
        /// </summary>
        public const int DefaultRepetitions = 1;


        //Attribute properties
        /// <summary>
        /// Specifies how big part of available samples will be used as testing samples during the training
        /// </summary>
        public double TestDataRatio { get; }

        /// <summary>
        /// The x in the x-fold cross-validation
        /// https://en.wikipedia.org/wiki/Cross-validation_(statistics)
        /// Parameter has two options.
        /// 0 - means auto setup to achieve full cross-validation if it is possible (related to specified TestDataRatio)
        /// GT 0 - means exact number of the folds
        /// </summary>
        public int Folds { get; }

        /// <summary>
        /// Defines how many times the generation of whole folds will be repeated
        /// </summary>
        public int Repetitions { get; }

        /// <summary>
        /// Task dependent networks settings to be applied when specific networks for readout unit are not specified
        /// </summary>
        public DefaultNetworksSettings DefaultNetworksCfg { get; }

        /// <summary>
        /// Readout units settings
        /// </summary>
        public ReadoutUnitsSettings ReadoutUnitsCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="readoutUnitsCfg">Readout units settings</param>
        /// <param name="testDataRatio">Specifies how big part of available samples will be used as testing samples during the training</param>
        /// <param name="folds">The x in the x-fold cross-validation</param>
        /// <param name="repetitions">Defines how many times the generation of whole folds will be repeated</param>
        /// <param name="defaultNetworksCfg">Task dependent networks settings to be applied when specific networks for readout unit are not specified</param>
        public ReadoutLayerSettings(ReadoutUnitsSettings readoutUnitsCfg,
                                    double testDataRatio,
                                    int folds = DefaultFoldsNum,
                                    int repetitions = DefaultRepetitions,
                                    DefaultNetworksSettings defaultNetworksCfg = null
                                    )
        {
            //Default settings
            TestDataRatio = testDataRatio;
            Folds = folds;
            Repetitions = repetitions;
            ReadoutUnitsCfg = (ReadoutUnitsSettings)readoutUnitsCfg.DeepClone();
            if (defaultNetworksCfg == null)
            {
                DefaultNetworksCfg = new DefaultNetworksSettings();
            }
            else
            {
                DefaultNetworksCfg = (DefaultNetworksSettings)defaultNetworksCfg.DeepClone();
            }
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ReadoutLayerSettings(ReadoutLayerSettings source)
            : this(source.ReadoutUnitsCfg, source.TestDataRatio, source.Folds, source.Repetitions, source.DefaultNetworksCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// This is the preferred way to instantiate ReadoutLayer settings.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public ReadoutLayerSettings(XElement elem)
        {
            //Validation
            XElement readoutLayerSettingsElem = Validate(elem, XsdTypeName);
            //Parsing
            TestDataRatio = double.Parse(readoutLayerSettingsElem.Attribute("testDataRatio").Value, CultureInfo.InvariantCulture);
            Folds = readoutLayerSettingsElem.Attribute("folds").Value == DefaultFoldsString ? DefaultFoldsNum : int.Parse(readoutLayerSettingsElem.Attribute("folds").Value, CultureInfo.InvariantCulture);
            Repetitions = int.Parse(readoutLayerSettingsElem.Attribute("repetitions").Value, CultureInfo.InvariantCulture);
            //Default networks settings
            XElement defaultNetworksElem = readoutLayerSettingsElem.Elements("defaultNetworks").FirstOrDefault();
            DefaultNetworksCfg = defaultNetworksElem == null ? new DefaultNetworksSettings() : new DefaultNetworksSettings(defaultNetworksElem);
            //Readout units
            XElement readoutUnitsElem = readoutLayerSettingsElem.Elements("readoutUnits").First();
            ReadoutUnitsCfg = new ReadoutUnitsSettings(readoutUnitsElem);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Collection of names of output fields
        /// </summary>
        public List<string> OutputFieldNameCollection
        {
            get
            {
                return (from rus in ReadoutUnitsCfg.ReadoutUnitCfgCollection select rus.Name).ToList();
            }
        }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultFolds { get { return (Folds == DefaultFoldsNum); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultRepetitions { get { return (Repetitions == DefaultRepetitions); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            if (TestDataRatio <= 0 || TestDataRatio > MaxTestDataRatio)
            {
                throw new ArgumentException($"Invalid TestDataRatio {TestDataRatio.ToString(CultureInfo.InvariantCulture)}. TestDataRatio must be GT 0 and GE {MaxTestDataRatio.ToString(CultureInfo.InvariantCulture)}.", "TestDataRatio");
            }
            if (Folds < 0)
            {
                throw new ArgumentException($"Invalid Folds {Folds.ToString(CultureInfo.InvariantCulture)}. Folds must be GE to 0 (0 means Auto folds).", "Folds");
            }
            if (Repetitions < 1)
            {
                throw new ArgumentException($"Invalid Repetitions {Repetitions.ToString(CultureInfo.InvariantCulture)}. Repetitions must be GE to 1.", "Repetitions");
            }
            foreach (ReadoutUnitSettings rus in ReadoutUnitsCfg.ReadoutUnitCfgCollection)
            {
                if (rus.TaskCfg.NetworkCfgCollection.Count == 0)
                {
                    if (DefaultNetworksCfg.GetTaskNetworksCfgs(rus.TaskCfg.Type).Count == 0)
                    {
                        throw new ArgumentException($"Readout unit {rus.Name} has not associated network(s) settings.", "ReadoutUnitsCfg");
                    }
                }
            }
            return;
        }

        /// <summary>
        /// Return network configurations associated with readout unit or default network configurations if no specific network configurations.
        /// </summary>
        /// <param name="readoutUnitIndex">Index of the readout unit</param>
        /// <returns></returns>
        public List<INonRecurrentNetworkSettings> GetReadoutUnitNetworksCollection(int readoutUnitIndex)
        {
            if (ReadoutUnitsCfg.ReadoutUnitCfgCollection[readoutUnitIndex].TaskCfg.NetworkCfgCollection.Count > 0)
            {
                return ReadoutUnitsCfg.ReadoutUnitCfgCollection[readoutUnitIndex].TaskCfg.NetworkCfgCollection;
            }
            else
            {
                return DefaultNetworksCfg.GetTaskNetworksCfgs(ReadoutUnitsCfg.ReadoutUnitCfgCollection[readoutUnitIndex].TaskCfg.Type);
            }
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new ReadoutLayerSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            rootElem.Add(new XAttribute("testDataRatio", TestDataRatio.ToString(CultureInfo.InvariantCulture)));
            if (!suppressDefaults || !IsDefaultFolds)
            {
                rootElem.Add(new XAttribute("folds", Folds == DefaultFoldsNum ? DefaultFoldsString : Folds.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultRepetitions)
            {
                rootElem.Add(new XAttribute("repetitions", Repetitions.ToString(CultureInfo.InvariantCulture)));
            }
            if (!DefaultNetworksCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(DefaultNetworksCfg.GetXml(suppressDefaults));
            }
            rootElem.Add(ReadoutUnitsCfg.GetXml(suppressDefaults));
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("readoutLayer", suppressDefaults);
        }


    }//ReadoutLayerSettings

}//Namespace
