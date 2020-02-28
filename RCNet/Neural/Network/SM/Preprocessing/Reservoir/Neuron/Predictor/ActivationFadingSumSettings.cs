using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron.Predictor
{
    /// <summary>
    /// Fading sum of the activation state
    /// </summary>
    [Serializable]
    public class ActivationFadingSumSettings : RCNetBaseSettings, IPredictorParamsSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PredictorActivationFadingSumType";
        /// <summary>
        /// Default value of strength of fading
        /// </summary>
        public const double DefaultStrength = 0.1;

        //Attribute properties
        /// <summary>
        /// Strength of fading
        /// </summary>
        public double Strength { get; }

        //Constructors
        /// <summary>
        /// Creates initialized instance using default values
        /// </summary>
        /// <param name="strength">Strength of fading</param>
        public ActivationFadingSumSettings(double strength = DefaultStrength)
        {
            Strength = strength;
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ActivationFadingSumSettings(ActivationFadingSumSettings source)
        {
            Strength = source.Strength;
            return;
        }

        /// <summary>
        /// Creates initialized instance using xml element
        /// </summary>
        /// <param name="elem">Xml element containing settings</param>
        public ActivationFadingSumSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Strength = double.Parse(settingsElem.Attribute("strength").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// ID of the predictor
        /// </summary>
        public PredictorsProvider.PredictorID ID { get { return PredictorsProvider.PredictorID.ActivationFadingSum; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultStrength { get { return (Strength == DefaultStrength); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return IsDefaultStrength; } }


        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (Strength < 0 || Strength >= 1)
            {
                throw new Exception($"Invalid Strength {Strength.ToString(CultureInfo.InvariantCulture)}. Strength must be GE to 0 and LT 1.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new ActivationFadingSumSettings(this);
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
            if (!suppressDefaults || !IsDefaultStrength)
            {
                rootElem.Add(new XAttribute("strength", Strength.ToString(CultureInfo.InvariantCulture)));
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
            return GetXml(PredictorsSettings.GetXmlName(ID), suppressDefaults);
        }

    }//PredictorActivationFadingSumSettings

}//Namespace
