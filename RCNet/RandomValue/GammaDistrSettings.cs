using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.RandomValue
{
    /// <summary>
    /// Gamma distribution parameters
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
        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return false; } }

        /// <summary>
        /// Type of random distribution
        /// </summary>
        public RandomCommon.DistributionType Type { get { return RandomCommon.DistributionType.Gamma; } }


        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
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

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new GammaDistrSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            return Validate(new XElement(rootElemName, new XAttribute("alpha", Alpha.ToString(CultureInfo.InvariantCulture)),
                                                       new XAttribute("beta", Beta.ToString(CultureInfo.InvariantCulture))),
                                                       XsdTypeName);
        }

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml(RandomCommon.GetDistrElemName(Type), suppressDefaults);
        }

    }//GammaDistrSettings

}//Namespace
