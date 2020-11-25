using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Configuration of the Activation predictor
    /// </summary>
    [Serializable]
    public class PredictorActivationSettings : RCNetBaseSettings, IPredictorSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PredictorActivationType";

        //Attribute properties

        //Constructors
        /// <summary>
        /// Creates initialized instance
        /// </summary>
        public PredictorActivationSettings()
        {
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public PredictorActivationSettings(PredictorActivationSettings source)
            : this()
        {
            return;
        }

        /// <summary>
        /// Creates initialized instance using xml element
        /// </summary>
        /// <param name="elem">Xml element containing settings</param>
        public PredictorActivationSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// ID of the predictor
        /// </summary>
        public PredictorsProvider.PredictorID ID { get { return PredictorsProvider.PredictorID.Activation; } }

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
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return true; } }


        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new PredictorActivationSettings(this);
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

    }//PredictorActivationSettings

}//Namespace
