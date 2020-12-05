using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Configuration of the Resampling
    /// </summary>
    [Serializable]
    public class ResamplingSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPResamplingType";
        /// <summary>
        /// Automatic number of time-points of the resampled pattern (code)
        /// </summary>
        public const string AutoTargetTimePointsCode = "Auto";
        /// <summary>
        /// Automatic number of time-points of the resampled pattern (num)
        /// </summary>
        public const int AutoTargetTimePointsNum = -1;
        //Default values
        /// <summary>
        /// Default threshold of signal begin detection
        /// </summary>
        public const double DefaultSignalBeginThreshold = 0d;
        /// <summary>
        /// Default threshold of signal end detection
        /// </summary>
        public const double DefaultSignalEndThreshold = 0d;
        /// <summary>
        /// Default value of the parameter specifying if to keep the same time scale over all input patterns
        /// </summary>
        public const bool DefaultUniformTimeScale = true;
        /// <summary>
        /// Default value of parameter specifying number of time-points of the resampled pattern (resampled pattern length)
        /// </summary>
        public const int DefaultTargetTimePoints = AutoTargetTimePointsNum;

        //Attribute properties
        /// <summary>
        /// Threshold of signal begin detection
        /// </summary>
        public double SignalBeginThreshold { get; }

        /// <summary>
        /// Threshold of signal end detection
        /// </summary>
        public double SignalEndThreshold { get; }

        /// <summary>
        /// Specifies whether to keep the same time scale over all input patterns
        /// </summary>
        public bool UniformTimeScale { get; }

        /// <summary>
        /// Number of time-points of the resampled pattern (resampled pattern length)
        /// </summary>
        public int TargetTimePoints { get; }

        //Constructors
        /// <summary>
        /// Creates an itialized instance.
        /// </summary>
        /// <param name="signalBeginThreshold">Threshold of signal begin detection</param>
        /// <param name="signalEndThreshold">Threshold of signal end detection</param>
        /// <param name="uniformTimeScale">Specifies whether to keep the same time scale over all input patterns</param>
        /// <param name="targetTimePoints">Number of time-points of the resampled pattern (resampled pattern length)</param>
        public ResamplingSettings(double signalBeginThreshold = DefaultSignalBeginThreshold,
                                  double signalEndThreshold = DefaultSignalEndThreshold,
                                  bool uniformTimeScale = DefaultUniformTimeScale,
                                  int targetTimePoints = DefaultTargetTimePoints
                                  )
        {
            SignalBeginThreshold = signalBeginThreshold;
            SignalEndThreshold = signalEndThreshold;
            UniformTimeScale = uniformTimeScale;
            TargetTimePoints = targetTimePoints;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public ResamplingSettings(ResamplingSettings source)
            : this(source.SignalBeginThreshold, source.SignalEndThreshold, source.UniformTimeScale, source.TargetTimePoints)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance from the given xml element.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public ResamplingSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            SignalBeginThreshold = double.Parse(settingsElem.Attribute("signalBeginThreshold").Value, CultureInfo.InvariantCulture);
            SignalEndThreshold = double.Parse(settingsElem.Attribute("signalEndThreshold").Value, CultureInfo.InvariantCulture);
            UniformTimeScale = bool.Parse(settingsElem.Attribute("uniformTimeScale").Value);
            string targetTimePoints = settingsElem.Attribute("targetTimePoints").Value;
            TargetTimePoints = targetTimePoints == AutoTargetTimePointsCode ? AutoTargetTimePointsNum : int.Parse(targetTimePoints, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultSignalBeginThreshold { get { return (SignalBeginThreshold == DefaultSignalBeginThreshold); } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultSignalEndThreshold { get { return (SignalEndThreshold == DefaultSignalEndThreshold); } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultUniformTimeScale { get { return (UniformTimeScale == DefaultUniformTimeScale); } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultTargetTimePoints { get { return (TargetTimePoints == DefaultTargetTimePoints); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultSignalBeginThreshold &&
                       IsDefaultSignalEndThreshold &&
                       IsDefaultUniformTimeScale &&
                       IsDefaultTargetTimePoints;
            }
        }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (SignalBeginThreshold < 0)
            {
                throw new ArgumentException($"Invalid SignalBeginThreshold {SignalBeginThreshold.ToString(CultureInfo.InvariantCulture)}. SignalBeginThreshold must be GE to 0.", "SignalBeginThreshold");
            }
            if (SignalEndThreshold < 0)
            {
                throw new ArgumentException($"Invalid SignalEndThreshold {SignalEndThreshold.ToString(CultureInfo.InvariantCulture)}. SignalEndThreshold must be GE to 0.", "SignalEndThreshold");
            }
            if (TargetTimePoints != AutoTargetTimePointsNum && TargetTimePoints <= 0)
            {
                throw new ArgumentException($"Invalid TargetTimePoints {TargetTimePoints.ToString(CultureInfo.InvariantCulture)}. TargetTimePoints must be equal to {AutoTargetTimePointsNum.ToString(CultureInfo.InvariantCulture)} for automatic target time points or GT 0.", "TargetTimePoints");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new ResamplingSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultSignalBeginThreshold)
            {
                rootElem.Add(new XAttribute("signalBeginThreshold", SignalBeginThreshold.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultSignalEndThreshold)
            {
                rootElem.Add(new XAttribute("signalEndThreshold", SignalEndThreshold.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultUniformTimeScale)
            {
                rootElem.Add(new XAttribute("uniformTimeScale", UniformTimeScale.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
            }

            if (!suppressDefaults || !IsDefaultTargetTimePoints)
            {
                rootElem.Add(new XAttribute("targetTimePoints", TargetTimePoints == AutoTargetTimePointsNum ? AutoTargetTimePointsCode : TargetTimePoints.ToString(CultureInfo.InvariantCulture)));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("resampling", suppressDefaults);
        }

    }//ResamplingSettings

}//Namespace

