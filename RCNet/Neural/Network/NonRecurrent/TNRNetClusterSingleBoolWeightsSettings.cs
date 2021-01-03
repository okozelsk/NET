using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Configuration of the SingleBool cluster macro weights.
    /// </summary>
    [Serializable]
    public class TNRNetClusterSingleBoolWeightsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "SingleBoolTNetClusterWeightsType";
        //Default values
        /// <summary>
        /// The default value of the parameter specifying the weight of the group of metrics related to training.
        /// </summary>
        public const double DefaultTrainingGroupWeight = 1d;
        /// <summary>
        /// The default value of the parameter specifying the weight of the group of metrics related to testing.
        /// </summary>
        public const double DefaultTestingGroupWeight = 1d;
        /// <summary>
        /// The default value of the parameter specifying the weight of the number of samples metric.
        /// </summary>
        public const double DefaultSamplesWeight = 1d;
        /// <summary>
        /// The default value of the parameter specifying the weight of the numerical precision metric.
        /// </summary>
        public const double DefaultNumericalPrecisionWeight = 1d;
        /// <summary>
        /// The default value of the parameter specifying the weight of the misrecognized falses metric.
        /// </summary>
        public const double DefaultMisrecognizedFalseWeight = 1d;
        /// <summary>
        /// The default value of the parameter specifying the weight of the unrecognized trues metric.
        /// </summary>
        public const double DefaultUnrecognizedTrueWeight = 0d;

        //Attribute properties
        /// <summary>
        /// Specifies the weight of the group of metrics related to training.
        /// </summary>
        public double TrainingGroupWeight { get; }

        /// <summary>
        /// Specifies the weight of the group of metrics related to testing.
        /// </summary>
        public double TestingGroupWeight { get; }

        /// <summary>
        /// Specifies the weight of the number of samples metric.
        /// </summary>
        public double SamplesWeight { get; }

        /// <summary>
        /// Specifies the weight of the numerical precision metric.
        /// </summary>
        public double NumericalPrecisionWeight { get; }

        /// <summary>
        /// Specifies the weight of the misrecognized falses metric.
        /// </summary>
        public double MisrecognizedFalseWeight { get; }

        /// <summary>
        /// Specifies the weight of the unrecognized trues metric.
        /// </summary>
        public double UnrecognizedTrueWeight { get; }


        //Constructors
        /// <summary>
        /// Creates an itialized instance.
        /// </summary>
        /// <param name="trainingGroupWeight">Specifies the weight of the group of metrics related to training.</param>
        /// <param name="testingGroupWeight">Specifies the weight of the group of metrics related to testing.</param>
        /// <param name="samplesWeight">Specifies the weight of the number of samples metric.</param>
        /// <param name="numericalPrecisionWeight">Specifies the weight of the numerical precision metric.</param>
        /// <param name="misrecognizedFalseWeight">Specifies the weight of the misrecognized falses metric.</param>
        /// <param name="unrecognizedTrueWeight">Specifies the weight of the unrecognized trues metric.</param>
        public TNRNetClusterSingleBoolWeightsSettings(double trainingGroupWeight = DefaultTrainingGroupWeight,
                                                      double testingGroupWeight = DefaultTestingGroupWeight,
                                                      double samplesWeight = DefaultSamplesWeight,
                                                      double numericalPrecisionWeight = DefaultNumericalPrecisionWeight,
                                                      double misrecognizedFalseWeight = DefaultMisrecognizedFalseWeight,
                                                      double unrecognizedTrueWeight = DefaultUnrecognizedTrueWeight
                                                      )
        {
            TrainingGroupWeight = trainingGroupWeight;
            TestingGroupWeight = testingGroupWeight;
            SamplesWeight = samplesWeight;
            NumericalPrecisionWeight = numericalPrecisionWeight;
            MisrecognizedFalseWeight = misrecognizedFalseWeight;
            UnrecognizedTrueWeight = unrecognizedTrueWeight;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public TNRNetClusterSingleBoolWeightsSettings(TNRNetClusterSingleBoolWeightsSettings source)
            : this(source.TrainingGroupWeight, source.TestingGroupWeight, source.SamplesWeight,
                   source.NumericalPrecisionWeight, source.MisrecognizedFalseWeight,
                   source.UnrecognizedTrueWeight)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public TNRNetClusterSingleBoolWeightsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            TrainingGroupWeight = double.Parse(settingsElem.Attribute("trainingGroupWeight").Value, CultureInfo.InvariantCulture);
            TestingGroupWeight = double.Parse(settingsElem.Attribute("testingGroupWeight").Value, CultureInfo.InvariantCulture);
            SamplesWeight = double.Parse(settingsElem.Attribute("samplesWeight").Value, CultureInfo.InvariantCulture);
            NumericalPrecisionWeight = double.Parse(settingsElem.Attribute("numericalPrecisionWeight").Value, CultureInfo.InvariantCulture);
            MisrecognizedFalseWeight = double.Parse(settingsElem.Attribute("misrecognizedFalseWeight").Value, CultureInfo.InvariantCulture);
            UnrecognizedTrueWeight = double.Parse(settingsElem.Attribute("unrecognizedTrueWeight").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultTrainingGroupWeight { get { return (TrainingGroupWeight == DefaultTrainingGroupWeight); } }
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultTestingGroupWeight { get { return (TestingGroupWeight == DefaultTestingGroupWeight); } }
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultSamplesWeight { get { return (SamplesWeight == DefaultSamplesWeight); } }
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultNumericalPrecisionWeight { get { return (NumericalPrecisionWeight == DefaultNumericalPrecisionWeight); } }
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultMisrecognizedFalseWeight { get { return (MisrecognizedFalseWeight == DefaultMisrecognizedFalseWeight); } }
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultUnrecognizedTrueWeight { get { return (UnrecognizedTrueWeight == DefaultUnrecognizedTrueWeight); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultTrainingGroupWeight &&
                       IsDefaultTestingGroupWeight &&
                       IsDefaultSamplesWeight &&
                       IsDefaultNumericalPrecisionWeight &&
                       IsDefaultMisrecognizedFalseWeight &&
                       IsDefaultUnrecognizedTrueWeight;
            }
        }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (TrainingGroupWeight < 0d)
            {
                throw new ArgumentException("TrainingGroupWeight must be GE 0.", "TrainingGroupWeight");
            }
            if (TestingGroupWeight < 0d)
            {
                throw new ArgumentException("TestingGroupWeight must be GE 0.", "TestingGroupWeight");
            }
            if (SamplesWeight < 0d)
            {
                throw new ArgumentException("SamplesWeight must be GE 0.", "SamplesWeight");
            }
            if (NumericalPrecisionWeight < 0d)
            {
                throw new ArgumentException("NumericalPrecisionWeight must be GE 0.", "NumericalPrecisionWeight");
            }
            if (MisrecognizedFalseWeight < 0d)
            {
                throw new ArgumentException("MisrecognizedFalseWeight must be GE 0.", "MisrecognizedFalseWeight");
            }
            if (UnrecognizedTrueWeight < 0d)
            {
                throw new ArgumentException("UnrecognizedTrueWeight must be GE 0.", "UnrecognizedTrueWeight");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new TNRNetClusterSingleBoolWeightsSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultTrainingGroupWeight)
            {
                rootElem.Add(new XAttribute("trainingGroupWeight", TrainingGroupWeight.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultTestingGroupWeight)
            {
                rootElem.Add(new XAttribute("testingGroupWeight", TestingGroupWeight.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultSamplesWeight)
            {
                rootElem.Add(new XAttribute("samplesWeight", SamplesWeight.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultNumericalPrecisionWeight)
            {
                rootElem.Add(new XAttribute("numericalPrecisionWeight", NumericalPrecisionWeight.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultUnrecognizedTrueWeight)
            {
                rootElem.Add(new XAttribute("unrecognizedTrueWeight", UnrecognizedTrueWeight.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultMisrecognizedFalseWeight)
            {
                rootElem.Add(new XAttribute("misrecognizedFalseWeight", NumericalPrecisionWeight.ToString(CultureInfo.InvariantCulture)));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("weights", suppressDefaults);
        }

    }//TNRNetClusterSingleBoolWeightsSettings

}//Namespace

