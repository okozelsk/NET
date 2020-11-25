using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Configuration of the ActivationPower predictor
    /// </summary>
    [Serializable]
    public class PredictorActivationPowerSettings : RCNetBaseSettings, IPredictorSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PredictorActivationPowerType";
        //Default values
        /// <summary>
        /// Default value of the exponent
        /// </summary>
        public const double DefaultExponent = 2d;
        /// <summary>
        /// Default value of the parameter specifying whether to keep original sign of the activation value
        /// </summary>
        public const bool DefaultKeepSign = false;

        //Attribute properties
        /// <summary>
        /// Exponent
        /// </summary>
        public double Exponent { get; }

        /// <summary>
        /// Specifies whether to keep original sign of the activation value
        /// </summary>
        public bool KeepSign { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="exponent">Exponent</param>
        /// <param name="keepSign">Specifies whether to keep original sign of the activation value</param>
        public PredictorActivationPowerSettings(double exponent = DefaultExponent, bool keepSign = DefaultKeepSign)
        {
            Exponent = exponent;
            KeepSign = keepSign;
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public PredictorActivationPowerSettings(PredictorActivationPowerSettings source)
            : this(source.Exponent, source.KeepSign)
        {
            return;
        }

        /// <summary>
        /// Creates initialized instance using xml element
        /// </summary>
        /// <param name="elem">Xml element containing settings</param>
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
        /// <summary>
        /// ID of the predictor
        /// </summary>
        public PredictorsProvider.PredictorID ID { get { return PredictorsProvider.PredictorID.ActivationPower; } }

        /// <summary>
        /// Specifies necessary size of the windowed history of activations
        /// </summary>
        public int RequiredWndSizeOfActivations { get { return 0; } }

        /// <summary>
        /// Specifies necessary size of the windowed history of firings
        /// </summary>
        public int RequiredWndSizeOfFirings { get { return 0; } }

        /// <summary>
        /// Indicates use of continuous stat of activations
        /// </summary>
        public bool NeedsContinuousActivationStat { get { return false; } }

        /// <summary>
        /// Indicates use of continuous stat of activation differences
        /// </summary>
        public bool NeedsContinuousActivationDiffStat { get { return false; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultExponent { get { return (Exponent == DefaultExponent); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultKeepSign { get { return (KeepSign == DefaultKeepSign); } }


        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return IsDefaultExponent && IsDefaultKeepSign; } }


        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            if (Exponent <= 0d)
            {
                throw new ArgumentException($"Invalid Exponent {Exponent.ToString(CultureInfo.InvariantCulture)}. Exponent must be GT0.", "Exponent");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new PredictorActivationPowerSettings(this);
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

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml(PredictorFactory.GetXmlName(ID), suppressDefaults);
        }

    }//PredictorActivationPowerSettings

}//Namespace
