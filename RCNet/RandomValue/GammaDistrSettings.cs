using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.RandomValue
{
    /// <summary>
    /// Configuration of the Gamma random distribution
    /// </summary>
    [Serializable]
    public class GammaDistrSettings : RCNetBaseSettings, IDistrSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "GammaDistrType";

        //Attributes
        /// <summary>
        /// Alpha, the shape parameter
        /// </summary>
        public double Alpha { get; }

        /// <summary>
        /// Beta, the rate parameter
        /// </summary>
        public double Beta { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="alpha">Shape parameter (alpha)</param>
        /// <param name="beta">Rate parameter (beta)</param>
        public GammaDistrSettings(double alpha, double beta)
        {
            Alpha = alpha;
            Beta = beta;
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public GammaDistrSettings(GammaDistrSettings source)
        {
            Alpha = source.Alpha;
            Beta = source.Beta;
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem"> Xml element containing the initialization settings.</param>
        public GammaDistrSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Alpha = double.Parse(settingsElem.Attribute("alpha").Value, CultureInfo.InvariantCulture);
            Beta = double.Parse(settingsElem.Attribute("beta").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return false; } }

        /// <inheritdoc />
        public RandomCommon.DistributionType Type { get { return RandomCommon.DistributionType.Gamma; } }


        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            if (Alpha <= 0)
            {
                throw new ArgumentException($"Incorrect Alpha ({Alpha.ToString(CultureInfo.InvariantCulture)}) value. Alpha must be GT 0.", "Alpha");
            }
            if (Beta <= 0)
            {
                throw new ArgumentException($"Incorrect Beta ({Beta.ToString(CultureInfo.InvariantCulture)}) value. Beta must be GT 0.", "Beta");
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new GammaDistrSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            return Validate(new XElement(rootElemName, new XAttribute("alpha", Alpha.ToString(CultureInfo.InvariantCulture)),
                                                       new XAttribute("beta", Beta.ToString(CultureInfo.InvariantCulture))),
                                                       XsdTypeName);
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml(RandomCommon.GetDistrElemName(Type), suppressDefaults);
        }

    }//GammaDistrSettings

}//Namespace
