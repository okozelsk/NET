using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Globalization;
using System.Reflection;
using RCNet.XmlTools;

namespace RCNet.Neural.Network.NonRecurrent.FF
{
    /// <summary>
    /// Setup parameters for the resilient propagation trainer
    /// </summary>
    [Serializable]
    public class RPropTrainerSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "FFNetRPropTrainerType";
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
        /// Number of attempts
        /// </summary>
        public int NumOfAttempts { get; }
        /// <summary>
        /// Number of attempt epochs
        /// </summary>
        public int NumOfAttemptEpochs { get; }
        /// <summary>
        /// An absolute value that is still considered as zero
        /// </summary>
        public double ZeroTolerance { get; }
        /// <summary>
        /// Positive Eta
        /// </summary>
        public double PositiveEta { get; }
        /// <summary>
        /// Negative Eta
        /// </summary>
        public double NegativeEta { get; }
        /// <summary>
        /// Delta initial value
        /// </summary>
        public double IniDelta { get; }
        /// <summary>
        /// Delta minimum value
        /// </summary>
        public double MinDelta { get; }
        /// <summary>
        /// Delta maximum value
        /// </summary>
        public double MaxDelta { get; }

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="numOfAttempts">Number of attempts</param>
        /// <param name="numOfAttemptEpochs">Number of attempt epochs</param>
        /// <param name="zeroTolerance">An absolute value that is still considered as zero</param>
        /// <param name="positiveEta">Positive Eta</param>
        /// <param name="negativeEta">Negative Eta</param>
        /// <param name="iniDelta">Delta initial value</param>
        /// <param name="minDelta">Delta minimum value</param>
        /// <param name="maxDelta">Delta maximum value</param>
        public RPropTrainerSettings(int numOfAttempts,
                                    int numOfAttemptEpochs,
                                    double zeroTolerance = DefaultZeroTolerance,
                                    double positiveEta = DefaultPositiveEta,
                                    double negativeEta = DefaultNegativeEta,
                                    double iniDelta = DefaultIniDelta,
                                    double minDelta= DefaultMinDelta,
                                    double maxDelta = DefaultMaxDelta
                                    )
        {
            NumOfAttempts = numOfAttempts;
            NumOfAttemptEpochs = numOfAttemptEpochs;
            ZeroTolerance = zeroTolerance;
            PositiveEta = positiveEta;
            NegativeEta = negativeEta;
            IniDelta = iniDelta;
            MinDelta = minDelta;
            MaxDelta = maxDelta;
            Check();
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public RPropTrainerSettings(RPropTrainerSettings source)
        {
            NumOfAttempts = source.NumOfAttempts;
            NumOfAttemptEpochs = source.NumOfAttemptEpochs;
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
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            NumOfAttempts = int.Parse(settingsElem.Attribute("attempts").Value, CultureInfo.InvariantCulture);
            NumOfAttemptEpochs = int.Parse(settingsElem.Attribute("attemptEpochs").Value, CultureInfo.InvariantCulture);
            ZeroTolerance = double.Parse(settingsElem.Attribute("zeroTolerance").Value, CultureInfo.InvariantCulture);
            PositiveEta = double.Parse(settingsElem.Attribute("positiveEta").Value, CultureInfo.InvariantCulture);
            NegativeEta = double.Parse(settingsElem.Attribute("negativeEta").Value, CultureInfo.InvariantCulture);
            IniDelta = double.Parse(settingsElem.Attribute("iniDelta").Value, CultureInfo.InvariantCulture);
            MinDelta = double.Parse(settingsElem.Attribute("minDelta").Value, CultureInfo.InvariantCulture);
            MaxDelta = double.Parse(settingsElem.Attribute("maxDelta").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultZeroTolerance { get { return (ZeroTolerance == DefaultZeroTolerance); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultPositiveEta { get { return (PositiveEta == DefaultPositiveEta); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultNegativeEta { get { return (NegativeEta == DefaultNegativeEta); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultIniDelta { get { return (IniDelta == DefaultIniDelta); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultMinDelta { get { return (MinDelta == DefaultMinDelta); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultMaxDelta { get { return (MaxDelta == DefaultMaxDelta); } }

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
            if (NumOfAttempts < 1)
            {
                throw new ArgumentException($"Invalid NumOfAttempts {NumOfAttempts.ToString(CultureInfo.InvariantCulture)}. NumOfAttempts must be GE to 1.", "NumOfAttempts");
            }
            if (NumOfAttemptEpochs < 1)
            {
                throw new ArgumentException($"Invalid NumOfAttemptEpochs {NumOfAttemptEpochs.ToString(CultureInfo.InvariantCulture)}. NumOfAttemptEpochs must be GE to 1.", "NumOfAttemptEpochs");
            }
            if (ZeroTolerance < 0 || ZeroTolerance >= 1)
            {
                throw new ArgumentException($"Invalid ZeroTolerance {ZeroTolerance.ToString(CultureInfo.InvariantCulture)}. ZeroTolerance must be GE to 0 and LT 1.", "ZeroTolerance");
            }
            if (PositiveEta <= 1)
            {
                throw new ArgumentException($"Invalid PositiveEta {PositiveEta.ToString(CultureInfo.InvariantCulture)}. PositiveEta must be GT 1.", "PositiveEta");
            }
            if (NegativeEta < 0 || NegativeEta >= 1)
            {
                throw new ArgumentException($"Invalid NegativeEta {NegativeEta.ToString(CultureInfo.InvariantCulture)}. NegativeEta must be GE to 0 and LT 1.", "NegativeEta");
            }
            if (IniDelta < 0)
            {
                throw new ArgumentException($"Invalid IniDelta {IniDelta.ToString(CultureInfo.InvariantCulture)}. IniDelta must be GE to 0.", "IniDelta");
            }
            if (MinDelta < 0)
            {
                throw new ArgumentException($"Invalid MinDelta {MinDelta.ToString(CultureInfo.InvariantCulture)}. MinDelta must be GE to 0.", "MinDelta");
            }
            if (MaxDelta < 0)
            {
                throw new ArgumentException($"Invalid MinDelta {MaxDelta.ToString(CultureInfo.InvariantCulture)}. MaxDelta must be GE to 0.", "MaxDelta");
            }
            if(MaxDelta <= MinDelta)
            {
                throw new ArgumentException($"Invalid MinDelta or MaxDelta. MaxDelta must be GT MinDelta.", "MaxDelta/MinDelta");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new RPropTrainerSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName, new XAttribute("attempts", NumOfAttempts.ToString(CultureInfo.InvariantCulture)),
                                                           new XAttribute("attemptEpochs", NumOfAttemptEpochs.ToString(CultureInfo.InvariantCulture)));
            if (!suppressDefaults || !IsDefaultZeroTolerance)
            {
                rootElem.Add(new XAttribute("zeroTolerance", ZeroTolerance.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultPositiveEta)
            {
                rootElem.Add(new XAttribute("positiveEta", PositiveEta.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultNegativeEta)
            {
                rootElem.Add(new XAttribute("negativeEta", NegativeEta.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultIniDelta)
            {
                rootElem.Add(new XAttribute("iniDelta", IniDelta.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultMinDelta)
            {
                rootElem.Add(new XAttribute("minDelta", MinDelta.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultMaxDelta)
            {
                rootElem.Add(new XAttribute("maxDelta", MaxDelta.ToString(CultureInfo.InvariantCulture)));
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
            return GetXml("resPropTrainer", suppressDefaults);
        }

    }//RPropTrainerSettings

}//Namespace
