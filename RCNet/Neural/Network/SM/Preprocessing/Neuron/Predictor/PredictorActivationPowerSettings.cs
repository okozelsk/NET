using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Configuration of the ActivationPower predictor computer.
    /// </summary>
    [Serializable]
    public class PredictorActivationPowerSettings : RCNetBaseSettings, IPredictorSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "PredictorActivationPowerType";
        //Default values
        /// <summary>
        /// The default value of the exponent.
        /// </summary>
        public const double DefaultExponent = 2d;
        /// <summary>
        /// The default value of the parameter specifying whether to keep the original sign of the activation value.
        /// </summary>
        public const bool DefaultKeepSign = false;

        //Attribute properties
        /// <summary>
        /// The exponent.
        /// </summary>
        public double Exponent { get; }

        /// <summary>
        /// Specifies whether to keep the original sign of the activation value.
        /// </summary>
        public bool KeepSign { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="exponent">The exponent.</param>
        /// <param name="keepSign">Specifies whether to keep the original sign of the activation value.</param>
        public PredictorActivationPowerSettings(double exponent = DefaultExponent, bool keepSign = DefaultKeepSign)
        {
            Exponent = exponent;
            KeepSign = keepSign;
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public PredictorActivationPowerSettings(PredictorActivationPowerSettings source)
            : this(source.Exponent, source.KeepSign)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public PredictorActivationPowerSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Exponent = double.Parse(settingsElem.Attribute("exponent").Value, CultureInfo.InvariantCulture);
            KeepSign = bool.Parse(settingsElem.Attribute("keepSign").Value);
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public PredictorsProvider.PredictorID ID { get { return PredictorsProvider.PredictorID.ActivationPower; } }

        /// <inheritdoc/>
        public int RequiredWndSizeOfActivations { get { return 0; } }

        /// <inheritdoc/>
        public int RequiredWndSizeOfFirings { get { return 0; } }

        /// <inheritdoc/>
        public bool NeedsContinuousActivationStat { get { return false; } }

        /// <inheritdoc/>
        public bool NeedsContinuousActivationDiffStat { get { return false; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultExponent { get { return (Exponent == DefaultExponent); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultKeepSign { get { return (KeepSign == DefaultKeepSign); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return IsDefaultExponent && IsDefaultKeepSign; } }


        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (Exponent <= 0d)
            {
                throw new ArgumentException($"Invalid Exponent {Exponent.ToString(CultureInfo.InvariantCulture)}. Exponent must be GT0.", "Exponent");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new PredictorActivationPowerSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultExponent)
            {
                rootElem.Add(new XAttribute("exponent", Exponent.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultKeepSign)
            {
                rootElem.Add(new XAttribute("keepSign", KeepSign.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml(PredictorFactory.GetXmlName(ID), suppressDefaults);
        }

    }//PredictorActivationPowerSettings

}//Namespace
