using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Configuration of the Real cluster macro weights.
    /// </summary>
    [Serializable]
    public class TNRNetClusterRealWeightsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "RealTNetClusterWeightsType";
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


        //Constructors
        /// <summary>
        /// Creates an itialized instance.
        /// </summary>
        /// <param name="trainingGroupWeight">Specifies the weight of the group of metrics related to training.</param>
        /// <param name="testingGroupWeight">Specifies the weight of the group of metrics related to testing.</param>
        /// <param name="samplesWeight">Specifies the weight of the number of samples metric.</param>
        /// <param name="numericalPrecisionWeight">Specifies the weight of the numerical precision metric.</param>
        public TNRNetClusterRealWeightsSettings(double trainingGroupWeight = DefaultTrainingGroupWeight,
                                                      double testingGroupWeight = DefaultTestingGroupWeight,
                                                      double samplesWeight = DefaultSamplesWeight,
                                                      double numericalPrecisionWeight = DefaultNumericalPrecisionWeight
                                                      )
        {
            TrainingGroupWeight = trainingGroupWeight;
            TestingGroupWeight = testingGroupWeight;
            SamplesWeight = samplesWeight;
            NumericalPrecisionWeight = numericalPrecisionWeight;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public TNRNetClusterRealWeightsSettings(TNRNetClusterRealWeightsSettings source)
            : this(source.TrainingGroupWeight, source.TestingGroupWeight, source.SamplesWeight,
                   source.NumericalPrecisionWeight)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public TNRNetClusterRealWeightsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            TrainingGroupWeight = double.Parse(settingsElem.Attribute("trainingGroupWeight").Value, CultureInfo.InvariantCulture);
            TestingGroupWeight = double.Parse(settingsElem.Attribute("testingGroupWeight").Value, CultureInfo.InvariantCulture);
            SamplesWeight = double.Parse(settingsElem.Attribute("samplesWeight").Value, CultureInfo.InvariantCulture);
            NumericalPrecisionWeight = double.Parse(settingsElem.Attribute("numericalPrecisionWeight").Value, CultureInfo.InvariantCulture);
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

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultTrainingGroupWeight &&
                       IsDefaultTestingGroupWeight &&
                       IsDefaultSamplesWeight &&
                       IsDefaultNumericalPrecisionWeight;
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
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new TNRNetClusterRealWeightsSettings(this);
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
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("weights", suppressDefaults);
        }

    }//TNRNetClusterRealWeightsSettings

}//Namespace

