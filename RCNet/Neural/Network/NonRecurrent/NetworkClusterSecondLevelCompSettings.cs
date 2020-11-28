using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using RCNet.Neural.Network.NonRecurrent.FF;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Configuration of the 2nd level computation of the network cluster
    /// </summary>
    [Serializable]
    public class NetworkClusterSecondLevelCompSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NetworkClusterSecondLevelCompType";
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
        /// Default value of the parameter specifying computation mode of the cluster
        /// </summary>
        public const TrainedNetworkCluster.SecondLevelCompMode DefaultCompMode = TrainedNetworkCluster.SecondLevelCompMode.AveragedOutputs;
        /// <summary>
        /// Default value of the parameter specifying required test data ratio constituting one fold
        /// </summary>
        public const double DefaultTestDataRatio = 0.333333333d;
        /// <summary>
        /// Default number of folds - string code
        /// </summary>
        public const string DefaultFoldsString = AutoFoldsCode;
        /// <summary>
        /// Default number of folds - numeric code
        /// </summary>
        public const int DefaultFoldsNum = AutoFolds;

        //Attribute properties
        /// <summary>
        /// 2nd level network configuration
        /// </summary>
        public FeedForwardNetworkSettings NetCfg { get; }

        /// <summary>
        /// Computation mode of the cluster
        /// </summary>
        public TrainedNetworkCluster.SecondLevelCompMode CompMode { get; }

        /// <summary>
        /// Required test data ratio constituting one fold
        /// </summary>
        public double TestDataRatio { get; }

        /// <summary>
        /// Number of folds of 2nd level x-fold cross-validation computation
        /// </summary>
        public int Folds { get; }


        //Constructors
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        /// <param name="netCfg">2nd level network configuration</param>
        /// <param name="compMode">Computation mode</param>
        /// <param name="testDataRatio">Reqired test data ratio constituing one fold</param>
        /// <param name="folds">Number of folds of 2nd level x-fold cross-validation computation</param>
        public NetworkClusterSecondLevelCompSettings(FeedForwardNetworkSettings netCfg,
                                                  TrainedNetworkCluster.SecondLevelCompMode compMode = DefaultCompMode,
                                                  double testDataRatio = DefaultTestDataRatio,
                                                  int folds = DefaultFoldsNum
                                                  )
        {
            NetCfg = (FeedForwardNetworkSettings)netCfg.DeepClone();
            CompMode = compMode;
            TestDataRatio = testDataRatio;
            Folds = folds;
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public NetworkClusterSecondLevelCompSettings(NetworkClusterSecondLevelCompSettings source)
            : this(source.NetCfg, source.CompMode, source.TestDataRatio, source.Folds)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml data containing the settings.</param>
        public NetworkClusterSecondLevelCompSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            NetCfg = new FeedForwardNetworkSettings(settingsElem.Element("ff"));
            CompMode = (TrainedNetworkCluster.SecondLevelCompMode)Enum.Parse(typeof(TrainedNetworkCluster.SecondLevelCompMode), settingsElem.Attribute("mode").Value, true);
            TestDataRatio = double.Parse(settingsElem.Attribute("testDataRatio").Value, CultureInfo.InvariantCulture);
            Folds = settingsElem.Attribute("folds").Value == DefaultFoldsString ? DefaultFoldsNum : int.Parse(settingsElem.Attribute("folds").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultCompMode { get { return (CompMode == DefaultCompMode); } }
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultTestDataRatio { get { return (TestDataRatio == DefaultTestDataRatio); } }
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultFolds { get { return (Folds == DefaultFoldsNum); } }
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
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance.
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new NetworkClusterSecondLevelCompSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName, NetCfg.GetXml(suppressDefaults));
            if (!suppressDefaults || !IsDefaultCompMode)
            {
                rootElem.Add(new XAttribute("mode", CompMode.ToString()));
            }
            if (!suppressDefaults || !IsDefaultTestDataRatio)
            {
                rootElem.Add(new XAttribute("testDataRatio", TestDataRatio.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultFolds)
            {
                rootElem.Add(new XAttribute("folds", Folds == DefaultFoldsNum ? DefaultFoldsString : Folds.ToString(CultureInfo.InvariantCulture)));
            }
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
            return GetXml("clusterSecondLevelComputation", suppressDefaults);
        }

    }//NetworkClusterSecondLevelCompSettings

}//Namespace
