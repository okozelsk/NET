using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Configuration of the input data resampling.
    /// </summary>
    [Serializable]
    public class ResamplingSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "NPResamplingType";
        /// <summary>
        /// The string code of the automatic number of target time-points.
        /// </summary>
        public const string AutoTargetTimePointsCode = "Auto";
        /// <summary>
        /// The numeric code of the automatic number of target time-points.
        /// </summary>
        public const int AutoTargetTimePointsNum = -1;
        //Default values
        /// <summary>
        /// The default threshold of signal begin detection.
        /// </summary>
        public const double DefaultSignalBeginThreshold = 0d;
        /// <summary>
        /// The default threshold of signal end detection.
        /// </summary>
        public const double DefaultSignalEndThreshold = 0d;
        /// <summary>
        /// The default value of the parameter specifying whether all the variables in the input pattern should have the same signal begin/end.
        /// </summary>
        public const bool DefaultUniformTimeScale = true;
        /// <summary>
        /// The default value of the parameter specifying whether the input pattern variable's data will be upsampled and/or downsampled to have specified fixed length (GT 0).
        /// </summary>
        public const int DefaultTargetTimePoints = AutoTargetTimePointsNum;

        //Attribute properties
        /// <summary>
        /// The threshold of the signal begin detection.
        /// </summary>
        public double SignalBeginThreshold { get; }

        /// <summary>
        /// The threshold of the signal end detection.
        /// </summary>
        public double SignalEndThreshold { get; }

        /// <summary>
        /// Specifies whether all the variables in the input pattern should have the same signal begin/end.
        /// </summary>
        public bool UniformTimeScale { get; }

        /// <summary>
        /// Specifies whether the input pattern variable's data will be upsampled and/or downsampled to have specified fixed length (GT 0).
        /// </summary>
        public int TargetTimePoints { get; }

        //Constructors
        /// <summary>
        /// Creates an itialized instance.
        /// </summary>
        /// <param name="signalBeginThreshold">The threshold of the signal begin detection.</param>
        /// <param name="signalEndThreshold">The threshold of the signal end detection.</param>
        /// <param name="uniformTimeScale">Specifies whether all the variables in the input pattern should have the same signal begin/end.</param>
        /// <param name="targetTimePoints">Specifies whether the input pattern variable's data will be upsampled and/or downsampled to have specified fixed length (GT 0).</param>
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
        /// <param name="source">The source instance.</param>
        public ResamplingSettings(ResamplingSettings source)
            : this(source.SignalBeginThreshold, source.SignalEndThreshold, source.UniformTimeScale, source.TargetTimePoints)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
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
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultSignalBeginThreshold { get { return (SignalBeginThreshold == DefaultSignalBeginThreshold); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultSignalEndThreshold { get { return (SignalEndThreshold == DefaultSignalEndThreshold); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultUniformTimeScale { get { return (UniformTimeScale == DefaultUniformTimeScale); } }

        /// <summary>
        /// Checks the defaults.
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

