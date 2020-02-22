using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Globalization;
using System.Reflection;
using RCNet.XmlTools;
using RCNet.MathTools.PS;

namespace RCNet.Neural.Network.NonRecurrent.FF
{
    /// <summary>
    /// Startup parameters for the QRD regression trainer
    /// </summary>
    [Serializable]
    public class QRDRegrTrainerSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "FFNetQRDRegrTrainerType";
        /// <summary>
        /// Default margin of noise values from zero
        /// </summary>
        public const double DefaultNoiseZeroMargin = 0.75;
        /// <summary>
        /// Seeker's default min noise intensity
        /// </summary>
        public const double DefaultMinNoise = 0;
        /// <summary>
        /// Seeker's default max noise intensity
        /// </summary>
        public const double DefaultMaxNoise = 0.1;
        /// <summary>
        /// Seeker's default number of steps within the interval
        /// </summary>
        public const int DefaultSteps = 10;

        //Attribute properties
        /// <summary>
        /// Number of attempts
        /// </summary>
        public int NumOfAttempts { get; }
        /// <summary>
        /// Number of attempt epochs
        /// </summary>
        public int NumOfAttemptEpochs { get; }
        /// <summary>
        /// Margin of noise values from zero
        /// </summary>
        public double NoiseZeroMargin { get; }
        /// <summary>
        /// Configuration of seeker of Noise hyperparameter value
        /// </summary>
        public ParamSeekerSettings NoiseSeekerCfg { get; }

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="numOfAttempts">Number of attempts</param>
        /// <param name="numOfAttemptEpochs">Number of attempt epochs</param>
        /// <param name="noiseZeroMargin">Margin of noise values from zero</param>
        /// <param name="noiseSeekerCfg">Configuration of seeker of MaxNoise hyperparameter value</param>
        public QRDRegrTrainerSettings(int numOfAttempts,
                                      int numOfAttemptEpochs,
                                      double noiseZeroMargin = DefaultNoiseZeroMargin,
                                      ParamSeekerSettings noiseSeekerCfg = null
                                      )
        {
            NumOfAttempts = numOfAttempts;
            NumOfAttemptEpochs = numOfAttemptEpochs;
            NoiseZeroMargin = noiseZeroMargin;
            if (noiseSeekerCfg == null)
            {
                NoiseSeekerCfg = new ParamSeekerSettings(DefaultMinNoise, DefaultMaxNoise, DefaultSteps);
            }
            else
            {
                NoiseSeekerCfg = (ParamSeekerSettings)noiseSeekerCfg.DeepClone();
            }
            Check();
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public QRDRegrTrainerSettings(QRDRegrTrainerSettings source)
        {
            NumOfAttempts = source.NumOfAttempts;
            NumOfAttemptEpochs = source.NumOfAttemptEpochs;
            NoiseZeroMargin = source.NoiseZeroMargin;
            NoiseSeekerCfg = (ParamSeekerSettings)source.NoiseSeekerCfg.DeepClone();
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// Content of xml element is always validated against the xml schema.
        /// </summary>
        /// <param name="elem">Xml data containing trainer settings</param>
        public QRDRegrTrainerSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            NumOfAttempts = int.Parse(settingsElem.Attribute("attempts").Value, CultureInfo.InvariantCulture);
            NumOfAttemptEpochs = int.Parse(settingsElem.Attribute("attemptEpochs").Value, CultureInfo.InvariantCulture);
            NoiseZeroMargin = double.Parse(settingsElem.Attribute("noiseZeroMargin").Value, CultureInfo.InvariantCulture);
            XElement noiseSeekerSettingsElem = settingsElem.Descendants("noise").FirstOrDefault();
            if (noiseSeekerSettingsElem != null)
            {
                NoiseSeekerCfg = new ParamSeekerSettings(noiseSeekerSettingsElem);
            }
            else
            {
                NoiseSeekerCfg = new ParamSeekerSettings(DefaultMinNoise, DefaultMaxNoise, DefaultSteps);
            }
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultNoiseZeroMargin { get { return (NoiseZeroMargin == DefaultNoiseZeroMargin); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultNoiseSeeker { get { return (NoiseSeekerCfg.Min == DefaultMinNoise && NoiseSeekerCfg.Max == DefaultMaxNoise && NoiseSeekerCfg.NumOfSteps == DefaultSteps); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return false; } }


        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (NumOfAttempts < 1)
            {
                throw new Exception($"Invalid NumOfAttempts {NumOfAttempts.ToString(CultureInfo.InvariantCulture)}. NumOfAttempts must be GE to 1.");
            }
            if (NumOfAttemptEpochs < 1)
            {
                throw new Exception($"Invalid NumOfAttemptEpochs {NumOfAttemptEpochs.ToString(CultureInfo.InvariantCulture)}. NumOfAttemptEpochs must be GE to 1.");
            }
            if (NoiseZeroMargin < 0 || NoiseZeroMargin >= 1)
            {
                throw new Exception($"Invalid NoiseZeroMargin {NoiseZeroMargin.ToString(CultureInfo.InvariantCulture)}. NoiseZeroMargin must be GE to 0 and LT 1.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new QRDRegrTrainerSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName, new XAttribute("attempts", NumOfAttempts.ToString(CultureInfo.InvariantCulture)),
                                                           new XAttribute("attemptEpochs", NumOfAttemptEpochs.ToString(CultureInfo.InvariantCulture)));
            if (!suppressDefaults || !IsDefaultNoiseZeroMargin)
            {
                rootElem.Add(new XAttribute("noiseZeroMargin", NoiseZeroMargin.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultNoiseSeeker)
            {
                rootElem.Add(NoiseSeekerCfg.GetXml("noise", suppressDefaults));
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
            return GetXml("qrdRegrTrainer", suppressDefaults);
        }

    }//QRDRegrTrainerSettings

}//Namespace
