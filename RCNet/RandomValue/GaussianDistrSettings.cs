using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.RandomValue
{
    /// <summary>
    /// Configuration of the Gaussian random distribution.
    /// </summary>
    [Serializable]
    public class GaussianDistrSettings : RCNetBaseSettings, IDistrSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "GaussianDistrType";
        //Default values
        /// <summary>
        /// The default value of the mean.
        /// </summary>
        public const double DefaultMeanValue = 0d;
        /// <summary>
        /// The default value of the standard deviation.
        /// </summary>
        public const double DefaultStdDevValue = 1d;

        //Attributes
        /// <summary>
        /// The mean.
        /// </summary>
        public double Mean { get; }

        /// <summary>
        /// The standard deviation.
        /// </summary>
        public double StdDev { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="mean">The mean.</param>
        /// <param name="stdDev">The standard deviation.</param>
        public GaussianDistrSettings(double mean = DefaultMeanValue, double stdDev = DefaultStdDevValue)
        {
            Mean = mean;
            StdDev = stdDev;
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public GaussianDistrSettings(GaussianDistrSettings source)
        {
            Mean = source.Mean;
            StdDev = source.StdDev;
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem"> A xml element containing the configuration data.</param>
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
