using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RCNet.RandomValue
{
    /// <summary>
    /// Exponential distribution parameters
    /// </summary>
    [Serializable]
    public class ExponentialDistrSettings : RCNetBaseSettings, IDistrSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ExponentialDistrCfgType";

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
        public ExponentialDistrSettings(double mean)
        {
            Mean = mean;
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ExponentialDistrSettings(ExponentialDistrSettings source)
        {
            Mean = source.Mean;
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem"> Xml data containing settings.</param>
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
        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return false; } }

        /// <summary>
        /// Type of random distribution
        /// </summary>
        public RandomCommon.DistributionType Type { get { return RandomCommon.DistributionType.Exponential; } }


        //Methods
        private void Check()
        {
            if (Mean == 0)
            {
                throw new Exception($"Incorrect Mean ({Mean.ToString(CultureInfo.InvariantCulture)}) value. Mean must not be EQ to 0.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new ExponentialDistrSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            return Validate(new XElement(rootElemName, new XAttribute("mean", Mean.ToString(CultureInfo.InvariantCulture))), XsdTypeName);
        }

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml(RandomCommon.GetDistrElemName(Type), suppressDefaults);
        }


    }//ExponentialDistrSettings

}//Namespace
