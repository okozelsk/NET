using RCNet.MathTools;
using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Configuration of the ActivationDiffRescalledRange predictor
    /// </summary>
    [Serializable]
    public class PredictorActivationDiffRescalledRangeSettings : RCNetBaseSettings, IPredictorSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PredictorActivationDiffRescalledRangeType";

        //Attribute properties
        /// <summary>
        /// Specifies data window size
        /// </summary>
        public int Window { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="window">Specifies data window size</param>
        public PredictorActivationDiffRescalledRangeSettings(int window)
        {
            Window = window;
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public PredictorActivationDiffRescalledRangeSettings(PredictorActivationDiffRescalledRangeSettings source)
            : this(source.Window)
        {
            return;
        }

        /// <summary>
        /// Creates initialized instance using xml element
        /// </summary>
        /// <param name="elem">Xml element containing settings</param>
        public PredictorActivationDiffRescalledRangeSettings(XElement elem)
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
        /// ID of the predictor
        /// </summary>
        public PredictorsProvider.PredictorID ID { get { return PredictorsProvider.PredictorID.ActivationDiffRescalledRange; } }

        /// <summary>
        /// Specifies necessary size of the windowed history of activations
        /// </summary>
        public int RequiredWndSizeOfActivations { get { return Window; } }

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
        public override bool ContainsOnlyDefaults { get { return false; } }


        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            if (Window < 2 || Window > 1024)
            {
                throw new ArgumentException($"Invalid Window size {Window.ToString(CultureInfo.InvariantCulture)}. Window size must be GE2 and LE1024.", "Window");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new PredictorActivationDiffRescalledRangeSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("window", Window.ToString(CultureInfo.InvariantCulture))
                                             );
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

    }//PredictorActivationDiffRescalledRangeSettings

}//Namespace
