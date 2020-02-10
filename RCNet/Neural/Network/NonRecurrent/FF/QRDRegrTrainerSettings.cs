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
    public class QRDRegrTrainerSettings : RCNetBaseSettings, INonRecurrentNetworkTrainerSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "FFNetQRDRegrTrainerCfgType";
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
        public const double DefaultMaxNoise = 0.05;
        /// <summary>
        /// Seeker's default number of steps within the interval
        /// </summary>
        public const int DefaultSteps = 10;

        //Attribute properties
        /// <summary>
        /// Number of attempts
        /// </summary>
        public int NumOfAttempts { get; set; }
        /// <summary>
        /// Number of attempt epochs
        /// </summary>
        public int NumOfAttemptEpochs { get; set; }
        /// <summary>
        /// Margin of noise values from zero
        /// </summary>
        public double NoiseZeroMargin { get; set; }
        /// <summary>
        /// Configuration of seeker of MaxNoise hyperparameter value
        /// </summary>
        public ParamSeekerSettings MaxNoiseSeekerCfg { get; set; }

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="numOfAttempts">Number of attempts</param>
        /// <param name="numOfAttemptEpochs">Number of attempt epochs</param>
        /// <param name="noiseZeroMargin">Margin of noise values from zero</param>
        /// <param name="maxNoiseSeekerCfg">Configuration of seeker of MaxNoise hyperparameter value</param>
        public QRDRegrTrainerSettings(int numOfAttempts,
                                      int numOfAttemptEpochs,
                                      double noiseZeroMargin = DefaultNoiseZeroMargin,
                                      ParamSeekerSettings maxNoiseSeekerCfg = null
                                      )
        {
            NumOfAttempts = numOfAttempts;
            NumOfAttemptEpochs = numOfAttemptEpochs;
            NoiseZeroMargin = noiseZeroMargin;
            if (maxNoiseSeekerCfg == null)
            {
                MaxNoiseSeekerCfg = new ParamSeekerSettings(DefaultMinNoise, DefaultMaxNoise, DefaultSteps);
            }
            else
            {
                MaxNoiseSeekerCfg = maxNoiseSeekerCfg.DeepClone();
            }
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
            MaxNoiseSeekerCfg = source.MaxNoiseSeekerCfg.DeepClone();
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
            MaxNoiseSeekerCfg = new ParamSeekerSettings(settingsElem.Descendants("maxNoise").First());
            return;
        }

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public INonRecurrentNetworkTrainerSettings DeepClone()
        {
            return new QRDRegrTrainerSettings(this);
        }

    }//QRDRegrTrainerSettings

}//Namespace
