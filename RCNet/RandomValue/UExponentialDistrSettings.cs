using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.RandomValue
{
    /// <summary>
    /// Configuration of the Exponential random distribution (unsigned)
    /// </summary>
    [Serializable]
    public class UExponentialDistrSettings : RCNetBaseSettings, IDistrSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "UExponentialDistrType";

        //Attributes
        /// <summary>
        /// Mean
        /// </summary>
        public double Mean { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="mean">Mean</param>
        public UExponentialDistrSettings(double mean)
        {
            Mean = mean;
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public UExponentialDistrSettings(UExponentialDistrSettings source)
        {
            Mean = source.Mean;
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem"> Xml element containing the initialization settings.</param>
        public UExponentialDistrSettings(XElement elem)
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
            if (Mean <= 0)
            {
                throw new ArgumentException($"Incorrect Mean ({Mean.ToString(CultureInfo.InvariantCulture)}) value. Mean must be GT 0.", "Mean");
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new UExponentialDistrSettings(this);
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


    }//UExponentialDistrSettings

}//Namespace
