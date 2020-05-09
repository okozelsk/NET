using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Firing count predictor settings
    /// </summary>
    [Serializable]
    public class FiringCountSettings : RCNetBaseSettings, IPredictorParamsSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PredictorFiringCountType";
        /// <summary>
        /// Default value of window length
        /// </summary>
        public const int DefaultWindow = 64;

        //Attribute properties
        /// <summary>
        /// Window length
        /// </summary>
        public int Window { get; }

        //Constructors
        /// <summary>
        /// Creates initialized instance
        /// </summary>
        /// <param name="window">Strength of fading</param>
        public FiringCountSettings(int window = DefaultWindow)
        {
            Window = window;
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public FiringCountSettings(FiringCountSettings source)
        {
            Window = source.Window;
            return;
        }

        /// <summary>
        /// Creates initialized instance using xml element
        /// </summary>
        /// <param name="elem">Xml element containing settings</param>
        public FiringCountSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Window = int.Parse(settingsElem.Attribute("window").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Predictor's ID
        /// </summary>
        public PredictorsProvider.PredictorID ID { get { return PredictorsProvider.PredictorID.FiringCount; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultWindow { get { return (Window == DefaultWindow); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return IsDefaultWindow; } }


        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (Window < 1)
            {
                throw new Exception($"Invalid Window {Window.ToString(CultureInfo.InvariantCulture)}. Window must be GE to 1.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new FiringCountSettings(this);
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
            if (!suppressDefaults || !IsDefaultWindow)
            {
                rootElem.Add(new XAttribute("window", Window.ToString(CultureInfo.InvariantCulture)));
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

    }//PredictorFiringCountSettings

}//Namespace
