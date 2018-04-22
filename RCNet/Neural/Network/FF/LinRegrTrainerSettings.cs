using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Globalization;
using System.Reflection;
using RCNet.XmlTools;

namespace RCNet.Neural.Network.FF
{
    /// <summary>
    /// Startup parameters for the linear regression trainer
    /// </summary>
    [Serializable]
    public class LinRegrTrainerSettings
    {
        //Constants
        /// <summary>
        /// Default maximum stretch value of TanH function
        /// </summary>
        public const double DefaultMaxStretch = 8;
        /// <summary>
        /// Default value of the highest noise intensity
        /// </summary>
        public const double DefaultHiNoiseIntensity = 0.05;
        /// <summary>
        /// Default margin of noise values from zero
        /// </summary>
        public const double DefaultZeroMargin = 0.75;

        //Attribute properties
        /// <summary>
        /// Value of the highest noise intensity
        /// </summary>
        public double HiNoiseIntensity { get; set; }
        /// <summary>
        /// Maximum stretch value of TanH function
        /// </summary>
        public double MaxStretch { get; set; }
        /// <summary>
        /// Margin of noise values from zero
        /// </summary>
        public double ZeroMargin { get; set; }

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="hiNoiseIntensity">The highest white noise intensity. Between 0 and 1</param>
        /// <param name="maxStretch">Maximum stretch value of TanH function. GT 1</param>
        /// <param name="zeroMargin">Margin of noise values from zero. Between 0 and 1.</param>
        public LinRegrTrainerSettings(double hiNoiseIntensity = DefaultHiNoiseIntensity,
                                      double maxStretch = DefaultMaxStretch,
                                      double zeroMargin = DefaultZeroMargin
                                      )
        {
            HiNoiseIntensity = hiNoiseIntensity;
            MaxStretch = maxStretch;
            ZeroMargin = zeroMargin;
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public LinRegrTrainerSettings(LinRegrTrainerSettings source)
        {
            HiNoiseIntensity = source.HiNoiseIntensity;
            MaxStretch = source.MaxStretch;
            ZeroMargin = source.ZeroMargin;
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// Content of xml element is always validated against the xml schema.
        /// </summary>
        /// <param name="elem">Xml data containing linear regression trainer settings</param>
        public LinRegrTrainerSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.FF.LinRegrTrainerSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement linRegrTrainerSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            HiNoiseIntensity = double.Parse(linRegrTrainerSettingsElem.Attribute("hiNoiseIntensity").Value, CultureInfo.InvariantCulture);
            MaxStretch = double.Parse(linRegrTrainerSettingsElem.Attribute("maxStretch").Value, CultureInfo.InvariantCulture);
            ZeroMargin = double.Parse(linRegrTrainerSettingsElem.Attribute("zeroMargin").Value, CultureInfo.InvariantCulture);
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            LinRegrTrainerSettings cmpSettings = obj as LinRegrTrainerSettings;
            if (HiNoiseIntensity != cmpSettings.HiNoiseIntensity ||
                MaxStretch != cmpSettings.MaxStretch ||
                ZeroMargin != cmpSettings.ZeroMargin
                )
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// See the base.
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public LinRegrTrainerSettings DeepClone()
        {
            LinRegrTrainerSettings clone = new LinRegrTrainerSettings(this);
            return clone;
        }

    }//LinRegrTrainerSettings

}//Namespace
