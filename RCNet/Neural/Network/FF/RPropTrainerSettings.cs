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
    /// Setup parameters for the resilient propagation trainer
    /// </summary>
    [Serializable]
    public class RPropTrainerSettings
    {
        //Constants
        /// <summary>
        /// A default absolute value that is still considered as zero
        /// </summary>
        public const double DefaultZeroTolerance = 1E-17d;
        /// <summary>
        /// Default positive Eta
        /// </summary>
        public const double DefaultPositiveEta = 1.2d;
        /// <summary>
        /// Default negative Eta
        /// </summary>
        public const double DefaultNegativeEta = 0.5d;
        /// <summary>
        /// Delta default initialization value
        /// </summary>
        public const double DefaultIniDelta = 0.1d;
        /// <summary>
        /// Delta default minimum value
        /// </summary>
        public const double DefaultMinDelta = 1E-6d;
        /// <summary>
        /// Delta default maximum value
        /// </summary>
        public const double DefaultMaxDelta = 50d;

        //Attribute properties
        /// <summary>
        /// An absolute value that is still considered as zero
        /// </summary>
        public double ZeroTolerance { get; set; } = DefaultZeroTolerance;
        /// <summary>
        /// Positive Eta
        /// </summary>
        public double PositiveEta { get; set; } = DefaultPositiveEta;
        /// <summary>
        /// Negative Eta
        /// </summary>
        public double NegativeEta { get; set; } = DefaultNegativeEta;
        /// <summary>
        /// Delta initial value
        /// </summary>
        public double IniDelta { get; set; } = DefaultIniDelta;
        /// <summary>
        /// Delta minimum value
        /// </summary>
        public double MinDelta { get; set; } = DefaultMinDelta;
        /// <summary>
        /// Delta maximum value
        /// </summary>
        public double MaxDelta { get; set; } = DefaultMaxDelta;

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="zeroTolerance">An absolute value that is still considered as zero</param>
        /// <param name="positiveEta">Positive Eta</param>
        /// <param name="negativeEta">Negative Eta</param>
        /// <param name="iniDelta">Delta initial value</param>
        /// <param name="minDelta">Delta minimum value</param>
        /// <param name="maxDelta">Delta maximum value</param>
        public RPropTrainerSettings(double zeroTolerance = DefaultZeroTolerance,
                                    double positiveEta = DefaultPositiveEta,
                                    double negativeEta = DefaultNegativeEta,
                                    double iniDelta = DefaultIniDelta,
                                    double minDelta= DefaultMinDelta,
                                    double maxDelta = DefaultMaxDelta
                                    )
        {
            ZeroTolerance = zeroTolerance;
            PositiveEta = positiveEta;
            NegativeEta = negativeEta;
            IniDelta = iniDelta;
            MinDelta = minDelta;
            MaxDelta = maxDelta;
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public RPropTrainerSettings(RPropTrainerSettings source)
        {
            ZeroTolerance = source.ZeroTolerance;
            PositiveEta = source.PositiveEta;
            NegativeEta = source.NegativeEta;
            IniDelta = source.IniDelta;
            MinDelta = source.MinDelta;
            MaxDelta = source.MaxDelta;
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// Content of xml element is always validated against the xml schema.
        /// </summary>
        /// <param name="elem">Xml data containing resilient propagation trainer settings</param>
        public RPropTrainerSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.FF.RPropTrainerSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement rPropTrainerSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            ZeroTolerance = double.Parse(rPropTrainerSettingsElem.Attribute("zeroTolerance").Value, CultureInfo.InvariantCulture);
            PositiveEta = double.Parse(rPropTrainerSettingsElem.Attribute("positiveEta").Value, CultureInfo.InvariantCulture);
            NegativeEta = double.Parse(rPropTrainerSettingsElem.Attribute("negativeEta").Value, CultureInfo.InvariantCulture);
            IniDelta = double.Parse(rPropTrainerSettingsElem.Attribute("iniDelta").Value, CultureInfo.InvariantCulture);
            MinDelta = double.Parse(rPropTrainerSettingsElem.Attribute("minDelta").Value, CultureInfo.InvariantCulture);
            MaxDelta = double.Parse(rPropTrainerSettingsElem.Attribute("maxDelta").Value, CultureInfo.InvariantCulture);
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            RPropTrainerSettings cmpSettings = obj as RPropTrainerSettings;
            if (ZeroTolerance != cmpSettings.ZeroTolerance ||
                PositiveEta != cmpSettings.PositiveEta ||
                NegativeEta != cmpSettings.NegativeEta ||
                IniDelta != cmpSettings.IniDelta ||
                MinDelta != cmpSettings.MinDelta ||
                MaxDelta != cmpSettings.MaxDelta
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
        public RPropTrainerSettings DeepClone()
        {
            RPropTrainerSettings clone = new RPropTrainerSettings(this);
            return clone;
        }

    }//RPropTrainerSettings

}//Namespace
