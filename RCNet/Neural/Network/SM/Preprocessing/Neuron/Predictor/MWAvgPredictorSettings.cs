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
    /// Moving weighted average predictor settings
    /// </summary>
    [Serializable]
    public abstract class MWAvgPredictorSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PredictorMWAvgType";
        /// <summary>
        /// Default value of window length
        /// </summary>
        public const int DefaultWindow = 64;
        /// <summary>
        /// Default weights type
        /// </summary>
        public const PredictorsProvider.PredictorMWAvgWeightsType DefaultWeights = PredictorsProvider.PredictorMWAvgWeightsType.Exponential;

        //Attribute properties
        /// <summary>
        /// Strength of fading
        /// </summary>
        /// <summary>
        /// Window length
        /// </summary>
        public int Window { get; }
        /// <summary>
        /// Type of weighting
        /// </summary>
        public PredictorsProvider.PredictorMWAvgWeightsType Weights { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="window">Window length</param>
        /// <param name="weights">Type of weighting</param>
        public MWAvgPredictorSettings(int window = DefaultWindow,
                                      PredictorsProvider.PredictorMWAvgWeightsType weights = DefaultWeights
                                      )
        {
            Window = window;
            Weights = weights;
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public MWAvgPredictorSettings(MWAvgPredictorSettings source)
        {
            Window = source.Window;
            Weights = source.Weights;
            return;
        }

        /// <summary>
        /// Creates initialized instance using xml element
        /// </summary>
        /// <param name="elem">Xml element containing settings</param>
        public MWAvgPredictorSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Window = int.Parse(settingsElem.Attribute("window").Value, CultureInfo.InvariantCulture);
            Weights = (PredictorsProvider.PredictorMWAvgWeightsType)Enum.Parse(typeof(PredictorsProvider.PredictorMWAvgWeightsType), settingsElem.Attribute("weights").Value, true);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultWindow { get { return (Window == DefaultWindow); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultWeights { get { return (Weights == DefaultWeights); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return IsDefaultWindow && IsDefaultWeights; } }

        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (Window < 1 )
            {
                throw new Exception($"Invalid Window {Window.ToString(CultureInfo.InvariantCulture)}. Window must be GT 0.");
            }
            if(Weights == PredictorsProvider.PredictorMWAvgWeightsType.Exponential && Window > 64)
            {
                throw new Exception($"Invalid Window {Window.ToString(CultureInfo.InvariantCulture)}. Window must be LE to 64.");
            }
            if (Weights == PredictorsProvider.PredictorMWAvgWeightsType.Linear && Window > 10240)
            {
                throw new Exception($"Invalid Window {Window.ToString(CultureInfo.InvariantCulture)}. Window must be LE to 10240.");
            }
            return;
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
            if (!suppressDefaults || !IsDefaultWeights)
            {
                rootElem.Add(new XAttribute("weights", Weights.ToString()));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

    }//PredictorMWAvgSettings

}//Namespace
