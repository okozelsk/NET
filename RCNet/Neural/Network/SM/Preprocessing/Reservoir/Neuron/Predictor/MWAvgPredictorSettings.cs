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
        /// Default value of leakage
        /// </summary>
        public const int DefaultLeakage = 0;
        /// <summary>
        /// Default weights type
        /// </summary>
        public const NeuronCommon.NeuronPredictorMWAvgWeightsType DefaultWeights = NeuronCommon.NeuronPredictorMWAvgWeightsType.Exponential;

        //Attribute properties
        /// <summary>
        /// Strength of fading
        /// </summary>
        /// <summary>
        /// Window length
        /// </summary>
        public int Window { get; }
        /// <summary>
        /// Leakage
        /// </summary>
        public int Leakage { get; }
        /// <summary>
        /// Type of weighting
        /// </summary>
        public NeuronCommon.NeuronPredictorMWAvgWeightsType Weights { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="window">Window length</param>
        /// <param name="leakage">Leakage</param>
        /// <param name="weights">Type of weighting</param>
        public MWAvgPredictorSettings(int window = DefaultWindow,
                                      int leakage = DefaultLeakage,
                                      NeuronCommon.NeuronPredictorMWAvgWeightsType weights = DefaultWeights
                                      )
        {
            Window = window;
            Leakage = leakage;
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
            Leakage = source.Leakage;
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
            Leakage = int.Parse(settingsElem.Attribute("leakage").Value, CultureInfo.InvariantCulture);
            Weights = (NeuronCommon.NeuronPredictorMWAvgWeightsType)Enum.Parse(typeof(NeuronCommon.NeuronPredictorMWAvgWeightsType), settingsElem.Attribute("weights").Value, true);
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
        public bool IsDefaultLeakage { get { return (Leakage == DefaultLeakage); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultWeights { get { return (Weights == DefaultWeights); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return IsDefaultWindow && IsDefaultLeakage && IsDefaultWeights; } }

        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (Window < 1 )
            {
                throw new Exception($"Invalid Strength {Window.ToString(CultureInfo.InvariantCulture)}. Window must be GT 0.");
            }
            if (Leakage < 0)
            {
                throw new Exception($"Invalid Leakage {Leakage.ToString(CultureInfo.InvariantCulture)}. Leakage must be GE to 0.");
            }
            if(Weights == NeuronCommon.NeuronPredictorMWAvgWeightsType.Exponential && Window > 64)
            {
                throw new Exception($"Invalid Strength {Window.ToString(CultureInfo.InvariantCulture)}. Window must be LE to 64.");
            }
            if (Weights == NeuronCommon.NeuronPredictorMWAvgWeightsType.Linear && Window > 10240)
            {
                throw new Exception($"Invalid Strength {Window.ToString(CultureInfo.InvariantCulture)}. Window must be LE to 10240.");
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
            if (!suppressDefaults || !IsDefaultLeakage)
            {
                rootElem.Add(new XAttribute("leakage", Leakage.ToString(CultureInfo.InvariantCulture)));
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
