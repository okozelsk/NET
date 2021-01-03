using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.RandomValue
{
    /// <summary>
    /// Configuration of the Exponential random distribution.
    /// </summary>
    [Serializable]
    public class ExponentialDistrSettings : RCNetBaseSettings, IDistrSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ExponentialDistrType";

        //Attributes
        /// <summary>
        /// The mean.
        /// </summary>
        public double Mean { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="mean">The mean.</param>
        public ExponentialDistrSettings(double mean)
        {
            Mean = mean;
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public ExponentialDistrSettings(ExponentialDistrSettings source)
        {
            Mean = source.Mean;
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem"> A xml element containing the configuration data.</param>
        public ExponentialDistrSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Mean = double.Parse(settingsElem.Attribute("mean").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }


        //Properties
        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return false; } }

        /// <inheritdoc />
        public RandomCommon.DistributionType Type { get { return RandomCommon.DistributionType.Exponential; } }


        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            if (Mean == 0)
            {
                throw new ArgumentException($"Incorrect Mean ({Mean.ToString(CultureInfo.InvariantCulture)}) value. Mean must not be EQ to 0.", "Mean");
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new ExponentialDistrSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            return Validate(new XElement(rootElemName, new XAttribute("mean", Mean.ToString(CultureInfo.InvariantCulture))), XsdTypeName);
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml(RandomCommon.GetDistrElemName(Type), suppressDefaults);
        }


    }//ExponentialDistrSettings

}//Namespace
