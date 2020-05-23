using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Xml.Linq;
using System.Reflection;
using RCNet.Extensions;
using RCNet.XmlTools;

namespace RCNet.RandomValue
{
    /// <summary>
    /// Class specifies properties of randomly generated values
    /// </summary>
    [Serializable]
    public class RandomValueSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "RandomValueType";

        //Default values
        /// <summary>
        /// Default value of RandomSign
        /// </summary>
        public const bool DefaultRandomSignValue = false;
        /// <summary>
        /// Default type of distribution
        /// </summary>
        public const RandomCommon.DistributionType DefaultDistributionType = RandomCommon.DistributionType.Uniform;

        //Attribute properties
        /// <summary>
        /// Min random value
        /// </summary>
        public double Min { get; }

        /// <summary>
        /// Max random value
        /// </summary>
        public double Max { get; }
        
        /// <summary>
        /// Specifies whether to randomize value sign
        /// </summary>
        public bool RandomSign { get; }
        
        /// <summary>
        /// Distribution parameters
        /// </summary>
        public IDistrSettings DistrCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="min">Min random value</param>
        /// <param name="max">Max random value</param>
        /// <param name="randomSign">Specifies whether to randomize value sign</param>
        /// <param name="distrCfg">Specific parameters of the distribution to be used</param>
        public RandomValueSettings(double min,
                                   double max,
                                   bool randomSign = DefaultRandomSignValue,
                                   IDistrSettings distrCfg = null
                                   )
        {
            Min = min;
            Max = max;
            RandomSign = randomSign;
            DistrCfg = distrCfg;
            if(DistrCfg == null)
            {
                DistrCfg = new UniformDistrSettings();
            }
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public RandomValueSettings(RandomValueSettings source)
        {
            Min = source.Min;
            Max = source.Max;
            RandomSign = source.RandomSign;
            DistrCfg = (IDistrSettings)((RCNetBaseSettings)source.DistrCfg).DeepClone();
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing RandomValueSettings settings.</param>
        public RandomValueSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Min = double.Parse(settingsElem.Attribute("min").Value, CultureInfo.InvariantCulture);
            Max = double.Parse(settingsElem.Attribute("max").Value, CultureInfo.InvariantCulture);
            RandomSign = bool.Parse(settingsElem.Attribute("randomSign").Value);
            XElement distrParamsElem = settingsElem.Elements().FirstOrDefault();
            if (distrParamsElem == null)
            {
                DistrCfg = new UniformDistrSettings();
            }
            else
            {
                DistrCfg = RandomCommon.CreateDistrSettings(distrParamsElem);
            }
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultDistrType { get { return DistrType == RandomCommon.DistributionType.Uniform; } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return false; } }

        /// <summary>
        /// Type of used random distribution
        /// </summary>
        public RandomCommon.DistributionType DistrType { get { return DistrCfg.Type; } }

        //Methods
        //Static methods
        /// <summary>
        /// If exists descendant element within the root element then function creates instance of the RandomValueSettings using
        /// descendant's xml settings. If not, function creates instance of the RandomValueSettings using specified default parameters.
        /// </summary>
        public static RandomValueSettings LoadOrDefault(XElement rootElem, string descendant, double defaultMin, double defaultMax, bool randomSign = false)
        {
            XElement descendantElement = rootElem.Elements(descendant).FirstOrDefault();
            if (descendantElement != null)
            {
                return new RandomValueSettings(descendantElement);
            }
            else
            {
                return new RandomValueSettings(defaultMin, defaultMax, randomSign);
            }
        }

        /// <summary>
        /// If exists descendant element within the root element then function creates instance of the RandomValueSettings using
        /// descendant's xml settings. If not, function creates instance of the RandomValueSettings using specified default parameters.
        /// </summary>
        public static RandomValueSettings LoadOrDefault(XElement rootElem, string descendant, double defaultConst, bool randomSign = false)
        {
            return LoadOrDefault(rootElem, descendant, defaultConst, defaultConst, randomSign);
        }

        /// <summary>
        /// If source is not null then function creates it's clone. If not, function creates instance of the RandomValueSettings using specified default parameters.
        /// </summary>
        public static RandomValueSettings CloneOrDefault(RandomValueSettings source, double defaultMin, double defaultMax, bool randomSign = false)
        {
            if(source == null)
            {
                return new RandomValueSettings(defaultMin, defaultMax, randomSign);
            }
            else
            {
                return (RandomValueSettings)source.DeepClone();
            }
        }

        /// <summary>
        /// If source is not null then function creates it's clone. If not, function creates instance of the RandomValueSettings using specified default parameters.
        /// </summary>
        public static RandomValueSettings CloneOrDefault(RandomValueSettings source, double defaultConst, bool randomSign = false)
        {
            return CloneOrDefault(source, defaultConst, defaultConst, randomSign);
        }

        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            if (Max < Min)
            {
                throw new ArgumentException($"Incorrect Min ({Min.ToString(CultureInfo.InvariantCulture)}) and Max ({Max.ToString(CultureInfo.InvariantCulture)}) values. Max must be GE to Min.", "Max/Min");
            }
            return;
        }
        
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new RandomValueSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName, new XAttribute("min", Min.ToString(CultureInfo.InvariantCulture)),
                                                           new XAttribute("max", Max.ToString(CultureInfo.InvariantCulture)));
            if(!suppressDefaults || RandomSign != DefaultRandomSignValue)
            {
                rootElem.Add(new XAttribute("randomSign", RandomSign.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
            }
            if (!suppressDefaults || DistrType != RandomCommon.DistributionType.Uniform)
            {
                rootElem.Add(((RCNetBaseSettings)DistrCfg).GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

    }//RandomValueSettings

}//Namespace
