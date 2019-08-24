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
    public class RandomValueSettings
    {
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
        /// Specifies what distribution to use
        /// </summary>
        public RandomClassExtensions.DistributionType DistrType { get; }

        /// <summary>
        /// Gaussian distribution parameters
        /// </summary>
        public GaussianDistrSettings GaussianDistrCfg { get; set; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="min">Min random value</param>
        /// <param name="max">Max random value</param>
        /// <param name="randomSign">Specifies whether to randomize value sign</param>
        /// <param name="distrType">Specifies what distribution to use</param>
        /// <param name="gaussianDistrCfg">Specifies gaussian distribution parameters</param>
        public RandomValueSettings(double min = -1,
                                    double max = 1,
                                    bool randomSign = false,
                                    RandomClassExtensions.DistributionType distrType = RandomClassExtensions.DistributionType.Uniform,
                                    GaussianDistrSettings gaussianDistrCfg = null
                                    )
        {
            Min = min;
            Max = max;
            RandomSign = randomSign;
            DistrType = distrType;
            GaussianDistrCfg = gaussianDistrCfg;
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
            DistrType = source.DistrType;
            GaussianDistrCfg = null;
            if (source.GaussianDistrCfg != null)
            {
                GaussianDistrCfg = source.GaussianDistrCfg.DeepClone();
            }
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing RandomValueSettings settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public RandomValueSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RandomValue.RandomValueSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement randomValueSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            Min = double.Parse(randomValueSettingsElem.Attribute("min").Value, CultureInfo.InvariantCulture);
            Max = double.Parse(randomValueSettingsElem.Attribute("max").Value, CultureInfo.InvariantCulture);
            RandomSign = bool.Parse(randomValueSettingsElem.Attribute("randomSign").Value);
            DistrType = RandomClassExtensions.ParseDistributionType(randomValueSettingsElem.Attribute("distribution").Value);
            //Gaussian parameters
            GaussianDistrCfg = null;
            XElement gaussianParamsElem = randomValueSettingsElem.Descendants("gaussianDistr").FirstOrDefault();
            if(gaussianParamsElem != null)
            {
                GaussianDistrCfg = new GaussianDistrSettings(gaussianParamsElem);
            }
            Check();
            return;
        }

        //Methods
        //Static methods
        /// <summary>
        /// If exists descendant element within the root element then function creates instance of the RandomValueSettings using
        /// descendant's xml settings. If not, function creates instance of the RandomValueSettings using specified default parameters.
        /// </summary>
        public static RandomValueSettings LoadOrDefault(XElement rootElem, string descendant, double defaultMin, double defaultMax, bool randomSign = false)
        {
            XElement descendantElement = rootElem.Descendants(descendant).FirstOrDefault();
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
                return source.DeepClone();
            }
        }

        /// <summary>
        /// If source is not null then function creates it's clone. If not, function creates instance of the RandomValueSettings using specified default parameters.
        /// </summary>
        public static RandomValueSettings CloneOrDefault(RandomValueSettings source, double defaultConst, bool randomSign = false)
        {
            return CloneOrDefault(source, defaultConst, defaultConst, randomSign);
        }

        //Instance methods
        private void Check()
        {
            if(Max < Min)
            {
                throw new Exception($"Incorrect min ({Min.ToString(CultureInfo.InvariantCulture)}) and max ({Max.ToString(CultureInfo.InvariantCulture)}) values. Max must be GE to min.");
            }
            return;
        }
        
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            RandomValueSettings cmpSettings = obj as RandomValueSettings;
            if (Min != cmpSettings.Min ||
                Max != cmpSettings.Max ||
                RandomSign != cmpSettings.RandomSign ||
                DistrType != cmpSettings.DistrType ||
                !Equals(GaussianDistrCfg, cmpSettings.GaussianDistrCfg)
                )
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// See the base.
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public RandomValueSettings DeepClone()
        {
            return new RandomValueSettings(this);
        }

        //Inner classes
        /// <summary>
        /// Gaussian distribution parameters
        /// </summary>
        [Serializable]
        public class GaussianDistrSettings
        {
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
            public GaussianDistrSettings(double mean = 0, double stdDev = 1)
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
            /// <param name="elem"> Xml data containing settings.</param>
            public GaussianDistrSettings(XElement elem)
            {
                //Parsing
                Mean = double.Parse(elem.Attribute("mean").Value, CultureInfo.InvariantCulture);
                StdDev = double.Parse(elem.Attribute("stdDev").Value, CultureInfo.InvariantCulture);
                Check();
                return;
            }

            //Methods
            private void Check()
            {
                if (StdDev <= 0)
                {
                    throw new Exception($"Incorrect StdDev ({StdDev.ToString(CultureInfo.InvariantCulture)}) value. StdDev must be GT 0.");
                }
                return;
            }

            /// <summary>
            /// See the base.
            /// </summary>
            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                GaussianDistrSettings cmpSettings = obj as GaussianDistrSettings;
                if (Mean != cmpSettings.Mean || StdDev != cmpSettings.StdDev)
                {
                    return false;
                }
                return true;
            }

            /// <summary>
            /// See the base.
            /// </summary>
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            /// <summary>
            /// Creates the deep copy instance of this instance
            /// </summary>
            public GaussianDistrSettings DeepClone()
            {
                return new GaussianDistrSettings(this);
            }

        }//GaussianSettings

    }//RandomValueSettings

}//Namespace
