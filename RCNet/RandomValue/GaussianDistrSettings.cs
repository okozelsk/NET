using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.RandomValue
{
    /// <summary>
    /// Configuration of the Gaussian random distribution
    /// </summary>
    [Serializable]
    public class GaussianDistrSettings : RCNetBaseSettings, IDistrSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "GaussianDistrType";
        //Default values
        /// <summary>
        /// Default value of Mean
        /// </summary>
        public const double DefaultMeanValue = 0d;
        /// <summary>
        /// Default value of StdDev
        /// </summary>
        public const double DefaultStdDevValue = 1d;

        //Attributes
        /// <summary>
        /// Mean
        /// </summary>
        public double Mean { get; }

        /// <summary>
        /// Standard deviation
        /// </summary>
        public double StdDev { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="mean">Mean</param>
        /// <param name="stdDev">Standard deviation</param>
        public GaussianDistrSettings(double mean = DefaultMeanValue, double stdDev = DefaultStdDevValue)
        {
            Mean = mean;
            StdDev = stdDev;
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public GaussianDistrSettings(GaussianDistrSettings source)
        {
            Mean = source.Mean;
            StdDev = source.StdDev;
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem"> Xml element containing the initialization settings.</param>
        public GaussianDistrSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Mean = double.Parse(settingsElem.Attribute("mean").Value, CultureInfo.InvariantCulture);
            StdDev = double.Parse(settingsElem.Attribute("stdDev").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return (Mean == DefaultMeanValue && StdDev == DefaultStdDevValue); } }

        /// <inheritdoc />
        public RandomCommon.DistributionType Type { get { return RandomCommon.DistributionType.Gaussian; } }


        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            if (StdDev <= 0)
            {
                throw new ArgumentException($"Incorrect StdDev ({StdDev.ToString(CultureInfo.InvariantCulture)}) value. StdDev must be GT 0.", "StdDev");
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new GaussianDistrSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || Mean != DefaultMeanValue)
            {
                rootElem.Add(new XAttribute("mean", Mean.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || StdDev != DefaultStdDevValue)
            {
                rootElem.Add(new XAttribute("stdDev", StdDev.ToString(CultureInfo.InvariantCulture)));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml(RandomCommon.GetDistrElemName(Type), suppressDefaults);
        }

    }//GaussianDistrSettings

}//Namespace
