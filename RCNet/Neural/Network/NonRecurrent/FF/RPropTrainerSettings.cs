using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.NonRecurrent.FF
{
    /// <summary>
    /// Configuration of the RPropTrainer.
    /// </summary>
    [Serializable]
    public class RPropTrainerSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "FFNetRPropTrainerType";
        //Default values
        /// <summary>
        /// A default absolute value that is still considered as zero.
        /// </summary>
        public const double DefaultZeroTolerance = 1E-17d;
        /// <summary>
        /// The default positive Eta.
        /// </summary>
        public const double DefaultPositiveEta = 1.2d;
        /// <summary>
        /// The default negative Eta.
        /// </summary>
        public const double DefaultNegativeEta = 0.5d;
        /// <summary>
        /// The default initialization Delta value.
        /// </summary>
        public const double DefaultIniDelta = 0.1d;
        /// <summary>
        /// The default minimum Delta value.
        /// </summary>
        public const double DefaultMinDelta = 1E-6d;
        /// <summary>
        /// The default maximum Delta value.
        /// </summary>
        public const double DefaultMaxDelta = 50d;

        //Attribute properties
        /// <summary>
        /// The number of attempts.
        /// </summary>
        public int NumOfAttempts { get; }
        /// <summary>
        /// The number of attempt epochs.
        /// </summary>
        public int NumOfAttemptEpochs { get; }
        /// <summary>
        /// An absolute value that is still considered as zero.
        /// </summary>
        public double ZeroTolerance { get; }
        /// <summary>
        /// The positive Eta.
        /// </summary>
        public double PositiveEta { get; }
        /// <summary>
        /// The negative Eta.
        /// </summary>
        public double NegativeEta { get; }
        /// <summary>
        /// The initial Delta.
        /// </summary>
        public double IniDelta { get; }
        /// <summary>
        /// The minimum Delta.
        /// </summary>
        public double MinDelta { get; }
        /// <summary>
        /// The maximum Delta.
        /// </summary>
        public double MaxDelta { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="numOfAttempts">The number of attempts.</param>
        /// <param name="numOfAttemptEpochs">The number of attempt epochs.</param>
        /// <param name="zeroTolerance">An absolute value that is still considered as zero.</param>
        /// <param name="positiveEta">The positive Eta.</param>
        /// <param name="negativeEta">The negative Eta.</param>
        /// <param name="iniDelta">The initial Delta.</param>
        /// <param name="minDelta">The minimum Delta.</param>
        /// <param name="maxDelta">The maximum Delta.</param>
        public RPropTrainerSettings(int numOfAttempts,
                                    int numOfAttemptEpochs,
                                    double zeroTolerance = DefaultZeroTolerance,
                                    double positiveEta = DefaultPositiveEta,
                                    double negativeEta = DefaultNegativeEta,
                                    double iniDelta = DefaultIniDelta,
                                    double minDelta = DefaultMinDelta,
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
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
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
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
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
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultZeroTolerance { get { return (ZeroTolerance == DefaultZeroTolerance); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultPositiveEta { get { return (PositiveEta == DefaultPositiveEta); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultNegativeEta { get { return (NegativeEta == DefaultNegativeEta); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultIniDelta { get { return (IniDelta == DefaultIniDelta); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultMinDelta { get { return (MinDelta == DefaultMinDelta); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultMaxDelta { get { return (MaxDelta == DefaultMaxDelta); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc/>
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
            if (MaxDelta <= MinDelta)
            {
                throw new ArgumentException($"Invalid MinDelta or MaxDelta. MaxDelta must be GT MinDelta.", "MaxDelta/MinDelta");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new RPropTrainerSettings(this);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("resPropTrainer", suppressDefaults);
        }

    }//RPropTrainerSettings

}//Namespace
