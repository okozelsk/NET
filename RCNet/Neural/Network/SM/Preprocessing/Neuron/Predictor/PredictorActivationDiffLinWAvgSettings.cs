using RCNet.MathTools;
using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Configuration of the ActivationDiffLinWAvg predictor
    /// </summary>
    [Serializable]
    public class PredictorActivationDiffLinWAvgSettings : RCNetBaseSettings, IPredictorSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PredictorActivationDiffLinWAvgType";
        /// <summary>
        /// Numeric value indicating no data window
        /// </summary>
        public const int NAWindowNum = 0;
        /// <summary>
        /// Code indicating no data window
        /// </summary>
        public const string NAWindowCode = "NA";
        //Default values
        /// <summary>
        /// Default value of the parameter specifying data window size
        /// </summary>
        public const int DefaultWindow = NAWindowNum;

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
        public PredictorActivationDiffLinWAvgSettings(int window = DefaultWindow)
        {
            Window = window;
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public PredictorActivationDiffLinWAvgSettings(PredictorActivationDiffLinWAvgSettings source)
            : this(source.Window)
        {
            return;
        }

        /// <summary>
        /// Creates initialized instance using xml element
        /// </summary>
        /// <param name="elem">Xml element containing settings</param>
        public PredictorActivationDiffLinWAvgSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            string attrValue = settingsElem.Attribute("window").Value;
            Window = attrValue == NAWindowCode ? NAWindowNum : int.Parse(attrValue, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// ID of the predictor
        /// </summary>
        public PredictorsProvider.PredictorID ID { get { return PredictorsProvider.PredictorID.ActivationDiffLinWAvg; } }

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
        public bool NeedsContinuousActivationDiffStat { get { return Window == NAWindowNum; } }

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
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            if (Window < 0 || Window == 1 || Window > 1024)
            {
                throw new ArgumentException($"Invalid Window size {Window.ToString(CultureInfo.InvariantCulture)}. Window size must be GE0 and NE1 and LE1024.", "Window");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new PredictorActivationDiffLinWAvgSettings(this);
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
            if (!suppressDefaults || !IsDefaultWindow)
            {
                rootElem.Add(new XAttribute("window", Window == NAWindowNum ? NAWindowCode : Window.ToString(CultureInfo.InvariantCulture)));
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

    }//PredictorActivationDiffLinWAvgSettings

}//Namespace
