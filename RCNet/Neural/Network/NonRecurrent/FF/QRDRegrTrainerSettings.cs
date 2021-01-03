using RCNet.MathTools;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.NonRecurrent.FF
{
    /// <summary>
    /// Configuration of the QRDRegrTrainer.
    /// </summary>
    [Serializable]
    public class QRDRegrTrainerSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "FFNetQRDRegrTrainerType";
        //Default values
        /// <summary>
        /// The default zero-margin of the noise.
        /// </summary>
        public const double DefaultNoiseZeroMargin = 0.75;
        /// <summary>
        /// The default min noise intensity.
        /// </summary>
        public const double DefaultMinNoise = 0;
        /// <summary>
        /// The default max noise intensity.
        /// </summary>
        public const double DefaultMaxNoise = 0.1;
        /// <summary>
        /// The default number of steps within the interval.
        /// </summary>
        public const int DefaultSteps = 10;

        //Attribute properties
        /// <summary>
        /// The number of attempts.
        /// </summary>
        public int NumOfAttempts { get; }
        /// <summary>
        /// The number of attempt epochs.
        /// </summary>
        public int NumOfAttemptEpochs { get; }
        /// <summary>
        /// The zero-margin of the noise.
        /// </summary>
        public double NoiseZeroMargin { get; }
        /// <summary>
        /// The configuration of the noise parameter value finder.
        /// </summary>
        public ParamValFinderSettings NoiseFinderCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="numOfAttempts">The number of attempts.</param>
        /// <param name="numOfAttemptEpochs">The number of attempt epochs.</param>
        /// <param name="noiseZeroMargin">The zero-margin of the noise.</param>
        /// <param name="noiseFinderCfg">The configuration of the noise parameter value finder.</param>
        public QRDRegrTrainerSettings(int numOfAttempts,
                                      int numOfAttemptEpochs,
                                      double noiseZeroMargin = DefaultNoiseZeroMargin,
                                      ParamValFinderSettings noiseFinderCfg = null
                                      )
        {
            NumOfAttempts = numOfAttempts;
            NumOfAttemptEpochs = numOfAttemptEpochs;
            NoiseZeroMargin = noiseZeroMargin;
            if (noiseFinderCfg == null)
            {
                NoiseFinderCfg = new ParamValFinderSettings(DefaultMinNoise, DefaultMaxNoise, DefaultSteps);
            }
            else
            {
                NoiseFinderCfg = (ParamValFinderSettings)noiseFinderCfg.DeepClone();
            }
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public QRDRegrTrainerSettings(QRDRegrTrainerSettings source)
        {
            NumOfAttempts = source.NumOfAttempts;
            NumOfAttemptEpochs = source.NumOfAttemptEpochs;
            NoiseZeroMargin = source.NoiseZeroMargin;
            NoiseFinderCfg = (ParamValFinderSettings)source.NoiseFinderCfg.DeepClone();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public QRDRegrTrainerSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            NumOfAttempts = int.Parse(settingsElem.Attribute("attempts").Value, CultureInfo.InvariantCulture);
            NumOfAttemptEpochs = int.Parse(settingsElem.Attribute("attemptEpochs").Value, CultureInfo.InvariantCulture);
            NoiseZeroMargin = double.Parse(settingsElem.Attribute("noiseZeroMargin").Value, CultureInfo.InvariantCulture);
            XElement noiseSeekerSettingsElem = settingsElem.Elements("noise").FirstOrDefault();
            if (noiseSeekerSettingsElem != null)
            {
                NoiseFinderCfg = new ParamValFinderSettings(noiseSeekerSettingsElem);
            }
            else
            {
                NoiseFinderCfg = new ParamValFinderSettings(DefaultMinNoise, DefaultMaxNoise, DefaultSteps);
            }
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultNoiseZeroMargin { get { return (NoiseZeroMargin == DefaultNoiseZeroMargin); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultNoiseSeeker { get { return (NoiseFinderCfg.Min == DefaultMinNoise && NoiseFinderCfg.Max == DefaultMaxNoise && NoiseFinderCfg.NumOfSubIntervals == DefaultSteps); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }


        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (NumOfAttempts < 1)
            {
                throw new ArgumentException($"Invalid NumOfAttempts {NumOfAttempts.ToString(CultureInfo.InvariantCulture)}. NumOfAttempts must be GE to 1.", "NumOfAttempts");
            }
            if (NumOfAttemptEpochs < 1)
            {
                throw new ArgumentException($"Invalid NumOfAttemptEpochs {NumOfAttemptEpochs.ToString(CultureInfo.InvariantCulture)}. NumOfAttemptEpochs must be GE to 1.", "NumOfAttemptEpochs");
            }
            if (NoiseZeroMargin < 0 || NoiseZeroMargin >= 1)
            {
                throw new ArgumentException($"Invalid NoiseZeroMargin {NoiseZeroMargin.ToString(CultureInfo.InvariantCulture)}. NoiseZeroMargin must be GE to 0 and LT 1.", "NoiseZeroMargin");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new QRDRegrTrainerSettings(this);
        }

        /// <inheritdoc/>
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
                rootElem.Add(NoiseFinderCfg.GetXml("noise", suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("qrdRegrTrainer", suppressDefaults);
        }

    }//QRDRegrTrainerSettings

}//Namespace
